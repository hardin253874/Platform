// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Sources;
using System.Globalization;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version2
{
    /// <summary>
    /// 
    /// </summary>
    partial class XmlDeserializerV2
    {
        /// <summary>
        ///     The alias map. key=alias, value=UpgradeId, alias type.
        /// </summary>
        private readonly Metadata _metadata = new Metadata( );

        /// <summary>
        ///     The alias map. key=alias, value=UpgradeId, alias type.
        /// </summary>
        private readonly IDictionary<string, Tuple<Guid, string>> _aliasMap = new Dictionary<string, Tuple<Guid, string>>( );

        /// <summary>
        ///     The binaries
        /// </summary>
        private readonly IList<BinaryDataEntry> _binaries = new List<BinaryDataEntry>( );

        /// <summary>
        ///     The documents
        /// </summary>
        private readonly IList<DocumentDataEntry> _documents = new List<DocumentDataEntry>( );

        /// <summary>
        ///     The data
        /// </summary>
        private readonly IList<SecureDataEntry> _secureData = new List<SecureDataEntry>( );

        /// <summary>
        ///     Registry of GUIDs that should not be removed as part of an upgrade.
        /// </summary>
        private readonly ISet<Guid> _doNotRemove = new HashSet<Guid>( );

        /// <summary>
        ///     Namespace manager
        /// </summary>
        private XmlNamespaceManager _namespaceManager;

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
        ///     Deserializes the alias.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeAlias( XmlReader xmlReader, Stack<string> xmlStack )
        {
            string idAttribute = xmlReader.GetAttribute( XmlConstants.Id );

            // id
            Guid id = Guid.Empty;

            if ( idAttribute == null || !Guid.TryParse( idAttribute, out id ) )
            {
                ThrowXmlException( "AliasMap does not contain a valid id.", xmlReader );
            }

            // type
            string aliasType = xmlReader.GetAttribute( XmlConstants.Type );

            string nsAlias = xmlReader.NamespaceURI + ":" + xmlReader.LocalName;
            var entry = new Tuple<Guid, string>( id, aliasType );

            _aliasMap[ xmlReader.NamespaceURI + ":" + xmlReader.LocalName ] = entry;

            // Add dictionary entry to lookup by short alias
            string prefix = _namespaceManager.LookupPrefix( xmlReader.NamespaceURI );
            if (string.IsNullOrEmpty(prefix))
                _aliasMap[ xmlReader.LocalName ] = entry;
            else
                _aliasMap[ prefix + ":" + xmlReader.LocalName ] = entry;
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

            if ( _metadata.Dependencies == null )
            {
                _metadata.Dependencies = new List<SolutionDependency>( );
            }

            SolutionDependency dependency = new SolutionDependency( );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.MetadataConstants.Dependency, ( reader, stack ) => DeserializeDependencyFields( dependency, reader ) );

            _metadata.Dependencies.Add( dependency );
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
        private void DeserializeMetadata( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.MetadataConstants.Metadata );

            if ( xmlReader.MoveToAttribute( XmlConstants.Id ) )
            {
                Guid appVerId;

                if ( Guid.TryParse( xmlReader.Value, out appVerId ) )
                {
                    _metadata.AppVerId = appVerId;
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
                        case XmlConstants.MetadataConstants.Application:
                            Guid appId;
                            if ( Guid.TryParse( xmlReader.GetAttribute( XmlConstants.Id ), out appId ) )
                            {
                                _metadata.AppId = appId;
                            }
                            break;
                        case XmlConstants.MetadataConstants.Package:
                            Guid packageId;
                            if ( Guid.TryParse( xmlReader.GetAttribute( XmlConstants.Id ), out packageId ) )
                            {
                                _metadata.AppVerId = packageId;
                            }
                            break;
                        case XmlConstants.MetadataConstants.Name:
                            _metadata.Name = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.ApplicationName:
                            _metadata.AppName = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.Description:
                            _metadata.Description = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.Type:
                            SourceType type;

                            if ( Enum.TryParse( xmlReader.ReadElementContentAsString( ), out type ) )
                            {
                                _metadata.Type = type;
                            }
                            break;
                        case XmlConstants.MetadataConstants.Version:
                            _metadata.Version = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.PlatformVersion:
                            _metadata.PlatformVersion = xmlReader.ReadElementContentAsString( );
                            break;
                        case XmlConstants.MetadataConstants.PublishDate:
                            DateTime publishDate;

                            if ( DateTime.TryParse( xmlReader.ReadElementContentAsString( ), null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out publishDate ) )
                            {
                                _metadata.PublishDate = publishDate;
                            }
                            break;
                        case XmlConstants.MetadataConstants.ReleaseDate:
                            DateTime dt;

                            if ( DateTime.TryParse( xmlReader.ReadElementContentAsString( ), null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt ) )
                            {
                                _metadata.ReleaseDate = dt;
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
                else if ( xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == XmlConstants.MetadataConstants.Metadata && xmlReader.Depth == xmlStack.Count - 1 )
                {
                    PopStack( xmlStack, XmlConstants.MetadataConstants.Metadata );
                    break;
                }
            }
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
        private void DeserializeRoot( XmlReader xmlReader )
        {
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
        }

        /// <summary>
        ///     Deserializes the XML.
        /// </summary>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeXml( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.Xml );

            // Load namespaces
            if ( xmlReader.NameTable == null )
                throw new InvalidOperationException( "xmlReader.NameTable is null" );

            _namespaceManager = new XmlNamespaceManager( xmlReader.NameTable );

            while ( xmlReader.MoveToNextAttribute( ) )
            {
                if ( xmlReader.Prefix == XmlConstants.XmlNs )
                {
                    _namespaceManager.AddNamespace( xmlReader.LocalName, xmlReader.Value );
                }
            }

            while ( xmlReader.Read( ) )
            {
                if ( xmlReader.NodeType == XmlNodeType.Element && !xmlReader.IsEmptyElement )
                {
                    switch ( xmlReader.Name )
                    {
                        case XmlConstants.MetadataConstants.Metadata:
                            DeserializeMetadata( xmlReader, xmlStack );
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
                else if ( xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == XmlConstants.Xml && xmlReader.Depth == xmlStack.Count - 1 )
                {
                    PopStack( xmlStack, XmlConstants.Xml );
                    break;
                }
            }
        }
    }
}
