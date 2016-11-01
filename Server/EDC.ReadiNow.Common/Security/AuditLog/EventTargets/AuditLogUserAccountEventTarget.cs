// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    ///     User account audit log event target
    /// </summary>
    internal class AuditLogUserAccountEventTarget : AuditLogEventTargetBase<UserAccount, AuditLogUserAccountEventTarget.AuditLogUserAccountDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogUserAccountEventTarget"/> class.
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        public AuditLogUserAccountEventTarget(IAuditLog auditLog) : base(auditLog)
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
            get { return "AuditLogUserAccountEventTarget:AuditLogSaveUserAccountDetails"; }
        }


        /// <summary>
        ///     Gets the name of delete details state key.
        /// </summary>
        /// <value>
        ///     The delete details state key.
        /// </value>
        protected override string DeleteDetailsStateKey
        {
            get { return "AuditLogUserAccountEventTarget:AuditLogDeleteUserAccountDetails"; }
        }


        /// <summary>
        ///     Called to gather audit log entity details for delete.
        /// </summary>
        /// <param name="accessRule">The user account.</param>
        /// <returns></returns>
        protected override AuditLogUserAccountDetails OnGatherAuditLogEntityDetailsForDelete(UserAccount accessRule)
        {
            return new AuditLogUserAccountDetails {UserName = accessRule.Name, IsTemporaryId = false};
        }


        /// <summary>
        ///     Called to gather audit log entity details for save.
        /// </summary>
        /// <param name="userAccount">The user account.</param>
        /// <returns></returns>
        protected override AuditLogUserAccountDetails OnGatherAuditLogEntityDetailsForSave(UserAccount userAccount)
        {
            var userAccountInternal = userAccount as IEntityInternal;
            IEntityFieldValues fields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;
            userAccount.GetChanges(out fields, out forwardRelationships, out reverseRelationships);

            var oldUserAccount = new Lazy<UserAccount>(() => Entity.Get<UserAccount>(userAccount.Id));

            var userAccountDetails = new AuditLogUserAccountDetails {UserName = userAccount.Name, OldUserName = userAccount.Name, IsTemporaryId = userAccountInternal.IsTemporaryId};

            IEnumerable<EntityRef> idsToLoad = new List<EntityRef> {"core:name", "core:password", "core:accountExpiration", "core:accountStatus", "core:userHasRole"};

            Dictionary<string, IEntity> fieldEntities = Entity.Get(idsToLoad).ToDictionary(e => e.Alias);

            if (fields != null && fields.Any())
            {
                object fieldObj;

                if (fields.TryGetValue(fieldEntities["name"].Id, out fieldObj))
                {
                    // Name was changed
                    userAccountDetails.OldUserName = oldUserAccount.Value.Name;
                }

                if (fields.TryGetValue(fieldEntities["password"].Id, out fieldObj))
                {
                    var newPassword = fieldObj as string;

                    // Password was changed                    
                    if (newPassword != oldUserAccount.Value.Password)
                    {
                        userAccountDetails.HasPasswordChanged = true;
                    }
                }

                if (fields.TryGetValue(fieldEntities["accountExpiration"].Id, out fieldObj))
                {
                    // Account expiration was changed                    
                    userAccountDetails.ExpirationDate = fieldObj as DateTime?;
                    userAccountDetails.OldExpirationDate = oldUserAccount.Value.AccountExpiration;
                }
            }

			if ( forwardRelationships != null && forwardRelationships.Count > 0 )
            {
                IChangeTracker<IMutableIdKey> accountStatusTracker;

                if (forwardRelationships.TryGetValue(fieldEntities["accountStatus"].Id, out accountStatusTracker))
                {
                    userAccountDetails.Status = userAccount.AccountStatus_Enum;
                    userAccountDetails.OldStatus = oldUserAccount.Value.AccountStatus_Enum;
                }

                IChangeTracker<IMutableIdKey> userRoleTracker;

                if (forwardRelationships.TryGetValue(fieldEntities["userHasRole"].Id, out userRoleTracker))
                {
                    IEnumerable<long> addedIds = userRoleTracker.Added.Select(a => a.Key);
                    userAccountDetails.AddedUserHasRoles.UnionWith(Entity.Get<Resource>(addedIds).Select(e => e.Name));

                    IEnumerable<long> removedIds = userRoleTracker.Removed.Select(a => a.Key);
                    userAccountDetails.RemovedUserHasRoles.UnionWith(Entity.Get<Resource>(removedIds).Select(e => e.Name));
                }
            }

            return userAccountDetails;
        }


        /// <summary>
        ///     Called to write delete audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The user account details.</param>
        protected override void OnWriteDeleteAuditLogEntries(bool success, AuditLogUserAccountDetails accessRuleDetails)
        {
            AuditLog.OnDeleteUserAccount(success, accessRuleDetails.UserName);
        }


        /// <summary>
        ///     Called to write save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The user account details.</param>
        protected override void OnWriteSaveAuditLogEntries(bool success, AuditLogUserAccountDetails accessRuleDetails)
        {
            if (accessRuleDetails.IsTemporaryId)
            {
                // Entity is being created
                AuditLog.OnCreateUserAccount(success, accessRuleDetails.UserName);
            }

            if (!accessRuleDetails.IsTemporaryId &&
                accessRuleDetails.OldUserName != accessRuleDetails.UserName)
            {
                // Name has changed
                AuditLog.OnRenameUserAccount(success, accessRuleDetails.OldUserName, accessRuleDetails.UserName);
            }

            if (!accessRuleDetails.IsTemporaryId &&
                accessRuleDetails.HasPasswordChanged)
            {
                // Password has changed
                AuditLog.OnChangeUserAccountPassword(success, accessRuleDetails.UserName);
            }

            if (!accessRuleDetails.IsTemporaryId &&
                accessRuleDetails.ExpirationDate != accessRuleDetails.OldExpirationDate)
            {
                // Expiration date has changed
                AuditLog.OnChangeUserAccountExpiry(success, accessRuleDetails.UserName, accessRuleDetails.OldExpirationDate, accessRuleDetails.ExpirationDate);
            }

            if (!accessRuleDetails.IsTemporaryId &&
                accessRuleDetails.Status != accessRuleDetails.OldStatus)
            {
                // Status has changed
                switch (accessRuleDetails.Status)
                {
                    case UserAccountStatusEnum_Enumeration.Expired:
                        AuditLog.OnExpiredUserAccount(success, accessRuleDetails.UserName);
                        break;
                    case UserAccountStatusEnum_Enumeration.Locked:
                        AuditLog.OnLockUserAccount(success, accessRuleDetails.UserName);
                        break;
                    default:
                        AuditLog.OnChangeUserAccountStatus(success, accessRuleDetails.UserName, accessRuleDetails.OldStatus.ToString(), accessRuleDetails.Status.ToString());
                        break;
                }
            }

            ISet<string> currentAccount = new HashSet<string> {accessRuleDetails.UserName};
			if ( accessRuleDetails.AddedUserHasRoles.Count > 0 )
            {
                foreach (string addedUserHasRole in accessRuleDetails.AddedUserHasRoles)
                {
                    // Role membership has changed
                    AuditLog.OnChangeUserRoleMembers(success, addedUserHasRole, currentAccount, new SortedSet<string>());
                }
            }

			if ( accessRuleDetails.RemovedUserHasRoles.Count > 0 )
            {
                foreach (string removedUserHasRole in accessRuleDetails.RemovedUserHasRoles)
                {
                    // Role membership has changed
                    AuditLog.OnChangeUserRoleMembers(success, removedUserHasRole, new SortedSet<string>(), currentAccount);
                }
            }
        }


        #region Nested type: AuditLogUserAccountDetails


        /// <summary>
        ///     This class is used to hold audit log user account changes.
        /// </summary>
        internal class AuditLogUserAccountDetails
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="AuditLogUserAccountEventTarget.AuditLogUserAccountDetails" /> class.
            /// </summary>
            public AuditLogUserAccountDetails()
            {
                AddedUserHasRoles = new SortedSet<string>();
                RemovedUserHasRoles = new SortedSet<string>();
            }


            /// <summary>
            ///     Gets or sets the name of the user.
            /// </summary>
            /// <value>
            ///     The name of the user.
            /// </value>
            public string UserName { get; set; }


            /// <summary>
            ///     Gets or sets the old name of the user.
            /// </summary>
            /// <value>
            ///     The old name of the user.
            /// </value>
            public string OldUserName { get; set; }


            /// <summary>
            ///     Gets or sets a value indicating whether this instance has a temporary identifier.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance has a temporary identifier; otherwise, <c>false</c>.
            /// </value>
            public bool IsTemporaryId { get; set; }


            /// <summary>
            ///     Gets or sets a value indicating whether this instance has password changed.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance has password changed; otherwise, <c>false</c>.
            /// </value>
            public bool HasPasswordChanged { get; set; }


            /// <summary>
            ///     Gets or sets the status.
            /// </summary>
            /// <value>
            ///     The status.
            /// </value>
            public UserAccountStatusEnum_Enumeration? Status { get; set; }


            /// <summary>
            ///     Gets or sets the old status.
            /// </summary>
            /// <value>
            ///     The old status.
            /// </value>
            public UserAccountStatusEnum_Enumeration? OldStatus { get; set; }


            /// <summary>
            ///     Gets or sets the expiration date.
            /// </summary>
            /// <value>
            ///     The expiration date.
            /// </value>
            public DateTime? ExpirationDate { get; set; }


            /// <summary>
            ///     Gets or sets the old expiration date.
            /// </summary>
            /// <value>
            ///     The old expiration date.
            /// </value>
            public DateTime? OldExpirationDate { get; set; }


            /// <summary>
            ///     Gets or sets the added user has roles.
            /// </summary>
            /// <value>
            ///     The added user has roles.
            /// </value>
            public ISet<string> AddedUserHasRoles { get; private set; }


            /// <summary>
            ///     Gets or sets the removed user has roles.
            /// </summary>
            /// <value>
            ///     The removed user has roles.
            /// </value>
            public ISet<string> RemovedUserHasRoles { get; private set; }
        }


        #endregion
    }
}