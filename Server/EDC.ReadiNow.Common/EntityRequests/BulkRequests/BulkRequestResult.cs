// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Contains the database results from running a BulkSqlQuery.
    /// Also contains a link back to the query, which has information necessary for decoding the results.
    /// </summary>
    internal class BulkRequestResult
    {
        public BulkRequestResult()
        {
            AllEntities = new Dictionary<long, EntityValue>();
            RootEntities = new HashSet<long>();
            RootEntitiesList = new List<long>( );
            FieldValues = new Dictionary<FieldKey, FieldValue>( );
            Relationships = new Dictionary<RelationshipKey, List<long>>();
        }

        /// <summary>
        /// The query that generated these results.
        /// </summary>
        public BulkSqlQuery BulkSqlQuery { get; set; }

        /// <summary>
        /// All entities .. referenced by tuple of EntityId/node id
        /// </summary>
        public Dictionary<long, EntityValue> AllEntities { get; private set; }

        /// <summary>
        /// Dictionary mapping from EntityId/NodeTag/FieldId to the data value.
        /// </summary>
        public Dictionary<FieldKey, FieldValue> FieldValues { get; private set; }

        /// <summary>
        /// Dictionary mapping from EntityId/NodeTag/RelTypeId to a list of the relationship entity/instance pairs.
        /// </summary>
        public Dictionary<RelationshipKey, List<long>> Relationships { get; private set; }

        /// <summary>
        /// A set copy of the root entities, and the nodes at which they start
        /// </summary>
        public HashSet<long> RootEntities { get; private set; }

        /// <summary>
        /// A list copy of the root entities, and the nodes at which they start
        /// </summary>
        public List<long> RootEntitiesList { get; private set; }
    }


    /// <summary>
    /// An entity/field key.
    /// </summary>
    internal class FieldKey : Tuple<long, long>
    {
        public FieldKey(long entityId, long fieldId)
            : base(entityId, fieldId) { }

        public long EntityId { get { return Item1; } }

        public long FieldId { get { return Item2; } }
    }


    /// <summary>
    /// An entity/field key.
    /// </summary>
    internal class FieldValue : Tuple<object, TypedValue>
    {
        public FieldValue( object rawValue, TypedValue typedValue )
            : base( rawValue, typedValue ) { }

        public object RawValue { get { return Item1; } }

        public TypedValue TypedValue { get { return Item2; } }
    }


    /// <summary>
    /// An entity/relationship/direction key.
    /// </summary>
    internal class RelationshipKey : Tuple<long, long>
    {
        public RelationshipKey(long entityId, long typeIdAndDirection)
            : base(entityId, typeIdAndDirection) { }

        public long EntityId { get { return Item1; } }

        // Reverse relationships are indicated with negative
        public long TypeIdAndDirection { get { return Item2; } }

        public long TypeId
        {
            get { return Math.Abs(TypeIdAndDirection); }
        }

        public Direction Direction
        {
            get { return TypeIdAndDirection < 0 ? Direction.Reverse : Direction.Forward; }
        }
    }

    /// <summary>
    /// Data to hold about a cached unsecured entity.
    /// </summary>
    internal class EntityValue
    {
        public EntityValue( )
        {
            Nodes = new TypicallySingleEntry<RequestNodeInfo>( );
        }

        /// <summary>
        /// This entity is implicitly secured
        /// </summary>
        public bool ImplicitlySecured { get; set; }

        /// <summary>
        /// The tags for the request nodes that provided content for this entity.
        /// </summary>
        public TypicallySingleEntry<RequestNodeInfo> Nodes { get; private set; }
    }
}
