// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using EDC.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Sources;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version1
{
    public class XmlDeserializer : IXmlApplicationDeserializer
    {
        /// <summary>
        ///     The alias map
        /// </summary>
        private readonly IDictionary<string, Guid> _aliasMap = new Dictionary<string, Guid>( );

        /// <summary>
        ///     The alias type map
        /// </summary>
        private readonly IDictionary<string, string> _aliasTypeMap = new Dictionary<string, string>( );

        /// <summary>
        ///     The binaries
        /// </summary>
        private readonly IList<BinaryDataEntry> _binaries = new List<BinaryDataEntry>( );

        /// <summary>
        ///     The documents
        /// </summary>
        private readonly IList<DocumentDataEntry> _documents = new List<DocumentDataEntry>( );

        /// <summary>
        ///     The entities
        /// </summary>
        private readonly IList<EntityEntry> _entities = new List<EntityEntry>( );

        /// <summary>
        ///     The field map
        /// </summary>
        private readonly IList<FieldContainer> _fieldMap = new List<FieldContainer>( );

        /// <summary>
        ///     The relationship map
        /// </summary>
        private readonly IList<RelationshipContainer> _relationshipMap = new List<RelationshipContainer>( );

        /// <summary>
        ///     The data
        /// </summary>
        private IDictionary<string, IList<DataEntry>> _data;

        /// <summary>
        ///     The data
        /// </summary>
        private readonly IList<SecureDataEntry> _secureData = new List<SecureDataEntry>( );

        /// <summary>
        ///     The relationships
        /// </summary>
        private IEnumerable<RelationshipEntry> _relationships;

        /// <summary>
        ///     Registry of GUIDs that should not be removed as part of an upgrade.
        /// </summary>
        private readonly ISet<Guid> _doNotRemove = new HashSet<Guid>( );

        /// <summary>
        ///     Initializes a new instance of the <see cref="XmlDeserializer" /> class.
        /// </summary>
        public XmlDeserializer( )
        {
            Version = "1.0";

            Namespaces = new Dictionary<string, string>
            {
                {
                    "k", "console"
                }
            };

            Metadata = new Metadata( );
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
        ///     Gets the binaries.
        /// </summary>
        /// <value>
        ///     The binaries.
        /// </value>
        public IEnumerable<BinaryDataEntry> Binaries => _binaries;

        /// <summary>
        ///     Gets the data.
        /// </summary>
        /// <value>
        ///     The data.
        /// </value>
        public IDictionary<string, IList<DataEntry>> Data
        {
            get
            {
                if ( _data == null )
                {
                    _data = new Dictionary<string, IList<DataEntry>>( );

                    foreach ( FieldContainer container in _fieldMap )
                    {
                        string type;

                        switch ( container.Type )
                        {
                            case XmlConstants.FieldConstants.AliasField:
                                type = Helpers.AliasName;
                                break;
                            case XmlConstants.FieldConstants.BitField:
                                type = Helpers.BitName;
                                break;
                            case XmlConstants.FieldConstants.DateTimeField:
                                type = Helpers.DateTimeName;
                                break;
                            case XmlConstants.FieldConstants.DecimalField:
                                type = Helpers.DecimalName;
                                break;
                            case XmlConstants.FieldConstants.GuidField:
                                type = Helpers.GuidName;
                                break;
                            case XmlConstants.FieldConstants.IntField:
                                type = Helpers.IntName;
                                break;
                            case XmlConstants.FieldConstants.NVarCharField:
                                type = Helpers.NVarCharName;
                                break;
                            case XmlConstants.FieldConstants.XmlField:
                                type = Helpers.XmlName;
                                break;
                            default:
                                if ( !_aliasTypeMap.TryGetValue( GetCanonicalAlias( container.Type ), out type ) )
                                {
                                    type = container.Type;
                                }
                                break;
                        }

                        if ( string.IsNullOrEmpty( type ) )
                        {
                            throw new InvalidOperationException( $"Entity '{container.EntityId:B}' field '{container.FieldId}' has no known type '{container.Type}'" );
                        }

                        IList<DataEntry> data;
                        if ( !_data.TryGetValue( type, out data ) )
                        {
                            data = new List<DataEntry>( );
                            _data[ type ] = data;
                        }

                        DataEntry entry = new DataEntry
                        {
                            EntityId = container.EntityId,
                            FieldId = ResolveAlias( container.FieldId ),
                            AliasMarkerId = container.AliasMarkerId
                        };

                        ///////

                        switch ( type )
                        {
                            case Helpers.AliasName:
                                {
                                    EntityAlias alias = new EntityAlias( container.Data );

                                    entry.Data = alias.Alias;
                                    entry.Namespace = alias.Namespace;
                                }
                                break;
                            case Helpers.BitName:
                                {
                                    string text = container.Data;

                                    bool bitValue;

                                    if ( !bool.TryParse( text, out bitValue ) )
                                    {
                                        Context.WriteWarning( $"Detected invalid bit value. Field '{entry.FieldId:B}' on entity '{entry.EntityId:B}'" );
                                        continue;
                                    }

                                    entry.Data = bitValue;
                                }
                                break;
                            case Helpers.DateTimeName:
                                {
                                    string text = container.Data;

                                    DateTime dateTimeValue;

                                    if ( !DateTime.TryParse( text, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTimeValue ) )
                                    {
                                        Context.WriteWarning( $"Detected invalid datetime value. Field '{entry.FieldId:B}' on entity '{entry.EntityId:B}'" );
                                        continue;
                                    }

                                    entry.Data = dateTimeValue;
                                }
                                break;
                            case Helpers.DecimalName:
                                {
                                    string text = container.Data;

                                    decimal decimalValue;

                                    if ( !decimal.TryParse( text, out decimalValue ) )
                                    {
                                        Context.WriteWarning( $"Detected invalid decimal value. Field '{entry.FieldId:B}' on entity '{entry.EntityId:B}'" );
                                        continue;
                                    }

                                    entry.Data = decimalValue;
                                }
                                break;
                            case Helpers.GuidName:
                                {
                                    string text = container.Data;

                                    Guid guidValue;

                                    if ( !Guid.TryParse( text, out guidValue ) )
                                    {
                                        Context.WriteWarning( $"Detected invalid guid value. Field '{entry.FieldId:B}' on entity '{entry.EntityId:B}'" );
                                        continue;
                                    }

                                    entry.Data = guidValue;
                                }
                                break;
                            case Helpers.IntName:
                                {
                                    string text = container.Data;

                                    int intValue;

                                    if ( !int.TryParse( text, out intValue ) )
                                    {
                                        Context.WriteWarning( $"Detected invalid int value. Field '{entry.FieldId:B}' on entity '{entry.EntityId:B}'" );
                                        continue;
                                    }

                                    entry.Data = intValue;
                                }
                                break;
                            case Helpers.NVarCharName:
                            case Helpers.XmlName:
                                {
                                    string text = container.Data;

                                    entry.Data = text;
                                }
                                break;
                        }

                        data.Add( entry );

                        ///////
                    }
                }

                return _data;
            }
        }

        /// <summary>
        ///     Gets the default namespace.
        /// </summary>
        /// <value>
        ///     The default namespace.
        /// </value>
        public string DefaultNamespace => "core";

        /// <summary>
        ///     Gets the documents.
        /// </summary>
        /// <value>
        ///     The documents.
        /// </value>
        public IEnumerable<DocumentDataEntry> Documents => _documents;

        /// <summary>
        ///     Gets the entities.
        /// </summary>
        /// <value>
        ///     The entities.
        /// </value>
        public IEnumerable<EntityEntry> Entities => _entities;

        /// <summary>
        ///     Gets the metadata.
        /// </summary>
        /// <value>
        ///     The metadata.
        /// </value>
        public Metadata Metadata
        {
            get;
        }

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
        ///     Gets the relationships.
        /// </summary>
        /// <value>
        ///     The relationships.
        /// </value>
        public IEnumerable<RelationshipEntry> Relationships
        {
            get
            {
                return _relationships ?? ( _relationships = _relationshipMap.Select( container => new RelationshipEntry( ResolveAlias( container.Type ), ResolveAlias( container.From ), ResolveAlias( container.To ) ) ) );
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
        /// Get the SecureData
        /// </summary>
        public IList<SecureDataEntry> SecureData => _secureData;

        /// <summary>
        /// Get the SecureData
        /// </summary>
        public ISet<Guid> DoNotRemove => _doNotRemove;


        /// <summary>
        ///     Deserializes the application using the specified XML reader.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public PackageData Deserialize( XmlReader xmlReader, IProcessingContext context = null )
        {
            if ( xmlReader == null )
            {
                throw new ArgumentNullException( nameof( xmlReader ) );
            }

            if ( context == null )
            {
                context = new ProcessingContext( );
            }

            Context = context;

            Stack<string> xmlStack = new Stack<string>( );

            while ( xmlReader.Read( ) )
            {
                if ( xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == XmlConstants.Xml && xmlReader.Depth == xmlStack.Count )
                {
                    DeserializeXml( xmlReader, xmlStack );
                    break;
                }
            }

            if ( xmlStack.Count > 0 )
            {
                throw new InvalidOperationException( $"Xml stack corruption detected. Expected empty stack but found '{string.Join( ",", xmlStack.ToArray( ).Reverse( ) )}'" );
            }

            PackageData result = new PackageData
            {
                Binaries = Binaries,
                Documents = Documents,
                Entities = Entities,
                Relationships = Relationships,
                DoNotRemove = DoNotRemove,
                FieldData = Data.ToDictionary( pair => pair.Key, pair => (IEnumerable<DataEntry>)pair.Value ),
                Metadata = Metadata,
                SecureData = SecureData
            };
            return result;
        }

        /// <summary>
        ///     Throws an XML exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <exception cref="XmlException">null</exception>
        private static void ThrowXmlException( string message, XmlReader xmlReader )
        {
            XmlTextReader xmlTextReader = xmlReader as XmlTextReader;

            if ( xmlTextReader?.HasLineInfo( ) == true )
            {
                throw new XmlException( message, null, xmlTextReader.LineNumber, xmlTextReader.LinePosition );
            }

            throw new XmlException( message );
        }

        /// <summary>
        ///     Advances the reader.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        /// <param name="startName">The start name.</param>
        /// <param name="endName">The end name.</param>
        /// <param name="action">The action.</param>
        private void AdvanceReader( XmlReader xmlReader, Stack<string> xmlStack, string startName, string endName, Action<XmlReader, Stack<string>> action )
        {
            while ( xmlReader.Read( ) )
            {
                if ( xmlReader.NodeType == XmlNodeType.Element && ( startName == null || xmlReader.Name == startName ) && xmlReader.Depth == xmlStack.Count )
                {
                    action?.Invoke( xmlReader, xmlStack );
                }
                else if ( xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == endName && xmlReader.Depth == xmlStack.Count - 1 )
                {
                    PopStack( xmlStack, endName );
                    break;
                }
            }
        }

        /// <summary>
        ///     Deserializes the alias.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeAlias( XmlReader xmlReader, Stack<string> xmlStack )
        {
            string idAttribute = xmlReader.GetAttribute( XmlConstants.Id );

            Guid id = Guid.Empty;
            if ( idAttribute == null || !Guid.TryParse( idAttribute, out id ) )
            {
                ThrowXmlException( "AliasMap does not contain a valid id.", xmlReader );
            }

            _aliasMap[ xmlReader.NamespaceURI + ":" + xmlReader.LocalName ] = id;

            string typeAttribute = xmlReader.GetAttribute( XmlConstants.Type );

            if ( !string.IsNullOrEmpty( typeAttribute ) )
            {
                _aliasTypeMap[ xmlReader.NamespaceURI + ":" + xmlReader.LocalName ] = typeAttribute;
            }
        }

        /// <summary>
        ///     Deserializes the alias map.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeAliasMap( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.AliasMap );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.AliasMap, DeserializeAlias );
        }

        /// <summary>
        ///     Deserializes the application.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        /// <exception cref="XmlException">
        ///     Invalid application id attribute
        ///     or
        ///     Missing application id attribute
        /// </exception>
        private void DeserializeApplication( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.MetadataConstants.Application );

            if ( xmlReader.MoveToAttribute( XmlConstants.Id ) )
            {
                Guid appId;

                if ( Guid.TryParse( xmlReader.Value, out appId ) )
                {
                    Metadata.AppId = appId;
                }
                else
                {
                    throw new XmlException( "Invalid application id attribute" );
                }
            }

            AdvanceReader( xmlReader, xmlStack, XmlConstants.MetadataConstants.Package, XmlConstants.MetadataConstants.Application, DeserializePackage );
        }

        /// <summary>
        ///     Deserializes the binaries.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeBinaries( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.BinaryConstants.Binaries );

            AdvanceReader( xmlReader, xmlStack, XmlConstants.BinaryConstants.Binary, XmlConstants.BinaryConstants.Binaries, DeserializeBinary );
        }

        /// <summary>
        ///     Deserializes the binary.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeBinary( XmlReader xmlReader, Stack<string> xmlStack )
        {
            string hashAttribute = xmlReader.GetAttribute( XmlConstants.BinaryConstants.Hash );

            if ( string.IsNullOrEmpty( hashAttribute ) )
            {
                ThrowXmlException( "Binary hash attribute is invalid.", xmlReader );
            }

            string extensionAttribute = xmlReader.GetAttribute( XmlConstants.BinaryConstants.Extension );

            string data = xmlReader.ReadString( );

            BinaryDataEntry entry = new BinaryDataEntry
            {
                DataHash = hashAttribute,
                FileExtension = extensionAttribute,
                LoadDataCallback = s => LoadBinaryData( data )
            };

            _binaries.Add( entry );
        }

        /// <summary>
        ///     Deserializes the dependencies.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeDependencies( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.MetadataConstants.Dependencies );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.MetadataConstants.Dependencies, DeserializeDependency );
        }

        /// <summary>
        ///     Deserializes the dependency.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeDependency( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.MetadataConstants.Dependency );

            if ( Metadata.Dependencies == null )
            {
                Metadata.Dependencies = new List<SolutionDependency>( );
            }

            SolutionDependency dependency = new SolutionDependency( );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.MetadataConstants.Dependency, ( reader, stack ) => DeserializeDependencyFields( dependency, reader ) );

            Metadata.Dependencies.Add( dependency );
        }

        /// <summary>
        ///     Deserializes the dependency fields.
        /// </summary>
        /// <param name="dependency">The dependency.</param>
        /// <param name="xmlReader">The XML text reader.</param>
        private void DeserializeDependencyFields( SolutionDependency dependency, XmlReader xmlReader )
        {
            switch ( xmlReader.Name )
            {
                case XmlConstants.MetadataConstants.Name:
                    string name = xmlReader.ReadElementContentAsString( );

                    dependency.Name = name;
                    break;
                case XmlConstants.MetadataConstants.DependencyName:
                    string dependencyName = xmlReader.ReadElementContentAsString( );

                    dependency.DependencyName = dependencyName;
                    break;
                case XmlConstants.MetadataConstants.ApplicationId:
                    string applicationId = xmlReader.ReadElementContentAsString( );

                    Guid appId;

                    if ( string.IsNullOrEmpty( applicationId ) || !Guid.TryParse( applicationId, out appId ) )
                    {
                        Context.WriteError( "Invalid dependency id." );
                        return;
                    }

                    dependency.DependencyApplication = appId;
                    break;
                case XmlConstants.MetadataConstants.MinimumVersion:
                    string minimumVersion = xmlReader.ReadElementContentAsString( );

                    Version minVersion;

                    if ( string.IsNullOrEmpty( minimumVersion ) || minimumVersion.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !System.Version.TryParse( minimumVersion, out minVersion ) )
                    {
                        minVersion = null;
                    }

                    dependency.MinimumVersion = minVersion;
                    break;
                case XmlConstants.MetadataConstants.MaximumVersion:
                    string maximumVersion = xmlReader.ReadElementContentAsString( );

                    Version maxVersion;

                    if ( string.IsNullOrEmpty( maximumVersion ) || maximumVersion.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !System.Version.TryParse( maximumVersion, out maxVersion ) )
                    {
                        maxVersion = null;
                    }

                    dependency.MaximumVersion = maxVersion;

                    break;
                case XmlConstants.MetadataConstants.IsRequired:
                    string isRequired = xmlReader.ReadElementContentAsString( );

                    bool required = true;

                    if ( !string.IsNullOrEmpty( isRequired ) )
                    {
                        bool.TryParse( isRequired, out required );
                    }

                    dependency.IsRequired = required;
                    break;
            }
        }

        /// <summary>
        ///     Deserializes the document.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeDocument( XmlReader xmlReader, Stack<string> xmlStack )
        {
            string hashAttribute = xmlReader.GetAttribute( XmlConstants.DocumentConstants.Hash );

            if ( string.IsNullOrEmpty( hashAttribute ) )
            {
                ThrowXmlException( "Document hash attribute is invalid.", xmlReader );
            }

            string extensionAttribute = xmlReader.GetAttribute( XmlConstants.DocumentConstants.Extension );

            string data = xmlReader.ReadString( );

            DocumentDataEntry entry = new DocumentDataEntry
            {
                DataHash = hashAttribute,
                FileExtension = extensionAttribute,
                LoadDataCallback = s => LoadBinaryData( data )
            };

            _documents.Add( entry );
        }

        /// <summary>
        ///     Deserializes the documents.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeDocuments( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.DocumentConstants.Documents );

            AdvanceReader( xmlReader, xmlStack, XmlConstants.DocumentConstants.Document, XmlConstants.DocumentConstants.Documents, DeserializeDocument );
        }

        /// <summary>
        ///     Deserializes the entities.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeEntities( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.EntityConstants.Entities );

            AdvanceReader( xmlReader, xmlStack, XmlConstants.EntityConstants.Entity, XmlConstants.EntityConstants.Entities, DeserializeEntity );
        }

        /// <summary>
        ///     Deserializes the entity.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeEntity( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.EntityConstants.Entity );

            Guid entityId = Guid.Empty;

            string entityIdAttribute = xmlReader.GetAttribute( XmlConstants.Id );

            if ( string.IsNullOrEmpty( entityIdAttribute ) || !Guid.TryParse( entityIdAttribute, out entityId ) )
            {
                ThrowXmlException( "Entity does not contain a valid id attribute.", xmlReader );
            }

            EntityEntry entry = new EntityEntry
            {
                EntityId = entityId
            };

            _entities.Add( entry );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.EntityConstants.Entity, ( reader, stack ) => DeserializeEntityFields( entityId, reader, stack ) );
        }

        /// <summary>
        ///     Deserializes the entity fields.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeEntityFields( Guid entityId, XmlReader xmlReader, Stack<string> xmlStack )
        {
            {
                if ( xmlReader.Name == XmlConstants.RelationshipConstants.Relationships )
                {
                    DeserializeEntityRelationships( entityId, xmlReader, xmlStack );
                }
                else
                {
                    string fieldIdAttribute = xmlReader.GetAttribute( XmlConstants.Id );

                    string dataType = xmlReader.Name;

                    string aliasMarkerIdAttribute = xmlReader.GetAttribute( XmlConstants.FieldConstants.AliasIdMarker );

                    FieldContainer container;

                    if ( fieldIdAttribute != null )
                    {
                        Guid fieldId = Guid.Empty;

                        if ( string.IsNullOrEmpty( fieldIdAttribute ) || !Guid.TryParse( fieldIdAttribute, out fieldId ) )
                        {
                            ThrowXmlException( "Field does not contain a valid id attribute.", xmlReader );
                        }

                        container = new FieldContainer( entityId, fieldId.ToString( "B" ), dataType, xmlReader.ReadString( ) );
                    }
                    else
                    {
                        container = new FieldContainer( entityId, xmlReader.Name, dataType, xmlReader.ReadString( ) );
                    }

                    int aliasMarkerId;
                    if ( !string.IsNullOrEmpty( aliasMarkerIdAttribute ) && int.TryParse( aliasMarkerIdAttribute, out aliasMarkerId ) )
                    {
                        container.AliasMarkerId = aliasMarkerId;
                    }

                    _fieldMap.Add( container );
                }
            }
        }

        /// <summary>
        ///     Deserializes the entity relationships.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeEntityRelationships( Guid entityId, XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.RelationshipConstants.Relationships );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.RelationshipConstants.Relationships, ( reader, stack ) =>
            {
                string relationshipIdAttribute = xmlReader.GetAttribute( XmlConstants.Id );

                string type;

                if ( relationshipIdAttribute != null )
                {
                    Guid typeId = Guid.Empty;

                    if ( string.IsNullOrEmpty( relationshipIdAttribute ) || !Guid.TryParse( relationshipIdAttribute, out typeId ) )
                    {
                        ThrowXmlException( "Relationship does not contain a valid id attribute.", xmlReader );
                    }

                    type = typeId.ToString( "B" );
                }
                else
                {
                    type = xmlReader.Name;
                }

                string text = xmlReader.ReadString( );

                RelationshipContainer container = new RelationshipContainer( entityId.ToString( "B" ), type, text );
                _relationshipMap.Add( container );
            } );
        }

        /// <summary>
        ///     Deserializes the binaries.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeSecureData( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.SecureDataConstants.SecureData );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.SecureDataConstants.SecureData, ( reader, stack ) =>
            {
                string secureIdAttribute = xmlReader.GetAttribute( XmlConstants.SecureDataConstants.SecureId );

                Guid secureId = Guid.Empty;

                if ( string.IsNullOrEmpty( secureIdAttribute ) || !Guid.TryParse( secureIdAttribute, out secureId ) )
                {
                    ThrowXmlException( "SecureData missing a valid secureId.", xmlReader );
                }

                string context = xmlReader.GetAttribute( XmlConstants.SecureDataConstants.Context );

                if ( string.IsNullOrEmpty( context ) )
                {
                    ThrowXmlException( "SecureData missing non empty context.", xmlReader );
                }

                string encodedString = xmlReader.ReadString( );

                var data = Convert.FromBase64String( encodedString );

                SecureDataEntry entry = new SecureDataEntry( secureId, context, data );

                _secureData.Add( entry );
            } );

        }

        /// <summary>
        ///     Deserializes the package.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        /// <exception cref="XmlException">
        ///     Invalid application version id attribute
        ///     or
        ///     Missing application version id attribute
        /// </exception>
        private void DeserializePackage( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.MetadataConstants.Package );

            if ( xmlReader.MoveToAttribute( XmlConstants.Id ) )
            {
                Guid appVerId;

                if ( Guid.TryParse( xmlReader.Value, out appVerId ) )
                {
                    Metadata.AppVerId = appVerId;
                }
                else
                {
                    throw new XmlException( "Invalid application version id attribute" );
                }
            }

            while ( xmlReader.Read( ) )
            {
                if ( xmlReader.NodeType == XmlNodeType.Element && !xmlReader.IsEmptyElement )
                {
                    switch ( xmlReader.Name )
                    {
                        case XmlConstants.MetadataConstants.Name:
                            Metadata.Name = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.ApplicationName:
                            Metadata.AppName = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.Description:
                            Metadata.Description = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.Type:
                            SourceType type;

                            if ( Enum.TryParse( xmlReader.ReadElementContentAsString( ), out type ) )
                            {
                                Metadata.Type = type;
                            }
                            break;
                        case XmlConstants.MetadataConstants.Root:
                            Metadata.RootEntityId = XmlConvert.ToGuid( xmlReader.ReadElementContentAsString( ) );
                            break;
                        case XmlConstants.MetadataConstants.Version:
                            Metadata.Version = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.PlatformVersion:
                            Metadata.PlatformVersion = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.PublishDate:
                            DateTime publishDate;

                            if ( DateTime.TryParse( xmlReader.ReadElementContentAsString( ), null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out publishDate ) )
                            {
                                Metadata.PublishDate = publishDate;
                            }
                            break;
                        case XmlConstants.MetadataConstants.ReleaseDate:
                            DateTime dt;

                            if ( DateTime.TryParse( xmlReader.ReadElementContentAsString( ), null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt ) )
                            {
                                Metadata.ReleaseDate = dt;
                            }
                            break;
                        case XmlConstants.MetadataConstants.Dependencies:
                            DeserializeDependencies( xmlReader, xmlStack );
                            break;
                        case XmlConstants.EntityConstants.Entities:
                            DeserializeEntities( xmlReader, xmlStack );
                            break;
                        case XmlConstants.RelationshipConstants.Relationships:
                            DeserializeRelationships( xmlReader, xmlStack );
                            break;
                        case XmlConstants.BinaryConstants.Binaries:
                            DeserializeBinaries( xmlReader, xmlStack );
                            break;
                        case XmlConstants.DocumentConstants.Documents:
                            DeserializeDocuments( xmlReader, xmlStack );
                            break;
                        case XmlConstants.SecureDataConstants.SecureData:
                            DeserializeSecureData( xmlReader, xmlStack );
                            break;
                        case XmlConstants.AliasMap:
                            DeserializeAliasMap( xmlReader, xmlStack );
                            break;
                        case XmlConstants.DoNotRemoveConstants.DoNotRemove:
                            DeserializeDoNotRemove( xmlReader, xmlStack );
                            break;
                    }
                }
                else if ( xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == XmlConstants.MetadataConstants.Package && xmlReader.Depth == xmlStack.Count - 1 )
                {
                    PopStack( xmlStack, XmlConstants.MetadataConstants.Package );
                    break;
                }
            }
        }

        /// <summary>
        ///     Deserializes the relationship.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeRelationship( XmlReader xmlReader, Stack<string> xmlStack )
        {
            string relationshipIdAttribute = xmlReader.GetAttribute( XmlConstants.Id );

            string type;

            if ( relationshipIdAttribute != null )
            {
                Guid typeId = Guid.Empty;

                if ( string.IsNullOrEmpty( relationshipIdAttribute ) || !Guid.TryParse( relationshipIdAttribute, out typeId ) )
                {
                    ThrowXmlException( "Relationship does not contain a valid id attribute.", xmlReader );
                }

                type = typeId.ToString( "B" );
            }
            else
            {
                type = xmlReader.Name;
            }

            string from = null;
            string to = null;

            xmlStack.Push( xmlReader.Name );

            AdvanceReader( xmlReader, xmlStack, null, xmlReader.Name, ( r, s ) =>
            {
                if ( r.Name == XmlConstants.RelationshipConstants.From )
                {
                    from = r.ReadString( );
                }
                else if ( r.Name == XmlConstants.RelationshipConstants.To )
                {
                    to = r.ReadString( );
                }
            } );

            RelationshipContainer container = new RelationshipContainer( from, type, to );
            _relationshipMap.Add( container );
        }

        /// <summary>
        ///     Deserializes the relationships.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeRelationships( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.RelationshipConstants.Relationships );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.RelationshipConstants.Relationships, DeserializeRelationship );
        }

        /// <summary>
        ///     Deserializes the doNotRemove.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeDoNotRemove( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.DoNotRemoveConstants.DoNotRemove );

            AdvanceReader( xmlReader, xmlStack, XmlConstants.DoNotRemoveConstants.LeaveEntity, XmlConstants.DoNotRemoveConstants.DoNotRemove, DeserializeDoNotRemoveLeaveEntity );
        }

        /// <summary>
        ///     Deserializes the leaveEntity entries.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeDoNotRemoveLeaveEntity( XmlReader xmlReader, Stack<string> xmlStack )
        {
            string idAttribute = xmlReader.GetAttribute( XmlConstants.Id );

            Guid id = Guid.Empty;
            if ( idAttribute == null || !Guid.TryParse( idAttribute, out id ) )
            {
                ThrowXmlException( "leaveEntity does not contain a valid id.", xmlReader );
            }

            if ( !_doNotRemove.Contains( id ) )
            {
                _doNotRemove.Add( id );
            }
        }

        /// <summary>
        ///     Deserializes the XML.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeXml( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.Xml );

            XmlNamespaceManager namespaceManager = null;

            if ( xmlReader.NameTable != null )
            {
                namespaceManager = new XmlNamespaceManager( xmlReader.NameTable );
            }

            if ( namespaceManager != null )
            {
                while ( xmlReader.MoveToNextAttribute( ) )
                {
                    if ( xmlReader.Prefix == XmlConstants.XmlNs )
                    {
                        namespaceManager.AddNamespace( xmlReader.LocalName, xmlReader.Value );
                    }
                }
            }

            AdvanceReader( xmlReader, xmlStack, XmlConstants.MetadataConstants.Application, XmlConstants.Xml, DeserializeApplication );
        }

        /// <summary>
        ///     Gets the canonical alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        private string GetCanonicalAlias( string alias )
        {
            EntityAlias entityAlias = new EntityAlias( alias );

            string ns;
            if ( Namespaces.TryGetValue( entityAlias.Namespace, out ns ) )
            {
                return $"{ns}:{entityAlias.Alias}";
            }

            return entityAlias.ToString( );
        }

        /// <summary>
        ///     Loads the binary data.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private byte[] LoadBinaryData( string value )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return new byte[ 0 ];
            }

            return CompressionHelper.Decompress( Convert.FromBase64String( value ) );
        }

        /// <summary>
        ///     Pops the stack.
        /// </summary>
        /// <param name="xmlStack">The XML stack.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        private void PopStack( Stack<string> xmlStack, string name )
        {
            if ( xmlStack.Count <= 0 )
            {
                throw new InvalidOperationException( $"Xml stack corruption detected. Expected empty stack but found '{string.Join( ",", xmlStack.ToArray( ).Reverse( ) )}'" );
            }

            string value = xmlStack.Pop( );

            if ( value != name )
            {
                throw new InvalidOperationException( $"Xml stack corruption detected. Expected '{name}' but found '{value}'" );
            }
        }

        /// <summary>
        ///     Resolves the alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">@Invalid alias</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        private Guid ResolveAlias( string alias )
        {
            if ( alias == null )
            {
                throw new ArgumentNullException( nameof( alias ) );
            }

            if ( alias.Trim( ) == string.Empty )
            {
                throw new ArgumentException( @"Invalid alias", nameof( alias ) );
            }

            Guid id;
            if ( Guid.TryParse( alias, out id ) )
            {
                return id;
            }

            if ( _aliasMap.TryGetValue( GetCanonicalAlias( alias ), out id ) )
            {
                return id;
            }

            throw new InvalidOperationException( $"Cannot resolve alias '{alias}' to id." );
        }

        /// <summary>
        ///     Class representing the FieldContainer type.
        /// </summary>
        private class FieldContainer
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="FieldContainer" /> class.
            /// </summary>
            /// <param name="entityId">The entity identifier.</param>
            /// <param name="fieldId">The field identifier.</param>
            /// <param name="type">The type.</param>
            /// <param name="data">The data.</param>
            /// <param name="aliasMarkerId">The alias marker identifier.</param>
            public FieldContainer( Guid entityId, string fieldId, string type, string data = null, int aliasMarkerId = 0 )
            {
                EntityId = entityId;
                FieldId = fieldId;
                Type = type;
                Data = data;
                AliasMarkerId = aliasMarkerId;
            }

            /// <summary>
            ///     Gets or sets the alias marker identifier.
            /// </summary>
            /// <value>
            ///     The alias marker identifier.
            /// </value>
            public int AliasMarkerId
            {
                get;
                set;
            }

            /// <summary>
            ///     Gets or sets the entity identifier.
            /// </summary>
            /// <value>
            ///     The entity identifier.
            /// </value>
            public Guid EntityId
            {
                get;
            }

            /// <summary>
            ///     Gets the field identifier.
            /// </summary>
            /// <value>
            ///     The field identifier.
            /// </value>
            public string FieldId
            {
                get;
            }

            /// <summary>
            ///     Gets or sets the data.
            /// </summary>
            /// <value>
            ///     The data.
            /// </value>
            public string Data
            {
                get;
            }

            /// <summary>
            ///     Gets the type.
            /// </summary>
            /// <value>
            ///     The type.
            /// </value>
            public string Type
            {
                get;
            }

            /// <summary>
            ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals( object obj )
            {
                FieldContainer container = obj as FieldContainer;

                if ( container != null )
                {
                    return container.EntityId == EntityId && container.FieldId == FieldId && container.Type == Type && container.Data == Data && container.AliasMarkerId == AliasMarkerId;
                }

                return false;
            }

            /// <summary>
            ///     Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
            /// </returns>
            public override int GetHashCode( )
            {
                unchecked
                {
                    int hashCode = 17;

                    hashCode = hashCode * 92821 + EntityId.GetHashCode( );

                    if ( FieldId != null )
                    {
                        hashCode = hashCode * 92821 + FieldId.GetHashCode( );
                    }

                    if ( Type != null )
                    {
                        hashCode = hashCode * 92821 + Type.GetHashCode( );
                    }

                    if ( Data != null )
                    {
                        hashCode = hashCode * 92821 + Data.GetHashCode( );
                    }

                    hashCode = hashCode * 92821 + AliasMarkerId.GetHashCode( );

                    return hashCode;
                }
            }

            /// <summary>
            ///     Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            ///     A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString( )
            {
                return $"{EntityId:B} - {FieldId} => {Data}";
            }
        }

        /// <summary>
        ///     Class representing the RelationshipContainer type.
        /// </summary>
        private class RelationshipContainer
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="RelationshipContainer" /> class.
            /// </summary>
            /// <param name="from">From.</param>
            /// <param name="type">The type.</param>
            /// <param name="to">To.</param>
            public RelationshipContainer( string from, string type, string to )
            {
                From = from;
                Type = type;
                To = to;
            }


            /// <summary>
            ///     Gets from.
            /// </summary>
            /// <value>
            ///     From.
            /// </value>
            public string From
            {
                get;
            }

            /// <summary>
            ///     Gets the type.
            /// </summary>
            /// <value>
            ///     The type.
            /// </value>
            public string Type
            {
                get;
            }

            /// <summary>
            ///     Gets to.
            /// </summary>
            /// <value>
            ///     To.
            /// </value>
            public string To
            {
                get;
            }

            /// <summary>
            ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals( object obj )
            {
                RelationshipContainer container = obj as RelationshipContainer;

                if ( container != null )
                {
                    return Equals( container.From, From ) && Equals( container.Type, Type ) && Equals( container.To, To );
                }

                return false;
            }

            /// <summary>
            ///     Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
            /// </returns>
            public override int GetHashCode( )
            {
                unchecked
                {
                    int hashCode = 17;

                    hashCode = hashCode * 92821 + From.GetHashCode( );

                    if ( Type != null )
                    {
                        hashCode = hashCode * 92821 + Type.GetHashCode( );
                    }

                    if ( To != null )
                    {
                        hashCode = hashCode * 92821 + To.GetHashCode( );
                    }

                    return hashCode;
                }
            }

            /// <summary>
            ///     Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            ///     A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString( )
            {
                return $"{From} -> {Type} -> {To}";
            }
        }
    }
}