// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EDC.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using System.Globalization;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version2
{
    public partial class XmlDeserializerV2 : IXmlApplicationDeserializer
    {
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

            DeserializeRoot( xmlReader );

            DecodeMembers( );

            PackageData result = new PackageData
            {
                Binaries = _binaries,
                Documents = _documents,
                Entities = _entities,
                Relationships = _relationships,
                DoNotRemove = _doNotRemove,
                FieldData = _fieldData.ToDictionary( pair => pair.Key, pair => (IEnumerable<DataEntry>)pair.Value ),
                Metadata = _metadata,
                SecureData = _secureData
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

            Tuple<Guid, string> idAndType;

            if ( _aliasMap.TryGetValue( alias, out idAndType ) )
            {
                return idAndType.Item1;
            }

            throw new InvalidOperationException( $"Cannot resolve alias '{alias}' to id." );
        }

        /// <summary>
        ///     Process all members, resolve aliases, and determine if they are fields or relationships.
        ///     Note: type relationship instances also get stored here.
        /// </summary>
        private void DecodeMembers( )
        {
            _relationships = new List<RelationshipEntry>( );
            _fieldData = new Dictionary<string, IList<DataEntry>>( );

            foreach ( Member member in _members )
            {
                Guid memberId;
                string memberType;

                Guid entityId = ResolveAlias( member.EntityId );

                MemberGuidAndType( member, out memberId, out memberType );

                switch ( memberType )
                {
                    case XmlConstants.RelationshipConstants.Rel:
                    case XmlConstants.EntityConstants.Type:
                        Guid toId = ResolveAlias( member.Data );
                        _relationships.Add( new RelationshipEntry( memberId, entityId, toId ) );
                        break;

                    case XmlConstants.RelationshipConstants.RevRel:
                        Guid fromId = ResolveAlias( member.Data );
                        _relationships.Add( new RelationshipEntry( memberId, fromId, entityId ) );
                        break;

                    default:
                        ProcessField( member, entityId, memberId, memberType );
                        break;
                }

            }
        }

        /// <summary>
        ///     Extract the member GUID and the member type, either from the member entry, or by looking up the alias.
        /// </summary>
        /// <param name="member">The member entry from the parsed XML</param>
        /// <param name="memberId">The resulting member ID guid</param>
        /// <param name="memberType">The resulting member type</param>
        private void MemberGuidAndType( Member member, out Guid memberId, out string memberType )
        {
            string alias = member.MemberId;

            if ( string.IsNullOrEmpty( alias ) )
            {
                throw new InvalidOperationException( "Member has no memberId." );
            }

            Guid id;
            if ( Guid.TryParse( alias, out id ) )
            {
                // MemberID was a guid (and probably came from an id or typeId attribute)
                // MeberType information is already present on member.
                memberId = id;
                memberType = member.MemberType;
            }
            else
            {
                // MemberID must be a declared alias
                // MeberType must be available on the alias
                Tuple<Guid, string> idAndType;
                if ( _aliasMap.TryGetValue( alias, out idAndType ) )
                {
                    memberId = idAndType.Item1;
                    memberType = idAndType.Item2;
                }
                else
                {
                    throw new InvalidOperationException( $"Cannot resolve alias '{alias}' to id." );
                }
            }

            if ( string.IsNullOrEmpty( memberType ) )
            {
                throw new InvalidOperationException( $"Cannot member type for {member.MemberId}" );
            }
        }

        /// <summary>
        ///     Gets the data.
        /// </summary>
        /// <value>
        ///     The data.
        /// </value>
        private void ProcessField( Member member, Guid entityId, Guid memberId, string memberType )
        {
            string xmlText = member.Data;
            string type = null;
            bool ok = false;

            DataEntry entry = new DataEntry
            {
                EntityId = entityId,
                FieldId = memberId,
                AliasMarkerId = memberId == Guids.ReverseAlias ? 1 : 0
            };

            // Process field types
            switch ( memberType )
            {
                case XmlConstants.FieldConstants.TextField:
                    type = Helpers.NVarCharName;
                    ok = true;
                    entry.Data = xmlText;
                    break;

                case XmlConstants.FieldConstants.IntField:
                    int intValue;
                    type = Helpers.IntName;
                    ok = int.TryParse( xmlText, out intValue );
                    entry.Data = intValue;
                    break;

                case XmlConstants.FieldConstants.AliasField:
                    type = Helpers.AliasName;
                    EntityAlias alias = new EntityAlias( xmlText );
                    entry.Data = alias.Alias;
                    entry.Namespace = alias.Namespace;
                    ok = true;
                    break;

                case XmlConstants.FieldConstants.BoolField:
                    bool boolValue;
                    type = Helpers.BitName;
                    ok = bool.TryParse( xmlText, out boolValue );
                    entry.Data = boolValue;
                    break;

                case XmlConstants.FieldConstants.DateTimeField:
                    DateTime dateTimeValue;
                    type = Helpers.DateTimeName;
                    ok = DateTime.TryParse( xmlText, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dateTimeValue );
                    entry.Data = dateTimeValue;
                    break;

                case XmlConstants.FieldConstants.DecimalField:
                    decimal decimalValue;
                    type = Helpers.DecimalName;
                    ok = decimal.TryParse( xmlText, out decimalValue );
                    entry.Data = decimalValue;
                    break;

                case XmlConstants.FieldConstants.GuidField:
                    Guid guidValue;
                    type = Helpers.GuidName;
                    ok = Guid.TryParse( xmlText, out guidValue );
                    entry.Data = guidValue;
                    break;

                case XmlConstants.FieldConstants.XmlField:
                    type = Helpers.XmlName;
                    ok = true;
                    entry.Data = xmlText;
                    break;
            }

            // Lookup Field type table
            if ( string.IsNullOrEmpty( type ) )
            {
                throw new InvalidOperationException( $"Entity '{member.EntityId}' field '{memberId}' has no known type '{memberType}'" );
            }

            IList<DataEntry> dataEntries;
            if ( !_fieldData.TryGetValue( type, out dataEntries ) )
            {
                dataEntries = new List<DataEntry>( );
                _fieldData[ type ] = dataEntries;
            }

            // Store entry
            if ( !ok )
            {
                Context.WriteWarning( $"Detected invalid {memberType} value. Field '{memberId}' on entity '{entityId}'" );
            }
            else
            {
                dataEntries.Add( entry );
            }
        }
    }
}