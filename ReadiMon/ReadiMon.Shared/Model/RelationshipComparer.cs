// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     Relationship comparer.
	/// </summary>
	public class RelationshipComparer : IComparer<Relationship>
	{
		/// <summary>
		///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in
		///     the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero
		///     <paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than
		///     <paramref name="y" />.
		/// </returns>
		public int Compare( Relationship x, Relationship y )
		{
			if ( x.Type != y.Type )
			{
				return String.Compare( x.Type, y.Type, StringComparison.Ordinal );
			}

			var fwdX = x as ForwardRelationship;
			var fwdY = y as ForwardRelationship;

			if ( fwdX != null && fwdY != null )
			{
				return String.Compare( fwdX.To, fwdY.To, StringComparison.Ordinal );
			}

			var revX = x as ReverseRelationship;
			var revY = y as ReverseRelationship;

			if ( revX != null && revY != null )
			{
				return String.Compare( revX.From, revY.From, StringComparison.Ordinal );
			}

			return 0;
		}
	}
}