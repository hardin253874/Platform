// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using ReadiNow.Reporting;
using ReadiNow.Reporting.Request;
using ReadiNow.Reporting.Result;

namespace EDC.SoftwarePlatform.Services.ExportData
{
    public class ExportDataInterface
    {
        /// <summary>
        ///     Export Data for given file format.
        /// </summary>
        /// <param name="reportId">Report Id.</param>
        /// <param name="settings">Report settings</param>
        /// <param name="format">File format.</param>
        /// <returns>ExportInfo</returns>
        public ExportInfo ExportData(long reportId, ReportSettings settings, ExportFormat format)
        {
            var exportInfo = new ExportInfo();
            //set the page size to maximum.
            settings.SupportPaging = true;
            settings.InitialRow = 0;
            settings.PageSize = 10001; // The maximum number of records can be exported is limited to 10000.

            //to retrive the report object and run the report, dosen't need the writeable permission.
            var reportingInterface = new ReportingInterface();
            ReportResult reportResult = reportingInterface.RunReport( reportId, settings );

            // Ensure that the report result contains the report metadata (which is loaded in a security bypass context).
            // This ensures that when processing the grid data the required metadata is available
            // and will not throw security exceptions.            
            var metadata = reportResult.Metadata;

            //Remove the last record if the number of records are more then 10000.            
            List<DataRow> rows;
            if (reportResult.GridData.Count > 10000)
            {
                exportInfo.ResponseMessage = "There are more than 10,000 records in this report. Only the first 10,000 records will be downloaded.";
                rows = reportResult.GridData.GetRange(0, 10000);
            }
            else
            {
                rows = reportResult.GridData;
            }

            Stream fileStream;
            // Generate content
            switch (format)
            {
                case ExportFormat.Csv:
                    {
                        byte[] resultText = ExportToCsv.CreateCsvDocument(reportResult, rows);
                        fileStream = new MemoryStream(resultText);
                        break;
                    }
                case ExportFormat.Excel:
                    {
                        fileStream = ExportToExcel.CreateExcelDocument(reportResult, rows);
                        break;
                    }
                case ExportFormat.Word:
                    {
                        string reportName = Entity.GetName( reportId );
                        fileStream = ExportToWord.CreateWordDocument( reportResult, rows, reportName );
                        break;
                    }
                default:
                    throw new ArgumentException("fileFormat");
            }

            //save the file to the database.
            string hash;

            using (fileStream)
            {
                hash = FileRepositoryHelper.AddTemporaryFile(fileStream);
            }
            exportInfo.FileHash = hash;
            return exportInfo;
        }

        /// <summary>
        ///     Get the file extension from file format.
        /// </summary>
        /// <param name="format">File format</param>
        /// <returns>Extension of the file</returns>
        private static string GetFileExtensionFromFormat(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Excel:
                    {
                        return "xlsx";
                    }
                case ExportFormat.Word:
                    {
                        return "docx";
                    }
                case ExportFormat.Csv:
                    {
                        return "csv";
                    }
            }
            return "";
        }
    }

    /// <summary>
    ///     Defines the Export Format type.
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>
        ///     The Excel format type.
        /// </summary>
        Excel,

        /// <summary>
        ///     The CSV format type.
        /// </summary>
        Csv,

        /// <summary>
        ///     The Word format type.
        /// </summary>
        Word
    }

    /// <summary>
    ///     ExportInfo.
    /// </summary>
    public class ExportInfo
    {
        public string FileHash
        {
            get;
            set;
        }

        public string ResponseMessage
        {
            get;
            set;
        }
    }
}