// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Represents a reader that returns no content at all. Used as a base-line for diff.
	/// </summary>
	internal class EmptySource : IDataSource
	{
		/// <summary>
		///     Clean up
		/// </summary>
		public void Dispose( )
		{
		}

		/// <summary>
		///     Gets the binary data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<BinaryDataEntry> GetBinaryData( IProcessingContext context )
		{
			return Enumerable.Empty<BinaryDataEntry>( );
		}

		public IEnumerable<DocumentDataEntry> GetDocumentData( IProcessingContext context )
		{
			return Enumerable.Empty<DocumentDataEntry>( );
		}

		/// <summary>
		///     Return empty set of entities.
		/// </summary>
		public IEnumerable<EntityEntry> GetEntities( IProcessingContext context )
		{
			return Enumerable.Empty<EntityEntry>( );
		}

		/// <summary>
		///     Return empty set of field data.
		/// </summary>
		public IEnumerable<DataEntry> GetFieldData( string dataTable, IProcessingContext context )
		{
			return Enumerable.Empty<DataEntry>( );
		}

		/// <summary>
		///     Loads the application metadata.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public Metadata GetMetadata( IProcessingContext context )
		{
			return new Metadata
			{
				AppId = Guid.Empty,
				AppName = "Empty",
				AppVerId = Guid.Empty,
				Description = "Empty",
				Name = "Empty",
				Version = "1",
                Publisher = "Readinow Corporation",
                PublisherUrl = "http://www.readinow.com",
				ReleaseDate = DateTime.UtcNow,
				Dependencies = null,
				Type = SourceType.AppPackage,
				PlatformVersion = null
			};
		}

		/// <summary>
		///     Return empty set of relationships.
		/// </summary>
		public IEnumerable<RelationshipEntry> GetRelationships( IProcessingContext context )
		{
			return Enumerable.Empty<RelationshipEntry>( );
		}

        /// <summary>
        ///     Return empty set of SecureData
        /// </summary>
        public IEnumerable<SecureDataEntry> GetSecureData(IProcessingContext context)
        {
            return Enumerable.Empty<SecureDataEntry>();
        }

        /// <summary>
        /// Gets the entities that should not be removed as part of an upgrade operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public IEnumerable<Guid> GetDoNotRemove( IProcessingContext context )
        {
            return Enumerable.Empty<Guid>( );
        }

        /// <summary>
        ///     Sets up this instance.
        /// </summary>
        public void Setup( IProcessingContext context )
		{
		}

		/// <summary>
		///     Tears down this instance.
		/// </summary>
		public void TearDown( IProcessingContext context )
		{
		}


		/// <summary>
		///     Gets the missing field data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<DataEntry> GetMissingFieldData( IProcessingContext context )
		{
			return Enumerable.Empty<DataEntry>( );
		}

		/// <summary>
		///     Gets the missing relationships.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public IEnumerable<RelationshipEntry> GetMissingRelationships( IProcessingContext context )
		{
			return Enumerable.Empty<RelationshipEntry>( );
		}
	}
}