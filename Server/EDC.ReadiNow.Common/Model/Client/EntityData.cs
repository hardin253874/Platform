// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;
using EDC.ReadiNow.Model;
using TypedValue = EDC.ReadiNow.Metadata.TypedValue;
using System.Xml;
using System.IO;
using EDC.Core;
using System.Diagnostics;

namespace EDC.ReadiNow.Model.Client
{
    /// <summary>
    /// Contains information about an entity.
    /// </summary>
    /// <remarks>
    /// This may not be complete information. More likely only the requested information (for download) or
    /// the updated information (for upload).
    /// </remarks>
    [DataContract(Namespace = Constants.DataContractNamespace, IsReference = true)]
    [DebuggerDisplay("EntityData {Id}")]
    public partial class EntityData
    {
        /// <summary>
        /// The ID of the entity.
        /// </summary>
        [DataMember]
        public EntityRef Id { get; set; }

        /// <summary>
        /// The primary types of the entity.
        /// </summary>
        [DataMember]
        public List<EntityRef> TypeIds { get; set; }

        /// <summary>
        /// Collection of fields and their data.
        /// </summary>
        [DataMember]
        public List<FieldData> Fields { get; set; }

        /// <summary>
        /// Relationships to this entity. Note: one entry per 'type' of relationship.
        /// Details of instances are kept within the RelationshipData object.
        /// </summary>
        [DataMember]
        public List<RelationshipData> Relationships { get; set; }

        /// <summary>
        /// State of the data in this entity.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DataState DataState { get; set; }

		/// <summary>
		///		Should the state of the data be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDataState( )
	    {
		    return DataState != DataState.Unchanged;
	    }

	    /// <summary>
        /// Serializes the data contract.
        /// </summary>
        public string TestSerialize()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (TextWriter tw = new StringWriter())
            using (XmlWriter xw = XmlTextWriter.Create(tw, settings))
            {
                DataContractSerializer dc = new DataContractSerializer(typeof(EntityData));
                dc.WriteObject(xw, this);
                xw.Flush();
                tw.Flush();
                return tw.ToString();
            }
        }

        /// <summary>
        /// Deserializes a datacontract from XML.
        /// </summary>
        /// <param name="xml">XML serialized by TestSerialize().</param>
        public static EntityData TestDeserialize(string xml)
        {
            using (StringReader sr = new StringReader(xml))
            using (XmlReader xr = new XmlTextReader(sr))
            {
                DataContractSerializer dc = new DataContractSerializer(typeof(EntityData));
                object obj = dc.ReadObject(xr);
                EntityData entityData = (EntityData)obj;
                return entityData;
            }
        }

        /// <summary>
        /// Locates the requested relationship, or returns null if not found.
        /// </summary>
        /// <param name="relationshipTypeId">The relationship type id.</param>
        /// <returns>RelationshipData.</returns>
        public RelationshipData GetRelationship(EntityRef relationshipTypeId)
        {
            return Relationships.SingleOrDefault(r => r != null && r.RelationshipTypeId != null && r.RelationshipTypeId.Id == relationshipTypeId.Id);
        }

		/// <summary>
		/// Locates the requested relationship, or returns null if not found.
		/// </summary>
		/// <param name="relationshipTypeId">The relationship type id.</param>
		/// <param name="direction">The direction.</param>
		/// <returns>
		/// RelationshipData.
		/// </returns>
        public RelationshipData GetRelationship(EntityRef relationshipTypeId, Direction direction)
        {
            Direction actualDirection = Entity.GetDirection( relationshipTypeId, direction == Direction.Reverse );
            return GetRelationship( relationshipTypeId.Id, actualDirection );
        }

        /// <summary>
        /// Locates the requested relationship, or returns null if not found.
        /// </summary>
        /// <param name="relationshipTypeId">The relationship type id.</param>
        /// <param name="actualDirection">The direction that the relationship is actually being followed. (As opposed to the direction requested, which may subsequently be reversed if a reverse alias was used).</param>
        /// <returns>
        /// RelationshipData.
        /// </returns>
        public RelationshipData GetRelationship( long relationshipTypeId, Direction actualDirection )
        {
            bool isRev = actualDirection == Direction.Reverse;
            return Relationships.SingleOrDefault( r => r?.RelationshipTypeId != null && r.RelationshipTypeId.Id == relationshipTypeId && r.IsReverseActual == isRev );
        }

