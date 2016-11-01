// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    ///     This interface is used by audit log event targets to write to the audit log.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IAuditLogEventTarget<in TEntity> where TEntity : IEntity
    {
        /// <summary>
        ///     Gathers the audit log entity details required for save.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="state">The state.</param>
        void GatherAuditLogEntityDetailsForSave(TEntity entity, IDictionary<string, object> state);


        /// <summary>
        ///     Gathers the audit log entity details required for delete.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="state">The state.</param>
        void GatherAuditLogEntityDetailsForDelete(TEntity entity, IDictionary<string, object> state);


        /// <summary>
        /// Writes the save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="state">The state.</param>
        void WriteSaveAuditLogEntries(bool success, long entityId, IDictionary<string, object> state);


        /// <summary>
        /// Writes the delete audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="state">The state.</param>
        void WriteDeleteAuditLogEntries(bool success, long entityId, IDictionary<string, object> state);
    }
}