// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
    /// <summary>
    ///     Form data request Result class.
    /// </summary>
    [DataContract]
    public class FormDataRequest
    {
        /// <summary>
        ///     Gets or sets the entity.
        /// </summary>
        /// <value>
        ///     The entity.
        /// </value>
        [DataMember(Name = "entityId", EmitDefaultValue = false, IsRequired = true)]
        public string EntityId { get; set; }


        /// <summary>
        ///     Gets or sets the form id.
        /// </summary>
        /// <value>
        ///     The form id.
        /// </value>
        [DataMember(Name = "formId", EmitDefaultValue = false, IsRequired = true)]
        public string FormId { get; set; }

        /// <summary>
        ///     Gets or sets the query.
        /// </summary>
        /// <value>
        ///     The form data query.
        /// </value>
        [DataMember(Name = "query", EmitDefaultValue = false, IsRequired = true)]
        public string Query { get; set; }

        /// <summary>
        ///     Gets or sets the hint.
        /// </summary>
        /// <value>
        ///     The query hint.
        /// </value>
        [DataMember(Name = "hint", EmitDefaultValue = false, IsRequired = false)]
        public string Hint { get; set; }

        /// <summary>
        ///     Should the hint be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeHint()
        {
            return Hint != null;
        }

        /// <summary>
        ///     Should the entity id be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeEntityId()
        {
            return EntityId != null;
        }

        /// <summary>
        ///     Should the form id be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeFormId()
        {
            return FormId != null;
        }

        /// <summary>
        ///     Should the query be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeQuery()
        {
            return Query != null;
        }
    }
}