        /// <summary>
        /// Locates the requested field, or returns null if not found.
        /// </summary>
        /// <param name="fieldTypeId">The field type id.</param>
        /// <returns>FieldData.</returns>
        public FieldData GetField(EntityRef fieldTypeId)
        {
            return GetField( fieldTypeId.Id );
        }

        /// <summary>
        /// Locates the requested field, or returns null if not found.
        /// </summary>
        /// <param name="fieldTypeId">The field type id.</param>
        /// <returns>FieldData.</returns>
        public FieldData GetField( long fieldTypeId )
        {
            return Fields.Find( f => f?.FieldId != null && f.FieldId.Id == fieldTypeId );
        }

    }

    /// <summary>
    /// Data for an individual field.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [DebuggerDisplay("FieldData {FieldId}")]
    public class FieldData
    {
        [DataMember]
        public EntityRef FieldId { get; set; }

        [DataMember]
        public TypedValue Value { get; set; }  
    }


    /// <summary>
    /// Data about multiple relationships instances of a given type of relationship.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [DebuggerDisplay("RelationshipData {RelationshipTypeId} Count={Instances == null ? -1 : Instances.Count}")]
    public class RelationshipData
    {
        /// <summary>
        /// The type (definition) of the relationship. E.g. 'person works for company'.
        /// </summary>
        [DataMember]
        public IEntityRef RelationshipTypeId { get; set; }

        /// <summary>
        /// True if the relationship is being followed in the forward direction, otherwise false.
        /// </summary>
        /// <remarks>
        /// If the relationship is already a reverse alias, then this will reverse it again. Hopefully.
        /// </remarks>
        [DataMember(EmitDefaultValue = false)]
		public bool IsReverse
		{
			get; set;
		}

		/// <summary>
		///		Should the is reverse value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIsReverse( )
	    {
		    return IsReverse;
	    }

        /// <summary>
        /// True if the relationship is being followed in reverse. 
        /// </summary>
        /// <remarks>
        /// This indicates the true direction of the relationship, irrespective of whether the alias was a reverse alias.
        /// </remarks>
        public bool IsReverseActual { get; set; }

        /// <summary>
        /// The specific instances of the relationship.
        /// </summary>
        [DataMember]
        public List<RelationshipInstanceData> Instances { get; set; }

        /// <summary>
        /// Used when saving data. Set to true to indicate that all existing relationship entries should be purged prior to saving.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
		public bool RemoveExisting { get; set; }

		/// <summary>
		///		Should the remove existing value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRemoveExisting( )
	    {
		    return RemoveExisting;
	    }

        /// <summary>
        /// Used when saving data. Set to true to indicate that all existing related entities should be deleted.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool DeleteExisting { get; set; }

		/// <summary>
		///		Should the delete existing value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDeleteExisting( )
	    {
		    return DeleteExisting;
	    }

        /// <summary>
        /// Used when saving data. Set to true to indicate that all existing related entities should be deleted.
        /// </summary>
        public bool AutoCardinality { get; set; }

        /// <summary>
        /// True if the relationship is being followed in a direction, with a cardinality, that will point to one resource at most.
        /// </summary>
        /// <remarks>
        /// True if we are following a one-to-one or many-to-one forward, or a one-to-one or one-to-many in reverse (or with a reverse alias).
        /// </remarks>
        public bool IsLookup { get; set; }

        /// <summary>
        /// The list of related entities.
        /// </summary>
        public IEnumerable<EntityData> Entities
        {
            get
            {
                if (Instances == null)
                    return Enumerable.Empty<EntityData>();
                return Instances.Select(i => i.Entity);
            }
        }
    }


    /// <summary>
    /// Data about an instance of a relationship.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [DebuggerDisplay("RelationshipInstanceData {Entity.Id}")]
    public class RelationshipInstanceData
    {
        /// <summary>
        /// Entity being referenced by this relationship instance.
        /// </summary>
        [DataMember]
        public EntityData Entity { get; set; }

		/// <summary>
		/// Relationship instance entity data.
		/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public EntityData RelationshipInstanceEntity { get; set; }

	    [UsedImplicitly]
		private bool ShouldSerializeRelationshipInstanceEntity( )
	    {
		    return RelationshipInstanceEntity != null;
	    }

        /// <summary>
        /// State of the relationship. Not the entity at the other end of the relationship itself.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DataState DataState { get; set; }

		/// <summary>
		///		Should the state of the data be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDataState( )
	    {
		    return DataState != DataState.Unchanged;
	    }
    }
}
