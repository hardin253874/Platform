// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using EDC.ReadiNow.Annotations;
using EDC.ReadiNow.Metadata;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
    /// <summary>
    ///     Related entity filter details
    /// </summary>
    [DataContract]
    public class RelatedEntityFilter
    {
        /// <summary>
        ///     The related entity ids.
        /// </summary>
        /// <value>
        ///     The related entity ids.
        /// </value>
        [DataMember(Name = "eids", EmitDefaultValue = false, IsRequired = false)]
        public List<long> RelatedEntityIds { get; set; }

		/// <summary>
		///		Should the related entity ids be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRelatedEntityIds( )
	    {
			return RelatedEntityIds != null;
	    }


        /// <summary>
        ///     The relationship id.
        /// </summary>
        /// <value>
        ///     The relationship id.
        /// </value>
        [DataMember(Name = "relid", EmitDefaultValue = false, IsRequired = false)]
        public long RelationshipId { get; set; }

		/// <summary>
		///		Should the relationship identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRelationshipId( )
	    {
			return RelationshipId != 0;
	    }


        /// <summary>
        ///     The relationship direction.
        /// </summary>
        /// <value>
        ///     The relationship direction.
        /// </value>
        [DataMember(Name = "dir", EmitDefaultValue = false, IsRequired = false)]
        public RelationshipDirection RelationshipDirection { get; set; }

		/// <summary>
		///		Should the relationship direction be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRelationshipDirection( )
	    {
			return RelationshipDirection != RelationshipDirection.Forward;
	    }
    }
}