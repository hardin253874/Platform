// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
	internal class TenantAppSource : SqlBase, IDataSource
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
		///     Gets or sets the relationship restrictions.
		/// </summary>
		/// <value>
		///     The relationship restrictions.
		/// </value>
		public List<RelationshipRestriction> RelationshipRestrictions
		{
			get;
			set;
		}

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
        ///     The package ID of the application prior to being assigned a new one (if the application had originally come from the application library)
        /// </summary>
        /// <value>
        ///     The package id.
        /// </value>
        public Guid? OriginalPackageId
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
				var data = new List<EntityEntry>( );

				/////
				// Query entities that are part of the solution
				/////
				using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandText = "spGetTenantAppEntities";
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

							var entry = new EntityEntry
							{
								EntityId = reader.GetGuid( 0 )
							};

							data.Add( entry );
						}
					}
				}

				_entityCache = data;
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

							if ( dataTable == Helpers.DateTimeName )
							{
								if ( entry.Data != null && entry.Data != DBNull.Value )
								{
									if ( entry.Data is DateTime )
									{
										DateTime dt = ( DateTime ) entry.Data;

										if ( dt.Kind == DateTimeKind.Unspecified )
										{
											entry.Data = DateTime.SpecifyKind( ( DateTime ) entry.Data, DateTimeKind.Utc );
										}
									}
									else
									{
										DateTime dt;

										if ( DateTime.TryParse( entry.Data.ToString( ), null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeLocal, out dt ) )
										{
											entry.Data = dt;
										}
									}
								}
							}

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
			    WellKnownAliases aliases = WellKnownAliases.CurrentTenant;

                var cardinality = new Dictionary<long, CardinalityEnum_Enumeration>( );

				/////
				// Cache the various types of cardinality.
				/////
				using ( new TenantAdministratorContext( TenantId ) )
				{
					cardinality [ aliases.OneToOne ] = CardinalityEnum_Enumeration.OneToOne;
					cardinality [ aliases.OneToMany ] = CardinalityEnum_Enumeration.OneToMany;
					cardinality [ aliases.ManyToOne ] = CardinalityEnum_Enumeration.ManyToOne;
					cardinality [ aliases.ManyToMany ] = CardinalityEnum_Enumeration.ManyToMany;
				}

				var dict = new Dictionary<RelationshipEntryKey, RelationshipEntry>( );

				var toOne = new Dictionary<RelationshipEntryCardinalityKey, RelationshipEntry>( );
				var fromOne = new Dictionary<RelationshipEntryCardinalityKey, RelationshipEntry>( );

				/////
				// Query entities that are part of the solution
				/////
				using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandText = "spGetTenantAppRelationships";
					command.CommandType = CommandType.StoredProcedure;
					command.AddParameterWithValue( "@solutionId", SolutionId );
					command.AddParameterWithValue( "@tenant", TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						/////
						// Read the base relationships
						/////
						ReadRelationships( context, reader, cardinality, dict, toOne, fromOne, false );

						/////
						// Move to the discarded install relationships (if there are any)
						/////
						if ( reader.NextResult( ) )
						{
							/////
							// Read discarded relationships
							/////
							ReadRelationships( context, reader, cardinality, dict, toOne, fromOne, true );
						}
					}
				}

				_relationshipCache = dict.Values.ToList( );
			}

			return _relationshipCache;
		}

		/// <summary>
		/// Reads the relationships.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="dict">The dictionary.</param>
		/// <param name="toOne">To one.</param>
		/// <param name="fromOne">From one.</param>
		/// <param name="droppedRelationships">if set to <c>true</c> [dropped relationships].</param>
		private void ReadRelationships( IProcessingContext context, IDataReader reader, Dictionary<long, CardinalityEnum_Enumeration> cardinality, Dictionary<RelationshipEntryKey, RelationshipEntry> dict, Dictionary<RelationshipEntryCardinalityKey, RelationshipEntry> toOne, Dictionary<RelationshipEntryCardinalityKey, RelationshipEntry> fromOne, bool droppedRelationships )
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

				CardinalityEnum_Enumeration? card = null;

				if (reader.FieldCount > 3 && !reader.IsDBNull( 3 ) )
				{
					CardinalityEnum_Enumeration cardValue;

					if ( cardinality.TryGetValue( reader.GetInt64( 3 ), out cardValue ) )
					{
						card = cardValue;
					}
				}

				RelationshipEntry entry = card == null ? new RelationshipEntry( typeId, fromId, toId ) : new RelationshipEntry( typeId, fromId, toId, card.Value );

				if ( RelationshipRestrictions != null )
				{
					if ( RelationshipRestrictions.Any( restriction => !restriction.IsAllowed( entry ) ) )
					{
						continue;
					}
				}

				var key = entry.GetKey( );

				RelationshipEntry value;

				bool violation = false;

				Action addFrom = null;
				Action addTo = null;

				if ( entry.Cardinality != null && ( entry.Cardinality == CardinalityEnum_Enumeration.ManyToOne || entry.Cardinality == CardinalityEnum_Enumeration.OneToOne ) )
				{
					var cardinalityKey = new RelationshipEntryCardinalityKey( entry.TypeId, entry.FromId );

					if ( toOne.TryGetValue( cardinalityKey, out value ) )
					{
						if ( entry.TypeId != value.TypeId || entry.FromId != value.FromId || entry.ToId != value.ToId )
						{
							/////
							// Cardinality violation.
							/////
							EventLog.Application.WriteWarning( string.Format( "Detected cardinality violation {7}({0}).\n\nExisting Type: {1}\nExisting From: {2}\nExisting To: {3}\n\nDropped Type: {4}\nDropped From: {5}\nDropped To: {6}\n",
								entry.Cardinality,
								value.TypeId.ToString( "B" ),
								value.FromId.ToString( "B" ),
								value.ToId.ToString( "B" ),
								entry.TypeId.ToString( "B" ),
								entry.FromId.ToString( "B" ),
								entry.ToId.ToString( "B" ),
								droppedRelationships ? "processing previously dropped relationship " : "" ) );
						}

						violation = true;
					}
					else
					{
						RelationshipEntry relationshipEntry = entry;
						addFrom = ( ) => toOne.Add( cardinalityKey, relationshipEntry );
					}
				}

				if ( !violation && entry.Cardinality != null && ( entry.Cardinality == CardinalityEnum_Enumeration.OneToMany || entry.Cardinality == CardinalityEnum_Enumeration.OneToOne ) )
				{
					var cardinalityKey = new RelationshipEntryCardinalityKey( entry.TypeId, entry.ToId );

					if ( fromOne.TryGetValue( cardinalityKey, out value ) )
					{
						if ( entry.TypeId != value.TypeId || entry.FromId != value.FromId || entry.ToId != value.ToId )
						{
							/////
							// Cardinality violation.
							/////
							EventLog.Application.WriteWarning( string.Format( "Detected cardinality violation {7}({0}).\n\nExisting Type: {1}\nExisting From: {2}\nExisting To: {3}\n\nDropped Type: {4}\nDropped From: {5}\nDropped To: {6}\n",
								entry.Cardinality,
								value.TypeId.ToString( "B" ),
								value.FromId.ToString( "B" ),
								value.ToId.ToString( "B" ),
								entry.TypeId.ToString( "B" ),
								entry.FromId.ToString( "B" ),
								entry.ToId.ToString( "B" ),
								droppedRelationships ? "processing previously dropped relationship " : "" ) );
						}

						violation = true;
					}
					else
					{
						RelationshipEntry relationshipEntry = entry;
						addTo = ( ) => fromOne.Add( cardinalityKey, relationshipEntry );
					}
				}

				if ( violation )
				{
					continue;
				}

				addFrom?.Invoke( );

				addTo?.Invoke( );

				if ( !dict.TryGetValue( key, out value ) )
				{
					dict.Add( key, entry );
				}
				else
				{
					if ( entry.Cardinality != CardinalityEnum_Enumeration.ManyToMany )
					{
						if ( entry.TypeId != value.TypeId || entry.FromId != value.FromId || entry.ToId != value.ToId )
						{
							/////
							// Cardinality violation.
							/////
							EventLog.Application.WriteWarning( string.Format( "Detected cardinality violation {7}({0}).\n\nExisting Type: {1}\nExisting From: {2}\nExisting To: {3}\n\nDropped Type: {4}\nDropped From: {5}\nDropped To: {6}\n",
								entry.Cardinality,
								value.TypeId.ToString( "B" ),
								value.FromId.ToString( "B" ),
								value.ToId.ToString( "B" ),
								entry.TypeId.ToString( "B" ),
								entry.FromId.ToString( "B" ),
								entry.ToId.ToString( "B" ),
								droppedRelationships ? "processing previously dropped relationship " : "" ) );
						}
					}
				}
			}
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

			Precache( context );
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
				IList<ApplicationDependency> applicationDependencies = solution.GetDependencies( true );

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
        ///     Pre-caches this source data.
        /// </summary>
        private void Precache( IProcessingContext context )
		{
			IDataSource source = this;

			/////
			// Pre-cache all the application entities, relationships and data.
			/////
			source.GetEntities( context );
			source.GetRelationships( context );
			foreach ( string fieldDataTable in Helpers.FieldDataTables )
			{
				source.GetFieldData( fieldDataTable, context );
			}
			source.GetBinaryData( context );
		}


        IEnumerable<SecureDataEntry> IDataSource.GetSecureData(IProcessingContext context)
        {
            return Enumerable.Empty<SecureDataEntry>();     // You can't get secure data from a tenant app
        }

        /// <summary>
        /// Gets the entities that should not be removed as part of an upgrade operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public IEnumerable<Guid> GetDoNotRemove( IProcessingContext context )
        {
            // Namely, get the entities that:
            // 1. used to be part of the application,
            // 2. which are no longer part of the application,
            // 3. which are still present in the tenant (i.e. the app developer removed them from the app, but didn't delete them altogether).
            // 4. we also need to carry over any entities marked as 'do not remove' in previous versions

            // Step 0: Determine what the original version of this package was
            Guid? packageId = OriginalPackageId;
            if ( packageId == null )
            {
                context.WriteInfo( "Tenant application does not have a package Id" );
                return Enumerable.Empty<Guid>( );
            }

            // Step 1: Get entities in the previous version
            LibraryAppSource appLibrarySource = new LibraryAppSource
            {
                AppVerId = packageId.Value
            };
            IEnumerable<Guid> entitiesInPrevVersion = appLibrarySource
                .GetEntities( context )
                .Select( e => e.EntityId )
                .ToList( );

            // Step 2: Disregard entities that are still in the application
            IEnumerable<Guid> entitiesInTenantApp = ((IDataSource)this)
                .GetEntities( context )
                .Select( e => e.EntityId );
            IEnumerable<Guid> missingEntities = entitiesInPrevVersion.Except( entitiesInTenantApp ).ToList();

            // Step 3: Check database to see if the entities are still present
            ISet<Guid> doNotRemove = new HashSet<Guid>( );
            DataTable dt = TableValuedParameter.Create( TableValuedParameterType.Guid );
            foreach ( var guid in missingEntities )
            {
                dt.Rows.Add( guid );
            }
            using ( IDbCommand command = CreateCommand( ) )
            {
                command.CommandText = "dbo.spGetEntityIdsByUpgradeIds";
                command.CommandType = CommandType.StoredProcedure;
                command.AddParameter( "@tenantId", DbType.Int64, TenantId );
                command.AddTableValuedParameter( "@data", dt );

                using ( IDataReader reader = command.ExecuteReader( ) )
                {
                    while ( reader.Read( ) )
                    {
                        var guid = reader.GetGuid( 0 );
                        doNotRemove.Add( guid ); // entities still in the tenant should be marked as 'do not remove'
                    }
                }
            }

            // Step 4: Include entities marked as 'do not remove' in previous tenants
            var carriedOver = appLibrarySource.GetDoNotRemove( context );
            foreach ( Guid guid in carriedOver )
                doNotRemove.Add( guid );

            // Entities that have been removed from the application, but still present in the tenant,
            // should get marked as 'do not delete' to indicate that when the application is upgraded it should not delete the entities.
            return doNotRemove;
        }
    }
}