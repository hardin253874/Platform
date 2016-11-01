// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Core.FeatureSwitch;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    /// <summary>
    /// A repository for console navigation trees.
    /// </summary>
    public class ConsoleTreeRepository : IConsoleTreeRepository
    {
        private ConcurrentDictionary<long, EntityRef> tenantToInstancesOfType;
        private ConcurrentDictionary<long, EntityRef> tenantToFolderContents;

        /// <summary>
        /// Create a new <see cref="ConsoleTreeRepository"/>.
        /// </summary>
        /// <param name="userRoleRepository">
        /// The <see cref="IUserRoleRepository"/> used to determine the
        /// user's roles. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="userRoleRepository"/> cannot be null.
        /// </exception>
        public ConsoleTreeRepository(IUserRoleRepository userRoleRepository)
        {
            if ( userRoleRepository == null )
            {
                throw new ArgumentNullException( nameof(userRoleRepository) );
            }

            UserRoleRepository = userRoleRepository;
            FeatureSwitch = Factory.FeatureSwitch;
            tenantToInstancesOfType = new ConcurrentDictionary<long, EntityRef>();
            tenantToFolderContents = new ConcurrentDictionary<long, EntityRef>();
        }

        /// <summary>
        /// An <see cref="EntityRef"/> for "core:instancesOfType".
        /// </summary>
        public EntityRef InstancesOfType
        {
            get
            {
                if (!ReadiNow.IO.RequestContext.IsSet)
                {
                    throw new InvalidOperationException("Request context not set");
                }

                return tenantToInstancesOfType.GetOrAdd(
                    ReadiNow.IO.RequestContext.TenantId,
                    tenantId => new EntityRef("core:instancesOfType"));
            }
        }

        /// <summary>
        /// An <see cref="EntityRef"/> for "console:folderContents".
        /// </summary>
        public EntityRef FolderContents
        {
            get
            {
                if (!ReadiNow.IO.RequestContext.IsSet)
                {
                    throw new InvalidOperationException("Request context not set");
                }

                return tenantToFolderContents.GetOrAdd(
                    ReadiNow.IO.RequestContext.TenantId,
                    tenantId => new EntityRef("console:folderContents"));
            }
        }

        /// <summary>
        /// The <see cref="IUserRoleRepository"/> used to determine the user's roles.
        /// </summary>
        public IUserRoleRepository UserRoleRepository { get; }

        /// <summary>
        /// The <see cref="IFeatureSwitch"/> service.
        /// </summary>
        public IFeatureSwitch FeatureSwitch { get; }

        /// <summary>
        /// The ID of the nav feature switch field.
        /// </summary>
        private long _navFeatureSwitchId = -1;

        /// <summary>
        /// The ID of the nav feature switch field.
        /// </summary>
        private long NavFeatureSwitchId
        {
            get
            {
                if ( _navFeatureSwitchId == -1 )
                {
                    try
                    {
                        _navFeatureSwitchId = new EntityRef( "core:navFeatureSwitch" ).Id;
                    }
                    catch
                    {
                        _navFeatureSwitchId = 0;
                    }
                }
                return _navFeatureSwitchId;
            }
        }


        /// <summary>
        /// Build and return the console navigation tree.
        /// </summary>
        /// <returns>
        /// A <see cref="EntityData"/> containing the tree structure.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ReadiNow.IO.RequestContext"/> is not set.
        /// </exception>
        public EntityData GetTree()
        {
            if (!ReadiNow.IO.RequestContext.IsSet)
            {
                throw new InvalidOperationException("Request context not set");
            }

            EntityData entityData;
            const string hintText = "navItems";

            using (new SecurityBypassContext())
            using (Profiler.Measure("ConsoleTreeRepository.GetTree"))
            {
                // Construct and run the query
                EntityRequest entityRequest = new EntityRequest
                {
                    Entities = new EntityRef(WellKnownAliases.CurrentTenant.TopMenu).ToEnumerable( ),
                    RequestString = ConsoleTreeRequest,
                    Hint = hintText,
                    IgnoreResultCache = true // this data is cached by the CachingConsoleTreeRepository
                };
                entityData = BulkRequestRunner.GetEntities( entityRequest ).FirstOrDefault();

                // It's not completely clear why we are using SecurityBypassContext and manually checking, rather than relying on the security model.
                // One possible explanation is that the 'securesTo' flags cause default reports and picker reports to be readable, which may be undesirable in the UI

                // If the user is not an administrator, remove entries without the "core:allowDisplay" relationship
                // to either their own user account or a role they are in.                
                Prune( entityData, ReadiNow.IO.RequestContext.GetContext().Identity.Id );

                return entityData;
            }
        }

        /// <summary>
        /// Remove entities that lack a "core:allowDisplay" relationship to the current user
        /// or the user's roles.
        /// </summary>
        /// <param name="entityData">The <see cref="EntityData"/> to start at. This cannot be null.</param>
        /// <param name="userId">The ID of the user.</param>        
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityData"/> cannot be null.
        /// </exception>
        public void Prune(EntityData entityData, long userId)
        {
            if (entityData == null)
            {
                throw new ArgumentNullException("entityData");
            }

            using (Profiler.Measure("ConsoleTreeRepository.Prune"))
            {
                ISet<long> subjectIds;
                Queue<EntityData> entityQueue;
                bool firstTime;
                HashSet<long> visited;
                
                subjectIds = new HashSet<long>(UserRoleRepository.GetUserRoles(userId))
                {
                    userId
                };

                bool userIsAdmin = subjectIds.Contains(WellKnownAliases.CurrentTenant.AdministratorRole);                

                entityQueue = new Queue<EntityData>(new[] {entityData});
                firstTime = true;
                visited = new HashSet<long>();
				while ( entityQueue.Count > 0 )
                {
                    EntityData current;
                    RelationshipData relationshipData;

                    current = entityQueue.Dequeue();
                    relationshipData = current.Relationships.SingleOrDefault(
                        r => r != null
                             && r.RelationshipTypeId != null
                             && r.RelationshipTypeId.Alias == (firstTime ? InstancesOfType : FolderContents).Alias);
                    firstTime = false;
                    if (relationshipData != null)
                    {
                        foreach (
                            RelationshipInstanceData instance in
                                new List<RelationshipInstanceData>(relationshipData.Instances))
                        {
                            if ( !CanSeeElement( instance.Entity, subjectIds, userId, userIsAdmin))
                            {
                                relationshipData.Instances.Remove(instance);
                            }
                            else
                            {
                                if (!visited.Contains(instance.Entity.Id.Id))
                                {
                                    entityQueue.Enqueue(instance.Entity);
                                    visited.Add(instance.Entity.Id.Id);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove entities that lack a "core:allowDisplay" relationship to any of the
        /// given <paramref name="subjectIds"/>.
        /// </summary>
        /// <param name="entityData">
        /// The <see cref="EntityData"/> to check. This cannot be null.
        /// </param>
        /// <param name="subjectIds">
        /// The subject IDs to check for. This cannot be null.
        /// </param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="userIsAdmin">True if the user is an admin</param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public bool CanSeeElement(EntityData entityData, ISet<long> subjectIds, long userId, bool userIsAdmin)
        {
            if (entityData == null)
            {
                throw new ArgumentNullException("entityData");
            }
            if (subjectIds == null)
            {
                throw new ArgumentNullException("subjectIds");
            }

            // Check for feature switches
            string navFeatureSwitch = entityData.GetField( NavFeatureSwitchId )?.Value?.ValueString;
            if ( navFeatureSwitch != null )
            {
                if ( !FeatureSwitch.Get( navFeatureSwitch ) )
                    return false;
            }

            if ( userIsAdmin )
            {
                FieldData isPrivatelyOwnedFieldData = entityData.GetField( WellKnownAliases.CurrentTenant.IsPrivatelyOwned );

                bool isPrivatelyOwned = isPrivatelyOwnedFieldData != null && (bool?)isPrivatelyOwnedFieldData.Value.Value == true;

                if ( !isPrivatelyOwned )
                    return true; // admin can see all public elements
            }
            else
            {                
                RelationshipData allowDisplayRelationshipData;

                allowDisplayRelationshipData = entityData.GetRelationship( WellKnownAliases.CurrentTenant.AllowDisplay, Direction.Reverse );

                bool allowDisplay = allowDisplayRelationshipData != null && allowDisplayRelationshipData.Entities.Any( e => subjectIds.Contains( e.Id.Id ) );
                if ( allowDisplay )
                    return true;
            }

            // Finally, in both cases check if the nav item is owned by the user

            RelationshipData securityOwnerRelationshipData = entityData.GetRelationship( WellKnownAliases.CurrentTenant.SecurityOwner, Direction.Forward );

            bool securityOwner = securityOwnerRelationshipData != null && securityOwnerRelationshipData.Entities.Any( e => userId == e.Id.Id );
            if ( securityOwner )
                return true;

            return false;
        }

        public string ConsoleTreeRequest = @"
            let @ALIAS = { alias }
            let @ALIASNAME = { alias, name }
            let @NAVTYPE = {
                alias, name,
                k:typeConsoleBehavior.@BEHAVIOR
            }
            let @BEHAVIOR = {
                alias, name, description,
                isOfType.@ALIASNAME,
                k:treeIconUrl,
                k:treeIcon.{ name, imageBackgroundColor},
                k:treeIconBackgroundColor,
                k:html5ViewId,
                k:consoleBehaviorHidden
            }
            let @REPORT = {
                reportUsesDefinition.@NAVTYPE
            }
            let @NAVELEMENT = {
                alias, name, description,
                isOfType.@NAVTYPE,
                k:resourceConsoleBehavior.@BEHAVIOR,
                k:folderContents.@NAVELEMENT,
                k:consoleOrder,
                k:isTopMenuVisible,
                k:showApplicationTabs,
                inSolution.name,
                k:isAppTab,
                k:navigationElementIcon.{ alias, name, imageBackgroundColor},
                k:navElementTreeIconBackgroundColor,
                reportUsesDefinition.@NAVTYPE,
                chartReport.@REPORT,
                boardReport.@REPORT,
                hideOnDesktop, hideOnTablet, hideOnMobile,
                k:resourceInFolder.@ALIASNAME,
                allowedDisplayBy.@ALIAS,
                securityOwner.id,
                isPrivatelyOwned,
                navFeatureSwitch
            }
            instancesOfType.@NAVELEMENT";

    }
}