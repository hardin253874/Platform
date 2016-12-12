// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.Collections.Generic;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// Presents a list as either an IEntityRelationshipCollection or an IEntityCollection.
    /// </summary>
    /// <remarks>
    /// Read only.
    /// </remarks>
    public class EntityCollectionAdapter<T> : IEntityRelationshipCollection<T>, IEntityCollection<T> where T : class, IEntity
    {
        private static readonly string ReadOnlyError = "Collection is read-only.";

        readonly IReadOnlyList<T> _inner;

        public EntityCollectionAdapter( IReadOnlyList<T> inner )
        {
            _inner = inner;
        }

        public IEntityCollection<T> Entities
        {
            get { return this; }
        }

        public bool Contains( T item )
        {
            return _inner.Contains( item );
        }

        public void CopyTo( T [ ] array, int arrayIndex )
        {
            if (_inner == null)
                return;

            List<T> list = _inner as List<T>;
            if (list != null)
            {
                list.CopyTo(array, arrayIndex);
            }
            else
            {
                int pos = arrayIndex;
                foreach (T value in _inner)
                {
                    if (pos < array.Length)
                        array[pos++] = value;
                    else
                        break;
                }
            }
        }

        public int Count
        {
            get {  return _inner.Count; }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public IEnumerator<T> GetEnumerator( )
        {
            return _inner.GetEnumerator( );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
        {
            return ( ( System.Collections.IEnumerable ) _inner ).GetEnumerator( );
        }

        public T this [ int index ]
        {
            get
            {
                return _inner [ index ];
            }
            set
            {
                throw new InvalidOperationException( ReadOnlyError );
            }
        }

        public IChangeTracker<IMutableIdKey> Tracker
        {
            get
            {
                throw new InvalidOperationException( ReadOnlyError );
            }
        }

        #region Invalid write operatoins
        public void Add( T item )
        {
            throw new InvalidOperationException( ReadOnlyError );
        }

        public void Clear( )
        {
            throw new InvalidOperationException( ReadOnlyError );
        }

        public bool Remove( T item )
        {
            throw new InvalidOperationException( ReadOnlyError );
        }

        public void AddRange( IEnumerable<T> collection )
        {
            throw new InvalidOperationException( ReadOnlyError );
        }

        public void RemoveRange( IEnumerable<T> collection )
        {
            throw new InvalidOperationException( ReadOnlyError );
        }
        #endregion
    }
}
