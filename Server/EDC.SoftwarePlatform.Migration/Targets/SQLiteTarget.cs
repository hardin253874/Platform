// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using EDC.IO;
using EDC.Security;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Sources;
using EDC.SoftwarePlatform.Migration.Storage;
using System.Text;

namespace EDC.SoftwarePlatform.Migration.Targets
{
	/// <summary>
	///     Writes data for one application into the application library.
	/// </summary>
	internal class SqLiteTarget : IDataTarget
	{
		/// <summary>
		///     Database to write the data to.
		/// </summary>
		public SqliteStorageProvider StorageProvider
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the version override.
		/// </summary>
		/// <value>
		///     The version override.
		/// </value>
		public string VersionOverride
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
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		void IDataTarget.WriteBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{
			int rowCount = 0;

			using ( var command = CreateCommand( ) as SQLiteCommand )
			{
				Debug.Assert( command != null, "command != null" );

				const string sql = "insert into _Filestream_Binary (FileExtension, DataHash, Data) values (@fileExtension, @dataHash, @data)";

				command.CommandType = CommandType.Text;
				command.CommandText = sql;                

                foreach ( BinaryDataEntry dataEntry in data )
				{
					if ( dataEntry.Data == null )
					{
						context.WriteWarning( $"Unexpected null values when updating binary table. DataHash:{dataEntry.DataHash}" );
						continue;
					}
                    
                    if (!IsBinaryDataValid(dataEntry.Data, dataEntry.DataHash))
                    {
                        context.WriteWarning( $"The binary data is corrupt. It will be skipped. The data hash does not match the expected value. DataHash:{dataEntry.DataHash}" );
                        continue;
                    }

                    command.Parameters.Clear( );

					command.Parameters.Add( "@fileExtension", DbType.String ).Value = dataEntry.FileExtension;
					command.Parameters.Add( "@dataHash", DbType.String ).Value = dataEntry.DataHash;					

					byte[] compressedData = CompressionHelper.Compress( dataEntry.Data );
					command.Parameters.Add( "@data", DbType.Binary, compressedData.Length ).Value = compressedData;

					rowCount += command.ExecuteNonQuery( );

					if ( rowCount % 100 == 0 )
					{
						context.WriteInfo( $"Copying binary data... {rowCount} rows" );
					}
				}
			}
		}

        public void WriteDocumentData(IEnumerable<DocumentDataEntry> data, IProcessingContext context)
	    {
            int rowCount = 0;

            using (var command = CreateCommand() as SQLiteCommand)
            {
                Debug.Assert(command != null, "command != null");

                const string sql = "insert into _Filestream_Document (FileExtension, DataHash, Data) values (@fileExtension, @dataHash, @data)";

                command.CommandType = CommandType.Text;
                command.CommandText = sql;                

                foreach (DocumentDataEntry dataEntry in data)
                {
                    if (dataEntry.Data == null)
                    {
                        context.WriteWarning( $"Unexpected null values when updating binary table. DataHash:{dataEntry.DataHash}" );
                        continue;
                    }

                    if (!IsBinaryDataValid(dataEntry.Data, dataEntry.DataHash))
                    {
                        context.WriteWarning( $"The document data is corrupt. It will be skipped. The data hash does not match the expected value. DataHash:{dataEntry.DataHash}" );
                        continue;
                    }

                    command.Parameters.Clear();

                    command.Parameters.Add("@fileExtension", DbType.String).Value = dataEntry.FileExtension;
                    command.Parameters.Add("@dataHash", DbType.String).Value = dataEntry.DataHash;

                    byte[] compressedData = CompressionHelper.Compress(dataEntry.Data);
                    command.Parameters.Add("@data", DbType.Binary, compressedData.Length).Value = compressedData;

                    rowCount += command.ExecuteNonQuery();

                    if (rowCount % 100 == 0)
                    {
                        context.WriteInfo( $"Copying document data... {rowCount} rows" );
                    }
                }
            }
        }

	    /// <summary>
		///     Write in collection of entities.
		/// </summary>
		void IDataTarget.WriteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
			using ( IDbCommand command = CreateCommand( ) )
			{
				const string sql = "insert into _Entity (Uid) values (@entityUid)";
				command.CommandText = sql;
				command.Parameters.Add( new SQLiteParameter( "@entityUid", DbType.String ) );

				int rowCount = 0;

				foreach ( EntityEntry entity in entities )
				{
					( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = entity.EntityId;
					rowCount += command.ExecuteNonQuery( );

					if ( rowCount % 100 == 0 )
					{
						context.WriteInfo( $"Copying Entity data... {rowCount} rows" );
					}
				}
			}
		}

