// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Common;
using EDC.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// BulkRequestSqlBuilder supports BulkRequestRunner.
    /// Takes an EntityMemberRequest and builds a SQL statement for loading all of the data from SQL server.
    /// Note: If the same relationships/fields are requested via different query nodes then they get returned twice.
    /// This is intentional as it means that when security gets later applied, we know what rels/fields to return, depending on what paths
    /// can or can't be accessed.
    /// </summary>
    class BulkRequestSqlBuilder
    {                
        /// <summary>
        /// Lookup from request-nodes to any temp internal information we want to store about those nodes.
        /// </summary>
        private readonly Dictionary<EntityMemberRequest, RequestNodeInfo> _requestNodeMap =
            new Dictionary<EntityMemberRequest, RequestNodeInfo>( );

        /// <summary>
        /// The root request object (i.e. the input to this class).
        /// </summary>
        private EntityMemberRequest _request;

        /// <summary>
        /// The root query object (i.e. the output of this class).
        /// </summary>
        private BulkSqlQuery _result;

        /// <summary>
        /// Hide the constructor.
        /// </summary>
        private BulkRequestSqlBuilder( )
        {
        }


        /// <summary>
        /// Build the overall T-SQL query.
        /// </summary>
        /// <param name="request">The root level entity member request.</param>
        /// <param name="hintText">A hint about what this query is doing. Use for logging/diagnostics only.</param>
        public static BulkSqlQuery BuildSql( EntityMemberRequest request, string hintText = null )
        {
            if ( request == null )
                throw new ArgumentNullException( "request" );

            var builder = new BulkRequestSqlBuilder
            {
	            _request = request,
            };

	        builder.BuildSql( );

            return builder._result;
        }


        /// <summary>
        /// Implementation for building the overall T-SQL query.
        /// </summary>
        private void BuildSql( )
        {
            _result = new BulkSqlQuery
            {
	            Request = _request
            };	        

            // Walk the request object graph to get individual nodes
            var allRequestNodes = Delegates.WalkGraph( _request, rq => rq.Relationships.Select( r => r.RequestedMembers ) );

            // Build info for each node
            int nextTag = 0;
            foreach ( EntityMemberRequest node in allRequestNodes )
            {
                var nodeInfo = new RequestNodeInfo
                {
                    Request = node,
                    Tag = nextTag++
                };
                _requestNodeMap.Add( node, nodeInfo );
                _result.RequestNodes.Add( nodeInfo.Tag, nodeInfo );
            }

            ProcessRelationships( );
            ProcessFields( );
        }


        /// <summary>
        /// Maps field IDs to the database tables that they get stored in.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        private FieldInfo RegisterFieldInfo(IEntityRef fieldId)
        {
            // Get/convert the type info for the field (and store for later, since we're already here)
            FieldInfo fieldInfo;
            if (!_result.FieldTypes.TryGetValue(fieldId.Id, out fieldInfo))
            {
                // Get the field
                Field field = Entity.Get<Field>(fieldId.Id);

                fieldInfo = new FieldInfo();
                _result.FieldTypes.Add(fieldId.Id, fieldInfo);

                fieldInfo.DatabaseType = field.ConvertToDatabaseType();
                fieldInfo.IsWriteOnly = field.IsFieldWriteOnly ?? false;
                fieldInfo.IsCalculated = Factory.CalculatedFieldMetadataProvider.IsCalculatedField(fieldId.Id);
                fieldInfo.IsVirtualAccessControlField = BulkRequestHelper.IsVirtualAccessControlField(new EntityRef(fieldId));

                FieldType fieldType = field.GetFieldType();
                fieldInfo.DatabaseTable = string.Intern( fieldType.DbFieldTable );  // intern to avoid lots of copies of the same database table names in memory
            }

            return fieldInfo;
        }


        /// <summary>
        /// Maps field IDs to the database tables that they get stored in.
        /// </summary>
        /// <param name="tagField"></param>
        /// <returns></returns>
        private string MapTagAndFieldPairToTableName( Tuple<int, long> tagField )
        {
            long fieldId = tagField.Item2;
            FieldInfo fieldInfo = _result.FieldTypes[fieldId];
            return fieldInfo.DatabaseTable;
        }


        /// <summary>
        /// Prepare the SQL to load in all values of all fields in the entity request.
        /// </summary>
        private void ProcessFields( )
        {
	        ISet<IEntityRef> fields = new HashSet<IEntityRef>( );

	        foreach ( var v in _requestNodeMap.Values )
	        {
		        fields.UnionWith( v.Request.Fields );
	        }

            // Cache whether the fields are virtual
	        BulkRequestHelper.IsVirtualField( fields );

	        // Creates a lookup, that for each type of table contains a list of tuples,
            // with each tuple matching node tags to fields that need to be loaded for that node tag.
            ILookup<string, Tuple<int, long>> fieldMap =
                ( from nodeInfo in _requestNodeMap.Values
                  from field in nodeInfo.Request.Fields
                  let fieldInfo = RegisterFieldInfo(field)
                  where !(fieldInfo.IsVirtualAccessControlField || fieldInfo.IsCalculated)
                  select new Tuple<int, long>( nodeInfo.Tag, field.Id )
                ).ToLookup(MapTagAndFieldPairToTableName);

            // Create sql
            foreach ( var list in fieldMap )
            {
                string tableName = list.Key;

                // Frustrate injection attack.
                if ( !( tableName.StartsWith( "Data_" ) && tableName.Length < 15 ) )
                {
                    throw new InvalidOperationException( "Field type table name was invalid: " + tableName );
                }

				DataTable dt = TableValuedParameter.Create( TableValuedParameterType.BulkFldType );
	            var map = new HashSet<Tuple<int, long>>( );

	            foreach ( Tuple<int, long> entry in list )
	            {
		            if ( map.Contains( entry ) )
		            {
			            continue;
		            }

		            map.Add( entry );
		            dt.Rows.Add( entry.Item1, entry.Item2 );
	            }

	            string tvpName = "@fld" + tableName;

				_result.TableValuedParameters [ tvpName ] = dt;	            
            }
        }


        /// <summary>
        /// Generate SQL for recursively following relationships in either direction.
        /// </summary>
        private void ProcessRelationships( )
        {
			if ( _request.Relationships.Count > 0 )
            {                
                ProcessRelationships( Direction.Forward );
                ProcessRelationships( Direction.Reverse );                
            }            
        }


        /// <summary>
        /// Prepare the SQL to load in relationships of all applicable types, in one direction.
        /// </summary>
        private void ProcessRelationships( Direction relDirection )
        {
            // Creates a list of relationship transition rules, where each tuple contains (in order):
            // 1. the node that the relationship instruction applies to
            // 2. the relationship ID
            // 3. the node that the relationship points to

            var relRequests =
                from nodeInfo in _requestNodeMap.Values
                from relReq in nodeInfo.Request.Relationships
                where Entity.GetDirection( relReq.RelationshipTypeId, relReq.IsReverse ) == relDirection
                select new { NodeInfo = nodeInfo, RelRequest = relReq };

	        var dt = TableValuedParameter.Create( TableValuedParameterType.BulkRelType );

	        //var relMap = new List<Tuple<int, long, int>>( );
	        var map = new HashSet<Tuple<int, long, int>>( );

            foreach ( var relReqInfo in relRequests )
            {
                if ( relReqInfo.RelRequest.MetadataOnly )
                    continue; // Don't load actual data

                int nodeId = relReqInfo.NodeInfo.Tag;
                var relReq = relReqInfo.RelRequest;
                var mapRule = new Tuple<int, long, int>( nodeId, relReq.RelationshipTypeId.Id,
                                                        _requestNodeMap [ relReq.RequestedMembers ].Tag );

	            if ( !map.Contains( mapRule ) )
	            {
		            map.Add( mapRule );

					dt.Rows.Add( mapRule.Item1, mapRule.Item2, mapRule.Item3 );
	            }

                if ( relReq.IsRecursive )
                {
                    var recursiveRule = new Tuple<int, long, int>( nodeId, relReq.RelationshipTypeId.Id, nodeId );

	                if ( ! map.Contains( recursiveRule ) )
	                {
		                map.Add( recursiveRule );

						dt.Rows.Add( recursiveRule.Item1, recursiveRule.Item2, recursiveRule.Item3 );
	                }
                }
            }

			if ( map.Count == 0 )
                return;

			string tblName = relDirection == Direction.Forward ? "@relFwd" : "@relRev";

			_result.TableValuedParameters [ tblName ] = dt;                        

            // Gather metadata for relationship requests
            var relReqs = from node in _request.WalkNodes( )
                          from relReq in node.Relationships
                          select relReq;

            foreach ( var relReq in relReqs )
            {
                var relTypeId = relReq.RelationshipTypeId.Id;
                var relEntity = Entity.Get<Relationship>( relTypeId );
                if ( relEntity == null )
                    continue;

                long key = relTypeId;
                var direction = Entity.GetDirection( relReq.RelationshipTypeId, relReq.IsReverse );
                if ( direction == Direction.Reverse )
                    key = -key;

                _result.Relationships [ key ] = new RelationshipInfo
                {
                    CloneAction = relEntity.CloneAction_Enum ?? CloneActionEnum_Enumeration.Drop,
                    ReverseCloneAction = relEntity.ReverseCloneAction_Enum ?? CloneActionEnum_Enumeration.Drop,
                    IsLookup = relEntity.IsLookup( direction ),
                    ImpliesSecurity = ( direction == Direction.Forward ? relEntity.SecuresTo : relEntity.SecuresFrom ) ?? false
                        || ( relReq.RelationshipTypeId.Alias == "isOfType" && relReq.RelationshipTypeId.Namespace == "core" && !relReq.IsReverse ) // Allow types to be loaded implicitly, without granting write to the type.
                };
            }

        }
    }



}
