// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using EDC.Collections.Generic;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// A context that captures entities affecting caches.
    /// </summary>
    public sealed class CacheContext: IDisposable
    {
        private readonly CacheContextEntry _entry;
        private readonly CacheContextEntry _parent;
        private bool _disposed = false;

        [ThreadStatic]
        private static CacheContextEntry _current;

        /// <summary>
        /// Create a new <see cref="CacheContext"/>.
        /// </summary>
        public CacheContext()
            : this(ContextType.New)
        {
            // Do nothing
        }

        /// <summary>
        /// Creata a new <see cref="CacheContext"/> from
        /// an existing set of entities.
        /// </summary>
        /// <param name="contextType">
        /// Whether this is a new or attached context.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="contextType"/> is unknown or invalid.
        /// </exception>
        internal CacheContext(ContextType contextType)
        {
            ContextType = contextType;
            _parent = _current;

            if (contextType == ContextType.New)
            {
                _entry = new CacheContextEntry();

                if ( _parent != null )
                {
                    _entry.Entities.ItemsAdded += ( o, e ) => _parent.Entities.Add( e.Items );
                    _entry.RelationshipTypes.ItemsAdded += ( o, e ) => _parent.RelationshipTypes.Add( e.Items );
                    _entry.FieldTypes.ItemsAdded += ( o, e ) => _parent.FieldTypes.Add( e.Items );
                    _entry.EntityInvalidatingRelationshipTypes.ItemsAdded += ( o, e ) => _parent.EntityInvalidatingRelationshipTypes.Add( e.Items );
                    _entry.EntityTypes.ItemsAdded += ( o, e ) => _parent.EntityTypes.Add( e.Items );
                }
            }
            else if(contextType == ContextType.Attached)
            {
                _entry = _current;
            }
            else if (contextType == ContextType.Detached)
            {
                _entry = new CacheContextEntry();
            }
            else if ( contextType == ContextType.None )
            {
                _entry = null;
            }
            else
            {
                throw new ArgumentException("Unknown contextType", "contextType");
            }

            // Placed last incase exception is thrown in constructor.
            _current = _entry;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~CacheContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        /// <param name="disposing">
        /// True if called from Dispose, false if called from the finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            // No unmanaged resources so disposing is unused

            if (!_disposed)
            {
                _current = _parent;

                _disposed = true;
            }
        }

        /// <summary>
        /// Add the invalidations for the given key to the context. Used for a cache hit. 
        /// </summary>
        /// <param name="cacheInvalidator"></param>
        /// <param name="key"></param>
        public void AddInvalidationsFor<TKey, TValue>(CacheInvalidator<TKey, TValue> cacheInvalidator, TKey key)
        {
            if (cacheInvalidator == null)
                throw new ArgumentNullException("cacheInvalidator");
            if ( _entry == null )
                throw new InvalidOperationException( "Not valid for ContextType.None" );

            Entities.Add(cacheInvalidator.EntityToCacheKey.GetKeys(key));
            RelationshipTypes.Add(cacheInvalidator.RelationshipTypeToCacheKey.GetKeys(key));
            FieldTypes.Add(cacheInvalidator.FieldTypeToCacheKey.GetKeys(key));
            EntityInvalidatingRelationshipTypes.Add(cacheInvalidator.EntityInvalidatingRelationshipTypesToCacheKey.GetKeys(key));
            EntityTypes.Add(cacheInvalidator.EntityTypeToCacheKey.GetKeys(key));
        }

        /// <summary>
        /// Get the current (top most) cache context if the context exists or a 
        /// detached context otherwise.
        /// </summary>
        /// <returns>
        /// The current <see cref="CacheContext"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// There is no current cache context.
        /// </exception>
        /// <seealso cref="IsSet"/>
        public static CacheContext GetContext()
        {
            return new CacheContext(IsSet() ? ContextType.Attached : ContextType.Detached);
        }

        /// <summary>
        /// Has the context been set, i.e. are we in a using block? 
        /// </summary>
        /// <returns>
        /// True if the context has been set, false otherwise.
        /// </returns>
        public static bool IsSet()
        {
            return _current != null;
        }

        /// <summary>
        /// Entities accessed.
        /// </summary>
        public AddOnlySet<long> Entities
        {
            get
            {
                if ( _entry == null )
                    throw new InvalidOperationException( "Not valid for ContextType.None" );
                return _entry.Entities;
            }
        }

        /// <summary>
        /// Relationship types accessed.
        /// </summary>
        /// <remarks>
        /// When an instance of those relationships are modified, it invalidates the 
        /// entries in the cache. This is useful for queries and report conditions, 
        /// that follow a relationship.
        /// </remarks>
        public AddOnlySet<long> RelationshipTypes
        {
            get
            {
                if ( _entry == null )
                    throw new InvalidOperationException( "Not valid for ContextType.None" );
                return _entry.RelationshipTypes;
            }
        }

        /// <summary>
        /// Field types accessed.
        /// </summary>
        /// <remarks>
        /// When an instance of those fields are modified, it invalidates the 
        /// entries in the cache. This is useful for queries and report conditions, 
        /// that reference a field.
        /// </remarks>
        public AddOnlySet<long> FieldTypes
        {
            get
            {
                if ( _entry == null )
                    throw new InvalidOperationException( "Not valid for ContextType.None" );
                return _entry.FieldTypes;
            }
        }

        /// <summary>
        /// Relationships to follow to find more invalidating entities.
        /// </summary>
        /// <remarks>
        /// When an entity is saved, follow these relationships to see whether
        /// they relate to other entities in <see cref="EntityTypes"/>. This ensures
        /// cache-invalidating changes cannot be circumvented by saving the other end
        /// of a modified relationship.
        /// </remarks>
        public AddOnlySet<long> EntityInvalidatingRelationshipTypes
        {
            get
            {
                if ( _entry == null )
                    throw new InvalidOperationException( "Not valid for ContextType.None" );
                return _entry.EntityInvalidatingRelationshipTypes;
            }
        }

        /// <summary>
        /// EntityTypes.
        /// </summary>
        /// <remarks>
        /// Invalidate entries in the cache when entities of these types are saved.
        /// </remarks>
        public AddOnlySet<long> EntityTypes
        {
            get
            {
                if ( _entry == null )
                    throw new InvalidOperationException( "Not valid for ContextType.None" );
                return _entry.EntityTypes;
            }
        }

        /// <summary>
        /// Whether this context instance is new or attached.
        /// </summary>
        public ContextType ContextType { get; private set; }

    }
}
