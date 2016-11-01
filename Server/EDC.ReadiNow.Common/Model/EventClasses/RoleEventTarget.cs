// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Security.AuditLog;
using EDC.ReadiNow.Security.AuditLog.EventTargets;

namespace EDC.ReadiNow.Model.EventClasses
{
	/// <summary>
	///     Role events.
	/// </summary>
    public class RoleEventTarget : PermissionInvalidationBase, IEntityEventSave, IEntityEventDelete, IEntityEventError
	{
        /// <summary>
        /// The audit log event target
        /// </summary>
        private readonly AuditLogRoleEventTarget _auditLogEventTarget = new AuditLogRoleEventTarget(AuditLogInstance.Get());

		/// <summary>
		///     Invalid Roles key.
		/// </summary>
		private const string InvalidRolesKey = "InvalidRoles";

		/// <summary>
		///     Invalid Users key.
		/// </summary>
		private const string InvalidUsersKey = "InvalidUsers";        

		/// <summary>
		///     Called after deletion of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before delete and after delete callbacks.</param>
		public void OnAfterDelete( IEnumerable<long> entities, IDictionary<string, object> state )
		{
            foreach (long entityId in entities)
            {
                _auditLogEventTarget.WriteDeleteAuditLogEntries(true, entityId, state);
            }
		}

		/// <summary>
		///     Called after saving of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			if ( entities == null )
			{
				return;
			}

			object invalidObjects;

            foreach (long entityId in entities.Select(e => e.Id))
            {
                _auditLogEventTarget.WriteSaveAuditLogEntries(true, entityId, state);
            }

			/////
			// Invalidate the changed users in that role.
			/////
			if ( state.TryGetValue( InvalidUsersKey, out invalidObjects ) )
			{
				if ( invalidObjects != null )
				{
                    // ResolveInvalidObjects(invalidObjects as ISet<long>, invalidUser => EntityAccessControlCacheInvalidator.InvalidateUser(new EntityRef(invalidUser)));
				}

				state.Remove( InvalidUsersKey );
			}

			/////
			// Resolve the invalid roles.
			/////
			if ( state.TryGetValue( InvalidRolesKey, out invalidObjects ) )
			{
				if ( invalidObjects != null )
				{
                    // ResolveInvalidObjects(invalidObjects as ISet<long>, invalidRole => EntityAccessControlCacheInvalidator.InvalidateRole(new EntityRef(invalidRole)));
				}

				state.Remove( InvalidRolesKey );
			}

			/////
			// Invalidate the changed permission grants for that role.
			/////
			if ( state.TryGetValue( InvalidPermissionsKey, out invalidObjects ) )
			{
				if ( invalidObjects != null )
				{
                    // ResolveInvalidObjects(invalidObjects as ISet<long>, invalidPermission => EntityAccessControlCacheInvalidator.InvalidatePermission(new EntityRef(invalidPermission)));
				}

				state.Remove( InvalidPermissionsKey );
			}
		}

		/// <summary>
		///     Called before deleting an enumeration of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before delete and after delete callbacks.</param>
		/// <returns>
		///     True to cancel the delete operation; false otherwise.
		/// </returns>
		public bool OnBeforeDelete( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
            foreach (var role in entities.Select(entity => entity.As<Role>()).Where(role => role != null))
            {
                // Gather role details required OnAfterDelete
                _auditLogEventTarget.GatherAuditLogEntityDetailsForDelete(role, state);
            }  

            return false;
		}

		/// <summary>
		///     Called before saving the enumeration of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		/// <returns>
		///     True to cancel the save operation; false otherwise.
		/// </returns>
		public bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
            ISet<long> invalidRoles = new HashSet<long>();

            IList<IEntity> enumerable = entities as IList<IEntity> ?? entities.ToList();

		    foreach (IEntity entity in enumerable)
		    {
                var role = entity.As<Role>();
                if (role == null)
                {
                    continue;
                }

                _auditLogEventTarget.GatherAuditLogEntityDetailsForSave(role, state);
		    }

            LocateInvalidObjects( enumerable, new EntityRef( WellKnownAliases.CurrentTenant.RoleMembers ), Direction.Reverse, invalidObjects => state [ InvalidUsersKey ] = invalidObjects );
            LocateInvalidObjects( enumerable, new EntityRef( WellKnownAliases.CurrentTenant.IncludesRoles ), Direction.Forward, invalidRoles.UnionWith );
            LocateInvalidObjects( enumerable, new EntityRef( WellKnownAliases.CurrentTenant.IncludedByRoles ), Direction.Reverse, invalidRoles.UnionWith );

			if ( invalidRoles.Count > 0 )
            {
                state[InvalidRolesKey] = invalidRoles;
            }

			return false;
		}

        /// <summary>
        /// Called if a failure occurs saving the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnSaveFailed(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                return;
            }

            foreach (long entity in entities.Select(e => e.Id))
            {
                _auditLogEventTarget.WriteSaveAuditLogEntries(false, entity, state);
            }
        }


        /// <summary>
        /// Called if a failure occurs deleting the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnDeleteFailed(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                return;
            }

            foreach (long entity in entities)
            {
                _auditLogEventTarget.WriteDeleteAuditLogEntries(false, entity, state);
            } 
        }       
	}
}