// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Storage;

namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Represents a reader for loading the application definition from within a tenant.
	/// </summary>
	internal class TenantAppStagingSource : SqlBase, IDataSource
	{
		/// <summary>
		///     Cached entity field data.
		/// </summary>
		private readonly Dictionary<string, List<DataEntry>> _entityDataCache = new Dictionary<string, List<DataEntry>>( );

		/// <summary>
		///     Cached binary data.
		/// </summary>
		private List<BinaryDataEntry> _binaryCache;

		/// <summary>
		///     The document cache
		/// </summary>
		private List<DocumentDataEntry> _documentCache;

		/// <summary>
		///     Cached entity data.
		/// </summary>
		private List<EntityEntry> _entityCache;

		/// <summary>
		///     Cached relationship data.
		/// </summary>
		private List<RelationshipEntry> _relationshipCache;

		/// <summary>
		///     Load entities and data that belong to this application only.
		/// </summary>
		/// <value>
		///     The solution id.
		/// </value>
		public long SolutionId
		{
			get;
			set;
		}

		/// <summary>
		///     The tenant to load the data from.
		/// </summary>
		/// <value>
		///     The tenant id.
		/// </value>
		public long TenantId
		{
			get;
			set;
		}

		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		IEnumerable<BinaryDataEntry> IDataSource.GetBinaryData( IProcessingContext context )
		{
			if ( _binaryCache == null )
			{
				var data = new List<BinaryDataEntry>( );
                Func<string, byte[]> loadBinaryData = token => FileRepositoryUtils.LoadFileData(Factory.BinaryFileRepository, token, context);

                using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "spGetTenantAppBinaryFileData";
					command.AddParameterWithValue( "@solutionId", SolutionId );
					command.AddParameterWithValue( "@tenant", TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var binaryDataEntry = new BinaryDataEntry
							{
								DataHash = reader.GetString( 0 ),								
								LoadDataCallback = loadBinaryData
                            };

							data.Add( binaryDataEntry );
						}
					}
				}

				_binaryCache = data;
			}

			return _binaryCache;
		}

		public IEnumerable<DocumentDataEntry> GetDocumentData( IProcessingContext context )
		{
			if ( _documentCache == null )
			{
				var data = new List<DocumentDataEntry>( );
                Func<string, byte[]> loadDocumentData = token => FileRepositoryUtils.LoadFileData(Factory.DocumentFileRepository, token, context);

                using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "spGetTenantAppDocumentFileData";
					command.AddParameterWithValue( "@solutionId", SolutionId );
					command.AddParameterWithValue( "@tenant", TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var binaryDataEntry = new DocumentDataEntry
							{
								DataHash = reader.GetString( 0 ),
								LoadDataCallback = loadDocumentData
                            };

							data.Add( binaryDataEntry );
						}
					}
				}

				_documentCache = data;
			}

			return _documentCache;
		}

		/// <summary>
		///     Load entities.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		IEnumerable<EntityEntry> IDataSource.GetEntities( IProcessingContext context )
		{
			if ( _entityCache == null )
			{
				var data = new Dictionary<Guid, EntityEntry>( );

				/////
				// Query entities that are part of the solution
				/////
				using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandText = "spGetTenantAppStagedEntities";
					command.CommandType = CommandType.StoredProcedure;
					command.AddParameterWithValue( "@solutionId", SolutionId );
					command.AddParameterWithValue( "@tenant", TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							if ( reader.IsDBNull( 0 ) )
							{
								context?.WriteWarning( "Unexpected null UpgradeId in Entity." );

								continue;
							}

							int depth = reader.GetInt32( 0 );
							Guid entityUpgradeId = reader.GetGuid( 1 );
							long entityId = reader.GetInt64( 2 );
							string entityName = reader.IsDBNull( 3 ) ? null : reader.GetString( 3 );
							long entityTypeId = reader.IsDBNull( 4 ) ? 0 : reader.GetInt64( 4 );
							string entityTypeName = reader.IsDBNull( 5 ) ? null : reader.GetString( 5 );

							EntityEntry entry;

							if ( !data.TryGetValue( entityUpgradeId, out entry ) )
							{
								entry = new EntityStagingEntry
								{
									EntityId = entityUpgradeId,
									Id = entityId,
									EntityName = entityName,
									EntityTypeId = entityTypeId,
									EntityTypeName = entityTypeName
								};

								data[ entityUpgradeId ] = entry;
							}

							var castEntry = entry as EntityStagingEntry;

							if ( depth > 0 && castEntry != null )
							{
								if ( castEntry.Parents == null )
								{
									castEntry.Parents = new List<EntityParentEntry>( );
								}

								castEntry.Parents.Add( new EntityParentEntry
								{
									Depth = depth,
									ParentEntityUpgradeId = reader.GetGuid( 6 ),
									RelationshipTypeUpgradeId = reader.GetGuid( 7 ),
									RelationshipTypeId = reader.IsDBNull( 8 ) ? 0 : reader.GetInt64( 8 ),
									RelationshipTypeName = reader.IsDBNull( 9 ) ? null : reader.GetString( 9 ),
									Reason = ( InclusionReason ) Enum.Parse( typeof ( InclusionReason ), reader.GetString( 10 ), true ),
								} );
							}
						}
					}
				}

				_entityCache = data.Values.ToList( );
			}

			return _entityCache;
		}

		/// <summary>
		///     Load field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that bits are represented as Booleans
		///     - ensure that XML is transformed so that entityRefs contain UpgradeIds
		///     - or where XML contains an alias, translate it to upgradeId|alias   (as the alias may be changed in the target)
		///     - ensure that aliases export their namespace and direction marker.
		/// </remarks>
		IEnumerable<DataEntry> IDataSource.GetFieldData( string dataTable, IProcessingContext context )
		{
			List<DataEntry> data;

			if ( !_entityDataCache.TryGetValue( dataTable, out data ) )
			{
				data = new List<DataEntry>( );

				bool isAliasTable = dataTable == "Alias";
				string extraSql = isAliasTable ? ", [Namespace], AliasMarkerId" : "";

				/////
				// Converter for XML entity ref data
				/////
				Func<Guid, Guid, object, object> converter = GetDataConverter( dataTable, context );

				/////
				// Query entities that are part of the solution
				/////
				using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandText = "spGetTenantAppFieldData";
					command.CommandType = CommandType.StoredProcedure;
					command.AddParameterWithValue( "@solutionId", SolutionId );
					command.AddParameterWithValue( "@tenant", TenantId );
					command.AddParameterWithValue( "@dataTable", dataTable );
					command.AddParameterWithValue( "@extraSql", extraSql );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var entry = new DataEntry
							{
								EntityId = reader.GetGuid( 0 ),
								FieldId = reader.GetGuid( 1 ),
								Data = reader.GetValue( 2 )
							};

							if ( isAliasTable )
							{
								entry.Namespace = reader.GetString( 3 );
								entry.AliasMarkerId = reader.GetInt32( 4 );
							}

							data.Add( entry );
						}
					}

					foreach ( DataEntry entry in data )
					{
						entry.Data = converter( entry.EntityId, entry.FieldId, entry.Data );
					}
				}

				_entityDataCache[ dataTable ] = data;
			}

			return data;
		}

		/// <summary>
		///     Loads the application metadata.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">@Invalid package Id</exception>
		Metadata IDataSource.GetMetadata( IProcessingContext context )
		{
			/////
			// The upgrade ID of the solution object is the application Id
			// The appVerId gets dynamically generated
			/////

			using ( new TenantAdministratorContext( TenantId ) )
			{
				Guid applicationId = Entity.GetUpgradeId( SolutionId );

				var solution = Entity.Get<Solution>( new EntityRef( SolutionId ) );

				if ( solution.PackageId == null )
				{
					throw new InvalidOperationException( @"Invalid package Id" );
				}

				IList<SolutionDependency> solutionDependencies = GetSolutionDependencies( context, solution );

				var metadata = new Metadata
				{
					AppVerId = solution.PackageId.Value,
					AppName = solution.Name,
					AppId = applicationId,
					Description = solution.Description,
					Name = solution.Name,
					Version = string.IsNullOrEmpty( solution.SolutionVersionString ) ? "1.0" : solution.SolutionVersionString,
					Publisher = solution.SolutionPublisher,
					PublisherUrl = solution.SolutionPublisherUrl,
					ReleaseDate = solution.SolutionReleaseDate ?? DateTime.MinValue,
					Dependencies = solutionDependencies,
					Type = SourceType.AppPackage,
					PlatformVersion = SystemInfo.PlatformVersion
				};
				return metadata;
			}
		}

		/// <summary>
		///     Load relationships.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		IEnumerable<RelationshipEntry> IDataSource.GetRelationships( IProcessingContext context )
		{
			if ( _relationshipCache == null )
			{
				var data = new List<RelationshipEntry>( );

				/////
				// Query the relationship types that are lookups.
				/////
				string lookups = $@"
DECLARE @cardinalityId BIGINT
DECLARE @oneToOneId BIGINT
DECLARE @manyToOneId BIGINT
DECLARE @oneToManyId BIGINT
DECLARE @manyToManyId BIGINT

SELECT @cardinalityId = EntityId FROM Data_Alias WHERE Data = 'cardinality' AND Namespace = 'core' AND TenantId = @tenant
SELECT @oneToOneId = EntityId FROM Data_Alias WHERE Data = 'oneToOne' AND Namespace = 'core' AND TenantId = @tenant
SELECT @oneToManyId = EntityId FROM Data_Alias WHERE Data = 'oneToMany' AND Namespace = 'core' AND TenantId = @tenant
SELECT @manyToOneId = EntityId FROM Data_Alias WHERE Data = 'manyToOne' AND Namespace = 'core' AND TenantId = @tenant
SELECT @manyToManyId = EntityId FROM Data_Alias WHERE Data = 'manyToMany' AND Namespace = 'core' AND TenantId = @tenant

SELECT e.UpgradeId, {( short ) CardinalityEnum_Enumeration.OneToOne} FROM Entity e JOIN Relationship r ON r.TenantId = e.TenantId AND e.Id = r.FromId AND r.TypeId = @cardinalityId AND ToId = @oneToOneId WHERE e.TenantId = @tenant
UNION ALL
SELECT e.UpgradeId, {( short ) CardinalityEnum_Enumeration.OneToMany} FROM Entity e JOIN Relationship r ON r.TenantId = e.TenantId AND e.Id = r.FromId AND r.TypeId = @cardinalityId AND ToId = @oneToManyId WHERE e.TenantId = @tenant
UNION ALL
SELECT e.UpgradeId, {( short ) CardinalityEnum_Enumeration.ManyToOne} FROM Entity e JOIN Relationship r ON r.TenantId = e.TenantId AND e.Id = r.FromId AND r.TypeId = @cardinalityId AND ToId = @manyToOneId WHERE e.TenantId = @tenant
UNION ALL
SELECT e.UpgradeId, {( short ) CardinalityEnum_Enumeration.ManyToMany} FROM Entity e JOIN Relationship r ON r.TenantId = e.TenantId AND e.Id = r.FromId AND r.TypeId = @cardinalityId AND ToId = @manyToManyId WHERE e.TenantId = @tenant
";

				var lookupTypes = new Dictionary<Guid, CardinalityEnum_Enumeration>( );

				/////
				// Query entities that are part of the solution
				/////
				using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandText = lookups;
					command.CommandType = CommandType.Text;

					command.AddParameterWithValue( "@tenant", TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							Guid id = reader.GetGuid( 0 );
							var cardinalityType = ( CardinalityEnum_Enumeration ) reader.GetInt32( 1 );

							lookupTypes[ id ] = cardinalityType;
						}
					}

					command.CommandText = "spGetTenantAppRelationships";
					command.CommandType = CommandType.StoredProcedure;
					command.AddParameterWithValue( "@solutionId", SolutionId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							if ( reader.IsDBNull( 0 ) )
							{
								context?.WriteWarning( "Unexpected null UpgradeId in Entity." );

								continue;
							}

							Guid typeId = reader.GetGuid( 0 );
							Guid fromId = reader.GetGuid( 1 );
							Guid toId = reader.GetGuid( 2 );

							CardinalityEnum_Enumeration cardinality;

							var entry = lookupTypes.TryGetValue( typeId, out cardinality ) ? new RelationshipEntry( typeId, fromId, toId, cardinality ) : new RelationshipEntry( typeId, fromId, toId );

							data.Add( entry );
						}
					}
				}

				_relationshipCache = data;
			}

			return _relationshipCache;
		}


		/// <summary>
		///     Sets up this instance.
		/// </summary>
		void IDataSource.Setup( IProcessingContext context )
		{
            using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
            using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = @"CREATE TABLE #candidateList ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY, [Explicit] BIT )
CREATE TABLE #dependents ( Id BIGINT PRIMARY KEY )";
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Tears down this instance.
		/// </summary>
		void IDataSource.TearDown( IProcessingContext context )
		{
            using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
            using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = @"DROP TABLE #candidateList
DROP TABLE #dependents";
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Gets the missing field data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<DataEntry> GetMissingFieldData( IProcessingContext context )
		{
			return Enumerable.Empty<DataEntry>( );
		}

		/// <summary>
		///     Gets the missing relationships.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<RelationshipEntry> GetMissingRelationships( IProcessingContext context )
		{
			return Enumerable.Empty<RelationshipEntry>( );
		}


        /// <summary>
        ///     Return empty set of SecureData
        /// </summary>
        public IEnumerable<SecureDataEntry> GetSecureData(IProcessingContext context)
        {
            return Enumerable.Empty<SecureDataEntry>();    // You can't get secure data from a tenant app
        }

        /// <summary>
        ///     Create a delegate to convert data after reading from the database.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Func<Guid, Guid, object, object> GetDataConverter( string dataTable, IProcessingContext context )
		{
			Func<Guid, Guid, object, object> converter;

			switch ( dataTable )
			{
				case "Xml":

					/////
					// Use Xml processor to remap EntityRefs located in XML
					/////
					var xmlProcessor = new XmlFieldProcessor
					{
						ConversionMode = XmlConversionMode.LocalIdToUpgradeGuid,
						TenantId = TenantId,
						DatabaseContext = DatabaseContext,
						ProcessingContext = context
					};

					converter = ( entityUpgradeId, fieldUpgradeId, value ) => xmlProcessor.RemapXmlEntities( entityUpgradeId, fieldUpgradeId, ( string ) value );
					break;

				default:
					converter = ( entityUpgradeId, fieldUpgradeId, value ) => value;
					break;
			}

			return converter;
		}

		/// <summary>
		///     Gets the solution dependencies.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="solution">The solution.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">No active database connection.</exception>
		private IList<SolutionDependency> GetSolutionDependencies( IProcessingContext context, Solution solution )
		{
			IList<SolutionDependency> solutionDependencies = new List<SolutionDependency>( );

            using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
			{
				IList<ApplicationDependency> applicationDependencies = solution.GetDependencies( );

				if ( applicationDependencies != null && applicationDependencies.Count > 0 )
				{
					foreach ( ApplicationDependency applicationDependency in applicationDependencies )
					{
						solutionDependencies.Add( new SolutionDependency( applicationDependency ) );
					}
				}

				if ( context != null )
				{
					context.Report.SolutionDependencies = solutionDependencies;
				}
			}

			return solutionDependencies;
        }

        /// <summary>
        /// Gets the entities that should not be removed as part of an upgrade operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public IEnumerable<Guid> GetDoNotRemove( IProcessingContext context )
        {
            return Enumerable.Empty<Guid>( );
        }
    }
}