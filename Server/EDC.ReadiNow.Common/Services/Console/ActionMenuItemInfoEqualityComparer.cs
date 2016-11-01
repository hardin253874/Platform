// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace EDC.ReadiNow.Services.Console
{
	/// <summary>
	///     ActionMenuItemInfo EqualityComparer.
	/// </summary>
	public class ActionMenuItemInfoEqualityComparer : IEqualityComparer<ActionMenuItemInfo>
	{
		/// <summary>
		///     Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">
		///     The first object of type
		///     <paramref>
		///         <name>T</name>
		///     </paramref>
		///     to compare.
		/// </param>
		/// <param name="y">
		///     The second object of type
		///     <paramref>
		///         <name>T</name>
		///     </paramref>
		///     to compare.
		/// </param>
		/// <returns>
		///     true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals( ActionMenuItemInfo x, ActionMenuItemInfo y )
		{
			if ( x == null )
			{
				return y == null;
			}
			if ( y == null )
			{
				return false;
			}

			if ( !string.IsNullOrEmpty( x.Name ) )
			{
				if ( x.Name.StartsWith( "zz_" ) )
				{
					return false;
				}
			}

			if ( !string.IsNullOrEmpty( y.Name ) )
			{
				if ( y.Name.StartsWith( "zz_" ) )
				{
					return false;
				}
			}

			return x.Id == y.Id && x.Name == y.Name;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public int GetHashCode( ActionMenuItemInfo obj )
		{
			if ( obj == null )
			{
				return 0;
			}
			return obj.Id.GetHashCode( );
		}
	}
}