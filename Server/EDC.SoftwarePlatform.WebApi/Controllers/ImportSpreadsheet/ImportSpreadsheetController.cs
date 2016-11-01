// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Exceptions;
using EDC.ReadiNow.Core;
using ReadiNow.Connector.ImportSpreadsheet;
using Interface = ReadiNow.Connector.ImportSpreadsheet;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
	[RoutePrefix( "data/v2/importSpreadsheet" )]
	public class ImportSpreadsheetController : ApiController
	{
        ISpreadsheetInspector _spreadsheetInspector;
        ISpreadsheetImporter _spreadsheetImporter;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _spreadsheetInspector = Factory.Current.Resolve<ISpreadsheetInspector>( );
            _spreadsheetImporter = Factory.Current.Resolve<ISpreadsheetImporter>();
        }


        /// <summary>
        ///     Start a new import run
        /// </summary>
        /// <param name="importConfigId">ID of import configuration to use.</param>
        /// <param name="fileToken">Token of the file to use</param>
        /// <param name="testRun">If true, then this is a test import.</param>
        /// <returns>
        ///     Import taskId
        /// </returns>
        [Route( "import" )]
        [HttpGet]
        public HttpResponseMessage<long> ImportSpreadsheetData( [FromUri(Name = "config")] long importConfigId, [FromUri( Name = "file" )] string fileToken, [FromUri(Name = "filename")] string fileName, [FromUri( Name = "testrun" )] bool testRun = false )
        {
            if ( importConfigId <= 0 )
                throw new WebArgumentException( "importConfigId" );
            if ( string.IsNullOrEmpty( fileToken ) )
                throw new WebArgumentException( "fileToken" );

            string tzName = null;
            if (Request.Headers.Contains("tz"))
                tzName = Request.Headers.GetValues( "tz" ).FirstOrDefault( );

            ImportSettings importSettings = new ImportSettings
            {
                ImportConfigId = importConfigId,
                FileToken = fileToken,
                FileName = fileName,
                TimeZoneName = tzName,
                TestRun = testRun
            };

            long importRunId = _spreadsheetImporter.StartImport( importSettings );

            return new HttpResponseMessage<long>( importRunId, HttpStatusCode.OK );
        }


        /// <summary>
        ///     Get status of an import.
        /// </summary>
        /// <param name="importRunId">ID of import run</param>
        /// <returns>Import result</returns>
        [HttpGet]
	    [Route( "import/{importRunId}" )]
	    public HttpResponseMessage<ImportResultInfo> GetImportStatus( long importRunId )
        {
            if ( importRunId <= 0 )
                throw new WebArgumentOutOfRangeException( nameof( importRunId ) );

	        var result = _spreadsheetImporter.GetImportStatus( importRunId );

	        return new HttpResponseMessage<ImportResultInfo>( PackImportResultInfo( result ), HttpStatusCode.OK );
        }


        /// <summary>
        ///     Cancel an import operation.
        /// </summary>
        /// <param name="importRunId">ID of import run to cancel</param>
        /// <returns>Import result</returns>
        [Route( "cancel/{importRunId}" )]
        [HttpGet]
        public HttpResponseMessage<ImportResultInfo> CancelImport( long importRunId )
        {
            if ( importRunId <= 0 )
                throw new WebArgumentOutOfRangeException( nameof( importRunId ) );

            var result = _spreadsheetImporter.CancelImportOperation( importRunId );

            return new HttpResponseMessage<ImportResultInfo>( PackImportResultInfo( result ), HttpStatusCode.OK );
        }


        /// <summary>
        ///     Get sample data from a spreadsheet.
        /// </summary>
        /// <param name="hrow">Header row no of the sheet</param>
        /// <param name="drow">Data row no of the sheet</param>
        /// <param name="last">Last data row no of the sheet</param>
        /// <param name="fileInfo">The file information.</param>
        /// <param name="sheet">sheet info</param>
        /// <returns>
        ///     Sheet Info
        /// </returns>
        [Route( "sample" )]
        [HttpGet]
		public HttpResponseMessage<SampleTable> GetSampleTable( [FromUri] int hrow, [FromUri] int drow, [FromUri] FileInfo fileInfo, [FromUri] string sheet, [FromUri] int? last = null )
        {
            if ( fileInfo == null )
                throw new WebArgumentNullException( nameof( fileInfo ) );
            if ( hrow < 0 )
                throw new WebArgumentOutOfRangeException( nameof( hrow ) );
            if ( drow < 0 )
                throw new WebArgumentOutOfRangeException( nameof( drow ) );

            Interface.SampleTable sampleTable;

            sampleTable = _spreadsheetInspector.GetSampleTable(fileInfo.FileId, GetFileFormat(fileInfo.FileFormat), sheet, hrow, drow, last );

            if ( sampleTable == null)
            {
                return new HttpResponseMessage<SampleTable>(HttpStatusCode.NotFound);
            }

			return new HttpResponseMessage<SampleTable>( PackSampleTable( sampleTable ), HttpStatusCode.OK );
		}


        /// <summary>
        ///     Get spreadsheet Information from the imported file.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <param name="sheet">Optionally, the sheet to initially load sample data for.</param>
        /// <returns>
        ///     Spreadsheet Info.
        /// </returns>
        [Route( "sheet" )]
        [HttpGet]
		public HttpResponseMessage<SpreadsheetInfo> GetSpreadsheetInfo( [FromUri] FileInfo fileInfo, [FromUri] string sheet )
		{
            if ( fileInfo == null )
                throw new WebArgumentNullException( nameof( fileInfo ) );

            var errorResult = new SpreadsheetInfo( );
			
            Interface.SpreadsheetInfo result;
            Interface.SampleTable sampleTable;
            string initialSheetId = null;

            try
			{
				result = _spreadsheetInspector.GetSpreadsheetInfo( fileInfo.FileId, GetFileFormat( fileInfo.FileFormat ) );

                if ( result == null )
                {
                    return new HttpResponseMessage<SpreadsheetInfo>( HttpStatusCode.NotFound );
                }

			    if ( result.SheetCollection != null )
			    {
                    if ( sheet != null )
                    {
                        initialSheetId = sheet;
                    }
                    else if ( result.SheetCollection != null && result.SheetCollection.Count > 0 )
                    {
                        // Assume a default
                        initialSheetId = result.SheetCollection [ 0 ].SheetId;
                    }
                }

			    sampleTable = _spreadsheetInspector.GetSampleTable( fileInfo.FileId, GetFileFormat( fileInfo.FileFormat ), initialSheetId, 1, 2, null );
			}
			catch ( FileFormatException ex )
			{
				errorResult.ErrorMessage = ex.Message;
				EventLog.Application.WriteError( "GetSpreadsheetInfo {0}", ex.Message );
				return new HttpResponseMessage<SpreadsheetInfo>( errorResult, HttpStatusCode.UnsupportedMediaType );
			}
            SpreadsheetInfo packedSpreadsheetInfo = PackSpreadsheetInfoResponse( result );
            if ( sampleTable != null )
            {
                packedSpreadsheetInfo.InitialSampleTable = PackSampleTable( sampleTable );
                packedSpreadsheetInfo.InitialSheetId = initialSheetId;
            }

            return new HttpResponseMessage<SpreadsheetInfo>( packedSpreadsheetInfo, HttpStatusCode.OK );
		}


		/// <summary>
		///     Packs the import result information.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		private ImportResultInfo PackImportResultInfo( Interface.ImportResultInfo result )
		{
			ImportStatus importStatus;
			var data = new ImportResultInfo
			{
                ImportStatus = Enum.TryParse( result.ImportStatus.ToString( ), true, out importStatus ) ? importStatus : ImportStatus.InProgress,
                ImportMessages = result.ImportMessages,
                RecordsSucceeded = result.RecordsSucceeded,
                RecordsFailed = result.RecordsFailed,
                RecordsTotal = result.RecordsTotal
			};
			return data;
		}


		/// <summary>
		///     Packs the spreadsheet information response.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		private static SpreadsheetInfo PackSpreadsheetInfoResponse( Interface.SpreadsheetInfo result )
		{
			ImportFormat importFormat;
			var sheetInfo = new SpreadsheetInfo
			{
				FileName = result.FileName,
				SheetCollection = result.SheetCollection != null && result.SheetCollection.Count > 0
					? new List<SheetInfo>( result.SheetCollection.Select( sheet => new SheetInfo
					{
						SheetName = sheet.SheetName,
						SheetId = sheet.SheetId
					} ) )
					: null,
				ImportFileFormat =
					Enum.TryParse( result.ImportFileFormat.ToString( ), true, out importFormat ) ? importFormat : ImportFormat.Excel
			};
			return sheetInfo;
        }

        /// <summary>
        ///     Packs the spreadsheet information response.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private static SampleTable PackSampleTable( Interface.SampleTable sampleTable )
        {
            SampleTable result = new SampleTable
            {
                Rows = sampleTable.Rows.Select(row => PackSampleRow( row ) ).ToList(),
                Columns = sampleTable.Columns.Select( column => PackSampleColumn( column ) ).ToList( ),
            };
            return result;
        }

        /// <summary>
        ///     Packs the spreadsheet information response.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private static SampleRow PackSampleRow( Interface.SampleRow sampleRow )
        {
            SampleRow result = new SampleRow
            {
                Values = sampleRow.Values
            };
            return result;
        }

        /// <summary>
        ///     Packs the spreadsheet information response.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private static SampleColumn PackSampleColumn( Interface.SampleColumn sampleColumn )
        {
            SampleColumn result = new SampleColumn
            {
                Name = sampleColumn.Name,
                ColumnName = sampleColumn.ColumnName
            };
            return result;
        }

        Interface.ImportFormat GetFileFormat( string importFileFormat )
        {
            // Note: .zip support works downstream if this method can be convinced to return  | Interface.ImportFormat.Zip;
            return ( Interface.ImportFormat )
                Enum.Parse( typeof( Interface.ImportFormat ), importFileFormat, true ); // | Interface.ImportFormat.Zip;
        }
    }
}