// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Services.LongRunningTask;
using ReadiNow.DocGen;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.Services.ReportTemplate
{
	/// <summary>
	///     Settings for document generation from a template.
	/// </summary>
	public class ReportTemplateSettings
	{
		public EntityRef ReportTemplate
		{
			get;
			set;
		}

		public List<EntityRef> SelectedResources
		{
			get;
			set;
		}

		public string TimeZone
		{
			get;
			set;
		}
	}

    /// <summary>
    ///     Settings for document generation from a template.
    /// </summary>
    public class RevisionData
    {
        public Stream Stream { get; set; }

        public string FileExtension { get; set; }
    }

	

	/// <summary>
	///     Defines a feature set for the report template functionality.
	/// </summary>
	public class ReportTemplateInterface
	{
		/// <summary>
		///     Start generating a document, combining report data and a given report template.
		/// </summary>
		/// <param name="settings">Settings used for the document generation.</param>
		/// <returns>An identifier or token that can be used to interact with the long running task.</returns>
		public string GenerateReportFromTemplate( ReportTemplateSettings settings )
		{
			LongRunningInfo taskInfo = LongRunningHelper.StartLongRunningInWorkerThread( info => GenerateDocumentWorker( settings, info ) );
			return taskInfo.TaskId.ToString( @"N" );
		}

		#region Private Methods

		/// <summary>
		///     Worker thread method to generate a report document.
		/// </summary>
		/// <param name="reportSettings">The report-template settings.</param>
		/// <param name="taskInfo">The long-running task info object.</param>
		private static void GenerateDocumentWorker( ReportTemplateSettings reportSettings, LongRunningInfo taskInfo )
		{
			// Load report template resource
			var reportTemplate = Entity.Get<ReadiNow.Model.ReportTemplate>( reportSettings.ReportTemplate );
			if ( reportTemplate == null )
				throw new Exception( "Report template could not be loaded." );

			// Load the document resource
			if ( reportTemplate.ReportTemplateUsesDocument == null )
				throw new Exception( "Report template has no associated template document." );

			long docId = reportTemplate.ReportTemplateUsesDocument.Id;

			// Load the document stream
			RevisionData templateData = GetLatestDocument( docId );
            Stream templateStream = templateData.Stream;
            string extension = templateData.FileExtension;

            // Verify supported types
            if (extension != ".docx" && extension != ".dotx" && extension != ".docm" && extension != ".dotm" )
            {
                throw new Exception( "Report template has an unsupported file extension." );
            }

            // Verify resource types
            VerifyResourceTypes( reportSettings, reportTemplate );

			// Fill settings
			var settings = new GeneratorSettings
			{
				TimeZoneName = reportSettings.TimeZone
			};
			if ( reportSettings.SelectedResources != null )
			{
				settings.SelectedResourceId = reportSettings.SelectedResources.First( ).Id;
			}

			// Generate the document
			var outputSteam = new MemoryStream( );
            Factory.DocumentGenerator.CreateDocument( templateStream, outputSteam, settings );
			outputSteam.Flush( );
			outputSteam.Position = 0;

			// Store file stream in the temporary table
			string token = PersistTemporaryFile( outputSteam );

			// Store the result into a document entity
			string filename = string.Format( "{0} {1:s}", reportTemplate.Name, DateTime.Now );
			long documentId = StoreFileInEntity( outputSteam, filename, extension, token );

			// Return token that is the document identifier that is used for the download of the document file entity.
			taskInfo.ResultData = documentId.ToString( CultureInfo.InvariantCulture );
			LongRunningHelper.SaveLongRunningTaskInfo( taskInfo );
		}

        /// <summary>
        /// Ensure that all resources are of the required type.
        /// </summary>
        internal static void VerifyResourceTypes( ReportTemplateSettings reportSettings, ReadiNow.Model.ReportTemplate reportTemplate )
        {
            EntityType appliesToType = reportTemplate.ReportTemplateAppliesToType;
            if ( appliesToType != null )
            {
                if ( reportSettings.SelectedResources == null || reportSettings.SelectedResources.Count == 0 )
                {
                    throw new Exception( "The report template must apply to a resource, but no resource was provided. ");
                }
                else
                {
                    foreach ( EntityRef resourceId in reportSettings.SelectedResources )
                    {
                        IEntity entity = Entity.Get( resourceId );
                        if ( !PerTenantEntityTypeCache.Instance.IsInstanceOf( entity, appliesToType.Id ) )
                        {
                            throw new Exception( string.Format( "The selected resource is not of the required type for this document. Expected type '{0}', but received '{1}' of type '{2}.'",
                                appliesToType.Name, Entity.GetName( resourceId.Id ), Entity.GetName( entity.TypeIds.First( ) ) ) );
                        }
                    }
                }

            }
        }

		/// <summary>
		///     Gets the current revision id for document id.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <returns></returns>
		/// <remarks></remarks>
		private static long GetCurrentRevisionIdForDocumentId( long entityId )
		{
			// Get the latest revision using the current document association (currentDocumentRevision)
			return Entity.Get<Document>( entityId ).CurrentDocumentRevision.Id;
		}

		/// <summary>
		///     Gets the current user account.
		/// </summary>
		/// <returns></returns>
		/// <remarks></remarks>
		private static UserAccount GetCurrentUserAccount( )
		{
			return Entity.Get<UserAccount>( RequestContext.GetContext( ).Identity.Id );
		}

		/// <summary>
		///     Gets the document stream for the specified entity identifier.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <returns>The open file stream for the document stored in the file stream table.</returns>
		/// <remarks></remarks>
        private static RevisionData GetDocumentStreamForRevisionId(long entityId)
		{
			Stream stream = null;
			// Get the document revision 
			var documentRevision = Entity.Get<DocumentRevision>( entityId );
			if ( documentRevision == null )
			{
				EventLog.Application.WriteError( "Unable to get the document revision stream, invalid document entityId {0}", entityId );
				return null;
			}

		    stream = FileRepositoryHelper.GetFileDataStreamForToken(Factory.DocumentFileRepository, documentRevision.FileDataHash);		                

            RevisionData result = new RevisionData
            {
                Stream = stream,
                FileExtension = (documentRevision.FileExtension ?? "").ToLowerInvariant().Trim()
            };
			return result;
		}

		/// <summary>
		///     Gets the latest document.
		/// </summary>
		/// <param name="documentEntityId">The document entity id.</param>
		/// <returns>A binary stream containing the document data.</returns>
		/// <remarks></remarks>
        public static RevisionData GetLatestDocument(long documentEntityId)
		{
			// Get the latest revision using the current document association (currentDocumentRevision) and return the binary stream
            long revisionId = GetCurrentRevisionIdForDocumentId( documentEntityId );
			return GetDocumentStreamForRevisionId( revisionId );
		}

		private static string PersistTemporaryFile( MemoryStream documentStream )
		{
		    return FileRepositoryHelper.AddTemporaryFile(documentStream);
		}

		private static long StoreFileInEntity( Stream fileStream, string filename, string extension, string token )
		{
			var document = Entity.Create<Document>( );
			document.Name = filename;

			// Create a new revision entity
			var revision = Entity.Create<DocumentRevision>( );
            revision.FileExtension = extension;
			revision.VersionComments = @"Generated document.";
			revision.Version = @"1.0";
			revision.Name = revision.Version;
			revision.Size = Convert.ToInt32( fileStream.Length );
			revision.ModifiedDate = DateTime.UtcNow;

			// Get the entity that represents the user
			UserAccount userAccount = GetCurrentUserAccount( );

			// Associate the revision to the account
			revision.RevisionUpdatedBy = userAccount;
			revision.Save( );

			// Associate the document to the revision (currentDocumentRevision)
			document.CurrentDocumentRevision = revision;

			// Associate the document to the revision (documentHasDocumentRevision - used for version history listing)
			document.DocumentHasDocumentRevision.Add( revision );

			// Associate the type to the document
			document.DocumentHasDocumentType = Entity.Get<DocumentType>( new EntityRef( "wordDocumentDocumentType" ) );
			document.DocumentFileType = Entity.Get<DocumentType>( new EntityRef( "wordDocumentDocumentType" ) );

			// Associate the created user to the document
			document.DocumentCreatedBy = userAccount;

			// Associate the last modified user to the document
			document.DocumentModifiedBy = userAccount;

			// Associate document to folder
			document.InFolder = Entity.Get<DocumentFolder>( new EntityRef( "generatedDocumentFolder" ) );
			document.FileDataHash = token;
            document.FileExtension = revision.FileExtension;
            document.Save( );

			revision.FileDataHash = document.FileDataHash;
			revision.Save( );
			return document.Id;
		}

		#endregion
	}
}