// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.Annotations;
using EDC.SoftwarePlatform.WebApi.Controllers.CalcEditor;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEngine
{
    [DataContract]
    public class CalcEngineExpression
    {
        /// <summary>
        ///     Gets or sets the expression.
        /// </summary>
        /// <value>
        ///     The expression.
        /// </value>
        [DataMember(Name = "expr", EmitDefaultValue = true, IsRequired = true)]
        public string Expression { get; set; }

        /// <summary>
        ///     Optional. The expected result type.
        /// </summary>
        [DataMember(Name = "expectedResultType", EmitDefaultValue = false, IsRequired = false)]
        public ExpressionType ExpectedResultType { get; set; }

        /// <summary>
        ///     Should the style be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeExpression()
        {
            return Expression != null;
        }

        /// <summary>
        ///     Should the ExpectedResultType be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeExpectedResultType()
        {
            return ExpectedResultType != null;
        }
    }
}