// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Xml;
using EDC.Collections.Generic;
using EDC.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.Xml;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version1
{
	/// <summary>
	///     Class representing the XmlSerializer type.
	/// </summary>
	internal class XmlSerializer : IXmlApplicationSerializer
	{
		/// <summary>
		///     The alias to data type map
		/// </summary>
		private readonly IDictionary<EntityAlias, string> _aliasToDataTypeMap = new Dictionary<EntityAlias, string>( );

		/// <summary>
		///     The data map
		/// </summary>
		private readonly IDictionary<string, IDictionary<Guid, IList<DataEntry>>> _dataMap = new Dictionary<string, IDictionary<Guid, IList<DataEntry>>>( );

		/// <summary>
		///     The relationship map
		/// </summary>
		private readonly IDictionary<Guid, IList<RelationshipEntry>> _relationshipMap = new Dictionary<Guid, IList<RelationshipEntry>>( );

		/// <summary>
		///     The upgrade identifier to alias map
		/// </summary>
		private readonly IDictionary<Guid, EntityAlias> _upgradeIdToAliasMap = new Dictionary<Guid, EntityAlias>( );

		/// <summary>
		///     Initializes a new instance of the <see cref="XmlSerializer" /> class.
		/// </summary>
		public XmlSerializer( )
		{
			Version = "1.0";

			Namespaces = new Dictionary<string, string>
			{
				{
					"console", "k"
				}
			};
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
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		private IProcessingContext Context
		{
			get;
			set;
        }

        /// <summary>
        ///     Sets the package data.
        /// </summary>
        /// <value>
        ///     The package data.
        /// </value>
        public PackageData PackageData
        {
            private get;
            set;
        }

		/// <summary>
		///     Gets the default namespace.
		/// </summary>
		/// <value>
		///     The default namespace.
		/// </value>
		public string DefaultNamespace => "core";

		/// <summary>
		///     Gets the namespaces.
		/// </summary>
		/// <value>
		///     The namespaces.
		/// </value>
		public IDictionary<string, string> Namespaces
		{
			get;
		}

		/// <summary>
		///     Sets the relationships.
		/// </summary>
		/// <value>
		///     The relationships.
		/// </value>
		private void PrepareRelationships()
        {
            var relationships = PackageData.Relationships;

			if ( relationships != null )
            {
                foreach ( RelationshipEntry relationship in relationships )
                {
                    IList<RelationshipEntry> existingRelationships;
                    if ( !_relationshipMap.TryGetValue( relationship.FromId, out existingRelationships ) )
                    {
                        existingRelationships = new List<RelationshipEntry>( );
                        _relationshipMap[ relationship.FromId ] = existingRelationships;
                    }

                    existingRelationships.Add( relationship );
                }
            }
        }

		/// <summary>
		///     Gets the version.
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		public string Version
		{
			get;
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
        ///     Serializes the application using the specified XML writer.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException">No xml writer specified.</exception>
        public void Serialize( XmlWriter xmlWriter, IProcessingContext context = null )
		{
			if ( xmlWriter == null )
			{
				throw new ArgumentNullException( nameof( xmlWriter ) );
			}

			if ( context == null )
			{
				context = new ProcessingContext( );
			}

			Context = context;

            RestructureFieldData( );
            PrepareRelationships( );

            Stack<string> xmlStack = new Stack<string>( );

			SerializeHeader( xmlWriter, xmlStack );

			if ( xmlStack.Count > 0 )
			{
				throw new InvalidOperationException( $"Xml stack corruption detected. Expected empty stack but found '{string.Join( ",", xmlStack.ToArray( ).Reverse( ) )}'" );
			}
		}

		/// <summary>
		///     Gets the inner text.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		private string GetInnerText( Guid id )
		{
			var alias = NameResolver?.Resolve( id );

			if ( alias != null )
			{
				_upgradeIdToAliasMap[ id ] = alias;

				return alias.ToString( );
			}

			return id.ToString( "B" );
		}

		/// <summary>
		///     Resolves the alias.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		private EntityAlias ResolveAlias( Guid id )
		{
			var entityAlias = NameResolver?.Resolve( id );

			if ( entityAlias != null )
			{
				_upgradeIdToAliasMap[ id ] = entityAlias;
			}

			return entityAlias;
		}

		/// <summary>
		///     Restructures the data.
		/// </summary>
		private void RestructureFieldData( )
		{
            if ( PackageData.FieldData == null )
                return;

		    foreach ( var pair in PackageData.FieldData )
		    {
                string dataTable = pair.Key;
                IEnumerable<DataEntry> data = pair.Value;

                IDictionary<Guid, IList<DataEntry>> value;

                if ( !_dataMap.TryGetValue( dataTable, out value ) )
                {
                    value = new Dictionary<Guid, IList<DataEntry>>( );
                    _dataMap[ dataTable ] = value;
                }

                foreach ( DataEntry entry in data )
                {
                    IList<DataEntry> entries;
                    if ( !value.TryGetValue( entry.EntityId, out entries ) )
                    {
                        entries = new List<DataEntry>( );
                        value[ entry.EntityId ] = entries;
                    }

                    entries.Add( entry );
                }

            }
		}

		/// <summary>
		///     Serializes the alias map.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeAliasMap( XmlWriter xmlWriter, Stack<string> xmlStack )
		{
			xmlWriter.WriteStartElement( XmlConstants.AliasMap, xmlStack );

			if ( _upgradeIdToAliasMap.Count > 0 )
			{
				foreach ( var upgradeIdAliasPair in _upgradeIdToAliasMap.OrderBy( p => p.Key ) )
				{
					string elementName = upgradeIdAliasPair.Value.Alias.ToCamelCase( );

					xmlWriter.WriteStartElement( elementName, upgradeIdAliasPair.Value.Namespace, xmlStack );
					xmlWriter.WriteAttributeString( XmlConstants.Id, upgradeIdAliasPair.Key.ToString( "B" ) );

					string dataType;

					if ( _aliasToDataTypeMap.TryGetValue( upgradeIdAliasPair.Value, out dataType ) )
					{
						xmlWriter.WriteAttributeString( XmlConstants.Type, dataType );
					}

					xmlWriter.WriteEndElement( elementName, xmlStack );
				}
			}

			xmlWriter.WriteEndElement( XmlConstants.AliasMap, xmlStack );
		}

		/// <summary>
		///     Serializes the binary.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeBinary( XmlWriter xmlWriter, Stack<string> xmlStack )
		{
		    IEnumerable<BinaryDataEntry> binaries = PackageData.Binaries;

			xmlWriter.WriteStartElement( XmlConstants.BinaryConstants.Binaries, xmlStack );

			if ( binaries != null )
			{
				foreach ( BinaryDataEntry binary in binaries )
				{
					if ( binary.Data != null )
					{
						xmlWriter.WriteStartElement( XmlConstants.BinaryConstants.Binary, xmlStack );
						xmlWriter.WriteAttributeString( XmlConstants.BinaryConstants.Hash, binary.DataHash );
						xmlWriter.WriteAttributeString( XmlConstants.BinaryConstants.Extension, binary.FileExtension );

						xmlWriter.WriteString( Convert.ToBase64String( CompressionHelper.Compress( binary.Data ) ) );

						xmlWriter.WriteEndElement( XmlConstants.BinaryConstants.Binary, xmlStack );
					}
				}
			}

			xmlWriter.WriteEndElement( XmlConstants.BinaryConstants.Binaries, xmlStack );
        }

		/// <summary>
		///     Serializes the dependencies.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeDependencies( XmlWriter xmlWriter, Stack<string> xmlStack )
		{
		    IList<SolutionDependency> dependencies = PackageData.Metadata?.Dependencies;

			xmlWriter.WriteStartElement( XmlConstants.MetadataConstants.Dependencies, xmlStack );

			if ( dependencies != null )
			{
				foreach ( SolutionDependency dependency in dependencies.OrderBy( d => d.DependencyApplication ) )
				{
					xmlWriter.WriteStartElement( XmlConstants.MetadataConstants.Dependency, xmlStack );

					if ( !string.IsNullOrEmpty( dependency.Name ) )
					{
						xmlWriter.WriteElementString( XmlConstants.MetadataConstants.Name, dependency.Name );
					}

					if ( !string.IsNullOrEmpty( dependency.DependencyName ) )
					{
						xmlWriter.WriteElementString( XmlConstants.MetadataConstants.DependencyName, dependency.DependencyName );
					}

					xmlWriter.WriteElementString( XmlConstants.MetadataConstants.ApplicationId, dependency.DependencyApplication.ToString( "B" ) );

					if ( dependency.MinimumVersion != null )
					{
						xmlWriter.WriteElementString( XmlConstants.MetadataConstants.MinimumVersion, dependency.MinimumVersion.ToString( 4 ) );
					}

					if ( dependency.MaximumVersion != null )
					{
						xmlWriter.WriteElementString( XmlConstants.MetadataConstants.MaximumVersion, dependency.MaximumVersion.ToString( 4 ) );
					}

					xmlWriter.WriteElementString( XmlConstants.MetadataConstants.IsRequired, dependency.IsRequired.ToString( ) );

					xmlWriter.WriteEndElement( XmlConstants.MetadataConstants.Dependency, xmlStack );
				}
			}

			xmlWriter.WriteEndElement( XmlConstants.MetadataConstants.Dependencies, xmlStack );
		}

		/// <summary>
		///     Serializes the list of entities that should not be removed during upgrade operations.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeDoNotRemove( XmlWriter xmlWriter, Stack<string> xmlStack )
		{
		    IEnumerable<Guid> doNotRemove = PackageData.DoNotRemove;

            xmlWriter.WriteStartElement( XmlConstants.DoNotRemoveConstants.DoNotRemove, xmlStack );

            if ( doNotRemove != null )
            {
                foreach ( Guid upgradeId in doNotRemove )
                {
                    xmlWriter.WriteStartElement( XmlConstants.DoNotRemoveConstants.LeaveEntity, xmlStack );
                    xmlWriter.WriteAttributeString( XmlConstants.Id, upgradeId.ToString( "B" ) );
                    xmlWriter.WriteEndElement( XmlConstants.DoNotRemoveConstants.LeaveEntity, xmlStack );
                }
            }

            xmlWriter.WriteEndElement( XmlConstants.DoNotRemoveConstants.DoNotRemove, xmlStack );
        }

        /// <summary>
        ///     Serializes the documents.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="xmlStack">The XML stack.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void SerializeDocuments( XmlWriter xmlWriter, Stack<string> xmlStack )
        {
            IEnumerable<DocumentDataEntry> documents = PackageData.Documents;

			xmlWriter.WriteStartElement( XmlConstants.DocumentConstants.Documents, xmlStack );

			if ( documents != null )
			{
				foreach ( DocumentDataEntry document in documents )
				{
					xmlWriter.WriteStartElement( XmlConstants.DocumentConstants.Document, xmlStack );
					xmlWriter.WriteAttributeString( XmlConstants.DocumentConstants.Hash, document.DataHash );
					xmlWriter.WriteAttributeString( XmlConstants.DocumentConstants.Extension, document.FileExtension );

					if ( document.Data != null )
					{
						xmlWriter.WriteString( Convert.ToBase64String( CompressionHelper.Compress( document.Data ) ) );
					}

					xmlWriter.WriteEndElement( XmlConstants.DocumentConstants.Document, xmlStack );
				}
			}

			xmlWriter.WriteEndElement( XmlConstants.DocumentConstants.Documents, xmlStack );
		}

		/// <summary>
		///     Serializes the entities.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeEntities( XmlWriter xmlWriter, Stack<string> xmlStack )
		{
		    IEnumerable<EntityEntry> entities = PackageData.Entities;

			/////
			// By this point the entire entity graph has been built. Now serialize it
			/////
			xmlWriter.WriteStartElement( XmlConstants.EntityConstants.Entities, xmlStack );

			if ( entities != null )
			{
				foreach ( EntityEntry entity in entities.OrderBy( e => e.EntityId ) )
				{
					xmlWriter.WriteStartElement( XmlConstants.EntityConstants.Entity, xmlStack );
					xmlWriter.WriteAttributeString( XmlConstants.Id, entity.EntityId.ToString( "B" ) );

					/////
					// Bubble Name and Description to the top.
					/////
					IDictionary<Guid, IList<DataEntry>> stringData;
					if ( _dataMap.TryGetValue( Helpers.NVarCharName, out stringData ) )
					{
						IList<DataEntry> entityStringData;
						if ( stringData.TryGetValue( entity.EntityId, out entityStringData ) )
						{
							var nameData = entityStringData.FirstOrDefault( de => de.FieldId == Guids.Name );

							if ( nameData != null )
							{
								SerializeField( xmlWriter, nameData, Helpers.NVarCharName, Helpers.NVarCharName.ToCamelCase( ), o => o.ToString( ), xmlStack );
								entityStringData.Remove( nameData );
							}

							var descriptionData = entityStringData.FirstOrDefault( de => de.FieldId == Guids.Description );

							if ( descriptionData != null )
							{
								SerializeField( xmlWriter, descriptionData, Helpers.NVarCharName, Helpers.NVarCharName.ToCamelCase( ), o => o.ToString( ), xmlStack );
								entityStringData.Remove( descriptionData );
							}
						}
					}

					foreach ( string dataType in _dataMap.Keys.OrderBy( k => k ) )
					{
						IList<DataEntry> dataEntries;

						Func<object, string> customFormatter;

						if ( dataType == Helpers.DateTimeName )
						{
							customFormatter = o =>
							{
								if ( o == null )
								{
									return SqlDateTime.MinValue.Value.ToUniversalTime( ).ToString( "u" );
								}

								var s = o as string;
								if ( s != null )
								{
									return s;
								}

								if ( o is DateTime )
								{
									return ( ( DateTime ) o ).ToUniversalTime( ).ToString( "u" );
								}

								DateTime dt;

								if ( !DateTime.TryParse( o.ToString( ), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dt ) )
								{
									Context.WriteWarning( "Failed to parse date time." );
									return o.ToString( );
								}

								return dt.ToString( "u" );
							};
						}
						else if ( dataType == Helpers.DecimalName )
						{
							customFormatter = o => ( ( decimal ) o ).ToString( "0.##########" );
						}
						else
						{
							customFormatter = o => o.ToString( );
						}

						if ( _dataMap[ dataType ].TryGetValue( entity.EntityId, out dataEntries ) )
						{
							string defaultElementName = dataType.ToCamelCase( );

							foreach ( DataEntry dataEntry in dataEntries.OrderBy( e => e.FieldId ) )
							{
								if ( dataEntry.Data != null )
								{
									SerializeField( xmlWriter, dataEntry, dataType, defaultElementName, customFormatter, xmlStack );
								}
							}
						}
					}

					IList<RelationshipEntry> relationships;
					if ( _relationshipMap.TryGetValue( entity.EntityId, out relationships ) )
					{
						xmlWriter.WriteStartElement( XmlConstants.RelationshipConstants.Relationships, xmlStack );

						string defaultElementName = XmlConstants.RelationshipConstants.Relationship;

                        // Note: ordering is done using SQL guid, to maintain backwards compatibily with .xml files exported prior to sorting
                        // as they gained their sort order from the LibraryAppSource proc
                        foreach ( RelationshipEntry relationship in relationships.OrderBy( r => new SqlGuid( r.TypeId ) ).ThenBy( r=> new SqlGuid( r.ToId ) ) )
						{
							SerializeInlineRelationship( xmlWriter, relationship, defaultElementName, xmlStack );
						}

						xmlWriter.WriteEndElement( XmlConstants.RelationshipConstants.Relationships, xmlStack );

						_relationshipMap.Remove( entity.EntityId );
					}

					xmlWriter.WriteEndElement( XmlConstants.EntityConstants.Entity, xmlStack );
				}
			}

			xmlWriter.WriteEndElement( XmlConstants.EntityConstants.Entities, xmlStack );
		}

		/// <summary>
		///     Serializes the field.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="dataEntry">The data entry.</param>
		/// <param name="dataType">Type of the data.</param>
		/// <param name="defaultElementName">Default name of the element.</param>
		/// <param name="customFormatter">The custom formatter.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeField( XmlWriter xmlWriter, DataEntry dataEntry, string dataType, string defaultElementName, Func<object, string> customFormatter, Stack<string> xmlStack )
		{
			string elementName;
			string nameSpace;

			bool resolved = TryGetFieldElementName( dataEntry.FieldId, dataType, out elementName, out nameSpace );

			xmlWriter.WriteStartElement( elementName ?? defaultElementName, nameSpace, xmlStack );

			if ( !resolved )
			{
				xmlWriter.WriteAttributeString( XmlConstants.Id, dataEntry.FieldId.ToString( "B" ) );
			}

			if ( dataType == Helpers.AliasName )
			{
				xmlWriter.WriteAttributeString( XmlConstants.FieldConstants.AliasIdMarker, dataEntry.AliasMarkerId.ToString( ) );
				xmlWriter.WriteString( customFormatter( dataEntry.Namespace + ":" + dataEntry.Data ) );
			}
			else
			{
				xmlWriter.WriteString( customFormatter( dataEntry.Data ) );
			}

			xmlWriter.WriteEndElement( elementName ?? defaultElementName, xmlStack );
		}

		/// <summary>
		///     Serializes the header.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeHeader( XmlWriter xmlWriter, Stack<string> xmlStack )
		{
			xmlWriter.WriteStartElement( XmlConstants.Xml, DefaultNamespace, xmlStack );
			xmlWriter.WriteAttributeString( XmlConstants.Version, Version );

			if ( xmlWriter.Settings?.Encoding != null )
			{
				xmlWriter.WriteAttributeString( XmlConstants.Encoding, xmlWriter.Settings.Encoding.WebName );
			}

			foreach ( KeyValuePair<string, string> namespaceAlias in Namespaces )
			{
				xmlWriter.WriteAttributeString( XmlConstants.XmlNs, namespaceAlias.Value, null, namespaceAlias.Key );
			}

			SerializeMetadata( xmlWriter, xmlStack );

			xmlWriter.WriteEndElement( XmlConstants.Xml, xmlStack );
		}

		/// <summary>
		///     Serializes the inline relationship.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="defaultElementName">Default name of the element.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeInlineRelationship( XmlWriter xmlWriter, RelationshipEntry relationship, string defaultElementName, Stack<string> xmlStack )
		{
			string elementName;
			string nameSpace;

			bool resolved = TryGetElementName( relationship.TypeId, out elementName, out nameSpace );

			xmlWriter.WriteStartElement( elementName ?? defaultElementName, nameSpace, xmlStack );

			if ( !resolved )
			{
				xmlWriter.WriteAttributeString( XmlConstants.Id, relationship.TypeId.ToString( "B" ) );
			}

			xmlWriter.WriteString( GetInnerText( relationship.ToId ) );

			xmlWriter.WriteEndElement( elementName ?? defaultElementName, xmlStack );
		}

		/// <summary>
		///     Serializes the metadata.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeMetadata( XmlWriter xmlWriter, Stack<string> xmlStack )
		{
            Metadata metadata = PackageData.Metadata;

			if ( metadata == null )
			{
				return;
			}

		    if ( metadata.Type == Sources.SourceType.DataExport )
            {
                xmlWriter.WriteStartElement(XmlConstants.MetadataConstants.Application, xmlStack);
                xmlWriter.WriteStartElement(XmlConstants.MetadataConstants.Package, xmlStack);

                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.Type, metadata.Type.ToString());
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.Root, metadata.RootEntityId.ToString("B"));
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.PlatformVersion, metadata.PlatformVersion ?? string.Empty);
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.PublishDate, DateTime.UtcNow.ToString("u"));
            }
		    else
            {
                xmlWriter.WriteStartElement(XmlConstants.MetadataConstants.Application, xmlStack);
                xmlWriter.WriteAttributeString(XmlConstants.Id, metadata.AppId.ToString("B"));

                xmlWriter.WriteStartElement(XmlConstants.MetadataConstants.Package, xmlStack);
                xmlWriter.WriteAttributeString(XmlConstants.Id, metadata.AppVerId.ToString("B"));

                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.Name, metadata.Name ?? string.Empty);
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.ApplicationName, metadata.AppName ?? string.Empty);
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.Description, metadata.Description ?? string.Empty);
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.Type, metadata.Type.ToString());
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.Version, VersionOverride ?? metadata.Version ?? string.Empty);
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.PlatformVersion, metadata.PlatformVersion ?? string.Empty);
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.ReleaseDate, metadata.ReleaseDate.ToUniversalTime().ToString("u"));
                xmlWriter.WriteElementString(XmlConstants.MetadataConstants.PublishDate, metadata.PublishDate.ToUniversalTime().ToString("u"));

            }

			SerializeDependencies( xmlWriter, xmlStack );

			SerializeEntities( xmlWriter, xmlStack );
			SerializeRelationships( xmlWriter, xmlStack );
            SerializeSecureData( xmlWriter, xmlStack) ;
			SerializeBinary( xmlWriter, xmlStack );
			SerializeDocuments( xmlWriter, xmlStack );
			SerializeAliasMap( xmlWriter, xmlStack );
            SerializeDoNotRemove( xmlWriter, xmlStack );


            xmlWriter.WriteEndElement( XmlConstants.MetadataConstants.Package, xmlStack );
			xmlWriter.WriteEndElement( XmlConstants.MetadataConstants.Application, xmlStack );
		}

		/// <summary>
		///     Serializes the relationship.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="relationship">The relationship.</param>
		/// <param name="defaultTypeName">Default name of the type.</param>
		/// <param name="xmlStack">The XML stack.</param>
		private void SerializeRelationship( XmlWriter xmlWriter, RelationshipEntry relationship, string defaultTypeName, Stack<string> xmlStack )
		{
			string typeAlias;
			string typeNamespace;

			bool resolvedType = TryGetElementName( relationship.TypeId, out typeAlias, out typeNamespace );

			xmlWriter.WriteStartElement( typeAlias ?? defaultTypeName, typeNamespace, xmlStack );

			if ( !resolvedType )
			{
				xmlWriter.WriteAttributeString( XmlConstants.Id, relationship.TypeId.ToString( "B" ) );
			}

			xmlWriter.WriteStartElement( XmlConstants.RelationshipConstants.From, xmlStack );
			xmlWriter.WriteString( GetInnerText( relationship.FromId ) );
			xmlWriter.WriteEndElement( XmlConstants.RelationshipConstants.From, xmlStack );

			xmlWriter.WriteStartElement( XmlConstants.RelationshipConstants.To, xmlStack );
			xmlWriter.WriteString( GetInnerText( relationship.ToId ) );
			xmlWriter.WriteEndElement( XmlConstants.RelationshipConstants.To, xmlStack );

			xmlWriter.WriteEndElement( typeAlias ?? defaultTypeName, xmlStack );
		}

		/// <summary>
		///     Serializes the relationships.
		/// </summary>
		private void SerializeRelationships( XmlWriter xmlWriter, Stack<string> xmlStack )
		{
			xmlWriter.WriteStartElement( XmlConstants.RelationshipConstants.Relationships, xmlStack );

			if ( _relationshipMap != null )
			{
				string defaultTypeName = XmlConstants.RelationshipConstants.Relationship;

				foreach ( RelationshipEntry relationship in _relationshipMap.Values.SelectMany( r => r ).OrderBy( r => r.FromId ).ThenBy( r => r.TypeId ).ThenBy( r => r.ToId ) )
				{
					SerializeRelationship( xmlWriter, relationship, defaultTypeName, xmlStack );
				}
			}

			xmlWriter.WriteEndElement( XmlConstants.RelationshipConstants.Relationships, xmlStack );
		}

        /// <summary>
        ///     Serializes secure data
        /// </summary>
        private void SerializeSecureData(XmlWriter xmlWriter, Stack<string> xmlStack)
        {
            var secureData = PackageData.SecureData;

            if ( secureData != null)
            {
                xmlWriter.WriteStartElement(XmlConstants.SecureDataConstants.SecureData, xmlStack);

                foreach (SecureDataEntry entry in secureData.OrderBy(r => r.Context).ThenBy(r => r.SecureId))
                {
                    xmlWriter.WriteStartElement(XmlConstants.SecureDataConstants.SecureDataEntry, xmlStack);
                    xmlWriter.WriteAttributeString(XmlConstants.SecureDataConstants.SecureId, entry.SecureId.ToString());
                    xmlWriter.WriteAttributeString(XmlConstants.SecureDataConstants.Context, entry.Context);

                    xmlWriter.WriteString(Convert.ToBase64String(entry.Data));

                    xmlWriter.WriteEndElement(XmlConstants.SecureDataConstants.SecureDataEntry, xmlStack);
                }

                xmlWriter.WriteEndElement(XmlConstants.SecureDataConstants.SecureData, xmlStack);

            }
        }


        /// <summary>
        ///     Tries the name of the get element.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <returns></returns>
        private bool TryGetElementName( Guid id, out string alias, out string nameSpace )
		{
			nameSpace = null;
			alias = null;

			var entityAlias = ResolveAlias( id );

			if ( entityAlias != null )
			{
				nameSpace = entityAlias.Namespace;
				alias = entityAlias.Alias.ToCamelCase( );

				return true;
			}

			return false;
		}

		/// <summary>
		///     Tries the name of the get field element.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="dataType">Type of the data.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="nameSpace">The name space.</param>
		/// <returns></returns>
		private bool TryGetFieldElementName( Guid id, string dataType, out string alias, out string nameSpace )
		{
			nameSpace = null;
			alias = null;

			var entityAlias = ResolveAlias( id );

			if ( entityAlias != null )
			{
				_aliasToDataTypeMap[ entityAlias ] = dataType;

				nameSpace = entityAlias.Namespace;
				alias = entityAlias.Alias.ToCamelCase( );

				return true;
			}

			return false;
		}
	}
}