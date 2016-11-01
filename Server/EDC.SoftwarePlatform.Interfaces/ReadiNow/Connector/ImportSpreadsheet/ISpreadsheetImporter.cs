// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiNow.Annotations;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    public interface ISpreadsheetImporter
    {
        /// <summary>
        ///     Begin task to import spreadsheet data.
        /// </summary>
        /// <param name="importSettings">The settings of the import.</param>
        /// <returns>Returns the ID of the import run.</returns>
        long StartImport( [NotNull] ImportSettings importSettings );

        /// <summary>
        ///     Get Import status of the importing task.
        /// </summary>
        /// <param name="importRunId">Import run ID.</param>
        /// <returns>Returns the import status info.</returns>
        [NotNull]
        ImportResultInfo GetImportStatus( long importRunId );

        /// <summary>
        ///     Cancel Import operation.
        /// </summary>
        /// <param name="importRunId">Import run ID.</param>
        /// <returns>Returns the import status info.</returns>
        [NotNull]
        ImportResultInfo CancelImportOperation( long importRunId );
    }

    public class ImportSettings
    {
        /// <summary>
        /// The ID of the import configuration to use.
        /// </summary>
        public long ImportConfigId { get; set; }

        /// <summary>
        /// The file token for the file to import.
        /// </summary>
        public string FileToken { get; set; }

        /// <summary>
        /// The file name for the file to import.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The user is performing a test run. Do not save data.
        /// </summary>
        public bool TestRun { get; set; }

        /// <summary>
        /// The timezone to use.
        /// </summary>
        public string TimeZoneName { get; set; }

        /// <summary>
        /// Suppress the read requirement check on the import config object.
        /// </summary>
        public bool SuppressSecurityCheckOnImportConfig { get; set; }

    }
}