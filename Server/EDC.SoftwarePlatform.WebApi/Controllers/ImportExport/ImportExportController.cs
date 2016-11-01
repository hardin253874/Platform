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

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportExport
{
    [RoutePrefix( "data/v2" )]
    public class ImportXmlController : ApiController
    {
        /// <summary>
        ///     Import an XML resource file.
        /// </summary>
        /// <param name="fileToken">Token of the file to use</param>
        [Route( "importXml" )]
        [HttpGet]
		public HttpResponseMessage<ImportResult> ImportXmlPostData( [FromUri] string fileId, [FromUri] string fileName = null )
        {
            IEntityXmlImporter importer = Factory.EntityXmlImporter;

            EntityXmlImportResult serviceResult;

            using ( Stream stream = FileRepositoryHelper.GetTemporaryFileDataStream( fileId ) )
            using ( var xmlReader = XmlReader.Create( stream ) )
            {
                serviceResult = importer.ImportXml( xmlReader, EntityXmlImportSettings.Default );
            }

            IEntityRepository entityRepository = Factory.EntityRepository;
            var entities = entityRepository.Get<Resource>( serviceResult.RootEntities );

            var result = new ImportResult( );
            result.Entities = entities.Select( FormatEntity ).ToList( );

            return new HttpResponseMessage<ImportResult>( result );
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