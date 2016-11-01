// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EDC.IO;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Storage;
using System.Text;

namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Represents a reader for loading a specific version of an application from the application library.
	/// </summary>
	internal class SqLiteSource : IDataSource
	{
		/// <summary>
		///     Database to read the data from.
		/// </summary>
		/// <value>
		///     The storage provider.
		/// </value>
		public SqliteStorageProvider StorageProvider
		{
			get;
			set;
		}

		/// <summary>
		///     Clean up
		/// </summary>
		public void Dispose( )
		{
			StorageProvider?.Dispose( );
		}

		/// <summary>
		///     Gets the binary data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<BinaryDataEntry> GetBinaryData( IProcessingContext context )
		{
			if ( !StorageProvider.DoesTableExist( "_Filestream_Binary" ) )
			{
				yield break;
			}

			using ( IDbCommand command = CreateCommand( ) )
			{
				const string sql = @"SELECT DISTINCT fb.FileExtension, 
                                                     fb.DataHash                                                     
                                     FROM _Filestream_Binary fb
                                     JOIN _Data_NVarChar dn ON dn.Data = fb.DataHash                                
                                     WHERE dn.FieldUid = @fileDataHashFieldId";

				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new SQLiteParameter( "@fileDataHashFieldId", DbType.String ) );
				( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = Helpers.FileDataHashFieldUpgradeId.ToString( ).ToLower( );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						var binaryDataEntry = new BinaryDataEntry
						{
							DataHash = reader.GetString( 1 ),
							FileExtension = !reader.IsDBNull( 0 ) ? reader.GetString( 0 ) : null,
							LoadDataCallback = LoadBinaryData
						};

						yield return binaryDataEntry;
					}
				}
			}
		}

		public IEnumerable<DocumentDataEntry> GetDocumentData( IProcessingContext context )
		{
			if ( !StorageProvider.DoesTableExist( "_Filestream_Document" ) )
			{
				yield break;
			}

			using ( IDbCommand command = CreateCommand( ) )
			{
				const string sql = @"SELECT DISTINCT fb.FileExtension, 
                                                     fb.DataHash
                                     FROM _Filestream_Document fb
                                     JOIN _Data_NVarChar dn ON dn.Data = fb.DataHash                                
                                     WHERE dn.FieldUid = @fileDataHashFieldId";

				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new SQLiteParameter( "@fileDataHashFieldId", DbType.String ) );
				( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = Helpers.FileDataHashFieldUpgradeId.ToString( ).ToLower( );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						var documentDataEntry = new DocumentDataEntry
						{
							DataHash = reader.GetString( 1 ),
							FileExtension = !reader.IsDBNull( 0 ) ? reader.GetString( 0 ) : null,
							LoadDataCallback = LoadDocumentData
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
		public IEnumerable<EntityEntry> GetEntities( IProcessingContext context )
		{
			/////
			// Query entities that are part of the solution
			/////
			const string sql = @"select Uid from _Entity";

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;

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
		///     - ensure that XML is transformed so that entityRefs contain Uids
		///     - or where XML contains an alias, translate it to uid|alias   (as the alias may be changed in the target)
		///     - ensure that aliases export their namespace and direction marker.
		/// </remarks>
		public IEnumerable<DataEntry> GetFieldData( string dataTable, IProcessingContext context )
		{
			/////
			// Converter for 'Bit' data to convert from 1/0 to true/false
			/////
			Func<object, object> converter = GetDataConverter( dataTable );

			bool isAliasTable = dataTable == "Alias";
			string extraSql = isAliasTable ? ", [Namespace], AliasMarkerId" : "";

			/////
			// Query entities that are part of the solution
			/////
			string sql = @"select EntityUid, FieldUid, Data" + extraSql + " from _Data_" + dataTable;

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						Guid? entityId = GetGuid( reader, 0 );

						if ( entityId == null )
						{
							continue;
						}

						Guid? fieldId = GetGuid( reader, 1 );

						if ( fieldId == null )
						{
							continue;
						}

						var entry = new DataEntry
						{
							EntityId = entityId.Value,
							FieldId = fieldId.Value,
							Data = converter( reader.GetValue( 2 ) )
						};

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
		///     Loads the application metadata.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public Metadata GetMetadata( IProcessingContext context )
		{
			string sAppName = StorageProvider.GetMetadata( "AppName" );

			string sAppVerId = StorageProvider.GetMetadata( "AppVerId" );

			Guid appVerId;

			Guid.TryParse( sAppVerId, out appVerId );

			string sAppId = StorageProvider.GetMetadata( "AppId" );

			Guid appId;

			Guid.TryParse( sAppId, out appId );

			string releaseDate = StorageProvider.GetMetadata( "ReleaseDate" );
			DateTime release;

			if ( !DateTime.TryParse( releaseDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out release ) )
			{
				release = DateTime.MinValue;
			}

			string metadataType = StorageProvider.GetMetadata( "Type" );

			var type = SourceType.Unknown;

			if ( !string.IsNullOrEmpty( metadataType ) )
			{
				if ( !Enum.TryParse( metadataType, out type ) )
				{
					type = SourceType.Unknown;
				}
			}

			string platformVersion = StorageProvider.GetMetadata( "PlatformVersion" );

			return new Metadata
			{
				AppId = appId,
				AppName = sAppName,
				AppVerId = appVerId,
				Description = StorageProvider.GetMetadata( "Description" ),
				Name = StorageProvider.GetMetadata( "Name" ),
				Version = StorageProvider.GetMetadata( "Version" ),
				Publisher = StorageProvider.GetMetadata( "Publisher" ),
				PublisherUrl = StorageProvider.GetMetadata( "PublisherUrl" ),
				ReleaseDate = release,
				Dependencies = StorageProvider.GetDependencies( ),
				Type = type,
				PlatformVersion = platformVersion
			};
		}

		/// <summary>
		///     Load relationships.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public IEnumerable<RelationshipEntry> GetRelationships( IProcessingContext context )
		{
			/////
			// Query entities that are part of the solution
			/////
			const string sql = @"select TypeUid, FromUid, ToUid from _Relationship r";

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						Guid? typeId = GetGuid( reader, 0 );

						if ( typeId == null )
						{
							continue;
						}

						Guid? fromId = GetGuid( reader, 1 );

						if ( fromId == null )
						{
							continue;
						}

						Guid? toId = GetGuid( reader, 2 );

						if ( toId == null )
						{
							continue;
						}

						RelationshipEntry entry = new RelationshipEntry( typeId.Value, fromId.Value, toId.Value );

						yield return entry;
					}
				}
			}
		}


        /// <summary>
        ///     Return empty set of SecureData
        /// </summary>
        public IEnumerable<SecureDataEntry> GetSecureData(IProcessingContext context)
        {
            if (!StorageProvider.DoesTableExist("_SecureData"))
            {
                yield break;
            }

            /////
            // Query entities that are part of the solution
            /////
            const string sql = @"select s.SecureId, s.Context, s.Data from _SecureData s";

            using (IDbCommand command = CreateCommand())
            {
                command.CommandText = sql;

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid? secureId = GetGuid(reader, 0);

                        if (secureId == null)
                        {
                            continue;
                        }

                        var secureContext = reader.GetString(1);

                        if (secureContext == null)
                        {
                            continue;
                        }

                        var dataString = reader.GetString(2);

                        if (dataString == null)
                        {
                            continue;
                        }

                        var bytes = Convert.FromBase64String(dataString);
                        
                        var entry = new SecureDataEntry( (Guid) secureId, secureContext, bytes);

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
        public IEnumerable<Guid> GetDoNotRemove( IProcessingContext context )
        {
            return Enumerable.Empty<Guid>( );
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
        /// <returns></returns>
        private static Func<object, object> GetDataConverter( string dataTable )
		{
			Func<object, object> converter;

			switch ( dataTable )
			{
				case "Bit":
					converter = value => value == null ? null : ( object ) ( ( long ) value == 1 );
					break;

				case "Guid":
					converter = value => value == null ? null : ( object ) new Guid( ( string ) value );
					break;
				case "DateTime":
					converter = value =>
					{
						if ( value == null )
						{
							return null;
						}

						DateTime dt;

						if ( DateTime.TryParse( value.ToString( ), null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out dt ) )
						{
							return dt;
						}

						return value;
					};
					break;
				default:
					converter = value => value;
					break;
			}

			return converter;
		}

		/// <summary>
		///     Creates the command.
		/// </summary>
		/// <returns></returns>
		private IDbCommand CreateCommand( )
		{
			return StorageProvider.CreateCommand( );
		}

		/// <summary>
		///     Gets the GUID.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="column">The column.</param>
		/// <returns></returns>
		private Guid? GetGuid( IDataReader reader, int column )
		{
			if ( reader.IsDBNull( column ) )
			{
				return null;
			}

			string guidText = reader.GetString( column );

			if ( guidText.Length == 0 )
			{
				return null;
			}

			Guid guid = Guid.Parse( guidText );

			return guid;
		}


		/// <summary>
		///     Loads the binary data with the given hash.
		/// </summary>
		/// <param name="dataHash">The data hash.</param>
		/// <returns></returns>
		private byte[ ] LoadBinaryData( string dataHash )
		{
			byte[ ] data = null;

			using ( IDbCommand command = CreateCommand( ) )
			{
				const string sql = @"SELECT fb.Data
                                     FROM _Filestream_Binary fb                                     
                                     WHERE fb.DataHash = @dataHash";

				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new SQLiteParameter( "@dataHash", DbType.String ) );
				( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = dataHash;

				using ( var reader = ( SQLiteDataReader ) command.ExecuteReader( ) )
				{
					Debug.Assert( reader != null, "reader != null" );

					if ( reader.Read( ) )
					{
						using ( var memoryStream = new MemoryStream( ) )
						{
							if ( !reader.IsDBNull( 0 ) )
							{
								using ( Stream stream = reader.GetStream( 0 ) )
								{
									stream.CopyTo( memoryStream );
								}

								data = CompressionHelper.Decompress( memoryStream.ToArray( ) );
							}
						}
					}
				}
			}

			return data;
		}

		/// <summary>
		///     Loads the binary data with the given hash.
		/// </summary>
		/// <param name="dataHash">The data hash.</param>
		/// <returns></returns>
		private byte[ ] LoadDocumentData( string dataHash )
		{
			byte[ ] data = null;

			using ( IDbCommand command = CreateCommand( ) )
			{
				const string sql = @"SELECT fb.Data
                                     FROM _Filestream_Document fb                                     
                                     WHERE fb.DataHash = @dataHash";

				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				command.Parameters.Add( new SQLiteParameter( "@dataHash", DbType.String ) );
				( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = dataHash;

				using ( var reader = ( SQLiteDataReader ) command.ExecuteReader( ) )
				{
					Debug.Assert( reader != null, "reader != null" );

					if ( reader.Read( ) )
					{
						using ( var memoryStream = new MemoryStream( ) )
						{
							if ( !reader.IsDBNull( 0 ) )
							{
								using ( Stream stream = reader.GetStream( 0 ) )
								{
									stream.CopyTo( memoryStream );
								}

								data = CompressionHelper.Decompress( memoryStream.ToArray( ) );
							}
						}
					}
				}
			}

			return data;
		}
	}
}