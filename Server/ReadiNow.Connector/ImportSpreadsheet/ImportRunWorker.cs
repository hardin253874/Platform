// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Linq;
using Autofac.Extras.AttributeMetadata;
using EDC.IO;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Utc;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Processing;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// Service for doing the actual in-thread processing of an import run.
    /// </summary>
    class ImportRunWorker : IImportRunWorker
    {
        internal IFileRepository FileRepository { get; }
        private readonly IEntityRepository _entityRepository;
        private readonly RecordImporter.Factory _recordImporterActivator;
        private readonly Func<ImportFormat, IDataFileReaderService> _readerActivator;
        private readonly IReaderToEntityAdapterProvider _readerToEntityAdapterProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityRepository">Used to load/save the import run and config details.</param>
        /// <param name="fileRepository">Used to get file stream of file being imported.</param>
        /// <param name="recordImporterActivator">Used to create an importer of records.</param>
        /// <param name="readerActivator">Used to activate a file parser.</param>
        /// <param name="readerToEntityAdapterProvider">Creates an adapter that writes data into entities.</param>
        public ImportRunWorker( IEntityRepository entityRepository, [WithKey( FileRepositoryModule.TemporaryFileRepositoryName )] IFileRepository fileRepository, RecordImporter.Factory recordImporterActivator, Func<ImportFormat, IDataFileReaderService> readerActivator, IReaderToEntityAdapterProvider readerToEntityAdapterProvider )
        {
            if ( entityRepository == null )
                throw new ArgumentNullException( nameof( entityRepository ) );
            if ( fileRepository == null )
                throw new ArgumentNullException( nameof( fileRepository ) );
            if ( recordImporterActivator == null )
                throw new ArgumentNullException( nameof( recordImporterActivator ) );
            if ( readerActivator == null )
                throw new ArgumentNullException( nameof( readerActivator ) );
            if ( readerToEntityAdapterProvider == null )
                throw new ArgumentNullException( nameof( readerToEntityAdapterProvider ) );

            _entityRepository = entityRepository;
            FileRepository = fileRepository;
            _recordImporterActivator = recordImporterActivator;
            _readerActivator = readerActivator;
            _readerToEntityAdapterProvider = readerToEntityAdapterProvider;
        }


        /// <summary>
        /// Start processing an import run.
        /// </summary>
        /// <param name="importRunId">ID of the import run to process.</param>
        public void StartImport( long importRunId )
        {
            using ( new SecurityBypassContext( ) )
            {
                ImportRun importRun = _entityRepository.Get<ImportRun>( importRunId );
                if ( importRun == null )
                    throw new ArgumentException( nameof( importRunId ) );

                importRun = importRun.AsWritable<ImportRun>( );

                // Verify that import run is ready to run.
                if ( importRun.ImportRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunQueued )
                {
                    throw new Exception( "Import run is not marked as queued, or has already been started." );
                }

                // Mark it as started
                importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunStarted;
                importRun.ImportRunStarted = DateTime.UtcNow;
                importRun.Save( );

                // Trap errors
                try
                {
                    string verb = "imported";
                    if ( importRun.ImportTestRun == true )
                    {
                        verb = "verified";
                        importRun.ImportMessages += "This is a test import. No records are being saved.\r\n";
                    }

                    StartImportSafe( importRun );

                    if ( importRun.ImportRecordsTotal == 0 || importRun.ImportRecordsTotal == null )
                        throw new Exception( $"No records were found to be {verb}." );
                    if ( importRun.ImportRecordsSucceeded == 0 )
                        throw new Exception( $"No records were successfully {verb}." );

                    // Mark run as completed
                    if ( importRun.ImportRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunCancelled )
                        importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunCompleted;
                }
                catch ( Exception ex )
                {
                    importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunFailed;
                    importRun.ImportMessages = "Failed: " + ex.Message + "\r\n" + importRun.ImportMessages; // TODO : Something better than this
                }
                finally
                {
                    importRun.ImportRunFinished = DateTime.UtcNow;
                    importRun.Save( );
                }
            }
        }


        /// <summary>
        /// Start processing an import run, after checking has been performed.
        /// </summary>
        /// <param name="importRun">A writable import run entity. Caller will save.</param>
        internal void StartImportSafe( ImportRun importRun )
        {
            ImportConfig importConfig = importRun.ImportConfigUsed;
            if ( importConfig == null )
                throw new ConnectorConfigException( "Import configuration could not be loaded." );
            if ( importConfig.ImportConfigMapping == null )
                throw new ConnectorConfigException( "Import configuration has no mapping." );

            IObjectsReader recordsReader = null;
            IReaderToEntityAdapter entityAdapter;
            IImportReporter importRunReporter;
            IRecordImporter recordImporter;
            ICancellationWatcher cancellationWatcher;
            BatchRunner batchRunner;

            try
            {
                // Reads records out of the file to count
                recordsReader = GetRecordsReader( importRun, importConfig );
                importRun.ImportRecordsTotal = recordsReader.GetObjects( ).Count( );
                importRun.Save( );

                // Re-reads records out of the file for importing
                recordsReader = GetRecordsReader( importRun, importConfig );

                // Writes records into entities
                entityAdapter = GetEntityAdapter( importConfig );

                // Create a reporter to process progress notifications
                importRunReporter = new ImportRunReporter(importRun);

                // Create a reporter to process progress notifications
                cancellationWatcher = new ImportRunCancellationWatcher(importRun);

                // Activate record impoter
                bool testRun = importRun.ImportTestRun == true;
                recordImporter = _recordImporterActivator(entityAdapter, importRunReporter, importConfig.ImportConfigMapping, testRun );
                if (recordImporter == null)
                    throw new Exception( "recordImporter failed to activate." );

                // Connects the reader to the writer
                batchRunner = new BatchRunner
                {
                    ObjectsReader = recordsReader,
                    RecordImporter = recordImporter,
                    CancellationWatcher = cancellationWatcher
                };

                // Go! Run as user
                using ( new SecurityBypassContext( false ) )
                {
                    batchRunner.ProcessAll( );
                }
            }
            finally
            {
                recordsReader?.Dispose( );
            }
        }


        /// <summary>
        /// Open up a records reader to read the contents of the file.
        /// </summary>
        /// <remarks>
        /// Caller closes stream.
        /// </remarks>
        private IObjectsReader GetRecordsReader( ImportRun importRun, ImportConfig importConfig )
        {
            // Get settings
            DataFileReaderSettings settings = CreateReaderSettings( importConfig );

            // Get the timezone
            if ( !string.IsNullOrEmpty( importRun.ImportTimeZone ) )
            {
                settings.TimeZoneInfo = TimeZoneHelper.GetTimeZoneInfo( importRun.ImportTimeZone );
            }

            // Get file reader
            IDataFileReaderService fileReader = _readerActivator( settings.ImportFormat );

            // Open stream
            string fileUploadId = importRun.ImportFileId;
            if ( string.IsNullOrEmpty( fileUploadId ) )
                throw new Exception( "File handle not set" );

            Stream fileStream;
            try
            {
                fileStream = FileRepository.Get(fileUploadId);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not retrieve file. " + ex.Message, ex);
            }
            IObjectsReader recordsReader = fileReader.OpenDataFile( fileStream, settings );
            return recordsReader;
        }


        /// <summary>
        /// Get reader settings from the config entity.
        /// </summary>
        /// <param name="importConfig">The import config entity.</param>
        /// <returns>Reader settings</returns>
        private static DataFileReaderSettings CreateReaderSettings( ImportConfig importConfig )
        {
            ImportFormat importFormat = ImportHelpers.GetImportFormat( importConfig );
            ApiResourceMapping mapping = importConfig.ImportConfigMapping;

            DataFileReaderSettings settings = new DataFileReaderSettings
            {
                ImportFormat = importFormat,
                HeadingRowNumber = mapping.ImportHeadingRow ?? 1,
                FirstDataRowNumber = mapping.ImportDataRow ?? 2,
                LastDataRowNumber = mapping.ImportLastDataRow, // default is null
                SheetId = mapping.MappingSourceReference
            };
            return settings;
        }


        /// <summary>
        /// Get an adapter for writing entities.
        /// </summary>
        private IReaderToEntityAdapter GetEntityAdapter( ImportConfig importConfig )
        {
            ApiResourceMapping mapping = importConfig.ImportConfigMapping;
            
            ReaderToEntityAdapterSettings settings = new ReaderToEntityAdapterSettings( );
            settings.UseTargetMemberNameForReporting = true;

            //settings.TimeZoneName
            IReaderToEntityAdapter entityAdapter = _readerToEntityAdapterProvider.GetAdapter( mapping.Id, settings );
            return entityAdapter;
        }

    }
}
