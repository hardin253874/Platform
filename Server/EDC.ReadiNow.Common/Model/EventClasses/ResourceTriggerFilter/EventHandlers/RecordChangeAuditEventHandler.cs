// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Database;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter.EventHandlers
{
    /// <summary>
    /// Handler which creates Log entry for a record change.
    /// </summary>
    public class RecordChangeAuditEventHandler : IFilteredSaveEventHandler
    {
        public bool OnBeforeSave(ResourceTriggerFilterDef policy, IEntity entity, bool isNew, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
        {
            using (new SecurityBypassContext())
            {
                var changedWatchedFields = changedFields.Intersect(policy.UpdatedFieldsToTriggerOn.Select(f => f.Id)).Except(IgnoredFields());
                var changedWatchedForwardRels = changedForwardRels.Intersect(policy.UpdatedRelationshipsToTriggerOn.Select(f => f.Id)).Except(IgnoredRels());
                var changedWatchedReverseRels = changedReverseRels.Intersect(policy.UpdatedRelationshipsToTriggerOn.Select(f => f.Id)).Except(IgnoredRels());

                var originalEntity = isNew ? null : GetOriginalEntity(entity, changedWatchedFields, changedWatchedForwardRels, changedWatchedReverseRels);      

                RecordChanges(entity, (formatter) =>
                {
                    AddChanges(formatter, policy, isNew, entity, originalEntity, changedWatchedFields, changedWatchedForwardRels, changedWatchedReverseRels);
                });

                return false;
            }
        }



        void IFilteredSaveEventHandler.OnAfterSave(ResourceTriggerFilterDef policy, IEntity entity, bool isNew, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
        {
            // do Nothing
        }

        bool IFilteredSaveEventHandler.OnBeforeReverseAdd(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity, bool isNew)
        {
            using (new SecurityBypassContext())
            {
                var change = isNew ? "created" : "updated";

                RecordChanges(policyEntity, (formatter) =>
                {
                    var rel = Entity.Get<Relationship>(relationshipId);

                    if (rel.IsLookup(direction))
                    {
                        formatter.AddChangedLookup(rel.DisplayName(direction), null, otherEntity);
                    }
                    else
                    {
                        formatter.AddChangedRelationship(rel.DisplayName(direction), Enumerable.Empty<long>(), otherEntity.Id.ToEnumerable<long>(), -1);  //TODO: Fix count
                }

                });

                return false;
            }
        }
        bool IFilteredSaveEventHandler.OnBeforeReverseRemove(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity)
        {
            using (new SecurityBypassContext())
            {
                RecordChanges(policyEntity, (formatter) =>
            {
                var rel = Entity.Get<Relationship>(relationshipId);

                if (rel.IsLookup(direction))
                {
                    formatter.AddChangedLookup(rel.DisplayName(direction), otherEntity, null);
                }
                else
                {
                    formatter.AddChangedRelationship(rel.DisplayName(direction), otherEntity.Id.ToEnumerable<long>(), Enumerable.Empty<long>(), -1); //TODO: Fix count
                }

            });

                return false;
            }
        }


        void IFilteredSaveEventHandler.OnAfterReverseAdd(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity, bool isNew)
        {
            // do nothing
        }

        void IFilteredSaveEventHandler.OnAfterReverseRemove(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity)
        {
            // do nothing
        }


        bool IFilteredSaveEventHandler.OnBeforeDelete(ResourceTriggerFilterDef policy, IEntity entity)
        {
            using (new SecurityBypassContext())
            {
                RecordChanges(entity, (formatter) =>
                {
                    formatter.IsDelete = true;
                });

                return false;
            }
        }


        void IFilteredSaveEventHandler.OnAfterDelete(ResourceTriggerFilterDef policy, IEntity entity)
        {
            // do nothing
        }

        /// <summary>
        /// Create a formatter, populate it using the provided action and then log th result.
        /// </summary>
        /// <param name="entity">The entity that is changing</param>
        /// <param name="updateFormaterAction">Action to update the formatter</param>
        /// <returns></returns>
        private void RecordChanges(IEntity entity, Action<RecordChangeAuditFormatter> updateFormaterAction)
        {
            var ctx = RequestContext.GetContext();
            var userIdentity = ctx.Identity;
            var secondaryIdentity = ctx.SecondaryIdentity;

            string entityName;

            // Get the name of the entity. If it is a new entity we can use the name, otherwise we get the entity before th
            if (entity.IsTemporaryId)
            {
                entityName = entity.GetField<string>(Resource.Name_Field);
            }
            else
            {
                var originalEntity = Entity.Get(entity.Id, Resource.Name_Field);       
                entityName = originalEntity.GetField<string>(Resource.Name_Field);
            }


            var formatter = new RecordChangeAuditFormatter(entityName, userIdentity != null ? userIdentity.Name : null, secondaryIdentity != null ? secondaryIdentity.Name : null);

            formatter.IsCreate = entity.IsTemporaryId;

            updateFormaterAction(formatter);

            // Only log if we have something to say. (A zero count should only occur when ignored fields are being tracked.)
            if (formatter.Count > 0 || formatter.IsDelete)
            {
                var logName = formatter.GetNameString();
                var description = formatter.ToString();

                var eventTime = DateTime.UtcNow;

                // Generate the log after the transaction completes.
                using (var dbCtx = DatabaseContext.GetContext())
                {
                    dbCtx.AddPostDisposeAction(() =>
                    {
                        using (new SecurityBypassContext())
                        using (new WorkflowRunContext { DisableTriggers = true })
                        {
                            var logEntry = new RecordChangeLogEntry { ObjectReferencedInLog = entity.As<UserResource>(), Name = logName, Description = description, LogEventTime = eventTime };
                            Factory.ActivityLogWriter.WriteLogEntry(logEntry.Cast<TenantLogEntry>());
                        }
                    });
                }
            }
        }




        /// <summary>
        /// Add a textural record of all of the changes to the entity into the provided string builder
        /// </summary>
        void AddChanges(RecordChangeAuditFormatter formatter, ResourceTriggerFilterDef policy, bool isNew, IEntity entity, IEntity originalEntity, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
        {
            var allChanges = changedFields.Union(changedForwardRels).Union(changedReverseRels);

            formatter.IsCreate = isNew;

            AddChangedFields(formatter, originalEntity, entity, changedFields);
            AddAllChangedRelationships(formatter, originalEntity, entity, changedForwardRels, changedReverseRels);
        }

		/// <summary>
		/// Get the original entity with the changed fields and relationships have been fetched.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="changedFields">The changed fields.</param>
		/// <param name="changedForwardRels">The changed forward rels.</param>
		/// <param name="changedReverseRels">The changed reverse rels.</param>
		/// <returns></returns>
		IEntity GetOriginalEntity(IEntity entity, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
        {
            var allChanges = changedFields.Union(changedForwardRels).Union(changedReverseRels);

            return Entity.Get(entity.Id, allChanges.Select(id => new EntityRef(id)).ToArray());
        }


        /// <summary>
        ///  Add a textural record of all of the field changes to an entity into the provided string builder
        /// </summary>
        void AddChangedFields(RecordChangeAuditFormatter formatter, IEntity originalEntity, IEntity changedEntity, IEnumerable<long> changedFields)
        {
            var fields = changedFields.Select(id => Entity.Get<Field>(id, Field.Name_Field)).OrderBy(f => f.Name);     // These fields should be all cached so no need for a bulk request
            foreach (var field in fields)
            {
                if (!IgnoreFieldType(field))
                {
                    var oldValue = originalEntity == null ? null : originalEntity.GetField(field);
                    var newValue = changedEntity.GetField(field);

                    formatter.AddChangedField(field, oldValue, newValue);
                }
            }
        }

        void AddAllChangedRelationships(RecordChangeAuditFormatter formatter, IEntity originalEntity, IEntity changedEntity, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
        {
            AddChangedRelationships(formatter, changedForwardRels, Direction.Forward, originalEntity, changedEntity);
            AddChangedRelationships(formatter, changedReverseRels, Direction.Reverse, originalEntity, changedEntity);
        }

        void AddChangedRelationships(RecordChangeAuditFormatter formatter, IEnumerable<long> relIds, Direction direction, IEntity originalEntity, IEntity changedEntity)
        {
            var rels = Entity.Get<Relationship>(relIds.Select(id => new EntityRef(id)));

            foreach (var rel in rels)
            {
                if (rel.IsLookup(direction))
                {
                    var oldResource = originalEntity == null ? null : originalEntity.GetRelationships(rel, direction).FirstOrDefault();
                    var newResource = changedEntity.GetRelationships(rel, direction).FirstOrDefault();

                    formatter.AddChangedLookup(rel.DisplayName(direction), oldResource, newResource);
                }
                else
                {
                    var tracker = changedEntity.GetRelationships(rel, direction).Tracker;

                    var removed = tracker.Removed.Select(r => r.Key);
                    var added = tracker.Added.Select(r => r.Key);
                    var count = tracker.Count;

                    formatter.AddChangedRelationship(rel.DisplayName(direction), removed, added, count);
                }
            }
        }

        /// <summary>
        /// Ignore specific fields
        /// </summary>
        IEnumerable<long> IgnoredFields()
        {
            var aliases = WellKnownAliases.CurrentTenant;
            return new List<long> { aliases.CreatedDate, aliases.ModifiedDate };
        }

        /// <summary>
        /// Ignore specific relationships
        /// </summary>
        IEnumerable<long> IgnoredRels()
        {
            var aliases = WellKnownAliases.CurrentTenant;
            return new List<long> { aliases.CreatedBy, aliases.LastModifiedBy };
        }

        /// <summary>
        /// Ignore specific fields
        /// </summary>
        IEnumerable<long> IgnoredFieldTypes()
        {
            var aliases = WellKnownAliases.CurrentTenant;
            return new List<long> { aliases.CreatedDate, aliases.ModifiedDate };
        }

        /// <summary>
        /// Ignore the given field type
        /// </summary>
        bool IgnoreFieldType(Field field)
        {
            var fieldType = field.IsOfType.FirstOrDefault();
            var alias = fieldType != null ? fieldType.Alias :  null;
            return alias == "core:autoNumberField" || field.IsCalculatedField == true;
        }


    }

  
}
