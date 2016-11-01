// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.Annotations;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEngine
{
    /// <summary>
    ///     Evaluation Result class.
    /// </summary>
    [DataContract]
    public class CalcEngineEvalRequest
    {
        /// <summary>
        ///     Gets or sets the context entity.
        /// </summary>
        /// <value>
        ///     The context entity.
        /// </value>
        [DataMember(Name = "contextEntity", EmitDefaultValue = true, IsRequired = true)]
        public EntityNugget ContextEntity { get; set; }


        /// <summary>
        ///     Gets or sets the expressions.
        /// </summary>
        /// <value>
        ///     The expressions.
        /// </value>
        [DataMember(Name = "expressions", EmitDefaultValue = true, IsRequired = true)]
        public Dictionary<string, CalcEngineExpression> Expressions { get; set; }        

        /// <summary>
        ///     Should the context entity be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeContextEntity()
        {
            return ContextEntity != null;
        }

        /// <summary>
        ///     Should the expressions be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeExpressions()
        {
            return Expressions != null;
        }
    }
}