// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Xml;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version2
{
    /// <summary>
    /// 
    /// </summary>
    partial class XmlDeserializerV2
    {
        /// <summary>
        ///     The relationship map
        /// </summary>
        private readonly IList<Member> _members = new List<Member>( );

        /// <summary>
        ///     The field map
        /// </summary>
        private readonly IList<EntityEntry> _entities = new List<EntityEntry>( );

        /// <summary>
        ///     The IsOfType upgrade Id.
        /// </summary>
        private readonly string IsOfType = "e1afc9e2-a526-4dc6-b90f-e2271e130f24";

        /// <summary>
        ///     Gets the relationships.
        /// </summary>
        private IList<RelationshipEntry> _relationships;

        /// <summary>
        ///     Gets the fields.
        /// </summary>
        private IDictionary<string, IList<DataEntry>> _fieldData;


        /// <summary>
        ///     Deserializes the entities.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeEntities( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.EntityConstants.Entities );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.EntityConstants.Entities, ( reader, stack ) =>
            {
                if ( reader.Name == XmlConstants.EntityConstants.Group )
                {
                    DeserializeEntityGroup( reader, stack );
                }
                else
                {
                    string entityId = DeserializeEntity( reader, stack );
                    _metadata.RootEntities.Add( Guid.Parse( entityId ) );
                }
            } );
        }

        /// <summary>
        ///     Deserializes the entities.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeEntityGroup( XmlReader xmlReader, Stack<string> xmlStack )
        {
            xmlStack.Push( XmlConstants.EntityConstants.Group );

            AdvanceReader( xmlReader, xmlStack, null, XmlConstants.EntityConstants.Group, ( reader, stack ) =>
            {
                string entityId = DeserializeEntity( reader, stack );
                _metadata.RootEntities.Add( Guid.Parse( entityId  ) );
            } );
        }

        private void PushMember( string entityId, string memberId, string memberType, string data )
        {
            _members.Add( new Member( entityId, memberId, memberType, data ) );
        }

        /// <summary>
        ///     Deserializes the entity.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        /// <returns>The GUID of the entity.</returns>
        private string DeserializeEntity( XmlReader xmlReader, Stack<string> xmlStack )
        {
            string elementName = xmlReader.Name;

            xmlStack.Push( elementName );

            Guid entityId = Guid.Empty;

            // Read Entity ID
            string entityIdAttribute = xmlReader.GetAttribute( XmlConstants.Id );

            if ( entityIdAttribute != null )
            {
                if ( entityIdAttribute == string.Empty || !Guid.TryParse( entityIdAttribute, out entityId ) )
                {
                    ThrowXmlException( "Entity does not contain a valid id attribute.", xmlReader );
                }
            }

            EntityEntry entry = new EntityEntry
            {
                EntityId = entityId
            };

            _entities.Add( entry );


            // Store the entity type as a member
            string typeData = elementName;
            if ( elementName == XmlConstants.EntityConstants.Entity )
            {
                typeData = xmlReader.GetAttribute( XmlConstants.EntityConstants.TypeId );
            }
            if ( !string.IsNullOrEmpty( typeData ) )
            {
                PushMember( entityIdAttribute, IsOfType, XmlConstants.EntityConstants.Type, typeData );
            }

            AdvanceReader( xmlReader, xmlStack, null, elementName, ( reader, stack ) => DeserializeEntityMembers( entityIdAttribute, reader, stack ) );

            return entityIdAttribute; // ID returned as string to prevent unnecessary string operations.
        }

        /// <summary>
        ///     Deserializes the entity fields.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="xmlReader">The XML text reader.</param>
        /// <param name="xmlStack">The XML stack.</param>
        private void DeserializeEntityMembers( string entityId, XmlReader xmlReader, Stack<string> xmlStack )
        {
            string elementName = xmlReader.Name;

            string memberIdAttribute = xmlReader.GetAttribute( XmlConstants.Id );

            string aliasMarkerIdAttribute = xmlReader.GetAttribute( XmlConstants.FieldConstants.AliasIdMarker );

            string memberId;
            string memberType;

            // Determine memberId/memberType
            if ( memberIdAttribute != null )
            {
                Guid fieldId = Guid.Empty;

                if ( string.IsNullOrEmpty( memberIdAttribute ) || !Guid.TryParse( memberIdAttribute, out fieldId ) )
                {
                    ThrowXmlException( "Field does not contain a valid id attribute.", xmlReader );
                }

                memberId = memberIdAttribute;   // element is a field type, or 'relationship' or 'reverseRelationship'
                memberType = elementName;
            }
            else
            {
                memberId = elementName; // element name is an alias
                memberType = null;
            }

            // Alias marker
            int aliasMarkerId;
            if ( !string.IsNullOrEmpty( aliasMarkerIdAttribute ) )
                int.TryParse( aliasMarkerIdAttribute, out aliasMarkerId );

            // Read content
            xmlStack.Push( elementName );

            bool nestedContent = false;
            string data = string.Empty;

            while ( xmlReader.Read( ) )
            {
                if ( xmlReader.NodeType == XmlNodeType.Text )
                {
                    // Inline field or relationship value
                    data = xmlReader.Value;
                }
                else if ( xmlReader.NodeType == XmlNodeType.Element )
                {
                    // Read nested element
                    string childId = DeserializeEntity( xmlReader, xmlStack );

                    // Register relationship to parent
                    PushMember( entityId, memberId, memberType, childId );
                    nestedContent = true;
                    data = null;
                }
                else if ( xmlReader.NodeType == XmlNodeType.EndElement )
                {
                    PopStack( xmlStack, elementName );
                    break;
                }
            }

            // Field or inline relationship
            if ( !nestedContent )
            {
                PushMember( entityId, memberId, memberType, data );
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

            PushMember( from, type, XmlConstants.RelationshipConstants.Rel, to );
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
    }
}
