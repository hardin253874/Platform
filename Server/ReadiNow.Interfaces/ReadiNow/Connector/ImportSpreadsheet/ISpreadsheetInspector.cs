// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiNow.Annotations;

namespace ReadiNow.Connector.ImportSpreadsheet
{
    /// <summary>
    /// Interface for gathering metadata about an upload file.
    /// </summary>
    public interface ISpreadsheetInspector
    {
        /// <summary>
        ///     Get spreadsheet Information from the imported file.
        /// </summary>
        /// <param name="fileUploadId">File upload Id. </param>
        /// <param name="fileFormat">Imported file fileFormat ( Excel or CSV)</param>
        /// <returns>Spreadsheet info</returns>
        /// <exception cref="FileFormatException"></exception>
        [NotNull]
        SpreadsheetInfo GetSpreadsheetInfo( string fileUploadId, ImportFormat fileFormat );

        /// <summary>
        ///     Get sheet info from the imported spreadsheet.
        /// </summary>
        /// <param name="fileUploadId">File upload Id.</param>
        /// <param name="fileFormat">Imported file fileFormat ( Excel or CSV)</param>
        /// <param name="sheetId">Selected sheet info.</param>
        /// <param name="headerRowNo">Header row no.</param>
        /// <param name="dataRowNo">Data row number to start reading data.</param>
        /// <param name="lastRowNo">Optional last row number to include.</param>
        /// <returns>Sample data table, or null if it could not be found.</returns>
        SampleTable GetSampleTable( [NotNull] string fileUploadId, ImportFormat fileFormat, string sheetId, int headerRowNo, int dataRowNo, int? lastRowNo );
    }
}