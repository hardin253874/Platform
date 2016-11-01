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
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Storage;

namespace EDC.SoftwarePlatform.Migration.Targets
{
	/// <summary>
	///     Tenant repair target.
	/// </summary>
	internal class TenantRepairTarget : SqlBase, IMergeTarget
	{
		/// <summary>
		///     The _upgrade automatic unique identifier map
		/// </summary>
		private readonly Dictionary<Guid, long> _upgradeToIdMap = new Dictionary<Guid, long>( );

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantMergeTarget" /> class.
		/// </summary>
		public TenantRepairTarget( )
			: base( true, 600 )
		{
		}

		/// <summary>
		///     Gets or sets the application unique identifier.
		/// </summary>
		/// <value>
		///     The application unique identifier.
		/// </value>
		public Guid ApplicationId
		{
			get;
			set;
		}

		/// <summary>
		///     The tenant to merge data into.
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
		///     Deletes binary data entries.
		/// </summary>
		/// <param name="binaryData"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteBinaryData( IEnumerable<BinaryDataEntry> binaryData, IProcessingContext context )
		{
		}

		public void DeleteDocumentData( IEnumerable<DocumentDataEntry> binaryData, IProcessingContext context )
		{
		}

		/// <summary>
		///     Deletes entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
			/////
			// Populate the UpgradeId to Id map.
			/////
			PopulateUpgradeIdToIdMap( );
		}

		/// <summary>
		///     Deletes field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Deletes relationships.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
		}

		/// <summary>
		///     Called for binary data with new values.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="context"></param>
		void IMergeTarget.UpdateBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{
		}

		public void UpdateDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Called for field data with new values.
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		void IMergeTarget.UpdateFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Relationships *may* change if their EntityId has been changed (although this is unlikely in practice)
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IMergeTarget.UpdateRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
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
			context?.WriteInfo( "Initializing..." );

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = CommandText.TenantRepairTargetSetupCommandText;
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );

				command.CommandText = @"
IF ( @context IS NOT NULL )
BEGIN
	DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
	SET CONTEXT_INFO @contextInfo
END";
				command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ), 128 );
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Tears down.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.TearDown( IProcessingContext context )
		{
			context?.WriteInfo( "Finalizing..." );

			using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = CommandText.TenantRepairTargetTearDownCommandText;
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
            Func<DataColumn[]> getColumnsAction = () => new[]
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
                        context.WriteWarning( $"An error occurred putting binary file into file repository. DataHash: {entry.DataHash}. Error {ex.ToString( )}." );

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
				Context = context,
				PopulateRowAction = populateRowAction,
                SkipCommandExec = true,
                ExecuteAction = ExecuteAction.Writing
			};

			Execute( executionArguments );
		}

		public void WriteDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context )
		{
            Func<DataColumn[]> getColumnsAction = () => new[]
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
                        context.WriteWarning( $"An error occurred putting document into file repository. DataHash: {entry.DataHash}. Error {ex.ToString( )}." );

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
				Context = context,
				PopulateRowAction = populateRowAction,
				SkipCommandExec = true,
				ExecuteAction = ExecuteAction.Writing,
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Write in collection of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="context">The context.</param>
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

			/////
			// Populate the UpgradeId to Id map.
			/////
			PopulateUpgradeIdToIdMap( );
		}

		/// <summary>
		///     Writes the field data.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
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
				CommandText = string.Format( CommandText.TenantRepairTargetWriteFieldCommandText, dataTable ),
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
		///     Executes the specified entries.
		/// </summary>
		/// <typeparam name="TEntry">The type of the entry.</typeparam>
		/// <param name="arguments">The arguments.</param>
		/// <exception cref="System.InvalidOperationException">No active database connection.</exception>
		//private void Execute<TEntry>( IEnumerable<TEntry> entries, Func<DataColumn [ ]> getColumnsAction, string tableName, IProcessingContext context, Func<TEntry, DataRow, PopulateRowResult> populateRowAction, string commandText, ExecuteAction executeAction, Action<IDbCommand> setupCommandAction = null, Action<IDbCommand> customCommandExecuteAction = null )
		private void Execute<TEntry>( ExecutionArguments<TEntry> arguments )
		{
			/////
			// Argument checks.
			/////
			if ( arguments?.Entries == null || arguments.GetColumnsAction == null || string.IsNullOrEmpty( arguments.TableName ) || arguments.PopulateRowAction == null || (string.IsNullOrEmpty( arguments.CommandText ) && !arguments.SkipCommandExec) )
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

			if ( table.Rows.Count > 0 && !arguments.SkipCommandExec)
			{
				using ( IDbCommand command = CreateCommand( ) )
				{
					/////
					// Clear existing data.
					/////
					command.CommandText = $"TRUNCATE TABLE #{arguments.TableName}";
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery( );

					/////
					// Bulk load into the staging table.
					/////
					using ( var bulkCopy = new SqlBulkCopy( connection ) )
					{
						bulkCopy.BulkCopyTimeout = 600;
						bulkCopy.NotifyAfter = 100;

						if ( arguments.Context != null )
						{
							bulkCopy.SqlRowsCopied += ( sender, args ) => arguments.Context.WriteProgress( $"{arguments.ExecuteAction} {arguments.TableName} data... {args.RowsCopied} rows" );
						}

						bulkCopy.DestinationTableName = $"#{arguments.TableName}";
						bulkCopy.WriteToServer( table );
					}

					command.CommandText = arguments.CommandText;
					command.CommandType = CommandType.Text;

					/////
					// Additional command setup.
					/////
					arguments.SetupCommandAction?.Invoke( command );

					arguments.Context?.WriteInfo( $"Committing {arguments.TableName} data..." );

					if ( arguments.CustomCommandExecuteAction != null )
					{
						arguments.CustomCommandExecuteAction( command );
					}
					else
					{
						command.ExecuteNonQuery( );
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


		/// <summary>
		/// Method executed prior to entity/relationship deletion.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="context">The context.</param>
		public void PreDeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
		}


        void IDataTarget.WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context)
        {
            // do nothing
        }

        /// <summary>
        ///     Write list of entities that should not be removed during upgrade operations.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        void IDataTarget.WriteDoNotRemove( IEnumerable<Guid> entities, IProcessingContext context )
        {
            // do nothing
        }
    }
}