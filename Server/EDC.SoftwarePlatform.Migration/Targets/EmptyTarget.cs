// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Targets
{
	/// <summary>
	///     Empty target.
	/// </summary>
	internal class EmptyTarget : IMergeTarget
	{
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
		public void SetMetadata( Metadata metadata, IProcessingContext context )
		{
		}

		/// <summary>
		///     Setups the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public void Setup( IProcessingContext context )
		{
		}

		/// <summary>
		///     Tears down.
		/// </summary>
		/// <param name="context">The context.</param>
		public void TearDown( IProcessingContext context )
		{
		}

		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		public void WriteBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		public void WriteDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Write in collection of entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="context"></param>
		public void WriteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
		}

		/// <summary>
		///     Write in collection of field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that data types are converted to their correct internal storage formats (e.g. 1/0 for bits in sqllite)
		///     - ensure that XML is transformed so that entityRefs are remapped to the local ID space. Entities will be received as either uid or uid|alias
		///     - ensure that aliases import their namespace and direction marker.
		/// </remarks>
		public void WriteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Write in collection of relationships.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		public void WriteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
		}

        /// <summary>
        ///     Write in collection of secure data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        void IDataTarget.WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context)
        {
        }

        /// <summary>
        ///     Write list of entities that should not be removed during upgrade operations.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        void IDataTarget.WriteDoNotRemove( IEnumerable<Guid> entities, IProcessingContext context )
        {
        }

        /// <summary>
        ///     Deletes binary data entries.
        /// </summary>
        /// <param name="binaryData"></param>
        /// <param name="context"></param>
        public void DeleteBinaryData( IEnumerable<BinaryDataEntry> binaryData, IProcessingContext context )
		{
		}

		/// <summary>
		///     Deletes the document data.
		/// </summary>
		/// <param name="binaryData">The binary data.</param>
		/// <param name="context">The context.</param>
		public void DeleteDocumentData( IEnumerable<DocumentDataEntry> binaryData, IProcessingContext context )
		{
		}

		/// <summary>
		///     Deletes entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="context"></param>
		public void DeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
		}

		/// <summary>
		///     Deletes field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		public void DeleteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Deletes relationships.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		public void DeleteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
		}

		/// <summary>
		///     Called for binary data with new values.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="context"></param>
		public void UpdateBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Updates the document data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		public void UpdateDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Called for field data with new values.
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="data"></param>
		/// <param name="context"></param>
		public void UpdateFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
		}

		/// <summary>
		///     Relationships *may* change if their EntityId has been changed (although this is unlikely in practice)
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		public void UpdateRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
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