		/// <summary>
		///     Write in collection of field data.
		/// </summary>
		void IDataTarget.WriteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
			bool isAliasTable = dataTable == "Alias";

			// Converter for 'Bit' data to convert true/false to 1/0
			Func<object, object> converter = GetDataConverter( dataTable );

			using ( IDbCommand command = CreateCommand( ) )
			{
				string sql = "insert into _Data_" + dataTable + " (EntityUid, FieldUid, Data) values (@entity, @field, @data)";

				command.Parameters.Add( new SQLiteParameter( "@entity", DbType.String ) );
				command.Parameters.Add( new SQLiteParameter( "@field", DbType.String ) );
				command.Parameters.Add( new SQLiteParameter( "@data", DbType.String ) );

				if ( isAliasTable )
				{
					sql = "insert into _Data_" + dataTable + " (EntityUid, FieldUid, Data, [Namespace], AliasMarkerId) values (@entity, @field, @data, @namespace, @marker)";
					command.Parameters.Add( new SQLiteParameter( "@namespace", DbType.String ) );
					command.Parameters.Add( new SQLiteParameter( "@marker", DbType.Int32 ) );
				}
				command.CommandText = sql;

				int rowCount = 0;

				foreach ( DataEntry dataEntry in data )
				{
					( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = dataEntry.EntityId;
					( ( SQLiteParameter ) command.Parameters[ 1 ] ).Value = dataEntry.FieldId;
					( ( SQLiteParameter ) command.Parameters[ 2 ] ).Value = converter( dataEntry.Data );
					if ( isAliasTable )
					{
						( ( SQLiteParameter ) command.Parameters[ 3 ] ).Value = dataEntry.Namespace;
						( ( SQLiteParameter ) command.Parameters[ 4 ] ).Value = dataEntry.AliasMarkerId;
					}
					rowCount += command.ExecuteNonQuery( );

					if ( rowCount % 100 == 0 )
					{
						context.WriteInfo( $"Copying '{dataTable}' Field data... {rowCount} rows" );
					}
				}
			}
		}

		/// <summary>
		///     Write in collection of relationships.
		/// </summary>
		void IDataTarget.WriteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
			using ( IDbCommand command = CreateCommand( ) )
			{
				const string sql = "insert into _Relationship (FromUid, ToUid, TypeUid, EntityUid) values (@from, @to, @type, @entity)";
				command.CommandText = sql;
				command.Parameters.Add( new SQLiteParameter( "@from", DbType.String ) );
				command.Parameters.Add( new SQLiteParameter( "@to", DbType.String ) );
				command.Parameters.Add( new SQLiteParameter( "@type", DbType.String ) );
				command.Parameters.Add( new SQLiteParameter( "@entity", DbType.String ) );

				int rowCount = 0;

				foreach ( RelationshipEntry relationship in relationships )
				{
					( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = relationship.FromId;
					( ( SQLiteParameter ) command.Parameters[ 1 ] ).Value = relationship.ToId;
					( ( SQLiteParameter ) command.Parameters[ 2 ] ).Value = relationship.TypeId;

					rowCount += command.ExecuteNonQuery( );

					if ( rowCount % 100 == 0 )
					{
						context.WriteInfo( $"Copying Relationship data... {rowCount} rows" );
					}
				}
			}
		}

