// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    ///     Audit log access rule event target
    /// </summary>
    internal class AuditLogAccessRuleEventTarget : AuditLogEventTargetBase<AccessRule, AuditLogAccessRuleEventTarget.AuditLogAccessRuleDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogAccessRuleEventTarget"/> class.
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        public AuditLogAccessRuleEventTarget(IAuditLog auditLog) : base(auditLog)
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
            get { return "AuditLogAccessRuleEventTarget:AuditLogSaveAccessRuleDetails"; }
        }


        /// <summary>
        ///     Gets the name of delete details state key.
        /// </summary>
        /// <value>
        ///     The delete details state key.
        /// </value>
        protected override string DeleteDetailsStateKey
        {
            get { return "AuditLogAccessRuleEventTarget:AuditLogDeleteAccessRuleDetails"; }
        }


        /// <summary>
        ///     Called to gather audit log entity details for save.
        /// </summary>
        /// <param name="accessRule">The access rule.</param>
        /// <returns></returns>
        protected override AuditLogAccessRuleDetails OnGatherAuditLogEntityDetailsForSave(AccessRule accessRule)
        {
            var accessRuleInternal = accessRule as IEntityInternal;
            IEntityFieldValues fields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;
            accessRule.GetChanges(out fields, out forwardRelationships, out reverseRelationships);

            var oldAccessRule = new Lazy<AccessRule>(() => Entity.Get<AccessRule>(accessRule.Id));

            IEnumerable<EntityRef> idsToLoad = new List<EntityRef> {"core:accessRuleEnabled", "core:permissionAccess", "core:accessRuleReport"};

            Dictionary<string, IEntity> fieldEntities = Entity.Get(idsToLoad).ToDictionary(e => e.Alias);

            var accessRuleDetails = new AuditLogAccessRuleDetails {IsTemporaryId = accessRuleInternal.IsTemporaryId};

            if (fields != null && fields.Any())
            {
                object fieldObj;

                if (fields.TryGetValue(fieldEntities["accessRuleEnabled"].Id, out fieldObj))
                {
                    // Enabled was changed
                    accessRuleDetails.Enabled = fieldObj as bool?;
                    accessRuleDetails.OldEnabled = oldAccessRule.Value.AccessRuleEnabled;
                }
            }

            SecurableEntity controlAccess = accessRule.ControlAccess;
            if (controlAccess != null)
            {
                accessRuleDetails.SecuredTypeName = controlAccess.Name;
            }

            Subject allowAccessBy = accessRule.AllowAccessBy;
            if (allowAccessBy != null)
            {
                accessRuleDetails.SubjectName = allowAccessBy.Name;
            }

            Report accessRuleReport = accessRule.AccessRuleReport;
            if (accessRuleReport != null)
            {
                accessRuleDetails.AccessRuleReportName = accessRuleReport.Name;
            }

			if ( forwardRelationships != null && forwardRelationships.Count > 0 )
            {
                IChangeTracker<IMutableIdKey> permissionsTracker;

                if (forwardRelationships.TryGetValue(fieldEntities["permissionAccess"].Id, out permissionsTracker))
                {
                    IEntityCollection<Permission> oldPermissions = oldAccessRule.Value.PermissionAccess;
                    if (oldPermissions != null)
                    {
                        accessRuleDetails.OldPermissions.UnionWith(oldPermissions.Select(e => e.Name));
                    }                  

                    IEntityCollection<Permission> newPermissions = accessRule.PermissionAccess;
                    if (newPermissions != null)
                    {
                        accessRuleDetails.NewPermissions.UnionWith(newPermissions.Select(e => e.Name));   
                    }                    
                }

                IChangeTracker<IMutableIdKey> reportTracker;

                if (forwardRelationships.TryGetValue(fieldEntities["accessRuleReport"].Id, out reportTracker))
                {
                    accessRuleDetails.IsAccessRuleReportChanged = true;
                }
            }

            return accessRuleDetails;
        }


        /// <summary>
        ///     Called to gather audit log entity details for delete.
        /// </summary>
        /// <param name="accessRule">The access rule.</param>
        /// <returns></returns>
        protected override AuditLogAccessRuleDetails OnGatherAuditLogEntityDetailsForDelete(AccessRule accessRule)
        {
            var accessRuleDetails = new AuditLogAccessRuleDetails {IsTemporaryId = false};

            SecurableEntity controlAccess = accessRule.ControlAccess;
            if (controlAccess != null)
            {
                accessRuleDetails.SecuredTypeName = controlAccess.Name;
            }

            Subject allowAccessBy = accessRule.AllowAccessBy;
            if (allowAccessBy != null)
            {
                accessRuleDetails.SubjectName = allowAccessBy.Name;
            }

            Report accessRuleReport = accessRule.AccessRuleReport;
            if (accessRuleReport != null)
            {
                accessRuleDetails.AccessRuleReportName = accessRuleReport.Name;
            }

            return accessRuleDetails;
        }


        /// <summary>
        ///     Called to write save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The access rule details.</param>
        protected override void OnWriteSaveAuditLogEntries(bool success, AuditLogAccessRuleDetails accessRuleDetails)
        {
            if (accessRuleDetails.IsTemporaryId)
            {
                // Access rule is being created
                AuditLog.OnCreateAccessRule(success, accessRuleDetails.SubjectName, accessRuleDetails.SecuredTypeName);
            }

            if (!accessRuleDetails.IsTemporaryId &&
				accessRuleDetails.OldPermissions.Count > 0 || accessRuleDetails.NewPermissions.Count > 0 )
            {
                // Access rule permissions have changed
                AuditLog.OnChangeAccessRulePermissions(success, accessRuleDetails.SubjectName, accessRuleDetails.SecuredTypeName,
                    accessRuleDetails.AccessRuleReportName, accessRuleDetails.OldPermissions, accessRuleDetails.NewPermissions);
            }

            if (!accessRuleDetails.IsTemporaryId &&
                accessRuleDetails.Enabled != accessRuleDetails.OldEnabled)
            {
                // Access rule enabled state has changed
                AuditLog.OnEnableAccessRule(success, accessRuleDetails.SubjectName, accessRuleDetails.SecuredTypeName, accessRuleDetails.AccessRuleReportName,
                    accessRuleDetails.OldEnabled, accessRuleDetails.Enabled);
            }

            if (!accessRuleDetails.IsTemporaryId &&
                accessRuleDetails.IsAccessRuleReportChanged)
            {
                // Access rule query has changed
                AuditLog.OnChangeAccessRuleQuery(success, accessRuleDetails.SubjectName, accessRuleDetails.SecuredTypeName, accessRuleDetails.AccessRuleReportName);
            }
        }


        /// <summary>
        ///     Called to write delete audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The access rule details.</param>
        protected override void OnWriteDeleteAuditLogEntries(bool success, AuditLogAccessRuleDetails accessRuleDetails)
        {
            AuditLog.OnDeleteAccessRule(success, accessRuleDetails.SubjectName, accessRuleDetails.SecuredTypeName, accessRuleDetails.AccessRuleReportName);
        }


        #region Nested type: AuditLogAccessRuleDetails


        /// <summary>
        ///     Audit log access rule changes.
        /// </summary>
        internal class AuditLogAccessRuleDetails
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="AuditLogAccessRuleDetails" /> class.
            /// </summary>
            public AuditLogAccessRuleDetails()
            {
                OldPermissions = new SortedSet<string>();
                NewPermissions = new SortedSet<string>();
            }


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
            ///     Gets or sets a value indicating whether this instance is access rule report changed.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance is access rule report changed; otherwise, <c>false</c>.
            /// </value>
            public bool IsAccessRuleReportChanged { get; set; }


            /// <summary>
            ///     Gets or sets the enabled.
            /// </summary>
            /// <value>
            ///     The enabled.
            /// </value>
            public bool? Enabled { get; set; }


            /// <summary>
            ///     Gets or sets the old enabled.
            /// </summary>
            /// <value>
            ///     The old enabled.
            /// </value>
            public bool? OldEnabled { get; set; }


            /// <summary>
            ///     Gets or sets a value indicating whether this instance is temporary identifier.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance is temporary identifier; otherwise, <c>false</c>.
            /// </value>
            public bool IsTemporaryId { get; set; }


            /// <summary>
            /// Gets the old permissions.
            /// </summary>
            /// <value>
            /// The old permissions.
            /// </value>
            public ISet<string> OldPermissions { get; private set; }


            /// <summary>
            /// Gets the new permissions.
            /// </summary>
            /// <value>
            /// The new permissions.
            /// </value>
            public ISet<string> NewPermissions { get; private set; }
        }


        #endregion
    }
}