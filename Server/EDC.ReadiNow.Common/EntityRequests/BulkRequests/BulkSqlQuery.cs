// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Represents a pre-prepared Bulk SQL query that can fetch a graph of entities in one call.
    /// Contains the SQL string, as well as various lookups that are required to interpret the results.
    /// And anything else that we can precalculate and cache prior to running the main query.
    /// </summary>
    internal class BulkSqlQuery
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal BulkSqlQuery()
        {
            RequestNodes = new Dictionary<int, RequestNodeInfo>();
            RequestNodesRev = new Dictionary<RequestNodeInfo, int>();
            FieldTypes = new Dictionary<long, FieldInfo>();
            Relationships = new Dictionary<long, RelationshipInfo>();
            TableValuedParameters = new Dictionary<string, DataTable>( );
        }


        /// <summary>
        /// The (root) entity member request that was used to compile this query.
        /// </summary>
        internal EntityMemberRequest Request { get; set; }

        /// <summary>
        /// Map of NodeIDs that got allocated within the generated SQL, to node information objects.
        /// </summary>
        internal Dictionary<int, RequestNodeInfo> RequestNodes { get; private set; }

        /// <summary>
        /// Reverse map of NodeIDs and Nodes.
        /// </summary>
        internal Dictionary<RequestNodeInfo, int> RequestNodesRev { get; private set; }

        /// <summary>
        /// Lookup of the database type for each field ID used in the query.
        /// </summary>
        internal Dictionary<long, FieldInfo> FieldTypes { get; private set; }

        /// <summary>
        /// Relationship typeID (negated for reverse) to info.
        /// </summary>
        internal Dictionary<long, RelationshipInfo> Relationships { get; private set; }

        /// <summary>
        /// The table valued parameters
        /// </summary>
        internal Dictionary<string, DataTable> TableValuedParameters;                	
    }


    /// <summary>
    /// A node in an entity member request.
    /// </summary>
    internal class RequestNodeInfo
    {
        /// <summary>
        /// The fields and relationships requested from this node.
        /// </summary>
        public EntityMemberRequest Request { get; set; }

        /// <summary>
        /// The temporary ID assigned to the node.
        /// </summary>
        public int Tag { get; set; }
    }


    /// <summary>
    /// Information about a relationship.
    /// </summary>
    internal class RelationshipInfo
    {
        public bool IsLookup { get; set; }

        public bool ImpliesSecurity { get; set; }

        public CloneActionEnum_Enumeration CloneAction { get; set; }

        public CloneActionEnum_Enumeration ReverseCloneAction { get; set; }
    }


    /// <summary>
    /// Information about a field.
    /// </summary>
    internal class FieldInfo
    {
        public DatabaseType DatabaseType { get; set; }

        public string DatabaseTable { get; set; }

        public bool IsWriteOnly { get; set; }

        public bool IsCalculated { get; set; }

        public bool IsVirtualAccessControlField { get; set; }
    }
}
