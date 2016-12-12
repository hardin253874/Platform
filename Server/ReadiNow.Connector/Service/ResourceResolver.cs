// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Diagnostics;

namespace ReadiNow.Connector.Service
{
    /// <summary>
    /// A class for finding records by an API mapping rule. (i.e. by name).
    /// </summary>
    class ResourceResolver : IResourceResolver
    {
        private readonly long _typeId;
        private readonly IEntityResolver _entityResolver;
        private readonly bool _allowGuids;

        /// <summary>
        /// Constructor
        /// </summary>
        public ResourceResolver( IEntityResolver entityResolver, long typeId, bool allowGuids)
        {
            _typeId = typeId;
            _entityResolver = entityResolver;
            _allowGuids = allowGuids;
        }

		/// <summary>
		/// Look up an entity according to its identifier and any rules specified in the mapping resource.
		/// </summary>
		/// <param name="resourceIdentity">The resource identity from the URI, such as name or autonumber.</param>
		/// <returns>
		/// The entity
		/// </returns>
		/// <exception cref="System.ArgumentNullException">resourceIdentity</exception>
        public ResourceResolverEntry ResolveResource( string resourceIdentity )
        {
            if ( string.IsNullOrEmpty( resourceIdentity ) )
                throw new ArgumentNullException( "resourceIdentity" );

            var identities = new [ ] { resourceIdentity };
            var dict = ResolveResources( identities );
            ResourceResolverEntry result = dict[ resourceIdentity ];

            return result;
        }

        /// <summary>
        /// Look up an entity according to its identifier and any rules specified in the mapping resource.
        /// </summary>
        /// <param name="resourceIdentities">The resource identities.</param>
        /// <returns>
        /// The entity
        /// </returns>
        /// <exception cref="System.ArgumentNullException">resourceIdentities</exception>
        public IDictionary<object, ResourceResolverEntry> ResolveResources( IReadOnlyCollection<object> resourceIdentities )
        {
            if ( resourceIdentities == null )
                throw new ArgumentNullException( "resourceIdentities" );

            if ( resourceIdentities.Count == 0 )
                return new Dictionary<object, ResourceResolverEntry>( );

            // Determine if we should use GUIDs by inspecing first. (TODO: this should be passed in configuration).
            bool useGuid = false;
            if ( _allowGuids )
            {
                object sample = resourceIdentities.First();
                Guid entityGuid;
                useGuid = Guid.TryParse( sample as string, out entityGuid );
            }

            if ( useGuid )
                return ResolveResourcesByGuid( resourceIdentities );
            else
                return ResolveResourcesByField( resourceIdentities );
        }

        private IDictionary<object, ResourceResolverEntry> ResolveResourcesByField( IReadOnlyCollection<object> resourceIdentities )
        {
            var result = new Dictionary<object, ResourceResolverEntry>( );

            // Resolve entity (use Name for now)
            ILookup<object, long> lookup = _entityResolver.GetEntitiesByField( resourceIdentities );

            // Look up IDs for the identitifiers
            Dictionary<long, object> entityIDs = new Dictionary<long, object>( );

            foreach (object resourceIdentity in resourceIdentities)
            {
                if ( !lookup.Contains( resourceIdentity ) )
                {
                    result[ resourceIdentity ] = new ResourceResolverEntry( ResourceResolverError.ResourceNotFoundByField );
                    continue;
                }
                
                // Check entity count
                long [ ] entityIds = lookup [ resourceIdentity ].Take( 2 ).ToArray( );

                if ( entityIds.Length != 1 )
                {
                    result [ resourceIdentity ] = new ResourceResolverEntry( ResourceResolverError.ResourceNotUniqueByField );
                    continue;
                }

                entityIDs.Add( entityIds [ 0 ], resourceIdentity );
            }

            // Load entities for the IDs.
            IEnumerable<IEntity> entities = Entity.Get( entityIDs.Keys );

            foreach (IEntity entity in entities)
            {
                object resourceIdentity = entityIDs [ entity.Id ];
                result [ resourceIdentity ] = new ResourceResolverEntry( entity );
            }

            return result;
        }


        /// <summary>
        /// Attempt to resolve a resource by its GUID.
        /// </summary>
        /// <param name="resourceIdentities"></param>
        /// <returns></returns>
        private IDictionary<object, ResourceResolverEntry> ResolveResourcesByGuid( IReadOnlyCollection<object> resourceIdentities )
        {
            var result = new Dictionary<object, ResourceResolverEntry>( );

            foreach ( object oResourceIdentity in resourceIdentities )
            {
                string resourceIdentity = oResourceIdentity as string;
                if ( resourceIdentity == null )
                    continue;

                Guid entityGuid = Guid.Parse( resourceIdentity );
                            
                IEntity entity = null;

                try
                {
                    long entityId = Entity.GetIdFromUpgradeId( entityGuid );
                    if ( entityId >= 0 )
                    {
                        entity = Entity.Get( entityId );
                    }

                    if ( entity != null )
                    {
                        bool correctType = PerTenantEntityTypeCache.Instance.IsInstanceOf( entity, _typeId );
                        if ( !correctType )
                        {
                            entity = null;
                        }
                    }
                }
                catch ( Exception ex )
                {
                    // Do not allow any error info or discreptency of behavior to reach the caller
                    // We are dealing with GUIDs, which are well known, and any info leak could be used to discover the presence or security of unrelated entities.
                    EventLog.Application.WriteError( ex.ToString( ) );
                }

                if ( entity == null )
                {
                    result[ resourceIdentity ] = new ResourceResolverEntry( ResourceResolverError.ResourceNotFoundByGuid );
                }
                else
                {
                    result [ resourceIdentity ] = new ResourceResolverEntry( entity );
                }
            }
            return result;
        }

        internal static string FormatResolverError( ResourceResolverError error, string identity )
        {
            string message;
            switch ( error )
            {
                case ResourceResolverError.ResourceNotFoundByField:
                    message = Messages.ResourceNotFoundByField;
                    break;
                case ResourceResolverError.ResourceNotUniqueByField:
                    message = Messages.ResourceNotUniqueByField;
                    break;
                case ResourceResolverError.ResourceNotFoundByGuid:
                    message = Messages.ResourceNotFoundByGuid;
                    break;
                default:
                    throw new InvalidOperationException( "Unexpected error state." );
            }
            string result = string.Format( message, identity );
            return result;
        }
    }
}
