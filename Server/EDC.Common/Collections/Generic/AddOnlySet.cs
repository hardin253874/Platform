// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EDC.Collections.Generic
{
    /// <summary>
    /// Delegate for <see cref="AddOnlySet{T}.ItemsAdded"/>.
    /// </summary>
    /// <param name="sender">
    /// Object that raised the event.
    /// </param>
    /// <param name="e">
    /// Event-specific args.s
    /// </param>
    public delegate void ItemsAddedEventHandler<T>(object sender, ItemsAddedEventArgs<T> e);

    /// <summary>
    /// An enumerable set (i.e. ignores duplicates), that only allows items to be added.
    /// </summary>
    /// <typeparam name="T">
    /// The type of item stored in the set.
    /// </typeparam>
    public class AddOnlySet<T>: IEnumerable<T>
    {
        /// <summary>
        /// Use a <see cref="ConcurrentDictionary{K, V}"/> with a dummy value to handle concurrency.
        /// </summary>
        private readonly ConcurrentDictionary<T, object> _set;

        /// <summary>
        /// Create a new <see cref="AddOnlySet{T}"/>.
        /// </summary>
        /// <param name="equalityComparer">
        /// Optional equality comparer.
        /// </param>
        public AddOnlySet(IEqualityComparer<T> equalityComparer = null)
        {
            _set = new ConcurrentDictionary<T, object>(equalityComparer ?? EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Raised when items are added to the set.
        /// </summary>
        public event ItemsAddedEventHandler<T> ItemsAdded;

        /// <summary>
        /// Add items to the set.
        /// </summary>
        /// <param name="items">
        /// The items to add. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> cannot be null.
        /// </exception>
        public void Add(params T[] items)
        {
	        if ( items == null )
	        {
				throw new ArgumentNullException( nameof( items ) );
	        }

	        if ( items.Length <= 0 )
	        {
		        return;
	        }

	        Add((IEnumerable<T>)items);
        }

        /// <summary>
        /// Add items to the set.
        /// </summary>
        /// <param name="items">
        /// The items to add. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="items"/> cannot contain null.
        /// </exception>
        public void Add(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException( nameof( items ) );
            }

	        IList<T> itemsAdded = items.Where(item => _set.TryAdd(item, null)).ToList();

	        if ( itemsAdded.Count > 0 )
	        {
		        RaiseItemsAdded( itemsAdded );
	        }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _set.Select(pair => pair.Key).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Number of items in the set.
        /// </summary>
        public int Count
        {
            get { return _set.Count; }
        }

        /// <summary>
        /// Raise the <see cref="ItemsAdded"/> event.
        /// </summary>
        /// <param name="items">
        /// The items to add. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> cannot be null.
        /// </exception>
        protected void RaiseItemsAdded(ICollection<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException( nameof( items ) );
            }

            ItemsAddedEventHandler<T> itemsAddedEventHandler;

            itemsAddedEventHandler = ItemsAdded;
            if (itemsAddedEventHandler != null)
            {
                itemsAddedEventHandler(this, new ItemsAddedEventArgs<T>(items)); 
            }
        }
    }
}
