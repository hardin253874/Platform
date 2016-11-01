// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Extensions methods for <see cref="EntityType"/>.
    /// </summary>
    public static class EntityTypeExtensions
    {
        /// <summary>
        /// Get all securing relationships.
        /// </summary>
        /// <param name="entityType">
        /// The <see cref="EntityType"/> to check. This cannot be null.
        /// </param>
        /// <param name="ancestorsOnly">
        /// True if only ancestores are included, false if all possible types are checked.
        /// </param>
        /// <param name="isModifyPermission">
        /// If true, only consider relationships that should be traversed in modify & delete scenarios.
        /// </param>
        /// <param name="getTargets">
        /// False to follow relationships in reverse to potential sources of grants.
        /// True to follow relationships in forward to target types that receive grants.
        /// </param>
        /// <returns>
        /// The relationships with the secures to or from flags set. 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityType"/> cannot be null.
        /// </exception>
        public static IList<Tuple<Relationship, Direction, EntityType, object>> GetSecuringRelationships(
            this EntityType entityType, bool ancestorsOnly, bool isModifyPermission, bool getTargets = false)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }

            IList<EntityType> entityTypes;
            IEqualityComparer<Relationship> relationshipEqualityComparer;
            IList<Tuple<Relationship, Direction, EntityType, object>> forwardRelationships;
            IList<Tuple<Relationship, Direction, EntityType, object>> reverseRelationships;

            relationshipEqualityComparer = new EntityIdEqualityComparer<Relationship>();
            if (ancestorsOnly)
            {
                entityTypes = entityType.GetAncestorsAndSelf()
                                        .ToList();
            }
            else
            {
                entityTypes = entityType.GetDescendantsAndSelf()
                                        .SelectMany(et => et.GetAncestorsAndSelf())
                                        .Distinct(new EntityIdEqualityComparer<EntityType>())
                                        .ToList();
            }

            bool isOnlyReadPerm = !isModifyPermission;



            forwardRelationships = entityTypes.SelectMany(et2 => getTargets ? et2.ReverseRelationships : et2.Relationships)
                .Distinct(relationshipEqualityComparer)
                .Where(r => (r.SecuresFrom == true) && (isOnlyReadPerm || r.SecuresFromReadOnly != true) && (r.ToType != null) )
                .Select(r => new Tuple<Relationship, Direction, EntityType, object>(r, Direction.Forward, r.ToType, null))
                .ToList();
            reverseRelationships = entityTypes.SelectMany(et2 => getTargets ? et2.Relationships : et2.ReverseRelationships)
                .Distinct(relationshipEqualityComparer)
                .Where(r => ( r.SecuresTo == true ) && ( isOnlyReadPerm || r.SecuresToReadOnly != true ) && ( r.FromType != null ) )
                .Select(r => new Tuple<Relationship, Direction, EntityType, object>(r, Direction.Reverse, r.FromType, null))
                .ToList();

            return forwardRelationships.Union(reverseRelationships).ToList();
        }


    }
}
