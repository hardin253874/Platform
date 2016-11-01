// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.Annotations;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
    [DataContract]
    public class FormDataResponse
    {
        /// <summary>
        ///     Gets or sets the entity.
        /// </summary>
        /// <value>
        ///     The entity.
        /// </value>
        [DataMember(Name = "formDataEntity", EmitDefaultValue = false, IsRequired = true)]
        public JsonQueryResult FormDataEntity { get; set; }

        /// <summary>
        ///     Gets or sets any initially hidden controls.
        /// </summary>
        /// <value>
        ///     The hidden controls.
        /// </value>
        [DataMember(Name = "initiallyHiddenControls", EmitDefaultValue = false, IsRequired = false)]
        public ISet<long> InitiallyHiddenControls { get; set; }        

        /// <summary>
        ///     Should the initially hidden controls be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeInitiallyHiddenControls()
        {
            return InitiallyHiddenControls != null;
        }

        /// <summary>
        ///     Should the entity be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeFormDataEntity()
        {
            return FormDataEntity != null;
        }        
    }
}