// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    /// Audit log role event target
    /// </summary>
    internal class AuditLogRoleEventTarget : AuditLogEventTargetBase<Role, AuditLogRoleEventTarget.AuditLogRoleDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogRoleEventTarget"/> class.
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        public AuditLogRoleEventTarget(IAuditLog auditLog) : base(auditLog)
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
            get { return "AuditLogRoleEventTarget:AuditLogSaveRoleDetails"; }
        }


        /// <summary>
        ///     Gets the name of delete details state key.
        /// </summary>
        /// <value>
        ///     The delete details state key.
        /// </value>
        protected override string DeleteDetailsStateKey
        {
            get { return "AuditLogRoleEventTarget:AuditLogDeleteRoleDetails"; }
        }


        /// <summary>
        ///     Called to gather audit log entity details for save.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        protected override AuditLogRoleDetails OnGatherAuditLogEntityDetailsForSave(Role role)
        {
            var roleInternal = role as IEntityInternal;
            IEntityFieldValues fields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;
            role.GetChanges(out fields, out forwardRelationships, out reverseRelationships);

            var oldRole = new Lazy<Role>(() => Entity.Get<Role>(role.Id));

            var roleDetails = new AuditLogRoleDetails {RoleName = role.Name, OldRoleName = role.Name, IsTemporaryId = roleInternal.IsTemporaryId};

            IEnumerable<EntityRef> idsToLoad = new List<EntityRef> {"core:name", "core:includesRoles", "core:userHasRole"};

            Dictionary<string, IEntity> fieldEntities = Entity.Get(idsToLoad).ToDictionary(e => e.Alias);

            if (fields != null && fields.Any())
            {
                object fieldObj;

                if (fields.TryGetValue(fieldEntities["name"].Id, out fieldObj))
                {
                    // Name was changed
                    roleDetails.OldRoleName = oldRole.Value.Name;
                }
            }

			if ( forwardRelationships != null && forwardRelationships.Count > 0 )
            {
                IChangeTracker<IMutableIdKey> includesRolesTracker;

                if (forwardRelationships.TryGetValue(fieldEntities["includesRoles"].Id, out includesRolesTracker))
                {
                    IEnumerable<long> addedIds = includesRolesTracker.Added.Select(a => a.Key);
                    roleDetails.AddedMembers.UnionWith(Entity.Get<Resource>(addedIds).Select(e => e.Name));

                    IEnumerable<long> removedIds = includesRolesTracker.Removed.Select(a => a.Key);
                    roleDetails.RemovedMembers.UnionWith(Entity.Get<Resource>(removedIds).Select(e => e.Name));
                }
            }

			if ( reverseRelationships != null && reverseRelationships.Count > 0 )
            {
                IChangeTracker<IMutableIdKey> userTracker;

                if (reverseRelationships.TryGetValue(fieldEntities["userHasRole"].Id, out userTracker))
                {
                    IEnumerable<long> addedIds = userTracker.Added.Select(a => a.Key);
                    roleDetails.AddedMembers.UnionWith(Entity.Get<Resource>(addedIds).Select(e => e.Name));

                    IEnumerable<long> removedIds = userTracker.Removed.Select(a => a.Key);
                    roleDetails.RemovedMembers.UnionWith(Entity.Get<Resource>(removedIds).Select(e => e.Name));
                }

                IChangeTracker<IMutableIdKey> includedByRoleTracker;

                if (reverseRelationships.TryGetValue(fieldEntities["includesRoles"].Id, out includedByRoleTracker))
                {
                    IEnumerable<long> addedIds = includedByRoleTracker.Added.Select(a => a.Key);
                    roleDetails.AddedIncludedByRoles.UnionWith(Entity.Get<Resource>(addedIds).Select(e => e.Name));

                    IEnumerable<long> removedIds = includedByRoleTracker.Removed.Select(a => a.Key);
                    roleDetails.RemovedIncludedByRoles.UnionWith(Entity.Get<Resource>(removedIds).Select(e => e.Name));
                }
            }

            return roleDetails;
        }


        /// <summary>
        ///     Called to gather audit log entity details for delete.
        /// </summary>
        /// <param name="accessRule">The role.</param>
        /// <returns></returns>
        protected override AuditLogRoleDetails OnGatherAuditLogEntityDetailsForDelete(Role accessRule)
        {
            return new AuditLogRoleDetails {RoleName = accessRule.Name, IsTemporaryId = false};
        }


        /// <summary>
        ///     Called to write save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The role details.</param>
        protected override void OnWriteSaveAuditLogEntries(bool success, AuditLogRoleDetails accessRuleDetails)
        {
            if (accessRuleDetails.IsTemporaryId)
            {
                // Role is being created
                AuditLog.OnCreateUserRole(success, accessRuleDetails.RoleName);
            }

            if (!accessRuleDetails.IsTemporaryId &&
                accessRuleDetails.OldRoleName != accessRuleDetails.RoleName)
            {
                // Name has changed
                AuditLog.OnRenameUserRole(success, accessRuleDetails.OldRoleName, accessRuleDetails.RoleName);
            }

			if ( accessRuleDetails.AddedMembers.Count > 0 || accessRuleDetails.RemovedMembers.Count > 0 )
            {
                // Role membership has changed
                AuditLog.OnChangeUserRoleMembers(success, accessRuleDetails.RoleName, accessRuleDetails.AddedMembers, accessRuleDetails.RemovedMembers);
            }

            ISet<string> currentRole = new HashSet<string> {accessRuleDetails.RoleName};
			if ( accessRuleDetails.AddedIncludedByRoles.Count > 0 )
            {
                foreach (string addedIncludedByRole in accessRuleDetails.AddedIncludedByRoles)
                {
                    // Role membership has changed
                    AuditLog.OnChangeUserRoleMembers(success, addedIncludedByRole, currentRole, new SortedSet<string>());
                }
            }

			if ( accessRuleDetails.RemovedIncludedByRoles.Count > 0 )
            {
                foreach (string removedIncludedByRole in accessRuleDetails.RemovedIncludedByRoles)
                {
                    // Role membership has changed
                    AuditLog.OnChangeUserRoleMembers(success, removedIncludedByRole, new SortedSet<string>(), currentRole);
                }
            }
        }


        /// <summary>
        ///     Called to write delete audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The role details.</param>
        protected override void OnWriteDeleteAuditLogEntries(bool success, AuditLogRoleDetails accessRuleDetails)
        {
            AuditLog.OnDeleteUserRole(success, accessRuleDetails.RoleName);
        }


        #region Nested type: AuditLogRoleDetails


        /// <summary>
        ///     This class is used to hold audit log role changes.
        /// </summary>
        internal class AuditLogRoleDetails
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="AuditLogRoleDetails" /> class.
            /// </summary>
            public AuditLogRoleDetails()
            {
                AddedMembers = new SortedSet<string>();
                RemovedMembers = new SortedSet<string>();
                AddedIncludedByRoles = new SortedSet<string>();
                RemovedIncludedByRoles = new SortedSet<string>();
            }


            /// <summary>
            ///     Gets or sets the name of the role.
            /// </summary>
            /// <value>
            ///     The name of the role.
            /// </value>
            public string RoleName { get; set; }


            /// <summary>
            ///     Gets or sets the old name of the role.
            /// </summary>
            /// <value>
            ///     The old name of the role.
            /// </value>
            public string OldRoleName { get; set; }


            /// <summary>
            ///     Gets or sets a value indicating whether this instance has a temporary identifier.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance has a temporary identifier; otherwise, <c>false</c>.
            /// </value>
            public bool IsTemporaryId { get; set; }


            /// <summary>
            ///     Gets or sets the added members.
            /// </summary>
            /// <value>
            ///     The added members.
            /// </value>
            public ISet<string> AddedMembers { get; private set; }


            /// <summary>
            ///     Gets or sets the removed members.
            /// </summary>
            /// <value>
            ///     The removed members.
            /// </value>
            public ISet<string> RemovedMembers { get; private set; }


            /// <summary>
            ///     Gets or sets the added included by roles.
            /// </summary>
            /// <value>
            ///     The added included by roles.
            /// </value>
            public ISet<string> AddedIncludedByRoles { get; private set; }


            /// <summary>
            ///     Gets or sets the removed included by roles.
            /// </summary>
            /// <value>
            ///     The removed included by roles.
            /// </value>
            public ISet<string> RemovedIncludedByRoles { get; private set; }
        }


        #endregion
    }
}