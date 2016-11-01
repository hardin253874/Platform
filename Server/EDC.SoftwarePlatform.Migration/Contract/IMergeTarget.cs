// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	internal interface IMergeTarget : IDataTarget
	{
		/// <summary>
		///     Deletes binary data entries.
		/// </summary>
		void DeleteBinaryData( IEnumerable<BinaryDataEntry> binaryData, IProcessingContext context );

		/// <summary>
		///     Deletes the document data.
		/// </summary>
		/// <param name="binaryData">The binary data.</param>
		/// <param name="context">The context.</param>
		void DeleteDocumentData( IEnumerable<DocumentDataEntry> binaryData, IProcessingContext context );

		/// <summary>
		///     Deletes entities.
		/// </summary>
		void DeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context );

		/// <summary>
		///     Deletes field data.
		/// </summary>
		void DeleteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context );

		/// <summary>
		///     Deletes relationships.
		/// </summary>
		void DeleteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context );

		/// <summary>
		///     Method executed prior to entity/relationship deletion.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="context">The context.</param>
		void PreDeleteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context );

		/// <summary>
		///     Called for binary data with new values.
		/// </summary>
		void UpdateBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context );

		/// <summary>
		///     Updates the document data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		void UpdateDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context );

		/// <summary>
		///     Called for field data with new values.
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		void UpdateFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context );

		/// <summary>
		///     Relationships *may* change if their EntityId has been changed (although this is unlikely in practice)
		///     Implementers may wish to use the same implementation as for WriteFieldData.
		/// </summary>
		void UpdateRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context );
	}
}