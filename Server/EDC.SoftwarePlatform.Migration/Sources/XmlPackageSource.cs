// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing.Xml.Version1;
using EDC.SoftwarePlatform.Migration.Processing.Xml.Version2;

namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Class representing the XmlPackageSource type.
	/// </summary>
	/// <seealso cref="EDC.SoftwarePlatform.Migration.Contract.IDataSource" />
	public class XmlPackageSource : IDataSource
	{
		/// <summary>
        ///     Deserialized data.
        /// </summary>
        private PackageData _packageData;

        /// <summary>
        ///     The _XML deserializer
        /// </summary>
        private IXmlApplicationDeserializer _xmlDeserializer;

        /// <summary>
        ///     Gets or sets the file format version.
        /// </summary>
        public Format Version
        {
            private get;
            set;
        }

        /// <summary>
        ///     Gets or sets the xml text reader.
        /// </summary>
        /// <value>
        ///     The XML text reader.
        /// </value>
        public XmlReader XmlReader
		{
			private get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		void IDisposable.Dispose( )
		{
			XmlReader?.Dispose( );
		}

		/// <summary>
		///     Gets the binary data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		IEnumerable<BinaryDataEntry> IDataSource.GetBinaryData( IProcessingContext context )
		{
			Deserialize( );

			return _packageData.Binaries;
		}

		/// <summary>
		///     Gets the binary data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		IEnumerable<DocumentDataEntry> IDataSource.GetDocumentData( IProcessingContext context )
		{
			Deserialize( );

			return _packageData.Documents;
		}

		/// <summary>
		///     Load entities.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		IEnumerable<EntityEntry> IDataSource.GetEntities( IProcessingContext context )
		{
			Deserialize( );

			return _packageData.Entities;
		}

		/// <summary>
		///     Load field data.
		/// </summary>
		/// <param name="dataTable"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		/// <remarks>
		///     Data sources MUST:
		///     - ensure that bits are represented as Booleans
		///     - ensure that XML is transformed so that entityRefs contain Upgrade ids
		///     - or where XML contains an alias, translate it to uprgadeId|alias   (as the alias may be changed in the target)
		///     - ensure that aliases export their namespace and direction marker.
		/// </remarks>
		IEnumerable<DataEntry> IDataSource.GetFieldData( string dataTable, IProcessingContext context )
		{
			Deserialize( );

			IEnumerable<DataEntry> data;
			if ( !_packageData.FieldData.TryGetValue( dataTable, out data ) )
			{
				data = new List<DataEntry>( );
			}
			return data;
		}

		/// <summary>
		///     Loads the application metadata.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		Metadata IDataSource.GetMetadata( IProcessingContext context )
		{
			Deserialize( );

			return _packageData.Metadata;
		}

		/// <summary>
		///     Gets the missing field data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		IEnumerable<DataEntry> IDataSource.GetMissingFieldData( IProcessingContext context )
		{
			return Enumerable.Empty<DataEntry>( );
		}

		/// <summary>
		///     Gets the missing relationships.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		IEnumerable<RelationshipEntry> IDataSource.GetMissingRelationships( IProcessingContext context )
		{
			return Enumerable.Empty<RelationshipEntry>( );
		}

		/// <summary>
		///     Load relationships.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		IEnumerable<RelationshipEntry> IDataSource.GetRelationships( IProcessingContext context )
		{
			Deserialize( );

			return _packageData.Relationships;
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
            return _packageData.DoNotRemove;
        }

        /// <summary>
        ///     Sets up this instance.
        /// </summary>
        /// <param name="context">The context.</param>
        void IDataSource.Setup( IProcessingContext context )
		{
            Deserialize( );
		}

		/// <summary>
		///     Tears down this instance.
		/// </summary>
		/// <param name="context">The context.</param>
		void IDataSource.TearDown( IProcessingContext context )
		{
		}

		/// <summary>
		///     Deserializes this instance.
		/// </summary>
		/// <exception cref="System.NullReferenceException">XmlTextReader has not been set.</exception>
		private void Deserialize( )
		{
			if ( _packageData != null )
			{
				return;
            }

            if ( XmlReader == null )
			{
				throw new NullReferenceException( "XmlTextReader has not been set." );
			}

			if ( _xmlDeserializer == null )
			{
                /////
                // TODO: Use Autofac to inject appropriate deserializer. Need to check version in the xml header.
                /////

			    switch ( Version )
			    {
                    case Format.Xml:
                        _xmlDeserializer = new XmlDeserializer( );
                        break;
                    case Format.XmlVer2:
                        _xmlDeserializer = new XmlDeserializerV2( );
                        break;
                    default:
			            throw new Exception( "Unsupported file version {Version}" );
			    }
                
			}

            _packageData = _xmlDeserializer.Deserialize( XmlReader );
		}
	}
}