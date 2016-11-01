// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Web.Http;
using System.Linq;
using System.IO;
using System.Xml;
using EDC.ReadiNow.Core;
using ReadiNow.ImportExport;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.ReadiNow.Model;
using System.Net.Http;
using EDC.Exceptions;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportExport
{
    [RoutePrefix( "data/v2" )]
    public class ImportExportXmlController : ApiController
    {
        /// <summary>
        ///     Import an XML resource file.
        /// </summary>
        /// <param name="fileToken">Token of the file to use</param>
        [Route( "importXml" )]
        [HttpGet]
		public HttpResponseMessage<ImportResult> ImportXmlGetData( [FromUri] string fileId, [FromUri] string fileName = null, [FromUri] bool ignoreDeps = false )
        {
            IEntityXmlImporter importer = Factory.EntityXmlImporter;

            EntityXmlImportResult serviceResult;

            // Decode settings
            var settings = new EntityXmlImportSettings
            {
                IgnoreMissingDependencies = ignoreDeps
            };

            // Import
            using ( Stream stream = FileRepositoryHelper.GetTemporaryFileDataStream( fileId ) )
            using ( var xmlReader = XmlReader.Create( stream ) )
            {
                serviceResult = importer.ImportXml( xmlReader, settings );
            }

            // Format result
            ImportResult webResult;
            if ( serviceResult.ErrorMessage != null )
            {
                webResult = new ImportResult
                {
                    Success = false,
                    Message = serviceResult.ErrorMessage
                };
            }
            else
            {
                IEntityRepository entityRepository = Factory.EntityRepository;
                var entities = entityRepository.Get<Resource>( serviceResult.RootEntities );

                webResult = new ImportResult
                {
                    Success = true,
                    Entities = entities.Select( FormatEntity ).ToList( )
                };
            }

            return new HttpResponseMessage<ImportResult>( webResult );
        }


        /// <summary>
        /// </summary>
        /// <param name="ids">Space separated list of IDs</param>
        /// <returns></returns>
        [Route( "exportXml" )]
        [HttpGet]
        public HttpResponseMessage GetXmlExport( [FromUri] string ids )
        {
            if ( ids == null )
                throw new WebArgumentException( "ids" );

            List<long> idList = ids.Split( ' ' ).Select( id => long.Parse( id ) ).ToList( );

            // Get name, and incidentally perform a root-level security check.
            string contentName;
            if ( idList.Count == 1 )
            {
                long entityId = idList[ 0 ];
                contentName = EDC.ReadiNow.Model.Entity.GetName( entityId );
            }
            else
            {
                EDC.ReadiNow.Model.Entity.Get( idList );
                contentName = "ReadiNow export";
            }
            

            string xml = Factory.EntityXmlExporter.GenerateXml( idList, EntityXmlExportSettings.Default );

            var response = new HttpResponseMessage( HttpStatusCode.OK )
            {
                Content = new StringContent( xml, Encoding.UTF8, "text/xml" )
            };

            // filename
            string safeChars = " -_()";
            string filename = string.Concat( contentName.Where( ch => char.IsLetterOrDigit( ch ) || safeChars.Contains( ch ) ) ) + ".xml";

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue( "attachment" )
            {
                FileName = filename
            };

            return response;
        }

        /// <summary>
        /// Format an imported entity result.
        /// </summary>
        private ImportResultEntry FormatEntity( Resource entity )
        {
            ImportResultEntry result = new ImportResultEntry
            {
                Name = entity.Name,
                TypeName = entity.IsOfType[0].Name
            };
            return result;
        }

    }
}