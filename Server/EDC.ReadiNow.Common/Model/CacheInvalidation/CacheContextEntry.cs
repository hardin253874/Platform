// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// An entry in the call context for <see cref="CacheContext"/>.
    /// </summary>
    public class CacheContextEntry
    {
        /// <summary>
        /// Create a new <see cref="CacheContextEntry"/>.
        /// </summary>
        public CacheContextEntry()
        {
            Entities = new AddOnlySet<long>( );
            RelationshipTypes = new AddOnlySet<long>( );
            FieldTypes = new AddOnlySet<long>( );
            EntityInvalidatingRelationshipTypes = new AddOnlySet<long>( );
            EntityTypes = new AddOnlySet<long>( );
        }

        /// <summary>
        /// Entities that, when modified or deleted (including fields), 
        /// invalidate cache entries.
        /// </summary>
        public AddOnlySet<long> Entities { get; private set; }

        /// <summary>
        /// When relationships of this type from this entity are created, 
        /// modified or deleted, invalidate entries in the cache.
        /// </summary>
        public AddOnlySet<long> RelationshipTypes { get; private set; }

        /// <summary>
        /// When relationships of this type from this entity are created, 
        /// modified or deleted, invalidate entries in the cache.
        /// </summary>
        public AddOnlySet<long> FieldTypes { get; private set; }

        /// <summary>
        /// Follow these relationships to find entities in <see cref="Entities"/>.
        /// </summary>
        public AddOnlySet<long> EntityInvalidatingRelationshipTypes { get; private set; }

        /// <summary>
        /// Invalidate cache entries when entities of these types are saved.
        /// </summary>
        public AddOnlySet<long> EntityTypes { get; private set; }
    }
}