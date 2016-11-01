// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
    /// <summary>
	///     IEqualityComparer for IEntity instances.
	/// </summary>
	public class EntityEqualityComparer : IEqualityComparer<IEntity>
	{
		/// <summary>
		///     Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">
		///     The first object to compare.
		/// </param>
		/// <param name="y">
		///     The second object to compare.
		/// </param>
		/// <returns>
		///     true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals( IEntity x, IEntity y )
		{
			if ( x == null && y == null )
			{
				return true;
			}

			if ( x == null || y == null )
			{
				return false;
			}

			var xInternal = x as IEntityInternal;
			var yInternal = y as IEntityInternal;

			return EntityInternalEquality( x, y, xInternal, yInternal );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		///     The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.
		/// </exception>
		public int GetHashCode( IEntity obj )
		{
			if ( obj == null )
			{
				throw new ArgumentNullException( "obj" );
			}

			var objInternal = obj as IEntityInternal;

			if ( objInternal == null )
			{
				return obj.Id.GetHashCode( ) ^ obj.TypeIds.GetHashCode( );
			}

			int hashCode = obj.Id.GetHashCode( ) ^ obj.TypeIds.GetHashCode( ) ^ objInternal.IsReadOnly.GetHashCode( ) ^ objInternal.IsTemporaryId.GetHashCode( );

			if ( objInternal.CloneSource != null )
			{
				hashCode ^= objInternal.CloneSource.GetHashCode( );
			}

			if ( objInternal.ModificationToken != null )
			{
				hashCode ^= objInternal.ModificationToken.GetHashCode( );
			}

			return hashCode;
		}

		/// <summary>
		///     Compares the equality.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="xInternal">The x internal.</param>
		/// <param name="yInternal">The y internal.</param>
		/// <returns>
		///     True of the instances are equal; false otherwise.
		/// </returns>
		private static bool CompareEquality( IEntity x, IEntity y, IEntityInternal xInternal, IEntityInternal yInternal )
		{
			return x.Id == y.Id &&
			       xInternal.CloneSource == yInternal.CloneSource &&
			       xInternal.IsReadOnly == yInternal.IsReadOnly &&
			       xInternal.IsTemporaryId == yInternal.IsTemporaryId &&
			       xInternal.ModificationToken == yInternal.ModificationToken;
		}

		/// <summary>
		///     Is the entity internal equality.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="xInternal">The x internal.</param>
		/// <param name="yInternal">The y internal.</param>
		/// <returns>
		///     true if the specified objects are equal; otherwise, false.
		/// </returns>
		private static bool EntityInternalEquality( IEntity x, IEntity y, IEntityInternal xInternal, IEntityInternal yInternal )
		{
			if ( xInternal == null && yInternal == null )
			{
				return x.Id == y.Id && Equals( x.TypeIds, y.TypeIds );
			}

			if ( xInternal != null && yInternal != null )
			{
				return CompareEquality( x, y, xInternal, yInternal );
			}

			return false;
		}
	}
}