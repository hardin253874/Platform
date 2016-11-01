// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    ///     Audit log report event target.
    /// </summary>
    internal class AuditLogReportEventTarget : AuditLogEventTargetBase<Report, AuditLogReportEventTarget.AuditLogReportDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogReportEventTarget"/> class.
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        public AuditLogReportEventTarget(IAuditLog auditLog) : base(auditLog)
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
            get { return "AuditLogReportEventTarget:AuditLogSaveReportDetails"; }
        }


        /// <summary>
        ///     Called to gather audit log entity details for save.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <returns></returns>
        protected override AuditLogReportDetails OnGatherAuditLogEntityDetailsForSave(Report report)
        {
            var reportInternal = report as IEntityInternal;
            IEntityFieldValues fields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;
            report.GetChanges(out fields, out forwardRelationships, out reverseRelationships);

            var reportDetails = new AuditLogReportDetails();

            if (reportInternal.IsTemporaryId)
            {
                return reportDetails;
            }

            if ((fields != null && fields.Any()) ||
				( reverseRelationships != null && reverseRelationships.Count > 0 ) ||
				( forwardRelationships != null && forwardRelationships.Count > 0 ) )
            {
                AccessRule accessRule = report.ReportForAccessRule;
                if (accessRule != null)
                {
                    // gather info to notify that an access rule's query has changed                    
                    reportDetails.IsAccessRuleReport = true;
                    reportDetails.AccessRuleReportName = report.Name;

                    SecurableEntity controlAccess = accessRule.ControlAccess;
                    if (controlAccess != null)
                    {
                        reportDetails.SecuredTypeName = controlAccess.Name;
                    }

                    Subject allowAccessBy = accessRule.AllowAccessBy;
                    if (allowAccessBy != null)
                    {
                        reportDetails.SubjectName = allowAccessBy.Name;
                    }
                }
            }

            return reportDetails;
        }


        /// <summary>
        ///     Called to write save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The report details.</param>
        protected override void OnWriteSaveAuditLogEntries(bool success, AuditLogReportDetails accessRuleDetails)
        {
            if (accessRuleDetails.IsAccessRuleReport)
            {
                AuditLog.OnChangeAccessRuleQuery(success, accessRuleDetails.SubjectName, accessRuleDetails.SecuredTypeName, accessRuleDetails.AccessRuleReportName);
            }
        }


        #region Nested type: AuditLogReportDetails


        /// <summary>
        /// </summary>
        public class AuditLogReportDetails
        {
            /// <summary>
            ///     Gets or sets the name of the subject.
            /// </summary>
            /// <value>
            ///     The name of the subject.
            /// </value>
            public string SubjectName { get; set; }


            /// <summary>
            ///     Gets or sets the name of the secured type.
            /// </summary>
            /// <value>
            ///     The name of the secured type.
            /// </value>
            public string SecuredTypeName { get; set; }


            /// <summary>
            ///     Gets or sets the name of the access rule report.
            /// </summary>
            /// <value>
            ///     The name of the access rule report.
            /// </value>
            public string AccessRuleReportName { get; set; }


            /// <summary>
            ///     Gets or sets a value indicating whether this instance is an access rule report.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance is an access rule report; otherwise, <c>false</c>.
            /// </value>
            public bool IsAccessRuleReport { get; set; }
        }


        #endregion
    }
}