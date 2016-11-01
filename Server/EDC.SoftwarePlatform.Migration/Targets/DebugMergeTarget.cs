// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Targets
{
	/// <summary>
	///     Debug target.
	/// </summary>
	internal class DebugMergeTarget : IMergeTarget
	{
		/// <summary>
		///     Deletes binary data entries.
		/// </summary>
		/// <param name="binaryData"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteBinaryData( IEnumerable<BinaryDataEntry> binaryData, IProcessingContext context )
		{
			foreach ( BinaryDataEntry binaryDataEntry in binaryData )
			{
				context.WriteInfo( "Deleted binary data: " + binaryDataEntry.DataHash );
			}
		}

        /// <summary>
        /// Deletes the document data.
        /// </summary>
        /// <param name="documentData">The document data.</param>
        /// <param name="context">The context.</param>
	    public void DeleteDocumentData(IEnumerable<DocumentDataEntry> documentData, IProcessingContext context)
	    {
            foreach (DocumentDataEntry documentDataEntry in documentData)
            {
                context.WriteInfo("Deleted document data: " + documentDataEntry.DataHash);
            }
        }

	    /// <summary>
		///     Deletes entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
			foreach ( EntityEntry entity in entities )
			{
				context.WriteInfo( "Deleted entity: " + entity.EntityId );
			}
        }

        /// <summary>
        ///     Deletes field data.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="data"></param>
        /// <param name="context"></param>
        void IMergeTarget.DeleteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
			foreach ( DataEntry entry in data )
			{
				context.WriteInfo( string.Format( "Deleted data: {0} {1}", entry.EntityId, entry.FieldId ) );
			}
		}

		/// <summary>
		///     Deletes relationships.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IMergeTarget.DeleteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
			foreach ( RelationshipEntry rel in relationships )
			{
				context.WriteInfo( string.Format( "Deleted relationship: {0} {1} {2}", rel.TypeId, rel.FromId, rel.ToId ) );
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
		}

		/// <summary>
		///     Set the application metadata.
		/// </summary>
		/// <param name="metadata"></param>
		/// <param name="context"></param>
		void IDataTarget.SetMetadata( Metadata metadata, IProcessingContext context )
		{
			context.WriteInfo( "Name: " + metadata.Name );
			context.WriteInfo( "Description: " + metadata.Description );
			context.WriteInfo( "Version: " + metadata.Version );
			context.WriteInfo( "Publisher: " + metadata.Publisher );
			context.WriteInfo( "PublisherUrl: " + metadata.PublisherUrl );
			context.WriteInfo( "ReleaseDate: " + metadata.ReleaseDate );
			context.WriteInfo( "AppId: " + metadata.AppId );
			context.WriteInfo( "AppVerId: " + metadata.AppVerId );
		}

		/// <summary>
		///     Called for binary data with new values.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="context"></param>
		void IMergeTarget.UpdateBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{
			foreach ( BinaryDataEntry entry in data )
			{
				context.WriteInfo( string.Format( "Updated binary data: {0}", entry.DataHash ) );
			}
		}

        /// <summary>
        /// Updates the document data.
        /// </summary>
        /// <param name="documentData">The document data.</param>
        /// <param name="context">The context.</param>
	    public void UpdateDocumentData(IEnumerable<DocumentDataEntry> documentData, IProcessingContext context)
	    {
            foreach (DocumentDataEntry documentDataEntry in documentData)
            {
                context.WriteInfo(string.Format("Updated binary data: {0}", documentDataEntry.DataHash));
            }
        }

	    /// <summary>
		///     Called for field data with new values.
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		void IMergeTarget.UpdateFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
			foreach ( DataEntry entry in data )
			{
				context.WriteInfo( string.Format( "Updated data: {0} {1} {2}", entry.EntityId, entry.FieldId, entry.Data ) );
			}
		}

		/// <summary>
		///     Relationships *may* change if their EntityId has been changed (although this is unlikely in practice)
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IMergeTarget.UpdateRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
			foreach ( RelationshipEntry rel in relationships )
			{
				context.WriteInfo( string.Format( "Updated relationship: {0} {1} {2}", rel.TypeId, rel.FromId, rel.ToId ) );
			}
		}

		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		void IDataTarget.WriteBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{
			foreach ( BinaryDataEntry dataEntry in data )
			{
				context.WriteInfo( string.Format( "Added Binary DataHash:{0}", dataEntry.DataHash ) );
			}
		}

	    public void WriteDocumentData(IEnumerable<DocumentDataEntry> data, IProcessingContext context)
	    {
            foreach (DocumentDataEntry dataEntry in data)
            {
                context.WriteInfo(string.Format("Added Document DataHash:{0}", dataEntry.DataHash));
            }
        }

	    /// <summary>
		///     Write in collection of entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="context"></param>
		void IDataTarget.WriteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
			foreach ( EntityEntry entity in entities )
			{
				context.WriteInfo( "Added entity: " + entity.EntityId );
			}
		}

		/// <summary>
		///     Write in collection of field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that data types are converted to their correct internal storage formats (e.g. 1/0 for bits in SqLite)
		///     - ensure that XML is transformed so that entityRefs are remapped to the local ID space. Entities will be received as either upgradeId or upgradeId|alias
		///     - ensure that aliases import their namespace and direction marker.
		/// </remarks>
		void IDataTarget.WriteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
			foreach ( DataEntry entry in data )
			{
				context.WriteInfo( string.Format( "Added data: {0} {1} {2}", entry.EntityId, entry.FieldId, entry.Data ) );
			}
		}

		/// <summary>
		///     Write in collection of relationships.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IDataTarget.WriteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
			foreach ( RelationshipEntry rel in relationships )
			{
				context.WriteInfo( string.Format( "Added relationship: {0} {1} {2}", rel.TypeId, rel.FromId, rel.ToId ) );
			}
		}

        /// <summary>
        ///     Write in collection of relationships.
        /// </summary>
        /// <param name="relationships"></param>
        /// <param name="context"></param>
        void IDataTarget.WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context)
        {
            foreach (var entry in data)
            {
                context.WriteInfo($"Added secure entry: {entry.SecureId} {entry.Context} Length:{entry.Data.Length}");
            }
        }

        /// <summary>
        ///     Write list of entities that should not be removed during upgrade operations.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        void IDataTarget.WriteDoNotRemove( IEnumerable<Guid> entities, IProcessingContext context )
        {
            foreach ( Guid entity in entities )
            {
                context.WriteInfo( "Do not remove: " + entity );
            }
        }

        /// <summary>
        ///     Sets up this instance.
        /// </summary>
        /// <param name="context">The context.</param>
        void IDataTarget.Setup( IProcessingContext context )
		{
		}

		/// <summary>
		///     Teardowns this instance.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.TearDown( IProcessingContext context )
		{
		}

		/// <summary>
		/// Method executed prior to entity/relationship deletion.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="context">The context.</param>
		public void PreDeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
		}
	}
}