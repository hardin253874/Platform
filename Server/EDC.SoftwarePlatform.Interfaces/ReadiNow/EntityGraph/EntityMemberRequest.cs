// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.EntityRequests
{
    /// <summary>
    /// Represents a request for various fields and related resources.
    /// </summary>
    /// <remarks>
    /// When placing a generic request for an entity, use EntityMemberRequest to
    /// specify the fields that should be loaded, and the relationships that should be followed and loaded.
    /// Note that this structure is recursively defined, so that related entities can also have their desired
    /// fields and relationships specified.
    /// </remarks>
    public class EntityMemberRequest : IDeserializationCallback
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMemberRequest"/> class.
        /// </summary>
        public EntityMemberRequest()
        {
            // See also: OnDeserialization
            Fields = new List<IEntityRef>();
            Relationships = new List<RelationshipRequest>();
        }


        /// <summary>
        /// List of fields that should be loaded. Pass a reference to each field definition to load.
        /// </summary>
        [DataMember]
        public List<IEntityRef> Fields { get; set; }


        /// <summary>
        /// List of related entities that should be loaded. Each relationship request specifies
        /// the relationship, along with information to load about the related entities.
        /// </summary>
        [DataMember]
        public List<RelationshipRequest> Relationships { get; set; }


        /// <summary>
        /// If true, specifies that all available fields should be loaded.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllFields { get; set; }


        /// <summary>
        /// Ensure lists are created after deserialization.
        /// </summary>
        /// <param name="sender"></param>
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (Fields == null)
                Fields = new List<IEntityRef>();
            if (Relationships == null)
                Relationships = new List<RelationshipRequest>();
        }


        /// <summary>
        /// Server side use only - flags if this node has been checked for recursion.
        /// </summary>
        public bool CheckedForRecursion { get; set; }


        /// <summary>
        /// Set on the root node by the parser to facilitate cache lookups. Hmm.
        /// </summary>
        public string RequestString { get; set; }


        /// <summary>
        /// If set to true, instances generated at this node cannot be combined with other nodes. Internal use only.
        /// </summary>
        public bool DisallowInstanceReuse { get; set; }

        /// <summary>
        /// Holds a cache key for this entity request.
        /// </summary>
        /// <remarks>
        /// This is internal .. and temporary.
        /// </remarks>
        public string CacheKey { get; set; }
    }

}
