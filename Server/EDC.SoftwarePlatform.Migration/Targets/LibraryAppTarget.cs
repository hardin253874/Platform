// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Storage;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Migration.Targets
{
	/// <summary>
	///     Writes data for one application into the application library.
	/// </summary>
	internal class LibraryAppTarget : SqlBase, IDataTarget
	{	    
        /// <summary>
        ///     Initializes a new instance of the <see cref="LibraryAppTarget" /> class.
        /// </summary>
        public LibraryAppTarget( )
			: base( true, 600 )
		{
		}

		/// <summary>
		///     The unique identifier that tags this version of this application in the library tables.
		/// </summary>
		public Guid ApplicationVersionId
		{
			get;
			set;
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
				new DataColumn( "OldDataHash", typeof ( string ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "NewDataHash", typeof ( string ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "FileExtension", typeof ( string ) )
				{
					AllowDBNull = true
				}			
			};

			Func<BinaryDataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				if ( entry.Data == null )
				{
					return PopulateRowResult.InvalidData;
				}			    

			    using ( var source = new MemoryStream( entry.Data ) )
				{
				    try
				    {
                        // Add the file to the repository
                        string newDataHash = Factory.AppLibraryFileRepository.Put(source);

                        row[0] = entry.DataHash;
                        row[1] = newDataHash;
				        row[2] = string.IsNullOrWhiteSpace(entry.FileExtension) ? null : entry.FileExtension;
				    }
				    catch(Exception ex)
				    {
                        context.WriteWarning(string.Format("An error occurred putting binary file into file repository. DataHash: {0}. Error {1}.", entry.DataHash, ex.ToString()));

                        return PopulateRowResult.InvalidData;
				    }
                }                

                return PopulateRowResult.Success;
			};

			Func<IDbCommand, int> customCommandExecuteAction = command =>
			{                
                // Upgrade datahashes and file extensions
			    command.CommandText = string.Format(CommandText.AppLibraryUpgradeFileDataHashesAndFileExtensions, "#Binary");
                command.CommandType = CommandType.Text;
                command.AddParameterWithValue("@appVerId", ApplicationVersionId);
                command.AddParameterWithValue("@fileDataHashFieldId", Helpers.FileDataHashFieldUpgradeId);
                command.AddParameterWithValue("@fileExtensionFieldId", Helpers.FileExtensionFieldUpgradeId);
                command.ExecuteNonQuery();                
                return 0;
			};

			Action<int> setCopiedCountAction = count =>
			{
				if ( context != null )
				{
					context.Report.Counts.Add( new StatisticsCount( "Copied Binary Data", count, StatisticsCountType.Copied ) );
				}
			};

			var executionArguments = new LibraryAppTargetExecutionArguments<BinaryDataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "#Binary",
				Context = context,
				SetCopiedCountAction = setCopiedCountAction,
				PopulateRowAction = populateRowAction,
				ClearExistingData = false,
				CustomCommandExecuteAction = customCommandExecuteAction
			};

			Execute( executionArguments );
		}

		public void WriteDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context )
		{
            Func<DataColumn[]> getColumnsAction = () => new[]
           {
                new DataColumn( "OldDataHash", typeof ( string ) )
                {
                    AllowDBNull = false
                },
                new DataColumn( "NewDataHash", typeof ( string ) )
                {
                    AllowDBNull = false
                },
                new DataColumn( "FileExtension", typeof ( string ) )
                {
                    AllowDBNull = true
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
                        // Add the file to the repository
                        string newDataHash = Factory.AppLibraryFileRepository.Put(source);

                        row[0] = entry.DataHash;
                        row[1] = newDataHash;
                        row[2] = string.IsNullOrWhiteSpace(entry.FileExtension) ? null : entry.FileExtension;
                    }
                    catch(Exception ex)
                    {
                        context.WriteWarning(string.Format("An error occurred putting document into file repository. DataHash: {0}. Error {1}.", entry.DataHash, ex.ToString()));

                        return PopulateRowResult.InvalidData;
                    }
                }

                return PopulateRowResult.Success;
            };

			Func<IDbCommand, int> customCommandExecuteAction = command =>
			{
                // Upgrade datahashes and extensions
                command.CommandText = string.Format(CommandText.AppLibraryUpgradeFileDataHashesAndFileExtensions, "#Document");
                command.CommandType = CommandType.Text;
                command.AddParameterWithValue("@appVerId", ApplicationVersionId);
                command.AddParameterWithValue("@fileDataHashFieldId", Helpers.FileDataHashFieldUpgradeId);
                command.AddParameterWithValue("@fileExtensionFieldId", Helpers.FileExtensionFieldUpgradeId);
                command.ExecuteNonQuery();

			    return 0;
			};

			Action<int> setCopiedCountAction = count =>
			{
				if ( context != null )
				{
					context.Report.Counts.Add( new StatisticsCount( "Copied Document Data", count, StatisticsCountType.Copied ) );
				}
			};

			var executionArguments = new LibraryAppTargetExecutionArguments<DocumentDataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "#Document",
				Context = context,
				SetCopiedCountAction = setCopiedCountAction,
				PopulateRowAction = populateRowAction,
				ClearExistingData = false,
				CustomCommandExecuteAction = customCommandExecuteAction
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
				new DataColumn( "Id", typeof ( long ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "AppVerUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "EntityUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				}
			};

			Func<EntityEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				row[ 1 ] = ApplicationVersionId;
				row[ 2 ] = entry.EntityId;

				return PopulateRowResult.Success;
			};

			Action<int> setCopiedCountAction = count =>
			{
				if ( context != null )
				{
					context.Report.Counts.Add( new StatisticsCount( "Copied Entities", count, StatisticsCountType.Copied ) );
				}
			};

			var executionArguments = new LibraryAppTargetExecutionArguments<EntityEntry>
			{
				Entries = entities,
				GetColumnsAction = getColumnsAction,
				TableName = "AppEntity",
				Context = context,
				SetCopiedCountAction = setCopiedCountAction,
				PopulateRowAction = populateRowAction
			};

			Execute( executionArguments );
        }

        /// <summary>
        ///     Write in collection of entities.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        void IDataTarget.WriteDoNotRemove( IEnumerable<Guid> entities, IProcessingContext context )
        {
            Func<DataColumn [ ]> getColumnsAction = ( ) => new [ ]
             {
                new DataColumn( "Id", typeof ( long ) )
                {
                    AllowDBNull = true
                },
                new DataColumn( "AppVerUid", typeof ( Guid ) )
                {
                    AllowDBNull = false
                },
                new DataColumn( "EntityUid", typeof ( Guid ) )
                {
                    AllowDBNull = false
                }
            };

            Func<Guid, DataRow, PopulateRowResult> populateRowAction = ( entityUid, row ) =>
            {
                row [ 1 ] = ApplicationVersionId;
                row [ 2 ] = entityUid;

                return PopulateRowResult.Success;
            };

            Action<int> setCopiedCountAction = count =>
            {
                if ( context != null )
                {
                    context.Report.Counts.Add( new StatisticsCount( "Copied DoNotRemove entries", count, StatisticsCountType.Copied ) );
                }
            };

            var executionArguments = new LibraryAppTargetExecutionArguments<Guid>
            {
                Entries = entities,
                GetColumnsAction = getColumnsAction,
                TableName = "AppDoNotRemove",
                Context = context,
                SetCopiedCountAction = setCopiedCountAction,
                PopulateRowAction = populateRowAction
            };

            Execute( executionArguments );
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
                command.CommandText = "CREATE TABLE #Binary ( OldDataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, NewDataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, FileExtension NVARCHAR(20) COLLATE Latin1_General_CI_AI NULL )";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE #Document ( OldDataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, NewDataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, FileExtension NVARCHAR(20) COLLATE Latin1_General_CI_AI NULL )";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
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
				command.CommandText = "DROP TABLE #Binary";
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
				command.CommandText = "DROP TABLE #Document";
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Write in collection of field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that data types are converted to their correct internal storage formats (e.g. 1/0 for bits in Sqlite)
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

			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "Id", typeof ( long ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "AppVerUid", typeof ( Guid ) )
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
				new DataColumn( "Data", DataTableType( dataTable ) )
				{
					AllowDBNull = true
				}
			};		 

            Func<DataEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				row[ 1 ] = ApplicationVersionId;
				row[ 2 ] = entry.EntityId;
				row[ 3 ] = entry.FieldId;
				row[ 4 ] = entry.Data;

                return PopulateRowResult.Success;
			};

			Action<int> setCopiedCountAction = count =>
			{
				if ( context != null )
				{
					context.Report.Counts.Add( new StatisticsCount( string.Format( "Copied {0} Data", dataTable ), count, StatisticsCountType.Copied ) );
				}
			};

			var executionArguments = new LibraryAppTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "AppData_" + dataTable,
				Context = context,
				SetCopiedCountAction = setCopiedCountAction,
				PopulateRowAction = populateRowAction
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
				new DataColumn( "Id", typeof ( long ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "AppVerUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "TypeUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "FromUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				},
				new DataColumn( "ToUid", typeof ( Guid ) )
				{
					AllowDBNull = false
				}
			};

			Func<RelationshipEntry, DataRow, PopulateRowResult> populateRowAction = ( entry, row ) =>
			{
				row[ 1 ] = ApplicationVersionId;

				row[ 2 ] = entry.TypeId;
				row[ 3 ] = entry.FromId;
				row[ 4 ] = entry.ToId;

				return PopulateRowResult.Success;
			};

			Action<int> setCopiedCountAction = count =>
			{
				if ( context != null )
				{
					context.Report.Counts.Add( new StatisticsCount( "Copied Relationships", count, StatisticsCountType.Copied ) );
				}
			};

			var executionArguments = new LibraryAppTargetExecutionArguments<RelationshipEntry>
			{
				Entries = relationships,
				GetColumnsAction = getColumnsAction,
				TableName = "AppRelationship",
				Context = context,
				SetCopiedCountAction = setCopiedCountAction,
				PopulateRowAction = populateRowAction
			};

			Execute( executionArguments );
		}

		/// <summary>
		///     Set the application metadata.
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="context"></param>
		void IDataTarget.SetMetadata( Metadata metadata, IProcessingContext context )
		{
			/////
			// Write the application
			/////
			App app = Entity.GetByField<App>( metadata.AppId.ToString( ), new EntityRef( "core:applicationId" ) ).FirstOrDefault( );
			if ( app == null )
			{
				app = new App
				{
					Name = metadata.AppName,
					Description = metadata.Description,
					ApplicationId = metadata.AppId,
					Publisher = metadata.Publisher,
					PublisherUrl = metadata.PublisherUrl,
					ReleaseDate = metadata.ReleaseDate == DateTime.MinValue.ToUniversalTime() ? ( DateTime? ) null : metadata.ReleaseDate.ToUniversalTime()
				};

				app.Save( );
			}

			/////
			// Write the app-package
			/////
			AppPackage package = app.ApplicationPackages.FirstOrDefault( ap => ap.AppVerId == metadata.AppVerId );

			if ( package == null )
			{
				package = new AppPackage( );

				var version = new Version( metadata.Version );

				AppPackage existingVersion = app.ApplicationPackages.FirstOrDefault( ap => ap.AppVersionString == version.ToString( ) );

				bool versionExists = false;

				while ( existingVersion != null )
				{
					versionExists = true;

					version = new Version( version.Major, version.Minor + 1 );

					existingVersion = app.ApplicationPackages.FirstOrDefault( ap => ap.AppVersionString == version.ToString( ) );
				}

				metadata.Version = version.ToString( );

				if ( versionExists )
				{
					context.WriteWarning( "Version already exists.. incrementing" );
				}
			}
			else
			{
				package = package.AsWritable<AppPackage>( );
				context.WriteWarning( "Already installed.. overwriting" );
			}

			string solutionNames = app.InSolution?.Name;

			/////
			// Localize the string values.
			/////
			package.Name = string.Format( "{0} Application Package {1}", solutionNames ?? app.Name, metadata.Version );
			package.Description = string.Format( "Application Package for version {1} of {0}.", app.Name, metadata.Version );
			package.AppVersionString = metadata.Version;
			package.AppVerId = metadata.AppVerId;
			package.PackageForApplication = app;

			if ( metadata.PublishDate != DateTime.MinValue && metadata.PublishDate > SqlDateTime.MinValue.Value )
			{
				package.PublishDate = metadata.PublishDate;
			}

			if ( metadata.Dependencies != null )
			{
				IEntityCollection<AppPackageDependency> dependencies = new EntityCollection<AppPackageDependency>( );

				foreach ( SolutionDependency dependency in metadata.Dependencies )
				{
					AppPackageDependency appPackageDependency = new AppPackageDependency
					{
						Name = dependency.Name,
						AppPackageDependencyName = dependency.DependencyName,
						AppPackageDependencyId = dependency.DependencyApplication,
						AppPackageMinimumVersion = dependency.MinimumVersion == null ? null : dependency.MinimumVersion.ToString( 4 ),
						AppPackageMaximumVersion = dependency.MaximumVersion == null ? null : dependency.MaximumVersion.ToString( 4 ),
						AppPackageIsRequired = dependency.IsRequired
					};

					dependencies.Add( appPackageDependency );
				}

				package.DependentAppPackageDetails = dependencies;
			}

			package.Save( );
		}

		/// <summary>
		///     Executes the specified entries.
		/// </summary>
		/// <typeparam name="TEntry">The type of the entry.</typeparam>
		/// <param name="arguments">The arguments.</param>
		/// <exception cref="System.InvalidOperationException">No active database connection.</exception>
		private void Execute<TEntry>( LibraryAppTargetExecutionArguments<TEntry> arguments )
		{
			/////
			// Argument checks.
			/////
			if ( arguments == null || arguments.Entries == null || arguments.GetColumnsAction == null || string.IsNullOrEmpty( arguments.TableName ) || arguments.PopulateRowAction == null )
			{
				return;
			}

			IList<TEntry> entryList = arguments.Entries as IList<TEntry> ?? arguments.Entries.ToList( );

			/////
			// Early out.
			/////
			if ( entryList.Count <= 0 )
			{
				if ( arguments.SetCopiedCountAction != null )
				{
					arguments.SetCopiedCountAction( 0 );
				}

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

			using ( IDbCommand command = CreateCommand( ) )
			{
				if ( arguments.ClearExistingData )
				{
					/////
					// Clear existing data.
					/////
					command.CommandText = string.Format( "DELETE FROM {0} WHERE AppVerUid = @appVer", arguments.TableName );
					command.AddParameterWithValue( "@appVer", ApplicationVersionId );
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery( );
				}

				int rowsCopied = 0;

				if ( table.Rows.Count > 0 )
				{
					/////
					// Bulk load into the staging table.
					/////
					using ( var bulkCopy = new SqlBulkCopy( connection ) )
					{
						bulkCopy.BulkCopyTimeout = 600;
						bulkCopy.NotifyAfter = 100;

						string sanitizedTableName = arguments.TableName.Replace( "#", "" ).Replace( "App", "" ).Replace( "Data", "" ).Replace( "_", "" );

						if ( arguments.Context != null )
						{
							bulkCopy.SqlRowsCopied += ( sender, args ) => arguments.Context.WriteProgress( string.Format( "Copying {0} data... {1} rows", sanitizedTableName, args.RowsCopied ) );
						}

						bulkCopy.DestinationTableName = string.Format( "{0}", arguments.TableName );
						bulkCopy.WriteToServer( table );

						rowsCopied = bulkCopy.RowsCopiedCount( );
					}
				}

				if ( arguments.SetCopiedCountAction != null )
				{
					arguments.SetCopiedCountAction( rowsCopied );
				}

				if ( arguments.CustomCommandExecuteAction != null )
				{
					arguments.CustomCommandExecuteAction( command );
				}
			}
		}

		/// <summary>
		///     Writes the field data_ alias.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		private void WriteFieldData_Alias( IEnumerable<DataEntry> data, IProcessingContext context )
		{
			Func<DataColumn[ ]> getColumnsAction = ( ) => new[ ]
			{
				new DataColumn( "Id", typeof ( long ) )
				{
					AllowDBNull = true
				},
				new DataColumn( "AppVerUid", typeof ( Guid ) )
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
				row[ 1 ] = ApplicationVersionId;
				row[ 2 ] = entry.EntityId;
				row[ 3 ] = entry.FieldId;
				row[ 4 ] = entry.Data;
				row[ 5 ] = entry.Namespace;
				row[ 6 ] = entry.AliasMarkerId;

				return PopulateRowResult.Success;
			};

			Action<int> setCopiedCountAction = count =>
			{
				if ( context != null )
				{
					context.Report.Counts.Add( new StatisticsCount( "Copied Alias Data", count, StatisticsCountType.Copied ) );
				}
			};

			var executionArguments = new LibraryAppTargetExecutionArguments<DataEntry>
			{
				Entries = data,
				GetColumnsAction = getColumnsAction,
				TableName = "AppData_Alias",
				Context = context,
				SetCopiedCountAction = setCopiedCountAction,
				PopulateRowAction = populateRowAction
			};

			Execute( executionArguments );
		}


        /// <summary>
        ///     Write the secure data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        void IDataTarget.WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context)
        {
            context.Report.Counts.Add(new StatisticsCount("Copied SecureData", 0, StatisticsCountType.Copied));
        }
    }
}