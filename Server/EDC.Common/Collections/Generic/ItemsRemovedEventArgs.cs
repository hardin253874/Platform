// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EDC.Collections.Generic
{
    /// <summary>
    /// Raised by an <see cref="AddOnlySet{T}"/> when items are removed form a collection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the items removed.
    /// </typeparam>
    public class ItemsRemovedEventArgs<T>: EventArgs
    {
        /// <summary>
        /// Create a new <see cref="ItemsRemovedEventArgs{T}"/>.
        /// </summary>
        /// <param name="items">
        /// The items to remove. This cannot be null.
        /// </param>
        public ItemsRemovedEventArgs(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException( nameof( items ) );
            }

            Items = new List<T>(items).AsReadOnly();            
        }
          
        /// <summary>
        /// The item removed.
        /// </summary>
        public ReadOnlyCollection<T> Items { get; private set; }
    }
}