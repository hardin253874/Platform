// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// A minimal set that will usually only hold one instance, but occasionally hold more.
    /// </summary>
    /// <remarks>
    /// Motivation : don't pay the price of hosting a set when 99.9% of the time there won't be more than one instance.
    /// Note to reader: Feel free to implement ISet.
    /// </remarks>
    internal class TypicallySingleEntry<T> : IEnumerable<T>
    {
        private bool hasEntry = false;

        private T _singleEntry;

        private ISet<T> _entries;

        /// <summary>
        /// Add an entry
        /// </summary>
        /// <param name="item"></param>
        public void Add( T item )
        {
            if ( !hasEntry )
            {
                hasEntry = true;
                _singleEntry = item;
            }
            else if ( _entries == null )
            {
                // Handle case of re-adding same single item
                if ( !EqualityComparer<T>.Default.Equals( _singleEntry, item ) )
                {
                    _entries = new HashSet<T>( );
                    _entries.Add( _singleEntry );
                    _entries.Add( item );
                }
            }
            else
            {
                _entries.Add( item );
            }
        }

        /// <summary>
        /// Get enumerator.
        /// </summary>
        public IEnumerator<T> GetEnumerator( )
        {
            if ( !hasEntry )
                return Enumerable.Empty<T>( ).GetEnumerator( );
            else if ( _entries == null )
                return _singleEntry.ToEnumerable<T>( ).GetEnumerator( );
            else
                return _entries.GetEnumerator( );
        }

        /// <summary>
        /// Get oldschool enumerator.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
        {
            if ( !hasEntry )
                return ( ( System.Collections.IEnumerable ) Enumerable.Empty<T>( ) ).GetEnumerator( );
            else if ( _entries == null )
                return ( ( System.Collections.IEnumerable ) _singleEntry.ToEnumerable<T>( ) ).GetEnumerator( );
            else
                return ( ( System.Collections.IEnumerable ) _entries ).GetEnumerator( );
        }
    }
}
