// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using EDC.SoftwarePlatform.Services.ExportData;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using ReportParameters = EDC.SoftwarePlatform.WebApi.Controllers.Report.ReportParameters;
using EDC.SoftwarePlatform.WebApi.Controllers.Report;
using EDC.Exceptions;
using EDC.ReadiNow.IO;
using ReadiNow.ExportData;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ExportData
{
    [RoutePrefix("data/v2/report/export")]
    public class ExportDataController : ApiController
    {
        /// <summary>
        ///     Exports a report for a given report identifier, analyser filter and sort conditions and generate a document for
        ///     given format..
        /// </summary>
        /// <param name="rid">The rid.</param>
        /// <param name="format">File format to export.</param>
        /// <param name="reportParameters">The report parameters.</param>
        /// <returns>
        ///     HttpResponseMessage{string}.
        /// </returns>
        [Route("{rid}/{format}")]
        [HttpPost]
        public HttpResponseMessage<ExportDataInfo> ExportData(long rid, string format, [FromBody] ReportParameters reportParameters)
        {
            WebApiHelpers.CheckEntityId<ReadiNow.Model.Report>("rid", rid);

            var queryString = Request.RequestUri.ParseQueryString();
            var settings = ReportController.SettingsFromQuery(queryString, reportParameters);
            settings.RequireFullMetadata = true;

            ExportFormat exportFormat;
            if (!Enum.TryParse(format, true, out exportFormat))
            {
                throw new WebArgumentException("Unknown format", "format");
            }

            var exportInterface = new ExportDataInterface();
            ExportInfo exportInfo = exportInterface.ExportData(rid, settings, exportFormat);
            return new HttpResponseMessage<ExportDataInfo>(PackResult(exportInfo), HttpStatusCode.OK);
        }

        /// <summary>
        ///     Download exported document.
        /// </summary>
        /// <param name="token">file hash</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="format">File format</param>
        /// <returns>
        ///     HttpResponseMessage
        /// </returns>
        [Route("download/{token}/{fileName}/{format}")]
        [HttpGet]
        public HttpResponseMessage GetExportDocument(string token, string fileName, string format)
        {
            Stream stream = FileRepositoryHelper.GetTemporaryFileDataStream(token);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            fileName = fileName + GetFileExtensionFromFormat(format);
            // Note: We are not setting the content length or the mime type
            // because the CompressionHandler will compress the stream.
            // Specifying a mimetype specified here will cause the browser (Chrome at least) to log a
            // message.
            // Specifying the length here will cause the browser to hang as the actual data it
            // receives (as it is compressed) will be less than the specified content length.
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            return response;
        }

        /// <summary>
        ///     Pack the export data info.
        /// </summary>
        /// <param name="exportInfo">ExportInfo</param>
        /// <returns>
        ///     ExportDataInfo
        /// </returns>
        private static ExportDataInfo PackResult(ExportInfo exportInfo)
        {
            var result = new ExportDataInfo
            {
                FileHash = exportInfo.FileHash,
                ResponseMessage = exportInfo.ResponseMessage
            };
            return result;
        }

        /// <summary>
        ///     Get the file extension from file format.
        /// </summary>
        /// <param name="format">File format</param>
        /// <returns>
        ///     Extension of the file
        /// </returns>
        private static string GetFileExtensionFromFormat(string format)
        {
            switch (format)
            {
                case "excel":
                    {
                        return ".xlsx";
                    }
                case "word":
                    {
                        return ".docx";
                    }
                case "csv":
                    {
                        return ".csv";
                    }
            }
            return "";
        }
    }
}