// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Storage;
using System.Security.Cryptography;
using System.Text;

namespace EDC.SoftwarePlatform.Migration.Targets
{
	/// <summary>
	///     Tenant Copy Target.
	/// </summary>
	internal class TenantCopyTarget : SqlBase, IDataTarget
	{
		/// <summary>
		///     The _upgrade automatic unique identifier map
		/// </summary>
		private readonly Dictionary<Guid, long> _upgradeToIdMap = new Dictionary<Guid, long>( );

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantCopyTarget" /> class.
		/// </summary>
		public TenantCopyTarget( )
			: base( true, 600 )
		{
		}

		/// <summary>
		///     Gets or sets the tenant unique identifier.
		/// </summary>
		/// <value>
		///     The tenant unique identifier.
		/// </value>
		public long TenantId
		{
			get;
			set;
		}

		/// <summary>
		///     Set the application metadata.
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="context"></param>
		void IDataTarget.SetMetadata( Metadata metadata, IProcessingContext context )
		{
		}

		/// <summary>
		///     Setups the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.Setup( IProcessingContext context )
		{
			if ( context != null )
			{
				context.WriteInfo( "Initializing..." );
			}

			using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = CommandText.TenantMergeTargetSetupCommandText;
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Tears down.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.TearDown( IProcessingContext context )
		{
			if ( context != null )
			{
				context.WriteInfo( "Finalizing..." );
			}

			using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = CommandText.TenantMergeTargetTearDownCommandText;
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		void IDataTarget.WriteBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{		
			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{				
				new DataColumn( "DataHash", typeof ( string ) )
				{
					AllowDBNull = false
				}
			};

			Func<BinaryDataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
                if (entry.Data == null)
                {                    
                    return PopulateRowResult.InvalidData;
                }

                using (var source = new MemoryStream(entry.Data))
                {
                    try
                    {                        
                        Factory.BinaryFileRepository.Put(source);                        
                    }
                    catch (Exception ex)
                    {
                        context.WriteWarning(string.Format("An error occurred copying binary file into file repository. DataHash: {0}. Error {1}.", entry.DataHash, ex.ToString()));

                        return PopulateRowResult.InvalidData;
                    }
                }                

				return PopulateRowResult.Ignore;
			};			

			var executionArguments = new ExecutionArguments<BinaryDataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Binary",
                SkipCommandExec = true,
                Context = context,
				PopulateRowAction = populateRowAction,				
				ExecuteAction = ExecuteAction.Writing
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		public void WriteDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context )
		{
			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
                new DataColumn( "DataHash", typeof ( string ) )
                {
                    AllowDBNull = false
                }
            };

			Func<DocumentDataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
                if (entry.Data == null)
                {
                    return PopulateRowResult.InvalidData;
                }

                using (var source = new MemoryStream(entry.Data))
                {
                    try
                    {
                        Factory.DocumentFileRepository.Put(source);
                    }
                    catch (Exception ex)
                    {
                        context.WriteWarning(string.Format("An error occurred putting document into file repository. DataHash: {0}. Error {1}.", entry.DataHash, ex.ToString()));

                        return PopulateRowResult.InvalidData;
                    }
                }

                return PopulateRowResult.Ignore;
            };			

