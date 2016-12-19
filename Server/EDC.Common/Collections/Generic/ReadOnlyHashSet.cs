// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     Read-Only hash set.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReadOnlyHashSet<T> : IReadOnlySet<T>
	{
		/// <summary>
		///     The inner set.
		/// </summary>
		private readonly ISet<T> _innerSet;

		/// <summary>
		///     Initializes a new instance of the <see cref="ReadOnlyHashSet{T}" /> class.
		/// </summary>
		/// <param name="innerSet">The inner set.</param>
		/// <exception cref="System.ArgumentNullException">innerSet</exception>
		public ReadOnlyHashSet( ISet<T> innerSet )
		{
			if ( innerSet == null )
			{
				throw new ArgumentNullException( nameof( innerSet ) );
			}

			_innerSet = innerSet;
		}

		/// <summary>
		///     Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		///     Determines whether the set contains the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public bool Contains( T item )
		{
			return _innerSet.Contains( item );
		}

		/// <summary>
		///     Copies the set to the specified array.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo( T[ ] array, int arrayIndex )
		{
			_innerSet.CopyTo( array, arrayIndex );
		}

		/// <summary>
		///     Determines whether this set is a proper subset of the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		public bool IsProperSubsetOf( IEnumerable<T> other )
		{
			return _innerSet.IsProperSubsetOf( other );
		}

		/// <summary>
		///     Determines whether this set is a proper superset of the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		public bool IsProperSupersetOf( IEnumerable<T> other )
		{
			return _innerSet.IsProperSupersetOf( other );
		}

		/// <summary>
		///     Determines whether this set is subset of the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		public bool IsSubsetOf( IEnumerable<T> other )
		{
			return _innerSet.IsSubsetOf( other );
		}

		/// <summary>
		///     Determines whether this set is superset of the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		public bool IsSupersetOf( IEnumerable<T> other )
		{
			return _innerSet.IsSupersetOf( other );
		}

		/// <summary>
		///     Determines whether this set overlaps the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		public bool Overlaps( IEnumerable<T> other )
		{
			return _innerSet.Overlaps( other );
		}

		/// <summary>
		///     Determines whether this set equals the specified value.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public bool SetEquals( IEnumerable<T> other )
		{
			return _innerSet.SetEquals( other );
		}

		/// <summary>
		///     Gets the number of elements in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return _innerSet.Count;
			}
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator( )
		{
			return _innerSet.GetEnumerator( );
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return GetEnumerator( );
		}
	}
}