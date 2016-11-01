// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    ///     Audit log solution event target
    /// </summary>
    internal class AuditLogSolutionEventTarget : AuditLogEventTargetBase<Solution, AuditLogSolutionEventTarget.AuditLogSolutionDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogSolutionEventTarget"/> class.
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        public AuditLogSolutionEventTarget(IAuditLog auditLog) : base(auditLog)
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
            get { return "AuditLogSolutionEventTarget:AuditLogSaveSolutionDetails"; }
        }


        /// <summary>
        ///     Gets the name of delete details state key.
        /// </summary>
        /// <value>
        ///     The delete details state key.
        /// </value>
        protected override string DeleteDetailsStateKey
        {
            get { return "AuditLogSolutionEventTarget:AuditLogDeleteSolutionDetails"; }
        }


        /// <summary>
        ///     Called to gather audit log entity details for save.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <returns></returns>
        protected override AuditLogSolutionDetails OnGatherAuditLogEntityDetailsForSave(Solution solution)
        {
            var solutionInternal = solution as IEntityInternal;
            return new AuditLogSolutionDetails {SolutionName = solution.Name, IsTemporaryId = solutionInternal.IsTemporaryId};
        }


        /// <summary>
        ///     Called to gather audit log entity details for delete.
        /// </summary>
        /// <param name="accessRule">The solution.</param>
        /// <returns></returns>
        protected override AuditLogSolutionDetails OnGatherAuditLogEntityDetailsForDelete(Solution accessRule)
        {
            return new AuditLogSolutionDetails {SolutionName = accessRule.Name, IsTemporaryId = false};
        }


        /// <summary>
        ///     Called to write save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The solution details.</param>
        protected override void OnWriteSaveAuditLogEntries(bool success, AuditLogSolutionDetails accessRuleDetails)
        {
            if (accessRuleDetails.IsTemporaryId)
            {
                // Solution is being created
                AuditLog.OnCreateApplication(success, accessRuleDetails.SolutionName);
            }
        }


        /// <summary>
        ///     Called to write delete audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The solution details.</param>
        protected override void OnWriteDeleteAuditLogEntries(bool success, AuditLogSolutionDetails accessRuleDetails)
        {
            AuditLog.OnDeleteApplication(success, accessRuleDetails.SolutionName);
        }


        #region Nested type: AuditLogSolutionDetails


        /// <summary>
        ///     This class is used to hold audit log solution changes.
        /// </summary>
        internal class AuditLogSolutionDetails
        {
            /// <summary>
            ///     Gets or sets the name of the user.
            /// </summary>
            /// <value>
            ///     The name of the user.
            /// </value>
            public string SolutionName { get; set; }


            /// <summary>
            ///     Gets or sets the old name of the user.
            /// </summary>
            /// <value>
            ///     The old name of the user.
            /// </value>
            public string SolutionVersion { get; set; }


            /// <summary>
            ///     Gets or sets a value indicating whether this instance has a temporary identifier.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance has a temporary identifier; otherwise, <c>false</c>.
            /// </value>
            public bool IsTemporaryId { get; set; }
        }


        #endregion
    }
}