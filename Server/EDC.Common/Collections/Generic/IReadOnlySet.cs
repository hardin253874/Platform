// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     IReadOnlySet interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReadOnlySet<T> : IReadOnlyCollection<T>
	{
		/// <summary>
		///     Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		bool IsReadOnly
		{
			get;
		}

		/// <summary>
		///     Determines whether the set contains the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		bool Contains( T item );

		/// <summary>
		///     Copies the set to the specified array.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		void CopyTo( T[ ] array, int arrayIndex );

		/// <summary>
		///     Determines whether this set is a proper subset of the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		bool IsProperSubsetOf( IEnumerable<T> other );

		/// <summary>
		///     Determines whether this set is a proper superset of the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		bool IsProperSupersetOf( IEnumerable<T> other );

		/// <summary>
		///     Determines whether this set is subset of the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		bool IsSubsetOf( IEnumerable<T> other );

		/// <summary>
		///     Determines whether this set is superset of the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		bool IsSupersetOf( IEnumerable<T> other );

		/// <summary>
		///     Determines whether this set overlaps the specified value.
		/// </summary>
		/// <param name="other">The value to compare.</param>
		/// <returns></returns>
		bool Overlaps( IEnumerable<T> other );

		/// <summary>
		///     Determines whether this set equals the specified value.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		bool SetEquals( IEnumerable<T> other );
	}
}