		/// <summary>
		///     Set the application metadata.
		/// </summary>
		void IDataTarget.SetMetadata( Metadata metadata, IProcessingContext context )
		{
			if ( !string.IsNullOrEmpty( metadata.Name ) )
			{
				StorageProvider.SetMetadata( "Name", metadata.Name );
			}

			if ( !string.IsNullOrEmpty( metadata.Description ) )
			{
				StorageProvider.SetMetadata( "Description", metadata.Description );
			}

			if ( !string.IsNullOrEmpty( metadata.AppName ) )
			{
				StorageProvider.SetMetadata( "AppName", metadata.AppName );
			}

			if ( metadata.AppVerId != Guid.Empty )
			{
				StorageProvider.SetMetadata( "AppVerId", metadata.AppVerId.ToString( ) );
			}

			if ( metadata.AppId != Guid.Empty )
			{
				StorageProvider.SetMetadata( "AppId", metadata.AppId.ToString( ) );
			}

			if ( ! string.IsNullOrEmpty( VersionOverride ) || ! string.IsNullOrEmpty( metadata.Version ) )
			{
				StorageProvider.SetMetadata( "Version", VersionOverride ?? metadata.Version );
			}

			if ( !string.IsNullOrEmpty( metadata.Publisher ) )
			{
				StorageProvider.SetMetadata( "Publisher", metadata.Publisher );
			}

			if ( !string.IsNullOrEmpty( metadata.PublisherUrl ) )
			{
				StorageProvider.SetMetadata( "PublisherUrl", metadata.PublisherUrl );
			}

			if ( metadata.ReleaseDate != DateTime.MinValue )
			{
				StorageProvider.SetMetadata( "ReleaseDate", metadata.ReleaseDate.ToString( CultureInfo.InvariantCulture ) );
			}

			if ( metadata.Dependencies != null && metadata.Dependencies.Count > 0 )
			{
				StorageProvider.SetDependencies( metadata.Dependencies );
			}

			if ( metadata.Type != SourceType.Unknown )
			{
				StorageProvider.SetMetadata( "Type", metadata.Type.ToString( ) );
			}

			if ( !string.IsNullOrEmpty( metadata.PlatformVersion ) )
			{
				StorageProvider.SetMetadata( "PlatformVersion", metadata.PlatformVersion );
			}
		}

		/// <summary>
		///     Setups the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.Setup( IProcessingContext context )
		{
		}

		/// <summary>
		///     Tears down.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.TearDown( IProcessingContext context )
		{
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
		///     Create a delegate to convert data before inserting into the database.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <returns></returns>
		private static Func<object, object> GetDataConverter( string dataTable )
		{
			Func<object, object> converter;

			switch ( dataTable )
			{
				case "Bit":
					converter = value => value == null ? ( object ) null : bool.Parse( value.ToString( ) ) ? 1 : 0;
					break;
				case "DateTime":
					converter = value =>
						{
							if ( value == null )
							{
								return null;
							}

							DateTime? dt = null;

							if ( value is DateTime )
							{
								dt = ( DateTime ) value;
							}
							else
							{
								DateTime nonNullDateTime;

								if ( DateTime.TryParse( value.ToString( ), out nonNullDateTime ) )
								{
									dt = nonNullDateTime;
								}
							}


							return dt?.ToString( @"yyyy-MM-dd HH\:mm\:ss", CultureInfo.InvariantCulture );
						};
					break;
				default:
					converter = value => value;
					break;
			}

			return converter;
		}


	    private bool IsBinaryDataValid(byte[] data, string dataHash)
	    {
            var tokenProvider = new Sha256FileTokenProvider();

            using (var source = new MemoryStream(data))
            {
                string newDataHash = tokenProvider.ComputeToken(source);

                if (newDataHash != dataHash)
                {
                    // Try older versions
                    newDataHash = CryptoHelper.ComputeSha1Hash(source);
                    if (newDataHash != dataHash)
                    {
                        // Might be a sha256 hash
                        newDataHash = CryptoHelper.ComputeSha256Hash(source);
                        if (newDataHash != dataHash)
                        {
                            return false;
                        }
                    }
                }
            }

	        return true;
	    }


        void IDataTarget.WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context)
        {
            int rowCount = 0;

            using (var command = CreateCommand() as SQLiteCommand)
            {
                Debug.Assert(command != null, "command != null");

                const string sql = "insert into _SecureData (SecureId, Context, Data) values (@secureId, @context, @data)";

                command.CommandType = CommandType.Text;
                command.CommandText = sql;

                foreach (SecureDataEntry dataEntry in data)
                {
                    if (dataEntry.Data == null)
                    {
                        context.WriteWarning($"Unexpected null values when updating SecureData table. SecuredId:{dataEntry.SecureId}");
                        continue;
                    }

                    command.Parameters.Clear();

                    command.Parameters.Add("@secureId", DbType.String).Value = dataEntry.SecureId;
                    command.Parameters.Add("@context", DbType.AnsiString).Value = dataEntry.Context;

                    var encodedData = Convert.ToBase64String(dataEntry.Data);    // encrypted so no point in compressing

                    command.Parameters.Add("@data", DbType.String).Value = encodedData;

                    rowCount += command.ExecuteNonQuery();

                    if (rowCount % 100 == 0)
                    {
                        context.WriteInfo($"Copying SecureData... {rowCount} rows");
                    }
                }
            }
        }

        /// <summary>
        ///     Write list of entities that should not be removed during upgrade operations.
        /// </summary>
        void IDataTarget.WriteDoNotRemove( IEnumerable<Guid> data, IProcessingContext context )
        {
            // Currently unsupported
        }

    }
}