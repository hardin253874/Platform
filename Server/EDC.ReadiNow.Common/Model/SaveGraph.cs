// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using EDC.Collections.Generic;
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.EventClasses;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     The save graph.
	/// </summary>
	internal class SaveGraph
	{
		/// <summary>
		///     The additions
		/// </summary>
		private Dictionary<string, DataTable> _additions;

		/// <summary>
		///     The current user
		/// </summary>
		private IEntity _currentUser;

		/// <summary>
		///     Whether the current user has been obtained.
		/// </summary>
		private bool _currentUserObtained;

		/// <summary>
		///     The deletions
		/// </summary>
		private Dictionary<string, DataTable> _deletions;

		/// <summary>
		///     The entity clones
		/// </summary>
		private Dictionary<CloneOption, DataTable> _entityClones;

		/// <summary>
		///     The field add key caches
		/// </summary>
		private Dictionary<string, HashSet<Tuple<long, long>>> _fieldAddKeyCaches;

		/// <summary>
		///     The field remove key caches
		/// </summary>
		private Dictionary<string, HashSet<Tuple<long, long>>> _fieldRemoveKeyCaches;

        /// <summary>
		///     Miscellaneous sql statements to run as part of the save.
		/// </summary>
        private readonly List<string> _miscSqlStatements = new List<string>();

		/// <summary>
		///     The relationship add keys
		/// </summary>
		private HashSet<Tuple<long, long, long>> _relationshipAddKeys;

		/// <summary>
		///     The relationship forward add keys
		/// </summary>
		private HashSet<Tuple<long, long>> _relationshipForwardAddKeys;

		/// <summary>
		///     The relationship forward remove keys
		/// </summary>
		private HashSet<Tuple<long, long>> _relationshipForwardRemoveKeys;

		/// <summary>
		///     The relationship remove keys
		/// </summary>
		private HashSet<Tuple<long, long, long>> _relationshipRemoveKeys;

		/// <summary>
		///     The relationship reverse add keys
		/// </summary>
		private HashSet<Tuple<long, long>> _relationshipReverseAddKeys;

		/// <summary>
		///     The relationship reverse remove keys
		/// </summary>
		private HashSet<Tuple<long, long>> _relationshipReverseRemoveKeys;        

        /// <summary>
        ///     The tenant identifier
        /// </summary>
        private long _tenantId = -1;

        /// <summary>
        /// The input entities table valued parameter.
        /// </summary>
        private DataTable _inputEntitiesTable;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveGraph" /> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     entities
        ///     or
        ///     context
        /// </exception>
        public SaveGraph( IEnumerable<IEntity> entities )
		{
			if ( entities == null )
			{
				throw new ArgumentNullException( "entities" );
			}

            Entities = new Dictionary<long, IEntity>();

            foreach (IEntity entity in entities)
            {
                Entities[entity.Id] = entity;
            }
            			
			AcceptedChangeTrackers = new List<IChangeTracker<IMutableIdKey>>( );

			CurrentUtcDate = DateTime.UtcNow;
			State = new Dictionary<string, object>( );
			Changes = new Dictionary<long, EntityChanges>( );			       
			Mapping = new BidirectionalDictionary<long, long>( );            

            EventTargetStateHelper.SetSaveGraph( State, this );
		}

		/// <summary>
		///     Gets the accepted change trackers.
		/// </summary>
		/// <value>
		///     The accepted change trackers.
		/// </value>
		public IList<IChangeTracker<IMutableIdKey>> AcceptedChangeTrackers
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the additions.
		/// </summary>
		/// <value>
		///     The additions.
		/// </value>
		private Dictionary<string, DataTable> Additions
		{
			get
			{
				return _additions ?? ( _additions = new Dictionary<string, DataTable>( ) );
			}
		}        

        /// <summary>
        ///     Gets the changes.
        /// </summary>
        /// <value>
        ///     The changes.
        /// </value>
        public Dictionary<long, EntityChanges> Changes
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the current user.
		/// </summary>
		/// <value>
		///     The current user.
		/// </value>
		public IEntity CurrentUser
		{
			get
			{
				if ( _currentUserObtained == false )
				{
					/////
					// Get current user
					/////
					long currentUserId = RequestContext.GetContext( ).Identity.Id;

					if ( currentUserId > 0 )
					{
						_currentUser = Entity.Get( currentUserId );
					}

					_currentUserObtained = true;
				}

				return _currentUser;
			}
		}

		/// <summary>
		///     Gets the current UTC date.
		/// </summary>
		/// <value>
		///     The current UTC date.
		/// </value>
		public DateTime CurrentUtcDate
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the deletions.
		/// </summary>
		/// <value>
		///     The deletions.
		/// </value>
		private Dictionary<string, DataTable> Deletions
		{
			get
			{
				return _deletions ?? ( _deletions = new Dictionary<string, DataTable>( ) );
			}
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <value>
		///     The entities.
		/// </value>
		public IDictionary<long, IEntity> Entities
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the misc commands.
		/// </summary>
		/// <value>
		///     The misc commands.
		/// </value>
		private Dictionary<CloneOption, DataTable> EntityClones
		{
			get
			{
				return _entityClones ?? ( _entityClones = new Dictionary<CloneOption, DataTable>( ) );
			}
		}

		/// <summary>
		///     Gets or sets the added field caches.
		/// </summary>
		/// <value>
		///     The field caches.
		/// </value>
		private Dictionary<string, HashSet<Tuple<long, long>>> FieldAddKeyCaches
		{
			get
			{
				return _fieldAddKeyCaches ?? ( _fieldAddKeyCaches = new Dictionary<string, HashSet<Tuple<long, long>>>( ) );
			}
		}

		/// <summary>
		///     Gets or sets the removed field caches.
		/// </summary>
		/// <value>
		///     The field caches.
		/// </value>
		private Dictionary<string, HashSet<Tuple<long, long>>> FieldRemoveKeyCaches
		{
			get
			{
				return _fieldRemoveKeyCaches ?? ( _fieldRemoveKeyCaches = new Dictionary<string, HashSet<Tuple<long, long>>>( ) );
			}
		}		      		

        /// <summary>
		///     Gets the mapping.
		/// </summary>
		/// <value>
		///     The mapping.
		/// </value>
		public BidirectionalDictionary<long, long> Mapping
		{
			get;
			private set;
		}        		

        /// <summary>
        ///     Gets the added relationship keys.
        /// </summary>
        /// <value>
        ///     The relationship keys.
        /// </value>
        private HashSet<Tuple<long, long, long>> RelationshipAddKeys
		{
			get
			{
				return _relationshipAddKeys ?? ( _relationshipAddKeys = new HashSet<Tuple<long, long, long>>( ) );
			}
		}

		/// <summary>
		///     Gets the added relationship keys.
		/// </summary>
		/// <value>
		///     The relationship keys.
		/// </value>
		private HashSet<Tuple<long, long>> RelationshipForwardAddKeys
		{
			get
			{
				return _relationshipForwardAddKeys ?? ( _relationshipForwardAddKeys = new HashSet<Tuple<long, long>>( ) );
			}
		}

		/// <summary>
		///     Gets the removed relationship keys.
		/// </summary>
		/// <value>
		///     The relationship keys.
		/// </value>
		private HashSet<Tuple<long, long>> RelationshipForwardRemoveKeys
		{
			get
			{
				return _relationshipForwardRemoveKeys ?? ( _relationshipForwardRemoveKeys = new HashSet<Tuple<long, long>>( ) );
			}
		}

		/// <summary>
		///     Gets the removed relationship keys.
		/// </summary>
		/// <value>
		///     The relationship keys.
		/// </value>
		private HashSet<Tuple<long, long, long>> RelationshipRemoveKeys
		{
			get
			{
				return _relationshipRemoveKeys ?? ( _relationshipRemoveKeys = new HashSet<Tuple<long, long, long>>( ) );
			}
		}

		/// <summary>
		///     Gets the added relationship keys.
		/// </summary>
		/// <value>
		///     The relationship keys.
		/// </value>
		private HashSet<Tuple<long, long>> RelationshipReverseAddKeys
		{
			get
			{
				return _relationshipReverseAddKeys ?? ( _relationshipReverseAddKeys = new HashSet<Tuple<long, long>>( ) );
			}
		}

		/// <summary>
		///     Gets the removed relationship keys.
		/// </summary>
		/// <value>
		///     The relationship keys.
		/// </value>
		private HashSet<Tuple<long, long>> RelationshipReverseRemoveKeys
		{
			get
			{
				return _relationshipReverseRemoveKeys ?? ( _relationshipReverseRemoveKeys = new HashSet<Tuple<long, long>>( ) );
			}
		}

		/// <summary>
		///     Gets the state.
		/// </summary>
		/// <value>
		///     The state.
		/// </value>
		public Dictionary<string, object> State
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		private long TenantId
		{
			get
			{
				if ( _tenantId == -1 )
				{
					_tenantId = RequestContext.TenantId;
				}

				return _tenantId;
			}
		}

		/// <summary>
		///     Adds the deep clone.
		/// </summary>
		/// <param name="sourceId">The source identifier.</param>
		/// <param name="destinationId">The destination identifier.</param>
		public void AddDeepClone( long sourceId, long destinationId )
		{
			DataTable dt;

			if ( !EntityClones.TryGetValue( CloneOption.Deep, out dt ) )
			{
				dt = TableValuedParameter.Create( TableValuedParameterType.EntityMap );
				EntityClones[ CloneOption.Deep ] = dt;
			}

			dt.Rows.Add( sourceId, destinationId );
		}

		/// <summary>
		///     Adds the field.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		/// <param name="nameSpace">The name space.</param>
		/// <param name="direction">The direction.</param>
		public void AddField( string tableName, long entityId, long fieldId, object data, string nameSpace, int? direction )
		{			
			var tableValuedParameterType = TableValuedParameter.ConvertTableNameToTableValuedParameterType( tableName );

			DataTable dt = GetAdditions( tableName, tableValuedParameterType );

			HashSet<Tuple<long, long>> fieldCache = GetFieldAddCache( tableName );

			var key = Tuple.Create( entityId, fieldId );

			if ( !fieldCache.Contains( key ) )
			{
				fieldCache.Add( key );

				if ( direction != null )
				{
					dt.Rows.Add( entityId, RequestContext.TenantId, fieldId, ( string ) data, nameSpace, direction.Value );
				}
				else
				{
					dt.Rows.Add( entityId, RequestContext.TenantId, fieldId, data );
				}
			}
		}

		/// <summary>
		///     Adds the metadata.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="isCreate">
		///     if set to <c>true</c> [is create].
		/// </param>
		public void AddMetadata( IEntity entity, bool isCreate )
		{
		    WellKnownAliases aliases = WellKnownAliases.CurrentTenant;

            /////
            // Set modified time
            /////
            AddField( "Data_DateTime", entity.Id, aliases.ModifiedDate, CurrentUtcDate, null, null );

			/////
			// Check for creation
			/////
			if ( isCreate )
			{
				/////
				// Set create time
				/////
				AddField( "Data_DateTime", entity.Id, aliases.CreatedDate, CurrentUtcDate, null, null );
			}

			if ( CurrentUser != null )
			{
				IEntityInternal entityInternal = entity as IEntityInternal;

				if ( !isCreate || ( entityInternal?.CloneSource != null ) )
				{
					ClearRelationship( entity.Id, aliases.LastModifiedBy, Direction.Forward );
				}

				AddRelationship( entity.Id, aliases.LastModifiedBy, CurrentUser.Id, Direction.Forward, true );

				/////
				// Check for creation
				/////
				if ( isCreate )
				{
					if ( entityInternal?.CloneSource != null )
					{
						ClearRelationship( entity.Id, aliases.SecurityOwner, Direction.Forward );
						ClearRelationship( entity.Id, aliases.CreatedBy, Direction.Forward );
					}

					/////
					// Set security owner user
					/////
					AddRelationship( entity.Id, aliases.SecurityOwner, CurrentUser.Id, Direction.Forward, true );

					/////
					// Set created-by user
					/////
					AddRelationship( entity.Id, aliases.CreatedBy, CurrentUser.Id, Direction.Forward, true );
				}
			}
		}

        /// <summary>
        /// Adds a miscellaneous sql statement to run as part of the save
        /// </summary>
        /// <param name="sql"></param>
        public void AddMiscellaneousSqlStatement(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            _miscSqlStatements.Add(sql);
        }

		/// <summary>
		///     Adds the relationship.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="relationshipType">Type of the relationship.</param>
		/// <param name="destinationId">The destination identifier.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="isLookup">if set to <c>true</c> [is lookup].</param>
		public void AddRelationship( long entityId, long relationshipType, long destinationId, Direction direction, bool isLookup = false )
		{			
			DataTable dt = GetAdditions( "Relationship", TableValuedParameterType.Relationship );

			long fromId;
			long toId;
			HashSet<Tuple<long, long>> cache;

			if ( direction == Direction.Forward )
			{
				fromId = entityId;
				toId = destinationId;
				cache = RelationshipForwardAddKeys;
			}
			else
			{
				fromId = destinationId;
				toId = entityId;
				cache = RelationshipReverseAddKeys;
			}

			var key = Tuple.Create( relationshipType, fromId, toId );
			var directionKey = Tuple.Create( relationshipType, fromId );

			if ( RelationshipAddKeys.Contains( key ) || ( isLookup && cache.Contains( directionKey ) ) )
			{
				return;
			}

			dt.Rows.Add( TenantId, relationshipType, fromId, toId );

			RelationshipAddKeys.Add( key );
			cache.Add( directionKey );
		}

		/// <summary>
		///     Adds the shallow clone.
		/// </summary>
		/// <param name="sourceId">The source identifier.</param>
		/// <param name="destinationId">The destination identifier.</param>
		public void AddShallowClone( long sourceId, long destinationId )
		{
			DataTable dt;

			if ( ! EntityClones.TryGetValue( CloneOption.Shallow, out dt ) )
			{
				dt = TableValuedParameter.Create( TableValuedParameterType.EntityMap );
				EntityClones[ CloneOption.Shallow ] = dt;
			}

			dt.Rows.Add( sourceId, destinationId );
		}

		/// <summary>
		///     Clears the relationship.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="relationshipType">Type of the relationship.</param>
		/// <param name="direction">The direction.</param>
		public void ClearRelationship( long entityId, long relationshipType, Direction direction )
		{			
			HashSet<Tuple<long, long>> cache;
			string typeName;
			TableValuedParameterType tvpType;

			if ( direction == Direction.Forward )
			{
				cache = RelationshipForwardRemoveKeys;
				typeName = "ForwardRelationship";
				tvpType = TableValuedParameterType.LookupRelationshipForward;
			}
			else
			{
				cache = RelationshipReverseRemoveKeys;
				typeName = "ReverseRelationship";
				tvpType = TableValuedParameterType.LookupRelationshipReverse;
			}

			var key = Tuple.Create( relationshipType, entityId );

			if ( cache.Contains( key ) )
			{
				return;
			}

			DataTable dt = GetDeletions( typeName, tvpType );

			dt.Rows.Add( RequestContext.TenantId, relationshipType, entityId );

			cache.Add( key );
		}

		/// <summary>
		///     Gets the specified table name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="cache">The cache.</param>
		/// <param name="onCreate">The on create.</param>
		/// <returns></returns>
		private static T Get<T>( string tableName, Dictionary<string, T> cache, Func<string, T> onCreate ) where T : class, new( )
		{
			if ( string.IsNullOrEmpty( tableName ) || cache == null )
			{
				return default( T );
			}

			T value;

			if ( !cache.TryGetValue( tableName, out value ) )
			{
				value = onCreate( tableName );
				cache[ tableName ] = value;
			}

			return value;
		}

		/// <summary>
		///     Gets the additions.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="tvpType">Type of the TVP.</param>
		/// <returns></returns>
		public DataTable GetAdditions( string tableName, TableValuedParameterType tvpType )
		{
			return Get( tableName, Additions, name => TableValuedParameter.Create( tvpType ) );
		}

		/// <summary>
		///     Gets the deletions.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="tvpType">Type of the TVP.</param>
		/// <returns></returns>
		public DataTable GetDeletions( string tableName, TableValuedParameterType tvpType )
		{
			return Get( tableName, Deletions, name => TableValuedParameter.Create( tvpType ) );
		}

		/// <summary>
		///     Gets the field added key cache.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns></returns>
		private HashSet<Tuple<long, long>> GetFieldAddCache( string tableName )
		{
			if ( string.IsNullOrEmpty( tableName ) )
			{
				return null;
			}

			HashSet<Tuple<long, long>> value;

			if ( !FieldAddKeyCaches.TryGetValue( tableName, out value ) )
			{
				value = new HashSet<Tuple<long, long>>( );
				FieldAddKeyCaches[ tableName ] = value;
			}

			return value;
		}

		/// <summary>
		///     Gets the field removed key cache.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns></returns>
		private HashSet<Tuple<long, long>> GetFieldRemoveCache( string tableName )
		{
			if ( string.IsNullOrEmpty( tableName ) )
			{
				return null;
			}

			HashSet<Tuple<long, long>> value;

			if ( !FieldRemoveKeyCaches.TryGetValue( tableName, out value ) )
			{
				value = new HashSet<Tuple<long, long>>( );
				FieldRemoveKeyCaches[ tableName ] = value;
			}

			return value;
		}

		/// <summary>
		///     Gets the name of the type.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">Unknown type  + key</exception>
		private string GetTypeName( string key )
		{
			switch ( key )
			{
				case "Data_Alias":
					return "Alias";
				case "Data_Bit":
					return "Bit";
				case "Data_DateTime":
					return "DateTime";
				case "Data_Decimal":
					return "Decimal";
				case "Data_Guid":
					return "Guid";
				case "Data_Int":
					return "Int";
				case "Data_NVarChar":
					return "NVarChar";
				case "Data_Xml":
					return "Xml";
				case "Relationship":
					return "Relationship";
				case "ForwardRelationship":
					return "RelationshipForward";
				case "ReverseRelationship":
					return "RelationshipReverse";
				default:
					throw new InvalidOperationException( "Unknown type " + key );
			}
		}

		/// <summary>
		///     Adds any addition parameters.
		/// </summary>
		/// <param name="command">The command.</param>
		private bool AddAdditionParameters( IDbCommand command )
		{
            bool haveAdditions = Additions.Count != 0;

		    if (haveAdditions)
		    {
                foreach (var pair in Additions.OrderBy(kvp => kvp.Key))
                {
                    string typeName = GetTypeName(pair.Key);

                    command.AddTableValuedParameter(string.Format("@merge{0}", typeName), pair.Value);
                }    
		    }			

            return haveAdditions;
        }

        /// <summary>
		///     Adds the miscellaneous sql statement parameter.
		/// </summary>
        /// <param name="command">The command.</param>		
		private bool AddMiscSqlStatementParameter(IDbCommand command)
        {
            bool haveMiscSql = _miscSqlStatements.Count != 0;

            if (haveMiscSql)
            {
                var miscSqlBuilder = new StringBuilder();
                foreach (string sql in _miscSqlStatements)
                {
                    miscSqlBuilder.AppendLine(sql);
                }

                command.AddParameterWithValue("@miscSql", miscSqlBuilder.ToString());                
            }

            return haveMiscSql;
        }

        /// <summary>
        ///     Adds any deletion parameters.
        /// </summary>
        /// <param name="command">The command.</param>
        private bool AddDeletionParameters( IDbCommand command)
		{
            bool haveDeletions = Deletions.Count != 0;

            if (haveDeletions)
            {
                foreach (var pair in Deletions.OrderBy(kvp => kvp.Key))
                {
                    string typeName = GetTypeName(pair.Key);

                    command.AddTableValuedParameter(string.Format("@delete{0}", typeName), pair.Value);                    
                }    
            }			

            return haveDeletions;
		}

		/// <summary>
		///     Adds any entity clone parameters.
		/// </summary>		
		/// <param name="command">The command.</param>
		private bool AddEntityClonesParameters( IDbCommand command )
		{
            bool haveClones = false;
             
			if ( EntityClones.Count > 0 )
			{
				DataTable dt;
				if ( EntityClones.TryGetValue( CloneOption.Shallow, out dt ) )
				{                                        
                    command.AddTableValuedParameter("@shallow", dt);
                    haveClones = true;
                }

				if ( EntityClones.TryGetValue( CloneOption.Deep, out dt ) )
				{                                        
                    command.AddTableValuedParameter("@deep", dt);
                    haveClones = true;
                }				
			}

            return haveClones;
		}

		/// <summary>
		///     Removes the field.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		public void RemoveField( string tableName, long entityId, long fieldId )
		{			
			var key = Tuple.Create( entityId, fieldId );

			var cache = GetFieldRemoveCache( tableName );

			if ( cache.Contains( key ) )
			{
				return;
			}

			DataTable dt = GetDeletions( tableName, TableValuedParameterType.LookupFieldKey );

			dt.Rows.Add( entityId, RequestContext.TenantId, fieldId );

			cache.Add( key );
		}

		/// <summary>
		///     Removes the relationship.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="relationshipType">Type of the relationship.</param>
		/// <param name="destinationId">The destination identifier.</param>
		/// <param name="direction">The direction.</param>
		public void RemoveRelationship( long entityId, long relationshipType, long destinationId, Direction direction )
		{			
			long fromId;
			long toId;

			if ( direction == Direction.Forward )
			{
				fromId = entityId;
				toId = destinationId;
			}
			else
			{
				fromId = destinationId;
				toId = entityId;
			}

			var key = Tuple.Create( relationshipType, fromId, toId );

			if ( RelationshipRemoveKeys.Contains( key ) )
			{
				return;
			}

			DataTable dt = GetDeletions( "Relationship", TableValuedParameterType.Relationship );

			dt.Rows.Add( RequestContext.TenantId, relationshipType, fromId, toId );

			RelationshipRemoveKeys.Add( key );
		}


        /// <summary>
        ///     Saves the specified CTX.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>
        ///     A mapping of the old entity IDs to new entity IDs. If no cloning occurred,
        ///     this returns an empty dictionary.
        /// </returns>
        public IDictionary<long, long> Save(DatabaseContext context)
		{
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IDictionary<long, long> clonedIds = new Dictionary<long, long>( );

			using ( DatabaseContextInfo dbContextInfo = DatabaseContextInfo.SetContextInfo( "Save" ) )
			using ( IDbCommand command = context.CreateCommand( ) )
			{
                command.CommandText = "dbo.spSaveGraphSave";
                command.CommandType = CommandType.StoredProcedure;
                command.AddParameter("@tenantId", DbType.Int64, RequestContext.TenantId);                
                
                PopulateInputEntitiesTable();

				long userId;
				RequestContext.TryGetUserId( out userId );

				command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );
				var transactionId = command.AddParameter( "@transactionId", DbType.Int64 );
				transactionId.Direction = ParameterDirection.Output;

				bool haveInputEntities = AddInputEntitiesParameter( command );
                bool haveClones = AddEntityClonesParameters( command );
				bool haveDeletions = AddDeletionParameters( command );
				bool haveAdditions = AddAdditionParameters( command );
                bool haveMiscStatements = AddMiscSqlStatementParameter(command);
                bool haveChanges = haveInputEntities || haveClones || haveDeletions || haveAdditions || haveMiscStatements;

                if (!haveChanges)
                {
                    return clonedIds;
                }
                
                using (IDataReader reader = command.ExecuteReader( ) )
				{
					/////
					// Read mapping between temporary ids and persisted ids.
					/////
                    while (reader.Read())
                    {
                        long oldId = reader.GetInt64(0);
                        long newId = reader.GetInt64(1);

                        Mapping[oldId] = newId;
                    }

					/////
					// If there were any clone operations, a separate result set containing the source
					// and destination ids will be returned
					/////
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            clonedIds[reader.GetInt64(0)] = reader.GetInt64(1);
                        }
                    }

					/////
					// Check for exceptions that occur AFTER the last expected result set is returned.
					/////
					if ( reader.NextResult( ) )
					{
						/////
						// Exceptions thrown from Sql Server will appear as a separate result set when
						// accessed using ExecuteReader. This means all results sets that occur before
						// the failure condition will still be successfully returned. Must check for
						// exceptions that occur AFTER the last EXPECTED result set has been returned.
						/////
					}
				}

				if ( transactionId.Value != null && transactionId.Value != DBNull.Value )
				{
					dbContextInfo.TransactionId = ( long ) transactionId.Value;
				}
			}

			return clonedIds;
		}        

        /// <summary>
        /// Add the input entities parameter
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private bool AddInputEntitiesParameter(IDbCommand command)
        {
            if (_inputEntitiesTable == null || _inputEntitiesTable.Rows.Count == 0)
            {
                return false;
            }

            command.AddTableValuedParameter("@inputEntities", _inputEntitiesTable);

            return true;
        }                

        /// <summary>
        /// Populate the input entities datatable.
        /// </summary>
        private void PopulateInputEntitiesTable()
        {
            if (Entities == null || Entities.Count == 0)
            {
                return;
            }

            _inputEntitiesTable = TableValuedParameter.Create(TableValuedParameterType.InputEntityType);
            
            foreach (IEntity entity in Entities.Values)
            {
                if (entity.IsReadOnly)
                {
                    Mapping[entity.Id] = entity.Id;
                    continue;
                }

                var entityInternal = entity as IEntityInternal;

                if (entityInternal == null) continue;

                if (entityInternal.IsTemporaryId)
                {
                    bool foundType = false;

                    int isClone = entityInternal.CloneSource == null ? 0 : 1;

                    foreach (long typeId in entity.TypeIds)
                    {
                        _inputEntitiesTable.Rows.Add(entity.Id, typeId, isClone);
                        foundType = true;

                    }

                    if (!foundType)
                    {
                        _inputEntitiesTable.Rows.Add(entity.Id, -1L, isClone);
                    }
                }
                else
                {
                    Mapping[entity.Id] = entity.Id;
                }
            }
        }

        /// <summary>
        ///     Entity Changes
        /// </summary>
        public class EntityChanges
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="EntityChanges" /> class.
			/// </summary>
			/// <param name="fields">The fields.</param>
			/// <param name="fieldsChanged">
			///     if set to <c>true</c> [fields changed].
			/// </param>
			/// <param name="forwardRelationships">The forward relationships.</param>
			/// <param name="forwardRelationshipsChanged">
			///     if set to <c>true</c> [forward relationships changed].
			/// </param>
			/// <param name="reverseRelationships">The reverse relationships.</param>
			/// <param name="reverseRelationshipsChanged">
			///     if set to <c>true</c> [reverse relationships changed].
			/// </param>
			/// <exception cref="System.ArgumentNullException">
			///     fields
			///     or
			///     forwardRelationships
			///     or
			///     reverseRelationships
			/// </exception>
			public EntityChanges( IEntityFieldValues fields, bool fieldsChanged, IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships, bool forwardRelationshipsChanged, IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships, bool reverseRelationshipsChanged )
			{
				Fields = fields;
				ForwardRelationships = forwardRelationships;
				ReverseRelationships = reverseRelationships;

				FieldsChanged = fieldsChanged;
				ForwardRelationshipsChanged = forwardRelationshipsChanged;
				ReverseRelationshipsChanged = reverseRelationshipsChanged;
			}

			/// <summary>
			///     Gets the fields.
			/// </summary>
			/// <value>
			///     The fields.
			/// </value>
			public IEntityFieldValues Fields
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets a value indicating whether the fields have changed.
			/// </summary>
			/// <value>
			///     <c>true</c> if the fields have changed; otherwise, <c>false</c>.
			/// </value>
			public bool FieldsChanged
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets the forward relationships.
			/// </summary>
			/// <value>
			///     The forward relationships.
			/// </value>
			public IDictionary<long, IChangeTracker<IMutableIdKey>> ForwardRelationships
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets a value indicating whether the forward relationships have changed.
			/// </summary>
			/// <value>
			///     <c>true</c> if the forward relationships have changed; otherwise, <c>false</c>.
			/// </value>
			public bool ForwardRelationshipsChanged
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets a value indicating whether this instance has changes.
			/// </summary>
			/// <value>
			///     <c>true</c> if this instance has changes; otherwise, <c>false</c>.
			/// </value>
			public bool HasChanges
			{
				get
				{
					return FieldsChanged || ForwardRelationshipsChanged || ReverseRelationshipsChanged;
				}
			}

			/// <summary>
			///     Gets the reverse relationships.
			/// </summary>
			/// <value>
			///     The reverse relationships.
			/// </value>
			public IDictionary<long, IChangeTracker<IMutableIdKey>> ReverseRelationships
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets a value indicating whether the reverse relationships have changed.
			/// </summary>
			/// <value>
			///     <c>true</c> if the reverse relationships have changed; otherwise, <c>false</c>.
			/// </value>
			public bool ReverseRelationshipsChanged
			{
				get;
				private set;
			}
		}
	}
}