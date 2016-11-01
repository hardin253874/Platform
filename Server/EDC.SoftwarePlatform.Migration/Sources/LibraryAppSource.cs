// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Storage;
using System.Linq;

namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Represents a reader for loading a specific version of an application from the application library.
	/// </summary>
	internal class LibraryAppSource : SqlBase, IDataSource
	{
		/// <summary>
		///     The type cardinalities
		/// </summary>
		private Dictionary<Guid, CardinalityEnum_Enumeration> _typeCardinalities;

		/// <summary>
		///     Gets or sets the app id.
		/// </summary>
		/// <value>
		///     The app id.
		/// </value>
		public Guid AppId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the app.
		/// </summary>
		/// <value>
		///     The name of the app.
		/// </value>
		public string AppName
		{
			get;
			set;
		}

		/// <summary>
		///     The application-version identifier to load.
		/// </summary>
		/// <value>
		///     The app version id.
		/// </value>
		public Guid AppVerId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the dependencies.
		/// </summary>
		/// <value>
		/// The dependencies.
		/// </value>
		public IList<SolutionDependency> Dependencies
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the publisher.
		/// </summary>
		/// <value>
		///     The publisher.
		/// </value>
		public string Publisher
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the publisher URL.
		/// </summary>
		/// <value>
		///     The publisher URL.
		/// </value>
		public string PublisherUrl
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the release date.
		/// </summary>
		/// <value>
		///     The release date.
		/// </value>
		public DateTime ReleaseDate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the publish date.
		/// </summary>
		/// <value>
		/// The publish date.
		/// </value>
		public DateTime PublishDate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type cardinalities.
		/// </summary>
		/// <value>
		///     The type cardinalities.
		/// </value>
		private Dictionary<Guid, CardinalityEnum_Enumeration> TypeCardinalities
		{
			get
			{
				if ( _typeCardinalities == null )
				{
					/////
					// Query the relationship types for cardinality.
					/////
					string cardinalityQuery = string.Format( @"
DECLARE @cardinalityUid UNIQUEIDENTIFIER
DECLARE @cardinalityId BIGINT
DECLARE @oneToOneUid UNIQUEIDENTIFIER
DECLARE @oneToOneId BIGINT
DECLARE @manyToOneUid UNIQUEIDENTIFIER
DECLARE @manyToOneId BIGINT
DECLARE @oneToManyUid UNIQUEIDENTIFIER
DECLARE @oneToManyId BIGINT
DECLARE @manyToManyUid UNIQUEIDENTIFIER
DECLARE @manyToManyId BIGINT

SELECT @cardinalityUid = EntityUid FROM AppData_Alias WHERE Data = 'cardinality' AND Namespace = 'core'
SELECT @cardinalityId = EntityId FROM Data_Alias WHERE Data = 'cardinality' AND Namespace = 'core' AND TenantId = @tenantId
SELECT @oneToOneUid = EntityUid FROM AppData_Alias WHERE Data = 'oneToOne' AND Namespace = 'core'
SELECT @oneToOneId = EntityId FROM Data_Alias WHERE Data = 'oneToOne' AND Namespace = 'core' AND TenantId = @tenantId
SELECT @manyToOneUid = EntityUid FROM AppData_Alias WHERE Data = 'manyToOne' AND Namespace = 'core'
SELECT @manyToOneId = EntityId FROM Data_Alias WHERE Data = 'manyToOne' AND Namespace = 'core' AND TenantId = @tenantId
SELECT @oneToManyUid = EntityUid FROM AppData_Alias WHERE Data = 'oneToMany' AND Namespace = 'core'
SELECT @oneToManyId = EntityId FROM Data_Alias WHERE Data = 'oneToMany' AND Namespace = 'core' AND TenantId = @tenantId
SELECT @manyToManyUid = EntityUid FROM AppData_Alias WHERE Data = 'manyToMany' AND Namespace = 'core'
SELECT @manyToManyId = EntityId FROM Data_Alias WHERE Data = 'manyToMany' AND Namespace = 'core' AND TenantId = @tenantId

SELECT e.UpgradeId, {0} FROM Entity e JOIN Relationship r ON e.TenantId = r.TenantId AND e.Id = r.FromId AND r.TypeId = @cardinalityId AND ToId = @oneToOneId WHERE e.TenantId = @tenantId
UNION ALL
SELECT FromUid, {0} FROM AppRelationship WHERE AppVerUid = @appVer AND TypeUid = @cardinalityUid AND ToUid = @oneToOneUid
UNION ALL
SELECT e.UpgradeId, {1} FROM Entity e JOIN Relationship r ON e.TenantId = r.TenantId AND e.Id = r.FromId AND r.TypeId = @cardinalityId AND ToId = @oneToManyId WHERE e.TenantId = @tenantId
UNION ALL
SELECT FromUid, {1} FROM AppRelationship WHERE AppVerUid = @appVer AND TypeUid = @cardinalityUid AND ToUid = @oneToManyUid
UNION ALL
SELECT e.UpgradeId, {2} FROM Entity e JOIN Relationship r ON e.TenantId = r.TenantId AND e.Id = r.FromId AND r.TypeId = @cardinalityId AND ToId = @manyToOneId WHERE e.TenantId = @tenantId
UNION ALL
SELECT FromUid, {2} FROM AppRelationship WHERE AppVerUid = @appVer AND TypeUid = @cardinalityUid AND ToUid = @manyToOneUid
UNION ALL
SELECT e.UpgradeId, {3} FROM Entity e JOIN Relationship r ON e.TenantId = r.TenantId AND e.Id = r.FromId AND r.TypeId = @cardinalityId AND ToId = @manyToManyId WHERE e.TenantId = @tenantId
UNION ALL
SELECT FromUid, {3} FROM AppRelationship WHERE AppVerUid = @appVer AND TypeUid = @cardinalityUid AND ToUid = @manyToManyUid
", ( short ) CardinalityEnum_Enumeration.OneToOne, ( short ) CardinalityEnum_Enumeration.OneToMany, ( short ) CardinalityEnum_Enumeration.ManyToOne, ( short ) CardinalityEnum_Enumeration.ManyToMany );

					var map = new Dictionary<Guid, CardinalityEnum_Enumeration>( );

					using ( IDbCommand command = CreateCommand( ) )
					{
						long tenantId = CallData<long>.GetValue( "TargetTenantId" );

						command.CommandText = cardinalityQuery;
						command.AddParameterWithValue( "@appVer", AppVerId );
						command.AddParameterWithValue( "@tenantId", tenantId );

						using ( IDataReader reader = command.ExecuteReader( ) )
						{
							while ( reader.Read( ) )
							{
								Guid id = reader.GetGuid( 0 );
								var cardinalityType = ( CardinalityEnum_Enumeration ) reader.GetInt32( 1 );

								map[ id ] = cardinalityType;
							}
						}
					}

					_typeCardinalities = map;
				}

				return _typeCardinalities;
			}
		}

		/// <summary>
		///     Gets or sets the version.
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		public string Version
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the binary data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<BinaryDataEntry> GetBinaryData( IProcessingContext context )
		{
		    Func<string, byte[]> loadBinaryData = token => FileRepositoryUtils.LoadFileData(Factory.AppLibraryFileRepository, token, context);            

            using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandType = CommandType.Text;
				command.CommandText = CommandText.AppLibraryGetBinaryData;

				command.AddParameterWithValue( "@appVer", AppVerId );
                command.AddParameterWithValue( "@isOfTypeId", Helpers.IsOfTypeRelationshipUpgradeId );
                command.AddParameterWithValue( "@inheritsId", Helpers.InheritsRelationshipUpgradeId );
                command.AddParameterWithValue( "@fileDataHashFieldId", Helpers.FileDataHashFieldUpgradeId );
                command.AddParameterWithValue( "@imageFileTypeId", Helpers.ImageFileTypeUpgradeId );

                using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						var binaryDataEntry = new BinaryDataEntry
						{
							DataHash = reader.GetString( 0 ),							
							LoadDataCallback = loadBinaryData
                        };

						yield return binaryDataEntry;
					}
				}
			}
		}

		public IEnumerable<DocumentDataEntry> GetDocumentData( IProcessingContext context )
		{
            Func<string, byte[]> loadDocumentData = token => FileRepositoryUtils.LoadFileData(Factory.AppLibraryFileRepository, token, context);

            using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandType = CommandType.Text;
				command.CommandText = CommandText.AppLibraryGetDocumentData;

                command.AddParameterWithValue( "@appVer", AppVerId);
                command.AddParameterWithValue( "@isOfTypeId", Helpers.IsOfTypeRelationshipUpgradeId );
                command.AddParameterWithValue( "@inheritsId", Helpers.InheritsRelationshipUpgradeId );
                command.AddParameterWithValue( "@fileDataHashFieldId", Helpers.FileDataHashFieldUpgradeId );
                command.AddParameterWithValue( "@imageFileTypeId", Helpers.ImageFileTypeUpgradeId );
                command.AddParameterWithValue( "@fileTypeId", Helpers.FileTypeUpgradeId );

                using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						var documentDataEntry = new DocumentDataEntry
						{
							DataHash = reader.GetString( 0 ),
							LoadDataCallback = loadDocumentData
                        };

						yield return documentDataEntry;
					}
				}
			}
		}

		/// <summary>
		///     Load entities.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public virtual IEnumerable<EntityEntry> GetEntities( IProcessingContext context )
		{
			/////
			// Query entities that are part of the solution
			/////
			const string sql = @"select EntityUid
                            from AppEntity e
                            where e.AppVerUid = @appVer";

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;
				command.AddParameterWithValue( "@appVer", AppVerId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						if ( reader.IsDBNull( 0 ) )
						{
							context.WriteWarning( "Unexpected null UpgradeId in Entity." );
							continue;
						}

						var entry = new EntityEntry
						{
							EntityId = reader.GetGuid( 0 )
						};

						yield return entry;
					}
				}
			}
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
		///     - ensure that XML is transformed so that entityRefs contain Upgrade ids
		///     - or where XML contains an alias, translate it to upgradeId|alias   (as the alias may be changed in the target)
		///     - ensure that aliases export their namespace and direction marker.
		/// </remarks>
		public IEnumerable<DataEntry> GetFieldData( string dataTable, IProcessingContext context )
		{
			bool isAliasTable = dataTable == "Alias";
			string extraSql = isAliasTable ? ", [Namespace], AliasMarkerId" : "";

			/////
			// Query entities that are part of the solution
			/////
			string sql = @"select EntityUid, FieldUid, Data" + extraSql + @"
                           from AppData_" + dataTable + @" d
                           where d.AppVerUid = @appVer";

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;
				command.AddParameterWithValue( "@appVer", AppVerId );

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

						yield return entry;
					}
				}
			}
        }

        /// <summary>
        /// Gets the entities that should not be removed as part of an upgrade operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual IEnumerable<Guid> GetDoNotRemove( IProcessingContext context )
        {
            /////
            // Query entities that have been marked as do-not-remove during upgrade of a solution.
            /////
            const string sql = @"select EntityUid
                            from AppDoNotRemove d
                            where d.AppVerUid = @appVer";

            using ( IDbCommand command = CreateCommand( ) )
            {
                command.CommandText = sql;
                command.AddParameterWithValue( "@appVer", AppVerId );

                using ( IDataReader reader = command.ExecuteReader( ) )
                {
                    while ( reader.Read( ) )
                    {
                        if ( reader.IsDBNull( 0 ) )
                        {
                            context.WriteWarning( "Unexpected null EntityUid in AppDoNotRemove." );
                            continue;
                        }

                        Guid entityUid = reader.GetGuid( 0 );
                        yield return entityUid;
                    }
                }
            }
        }

        /// <summary>
        ///     Loads the application metadata.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Metadata GetMetadata( IProcessingContext context )
		{
			return new Metadata
			{
				AppId = AppId,
				AppName = AppName,
				AppVerId = AppVerId,
				Name = Name,
				Description = Description,
				Version = Version,
				PublishDate = PublishDate,
				Publisher = Publisher,
				PublisherUrl = PublisherUrl,
				ReleaseDate = ReleaseDate,
				Type = SourceType.AppPackage,
				Dependencies = Dependencies,
				PlatformVersion = SystemInfo.PlatformVersion
			};
		}

		/// <summary>
		///     Load relationships.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public IEnumerable<RelationshipEntry> GetRelationships( IProcessingContext context )
		{
			Dictionary<Guid, CardinalityEnum_Enumeration> typeCardinalities = TypeCardinalities;

			var entries = new List<RelationshipEntry>( );

			/////
			// Query entities that are part of the solution
			/////
			const string sql = @"
                    select TypeUid, FromUid, ToUid
                            from AppRelationship r
                            where r.AppVerUid = @appVer";

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;
				command.CommandType = CommandType.Text;

				command.AddParameterWithValue( "@appVer", AppVerId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						Guid typeId = reader.GetGuid( 0 );
						Guid fromId = reader.GetGuid( 1 );
						Guid toId = reader.GetGuid( 2 );

						CardinalityEnum_Enumeration cardinality;

						var entry = typeCardinalities.TryGetValue( typeId, out cardinality ) ? new RelationshipEntry( typeId, fromId, toId, cardinality ) : new RelationshipEntry( typeId, fromId, toId );

						entries.Add( entry );
					}
				}
			}

			return entries;
		}


		/// <summary>
		///     Sets up this instance.
		/// </summary>
		public void Setup( IProcessingContext context )
		{
		}

		/// <summary>
		///     Tears down this instance.
		/// </summary>
		public void TearDown( IProcessingContext context )
		{
		}


		/// <summary>
		///     Gets the missing field data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<DataEntry> GetMissingFieldData( IProcessingContext context )
		{
			long tenantId = CallData<long>.GetValue( "TargetTenantId" );

			/////
			// Query entities that are part of the solution
			/////
			const string sql = @"select EntityUid, FieldUid
                           from AppDeploy_Field d
                           where d.AppVerUid = @appVer AND TenantId = @tenantId";

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;
				command.AddParameterWithValue( "@appVer", AppVerId );
				command.AddParameterWithValue( "@tenantId", tenantId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						var entry = new DataEntry
						{
							EntityId = reader.GetGuid( 0 ),
							FieldId = reader.GetGuid( 1 ),
						};

						yield return entry;
					}
				}
			}
		}

		/// <summary>
		///     Gets the missing relationships.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<RelationshipEntry> GetMissingRelationships( IProcessingContext context )
		{
			long tenantId = CallData<long>.GetValue( "TargetTenantId" );
			Dictionary<Guid, CardinalityEnum_Enumeration> typeCardinalities = TypeCardinalities;

			/////
			// Query entities that are part of the solution
			/////
			const string sql = @"SELECT TypeUid, FromUid, ToUid FROM AppDeploy_Relationship WHERE AppVerUid = @appVer AND TenantId = @tenantId";

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;
				command.AddParameterWithValue( "@appVer", AppVerId );
				command.AddParameterWithValue( "@tenantId", tenantId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						Guid typeId = reader.GetGuid( 0 );
						Guid fromId = reader.GetGuid( 1 );
						Guid toId = reader.GetGuid( 2 );

						CardinalityEnum_Enumeration cardinality;

						RelationshipEntry entry = typeCardinalities.TryGetValue( typeId, out cardinality ) ? new RelationshipEntry( typeId, fromId, toId, cardinality ) : new RelationshipEntry( typeId, fromId, toId );

						yield return entry;
					}
				}
			}
		}


        /// <summary>
        ///     Return empty set of SecureData - SecureData is not stored in the library
        /// </summary>
        public IEnumerable<SecureDataEntry> GetSecureData(IProcessingContext context)
        {
            return Enumerable.Empty<SecureDataEntry>();
        }


        /// <summary>
        ///     Deletes this instance.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Delete( IProcessingContext context )
		{
			const string sql = @"
DELETE FROM AppData_{1} WHERE AppVerUid = '{0}'";

			const string entitySql = @"
DELETE FROM AppRelationship WHERE AppVerUid = '{0}'
DELETE FROM AppEntity WHERE AppVerUid = '{0}'";

			using ( IDbCommand command = CreateCommand( ) )
			{
				foreach ( string fieldDataTable in Helpers.FieldDataTables )
				{
					command.CommandText = string.Format( sql, AppVerId, fieldDataTable );
					command.ExecuteNonQuery( );
				}

				command.CommandText = string.Format( entitySql, AppVerId );
				command.ExecuteNonQuery( );
			}
		}		
	}
}