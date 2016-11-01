// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     IDataTarget interface.
	/// </summary>
	internal interface IDataTarget : IDisposable
	{
		/// <summary>
		///     Set the application metadata.
		/// </summary>
		void SetMetadata( Metadata metadata, IProcessingContext context );

		/// <summary>
		///     Setups the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		void Setup( IProcessingContext context );

		/// <summary>
		///     Tears down.
		/// </summary>
		/// <param name="context">The context.</param>
		void TearDown( IProcessingContext context );

        /// <summary>
        ///     Writes the binary data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="context">The context.</param>
        void WriteBinaryData(IEnumerable<BinaryDataEntry> data, IProcessingContext context);

        /// <summary>
        ///     Writes the binary data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="context">The context.</param>
        void WriteDocumentData(IEnumerable<DocumentDataEntry> data, IProcessingContext context);

        /// <summary>
		///     Write in collection of entities.
		/// </summary>
		void WriteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context );

		/// <summary>
		///     Write in collection of field data.
		/// </summary>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that data types are converted to their correct internal storage formats (e.g. 1/0 for bits in sqllite)
		///     - ensure that XML is transformed so that entityRefs are remapped to the local ID space. Entities will be received as either uid or uid|alias
		///     - ensure that aliases import their namespace and direction marker.
		/// </remarks>
		void WriteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context );

		/// <summary>
		///     Write in collection of relationships.
		/// </summary>
		void WriteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context );

        /// <summary>
        ///     Write in collection of secureData entries.
        /// </summary>
        void WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context);

        ///// <summary>
        /////     Write list of entities that should not be removed during upgrade operations.
        ///// </summary>
        void WriteDoNotRemove( IEnumerable<Guid> data, IProcessingContext context );
    }
}