			var executionArguments = new ExecutionArguments<DocumentDataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Document",
                SkipCommandExec = true,
                Context = context,
				PopulateRowAction = populateRowAction,				
				ExecuteAction = ExecuteAction.Writing
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Write in collection of entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="context"></param>
		void IDataTarget.WriteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "UpgradeId", typeof ( Guid ) )
				{
					AllowDBNull = false
				}
			};

			Func<EntityEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				row[ 0 ] = entry.EntityId;
				return PopulateRowResult.Success;
			};

			var executionArguments = new ExecutionArguments<EntityEntry>
			{
				Entries = entities,
				GetColumnsAction = getColumnsAction,
				TableName = "Entities",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = CommandText.TenantMergeTargetWriteEntitiesCommandText,
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId )
			};

			Execute( executionArguments );

			PopulateUpgradeIdToIdMap( );
		}

		/// <summary>
		///     Write in collection of field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that data types are converted to their correct internal storage formats (e.g. 1/0 for bits in sqlite)
		///     - ensure that XML is transformed so that entityRefs are remapped to the local ID space. Entities will be received
		///     as either uid or uid|alias
		///     - ensure that aliases import their namespace and direction marker.
		/// </remarks>
		void IDataTarget.WriteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
			if ( dataTable == "Alias" )
			{
				WriteFieldData_Alias( data, context );
				return;
			}

			if ( dataTable == "Xml" )
			{
				WriteFieldData_Xml( data, context );
				return;
			}

			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "EntityId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "FieldId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "Data", Helpers.FieldDataTableTypes[ dataTable ] )
				{
					AllowDBNull = true
				}
			};

			Func<DataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				long val;

				if ( !_upgradeToIdMap.TryGetValue( entry.EntityId, out val ) )
				{
					return PopulateRowResult.MissingEntityDependency;
				}

				row[ 0 ] = val;

				if ( !_upgradeToIdMap.TryGetValue( entry.FieldId, out val ) )
				{
					return PopulateRowResult.MissingFieldDependency;
				}

				row[ 1 ] = val;
				row[ 2 ] = entry.Data;

				return PopulateRowResult.Success;
			};

			var executionArguments = new ExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = dataTable,
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetWriteFieldCommandText, dataTable, string.Empty, string.Empty, string.Empty, string.Empty ),
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId )
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Write in collection of relationships.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IDataTarget.WriteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "TypeId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "FromId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "ToId", typeof ( long ) )
				{
					AllowDBNull = false
				}
			};

			Func<RelationshipEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				long val;

				if ( !_upgradeToIdMap.TryGetValue( entry.TypeId, out val ) )
				{
					return PopulateRowResult.MissingTypeDependency;
				}

				row[ 0 ] = val;

				if ( !_upgradeToIdMap.TryGetValue( entry.FromId, out val ) )
				{
					return PopulateRowResult.MissingFromDependency;
				}

				row[ 1 ] = val;

				if ( !_upgradeToIdMap.TryGetValue( entry.ToId, out val ) )
				{
					return PopulateRowResult.MissingToDependency;
				}

				row[ 2 ] = val;
				return PopulateRowResult.Success;
			};

			var executionArguments = new ExecutionArguments<RelationshipEntry>
			{
				Entries = relationships,
				GetColumnsAction = getColumnsAction,
				TableName = "Relationships",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = CommandText.TenantMergeTargetWriteRelationshipsCommandText,
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId )
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Executes the specified arguments.
		/// </summary>
		/// <typeparam name="TEntry">The type of the entry.</typeparam>
		/// <param name="arguments">The arguments.</param>
		/// <exception cref="System.InvalidOperationException">No active database connection.</exception>
		private void Execute<TEntry>( ExecutionArguments<TEntry> arguments )
		{
			/////
			// Argument checks.
			/////
			if ( arguments == null || arguments.Entries == null || arguments.GetColumnsAction == null || string.IsNullOrEmpty( arguments.TableName ) || arguments.PopulateRowAction == null || (string.IsNullOrEmpty( arguments.CommandText ) && !arguments.SkipCommandExec) )
			{
				return;
			}

			IList<TEntry> entryList = arguments.Entries as IList<TEntry> ?? arguments.Entries.ToList( );

			/////
			// Early out.
			/////
			if ( entryList.Count <= 0 )
			{
				return;
			}

			var connection = DatabaseContext.GetUnderlyingConnection( ) as SqlConnection;

			if ( connection == null )
			{
				throw new InvalidOperationException( "No active database connection." );
			}

			var table = new DataTable( );

			/////
			// Populate the columns.
			/////
			table.Columns.AddRange( arguments.GetColumnsAction( ) );

			/////
			// Populate the rows.
			/////
			foreach ( TEntry entry in entryList )
			{
				DataRow row = table.NewRow( );

				if ( arguments.PopulateRowAction( entry, row ) != PopulateRowResult.Success )
				{
					continue;
				}

				table.Rows.Add( row );
			}

			if ( table.Rows.Count > 0 && !arguments.SkipCommandExec )
			{
				using ( IDbCommand command = CreateCommand( ) )
				{
					/////
					// Clear existing data.
					/////
					command.CommandText = string.Format( "TRUNCATE TABLE #{0}", arguments.TableName );
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery( );

					int rowsCopied;

					/////
					// Bulk load into the staging table.
					/////
					using ( var bulkCopy = new SqlBulkCopy( connection ) )
					{
						bulkCopy.BulkCopyTimeout = 600;
						bulkCopy.NotifyAfter = 100;

						if ( arguments.Context != null )
						{
							bulkCopy.SqlRowsCopied += ( sender, args ) => arguments.Context.WriteProgress( string.Format( "{0} {1} data... {2} rows", arguments.ExecuteAction, arguments.TableName, args.RowsCopied ) );
						}

						bulkCopy.DestinationTableName = string.Format( "#{0}", arguments.TableName );
						bulkCopy.WriteToServer( table );

						rowsCopied = bulkCopy.RowsCopiedCount( );
					}

					if ( arguments.SetCopiedCountAction != null )
					{
						arguments.SetCopiedCountAction( rowsCopied );
					}

					command.CommandText = arguments.CommandText;
					command.CommandType = CommandType.Text;

					/////
					// Additional command setup.
					/////
					if ( arguments.SetupCommandAction != null )
					{
						arguments.SetupCommandAction( command );
					}

					if ( arguments.Context != null )
					{
						arguments.Context.WriteInfo( string.Format( "Committing {0} data...", arguments.TableName ) );
					}

					int executeRows = arguments.CustomCommandExecuteAction != null ? arguments.CustomCommandExecuteAction( command ) : command.ExecuteNonQuery( );

					if ( arguments.SetExecuteCountAction != null )
					{
						arguments.SetExecuteCountAction( executeRows );
					}
				}
			}
		}

		/// <summary>
		///     Populates the upgrade unique identifier automatic unique identifier map.
		/// </summary>
		private void PopulateUpgradeIdToIdMap( )
		{
			_upgradeToIdMap.Clear( );

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = CommandText.PopulateUpgradeMapCommandText;
				command.CommandType = CommandType.Text;
				command.AddParameterWithValue( "@tenant", TenantId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						_upgradeToIdMap[ reader.GetGuid( 0 ) ] = reader.GetInt64( 1 );
					}
				}
			}
		}

		/// <summary>
		///     Writes the field data for the alias table.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		private void WriteFieldData_Alias( IEnumerable<DataEntry> data, IProcessingContext context )
		{
			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "EntityId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "FieldId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "Data", Helpers.FieldDataTableTypes[ "Alias" ] )
				{
					AllowDBNull = true
				},
				new DataColumn( "Namespace", typeof ( string ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "AliasMarkerId", typeof ( int ) )
				{
					AllowDBNull = false
				}
			};

			Func<DataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				long val;

				if ( !_upgradeToIdMap.TryGetValue( entry.EntityId, out val ) )
				{
					return PopulateRowResult.MissingEntityDependency;
				}

				row[ 0 ] = val;

				if ( !_upgradeToIdMap.TryGetValue( entry.FieldId, out val ) )
				{
					return PopulateRowResult.MissingFieldDependency;
				}

				row[ 1 ] = val;
				row[ 2 ] = entry.Data;
				row[ 3 ] = entry.Namespace;
				row[ 4 ] = entry.AliasMarkerId;

				return PopulateRowResult.Success;
			};

			var executionArguments = new ExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Alias",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetWriteFieldCommandText, "Alias", ", Namespace, AliasMarkerId", ", d.Namespace, d.AliasMarkerId", ", o.Namespace = d.Namespace, o.AliasMarkerId = d.AliasMarkerId", " OR d.Namespace <> o.Namespace OR d.AliasMarkerId <> o.AliasMarkerId" ),
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId )
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Writes the field data for the XML table.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		private void WriteFieldData_Xml( IEnumerable<DataEntry> data, IProcessingContext context )
		{
			/////
			// Use Xml processor to remap EntityRefs located in XML
			/////
			var xmlProcessor = new XmlFieldProcessor( null, null, _upgradeToIdMap )
			{
				ConversionMode = XmlConversionMode.UpgradeGuidToLocalId,
				TenantId = TenantId,
				DatabaseContext = DatabaseContext,
				ProcessingContext = context
			};

			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "EntityId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "FieldId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "Data", Helpers.FieldDataTableTypes[ "Xml" ] )
				{
					AllowDBNull = true
				}
			};

			Func<DataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				long val;

				if ( !_upgradeToIdMap.TryGetValue( entry.EntityId, out val ) )
				{
					return PopulateRowResult.MissingEntityDependency;
				}

				row[ 0 ] = val;

				if ( !_upgradeToIdMap.TryGetValue( entry.FieldId, out val ) )
				{
					return PopulateRowResult.MissingFieldDependency;
				}

				row[ 1 ] = val;
				row[ 2 ] = entry.Data == null ? DBNull.Value : ( object ) xmlProcessor.RemapXmlEntities( entry.EntityId, entry.FieldId, ( string ) entry.Data );

				return PopulateRowResult.Success;
			};

			var executionArguments = new ExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Xml",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetWriteFieldCommandText, "Xml", string.Empty, string.Empty, string.Empty, string.Empty ),
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId )
			};

			Execute( executionArguments );
		}


        void IDataTarget.WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context)
        { 
            Func<DataColumn[]> getColumnsAction = () => new[]
           {
                new DataColumn( "SecureId", typeof ( Guid ) )
                {
                    AllowDBNull = false
                },
                new DataColumn( "Context", typeof ( string ) )
                {
                    AllowDBNull = false
                },
                new DataColumn( "Data", typeof( string) )
                {
                    AllowDBNull = true
                }
            };

            Func<SecureDataEntry, DataRow, PopulateRowResult> populateRowAction = (entry, row) =>
            {
                row[0] = entry.SecureId;
                row[1] = entry.Context;
                row[2] = DecryptString(entry.Data);
                
                return PopulateRowResult.Success;
            };

            var executionArguments = new ExecutionArguments<SecureDataEntry>
            {
                Entries = data,
                GetColumnsAction = getColumnsAction,
                TableName = "Secured",
                Context = context,
                PopulateRowAction = populateRowAction,
                CommandText = string.Format(CommandText.TenantMergeTargetWriteSecuredCommandText, "Secured"),
                ExecuteAction = ExecuteAction.Writing,
                SetupCommandAction = c => c.AddParameterWithValue("@tenant", TenantId)
            };

            Execute(executionArguments);
        }


        void IDataTarget.WriteDoNotRemove(IEnumerable<Guid> data, IProcessingContext context)
        {
            // do nothing
        }

        /// <summary>
        /// Encrypt a string with DPAPI
        /// </summary>
        string DecryptString(Byte[] bytes)
        {
            var decryptedBytes = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}