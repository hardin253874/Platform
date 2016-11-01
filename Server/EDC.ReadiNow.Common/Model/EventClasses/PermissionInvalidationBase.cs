// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses
{
	/// <summary>
	///     Base class for permission invalidation event classes.
	/// </summary>
	public abstract class PermissionInvalidationBase
	{
		#region Private Members

		/// <summary>
		///     Invalid Permissions key.
		/// </summary>
		protected static readonly string InvalidPermissionsKey = "InvalidPermissions";

		/// <summary>
		///     Modify permission.
		/// </summary>
		private volatile Permission _modifyPermission;

		/// <summary>
		///     Read permission.
		/// </summary>
		private volatile Permission _readPermission;

		#endregion Private Members

		#region Private Properties

		/// <summary>
		///     Gets the modify permission.
		/// </summary>
		/// <value>
		///     The modify permission.
		/// </value>
		protected Permission ModifyPermission
		{
			get
			{
				if ( _modifyPermission == null )
				{
					lock ( SyncRoot )
					{
						if ( _modifyPermission == null )
						{
							return _modifyPermission = Entity.Get<Permission>( new EntityRef( "core", "modify" ) );
						}
					}
				}

				return _modifyPermission;
			}
		}

		/// <summary>
		///     Gets the read permission.
		/// </summary>
		/// <value>
		///     The read permission.
		/// </value>
		protected Permission ReadPermission
		{
			get
			{
				if ( _readPermission == null )
				{
					lock ( SyncRoot )
					{
						if ( _readPermission == null )
						{
							return _readPermission = Entity.Get<Permission>( new EntityRef( "core", "read" ) );
						}
					}
				}

				return _readPermission;
			}
		}

		#endregion Private Properties

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		protected readonly object SyncRoot = new object( );

		/// <summary>
		///     Locates the invalid objects.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="foundAction">The found action.</param>
		protected void LocateInvalidObjects( IEnumerable<IEntity> entities, EntityRef relationship, Direction direction, Action<ISet<long>> foundAction )
		{
			/////
			// ReSharper disable EmptyGeneralCatchClause
			/////

			/////
			// Dictionary of modifications made to the current entity.
			/////
			try
			{
				ISet<long> invalidObjects = new HashSet<long>( );

				/////
				// Loop through the entities and obtain their modifications prior to save.
				/////
				foreach ( IEntity entity in entities )
				{
					var entityInternal = entity as IEntityInternal;

					if ( entityInternal != null )
					{
						IDictionary<long, IChangeTracker<IMutableIdKey>> relationships;

						if ( EntityRelationshipModificationCache.Instance.TryGetValue( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( entityInternal.ModificationToken, direction ), out relationships ) )
						{
							GetRelationship( relationships, relationship, invalidObjects );
						}
					}
				}

				/////
				// Invalid objects.
				/////
				if ( invalidObjects.Count > 0 && foundAction != null )
				{
					foundAction( invalidObjects );
				}
			}

			catch

			{
				/////
				// Ignore any errors.
				/////
			}

			/////
			// ReSharper restore EmptyGeneralCatchClause
			/////
		}

		/// <summary>
		///     Resolves the invalid objects.
		/// </summary>
		/// <param name="invalidObjects">The invalid objects.</param>
		/// <param name="invalidateAction">The invalidate action.</param>
		protected void ResolveInvalidObjects( ISet<long> invalidObjects, Action<long> invalidateAction )
		{
			if ( invalidObjects == null || invalidateAction == null )
			{
				return;
			}

			foreach ( long invalidObject in invalidObjects )
			{
				/////
				// Run the invalidate action.
				/////
				invalidateAction( invalidObject );
			}
		}

		/// <summary>
		///     Gets the relationship.
		/// </summary>
		/// <param name="modifications">The modifications.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="invalidObjects">The invalid objects.</param>
		private void GetRelationship( IDictionary<long, IChangeTracker<IMutableIdKey>> modifications, EntityRef relationship, ISet<long> invalidObjects )
		{
			/////
			// Argument check.
			/////
			if ( modifications == null || relationship == null )
			{
				return;
			}

			IChangeTracker<IMutableIdKey> relationshipValues;

			if ( modifications.TryGetValue( relationship.Id, out relationshipValues ) )
			{
				ParseRelationship( relationshipValues, invalidObjects );
			}
		}

		/// <summary>
		///     Locates the invalid permission relationships.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="permission">The permission.</param>
		/// <param name="permissionRelationships">The permission relationships.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="invalidPermissions">The invalid permissions.</param>
		private void LocateInvalidPermissionRelationships( IEnumerable<IEntity> entities, Permission permission, IEntityRelationshipCollection<IEntity> permissionRelationships, Direction direction, ISet<long> invalidPermissions )
		{
			if ( permissionRelationships != null && permissionRelationships.Count > 0 )
			{
				IList<IEntity> enumerable = entities as IList<IEntity> ?? entities.ToList( );

				foreach ( var relationship in permissionRelationships.Where( entityRelationship => entityRelationship != null && entityRelationship.Entity != null ) )
				{
					LocateInvalidObjects( enumerable, relationship.Entity.Id, direction, invalidObjects =>
						{
							/////
							// If there are any invalid objects, mark the permission as invalid.
							/////
							if ( invalidObjects != null && invalidObjects.Count > 0 )
							{
								invalidPermissions.Add( permission.Id );
							}
						} );
				}
			}
		}

		/// <summary>
		///     Parses the relationship.
		/// </summary>
		/// <param name="relationship">The relationship.</param>
		/// <param name="invalidObjects">The invalid objects.</param>
		private void ParseRelationship( IChangeTracker<IMutableIdKey> relationship, ISet<long> invalidObjects )
		{
			/////
			// Argument check.
			/////
			if ( relationship == null )
			{
				return;
			}

			if ( invalidObjects == null )
			{
				/////
				// Reinitialize the set.
				/////
				invalidObjects = new HashSet<long>( );
			}

			if ( relationship.Flushed )
			{
				/////
				// All entities are invalid.
				/////
				foreach ( var modification in relationship.Where( pair => pair != null ) )
				{
					invalidObjects.Add( modification.Key );
				}

				return;
			}

			/////
			// Process the added relationships.
			/////
			if ( relationship.Added != null && relationship.Added.Any( ) )
			{
				/////
				// All added entities are invalid.
				/////
				foreach ( var modification in relationship.Added.Where( pair => pair != null ) )
				{
					invalidObjects.Add( modification.Key );
				}
			}

			/////
			// Process the removed relationships.
			/////
			if ( relationship.Removed != null && relationship.Removed.Any( ) )
			{
				/////
				// All removed entities are invalid.
				/////
				foreach ( var modification in relationship.Removed.Where( pair => pair != null ) )
				{
					invalidObjects.Add( modification.Key );
				}
			}
		}
	}
}