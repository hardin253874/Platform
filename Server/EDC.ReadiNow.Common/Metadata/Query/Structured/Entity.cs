// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using EDC.Core;
using EDC.ReadiNow.Annotations;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Something from which rows can be selected, fields can be identified, and relations can be followed.
    /// For example, a resource.
    /// </summary>
    // type: reportNode
    [DataContract(Namespace = Constants.DataContractNamespace)]    
    [KnownType(typeof(ResourceEntity)), XmlInclude(typeof(ResourceEntity)), ProtoInclude(100,typeof(ResourceEntity))]
    [KnownType(typeof(AggregateEntity)), XmlInclude(typeof(AggregateEntity)), ProtoInclude(101, typeof(AggregateEntity))]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public abstract class Entity : IDeserializationCallback
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        protected Entity()
        {
            // See also: OnDeserialization
            RelatedEntities = new List<Entity>();
            NodeId = Guid.NewGuid();
        }


        /// <summary>
        /// An identifier used within the context of the query to identify this entity within the relationship tree.
        /// </summary>
        // not in entity model
        [DataMember(Order = 1)]
        [XmlAttribute( "id" )]
        public Guid NodeId
        {
            get { return StructuredQueryHashingContext.GetGuid( _nodeId ); }
            set { _nodeId = value; }
        }
        [XmlIgnore]
        private Guid _nodeId;


        /// <summary>
        /// List of relationships, groupings, dependencies, etc, that are joined from this entity.
        /// </summary>
        // rel: relatedReportNodes
        [DataMember(Order = 2)]
        [XmlArray("RelatedEntities")]
        [XmlArrayItem("Entity")]
        public List<Entity> RelatedEntities { get; set; }


        /// <summary>
        /// List of boolean expressions that are directly applied to this resource.
        /// Note: conditions are combined using 'and'. (This is slightly redundant, given the 'and' expression, but done for convenient)
        /// </summary>
        // not added in entity model yet.. do we want it? if do, add the property to constructor
        [DataMember]
        [XmlArray("Conditions")]
        [XmlArrayItem("Condition")]
        public List<ScalarExpression> Conditions { get; set; }

        [DataMember(Order = 3)]
        [XmlIgnore]
        public long EntityId;

        #region XML Formatters
        /// <summary>
        /// Returns true if the related entities should be serialized.
        /// This is to ensure no element appears if empty.        
        /// </summary>
        /// <returns>True if the related entities should be serialized</returns>
        /// <remarks>The method name must be ShouldSerialize followed by the 
        /// PropertyName, in this case RelatedEntities.</remarks>
		public bool ShouldSerializeRelatedEntities()
        {
            return RelatedEntities != null &&
                   RelatedEntities.Count > 0;
        }

        /// <summary>
        /// Returns true if the conditions should be serialized.
        /// This is to ensure no element appears if empty.        
        /// </summary>
        /// <returns>True if the related entities should be serialized</returns>
        /// <remarks>The method name must be ShouldSerialize followed by the 
        /// PropertyName, in this case Conditions.</remarks>
		public bool ShouldSerializeConditions()
        {
            return Conditions != null &&
                   Conditions.Count > 0;
        }

        /// <summary>
        /// Ensure lists are created after deserialization.
        /// </summary>
        /// <param name="sender"></param>
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (RelatedEntities == null)
                RelatedEntities = new List<Entity>();

            if (Conditions == null)
                Conditions = new List<ScalarExpression>();
        }
        #endregion
    }
}
