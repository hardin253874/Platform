// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Storage;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Migration.Targets
{
	/// <summary>
	///     Tenant merge target.
	/// </summary>
	internal class TenantMergeTarget : SqlBase, IMergeTarget
	{
		/// <summary>
		///     The externally referenced entities
		/// </summary>
		private readonly HashSet<long> _externallyReferencedEntities = new HashSet<long>( );

		/// <summary>
		///     The _upgrade automatic unique identifier map
		/// </summary>
		private readonly Dictionary<Guid, long> _upgradeToIdMap = new Dictionary<Guid, long>( );

		/// <summary>
		///		The items to keep (temporarily at least)
		/// </summary>
		private readonly IDictionary<long, ISet<Guid>> _itemsToKeep;

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantMergeTarget" /> class.
		/// </summary>
		public TenantMergeTarget( )
			: base( true, 600 )
		{
			/////
			// TODO: The following guids are NOT to be deleted. Once all machines are running a post 2.79 build, this can be removed
			/////
			Guid applicationIdGuid = new Guid( "3d6b891f-6f7c-45ab-9cae-503c5de79972" );
			Guid releaseDateGuid = new Guid( "3175c598-b1ca-4cbf-b852-6787ab6fdab1" );
			Guid publishDateGuid = new Guid( "d49d2da3-2659-4113-b657-c6cd19dd72dc" );
			Guid publisherGuid = new Guid( "71942d98-75f3-4b8e-b542-ba1aa067ad4e" );
			Guid publisherUrlGuid = new Guid( "7647f57c-d473-4722-a76a-f2b2dc30e1d5" );
			Guid appVerIdGuid = new Guid( "6efb1485-9862-4630-bda9-3c34ec4cd091" );
			Guid appVersionStringGuid = new Guid( "aad0f45f-8c60-47c4-9b13-2f7158cc6078" );

			HashSet<Guid> globalTenantGuids = new HashSet<Guid>
			{
				applicationIdGuid,
				releaseDateGuid,
				publishDateGuid,
				publisherGuid,
				publisherUrlGuid,
				appVerIdGuid,
				appVersionStringGuid
			};

			_itemsToKeep = new Dictionary<long, ISet<Guid>>
			{
				[0] = globalTenantGuids
			};

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
		///     Gets or sets the cardinality upgrade identifier.
		/// </summary>
		/// <value>
		///     The cardinality identifier.
		/// </value>
		private Guid CardinalityId
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
			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "DataHash", typeof ( string ) )
				{
					AllowDBNull = false
				}
			};

            ISet<string> existingDataHashes = LoadExistingFileDataHashes();

            Func<BinaryDataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
                if (!existingDataHashes.Contains(entry.DataHash.ToUpperInvariant()))
			    {
                    Factory.BinaryFileRepository.Delete(entry.DataHash);
                }                

				return PopulateRowResult.Ignore;
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<BinaryDataEntry>
			{
				Entries = binaryData,
				GetColumnsAction = getColumnsAction,
				TableName = "Binary",
                SkipCommandExec = true,
				Context = context,
				PopulateRowAction = populateRowAction,
				ExecuteAction = ExecuteAction.Deleting,
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Binary Rows for Delete", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Binary Database Rows (Delete)", count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );
		}

		public void DeleteDocumentData( IEnumerable<DocumentDataEntry> documentData, IProcessingContext context )
		{
			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "DataHash", typeof ( string ) )
				{
					AllowDBNull = false
				}
			};

            ISet<string> existingDataHashes = LoadExistingFileDataHashes();

            Func<DocumentDataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
                if (!existingDataHashes.Contains(entry.DataHash.ToUpperInvariant()))
                {
                    Factory.DocumentFileRepository.Delete(entry.DataHash);
                }                

                return PopulateRowResult.Ignore;
            };
			
			var executionArguments = new TenantMergeTargetExecutionArguments<DocumentDataEntry>
			{
				Entries = documentData,
				GetColumnsAction = getColumnsAction,
				TableName = "Document",
                SkipCommandExec = true,
                Context = context,
				PopulateRowAction = populateRowAction,
				ExecuteAction = ExecuteAction.Deleting,
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Document Rows for Delete", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Document Database Rows (Delete)", count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Deletes entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
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

			Action<IDbCommand> setupCommandAction = command =>
			{
				command.AddParameterWithValue( "@batchId", GetBatchId( ) );
				command.AddParameterWithValue( "@tenant", TenantId );
				command.AddParameterWithValue( "@applicationId", ApplicationId );

				/////
				// Create the table here even if there are no uses for it.
				/////
                using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
                using ( IDbCommand createTableCommand = CreateCommand( ) )
				{
					createTableCommand.CommandText = "CREATE TABLE #externallyReferencedEntities ( Id BIGINT )";
					createTableCommand.CommandType = CommandType.Text;
					createTableCommand.ExecuteNonQuery( );
				}

				if ( _externallyReferencedEntities != null && _externallyReferencedEntities.Count > 0 )
				{
					BulkLoadExternallyReferencedEntities( );
				}
			};

			Func<IDbCommand, int> customCommandExecuteAction = command =>
			{
				int rowsDeleted = 0;

				/////
				// Get all the externally referenced entities.
				/////
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					if ( reader.Read( ) )
					{
						rowsDeleted = reader.GetInt32( 0 );
					}
				}

				return rowsDeleted;
			};

			Func<EntityEntry, bool> debugCallback = item =>
			{
				ISet<Guid> tenantItemsToKeep;

				if ( item != null && _itemsToKeep != null && _itemsToKeep.TryGetValue( TenantId, out tenantItemsToKeep ) )
				{
					if ( tenantItemsToKeep != null && tenantItemsToKeep.Contains( item.EntityId ) )
					{
						return true;
					}
				}

				return false;
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<EntityEntry>
			{
				Entries = entities,
				GetColumnsAction = getColumnsAction,
				TableName = "Entities",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = CommandText.TenantMergeTargetDeleteEntitiesCommandText,
				ExecuteAction = ExecuteAction.Deleting,
				SetupCommandAction = setupCommandAction,
				DebugCallback = debugCallback,
				CustomCommandExecuteAction = customCommandExecuteAction,
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Entity Rows for Delete", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Entity Database Rows (Delete)", count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );

			/////
			// Populate the UpgradeId to Id map.
			/////
			PopulateUpgradeIdToIdMap( );
		}

		/// <summary>
		/// Bulk loads the externally referenced entities.
		/// </summary>
		private void BulkLoadExternallyReferencedEntities( )
		{
			var table = new DataTable( );

			table.Columns.Add( new DataColumn( "Id", typeof( Int64 ) )
			{
				AllowDBNull = false
			} );

			foreach ( long id in _externallyReferencedEntities )
			{
				DataRow dataRow = table.NewRow( );
				dataRow [ 0 ] = id;

				table.Rows.Add( dataRow );
			}

			var connection = DatabaseContext.GetUnderlyingConnection( ) as SqlConnection;

			if ( connection == null )
			{
				return;
			}

			using ( var bulkCopy = new SqlBulkCopy( connection ) )
			{
				bulkCopy.BulkCopyTimeout = 600;

				bulkCopy.DestinationTableName = "#externallyReferencedEntities";
				bulkCopy.WriteToServer( table );
			}
		}

		/// <summary>
		///     Deletes field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
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
				new DataColumn( "Data", Helpers.FieldDataTableTypes[ dataTable ] )
				{
					AllowDBNull = true
				}
			};

			Func<DataEntry, DataRow, PopulateRowResult> populateRowAction = ( e, r ) =>
			{
				long val;

				if ( !_upgradeToIdMap.TryGetValue( e.EntityId, out val ) )
				{
					return PopulateRowResult.MissingEntityDependency;
				}

				if ( _externallyReferencedEntities.Contains( val ) )
				{
					return PopulateRowResult.ActiveReference;
				}

				r[ 0 ] = val;

				if ( !_upgradeToIdMap.TryGetValue( e.FieldId, out val ) )
				{
					return PopulateRowResult.MissingFieldDependency;
				}

				r[ 1 ] = val;
				r[ 2 ] = e.Data;

				return PopulateRowResult.Success;
			};

			Func<DataEntry, bool> debugCallback = item =>
			{
				ISet<Guid> tenantItemsToKeep;

				if ( item != null && _itemsToKeep != null && _itemsToKeep.TryGetValue( TenantId, out tenantItemsToKeep ) )
				{
					if ( tenantItemsToKeep != null && tenantItemsToKeep.Contains( item.EntityId ) )
					{
						return true;
					}
				}

				return false;
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = dataTable,
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetDeleteFieldCommandText, dataTable ),
				ExecuteAction = ExecuteAction.Deleting,
				DebugCallback = debugCallback,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( string.Format( "Staged {0} Rows for Delete", dataTable ), count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( string.Format( "Altered {0} Database Rows (Delete)", dataTable ), count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Deletes relationships.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
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

				if ( _externallyReferencedEntities.Contains( val ) )
				{
					return PopulateRowResult.ActiveReference;
				}

				row[ 0 ] = val;

				if ( !_upgradeToIdMap.TryGetValue( entry.FromId, out val ) )
				{
					return PopulateRowResult.MissingFromDependency;
				}

				if ( _externallyReferencedEntities.Contains( val ) )
				{
					return PopulateRowResult.ActiveReference;
				}

				row[ 1 ] = val;

				if ( !_upgradeToIdMap.TryGetValue( entry.ToId, out val ) )
				{
					return PopulateRowResult.MissingToDependency;
				}

				if ( _externallyReferencedEntities.Contains( val ) )
				{
					return PopulateRowResult.ActiveReference;
				}

				row[ 2 ] = val;

				return PopulateRowResult.Success;
			};

			Func<DataColumn[ ]> getInSolutionColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "UpgradeId", typeof ( Guid ) )
				{
					AllowDBNull = false
				}
			};

			Func<EntityEntry, DataRow, PopulateRowResult> populateInSolutionRowAction = ( entry, row ) =>
			{
				long val;

				if ( !_upgradeToIdMap.TryGetValue( entry.EntityId, out val ) )
				{
					return PopulateRowResult.MissingEntityDependency;
				}

				row[ 0 ] = entry.EntityId;

				return PopulateRowResult.Success;
			};

			Func<RelationshipEntry, bool> debugCallback = item =>
			{
				ISet<Guid> tenantItemsToKeep;

				if ( item != null && _itemsToKeep != null && _itemsToKeep.TryGetValue( TenantId, out tenantItemsToKeep ) )
				{
					if ( tenantItemsToKeep != null && ( tenantItemsToKeep.Contains( item.TypeId ) || tenantItemsToKeep.Contains( item.FromId ) || tenantItemsToKeep.Contains( item.ToId ) ) )
					{
						return true;
					}
				}

				return false;
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<RelationshipEntry>
			{
				Entries = relationships,
				GetColumnsAction = getColumnsAction,
				TableName = "Relationships",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = CommandText.TenantMergeTargetDeleteRelationshipsCommandText,
				ExecuteAction = ExecuteAction.Deleting,
				DebugCallback = debugCallback,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Relationship Rows for Delete", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Relationship Database Rows (Delete)", count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );

			if ( _externallyReferencedEntities != null && _externallyReferencedEntities.Count > 0 )
			{
				var externallyReferencedExecutionArguments = new TenantMergeTargetExecutionArguments<EntityEntry>
				{
					Entries = context.Report.RemovedEntities,
					GetColumnsAction = getInSolutionColumnsAction,
					TableName = "Entities",
					Context = context,
					PopulateRowAction = populateInSolutionRowAction,
					CommandText = CommandText.TenantMergeTargetDeleteEntityInSolutionRelationships,
					ExecuteAction = ExecuteAction.Deleting,
					SetupCommandAction = c =>
					{
						c.AddParameterWithValue( "@tenant", TenantId );
						c.AddParameterWithValue( "@applicationId", ApplicationId );
					}
				};

				Execute( externallyReferencedExecutionArguments );
			}
		}

		/// <summary>
		/// Method executed prior to entity/relationship deletion.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="context">The context.</param>
		void IMergeTarget.PreDeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
			Func<DataColumn [ ]> getColumnsAction = ( ) => new [ ]
			{
				new DataColumn( "UpgradeId", typeof ( Guid ) )
				{
					AllowDBNull = false
				}
			};

			Func<EntityEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				row [ 0 ] = entry.EntityId;

				return PopulateRowResult.Success;
			};

			Action<IDbCommand> setupCommandAction = command =>
			{
				command.AddParameterWithValue( "@tenant", TenantId );
				command.AddParameterWithValue( "@applicationId", ApplicationId );
			};

			Func<IDbCommand, int> customCommandExecuteAction = command =>
			{
				/////
				// Get all the externally referenced entities.
				/////
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						_externallyReferencedEntities.Add( reader.GetInt64( 0 ) );
					}
				}

				return _externallyReferencedEntities.Count;
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<EntityEntry>
			{
				Entries = entities,
				GetColumnsAction = getColumnsAction,
				TableName = "Entities",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = CommandText.TenantMergeTargetPreDeleteEntitiesCommandText,
				SetupCommandAction = setupCommandAction,
				CustomCommandExecuteAction = customCommandExecuteAction
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Set the application metadata.
		/// </summary>
		void IDataTarget.SetMetadata( Metadata metadata, IProcessingContext context )
		{
		}

		/// <summary>
		///     Called for binary data with new values.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="context"></param>
		/// <exception cref="System.InvalidOperationException">No active database connection.</exception>
		void IMergeTarget.UpdateBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{
			// No-op
		}

		/// <summary>
		///     Updates the document data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		public void UpdateDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context )
		{
			// No-op
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
			if ( dataTable == "Alias" )
			{
				UpdateFieldData_Alias( data, context );

				return;
			}

			if ( dataTable == "Xml" )
			{
				UpdateFieldData_Xml( data, context );

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
				},
				new DataColumn( "ExistingData", Helpers.FieldDataTableTypes[ dataTable ] )
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
				row[ 3 ] = entry.ExistingData;

				return PopulateRowResult.Success;
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = dataTable,
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetUpdateFieldCommandText, dataTable, string.Empty ),
				ExecuteAction = ExecuteAction.Updating,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( string.Format( "Staged {0} Rows for Update", dataTable ), count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( string.Format( "Altered {0} Database Rows (Update)", dataTable ), count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Relationships *may* change if their EntityId has been changed (although this is unlikely in practice)
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IMergeTarget.UpdateRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
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
				},
				new DataColumn( "OldToId", typeof ( long ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "OldFromId", typeof ( long ) )
				{
					AllowDBNull = true
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

				if ( entry.PreviousValue != null )
				{
					if ( !_upgradeToIdMap.TryGetValue( entry.PreviousValue.Value, out val ) )
					{
						return PopulateRowResult.MissingPreviousLookupDependency;
					}

					if ( entry.Cardinality.HasValue )
					{
						if ( entry.Cardinality.Value == ReadiNow.Model.CardinalityEnum_Enumeration.OneToMany )
						{
							row[ 3 ] = val;
						}
						else if ( entry.Cardinality.Value == ReadiNow.Model.CardinalityEnum_Enumeration.ManyToOne )
						{
							row[ 4 ] = val;
						}
						else if ( entry.Cardinality.Value == ReadiNow.Model.CardinalityEnum_Enumeration.OneToOne )
						{
							if ( entry.UpdateTo )
							{
								row [ 4 ] = val;
							}
							else
							{
								row [ 3 ] = val;
							}
						}
					}
				}

				return PopulateRowResult.Success;
			};

			var relationshipEntries = relationships as IList<RelationshipEntry> ?? relationships.ToList( );
			var cardinalityRelationships = relationshipEntries.Where( re => re.TypeId == CardinalityId );

			var executionArguments = new TenantMergeTargetExecutionArguments<RelationshipEntry>
			{
				Entries = relationshipEntries,
				GetColumnsAction = getColumnsAction,
				TableName = "Relationships",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = CommandText.TenantMergeTargetUpdateRelationshipsCommandText,
				ExecuteAction = ExecuteAction.Updating,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Relationship Rows for Update", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Relationship Database Rows (Update)", count, StatisticsCountType.Executed ) )
			};

			var cardinalityEntries = cardinalityRelationships as RelationshipEntry[ ] ?? cardinalityRelationships.ToArray( );

			if ( cardinalityEntries.Length > 0 )
			{
				executionArguments.Entries = cardinalityEntries;

				Execute( executionArguments );
			}

			executionArguments.Entries = relationshipEntries;

			Execute( executionArguments );
		}


		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		/// <exception cref="System.InvalidOperationException">No active database connection.</exception>
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
                        context.WriteWarning(string.Format("An error occurred putting binary file into file repository. DataHash: {0}. Error {1}.", entry.DataHash, ex.ToString()));

                        return PopulateRowResult.InvalidData;
                    }
                }                				

				return PopulateRowResult.Ignore;
			};			

			var executionArguments = new TenantMergeTargetExecutionArguments<BinaryDataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Binary",
				Context = context,
				PopulateRowAction = populateRowAction,
                SkipCommandExec = true,
                ExecuteAction = ExecuteAction.Writing,				
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Binary Rows for Add", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Binary Database Rows (Add)", count, StatisticsCountType.Executed ) )
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
                        context.WriteWarning(string.Format("An error occurred putting document into file repository. DataHash: {0}. Error {1}.", entry.DataHash, ex.ToString()));

                        return PopulateRowResult.InvalidData;
                    }
                }

                return PopulateRowResult.Ignore;
            };			

			var executionArguments = new TenantMergeTargetExecutionArguments<DocumentDataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Document",
				Context = context,
				PopulateRowAction = populateRowAction,
                SkipCommandExec = true,
                ExecuteAction = ExecuteAction.Writing,
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Document Rows for Add", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Document Database Rows (Add)", count, StatisticsCountType.Executed ) )
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

			var executionArguments = new TenantMergeTargetExecutionArguments<EntityEntry>
			{
				Entries = entities,
				GetColumnsAction = getColumnsAction,
				TableName = "Entities",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = CommandText.TenantMergeTargetWriteEntitiesCommandText,
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Entity Rows for Add", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Entity Database Rows (Add)", count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );

			/////
			// Populate the UpgradeId to Id map.
			/////
			PopulateUpgradeIdToIdMap( );
		}

        /// <summary>
        /// Converts the data to a date time string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>System.String.</returns>
        private string ConvertToDateTimeString(object data)
	    {
            if (data == null)
            {
                return null;
            }

            DateTime? dt = null;

            if (data is DateTime)
            {
                dt = (DateTime)data;
            }
            else
            {
                DateTime nonNullDateTime;

                if (DateTime.TryParse(data.ToString(), null, DateTimeStyles.AssumeUniversal, out nonNullDateTime))
                {
                    dt = nonNullDateTime;
                }
            }

            return dt?.ToString(@"yyyy-MM-dd HH\:mm\:ss.fff", CultureInfo.InvariantCulture);
	    }

		/// <summary>
		///     Write in collection of field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that data types are converted to their correct internal storage formats (e.g. 1/0 for bits in SQLite)
		///     - ensure that XML is transformed so that entityRefs are remapped to the local ID space. Entities will be received
		///     as either UID or UID|Alias
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

			Func<DataColumn[ ]> getMissingDependenciesAction = ( ) => new[ ]
			{
				new DataColumn( "AppVerUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "TenantId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "EntityUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "FieldUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "Data", typeof ( string ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "Type", typeof ( string ) )
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

			Func<DataEntry, DataRow, PopulateRowResult> populateMissingDependencyAction = ( entry, row ) =>
			{
				row[ 0 ] = CallData<Guid>.GetValue( "NewAppVerUid" );
				row[ 1 ] = TenantId;
				row[ 2 ] = entry.EntityId;
				row[ 3 ] = entry.FieldId;
				row[ 4 ] = dataTable == "DateTime" ? ConvertToDateTimeString(entry.Data) : entry.Data;
				row[ 5 ] = dataTable;

				return PopulateRowResult.Success;
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = dataTable,
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetWriteFieldCommandText, dataTable, string.Empty, string.Empty, string.Empty, string.Empty ),
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				RecordMissingDependencies = true,
				GetMissingDependenciesColumns = getMissingDependenciesAction,
				PopulateMissingDependenciesAction = populateMissingDependencyAction,
				MissingDataCommandText = CommandText.TenantMergeTargetWriteMissingFieldsCommandText,
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( string.Format( "Staged {0} Rows for Add", dataTable ), count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( string.Format( "Altered {0} Database Rows (Add)", dataTable ), count, StatisticsCountType.Executed ) ),
				SetDroppedCountAction = ( count, reason ) => context.Report.Counts.Add( new StatisticsCount( string.Format( "Dropped {0} Rows due to {1}", dataTable, reason ), count, StatisticsCountType.Executed ) )
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

			Func<DataColumn[ ]> getMissingDependenciesAction = ( ) => new[ ]
			{
				new DataColumn( "AppVerUid", typeof ( Guid ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "TenantId", typeof ( long ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "TypeUid", typeof ( Guid ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "FromUid", typeof ( Guid ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "ToUid", typeof ( Guid ) )
				{
					AllowDBNull = true
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

			Func<RelationshipEntry, DataRow, PopulateRowResult> populateMissingDependencyAction = ( entry, row ) =>
			{
				row[ 0 ] = CallData<Guid>.GetValue( "NewAppVerUid" );
				row[ 1 ] = TenantId;
				row[ 2 ] = entry.TypeId;
				row[ 3 ] = entry.FromId;
				row[ 4 ] = entry.ToId;

				return PopulateRowResult.Success;
			};

			Func<SqlBulkCopyColumnMapping[ ]> getMissingDependenciesColumMappings = ( ) => new[ ]
			{
				new SqlBulkCopyColumnMapping( "AppVerUid", "AppVerUid" ),
				new SqlBulkCopyColumnMapping( "TenantId", "TenantId" ),
				new SqlBulkCopyColumnMapping( "TypeUid", "TypeUid" ),
				new SqlBulkCopyColumnMapping( "FromUid", "FromUid" ),
				new SqlBulkCopyColumnMapping( "ToUid", "ToUid" )
			};

			Func<object, PopulateRowResult, string> generateLogMessage = ( objEntry, reason ) =>
			{
				var entry = ( RelationshipEntry ) objEntry;

				return string.Format( "Dropped relationship (Type: {0}, From: {1}, To: {2}) due to {3}", entry.TypeId.ToString( "B" ), entry.FromId.ToString( "B" ), entry.ToId.ToString( "B" ), reason.GetDescription( ) );
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<RelationshipEntry>
			{
				Entries = relationships,
				GetColumnsAction = getColumnsAction,
				TableName = "Relationships",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = CommandText.TenantMergeTargetWriteRelationshipsCommandText,
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				RecordMissingDependencies = true,
				GetMissingDependenciesColumns = getMissingDependenciesAction,
				PopulateMissingDependenciesAction = populateMissingDependencyAction,
				MissingDependenciesTableName = "Relationship",
				GetMissingDependenciesColumnMappings = getMissingDependenciesColumMappings,
				MissingDataCommandText = CommandText.TenantMergeTargetWriteMissingRelationshipsCommandText,
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Relationship Rows for Add", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Relationship Database Rows (Add)", count, StatisticsCountType.Executed ) ),
				SetDroppedCountAction = ( count, reason ) => context.Report.Counts.Add( new StatisticsCount( string.Format( "Dropped Relationship Rows due to {0}", reason ), count, StatisticsCountType.Dropped ) ),
				MissingDependencyAction = ( entry, reason ) => context.Report.MissingDependencies.Add( new MissingDependency<object>( entry, reason, generateLogMessage ) )
			};

			Execute( executionArguments );
		}


		/// <summary>
		///     Sets up this instance.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.Setup( IProcessingContext context )
		{
			if ( context != null )
			{
				context.WriteInfo( "Initializing..." );
			}

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = CommandText.TenantMergeTargetSetupCommandText;
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );

				command.CommandText = @"
IF ( @context IS NOT NULL )
BEGIN
	DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
	SET CONTEXT_INFO @contextInfo
END

SELECT UpgradeId
FROM Entity e
JOIN Data_Alias a ON
	e.TenantId = a.TenantId AND
	e.Id = a.EntityId AND
	a.Data = 'cardinality' AND
	a.Namespace = 'core' AND
	a.AliasMarkerId = 0
WHERE
	e.TenantId = @tenantId";
				command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ), 128 );
				command.AddParameterWithValue( "@tenantId", CallData<long>.GetValue( "TargetTenantId" ) );
				object id = command.ExecuteScalar( );

				if ( id != null && id != DBNull.Value )
				{
					CardinalityId = ( Guid ) id;
				}
			}
		}

		/// <summary>
		///     Tear down this instance.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.TearDown( IProcessingContext context )
		{
            // Ensure that the application has not stolen entities that belong to core apps
            // This can possibly / hopefully be removed in the future, once we can trust that the applications don't refer to core entities.
            if ( context != null )
            {
                context.WriteInfo( "Running spRepairApplicationReferences for tenant..." );
            }

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( DatabaseContextInfo.SetContextInfo( "Repair app references" ) )
			using ( IDbCommand command = CreateCommand( ) )
            {
                command.CommandText = "spRepairApplicationReferences";
                command.CommandType = CommandType.StoredProcedure;
                command.AddParameterWithValue( "@tenant", TenantId );
				command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );
				command.ExecuteNonQuery( );
            }
            
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

			Guid appVerUid = CallData<Guid>.GetValue( "ExistingAppVerUid" );

			using ( EntryPointContext.UnsafeToIncludeEntryPoint( ) )
			using ( IDbCommand command = CreateCommand( ) )
			{
				command.AddParameterWithValue( "@appVer", appVerUid );
				command.AddParameterWithValue( "@tenantId", TenantId );

				/////
				// Delete the relationship data.
				/////
				command.CommandText = "DELETE FROM AppDeploy_Relationship WHERE AppVerUid = @appVer AND TenantId = @tenantId";
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );

				/////
				// Delete the field data in a separate call
				/////
				command.CommandText = "DELETE FROM AppDeploy_Field WHERE AppVerUid = @appVer AND TenantId = @tenantId";
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Executes the specified entries.
		/// </summary>
		/// <typeparam name="TEntry">The type of the entry.</typeparam>
		/// <param name="arguments">The arguments.</param>
		/// <exception cref="System.InvalidOperationException">No active database connection.</exception>
		private void Execute<TEntry>( TenantMergeTargetExecutionArguments<TEntry> arguments )
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

			DataTable missingDependencies = null;

			if ( arguments.RecordMissingDependencies && arguments.GetMissingDependenciesColumns != null )
			{
				missingDependencies = new DataTable( );

				/////
				// Populate the columns.
				/////
				missingDependencies.Columns.AddRange( arguments.GetMissingDependenciesColumns( ) );
			}

			var droppedCount = new Dictionary<PopulateRowResult, int>( );

			/////
			// Populate the rows.
			/////
			foreach ( TEntry entry in entryList )
			{
				if ( arguments.DebugCallback != null )
				{
					if ( arguments.DebugCallback( entry ) )
					{
						continue;
					}
				}

				DataRow row = table.NewRow( );

				PopulateRowResult populateRowResult = arguments.PopulateRowAction( entry, row );

			    if (populateRowResult == PopulateRowResult.Ignore)
			    {
			        continue;
			    }

				if ( populateRowResult != PopulateRowResult.Success )
				{
					if ( !droppedCount.ContainsKey( populateRowResult ) )
					{
						droppedCount[ populateRowResult ] = 0;
					}

					droppedCount[ populateRowResult ]++;

					if ( arguments.RecordMissingDependencies && missingDependencies != null && ( populateRowResult == PopulateRowResult.MissingEntityDependency || populateRowResult == PopulateRowResult.MissingFieldDependency || populateRowResult == PopulateRowResult.MissingFromDependency || populateRowResult == PopulateRowResult.MissingPreviousLookupDependency || populateRowResult == PopulateRowResult.MissingToDependency || populateRowResult == PopulateRowResult.MissingTypeDependency ) )
					{
						DataRow missingDependencyRow = missingDependencies.NewRow( );

						if ( arguments.PopulateMissingDependenciesAction != null )
						{
							arguments.PopulateMissingDependenciesAction( entry, missingDependencyRow );

							missingDependencies.Rows.Add( missingDependencyRow );
						}
					}

					if ( arguments.MissingDependencyAction != null )
					{
						arguments.MissingDependencyAction( entry, populateRowResult );
					}

					continue;
				}

				table.Rows.Add( row );
			}

			if ( arguments.SetDroppedCountAction != null )
			{
				foreach ( KeyValuePair<PopulateRowResult, int> pair in droppedCount )
				{
					arguments.SetDroppedCountAction( pair.Value, pair.Key.GetDescription( ) );
				}
			}

			/////
			// Populate the temporary tables.
			/////
			if ( missingDependencies != null && missingDependencies.Rows.Count > 0 )
			{
				string bulkTableName = string.IsNullOrEmpty( arguments.MissingDependenciesTableName ) ? "#AppDeploy_Field" : string.Format( "#AppDeploy_{0}", arguments.MissingDependenciesTableName );

				/////
				// Clear existing data.
				/////
				using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandText = string.Format( "TRUNCATE TABLE {0}", bulkTableName );
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery( );
				}

				using ( var bulkCopy = new SqlBulkCopy( connection ) )
				{
					bulkCopy.BulkCopyTimeout = 600;

					if ( arguments.GetMissingDependenciesColumnMappings != null )
					{
						foreach ( SqlBulkCopyColumnMapping cm in arguments.GetMissingDependenciesColumnMappings( ) )
						{
							bulkCopy.ColumnMappings.Add( cm );
						}
					}

					bulkCopy.DestinationTableName = bulkTableName;
					bulkCopy.WriteToServer( missingDependencies );
				}

				/////
				// Run the missing data command.
				/////
				using ( IDbCommand command = CreateCommand( ) )
				{
					command.CommandText = arguments.MissingDataCommandText;
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery( );
				}
			}

			if ( table.Rows.Count > 0 && !arguments.SkipCommandExec)
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

						if ( arguments.GetColumnMappings != null )
						{
							foreach ( SqlBulkCopyColumnMapping cm in arguments.GetColumnMappings( ) )
							{
								bulkCopy.ColumnMappings.Add( cm );
							}
						}

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
		///     Get Batch Id to prepare delete entity
		/// </summary>
		/// <returns></returns>
		private long GetBatchId( )
		{
			long value;

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = CommandText.GetBatchIdCommandText;

				value = ( long ) ( decimal ) command.ExecuteScalar( );
			}
			return value;
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
		///     Updates the field data for the alias table.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		private void UpdateFieldData_Alias( IEnumerable<DataEntry> data, IProcessingContext context )
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

			var executionArguments = new TenantMergeTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Alias",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetUpdateAliasCommandText, "Alias", ", d.Namespace = u.Namespace, d.AliasMarkerId = u.AliasMarkerId" ),
				ExecuteAction = ExecuteAction.Updating,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Alias Rows for Update", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Alias Database Rows (Update)", count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Updates the field data for the Xml table.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		private void UpdateFieldData_Xml( IEnumerable<DataEntry> data, IProcessingContext context )
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
				},
				new DataColumn( "ExistingData", Helpers.FieldDataTableTypes[ "Xml" ] )
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
				row[ 3 ] = entry.ExistingData == null ? DBNull.Value : ( object ) xmlProcessor.RemapXmlEntities( entry.EntityId, entry.FieldId, ( string ) entry.ExistingData );

				return PopulateRowResult.Success;
			};

			var executionArguments = new TenantMergeTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Xml",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetUpdateFieldCommandText, "Xml", string.Empty ),
				ExecuteAction = ExecuteAction.Updating,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Xml Rows for Update", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Xml Database Rows (Update)", count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );
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

			var executionArguments = new TenantMergeTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Alias",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetWriteFieldCommandText, "Alias", ", Namespace, AliasMarkerId", ", d.Namespace, d.AliasMarkerId", ", o.Namespace = d.Namespace, o.AliasMarkerId = d.AliasMarkerId", " OR d.Namespace <> o.Namespace OR d.AliasMarkerId <> o.AliasMarkerId" ),
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Alias Rows for Add", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Alias Database Rows (Add)", count, StatisticsCountType.Executed ) )
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

			var executionArguments = new TenantMergeTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "Xml",
				Context = context,
				PopulateRowAction = populateRowAction,
				CommandText = string.Format( CommandText.TenantMergeTargetWriteFieldCommandText, "Xml", string.Empty, string.Empty, string.Empty, string.Empty ),
				ExecuteAction = ExecuteAction.Writing,
				SetupCommandAction = c => c.AddParameterWithValue( "@tenant", TenantId ),
				SetCopiedCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Staged Xml Rows for Add", count, StatisticsCountType.Copied ) ),
				SetExecuteCountAction = count => context.Report.Counts.Add( new StatisticsCount( "Altered Xml Database Rows (Add)", count, StatisticsCountType.Executed ) )
			};

			Execute( executionArguments );
		}


        /// <summary>
        /// Loads existing file data hashes from the database.
        /// </summary>
	    private ISet<string> LoadExistingFileDataHashes()
	    {	        
            var dataHashes = new HashSet<string>();

	        var command = CreateCommand();
	        command.CommandText = CommandText.LoadFileDataHashesCommandText;
            command.CommandType = CommandType.Text;
            using(var reader = command.ExecuteReader())
	        {
	            while (reader.Read())
	            {
	                string hash = reader.GetString(0);
	                if (!string.IsNullOrWhiteSpace(hash))
	                {
	                    dataHashes.Add(hash.ToUpperInvariant());
	                }
	            }
	        }

	        return dataHashes;	        
	    }


        void IDataTarget.WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context)
        {
            // Do nothing
        }


        void IDataTarget.WriteDoNotRemove( IEnumerable<Guid> data, IProcessingContext context )
        {
            // Do nothing
        }
    }
}