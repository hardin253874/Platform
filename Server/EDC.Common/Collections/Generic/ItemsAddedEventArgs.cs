// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
    /// <summary>
    /// Raised by an <see cref="AddOnlySet{T}"/> when items are added to a collection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the items added.
    /// </typeparam>
    public class ItemsAddedEventArgs<T>: EventArgs
    {
        /// <summary>
        /// Create a new <see cref="ItemsAddedEventArgs{T}"/>.
        /// </summary>
        /// <param name="items">
        /// The items to add. This cannot be null.
        /// </param>
        public ItemsAddedEventArgs(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException( nameof( items ) );
            }

            Items = new List<T>(items).AsReadOnly();
        }
          
        /// <summary>
        /// The item added.
        /// </summary>
        public IList<T> Items { get; private set; }
    }
}