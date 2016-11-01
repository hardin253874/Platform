// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Determines if two entities have the same ID.
	/// </summary>
	public class EntityIdEqualityComparer<T> : IEqualityComparer<T>
		where T : class, IEntity
	{
        public static readonly EntityIdEqualityComparer<T> Instance = new EntityIdEqualityComparer<T>();

        /// <summary>
        ///     Determine if two entities are the same.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals( T x, T y )
		{
			if ( x == null && y == null )
			{
				return true;
			}

			if ( x == null || y == null )
			{
				return false;
			}

			return x.Id == y.Id;
		}

		/// <summary>
		///     Calculates the hash-code.
		/// </summary>
		public int GetHashCode( T obj )
		{
			if ( obj == null )
			{
				return 0;
			}
			return obj.Id.GetHashCode( );
		}
	}
}