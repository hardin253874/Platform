// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Implementation of IEntityRepository that uses the static Entity model APIs.
    /// </summary>
    /// <remarks>
    /// Other ID types supported via EntityRepositoryExtensions.
    /// </remarks>
    class EntityRepository : IEntityRepository
    {
        /// <summary>
        /// Creates an entity of the specified strong type.
        /// </summary>
        /// <param name="type">The type of entity to create.</param>
        public IEntity Create( long type )
        {
            return Entity.Create( type );
        }

        /// <summary>
        /// Creates an entity of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        public T Create<T>( ) where T : class, IEntity
        {
            return Entity.Create<T>( );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="id">The ID of the entity to fetch.</param>
        public IEntity Get( long id )
        {
            return Entity.Get( id );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="id">The ID of the entity to fetch.</param>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        public T Get<T>( long id ) where T : class, IEntity
        {
            return Entity.Get<T>( id );
        }
        
        /// <summary>
        /// Creates an entity of the specified strong type.
        /// </summary>
        /// <param name="typeAlias">The type of entity to create.</param>
        public IEntity Create( string typeAlias )
        {
            return Entity.Create( typeAlias );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="alias">The ID of the entity to fetch.</param>
        public IEntity Get( string alias )
        {
            return Entity.Get( alias );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="alias">The ID of the entity to fetch.</param>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        public T Get<T>( string alias ) where T : class, IEntity
        {
            return Entity.Get<T>( alias );
        }

        /// <summary>
        /// Creates an entity of the specified strong type.
        /// </summary>
        /// <param name="type">The type of entity to create.</param>
        public IEntity Create( IEntityRef type )
        {
            return Entity.Create( type.Id );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="id">The ID of the entity to fetch.</param>
        public IEntity Get( IEntityRef id )
        {
            return Entity.Get( id.Id );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="id">The ID of the entity to fetch.</param>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        public T Get<T>( IEntityRef id ) where T : class, IEntity
        {
            return Entity.Get<T>( id.Id );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="id">The ID of the entity to fetch.</param>
        /// <param name="preloadQuery">The network of relationships to preload.</param>
        public IEntity Get( IEntityRef id, string preloadQuery )
        {
            BulkPreloader.Preload( new EntityRequests.EntityRequest( new EntityRef( id ), preloadQuery, "EntityRepository" ) );
            return Entity.Get( id.Id );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="id">The ID of the entity to fetch.</param>
        /// <param name="preloadQuery">The network of relationships to preload.</param>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        public T Get<T>( IEntityRef id, string preloadQuery ) where T : class, IEntity
        {
            BulkPreloader.Preload( new EntityRequests.EntityRequest( new EntityRef( id ), preloadQuery, "EntityRepository" ) );
            return Entity.Get<T>( id.Id );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="id">The ID of the entity to fetch.</param>
        /// <param name="preloadQuery">The network of relationships to preload.</param>
        public IEntity Get( long id, string preloadQuery )
        {
            BulkPreloader.Preload( new EntityRequests.EntityRequest( new EntityRef( id ), preloadQuery, "EntityRepository" ) );
            return Entity.Get( id );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="id">The ID of the entity to fetch.</param>
        /// <param name="preloadQuery">The network of relationships to preload.</param>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        public T Get<T>( long id, string preloadQuery ) where T : class, IEntity
        {
            BulkPreloader.Preload( new EntityRequests.EntityRequest( new EntityRef( id ), preloadQuery, "EntityRepository" ) );
            return Entity.Get<T>( id );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="alias">The ID of the entity to fetch.</param>
        /// <param name="preloadQuery">The network of relationships to preload.</param>
        public IEntity Get( string alias, string preloadQuery )
        {
            BulkPreloader.Preload( new EntityRequests.EntityRequest( new EntityRef( alias ), preloadQuery, "EntityRepository" ) );
            return Entity.Get( alias );
        }

        /// <summary>
        /// Gets the specified entity from the repository.
        /// </summary>
        /// <param name="alias">The ID of the entity to fetch.</param>
        /// <param name="preloadQuery">The network of relationships to preload.</param>
        /// <typeparam name="T">The type of entity to create.</typeparam>
        public T Get<T>( string alias, string preloadQuery ) where T : class, IEntity
        {
            BulkPreloader.Preload( new EntityRequests.EntityRequest( new EntityRef( alias ), preloadQuery, "EntityRepository" ) );
            return Entity.Get<T>( alias );
        }

        /// <summary>
        /// Gets the specified entities from the repository.
        /// </summary>
        /// <param name="ids">The IDs of the entities to fetch.</param>
        public IReadOnlyCollection<IEntity> Get( IEnumerable<long> ids )
        {
            return AsReadOnlyCollection( Entity.Get( ids ) );
        }

        /// <summary>
        /// Gets the specified entities from the repository.
        /// </summary>
        /// <typeparam name="T">The type of entity to load.</typeparam>
        /// <param name="ids">The IDs of the entities to fetch.</param>
        public IReadOnlyCollection<T> Get<T>( IEnumerable<long> ids ) where T : class, IEntity
        {
            return AsReadOnlyCollection( Entity.Get<T>( ids ) );
        }

        /// <summary>
        /// Gets the specified entities from the repository.
        /// </summary>
        /// <param name="ids">The IDs of the entities to fetch.</param>
        /// <param name="preloadQuery">The network of relationships to preload.</param>
        public IReadOnlyCollection<IEntity> Get( IEnumerable<long> ids, string preloadQuery )
        {
            BulkPreloader.Preload( new EntityRequests.EntityRequest( ids.Select( id => new EntityRef( id ) ), preloadQuery, "EntityRepository" ) );

            return AsReadOnlyCollection( Entity.Get( ids ) );
        }

        /// <summary>
        /// Gets the specified entities from the repository.
        /// </summary>
        /// <typeparam name="T">The type of entity to load.</typeparam>
        /// <param name="ids">The IDs of the entities to fetch.</param>
        /// <param name="preloadQuery">The network of relationships to preload.</param>
        public IReadOnlyCollection<T> Get<T>( IEnumerable<long> ids, string preloadQuery ) where T : class, IEntity
        {
            BulkPreloader.Preload( new EntityRequests.EntityRequest( ids.Select( id => new EntityRef( id ) ), preloadQuery, "EntityRepository" ) );

            return AsReadOnlyCollection( Entity.Get<T>( ids ) );
        }

        private IReadOnlyCollection<T> AsReadOnlyCollection<T>( IEnumerable<T> list )
        {
            if ( list == null )
                return null;
            IReadOnlyCollection<T> cast = list as IReadOnlyCollection<T>;
            if ( cast == null )
                return list.ToList( );
            else
                return cast;            
        }
    }
}
