// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Xml;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing.Xml.Version1;
using EDC.SoftwarePlatform.Migration.Processing.Xml.Version2;
using ReadiNow.ImportExport;

namespace EDC.SoftwarePlatform.Migration.Targets
{
	/// <summary>
	///     Class representing the XmlPackageTarget type.
	/// </summary>
	/// <seealso cref="EDC.SoftwarePlatform.Migration.Contract.IDataTarget" />
	public class XmlPackageTarget : IDataTarget
	{
		/// <summary>
		///     The xml serializer.
		/// </summary>
		private IXmlApplicationSerializer _serializer;

	    /// <summary>
		///     The package data.
		/// </summary>
		private PackageData _packageData;

        /// <summary>
        ///     Gets or sets the XML writer.
        /// </summary>
        /// <value>
        ///     The XML writer.
        /// </value>
        public XmlWriter XmlWriter
		{
			private get;
			set;
		}

		/// <summary>
		///     Gets or sets the name resolver.
		/// </summary>
		/// <value>
		///     The name resolver.
		/// </value>
		public INameResolver NameResolver
		{
			private get;
			set;
        }

        /// <summary>
        ///     The XML format.
        /// </summary>
        public Format Format
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the version override.
        /// </summary>
        /// <value>
        ///     The version override.
        /// </value>
        public string VersionOverride
		{
			get;
			set;
        }

        /// <summary>
        ///     Optional. Object containing additional settings to use when exporting.
        /// </summary>
        /// <value>
        ///     The settings object, or null.
        /// </value>
        public EntityXmlExportSettings EntityXmlExportSettings
        {
            get;
            set;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose( )
		{
			XmlWriter?.Dispose( );
		}

		void IDataTarget.SetMetadata( Metadata metadata, IProcessingContext context )
		{
            _packageData.Metadata = metadata;
		}

		/// <summary>
		///     Setups the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <exception cref="System.InvalidOperationException">No xml writer specified.</exception>
		void IDataTarget.Setup( IProcessingContext context )
		{
            /////
            // TODO: Use Autofac to inject appropriate serializer. Need to check version in the xml header.
            /////
		    switch ( Format )
		    {
                case Format.Xml:
                    _serializer = new XmlSerializer
                    {
                        NameResolver = NameResolver,
                        VersionOverride = VersionOverride
                    };
                    break;

                case Format.XmlVer2:
                    _serializer = new XmlSerializerV2
                    {
                        NameResolver = NameResolver,
                        VersionOverride = VersionOverride
                    };
                    break;

                default:
		            throw new InvalidOperationException( $"XmlPackageTarget cannot be used with {Format} format." );
		    }

		    _packageData = new PackageData( );
		}

		/// <summary>
		///     Tears down.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataTarget.TearDown( IProcessingContext context )
		{
		    _serializer.PackageData = _packageData;

            _serializer.Serialize( XmlWriter );
        }

		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		void IDataTarget.WriteBinaryData( IEnumerable<BinaryDataEntry> data, IProcessingContext context )
		{
            _packageData.Binaries = data;
		}

		/// <summary>
		///     Writes the binary data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="context">The context.</param>
		void IDataTarget.WriteDocumentData( IEnumerable<DocumentDataEntry> data, IProcessingContext context )
		{
            _packageData.Documents = data;
		}

		/// <summary>
		///     Write in collection of entities.
		/// </summary>
		/// <param name="entities"></param>
		/// <param name="context"></param>
		void IDataTarget.WriteEntities( IEnumerable<EntityEntry> entities, IProcessingContext context )
		{
            _packageData.Entities = entities;
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
		///     - ensure that XML is transformed so that entityRefs are remapped to the local ID space. Entities will be received
		///     as either uid or uid|alias
		///     - ensure that aliases import their namespace and direction marker.
		/// </remarks>
		void IDataTarget.WriteFieldData( string dataTable, IEnumerable<DataEntry> data, IProcessingContext context )
		{
		    if ( _packageData.FieldData == null )
		        _packageData.FieldData = new Dictionary<string, IEnumerable<DataEntry>>( );

            _packageData.FieldData[ dataTable ] = data;
		}

		/// <summary>
		///     Write in collection of relationships.
		/// </summary>
		/// <param name="relationships"></param>
		/// <param name="context"></param>
		void IDataTarget.WriteRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context )
		{
            _packageData.Relationships = relationships;
		}

        void IDataTarget.WriteSecureData(IEnumerable<SecureDataEntry> data, IProcessingContext context)
        {
            _packageData.SecureData = data;
        }

        /// <summary>
        ///     Write list of entities that should not be removed during upgrade operations.
        /// </summary>
        void IDataTarget.WriteDoNotRemove( IEnumerable<Guid> data, IProcessingContext context )
        {
            _packageData.DoNotRemove = data;
        }
    }
}