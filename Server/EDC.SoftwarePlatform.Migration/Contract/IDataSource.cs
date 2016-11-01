// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	internal interface IDataSource : IDisposable
	{
        /// <summary>
        ///     Gets the binary data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        IEnumerable<BinaryDataEntry> GetBinaryData(IProcessingContext context);

        /// <summary>
        ///     Gets the binary data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        IEnumerable<DocumentDataEntry> GetDocumentData(IProcessingContext context);

        /// <summary>
		///     Load entities.
		/// </summary>
		IEnumerable<EntityEntry> GetEntities( IProcessingContext context );

		/// <summary>
		///     Load field data.
		/// </summary>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that bits are represented as Booleans
		///     - ensure that XML is transformed so that entityRefs contain Upgrade ids
		///     - or where XML contains an alias, translate it to uprgadeId|alias   (as the alias may be changed in the target)
		///     - ensure that aliases export their namespace and direction marker.
		/// </remarks>
		IEnumerable<DataEntry> GetFieldData( string dataTable, IProcessingContext context );

		/// <summary>
		/// Gets the missing field data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		IEnumerable<DataEntry> GetMissingFieldData( IProcessingContext context );

		/// <summary>
		///     Loads the application metadata.
		/// </summary>
		Metadata GetMetadata( IProcessingContext context );

		/// <summary>
		///     Load relationships.
		/// </summary>
		IEnumerable<RelationshipEntry> GetRelationships( IProcessingContext context );

		/// <summary>
		/// Gets the missing relationships.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		IEnumerable<RelationshipEntry> GetMissingRelationships( IProcessingContext context );

        /// <summary>
        /// Gets the secure data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        IEnumerable<SecureDataEntry> GetSecureData(IProcessingContext context);

        /// <summary>
        /// Gets the entities that should not be removed as part of an upgrade operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        IEnumerable<Guid> GetDoNotRemove( IProcessingContext context );

        /// <summary>
        ///     Sets up this instance.
        /// </summary>
        /// <param name="context">The context.</param>
        void Setup( IProcessingContext context );

		/// <summary>
		///     Tears down this instance.
		/// </summary>
		/// <param name="context">The context.</param>
		void TearDown( IProcessingContext context );
	}
}