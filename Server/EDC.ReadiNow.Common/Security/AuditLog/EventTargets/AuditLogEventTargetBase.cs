// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.EventClasses;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    /// Base class for audit log event targets.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDetails">The type of the details.</typeparam>
    internal class AuditLogEventTargetBase<TEntity, TDetails> : IAuditLogEventTarget<TEntity>
        where TEntity : class, IEntity
        where TDetails : class
    {
        /// <summary>
        /// The audit log.
        /// </summary>
        public IAuditLog AuditLog { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogEventTargetBase{TEntity, TDetails}"/> class.
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        /// <exception cref="System.ArgumentNullException">auditLog</exception>
        public AuditLogEventTargetBase(IAuditLog auditLog)
        {
            if (auditLog == null)
            {
                throw new ArgumentNullException("auditLog");    
            }

            AuditLog = auditLog;
        }


        /// <summary>
        /// Gets the name of save details state key.
        /// </summary>
        /// <value>
        /// The save details state key.
        /// </value>
        protected virtual string SaveDetailsStateKey
        {
            get { return ""; }
        }


        /// <summary>
        /// Gets the name of delete details state key.
        /// </summary>
        /// <value>
        /// The delete details state key.
        /// </value>
        protected virtual string DeleteDetailsStateKey
        {
            get { return ""; }
        }


        #region IAuditLogEventTarget<TEntity> Members


        /// <summary>
        /// Gathers the audit log entity details for save.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="state">The state.</param>
        public void GatherAuditLogEntityDetailsForSave(TEntity entity, IDictionary<string, object> state)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");    
            }

            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (string.IsNullOrEmpty(SaveDetailsStateKey))
            {
                return;
            }

            IDictionary<long, TDetails> entityDetailsState = GetAuditLogDetailsState(state, SaveDetailsStateKey);

            TDetails details = OnGatherAuditLogEntityDetailsForSave(entity);
            if (details == null)
            {
                return;
            }

            entityDetailsState[entity.Id] = details;
        }


        /// <summary>
        /// Gathers the audit log entity details for delete.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="state">The state.</param>
        public void GatherAuditLogEntityDetailsForDelete(TEntity entity, IDictionary<string, object> state)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (string.IsNullOrEmpty(DeleteDetailsStateKey))
            {
                return;
            }

            IDictionary<long, TDetails> entityDetailsState = GetAuditLogDetailsState(state, DeleteDetailsStateKey);

            TDetails details = OnGatherAuditLogEntityDetailsForDelete(entity);
            if (details == null)
            {
                return;
            }

            entityDetailsState[entity.Id] = details;
        }


        /// <summary>
        /// Writes the save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="state">The state.</param>
        public void WriteSaveAuditLogEntries(bool success, long entityId, IDictionary<string, object> state)
        {            
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (string.IsNullOrEmpty(SaveDetailsStateKey))
            {
                return;
            }

            IDictionary<long, TDetails> entityDetailsState = GetAuditLogDetailsState(state, SaveDetailsStateKey);

            TDetails entityDetails;

            if (!entityDetailsState.TryGetValue(entityId, out entityDetails))
            {
                // Check for entities being created
                long temporaryId = EventTargetStateHelper.GetTemporaryIdFromId(state, entityId);

                if (!entityDetailsState.TryGetValue(temporaryId, out entityDetails))
                {
                    return;
                }
            }

            OnWriteSaveAuditLogEntries(success, entityDetails);
        }


        /// <summary>
        /// Writes the delete audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="state">The state.</param>
        public void WriteDeleteAuditLogEntries(bool success, long entityId, IDictionary<string, object> state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (string.IsNullOrEmpty(DeleteDetailsStateKey))
            {
                return;
            }

            IDictionary<long, TDetails> entityDetailsState = GetAuditLogDetailsState(state, DeleteDetailsStateKey);

            TDetails entityDetails;

            if (!entityDetailsState.TryGetValue(entityId, out entityDetails))
            {
                return;
            }

            OnWriteDeleteAuditLogEntries(success, entityDetails);
        }


        #endregion


        /// <summary>
        /// Gets the audit log details state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        private IDictionary<long, TDetails> GetAuditLogDetailsState(IDictionary<string, object> state, string keyName)
        {
            return EventTargetStateHelper.GetValue<IDictionary<long, TDetails>>(state, keyName, () => new Dictionary<long, TDetails>());
        }


        /// <summary>
        /// Called to gather audit log entity details for save.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected virtual TDetails OnGatherAuditLogEntityDetailsForSave(TEntity entity)
        {
            return null;
        }


        /// <summary>
        /// Called to gather audit log entity details for delete.
        /// </summary>
        /// <param name="accessRule">The entity.</param>
        /// <returns></returns>
        protected virtual TDetails OnGatherAuditLogEntityDetailsForDelete(TEntity accessRule)
        {
            return null;
        }


        /// <summary>
        /// Called to write save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The entity details.</param>
        protected virtual void OnWriteSaveAuditLogEntries(bool success, TDetails accessRuleDetails)
        {
        }


        /// <summary>
        /// Called to write delete audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The entity details.</param>
        protected virtual void OnWriteDeleteAuditLogEntries(bool success, TDetails accessRuleDetails)
        {
        }
    }
}