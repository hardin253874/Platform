// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Converts an unsecured BulkRequestResult to a secured EntityData structure.
    /// </summary>
    static internal class BulkRequestResultSecurityHelper
    {
		/// <summary>
		/// Pre-fetches the can-read security for the list of entities.
		/// </summary>
        /// <param name="entityAccessControlService">The security service to use.</param>
		/// <param name="data">The data.</param>
		/// <returns>
		/// A predicate backed by a dictionary that can quickly return whether an entity is readable.
		/// </returns>
        public static Predicate<long> GetEntityReadability( IEntityAccessControlService entityAccessControlService, BulkRequestResult data )
        {
            if ( entityAccessControlService == null )
            {
                throw new ArgumentNullException( "entityAccessControlService" );
            }

            Predicate<long> predicate;

            if (SecurityBypassContext.IsActive)
            {
                predicate = (long entityId) => true;
            }
            else
            {
                // Get readable entities
                List<EntityRef> entitiesToExplicitlyCheck = data.AllEntities
                    .Where(pair => !pair.Value.ImplicitlySecured)
                    .Select(pair => CreateEntityRefForSecurityCheck(data, pair.Key))    // Stop! If you change .Check to take longs, then discuss with Pete first.
                    .ToList();
                IDictionary<long, bool> readableEntities = entityAccessControlService.Check( entitiesToExplicitlyCheck, new [ ] { Permissions.Read } );

                // Lookup predicate
                predicate = (long entityId) =>
                {
                    // Check if implicitly secured by relationship
                    EntityValue ev;
                    if (!data.AllEntities.TryGetValue(entityId, out ev))
                        return false;   // assert false
                    if (ev.ImplicitlySecured)
                        return true;
                    
                    // Check if explicitly secured
                    bool canRead;
                    if (readableEntities.TryGetValue(entityId, out canRead))
                        return canRead;
                    return false;
                };
            }

            return predicate;
        }

        /// <summary>
        /// Creates an entity ref for the ID to be checked.
        /// </summary>
        /// <remarks>
        /// The BulkRequestResult graph typically already has type information for the entities being loaded.
        /// The security engine requires type information to determine applicable rules.
        /// So instead of having it attempt to activate the entities (additional DB trips) to determine the types, just
        /// pass the type information along instead.
        /// This is done by creating a EntityTypeOnly implementation of IEntity, and packing it into the EntityRef that we pass.
        /// </remarks>
        /// <param name="data"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private static EntityRef CreateEntityRefForSecurityCheck( BulkRequestResult data, long entityId )
        {
            EntityRef result;
            long isOfTypeRelId = WellKnownAliases.CurrentTenant.IsOfType;

            // Look up type information
            RelationshipKey key = new RelationshipKey(entityId, isOfTypeRelId);

            List<long> typeIds;
            if (data.Relationships.TryGetValue(key, out typeIds))
            {
                IEntity entity = new EntityTypeOnly( entityId, typeIds );
                result = new EntityRef(entity);
            }
            else
            {
                result = new EntityRef( entityId );
            }

            return result;
        }

    }
}
