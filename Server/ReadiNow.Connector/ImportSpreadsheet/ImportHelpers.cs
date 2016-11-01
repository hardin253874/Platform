// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// Misc static helpers for importing files.
    /// </summary>
    static class ImportHelpers
    {
        /// <summary>
        /// Read an import status out of an import run entity.
        /// </summary>
        /// <param name="importRun">The import run entity.</param>
        /// <returns>The import status.</returns>
        internal static ImportStatus GetImportStatus( ImportRun importRun )
        {
            if ( importRun == null )
                throw new ArgumentNullException( nameof( importRun ) );

            var runStatus = importRun.ImportRunStatus_Enum;
            if ( runStatus == null )
                return ImportStatus.Failed; // assert false

            switch ( runStatus.Value )
            {
                case WorkflowRunState_Enumeration.WorkflowRunQueued:
                case WorkflowRunState_Enumeration.WorkflowRunStarted:
                    return ImportStatus.InProgress;
                case WorkflowRunState_Enumeration.WorkflowRunCompleted:
                    return ImportStatus.Success;
                case WorkflowRunState_Enumeration.WorkflowRunFailed:
                    return ImportStatus.Failed;
                case WorkflowRunState_Enumeration.WorkflowRunCancelled:    // will do for now
                    return ImportStatus.Cancelled;
                case WorkflowRunState_Enumeration.WorkflowRunPaused:
                default:
                    throw new InvalidOperationException( );
            }
        }


        /// <summary>
        /// Read the import file type from an import confing.
        /// </summary>
        /// <param name="importConfig">The import config entity.</param>
        /// <returns>The import status.</returns>
        internal static ImportFormat GetImportFormat( ImportConfig importConfig )
        {
            if ( importConfig == null )
                throw new ArgumentNullException( nameof( importConfig ) );

            var fileType = importConfig.ImportFileType_Enum;
            if ( fileType == null )
                return ImportFormat.Excel;  // assert false

            switch ( fileType.Value )
            {
                case ImportFileTypeEnum_Enumeration.ImportFileTypeCsv:
                    return ImportFormat.CSV;
                case ImportFileTypeEnum_Enumeration.ImportFileTypeTab:
                    return ImportFormat.Tab;
                case ImportFileTypeEnum_Enumeration.ImportFileTypeExcel:
                    return ImportFormat.Excel;
                default:
                    throw new InvalidOperationException( );
            }
        }
    }
}
