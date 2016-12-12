// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Security;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// An entity repository that uses the entity info service.
    /// </summary>
    /// <remarks>
    /// All entities must be preloaded in the preload query.
    /// Types should be preloaded to prevent the security service from loading types.
    /// </remarks>
    public class GraphEntityRepository : IEntityRepository<long>
    {
        private static readonly IReadOnlyCollection<IEntity> EmptyEntityList = new IEntity [ 0 ];

        /// <summary>
        /// The security service.
        /// </summary>
        public IEntityAccessControlService EntityAccessControlService { get; set; }

        #region Non-suported operations
        public IEntity Create( long type )
        {
            throw new NotSupportedException( "This repository is read-only." );
        }

        public T Create<T>( ) where T : class, IEntity
        {
            throw new NotSupportedException( "This repository is read-only." );
        }

        public IEntity Get( long id )
        {
            throw new NotSupportedException( "Use overload that requires a preloadQuery" );
        }

        public T Get<T>( long id ) where T : class, IEntity
        {
            throw new NotSupportedException( "Use overload that requires a preloadQuery" );
        }

        public IReadOnlyCollection<IEntity> Get( IEnumerable<long> ids )
        {
            throw new NotSupportedException( "Use overload that requires a preloadQuery" );
        }

        public IReadOnlyCollection<T> Get<T>( IEnumerable<long> ids ) where T : class, IEntity
        {
            throw new NotSupportedException( "Use overload that requires a preloadQuery" );
        }
        #endregion


        /// <summary>
        /// Loads a graph of entities and returns the root.
        /// </summary>
        /// <param name="id">The root entity of the graph.</param>
        /// <param name="preloadQuery">The entity member request of what related content to load.</param>
        /// <returns>An IEntity that represents the root node, or null if it could not be found.</returns>
        public IEntity Get( long id, string preloadQuery )
        {
            IEntity entity = GetImpl( id.ToEnumerable( ), preloadQuery, SecurityOption.DemandAll ).SingleOrDefault( );
            return entity;
        }


        /// <summary>
        /// Loads a graph of entities and returns the root.
        /// </summary>
        /// <typeparam name="T">The strong type of the root entity that is expected.</typeparam>
        /// <param name="id">The root entity of the graph.</param>
        /// <param name="preloadQuery">The entity member request of what related content to load.</param>
        /// <returns>A strongly typed entity that represents the root node, or null if it could not be found.</returns>
        public T Get<T>( long id, string preloadQuery ) where T : class, IEntity
        {
            IEntity entity = GetImpl( id.ToEnumerable( ), preloadQuery, SecurityOption.DemandAll ).SingleOrDefault( );

            return entity?.As<T>( );
        }


        /// <summary>
        /// Loads a graph of entities and returns the root.
        /// </summary>
        /// <param name="ids">The root entities of the graph.</param>
        /// <param name="preloadQuery">The entity member request of what related content to load.</param>
        /// <returns>A collection of entities that represent the root nodes, or empty if none could be found.</returns>
        public IReadOnlyCollection<IEntity> Get( IEnumerable<long> ids, string preloadQuery )
        {
            var result = GetImpl( ids, preloadQuery, SecurityOption.SkipDenied ).ToList( );
            return result;
        }


        /// <summary>
        /// Loads a graph of entities and returns the root.
        /// </summary>
        /// <typeparam name="T">The strong type of the root entities that are expected.</typeparam>
        /// <param name="ids">The root entities of the graph.</param>
        /// <param name="preloadQuery">The entity member request of what related content to load.</param>
        /// <returns>A collection of entities that represent the root nodes, or empty if none could be found.</returns>
        public IReadOnlyCollection<T> Get<T>( IEnumerable<long> ids, string preloadQuery ) where T : class, IEntity
        {
            IReadOnlyCollection<T> result = GetImpl( ids, preloadQuery, SecurityOption.SkipDenied )
                .Select( e => e.As<T>( ) )
                .Where( e => e != null )
                .ToList( );

            return result;
        }


        /// <summary>
        /// Loads a graph of entities and returns the root.
        /// </summary>
        /// <param name="ids">The root entity of the graph.</param>
        /// <param name="preloadQuery">The entity member request of what related content to load.</param>
        /// <param name="securityOption"></param>
        /// <returns>An IEntity that represents the root node, or null if it could not be found.</returns>
        private IEnumerable<IEntity> GetImpl( IEnumerable<long> ids, string preloadQuery, SecurityOption securityOption )
        {
            CheckActivation( );

            if (ids == null)
                throw new ArgumentNullException("ids");
            if (preloadQuery == null)
                throw new ArgumentNullException("preloadQuery");

            // Check IDs
            // (implemented within 'select' to participate in the single-pass only)
            Func <long, EntityRef> checkAndConvertId = id =>
            {
                if (EntityId.IsTemporary(id))
                    throw new InvalidOperationException("GraphEntityRepository cannot load temporary entities.");
                return new EntityRef( id );
            };

            var entityRefs = ids.Select( checkAndConvertId );            

            EntityRequest request = new EntityRequest( entityRefs, preloadQuery, "GraphEntityRepository" );

            // Load data, unsecured, via EntityInfoService cache
            BulkRequestResult unsecuredResult = BulkResultCache.GetBulkResult( request );

            List<long> loadedRootEntities = unsecuredResult.RootEntitiesList;

            if ( loadedRootEntities.Count == 0 )
            {
                return EmptyEntityList;
            }

            // Create the graph
            long tenantId = RequestContext.TenantId;
            var graph = new SecuredGraphEntityDataRepository( tenantId, unsecuredResult, EntityAccessControlService );

            // Secure the root entities
            IReadOnlyCollection<long> securedRootEntityIds = graph.SecureList( loadedRootEntities, securityOption );

            // Create wrappers
            IEnumerable<IEntity> result = securedRootEntityIds.Select( id => new GraphEntity( id, graph ) );
            return result;
        }

        /// <summary>
        /// Ensure we're ready to run.
        /// </summary>
        private void CheckActivation( )
        {
            // Note: unfortunately we could not use constructor injection due to circular dependencies.
            // GraphRepo->AccessControl->QueryEngine->GraphRepo
            if ( EntityAccessControlService == null )
            {
                throw new InvalidOperationException( "GraphEntityRepository.EntityAccessControlService has not been set." );
            }
        }
    }
}
