// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
    /// <summary>
    /// FOrm visibility calc dependencies response class.
    /// </summary>
    [DataContract]
    public class FormVisibilityCalcDependenciesResponse
    {
        /// <summary>
        ///     Gets or sets the per control visibility calculation dependencies.
        /// </summary>
        /// <value>
        ///     The per control visibility calculation dependencies.
        /// </value>
        [DataMember(Name = "visibilityCalcDependencies", EmitDefaultValue = false, IsRequired = false)]
        public IDictionary<long, VisibilityCalcDependencies> VisibilityCalcDependencies { get; set; }

        /// <summary>
        ///     Should the control visibility dependencies be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeVisibilityCalcDependencies()
        {
            return VisibilityCalcDependencies != null;
        }
    }
}