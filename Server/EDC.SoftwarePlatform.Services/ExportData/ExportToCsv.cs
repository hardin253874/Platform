// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using EDC.Database;
using ReadiNow.Reporting.Result;

namespace EDC.SoftwarePlatform.Services.ExportData
{
    /// <summary>
    /// Class to provide all the methods to export data to CSV file.
    /// </summary>
    public class ExportToCsv
    {
        #region Export to CSV file methods

		/// <summary>
		/// Generate CSV document byte stream
		/// </summary>
		/// <param name="reportResult">ReportResult</param>
		/// <param name="rows">The rows.</param>
		/// <returns>
		/// byte[]
		/// </returns>
        public static byte[] CreateCsvDocument(ReportResult reportResult, List<DataRow> rows)
        {
            var currentCulure = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {

                var sb = new StringBuilder();
                int headerIndx = 1;

                foreach (ReportColumn col in reportResult.Metadata.ReportColumns.Values)
                {
                    //add separator 
                    if (!col.IsHidden && col.Type != "Image")
                    {
                        sb.Append(col.Title);
                        if (headerIndx != reportResult.Metadata.ReportColumns.Count)
                            sb.Append(',');
                    }
                    headerIndx++;
                }
                //append new line 
                sb.Append("\r\n");

                foreach (DataRow row in rows)
                {
                    int colIndx = 0;
                    bool first = true;
                    foreach (ReportColumn col in reportResult.Metadata.ReportColumns.Values)
                    {
                        if (!col.IsHidden && col.Type != "Image")
                        {
                            if (first)
                                first = false;
                            else
                                sb.Append(',');

                            string cellValue = ExportDataHelper.GetCellValue(row, colIndx);
                            DatabaseType cellType =DatabaseTypeHelper.ConvertFromDisplayName(col.Type);
	                        if (!string.IsNullOrEmpty(col.AutoNumberDisplayPattern))
                            {
                                cellType = DatabaseType.AutoIncrementType;
                            }
                            string csvValue = !string.IsNullOrEmpty(cellValue) ? ExportDataHelper.GetCsvCellValue(cellType, cellValue, col) : "";
                            csvValue = csvValue.Replace("\r", string.Empty).Replace("\n", string.Empty);
                            sb.Append(CsvEscape(csvValue));
                        }
                        colIndx++;
                    }
                    //append new line 
                    sb.Append("\r\n");
                }
                byte[] bytesInStream = Encoding.UTF8.GetBytes(sb.ToString());

                //return bytesInStream;
                return Encoding.UTF8.GetPreamble().Concat(bytesInStream).ToArray();

            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulure;
            }
        }

        //Private members for CSVEscape function.
        private const string Quote = "\"";
        private const string EscapedQuote = "\"\"";
        private static readonly char[] CharactersThatMustBeQuoted = { ',', '"', '\n' };
        /// <summary>
        /// Escape few special characters while writing to the CSV file.
        /// </summary>       
        public static string CsvEscape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (s.Contains(Quote))
                s = s.Replace(Quote, EscapedQuote);

            if (s.IndexOfAny(CharactersThatMustBeQuoted) > -1)
                s = Quote + s + Quote;

            return s;
        }
        #endregion
    }
}
