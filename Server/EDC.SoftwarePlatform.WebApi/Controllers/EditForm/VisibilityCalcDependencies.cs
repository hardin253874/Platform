// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
    /// <summary>
    /// Visibility calc dependencies class.
    /// </summary>
    [DataContract]
    public class VisibilityCalcDependencies
    {
        /// <summary>
        ///     Gets or sets the fields.
        /// </summary>
        /// <value>
        ///     The fields.
        /// </value>
        [DataMember(Name = "fields", EmitDefaultValue = false, IsRequired = true)]
        public HashSet<long> Fields { get; set; }

        /// <summary>
        ///     Gets or sets the relationships.
        /// </summary>
        /// <value>
        ///     The relationships.
        /// </value>
        [DataMember(Name = "relationships", EmitDefaultValue = false, IsRequired = false)]
        public HashSet<long> Relationships { get; set; }


        /// <summary>
        ///     Should the relationships be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeRelationships()
        {
            return Relationships != null;
        }

        /// <summary>
        ///     Should the fields be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeFields()
        {
            return Fields != null;
        }
    }
}