// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Mockable interface for performing operations on the entity repository.
    /// </summary>
    public interface IEntityRepository : IEntityRepository<long>
    {
        #region Alias

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IEntity Get( string id );

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    T Get<T>( string id ) where T : class, IEntity;

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <param name="preloadQuery">The network of relationships to preload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IEntity Get( string id, string preloadQuery );

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <param name="preloadQuery">The network of relationships to preload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    T Get<T>( string id, string preloadQuery ) where T : class, IEntity;
        #endregion

        #region IEntityRef

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IEntity Get( IEntityRef id );

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    T Get<T>( IEntityRef id ) where T : class, IEntity;

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <param name="preloadQuery">The network of relationships to preload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IEntity Get( IEntityRef id, string preloadQuery );

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <param name="preloadQuery">The network of relationships to preload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    T Get<T>( IEntityRef id, string preloadQuery ) where T : class, IEntity;
        #endregion
    }

    /// <summary>
    /// Mockable interface for performing operations on the entity repository.
    /// </summary>
    public interface IEntityRepository<TRef>
    {
        /// <summary>
        /// Creates an entity of the specified strong type.
        /// </summary>
        /// <param name="type">The type of entity to create.</param>
        IEntity Create( TRef type );

        /// <summary>
        /// Creates an entity of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        T Create<T>( ) where T : class, IEntity;

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IEntity Get( TRef id );

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    T Get<T>( TRef id ) where T : class, IEntity;

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <param name="preloadQuery">The network of relationships to preload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IEntity Get( TRef id, string preloadQuery );

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <param name="id">The ID of the entity to fetch.</param>
		/// <param name="preloadQuery">The network of relationships to preload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    T Get<T>( TRef id, string preloadQuery ) where T : class, IEntity;

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <param name="ids">The IDs of the entity to fetch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IReadOnlyCollection<IEntity> Get( IEnumerable<TRef> ids );

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <param name="ids">The IDs of the entity to fetch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IReadOnlyCollection<T> Get<T>( IEnumerable<TRef> ids ) where T : class, IEntity;

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <param name="ids">The ids.</param>
		/// <param name="preloadQuery">The network of relationships to preload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IReadOnlyCollection<IEntity> Get( IEnumerable<TRef> ids, string preloadQuery );

		/// <summary>
		/// Gets the specified entity from the repository.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <param name="ids">The ids.</param>
		/// <param name="preloadQuery">The network of relationships to preload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Access to single instances will cause a <see><cref>PlatformSecurityException</cref></see>
		/// if the current context does not have permission.
		/// </remarks>
	    IReadOnlyCollection<T> Get<T>( IEnumerable<TRef> ids, string preloadQuery ) where T : class, IEntity;
    }

}
