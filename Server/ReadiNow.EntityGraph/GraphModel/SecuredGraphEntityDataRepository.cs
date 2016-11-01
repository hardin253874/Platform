// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Core;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// Applies security over GraphEntityGragh.
    /// </summary>
    /// <remarks>
    /// Note: this may only be used by a single user account.
    /// </remarks>
    class SecuredGraphEntityDataRepository : GraphEntityGraph
    {
        /// <summary>
        /// The current user, for security.
        /// </summary>
        private long _securedForUser = 0;

        /// <summary>
        /// Predicate that resolves whether the current user has read access for a given entity. 
        /// </summary>
        private Predicate<long> _canRead;

        /// <summary>
        /// Handle for security service.
        /// </summary>
        private IEntityAccessControlService _entityAccessControlService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tenantId">Tenant that the data applies to.</param>
        /// <param name="unsecuredGraphData">Underlying graph data.</param>
        /// <param name="entityAccessControlService">Security service.</param>
        internal SecuredGraphEntityDataRepository( long tenantId, BulkRequestResult unsecuredGraphData, IEntityAccessControlService entityAccessControlService ) : base(tenantId, unsecuredGraphData )
        {
            _entityAccessControlService = entityAccessControlService;
        }


        /// <summary>
        /// Override access to relationships to apply security.
        /// </summary>
        /// <param name="entityId">Source entity.</param>
        /// <param name="relTypeId">Relationship type ID.</param>
        /// <param name="direction">Direction to follow relationship.</param>
        /// <param name="relationshipValues">Result containing target entity IDs.</param>
        /// <returns>True if the value was loaded, or if the value was originally requested. False if it was never originally loaded.</returns>
        public override bool TryGetRelationship( long entityId, long relTypeId, Direction direction, out IReadOnlyCollection<long> relationshipValues )
        {
            IReadOnlyCollection<long> unsecuredValues;

            if ( !base.TryGetRelationship( entityId, relTypeId, direction, out unsecuredValues ) )
            {
                relationshipValues = base.Empty;
                return false;
            }

            long relTypeAndDir = direction == Direction.Forward ? relTypeId : -relTypeId;
            
            relationshipValues = SecureList( unsecuredValues, SecurityOption.SkipDenied, relTypeAndDir );
            return true;
        }


        /// <summary>
        /// Returns a secured version of the list of entity IDs.
        /// </summary>
        /// <remarks>
        /// The first instance of an actual security check will result in security being calculated for the entire graph.
        /// The applicable user is noted, and if a different user attempts to access, then an exception is thrown. 
        /// Note: this does not affect the cacheability, as caching happens at BulkRequestResult, which is unsecured.
        /// </remarks>
        /// <param name="list">The entities to secure</param>
        /// <param name="secureDemand">True for a security demand, false for a security check. I.e. throw vs return non-existant.</param>
        /// <param name="relTypeAndDir">Optionally, the relationship along which the entities were reached. Use negative for reverse.</param>
        /// <returns>The list to which the user has read permission.</returns>
        internal IReadOnlyCollection<long> SecureList( IReadOnlyCollection<long> list, SecurityOption securityOption, long relTypeAndDir = 0 )
        {
            // Apply security bypass context
            if ( SecurityBypassContext.IsActive )
            {
                return list;
            }

            // If this relationship implies security, then immediately return results
            if ( relTypeAndDir != 0 )
            {
                RelationshipInfo relationshipInfo;
                if ( _unsecuredGraphData.BulkSqlQuery.Relationships.TryGetValue( relTypeAndDir, out relationshipInfo ) )
                {
                    if ( relationshipInfo.ImpliesSecurity )
                        return list;
                }
                // else assert false
            }

            // Determine if we have pre-calculated security
            if ( _canRead == null )
            {
                // Security check on entire graph excluding implicitly secured)
                // Results returned as a predicate that performs fast lookup
                _canRead = BulkRequestResultSecurityHelper.GetEntityReadability( _entityAccessControlService, _unsecuredGraphData );

                _securedForUser = RequestContext.GetContext( ).Identity.Id;
            }
            else
            {
                // Ensure we are still accessing as the same user.
                long currentUser = RequestContext.GetContext( ).Identity.Id;
                if ( currentUser != _securedForUser )
                {
                    throw new InvalidOperationException( "An entity graph may only be accessed by a single user." );
                }
            }

            if ( securityOption == SecurityOption.DemandAll )
            {
                if ( list.Any( id => !_canRead(id ) ) )
                {
                    // Security access has already failed, but for consistency of code paths, let Demand throw the exception.
                    var ids = list.Select( id => new EntityRef(id)).ToList();
                    _entityAccessControlService.Demand( ids, new [ ] { Permissions.Read } );
                    throw new Exception( "Assert false - Demand should have thrown PlatformSecurityException" );
                }
                return list;
            }
            else
            {
                List<long> result = list.Where( id => _canRead( id ) ).ToList( );
                return result;
            }

        }
    }
}
