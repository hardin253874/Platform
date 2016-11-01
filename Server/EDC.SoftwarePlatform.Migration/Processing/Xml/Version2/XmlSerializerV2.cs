// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Xml;
using EDC.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.Xml;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version2
{
    /// <summary>
    ///     Class representing the XmlSerializer type.
    /// </summary>
    internal class XmlSerializerV2 : IXmlApplicationSerializer
    {
        #region Declarations

        /// <summary>
        ///     The alias to data type map
        /// </summary>
        private readonly IDictionary<EntityAlias, string> _aliasToDataTypeMap = new Dictionary<EntityAlias, string>( );

        /// <summary>
        ///     The data map
        /// </summary>
        private readonly IDictionary<string, IDictionary<Guid, IList<DataEntry>>> _dataMap = new Dictionary<string, IDictionary<Guid, IList<DataEntry>>>( );

        /// <summary>
        ///     The upgrade identifier to alias map
        ///     Key is GUID, and direction. True for reverseAlias.
        /// </summary>
        private readonly IDictionary<Tuple<Guid, bool>, EntityAlias> _upgradeIdToAliasMap = new Dictionary<Tuple<Guid, bool>, EntityAlias>( );

        /// <summary>
        ///     The root entity in the hierarchy
        /// </summary>
        private EntityHierarchyEntry _root;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XmlSerializerV2" /> class.
        /// </summary>
        public XmlSerializerV2( )
        {
            Version = "2.0";

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
        #endregion

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
                throw new ArgumentNullException( nameof( xmlWriter ) );

            if ( context == null )
                context = new ProcessingContext( );

            Context = context;

            RestructureFieldData( );

            // XmlSerializerV2 needs metadata about relationships to build a nice hierarchy.
            // Most IDataSource don't provide it. So hit up the application library hacker to get some metadata.
            // Cross finders.
            if ( PackageData.Metadata.RelationshipTypeCallback == null )
            {
                AppLibraryRelationshipMetadataRepository appLibRelMetadataRepos = new AppLibraryRelationshipMetadataRepository( );
                PackageData.Metadata.RelationshipTypeCallback = appLibRelMetadataRepos.CreateMetadataCallback( PackageData.Relationships );
            }

            Stack<string> xmlStack = new Stack<string>( );

            SerializeHeader( xmlWriter, xmlStack );

            if ( xmlStack.Count > 0 )
            {
                throw new InvalidOperationException( $"Xml stack corruption detected. Expected empty stack but found '{string.Join( ",", xmlStack.ToArray( ).Reverse( ) )}'" );
            }
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
            if ( _upgradeIdToAliasMap.Count == 0 )
                return;

            using ( xmlWriter.WriteElementBlock( XmlConstants.AliasMap, xmlStack ) )
            {
                foreach ( var upgradeIdAliasPair in _upgradeIdToAliasMap.Where( p => p.Value != null ).OrderBy( p => p.Value.Alias ) )
                {
                    string ns = upgradeIdAliasPair.Value.Namespace;
                    string elementName = upgradeIdAliasPair.Value.Alias.ToCamelCase( );
                    Guid id = upgradeIdAliasPair.Key.Item1;

                    using ( xmlWriter.WriteElementBlock( elementName, ns, xmlStack ) )
                    {
                        xmlWriter.WriteAttributeString( XmlConstants.Id, XmlConvert.ToString( id ) );

                        string dataType;
                        if ( _aliasToDataTypeMap.TryGetValue( upgradeIdAliasPair.Value, out dataType ) )
                        {
                            xmlWriter.WriteAttributeString( XmlConstants.Type, DataTypeToXmlName( dataType ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts a data type string to an XML name string.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private string DataTypeToXmlName( string dataType )
        {
            switch ( dataType )
            {
                case Helpers.AliasName:
                    return XmlConstants.FieldConstants.AliasField;
                case Helpers.BitName:
                    return XmlConstants.FieldConstants.BoolField;
                case Helpers.DateTimeName:
                    return XmlConstants.FieldConstants.DateTimeField;
                case Helpers.DecimalName:
                    return XmlConstants.FieldConstants.DecimalField;
                case Helpers.GuidName:
                    return XmlConstants.FieldConstants.GuidField;
                case Helpers.IntName:
                    return XmlConstants.FieldConstants.IntField;
                case Helpers.NVarCharName:
                    return XmlConstants.FieldConstants.TextField;
                case Helpers.XmlName:
                    return XmlConstants.FieldConstants.XmlField;
                case XmlConstants.EntityConstants.Type:
                case XmlConstants.RelationshipConstants.Rel:
                case XmlConstants.RelationshipConstants.RevRel:
                    return dataType;
                default:
                    throw new InvalidOperationException( dataType );
            }
        }


        /// <summary>
        ///     Serializes the binary.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void SerializeBinary( XmlWriter xmlWriter, Stack<string> xmlStack )
        {
            IEnumerable<BinaryDataEntry> binaries = PackageData.Binaries;
            if ( binaries == null )
                return;

            using ( xmlWriter.WriteElementBlock( XmlConstants.BinaryConstants.Binaries, xmlStack ) )
            {
                foreach ( BinaryDataEntry binary in binaries )
                {
                    if ( binary.Data == null )
                        continue;
                    using ( xmlWriter.WriteElementBlock( XmlConstants.BinaryConstants.Binary, xmlStack ) )
                    {
                        xmlWriter.WriteAttributeString( XmlConstants.BinaryConstants.Hash, binary.DataHash );
                        if ( !string.IsNullOrEmpty( binary.FileExtension ) )
                        {
                            xmlWriter.WriteAttributeString( XmlConstants.BinaryConstants.Extension, binary.FileExtension );
                        }

                        xmlWriter.WriteString( Convert.ToBase64String( CompressionHelper.Compress( binary.Data ) ) );
                    }
                }
            }
        }

        /// <summary>
        ///     Serializes the dependencies.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void SerializeDependencies( XmlWriter xmlWriter, Stack<string> xmlStack )
        {
            IList<SolutionDependency> dependencies = PackageData.Metadata?.Dependencies;
            if ( dependencies == null )
                return;

            using ( xmlWriter.WriteElementBlock( XmlConstants.MetadataConstants.Dependencies, xmlStack ) )
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

                    xmlWriter.WriteElementString( XmlConstants.MetadataConstants.ApplicationId, XmlConvert.ToString( dependency.DependencyApplication ) );

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
        }

        /// <summary>
        ///     Serializes the list of entities that should not be removed during upgrade operations.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void SerializeDoNotRemove( XmlWriter xmlWriter, Stack<string> xmlStack )
        {
            IEnumerable<Guid> doNotRemove = PackageData.DoNotRemove;
            if ( doNotRemove == null )
                return;

            using ( xmlWriter.WriteElementBlock( XmlConstants.DoNotRemoveConstants.DoNotRemove, xmlStack ) )
            {
                foreach ( Guid upgradeId in doNotRemove )
                {
                    xmlWriter.WriteStartElement( XmlConstants.DoNotRemoveConstants.LeaveEntity, xmlStack );
                    xmlWriter.WriteAttributeString( XmlConstants.Id, XmlConvert.ToString( upgradeId ) );
                    xmlWriter.WriteEndElement( XmlConstants.DoNotRemoveConstants.LeaveEntity, xmlStack );
                }
            }
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
            if ( documents == null )
                return;

            using ( xmlWriter.WriteElementBlock( XmlConstants.DocumentConstants.Documents, xmlStack ) )
            {
                foreach ( DocumentDataEntry document in documents )
                {
                    using ( xmlWriter.WriteElementBlock( XmlConstants.DocumentConstants.Document, xmlStack ) )
                    {
                        xmlWriter.WriteAttributeString( XmlConstants.DocumentConstants.Hash, document.DataHash );
                        if ( !string.IsNullOrEmpty( document.FileExtension ) )
                        {
                            xmlWriter.WriteAttributeString( XmlConstants.DocumentConstants.Extension, document.FileExtension );
                        }

                        if ( document.Data != null )
                        {
                            xmlWriter.WriteString( Convert.ToBase64String( CompressionHelper.Compress( document.Data ) ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Serializes the entities.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void SerializeEntities( XmlWriter xmlWriter, Stack<string> xmlStack )
        {
            IEnumerable<EntityEntry> entities = PackageData.Entities;

            HierarchyBuilder builder = new HierarchyBuilder( );
            builder.RelationshipMetadataCallback = PackageData.Metadata.RelationshipTypeCallback;

            _root = builder.BuildEntityHierarchy( PackageData );

            /////
            // By this point the entire entity graph has been built. Now serialize it
            /////
            using ( xmlWriter.WriteElementBlock( XmlConstants.EntityConstants.Entities, xmlStack ) )
            {
                if ( _root.Children != null )
                {
                    var typeGroups = _root.Children
                        .GroupBy( e => e.TypeRelationship?.ToId )
                        .Select( g => new { TypeId = GuidOrAlias( g?.Key ), Group = g } )
                        .OrderBy( g => TypeOrder( g.TypeId ) ) // move 'solution' to the top
                        .ThenBy( g => g.TypeId );

                    foreach ( var group in typeGroups )
                    {
                        using ( xmlWriter.WriteElementBlock( XmlConstants.EntityConstants.Group, xmlStack ) )
                        {
                            string typeId = group.TypeId;
                            if ( typeId != null )
                            {
                                // Note : this is purely for grouping cosmetics, do not use it for reading information.
                                xmlWriter.WriteAttributeString( XmlConstants.EntityConstants.TypeId, typeId );
                            }

                            SerializeEntityList( xmlWriter, group.Group, xmlStack );
                        }
                    }
                }
            }
        }


        /// <summary>
        ///     Serializes the entities.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="entities">The XML writer.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void SerializeEntityList( XmlWriter xmlWriter, IEnumerable<EntityHierarchyEntry> entities, Stack<string> xmlStack )
        {
            foreach ( EntityHierarchyEntry entityNode in entities.OrderBy( e=> e.TypeRelationship?.ToId ).ThenBy( e=> e.Entity.EntityId ) )
            {
                SerializeEntity( xmlWriter, entityNode, xmlStack );
            }
        }

        private void SerializeEntity( XmlWriter xmlWriter, EntityHierarchyEntry entityNode, Stack<string> xmlStack )
        {
            EntityEntry entity = entityNode.Entity;

            Guid typeId = entityNode.TypeRelationship?.ToId ?? Guid.Empty;

            WriteEntityNameComment( xmlWriter, entity );

            // TODO : HANDLE MISSING TYPE RELATIONSHIP PROPERLY

            using ( WriteElementBlock( xmlWriter, typeId, XmlConstants.EntityConstants.Type, XmlConstants.EntityConstants.Entity, XmlConstants.EntityConstants.TypeId, xmlStack ) )
            {
                xmlWriter.WriteAttributeString( XmlConstants.Id, XmlConvert.ToString( entity.EntityId ) );

                SerializeEntityFields( xmlWriter, entity, xmlStack );

                SerializeEntityRelationships( xmlWriter, entityNode, xmlStack );

                SerializeNestedEntities( xmlWriter, entityNode, xmlStack );
            }
        }

        private void SerializeEntityRelationships( XmlWriter xmlWriter, EntityHierarchyEntry entityNode, Stack<string> xmlStack )
        {
            if ( entityNode.ForwardRelationships != null )
            {
                string relTag = XmlConstants.RelationshipConstants.Rel;

                foreach ( RelationshipEntry relationship in entityNode.ForwardRelationships )
                {
                    using ( WriteElementBlock( xmlWriter, relationship.TypeId, relTag, relTag, XmlConstants.Id, xmlStack, false ) )
                    {
                        xmlWriter.WriteString( GetInnerText( relationship.ToId ) );
                    }
                }
            }

            if ( entityNode.ReverseRelationships != null )
            {
                foreach ( RelationshipEntry relationship in entityNode.ReverseRelationships )
                {
                    string relTag = XmlConstants.RelationshipConstants.RevRel;

                    using ( WriteElementBlock( xmlWriter, relationship.TypeId, relTag, relTag, XmlConstants.Id, xmlStack, true ) )
                    {
                        xmlWriter.WriteString( GetInnerText( relationship.FromId ) );
                    }
                }
            }
        }

        private void SerializeNestedEntities( XmlWriter xmlWriter, EntityHierarchyEntry entityNode, Stack<string> xmlStack )
        {
            if ( entityNode.Children == null )
                return;

            // Children grouped by rel type
            var childrenByType = entityNode.Children
                .GroupBy( e => new Tuple<Direction, Guid>( e.Direction, e.RelationshipFromParent.TypeId ) )
                .OrderBy( t => t.Key );

            foreach ( var childList in childrenByType )
            {
                Direction dir = childList.Key.Item1;
                Guid relTypeId = childList.Key.Item2;

                string relTag = dir == Direction.Forward ? XmlConstants.RelationshipConstants.Rel : XmlConstants.RelationshipConstants.RevRel;

                using ( WriteElementBlock( xmlWriter, relTypeId, relTag, relTag, XmlConstants.Id, xmlStack, dir == Direction.Reverse ) )
                {
                    foreach ( EntityHierarchyEntry childNode in childList )
                    {
                        SerializeEntity( xmlWriter, childNode, xmlStack );
                    }
                }
            }
        }

        private void WriteEntityNameComment( XmlWriter xmlWriter, EntityEntry entity )
        {
            IDictionary<Guid, IList<DataEntry>> stringData;
            if ( !_dataMap.TryGetValue( Helpers.NVarCharName, out stringData ) )
                return;

            IList<DataEntry> entityStringData;
            if ( !stringData.TryGetValue( entity.EntityId, out entityStringData ) )
                return;
            
            string name = entityStringData.FirstOrDefault( de => de.FieldId == Guids.Name )?.Data as string;

            if ( !string.IsNullOrEmpty( name ) )
            {
                xmlWriter.WriteComment( string.Concat(' ', name, ' ' ) );
            }
        }

        private void SerializeEntityFields( XmlWriter xmlWriter, EntityEntry entity, Stack<string> xmlStack )
        {
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
                        SerializeField( xmlWriter, nameData, Helpers.NVarCharName, XmlConstants.FieldConstants.TextField, o => o.ToString( ), xmlStack );
                        entityStringData.Remove( nameData );
                    }

                    var descriptionData = entityStringData.FirstOrDefault( de => de.FieldId == Guids.Description );

                    if ( descriptionData != null )
                    {
                        SerializeField( xmlWriter, descriptionData, Helpers.NVarCharName, XmlConstants.FieldConstants.TextField, o => o.ToString( ), xmlStack );
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
                            return null;
                        }

                        var s = o as string;
                        if ( s != null )
                        {
                            return s;
                        }

                        if ( o is DateTime )
                        {
                            DateTime utc = DateTime.SpecifyKind( ( DateTime ) o, DateTimeKind.Utc );
                            return utc.ToString( "u" );
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
                    string defaultElementName = DataTypeToXmlName( dataType );

                    foreach ( DataEntry dataEntry in dataEntries.OrderBy( e => e.FieldId ) )
                    {
                        if ( dataEntry.Data != null )
                        {
                            SerializeField( xmlWriter, dataEntry, dataType, defaultElementName, customFormatter, xmlStack );
                        }
                    }
                }
            }
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

            bool resolved = TryGetElementName( dataEntry.FieldId, dataType, false, out elementName, out nameSpace );

            // XML fields are deprecated
            if ( dataType == Helpers.XmlName )
                return;

            using ( WriteElementBlock( xmlWriter, dataEntry.FieldId, dataType, defaultElementName, XmlConstants.Id, xmlStack ) )
            {
                if ( dataType == Helpers.AliasName )
                {
                    xmlWriter.WriteString( customFormatter( dataEntry.Namespace + ":" + dataEntry.Data ) );
                }
                else
                {
                    xmlWriter.WriteString( customFormatter( dataEntry.Data ) );
                }
            }
        }

        /// <summary>
        ///     Serializes the header.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void SerializeHeader( XmlWriter xmlWriter, Stack<string> xmlStack )
        {
            xmlWriter.WriteComment( XmlConstants.HeaderComment );
            using ( xmlWriter.WriteElementBlock( XmlConstants.Xml, DefaultNamespace, xmlStack ) )
            {
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

                SerializeEntities( xmlWriter, xmlStack );
                SerializeRelationships( xmlWriter, xmlStack );
                SerializeSecureData( xmlWriter, xmlStack );
                SerializeBinary( xmlWriter, xmlStack );
                SerializeDocuments( xmlWriter, xmlStack );
                SerializeAliasMap( xmlWriter, xmlStack );
                SerializeDoNotRemove( xmlWriter, xmlStack );
            }
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

            DateTime publishDate = metadata.PublishDate;
            if ( publishDate == DateTime.MinValue )
            {
                publishDate = DateTime.UtcNow;
            }

            using ( xmlWriter.WriteElementBlock( XmlConstants.MetadataConstants.Metadata, xmlStack ) )
            {
                if ( metadata.Type == Sources.SourceType.DataExport )
                {
                    xmlWriter.WriteElementString( XmlConstants.MetadataConstants.Type, metadata.Type.ToString( ) );
                    xmlWriter.WriteElementString( XmlConstants.MetadataConstants.PlatformVersion, metadata.PlatformVersion ?? string.Empty );
                    xmlWriter.WriteElementString( XmlConstants.MetadataConstants.PublishDate, DateTime.UtcNow.ToString( "u" ) );
                }
                else
                {
                    xmlWriter.WriteElementString( XmlConstants.MetadataConstants.Type, metadata.Type.ToString( ) );

                    using ( xmlWriter.WriteElementBlock( XmlConstants.MetadataConstants.Application, xmlStack ) )
                    {
                        xmlWriter.WriteAttributeString( XmlConstants.Id, XmlConvert.ToString( metadata.AppId ) );
                        xmlWriter.WriteElementString( XmlConstants.MetadataConstants.Name, metadata.Name ?? string.Empty );
                        xmlWriter.WriteElementString( XmlConstants.MetadataConstants.ApplicationName, metadata.AppName ?? string.Empty );
                        xmlWriter.WriteElementString( XmlConstants.MetadataConstants.Description, metadata.Description ?? string.Empty );
                    }
                    using ( xmlWriter.WriteElementBlock( XmlConstants.MetadataConstants.Package, xmlStack ) )
                    {
                        xmlWriter.WriteAttributeString( XmlConstants.Id, XmlConvert.ToString( metadata.AppVerId ) );
                        xmlWriter.WriteElementString( XmlConstants.MetadataConstants.Version, VersionOverride ?? metadata.Version ?? string.Empty );
                        xmlWriter.WriteElementString( XmlConstants.MetadataConstants.PlatformVersion, metadata.PlatformVersion ?? string.Empty );
                        xmlWriter.WriteElementString( XmlConstants.MetadataConstants.ReleaseDate, metadata.ReleaseDate.ToUniversalTime( ).ToString( "u" ) );
                        xmlWriter.WriteElementString( XmlConstants.MetadataConstants.PublishDate, publishDate.ToUniversalTime( ).ToString( "u" ) );
                    }
                }

                SerializeDependencies( xmlWriter, xmlStack );
            }
        }

        /// <summary>
        ///     Serializes the relationship.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="defaultTypeName">Default name of the type.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void SerializeRelationship( XmlWriter xmlWriter, RelationshipEntry relationship, Stack<string> xmlStack )
        {
            using ( WriteElementBlock( xmlWriter, relationship.TypeId, XmlConstants.RelationshipConstants.Rel, XmlConstants.RelationshipConstants.Rel, XmlConstants.Id, xmlStack ) )
            {
                using ( xmlWriter.WriteElementBlock( XmlConstants.RelationshipConstants.From, xmlStack ) )
                {
                    xmlWriter.WriteString( GetInnerText( relationship.FromId ) );
                }
                using ( xmlWriter.WriteElementBlock( XmlConstants.RelationshipConstants.To, xmlStack ) )
                {
                    xmlWriter.WriteString( GetInnerText( relationship.ToId ) );
                }
            }
        }

        /// <summary>
        ///     Serializes the relationships.
        /// </summary>
        private void SerializeRelationships( XmlWriter xmlWriter, Stack<string> xmlStack )
        {
            using ( xmlWriter.WriteElementBlock( XmlConstants.RelationshipConstants.Relationships, xmlStack ) )
            {
                if ( _root.ForwardRelationships != null )
                {
                    foreach ( RelationshipEntry relationship in _root.ForwardRelationships.OrderBy( r => r.TypeId ).ThenBy( r => r.FromId ).ThenBy( r => r.ToId ) )
                    {
                        SerializeRelationship( xmlWriter, relationship, xmlStack );
                    }
                }
            }
        }

        /// <summary>
        ///     Serializes secure data
        /// </summary>
        private void SerializeSecureData( XmlWriter xmlWriter, Stack<string> xmlStack )
        {
            var secureData = PackageData.SecureData;

            if ( secureData != null )
            {
                xmlWriter.WriteStartElement( XmlConstants.SecureDataConstants.SecureData, xmlStack );

                foreach ( SecureDataEntry entry in secureData.OrderBy( r => r.Context ).ThenBy( r => r.SecureId ) )
                {
                    xmlWriter.WriteStartElement( XmlConstants.SecureDataConstants.SecureDataEntry, xmlStack );
                    xmlWriter.WriteAttributeString( XmlConstants.SecureDataConstants.SecureId, entry.SecureId.ToString( ) );
                    xmlWriter.WriteAttributeString( XmlConstants.SecureDataConstants.Context, entry.Context );

                    xmlWriter.WriteString( Convert.ToBase64String( entry.Data ) );

                    xmlWriter.WriteEndElement( XmlConstants.SecureDataConstants.SecureDataEntry, xmlStack );
                }

                xmlWriter.WriteEndElement( XmlConstants.SecureDataConstants.SecureData, xmlStack );

            }
        }

        /// <summary>
        ///     Write an XML element using alias or ID with a dispose to close the element.
        /// </summary>
        /// <param name="xmlWriter">The writer</param>
        /// <param name="entityId">The ID of the element being represented.</param>
        /// <param name="aliasType">The type of the alias.</param>
        /// <param name="defaultElementName">The name of the XML element to use, if no alias can be found.</param>
        /// <param name="idAttribName">The name of the XML attribute that will contain the ID.</param>
        /// <param name="xmlStack">xmlStack</param>
        /// <param name="reverseAlias">If true, a reverse alias is required.</param>
        /// <returns></returns>
        private IDisposable WriteElementBlock( XmlWriter xmlWriter, Guid entityId, string aliasType, string defaultElementName, string idAttribName, Stack<string> xmlStack, bool reverseAlias = false )
        {
            string elementName;
            string nameSpace;

            bool resolved = TryGetElementName( entityId, aliasType, reverseAlias, out elementName, out nameSpace );

            IDisposable result = xmlWriter.WriteElementBlock( elementName ?? defaultElementName, nameSpace, xmlStack );

            if ( !resolved && entityId != Guid.Empty )
            {
                xmlWriter.WriteAttributeString( idAttribName, XmlConvert.ToString( entityId ) );
            }

            return result;
        }

        /// <summary>
        ///     Gets the inner text.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private string GetInnerText( Guid id )
        {
            var alias = ResolveAlias( id, false );

            if ( alias != null )
            {
                string ns = alias.Namespace;
                if ( ns.StartsWith( "core" ) )
                    return alias.Alias;
                if ( ns.StartsWith( "console" ) )
                    return "k:" + alias.Alias;
                return alias.ToString( );
            }

            return XmlConvert.ToString( id );
        }

        /// <summary>
        ///     Resolves the alias.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private EntityAlias ResolveAlias( Guid id, bool reverseAlias )
        {
            EntityAlias entityAlias;
            var key = new Tuple<Guid, bool>( id, reverseAlias );

            if ( _upgradeIdToAliasMap.TryGetValue( key, out entityAlias ) )
                return entityAlias;

            if ( reverseAlias )
            {
                if ( PackageData.Metadata.RelationshipTypeCallback == null )
                    return null;
                RelationshipTypeEntry relType = PackageData.Metadata.RelationshipTypeCallback( id );
                if ( relType?.ReverseAlias == null )
                    return null;
                entityAlias = new EntityAlias( relType.ReverseAlias );
            }
            else
            {
                entityAlias = NameResolver?.Resolve( id );
            }

            _upgradeIdToAliasMap[ key ] = entityAlias;

            return entityAlias;
        }


        /// <summary>
        ///     Tries the name of the get field element.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="aliasType">Type of the alias.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <returns></returns>
        private bool TryGetElementName( Guid id, string aliasType, bool reverseAlias, out string alias, out string nameSpace )
        {
            var entityAlias = ResolveAlias( id, reverseAlias );

            if ( entityAlias != null )
            {
                _aliasToDataTypeMap[ entityAlias ] = aliasType;

                nameSpace = entityAlias.Namespace;
                alias = entityAlias.Alias.ToCamelCase( );

                return true;
            }

            nameSpace = null;
            alias = null;

            return false;
        }

        /// <summary>
        /// Convert a GUID to a string, using alias if possible.
        /// </summary>
        /// <remarks>
        /// Use for ordering, not for data storage.
        /// </remarks>
        private string GuidOrAlias( Guid? id )
        {
            if ( id == null )
                return null;

            var entityAlias = ResolveAlias( id.Value, false );

            if ( entityAlias == null )
                return XmlConvert.ToString( id.Value );

            if ( entityAlias.Namespace == "core" )
                return entityAlias.Alias;

            string prefix;
            if ( Namespaces.TryGetValue( entityAlias.Namespace, out prefix ) )
                return string.Concat( prefix, ":", entityAlias.Alias );

            return entityAlias.ToString( );
        }

        /// <summary>
        /// Shuffles special type(s) to the top.
        /// </summary>
        /// <remarks>
        /// Use for ordering, not for data storage.
        /// </remarks>
        private int TypeOrder( string typeId )
        {
            if ( typeId == "solution" )
                return 1;

            // Entity with no type .. invalid but may be present. Place at end.
            if ( typeId == null )
                return 100;

            // Guid
            if ( typeId.Length == 36 && typeId[ 8 ] == '-' )
                return 99;

            // Alias
            return 2;
        }
    }
}