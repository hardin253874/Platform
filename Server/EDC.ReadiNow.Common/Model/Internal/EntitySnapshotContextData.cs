// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.Collections.Generic;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.Internal
{
	/// <summary>
	///     Snapshot data that is stored in the context.
	///     Note: This class is not thread safe as it is designed to be
	///     stored is thread local context.
	/// </summary>
	internal class EntitySnapshotContextData
	{
		#region Fields

		/// <summary>
		///     The entity cache.
		/// </summary>
		private readonly Dictionary<long, IEntity> _entityCache = new Dictionary<long, IEntity>( );


		/// <summary>
		///     The entity field cache.
		/// </summary>
		private readonly Dictionary<string, object> _entityFieldCache = new Dictionary<string, object>( );


		/// <summary>
		///     The entity relationship cache.
		/// </summary>
		private readonly Dictionary<string, IChangeTracker<IMutableIdKey>> _entityRelationshipCache = new Dictionary<string, IChangeTracker<IMutableIdKey>>( );


		/// <summary>
		///     The tenant id.
		/// </summary>
		private readonly long _tenantId;


		/// <summary>
		///     The user id.
		/// </summary>
		private readonly long _userId;

		#endregion Fields

		#region Constructor

		/// <summary>
		///     Initializes a new instance of the <see cref="EntitySnapshotContextData" /> class.
		/// </summary>
		internal EntitySnapshotContextData( )
		{
			RequestContext requestcontext = RequestContext.GetContext( );
			_tenantId = requestcontext.Tenant.Id;
			_userId = requestcontext.Identity.Id;
		}

		#endregion

		#region Public Properties

		/// <summary>
		///     Gets a value indicating whether this instance is valid.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is valid; otherwise, <c>false</c>.
		/// </value>
		internal bool IsValid
		{
			get
			{
				RequestContext requestcontext = RequestContext.GetContext( );
				return ( requestcontext.Tenant.Id == _tenantId &&
				         requestcontext.Identity.Id == _userId );
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		///     Sets the entity.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="entity">The entity.</param>
		internal void SetEntity( long id, IEntity entity )
		{
			if ( IsValid )
			{
				_entityCache[ id ] = entity;
			}
		}


		/// <summary>
		///     Sets the entity field.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="fieldId">The field id.</param>
		/// <param name="value">The value.</param>
		internal void SetEntityField( long entityId, long fieldId, object value )
		{
			if ( IsValid )
			{
				string key = string.Format( "{0}.{1}", entityId, fieldId );
				_entityFieldCache[ key ] = value;
			}
		}


		/// <summary>
		///     Sets the entity relationships.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="relationshipId">The relationship id.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="value">The value.</param>
		internal void SetEntityRelationships( long entityId, long relationshipId, Direction direction, IChangeTracker<IMutableIdKey> value )
		{
			if ( IsValid )
			{
				string key = string.Format( "{0}.{1}.{2}", entityId, relationshipId, direction );
				_entityRelationshipCache[ key ] = value;
			}
		}

		/// <summary>
		///     Tries to get the cached get entity.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		internal bool TryGetEntity( long id, out IEntity entity )
		{
			bool result = false;
			entity = null;

			if ( IsValid )
			{
				result = _entityCache.TryGetValue( id, out entity );
			}

			return result;
		}

		/// <summary>
		///     Tries to get the cache entity field value.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="fieldId">The field id.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		internal bool TryGetEntityField( long entityId, long fieldId, out object value )
		{
			bool result = false;
			value = null;

			if ( IsValid )
			{
				string key = string.Format( "{0}.{1}", entityId, fieldId );
				result = _entityFieldCache.TryGetValue( key, out value );
			}

			return result;
		}


		/// <summary>
		///     Tries to get the cached entity relationships.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="relationshipId">The relationship id.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		internal bool TryGetEntityRelationships( long entityId, long relationshipId, Direction direction, out IChangeTracker<IMutableIdKey> value )
		{
			bool result = false;
			value = null;

			if ( IsValid )
			{
				string key = string.Format( "{0}.{1}.{2}", entityId, relationshipId, direction );
				result = _entityRelationshipCache.TryGetValue( key, out value );
			}

			return result;
		}

		#endregion Public Methods
	}
}