// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Security.AuditLog;
using EDC.ReadiNow.Security.AuditLog.EventTargets;
using EDC.ReadiNow.Metadata.Tenants;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    ///     Event target for tenants.
    /// </summary>
    public class TenantEventTarget : IEntityEventSave, IEntityEventDelete, IEntityEventError
    {
        private string NewTenantKey = "TenantEventTarget_NewTenant";

        /// <summary>
        ///     The audit log event target
        /// </summary>
        private readonly AuditLogTenantEventTarget _auditLogEventTarget = new AuditLogTenantEventTarget(AuditLogInstance.Get());


        #region IEntityEventDelete Members


        /// <summary>
        ///     Called after deletion of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            foreach (long entityId in entities)
            {
                _auditLogEventTarget.WriteDeleteAuditLogEntries(true, entityId, state);
            }
        }


        /// <summary>
        ///     Called before deleting an enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        /// <returns>
        ///     True to cancel the delete operation; false otherwise.
        /// </returns>
        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (Tenant tenant in entities.Select(entity => entity.As<Tenant>()).Where(tenant => tenant != null))
            {
                // Gather tenant details required OnAfterDelete
                _auditLogEventTarget.GatherAuditLogEntityDetailsForDelete(tenant, state);
            }

            return false;
        }


        #endregion


        #region IEntityEventError Members


        /// <summary>
        ///     Called if a failure occurs saving the specified entities.
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
        ///     Called if a failure occurs deleting the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        public void OnDeleteFailed(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                return;
            }

            foreach (long entity in entities)
            {
                _auditLogEventTarget.WriteDeleteAuditLogEntries(false, entity, state);
            }
        }


        #endregion


        #region IEntityEventSave Members


        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (long entityId in entities.Select(e => e.Id))
            {
                _auditLogEventTarget.WriteSaveAuditLogEntries(true, entityId, state);
            }

            var newTenantTempIds = (List<long>) state[NewTenantKey];

            Database.DatabaseContext.GetContext().AddPostDisposeAction(() =>
            {
                foreach (var tempId in newTenantTempIds)
                {
                    var tenantId = EventTargetStateHelper.GetIdFromTemporaryId(state, tempId);
                    TenantHelper.NotifyTenantCreate(tenantId);
                }
            });

        }


        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var newTenants = new List<long>();

            foreach (IEntity entity in entities)
            {
                var tenant = entity.As<Tenant>();
                if (tenant == null)
                {
                    continue;
                }

                _auditLogEventTarget.GatherAuditLogEntityDetailsForSave(tenant, state);

                if (tenant.IsTemporaryId) // new tenant
                {
                    newTenants.Add(tenant.Id);
                }
            }

            state[NewTenantKey] = newTenants;

            return false;
        }


        #endregion
    }
}
