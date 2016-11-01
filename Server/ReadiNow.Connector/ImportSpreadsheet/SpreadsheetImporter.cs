// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Core;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// Service for supporting spreadsheet import operations.
    /// </summary>
    public class SpreadsheetImporter : ISpreadsheetImporter
    {
        private readonly IEntityRepository _entityRepository;
        private readonly IAsyncRunner _asyncRunner;
        private readonly IImportRunWorker _importRunWorker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityRepository">Injected service</param>
        /// <param name="asyncRunner">Injected service</param>
        /// <param name="importRunWorker">Injected service</param>
        public SpreadsheetImporter( IEntityRepository entityRepository, IAsyncRunner asyncRunner, IImportRunWorker importRunWorker )
        {
            if ( entityRepository == null )
                throw new ArgumentNullException( nameof( entityRepository ) );
            if ( importRunWorker == null )
                throw new ArgumentNullException( nameof( importRunWorker ) );
            if ( asyncRunner == null)
                throw new ArgumentNullException( nameof( asyncRunner ) );

            _entityRepository = entityRepository;
            _asyncRunner = asyncRunner;
            _importRunWorker = importRunWorker;
        }


        /// <summary>
        ///     Begin task to import spreadsheet data.
        /// </summary>
        /// <param name="importSettings">The settings of the import.</param>
        /// <returns>Returns the ID of the import run.</returns>
        public long StartImport( ImportSettings importSettings )
        {
            // Validate
            if ( importSettings == null )
                throw new ArgumentNullException( nameof(importSettings) );
            if ( string.IsNullOrEmpty( importSettings.FileToken ) )
                throw new ArgumentException( "importSettings.FileToken" );

            // Load the config
            ImportConfig importConfig = SecurityBypassContext.ElevateIf(
                importSettings.SuppressSecurityCheckOnImportConfig,
                ( ) => _entityRepository.Get<ImportConfig>( importSettings.ImportConfigId ) );

            if ( importConfig == null )
                throw new ArgumentException( "importSettings.ImportConfigId" );

            // Create a new import run
            ImportRun importRun = CreateImportRunEntity( importConfig, importSettings );
            SecurityBypassContext.Elevate( importRun.Save );

            long importRunId = importRun.Id;

            try
            {
                _asyncRunner.Start( ( ) => _importRunWorker.StartImport( importRunId ) );
            }
            catch
            {
                // Async operation failed to start
                // (This is not reached if the import itself fails)
                importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunFailed;
                importRun.ImportMessages = "Failed to start importer.";
                SecurityBypassContext.Elevate( importRun.Save );
                throw;
            }

            return importRun.Id;
        }


        /// <summary>
        ///     Creates an importRun entity - does not save it.
        /// </summary>
        /// <param name="importConfig">The import configuration.</param>
        /// <param name="importSettings">Settings passed in for the current run.</param>
        /// <returns>Returns the ID of the import run.</returns>
        internal ImportRun CreateImportRunEntity( ImportConfig importConfig, ImportSettings importSettings )
        {
            // Create a new import run
            ImportRun importRun = _entityRepository.Create<ImportRun>( );
            importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunQueued;
            importRun.ImportConfigUsed = importConfig;
            importRun.ImportFileId = importSettings.FileToken;
            importRun.ImportFileName = importSettings.FileName;            
            importRun.ImportTimeZone = importSettings.TimeZoneName;
            importRun.ImportRecordsSucceeded = 0;
            importRun.ImportRecordsFailed = 0;
            importRun.ImportTestRun = importSettings.TestRun;
            return importRun;
        }


        /// <summary>
        ///     Cancel Import operation.
        /// </summary>
        /// <param name="importRunId">Import run ID.</param>
        /// <returns>Returns the import status info.</returns>
        public ImportResultInfo CancelImportOperation( long importRunId )
        {
            ImportRun importRun = GetImportRun( importRunId ).AsWritable<ImportRun>( );

            // Check status - only cancel if still running
            ImportStatus importStatus = ImportHelpers.GetImportStatus( importRun );
            if ( importStatus == ImportStatus.InProgress )
            {
                // Cancel
                importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunCancelled;
                importRun.Save( );
            }

            ImportResultInfo result = GetImportResultInfo( importRun );
            return result;
        }


        /// <summary>
        ///     Get Import status of the importing task.
        /// </summary>
        /// <param name="importRunId">Import run ID.</param>
        /// <returns>Returns the import status info.</returns>
        public ImportResultInfo GetImportStatus( long importRunId )
        {
            ImportRun importRun = GetImportRun( importRunId );

            ImportResultInfo result = GetImportResultInfo( importRun );
            return result;
        }


        /// <summary>
        /// Load an import run entity.
        /// </summary>
        /// <param name="importRunId">The ID of the import run.</param>
        /// <returns>The import run.</returns>
        private ImportRun GetImportRun( long importRunId )
        {
            const string importRunQuery = "importRunStatus.alias, importRecordsTotal, importRecordsSucceeded, importMessages";
            return _entityRepository.Get<ImportRun>( importRunId, importRunQuery );
        }


        /// <summary>
        /// Extracts general progress/result information from an import run.
        /// </summary>
        /// <param name="importRun">The import run entity.</param>
        /// <returns>Result object.</returns>
        private ImportResultInfo GetImportResultInfo( ImportRun importRun )
        {
            ImportResultInfo result = new ImportResultInfo
            {
                ImportStatus = ImportHelpers.GetImportStatus( importRun ),
                ImportMessages = importRun.ImportMessages,
                RecordsTotal = importRun.ImportRecordsTotal ?? 0,
                RecordsSucceeded = importRun.ImportRecordsSucceeded ?? 0,
                RecordsFailed = importRun.ImportRecordsFailed ?? 0
            };
            return result;
        }

    }
}
