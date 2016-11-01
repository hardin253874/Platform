// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EDC.Common;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.EditForm
{
    /// <summary>
    /// Helper for the EditForm service
    /// </summary>
    public static class EditFormHelper
    {
		/// <summary>
		/// Fetch a form as Entity data
		/// </summary>
		/// <param name="formId">The form identifier.</param>
		/// <param name="isInDesignMode">if set to <c>true</c> [is in design mode].</param>
		/// <returns></returns>
		/// <exception cref="FormNotFoundException">
		/// </exception>
        public static EntityData GetFormAsEntityData(long formId, bool isInDesignMode = false)
        {
            string request = CustomEditFormHelper.GetHtmlFormQuery(isInDesignMode);

            // Check security and existance
            try
            {
                IEntity form = Entity.Get(formId);
                if (form == null)
                {
                    EventLog.Application.WriteError("Attempted to fetch an unknown form/screen. {0}", formId);
                    throw new FormNotFoundException();
                }
            }
            catch (ArgumentException) // from EntityIdentificationCache.GetId 
            {
                EventLog.Application.WriteError("Attempted to fetch an unknown form/screen. {0}", formId);
                throw new FormNotFoundException();
            }

            // Load form data
            EntityData formEntityData;
            using (Profiler.Measure("Get form data"))
            using (new SecurityBypassContext())
            {
                formEntityData = BulkRequestRunner.GetEntityData(formId, request, "GetFormAsEntityData");
            }
            if (formEntityData == null)
            {
                EventLog.Application.WriteError("Attempted to fetch an unknown form. {0}", formId);
                throw new FormNotFoundException();
            }


            //
            // If we are not in design mode we need to go an fetch any missing default reports.
            // We don't want to fetch these for design mode because that will 'fix' them in the form.
            //
            if (!isInDesignMode)
            {
                using (Profiler.Measure("Get form default reports"))
                using (new SecurityBypassContext())
                {
                    SetDefaultReportsForRelationshipControls(formEntityData);
                }
            }
            return formEntityData;
        }

        public static EntityData GetFormAsEntityData(EntityRef formRef, bool isInDesignMode = false)
        {
            return GetFormAsEntityData(formRef.Id, isInDesignMode);
        }

        /// <summary>
        /// Generate a default form for the given resource type.
        /// </summary>
        public static EntityData GenerateDefaultFormForResourceType(EntityRef typeRef, bool isInDesignMode)
        {
            using (Profiler.Measure("GenerateDefaultFormForResourceType"))
            using (new EntitySnapshotContext())
            using (new SecurityBypassContext())
            using (var generator = new DefaultLayoutGenerator(CurrentUiContext.Html, typeRef, isInDesignMode))
            {
                string request = CustomEditFormHelper.GetHtmlFormQuery(isInDesignMode);
                return generator.GetLayoutAsEntityData(request);
            }
        }


        /// <summary>
        /// Sets the default reports for relationship controls.
        /// </summary>
        /// <param name="form">The form.</param>
        private static void SetDefaultReportsForRelationshipControls(EntityData form)
        {
            // Find the template report 
            var relationshipDisplayReportRelId = Entity.GetId("console:relationshipDisplayReport");
            var pickerReportRelId = Entity.GetId("console:pickerReport");
            var isReversedFieldId = Entity.GetId("console:isReversed");
            var relationshipToRenderRelId = Entity.GetId("console:relationshipToRender");

            //  get all Inline and tabbed relationships
            var relationshipControls = new List<EntityData>();
            GetAllRelationshipControlsOnForm(form, relationshipControls);

            // list of reports that we need to load - mapped to relationship containers that we need to place them in
            var reportsToLoad = new List<Tuple<EntityRef, RelationshipData>>();

            foreach (EntityData relationshipControl in relationshipControls)
            {
                // get the relationship that this control represents 
                var relationshipToRenderRelation = GetRelationDataByRelationId(relationshipToRenderRelId,
                                                                                relationshipControl.Relationships);

                if (relationshipToRenderRelation == null || relationshipToRenderRelation.Instances.Count <= 0)
                    continue;
                var relationshipId = relationshipToRenderRelation.Instances.First().Entity.Id;
                var relationship = Entity.Get<Relationship>(relationshipId);

                // get the direction that we're following the relationship
                bool isReversed = false;
                var isReversedField = GetFieldDataByFieldId(isReversedFieldId, relationshipControl.Fields);
                if (isReversedField != null && isReversedField.Value.Value != null)
                {
                    bool.TryParse(isReversedField.Value.Value.ToString(), out isReversed);
                }

                // check display report
                CheckRelationshipReport(relationshipControl, relationshipDisplayReportRelId, relationship, isReversed, reportsToLoad, true);

                // check picker report
                CheckRelationshipReport(relationshipControl, pickerReportRelId, relationship, isReversed, reportsToLoad, false);
            }

            // load reports
            if (reportsToLoad.Count > 0)
            {
                var reportIds = reportsToLoad.Select(r => r.Item1).Distinct();
                var reports = BulkRequestRunner.GetEntitiesData(reportIds, CustomEditFormHelper.GetRelationshipReportQueryString, "Form report");
                var reportDict = reports.ToDictionarySafe(r => r.Id.Id);

                foreach (var task in reportsToLoad)
                {
                    EntityRef reportId = task.Item1;
                    EntityData reportData = reportDict[reportId.Id];
                    if (reportData != null)
                    {
                        // store data in the relationship container
                        RelationshipData container = task.Item2;
                        container.Instances.Add(
                            new RelationshipInstanceData()
                            {
                                DataState = DataState.Unchanged,
                                Entity = reportData
                            });
                    }
                }
            }
        }


        /// <summary>
        /// Checks that a relationship control has a display report defined.
        /// If it doesn't then find a suitable default report and convert it to the entity data format.
        /// </summary>
        private static void CheckRelationshipReport(EntityData relationshipControl, long relToReportRelId, Relationship relationship, bool isReversed, List<Tuple<EntityRef, RelationshipData>> reportsToLoad, bool displayReport)
        {
            // find the relationship container that contains the link to any explicitly set report
            var container = GetRelationDataByRelationId(relToReportRelId, relationshipControl.Relationships);
            if (container == null)
                return; // assert false

            // check if a report is explicitly set
            bool reportManuallySet = container.Instances.Count > 0;
            if (reportManuallySet)
                return;

            // convert report to entity-data (or get template report)
            EntityRef reportToLoad;

            // find the default display report
            if (displayReport)
            {
                var displayRpt = GetDefaultDisplayReport(relationship, isReversed);
                reportToLoad = displayRpt == null ? (EntityRef) "core:templateReport" : displayRpt.Id;
            }
            else
            {
                var pickerRpt = GetDefaultPickerReport(relationship, isReversed);
                reportToLoad = pickerRpt == null ? (EntityRef) "core:templateReport" : pickerRpt.Id;
            }
            
            reportsToLoad.Add(new Tuple<EntityRef, RelationshipData>(reportToLoad, container));
        }


        /// <summary>
        /// Returns the default display report.
        /// </summary>
        /// <param name="relationship"></param>
        /// <param name="isReversed"></param>
        /// <returns></returns>
        private static Report GetDefaultDisplayReport(Relationship relationship, bool isReversed)
        {
            if (relationship == null)
            {
                return null;
            }

            var entityType = isReversed ? relationship.FromType : relationship.ToType;

            if (entityType != null)
            {
                return entityType.DefaultDisplayReport;
            }

            return null;
        }

        /// <summary>
        /// Returns the default picker report.
        /// </summary>
        /// <param name="relationship"></param>
        /// <param name="isReversed"></param>
        /// <returns></returns>
        private static ResourcePicker GetDefaultPickerReport(Relationship relationship, bool isReversed)
        {
            if (relationship == null)
            {
                return null;
            }

            var entityType = isReversed ? relationship.FromType : relationship.ToType;

            if (entityType != null)
            {
                return entityType.DefaultPickerReport;
            }

            return null;
        }

        /// <summary>
        /// Gets all relationship controls on form.
        /// </summary>
        private static void GetAllRelationshipControlsOnForm(EntityData customEditForm, List<EntityData> results)
        {
            //List<RelationshipInstanceData> relationshipControls = new List<RelationshipInstanceData>();
            var containedControlsOnFormRelId = Entity.GetId("console:containedControlsOnForm");
            var formToRenderRelId = Entity.GetId("console:formToRender");
            //var relationshipToRenderRelId = Entity.GetId("console:relationshipToRender");
            var isOfTypeRellId = Entity.GetId("core:isOfType");

            var tabRelationshipRenderControlId = Entity.GetId("console:tabRelationshipRenderControl");
            var inlineRelationshipRenderControlId = Entity.GetId("console:inlineRelationshipRenderControl");
            var dropDownRelationshipRenderControlId = Entity.GetId("console:dropDownRelationshipRenderControl");

            var containedControlsOnFormRelation = GetRelationDataByRelationId(containedControlsOnFormRelId, customEditForm.Relationships);
            var formToRenderRelation = GetRelationDataByRelationId(formToRenderRelId, customEditForm.Relationships);

            var rels = new List<RelationshipInstanceData>();
            if (containedControlsOnFormRelation != null && containedControlsOnFormRelation.Instances.Count > 0)
            {
                rels.AddRange(containedControlsOnFormRelation.Instances);
            }
            if (formToRenderRelation != null && formToRenderRelation.Instances.Count > 0)
            {
                rels.AddRange(formToRenderRelation.Instances);
            }

            if (rels.Count > 0)
            {
                foreach (var control in rels)
                {
                    // following line is to check if this control is 'StructureControlOnForm'
                    var containedControlsOnFormRelation2 = GetRelationDataByRelationId(containedControlsOnFormRelId, control.Entity.Relationships);
                    var formToRenderRelation2 = GetRelationDataByRelationId(formToRenderRelId, control.Entity.Relationships);

                    var rels2 = new List<RelationshipInstanceData>();
					if ( containedControlsOnFormRelation2 != null && containedControlsOnFormRelation2.Instances.Count > 0 )
                    {
                        rels2.AddRange(containedControlsOnFormRelation2.Instances);
                    }
					if ( formToRenderRelation2 != null && formToRenderRelation2.Instances.Count > 0 )
                    {
                        rels2.AddRange(formToRenderRelation2.Instances);
                    }

					if ( rels2.Count > 0 )   // it is a strutureControlOnForm
                    {
                        GetAllRelationshipControlsOnForm(control.Entity, results);
                    }
                    else
                    {
                        var isOfTypeRelation = GetRelationDataByRelationId(isOfTypeRellId, control.Entity.Relationships);
						if ( isOfTypeRelation != null && isOfTypeRelation.Instances.Count > 0 )
                        {
                            // Check if it is a 'TabRelationshipControl' or 'InlineRelationshipControl'
                            foreach (var rel in isOfTypeRelation.Instances)
                            {
                                EntityData relEntity = rel.Entity;
                                if (relEntity.Id.Id == tabRelationshipRenderControlId ||
                                    relEntity.Id.Id == inlineRelationshipRenderControlId ||
                                    relEntity.Id.Id == dropDownRelationshipRenderControlId)
                                {
                                    results.Add(control.Entity);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the relation data by relation id.
        /// </summary>
        private static RelationshipData GetRelationDataByRelationId(long relationshipId, List<RelationshipData> relationships)
        {
            var relationship = (from f in relationships
                                where f.RelationshipTypeId.Id == relationshipId
                                select f).FirstOrDefault();

            return relationship;
        }

        /// <summary>
        /// Gets the field data by field id.
        /// </summary>
        private static FieldData GetFieldDataByFieldId(long fieldId, List<FieldData> fields)
        {
            var field = (from f in fields
                         where f.FieldId.Id == fieldId
                         select f).FirstOrDefault();

            return field;
        }

        /// <summary>
        /// Gets the default type of the display report for.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns></returns>
        internal static Report GetDefaultDisplayReportForType(IEnumerable<EntityType> types)
        {
            // Null checks, you would think wouldn't be necessary
            if (types == null || !types.Any(t => t != null))
            {
                return null;
            }

            // If we genuinely want to relate to resource, or editable resource, then use their default type.
            // But for any other type, if no default type is specified then default to the template report. (Rather than the default report for 'resource').
            string alias = types.First().Alias;
            bool ignoreTemplateReport = alias == "core:resource" || alias == "core:userResource";

            // Enumerator over type and inherited types
            IEnumerable<EntityType> allTypes = EntityTypeHelper.GetAllTypes(types, true);

            return (from e in allTypes
                    where e.DefaultDisplayReport != null &&
                          (e.Alias == null ||
                          ignoreTemplateReport ||
                           (e.Alias.Replace("core:", "") != "userResource" &&
                            e.Alias.Replace("core:", "") != "resource"))
                    select e.DefaultDisplayReport).FirstOrDefault();
        }

        /// <summary>
        /// Gets the default type of the picker report for.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns></returns>
        internal static ResourcePicker GetDefaultPickerReportForType(IEnumerable<EntityType> types)
        {
            // Null checks. Again.
            if (types == null || !types.Any(t => t != null))
            {
                return null;
            }

            // Enumerator over type and inherited types
            IEnumerable<EntityType> allTypes = EntityTypeHelper.GetAllTypes(types, true);

            return (from e in allTypes
                    where e.DefaultPickerReport != null
                    select e.DefaultPickerReport).FirstOrDefault();
        }


        /// <summary>
        /// Gets the proposed name of the form.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="proposedName">Name of the proposed.</param>
        /// <returns></returns>
        internal static string GetProposedFormName(EntityType entityType, string proposedName = null)
        {            
            // Select a different name that does not collide with existing forms
            var existingForms = entityType.GetRelationships(new EntityRef("console", "formsToEditType"));            

            string baseName;

	        var name = new EntityRef( "core", "name" );

            if (string.IsNullOrEmpty(proposedName))
            {
                string currentName = (string)entityType.GetField(name);
                baseName = string.Format("{0} Form", currentName);
                proposedName = baseName;
            }
            else
            {
                // Remove trailing numbers
                string regex = "(\\d+)$";
                Regex reg = new Regex(regex);
                baseName = reg.Replace(proposedName, "").Trim();
            }            

            int i = 0;
            while (true)
            {
                if (existingForms.All(form => (string)form.Entity.GetField(name) != proposedName))
                    break;

                i++;
                                
                proposedName = string.Format("{0} {1}", baseName, i);
            }
            return proposedName;
        }
    }
}
