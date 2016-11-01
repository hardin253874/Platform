// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Security.AuditLog;
using EDC.ReadiNow.Security.AuditLog.EventTargets;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportEventTarget : IEntityEventSave, IEntityEventError
    {
        /// <summary>
        /// The audit log event target
        /// </summary>
        private readonly AuditLogReportEventTarget _auditLogEventTarget = new AuditLogReportEventTarget(AuditLogInstance.Get());


        /// <summary>
        /// Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (long entityId in entities.Select(e => e.Id))
            {
                _auditLogEventTarget.WriteSaveAuditLogEntries(true, entityId, state);
            }
        }

        /// <summary>
        /// Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        /// True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (IEntity entity in entities)
            {
                var report = entity.As<Report>();

                if (report == null)
                {
                    continue;
                }

                _auditLogEventTarget.GatherAuditLogEntityDetailsForSave(report, state);

                EnsureIsDefaultReportForType(report, state);
            }

            return false;
        }

        /// <summary>
        /// Ensures the report is the default report for the type.
        /// </summary>        
        private void EnsureIsDefaultReportForType(Report report, IDictionary<string, object> state)
        {
            IEntityFieldValues changedFields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> changedFwdRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> changedRevRelationships;

            report.GetChanges(out changedFields, out changedFwdRelationships, out changedRevRelationships, false, false);

            if (changedRevRelationships == null || changedRevRelationships.Count == 0)
            {
                // No changes
                return;
            }

            SaveGraph saveGraph = EventTargetStateHelper.GetSaveGraph(state);

            ResourceEventTarget.EnsureIsOnlyRelationship(EntityType.DefaultPickerReport_Field.Id, true, report, changedRevRelationships, saveGraph);
            ResourceEventTarget.EnsureIsOnlyRelationship(EntityType.DefaultDisplayReport_Field.Id, true, report, changedRevRelationships, saveGraph);
        }
        
        /// <summary>
        /// Called if a failure occurs saving the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnSaveFailed(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                return;
            }

            foreach (long entity in entities.Select(e => e.Id))
            {
                _auditLogEventTarget.WriteSaveAuditLogEntries(false, entity, state);
            }
        }

        /// <summary>
        /// Called if a failure occurs deleting the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        public void OnDeleteFailed(IEnumerable<long> entities, IDictionary<string, object> state)
        {            
        }
    }
}
