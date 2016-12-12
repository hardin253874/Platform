// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Net;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.Connector.ImportSpreadsheet;

namespace ReadiNow.Connector.EndpointTypes
{
    /// <summary>
    /// 
    /// </summary>
    class SpreadsheetEndpoint
    {
        private readonly ISpreadsheetImporter _spreadsheetImporter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spreadsheetImporter">Service for starting a spreadsheet import.</param>
        public SpreadsheetEndpoint( ISpreadsheetImporter spreadsheetImporter )
        {
            if ( spreadsheetImporter == null )
                throw new ArgumentNullException( nameof( spreadsheetImporter ) );

            _spreadsheetImporter = spreadsheetImporter;
        }

        /// <summary>
        /// Process the request.
        /// </summary>
        /// <remarks>
        /// Assumes that user context has already been set.
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public ConnectorResponse HandleRequest( ConnectorRequest request, ApiSpreadsheetEndpoint endpoint )
        {
            if ( request == null )
                throw new ArgumentNullException( "request" );
            if ( endpoint == null )
                throw new ArgumentNullException( "endpoint" );
            if ( request.Verb != ConnectorVerb.Post )
                return new ConnectorResponse( HttpStatusCode.MethodNotAllowed );

            long importConfigId;
            using ( new SecurityBypassContext( ) )
            {
                if ( endpoint.EndpointImportConfig == null )
                    throw new ConnectorConfigException( "Endpoint does not point to an import configuration." );
                importConfigId = endpoint.EndpointImportConfig.Id;
            }

            ImportSettings settings = new ImportSettings
            {
                FileToken = request.FileUploadToken,
                ImportConfigId = importConfigId,
                TestRun = false,
                TimeZoneName = null, // Imports via WebAPI treat values as UTC  (but should they?)
                SuppressSecurityCheckOnImportConfig = true
            };

            // Start import
            try
            {
                _spreadsheetImporter.StartImport( settings );
                return new ConnectorResponse( HttpStatusCode.OK );
            }
            catch
            {
                return new ConnectorResponse( HttpStatusCode.InternalServerError );
            }
        }
    }
}
