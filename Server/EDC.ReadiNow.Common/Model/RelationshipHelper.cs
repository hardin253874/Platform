// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Extends the generated relationship class.
    /// </summary>
    public static class RelationshipHelper
    {
        /// <summary>
        /// Returns true if the relationship points to a single resource in the direction specified.
        /// Returns false if it may point to multiple resources.
        /// </summary>
        /// <param name="cardinalityAlias"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsLookup(string cardinalityAlias, Direction direction)
        {
            if (cardinalityAlias == "core:oneToOne")
                return true;
            if (direction == Direction.Forward)
            {
                return cardinalityAlias == "core:manyToOne";
            }
            else
            {
                return cardinalityAlias == "core:oneToMany";
            }
        }

		/// <summary>
		/// Returns true if the relationship points to a single resource in the direction specified.
		/// Returns false if it may point to multiple resources.
		/// </summary>
		/// <param name="relationship">The relationship.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">relationship</exception>
		/// <exception cref="System.ArgumentException">Cardinality not set.;relationship</exception>
        public static bool IsLookup(this Relationship relationship, Direction direction)
        {
            if ( relationship == null )
                throw new ArgumentNullException( "relationship" );
            if ( relationship.Cardinality == null )
                throw new ArgumentException( "Cardinality not set.", "relationship" );

            return IsLookup(relationship.Cardinality.Alias, direction);
        }

        /// <summary>
        /// Returns the opposite direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Direction Reverse( this Direction direction )
        {
            if ( direction == Direction.Forward )
                return Direction.Reverse;
            return Direction.Forward;
        }

        /// <summary>
        /// Return the display name of the relationship.
        /// </summary>
        /// <returns>The Forward or Reverse Name if set depending on direction, defaulting to the Name</returns>
        public static string DisplayName(this Relationship relationship, Direction direction)
        {
            if (direction == Direction.Forward)
                return relationship.ToName ?? relationship.Name;
            else
                return relationship.FromName ?? relationship.Name;
        }
    }
}
