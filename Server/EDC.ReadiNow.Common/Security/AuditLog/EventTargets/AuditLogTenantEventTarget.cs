// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    /// Audit log tenant event target.
    /// </summary>
    internal class AuditLogTenantEventTarget : AuditLogEventTargetBase<Tenant, AuditLogTenantEventTarget.AuditLogTenantDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogTenantEventTarget"/> class.
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        public AuditLogTenantEventTarget(IAuditLog auditLog) : base(auditLog)
        {
        }


        /// <summary>
        ///     Gets the name of save details state key.
        /// </summary>
        /// <value>
        ///     The save details state key.
        /// </value>
        protected override string SaveDetailsStateKey
        {
            get { return "AuditLogTenantEventTarget:AuditLogSaveTenantDetails"; }
        }


        /// <summary>
        ///     Gets the name of delete details state key.
        /// </summary>
        /// <value>
        ///     The delete details state key.
        /// </value>
        protected override string DeleteDetailsStateKey
        {
            get { return "AuditLogTenantEventTarget:AuditLogDeleteTenantDetails"; }
        }


        /// <summary>
        ///     Called to gather audit log entity details for save.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <returns></returns>
        protected override AuditLogTenantDetails OnGatherAuditLogEntityDetailsForSave(Tenant tenant)
        {
            var tenantInternal = tenant as IEntityInternal;

            return new AuditLogTenantDetails {TenantName = tenant.Name, IsTemporaryId = tenantInternal.IsTemporaryId};
        }


        /// <summary>
        ///     Called to gather audit log entity details for delete.
        /// </summary>
        /// <param name="accessRule">The tenant.</param>
        /// <returns></returns>
        protected override AuditLogTenantDetails OnGatherAuditLogEntityDetailsForDelete(Tenant accessRule)
        {
            return new AuditLogTenantDetails {TenantName = accessRule.Name, IsTemporaryId = false};
        }


        /// <summary>
        ///     Called to write save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The tenant details.</param>
        protected override void OnWriteSaveAuditLogEntries(bool success, AuditLogTenantDetails accessRuleDetails)
        {
            if (accessRuleDetails.IsTemporaryId)
            {
                // Tenant is being created
                AuditLog.OnCreateTenant(success, accessRuleDetails.TenantName);
            }
        }


        /// <summary>
        ///     Called to write delete audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The tenant details.</param>
        protected override void OnWriteDeleteAuditLogEntries(bool success, AuditLogTenantDetails accessRuleDetails)
        {
            AuditLog.OnDeleteTenant(success, accessRuleDetails.TenantName);
        }


        #region Nested type: AuditLogTenantDetails


        /// <summary>
        ///     Audit log tenant changes.
        /// </summary>
        internal class AuditLogTenantDetails
        {
            /// <summary>
            ///     Gets or sets the name of the tenant.
            /// </summary>
            /// <value>
            ///     The name of the tenant.
            /// </value>
            public string TenantName { get; set; }


            /// <summary>
            ///     Gets or sets a value indicating whether this instance is temporary identifier.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance is temporary identifier; otherwise, <c>false</c>.
            /// </value>
            public bool IsTemporaryId { get; set; }
        }


        #endregion
    }
}