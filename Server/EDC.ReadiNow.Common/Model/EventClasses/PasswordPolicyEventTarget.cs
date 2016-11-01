// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Security.AuditLog;
using EDC.ReadiNow.Security.AuditLog.EventTargets;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    ///     Event target for password policy instances.
    /// </summary>
    public class PasswordPolicyEventTarget : IEntityEventSave, IEntityEventError
    {
        /// <summary>
        /// The audit log event target
        /// </summary>
        private readonly AuditLogPasswordPolicyEventTarget _auditLogEventTarget = new AuditLogPasswordPolicyEventTarget(AuditLogInstance.Get());


        #region IEntityEventError Members


        /// <summary>
        ///     Called if a failure occurs saving the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <exception cref="System.NotImplementedException"></exception>
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
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnDeleteFailed(IEnumerable<long> entities, IDictionary<string, object> state)
        {
        }


        #endregion


        #region IEntityEventSave Members


        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (long entity in entities.Select(e => e.Id))
            {
                _auditLogEventTarget.WriteSaveAuditLogEntries(true, entity, state);
            }
        }


        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (PasswordPolicy passwordPolicy in entities.Select(entity => entity.As<PasswordPolicy>()).Where(passwordPolicy => passwordPolicy != null))
            {
                _auditLogEventTarget.GatherAuditLogEntityDetailsForSave(passwordPolicy, state);
            }

            return false;
        }


        #endregion
    }
}