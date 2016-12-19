// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Storage;
using System.Text;
using System.Security.Cryptography;

namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Tenant source - the entire contents of a tenant.
	/// </summary>
	internal class TenantSource : SqlBase, IDataSource
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
		///     Gets or sets the name of the tenant.
		/// </summary>
		/// <value>
		///     The name of the tenant.
		/// </value>
		public string TenantName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the binary data.
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
					command.CommandType = CommandType.Text;
				    command.CommandText = CommandText.TenantSourceGetBinaryDataCommandText;
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
					command.CommandType = CommandType.Text;
				    command.CommandText = CommandText.TenantSourceGetDocumentDataCommandText;
                    command.AddParameterWithValue( "@tenant", TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var documentDataEntry = new DocumentDataEntry
							{
								DataHash = reader.GetString( 0 ),								
								LoadDataCallback = loadDocumentData
                            };

							data.Add( documentDataEntry );
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
					command.CommandText = CommandText.TenantSourceGetEntitiesCommandText;
					command.CommandType = CommandType.Text;
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
		///     - ensure that XML is transformed so that entityRefs contain Upgrade ids
		///     - or where XML contains an alias, translate it to uprgadeId|alias   (as the alias may be changed in the target)
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
					command.CommandText = string.Format( CommandText.TenantSourceGetFieldDataCommandText, dataTable, extraSql );
					command.CommandType = CommandType.Text;
					command.AddParameterWithValue( "@tenant", TenantId );

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
		/// <param name="context"></param>
		/// <returns></returns>
		Metadata IDataSource.GetMetadata( IProcessingContext context )
		{
			return new Metadata
			{
				Name = TenantName,
				Type = SourceType.Tenant,
				PlatformVersion = SystemInfo.PlatformVersion
			};
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
				// Query entities that are part of the solution
				/////
				using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandText = CommandText.TenantSourceGetRelationshipsCommandText;
					command.CommandType = CommandType.Text;
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

							Guid typeId = reader.GetGuid( 0 );
							Guid fromId = reader.GetGuid( 1 );
							Guid toId = reader.GetGuid( 2 );

							RelationshipEntry entry = new RelationshipEntry( typeId, fromId, toId );

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
		/// <param name="context">The context.</param>
		void IDataSource.Setup( IProcessingContext context )
		{
		}

		/// <summary>
		///     Tears down this instance.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataSource.TearDown( IProcessingContext context )
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
        ///     Return the decrypted secure data stored with the tenant.
        /// </summary>
        public IEnumerable<SecureDataEntry> GetSecureData(IProcessingContext context)
        {
            var result = new List<SecureDataEntry>();

            /////
            // Query entities that are part of the solution
            /////
            using (IDbCommand command = CreateCommand())
            {
                command.CommandText = "spSecuredDataReadTenant";
                command.CommandType = CommandType.StoredProcedure;
                command.AddParameterWithValue("@tenantId", TenantId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0))
                        {
                            context?.WriteWarning("Unexpected null SecureDataEntry.");

                            continue;
                        }

                        var secureId = reader.GetGuid(0);
                        var secureContext = reader.GetString(1);
                        var data = reader.GetString(2);

                        var entry = new SecureDataEntry(secureId, secureContext, EncryptString(data));

                        result.Add(entry);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Encrypt a string with DPAPI
        /// </summary>
        Byte[] EncryptString(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
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