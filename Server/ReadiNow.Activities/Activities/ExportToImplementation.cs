// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.IO;
using ReadiNow.DocGen;
using EDC.SoftwarePlatform.Services.ExportData;
using ReadiNow.Reporting.Request;
using ReadiNow.ExportData;
using Autofac;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Implementation of the log activity available for use in Workflows.  Allows a workflow to add a message to the server logs.
    /// </summary>
    public class ExportToImplementation : ActivityImplementationBase, IRunNowActivity
    {
        private const string DescriptionFormat = "Report {0} exported on {1}";

        /// <summary>
        /// Runs when the activity is run by the workflow.
        /// </summary>
        /// <param name="context">The run state.</param>
        /// <param name="inputs">The inputs.</param>
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            using (Profiler.Measure("ExportToImplementation.OnRunNow"))
            {
                var name = GetArgumentValue<string>(inputs, "inExportToName");
                var report = GetArgumentEntity<Report>(inputs, "inExportToReport");
                var format = GetArgumentEntity<ExportFileTypeEnum>(inputs, "inExportToFormat");

                if (String.IsNullOrWhiteSpace(name))
                    name = GenerateName(report.Name);

                var description = string.Format(DescriptionFormat, report.Name ?? "[Unnamed]", DateTime.Now.ToShortTimeString());

                Document generatedDoc;

                using (CustomContext.SetContext(context.EffectiveSecurityContext))
                {

                    var timeZone = RequestContext.GetContext().TimeZone ?? TimeZoneHelper.SydneyTimeZoneName;

                    generatedDoc = ExportTo(report, name, description, timeZone, ToExportFormat(format));
                }

                context.SetArgValue(ActivityInstance, GetArgumentKey("core:outExportToDoc"), generatedDoc);
            }            
        }


        /// <summary>
        /// Generate an exported report
        /// </summary>
        /// <param name="Report">The report to run/param>
        /// <param name="newName">The name of the generated file</param>
        /// <param name="newDescription">The name of the generated description</param>
        /// <param name="timezoneName">the timezone the report will be evaluated in</param>
        /// <param name="exportFormat">The format for exporting</param>
        public static Document ExportTo(Report report, string newName, string newDescription, string timezoneName, ExportFormat exportFormat)
        {
            var settings = new ExportSettings
            {
                TimeZone = timezoneName,
                Format = exportFormat
            };

            var exportInterface = Factory.Current.Resolve<IExportDataInterface>();

            ExportInfo exportInfo = null;

            try
            {
                exportInfo = exportInterface.ExportData(report.Id, settings);
            }
            catch (Exception ex)
            {
                throw new WorkflowRunException("Failed to run report", ex);
            }

            var extension = ToExtension(exportFormat);
            var documentType = ToDocType(exportFormat);

            var doc = DocHelper.CreateDoc(exportInfo.FileHash, newName, newDescription, extension, documentType);

            return doc;
        }

        static string GenerateName(string reportName)
        {
            return reportName + ' ' + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert a format to a document type 
        /// </summary>
        static DocumentType ToDocType(ExportFormat format)
        {
            string alias = null;

            switch(format)
            {
                case ExportFormat.Csv:    alias =   "textDocumentDocumentType";     break;
                case ExportFormat.Excel:  alias =   "excelSpreadsheetDocumentType"; break;
                case ExportFormat.Word:   alias =   "wordDocumentDocumentType";     break;
                default: throw new ArgumentException();
            }

            return Entity.Get<DocumentType>(new EntityRef(alias));
        }

        /// <summary>
        ///     Convert a format to a extension stringe 
        /// </summary>
        static string ToExtension(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Csv:  return "csv";
                case ExportFormat.Excel: return "xlsx";
                case ExportFormat.Word: return "docx";
                default: throw new ArgumentException();
            }
        }

        static ExportFormat ToExportFormat(ExportFileTypeEnum format)
        {
            switch (format.Alias)
            {
                case "core:exportFileTypeCsv": return ExportFormat.Csv;
                case "core:exportFileTypeExcel": return  ExportFormat.Excel;
                case "core:exportFileTypeWord": return ExportFormat.Word;
                default: throw new ArgumentException();
            }
        }
    }
}
