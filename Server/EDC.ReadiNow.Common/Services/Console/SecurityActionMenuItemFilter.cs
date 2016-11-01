// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// Filter (remove) menu items that theuser cannot see.
    /// </summary>
    public class SecurityActionMenuItemFilter
    {
        /// <summary>
        /// Create a new <see cref="SecurityActionMenuItemFilter"/>.
        /// </summary>
        /// <param name="entityAccessControlService">
        /// The <see cref="IEntityAccessControlService"/> to perform security checks.
        /// If omitted or null, the default implementation is used.
        /// </param>
        /// <param name="userRoleRepository">
        /// The <see cref="IUserRoleRepository"/> to perform security checks.
        /// If omitted or null, the default implementation is used.
        /// </param>
        public SecurityActionMenuItemFilter(IEntityAccessControlService entityAccessControlService = null, IUserRoleRepository userRoleRepository = null)
        {
            Service = entityAccessControlService ?? Factory.EntityAccessControlService;
            UserRoleRepository = userRoleRepository ?? Factory.Current.Resolve<IUserRoleRepository>( );
        }

        /// <summary>
        /// The <see cref="IEntityAccessControlService"/> used.
        /// </summary>
        public IEntityAccessControlService Service { get; }

        /// <summary>
        /// The <see cref="IUserRoleRepository"/> used.
        /// </summary>
        public IUserRoleRepository UserRoleRepository { get; }

        /// <summary>
        /// Remove menu items the user does not have access to see/use.
        /// </summary>
        /// <param name="parentEntityId">
        /// Entity being viewed/edited in edit form.
        /// </param>
        /// <param name="selectedResourceIds">
        /// The resource IDs the menu items are acting on. This cannot be null.
        /// </param>
        /// <param name="actions">
        /// Menu items to filter. This cannot be null or contain null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actions"/> cannot be null or contain null.
        /// </exception>
        public void Filter(long parentEntityId, IList<long> selectedResourceIds, IList<ActionMenuItemInfo> actions)
        {
            if (selectedResourceIds == null)
            {
                throw new ArgumentNullException("selectedResourceIds");
            }
            if (actions == null || actions.Contains(null))
            {
                throw new ArgumentNullException("actions");
            }

            bool hasParentEntity = parentEntityId > 0;
            bool remove;

            var checkedPermissions = new Dictionary<long, bool>();
            var checkedForRead = new Dictionary<long, bool>();

            // Create a copy of the collection so entities can be removed if needed
            foreach (ActionMenuItemInfo menuItem in actions.ToArray())
            {
                remove = false;

                if (hasParentEntity && ActionRequiresParentModifyAccess(menuItem))
                {
                    remove = !Service.Check(new EntityRef(parentEntityId), new[] { Permissions.Modify });
                }

                if (!remove)
                { 
                    if (IsEntityMenuItem(menuItem))
                    {
                        // Firstly, there must be "view" access to any related entity
                        if (menuItem.EntityId > 0)
                        {
                            if (checkedForRead.ContainsKey(menuItem.EntityId))
                            {
                                remove = !checkedForRead[menuItem.EntityId];
                            }
                            else
                            {
                                var canRead = Service.Check(new EntityRef(menuItem.EntityId), new[] { Permissions.Read });
                                checkedForRead.Add(menuItem.EntityId, canRead);
                                remove = !canRead;
                            }
                        }

                        if (!remove)
                        {
                            remove = !KeepEntityMenuItem(menuItem, selectedResourceIds, checkedPermissions);
                        }
                    }
                    else if (menuItem.IsNew)
                    {
                        remove = !KeepCreateMenuItem(menuItem);
                    }
                }
                // Insert future menu item security checks here

                if (remove)
                {
                    actions.Remove(menuItem);
                }
                else if (menuItem.Children != null)
                {
                    Filter(parentEntityId, selectedResourceIds, menuItem.Children);

					if ( IsNewHolderMenuItem( menuItem ) && menuItem.Children.Count <= 0 )
                    {
                        actions.Remove(menuItem);
                    }
                }                
            }
        }

        /// <summary>
        /// Checks if a given action requires modify access for parent entity
        /// </summary>
        /// <param name="menuItem"></param>
        /// <returns></returns>
        private bool ActionRequiresParentModifyAccess(ActionMenuItemInfo menuItem)
        {
            return menuItem.Alias == "console:addRelationshipAction"
                   || menuItem.Alias == "console:removeRelationshipAction"
                   || menuItem.Alias == "console:deleteResourceAction"
                   || menuItem.IsNew;
        }

        /// <summary>
        /// Is the given <paramref name="menuItem"/> created
        /// from an <see cref="ActionMenuItem"/> entity?
        /// </summary>
        /// <param name="menuItem">
        /// The menu item to check.
        /// </param>
        /// <returns>
        /// True if the menu item was created from an entity, false otherwise.
        /// </returns>
        private bool IsEntityMenuItem(ActionMenuItemInfo menuItem)
        {
            return menuItem.Id > 0;
        }

        /// <summary>
        /// Determines whether the menu item is the new holder menu item.
        /// </summary>
        /// <param name="menuItem">The menu item.</param>
        /// <returns></returns>
        private bool IsNewHolderMenuItem(ActionMenuItemInfo menuItem)
        {
            return menuItem.HtmlActionState == ActionService.NewHolderMenuItemActionState;
        }

        /// <summary>
        /// Does the given <paramref name="menuItem"/> have a 
        /// target entity?
        /// </summary>
        /// <param name="menuItem">
        /// The menu item to check.
        /// </param>
        /// <returns>
        /// True if it has a target entity, false if not.
        /// </returns>
        private bool HasTargetEntity(ActionMenuItemInfo menuItem)
        {
            return menuItem.EntityId > 0;
        }

        /// <summary>
        /// Should an entity menu item be kept or removed?
        /// </summary>
        /// <remarks>
        /// Entity menu items are ones created from an <see cref="ActionMenuItem"/>
        /// entity. Users require read access to the target entity and any 
        /// additional permissions referenced by the 
        /// <see cref="ActionMenuItem.ActionRequiresPermission" /> relationship.
        /// </remarks>
        /// <param name="menuItem">
        /// The menu item to check.
        /// </param>
        /// <param name="selectedResourceIds">
        /// The IDs of the resources to check. Ths cannot be null.
        /// </param>
        /// <param name="checkedPermissions">
        /// Set of permissions that have already been checked.
        /// </param>
        /// <returns>
        /// True if it should be kept, false if it should be removed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="menuItem"/> should be an entity menu item
        /// and must have a target entity.
        /// </exception>
	    private bool KeepEntityMenuItem(ActionMenuItemInfo menuItem, IList<long> selectedResourceIds, IDictionary<long, bool> checkedPermissions )
        {
            if (menuItem == null)
            {
                throw new ArgumentNullException("menuItem");
            }
            if (selectedResourceIds == null)
            {
                throw new ArgumentNullException("selectedResourceIds");
            }
            if (!IsEntityMenuItem(menuItem))
            {
                throw new ArgumentException(@"Not an entity menu item", "menuItem");
            }

            IList<EntityRef> requiredPermissions = new List<EntityRef>();

		    bool result = true;

            //
            // There are some cases where we allow for very specific security on custom system actions (namely 'New Workflow')
            //
	        if (menuItem.IsSystem && !string.IsNullOrEmpty(menuItem.HtmlActionTarget))
	        {
	            return KeepTargetedEntityMenuItem(menuItem);
	        }

            using (new SecurityBypassContext())
            {
	            foreach ( var ct in Entity.GetRelationships( new EntityRef( menuItem.Id ), new EntityRef( "console:actionRequiresPermission" ), Direction.Forward ) )
	            {
		            if ( !checkedPermissions.ContainsKey( ct.Key ) )
		            {
			            requiredPermissions.Add( new EntityRef( ct.Key ) );
		            }
		            else
		            {
			            result &= checkedPermissions[ ct.Key ];
		            }
	            }

	            if ( !checkedPermissions.ContainsKey( Permissions.Read.Id ) )
	            {
		            requiredPermissions.Add( Permissions.Read );
	            }
				else
				{
					result &= checkedPermissions [ Permissions.Read.Id ];
				}

                // Filter actionRequiresRole
                foreach ( var ct in Entity.GetRelationships( new EntityRef( menuItem.Id ), new EntityRef( "actionRequiresRole" ), Direction.Forward ) )
                {
                    long roleId = ct.Key;
                    var userRoles = UserRoleRepository.GetUserRoles( RequestContext.UserId );
                    if ( !userRoles.Contains( roleId ) )
                    {
                        return false;
                    }
                }
            }

		    if ( requiredPermissions.Count > 0 && result )
		    {
			    IDictionary<long, bool> checkResults = Service.Check( selectedResourceIds.Select( id => new EntityRef( id ) ).ToList( ), requiredPermissions.ToList( ) );

			    foreach ( KeyValuePair<long, bool> pair in checkResults )
			    {
				    if ( pair.Value )
				    {
					    foreach ( var requiredPermission in requiredPermissions )
					    {
						    checkedPermissions[ requiredPermission.Id ] = true;
					    }
				    }
				    else
				    {
						/////
						// Check failed. If multiple permissions were requested don't update the checkedPermissions cache.
						/////
					    if ( requiredPermissions.Count == 1 )
					    {
						    checkedPermissions[ requiredPermissions[ 0 ].Id ] = false;
					    }

					    result = false;
				    }
			    }
		    }

		    return result;
        }

        /// <summary>
        /// Should this entity be removed, assuming it the action is a system based targeted action with
        /// some custom required permissions.
        /// </summary>
        /// <param name="menuItem">The menu item to check.</param>
        /// <returns>True if it should be kept, false if it should be removed.</returns>
        private bool KeepTargetedEntityMenuItem(ActionMenuItemInfo menuItem)
        {
            if (!menuItem.IsSystem)
            {
                // this is a system feature. just for us.
                return false;
            }

            if (string.IsNullOrEmpty(menuItem.HtmlActionTarget))
            {
                // this action hasn't been targeted
                return false;
            }

            IList<EntityRef> requiredPermissions = new List<EntityRef>();
            var result = true;

            //
            // The target will alter what the permissions should be checked against
            //
            if (menuItem.RequiresPermissions.Count > 0)
            {
                requiredPermissions.AddRange(menuItem.RequiresPermissions.Select(p => new EntityRef(p.Id)));
                if (requiredPermissions.Any(p => p.Id == Permissions.Create.Id))
                {
                    var entityType = Entity.Get<EntityType>(menuItem.EntityId);

                    // Create permission on a non-EntityType = access denied
                    result = entityType != null && Service.CanCreate(entityType);

                    // Remove in prep for extra checks below
                    requiredPermissions.Remove(Permissions.Create);
                }

                if (result)
                {
                    // Bare minimum access required is "View"
                    if (requiredPermissions.All(p => p.Id != Permissions.Read.Id))
                    {
                        requiredPermissions.Add(Permissions.Read);
                    }

                    result = Service.Check(menuItem.EntityId, requiredPermissions);
                }

                // Add to cache? I don't think this applies if it's not against the selected items?
            }

            return result;
        }

        /// <summary>
        /// Should a create menu item be kept or removed?
        /// </summary>
        /// <remarks>
        /// Create menu items should be shown when the user has 
        /// <see cref="Permissions.Create"/> on the target
        /// entity (which must be a type).
        /// </remarks>
        /// <param name="menuItem">
        /// The menu item to check.
        /// </param>
        /// <returns>
        /// True if it should be kept, false if it should be removed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="menuItem"/> should be a create menu item
        /// and must have a target entity.
        /// </exception>
        private bool KeepCreateMenuItem(ActionMenuItemInfo menuItem)
        {
            if (menuItem == null)
            {
                throw new ArgumentNullException("menuItem");
            }
            if (!menuItem.IsNew)
            {
                throw new ArgumentException("Not a create menu item", "menuItem");
            }
            if (!HasTargetEntity(menuItem))
            {
                throw new ArgumentException("Lacks a target entity Id");
            }

            EntityType entityType;

            entityType = Entity.Get<EntityType>(menuItem.EntityId);
            if (entityType == null)
            {
                throw new ArgumentException("Not an entity type", "menuItem");
            }

            return Service.CanCreate(entityType);
        }
    }
}

