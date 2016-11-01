// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
    /// <summary>
    /// A readonly set.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadOnlySet<T> : ISet<T>
    {
        /// <summary>
        ///     The inner set.
        /// </summary>
        private readonly ISet<T> _innerSet;


        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlySet{T}" /> class.
        /// </summary>
        /// <param name="innerInnerSet">The inner set.</param>
        /// <exception cref="System.ArgumentNullException">set</exception>
        public ReadOnlySet(ISet<T> innerInnerSet)
        {
            if (innerInnerSet == null)
            {
                throw new ArgumentNullException("innerInnerSet");    
            }

            _innerSet = innerInnerSet;
        }


        #region ISet<T> Members


        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _innerSet.GetEnumerator();
        }


        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _innerSet).GetEnumerator();
        }


        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="System.InvalidOperationException">The set is readonly.</exception>
        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException("The set is readonly.");
        }


        /// <summary>
        ///     Modifies the current set so that it contains all elements that are present in either the current set or the
        ///     specified collection.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <exception cref="System.InvalidOperationException">The set is readonly.</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("The set is readonly.");
        }


        /// <summary>
        ///     Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <exception cref="System.InvalidOperationException">The set is readonly.</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("The set is readonly.");
        }


        /// <summary>
        ///     Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <exception cref="System.InvalidOperationException">The set is readonly.</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("The set is readonly.");
        }


        /// <summary>
        ///     Modifies the current set so that it contains only elements that are present either in the current set or in the
        ///     specified collection, but not both.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <exception cref="System.InvalidOperationException">The set is readonly.</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("The set is readonly.");
        }


        /// <summary>
        ///     Determines whether a set is a subset of a specified collection.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _innerSet.IsSubsetOf(other);
        }


        /// <summary>
        ///     Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _innerSet.IsSupersetOf(other);
        }


        /// <summary>
        ///     Determines whether the current set is a proper (strict) superset of a specified collection.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _innerSet.IsProperSupersetOf(other);
        }


        /// <summary>
        ///     Determines whether the current set is a proper (strict) superset of a specified collection.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _innerSet.IsProperSubsetOf(other);
        }


        /// <summary>
        ///     Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            return _innerSet.Overlaps(other);
        }


        /// <summary>
        ///     Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            return _innerSet.SetEquals(other);
        }


        /// <summary>
        ///     Adds the specified item to the set.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The set is readonly.</exception>
        public bool Add(T item)
        {
            throw new InvalidOperationException("The set is readonly.");
        }


        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The set is readonly.</exception>
        public void Clear()
        {
            throw new InvalidOperationException("The set is readonly.");
        }


        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
        ///     otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return _innerSet.Contains(item);
        }


        /// <summary>
        ///     Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerSet.CopyTo(array, arrayIndex);
        }


        /// <summary>
        ///     Removes the first occurrence of a specific object from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> was successfully removed from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
        ///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">The set is readonly.</exception>
        public bool Remove(T item)
        {
            throw new InvalidOperationException("The set is readonly.");
        }


        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count
        {
            get { return _innerSet.Count; }
        }


        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }


        #endregion
    }
}