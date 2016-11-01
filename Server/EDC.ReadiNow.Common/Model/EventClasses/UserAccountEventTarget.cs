// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AuditLog;
using EDC.ReadiNow.Security.AuditLog.EventTargets;
using EDC.Security;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses
{
	/// <summary>
	///     This class is responsible for handling user account events.
	/// </summary>
	public class UserAccountEventTarget : PermissionInvalidationBase, IEntityEventSave, IEntityEventDelete, IEntityEventError
	{
        /// <summary>
        /// The audit log event target
        /// </summary>
        private readonly AuditLogUserAccountEventTarget _auditLogEventTarget = new AuditLogUserAccountEventTarget(AuditLogInstance.Get());

		/// <summary>
		///     Invalid Roles key.
		/// </summary>
		private const string InvalidRolesKey = "InvalidRoles";        

		/// <summary>
		///     Called after deletion of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state.</param>
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
		/// <param name="state">The state.</param>
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
		/// <param name="state">The state.</param>
		/// <returns>
		///     True to cancel the delete operation; false otherwise.
		/// </returns>
		public bool OnBeforeDelete( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
            foreach (var userAccount in entities.Select(entity => entity.As<UserAccount>()).Where(userAccount => userAccount != null))
            {
                // Gather user account details required OnAfterDelete
                _auditLogEventTarget.GatherAuditLogEntityDetailsForDelete(userAccount, state);
            }            

		    return false;
		}

		/// <summary>
		///     Called before saving the enumeration of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state.</param>
		/// <returns>
		///     True to cancel the save operation; false otherwise.
		/// </returns>
		public bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
            long passwordFieldId = Entity.GetId("core:password");

			IList<IEntity> enumerable = entities as IList<IEntity> ?? entities.ToList( );

			foreach ( IEntity entity in enumerable )
			{
				var userAccount = entity.As<UserAccount>( );
				if ( userAccount == null )
				{
					continue;
				}

				EntityFieldCache.Instance.Get( 0 );

                _auditLogEventTarget.GatherAuditLogEntityDetailsForSave(userAccount, state);

				var writableCacheKey = new EntityFieldModificationCache.EntityFieldModificationCacheKey( ( ( IEntityInternal ) entity.Entity ).ModificationToken );

                IEntityFieldValues cachedFieldValues;
                
                if ( EntityFieldModificationCache.Instance.TryGetValue( writableCacheKey, out cachedFieldValues ) )
				{
				    object newPassword;

                    if (cachedFieldValues.TryGetValue(passwordFieldId, out newPassword))
                    {                        
                        string password = newPassword as string;

                        var userAccountInternal = userAccount as IEntityInternal;
                        var savedUserAccount = Entity.Get<UserAccount>(userAccount.Id);

                        if (!userAccountInternal.IsTemporaryId &&
                            password == savedUserAccount.Password)
		                {
                            // Password is unchanged
                            continue;   
                        }

                        // Validate the password against the password policy
                        PasswordPolicyHelper.ValidatePassword(PasswordPolicyHelper.GetDefaultPasswordPolicy(), password);
                        
                        // Hash the password before saving
                        userAccount.Password = CryptoHelper.CreateEncodedSaltedHash(password);
						// The password field was modified, so set the last password change date.
						userAccount.PasswordLastChanged = DateTime.UtcNow;
                    }
				}

                if (HasUserAccountStatusChanged(userAccount))
                {                    
                    if (userAccount.AccountStatus_Enum == UserAccountStatusEnum_Enumeration.Active &&
                        userAccount.BadLogonCount > 0)
                    {
                        // Reset the bad logon account when the account is made active again.
                        userAccount.BadLogonCount = 0;
                    }
                }                                
			}

			return false;
		}

        /// <summary>
        /// Returns true if the user account status has changed, false otherwise.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <returns></returns>
        private bool HasUserAccountStatusChanged(UserAccount userAccount)
        {
            bool userAccountStatusChanged = false;
            IDictionary<long, IChangeTracker<IMutableIdKey>> relationshipValues;

            EntityRelationshipModificationCache.Instance.TryGetValue(new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey((userAccount as IEntityInternal).ModificationToken, Direction.Forward), out relationshipValues);

            if (relationshipValues != null)
            {
                userAccountStatusChanged = relationshipValues.ContainsKey(UserAccount.AccountStatus_Field.Id);
            }

            return userAccountStatusChanged;
        }


        #region IEntityEventError Members

        /// <summary>
        /// Called if a failure occurs saving the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
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

        #endregion 
    }
}