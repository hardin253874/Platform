// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEditor
{
	/// <summary>
	///     Evaluation Request class
	/// </summary>
	[DataContract]
	public class EvalRequest
	{
		/// <summary>
		///     Gets or sets the expression.
		/// </summary>
		/// <value>
		///     The expression.
		/// </value>
		[DataMember( Name = "expr", EmitDefaultValue = true, IsRequired = true )]
		public string Expression
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		[DataMember( Name = "context", EmitDefaultValue = true, IsRequired = false )]
		public string Context
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the host.
		/// </summary>
		/// <value>
		///     The host.
		/// </value>
		[DataMember( Name = "host", EmitDefaultValue = true, IsRequired = false )]
		public string Host
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the parameters.
		/// </summary>
		/// <value>
		///     The parameters.
		/// </value>
		[DataMember( Name = "params", EmitDefaultValue = false, IsRequired = false )]
		public List<ExpressionParameter> Parameters
		{
			get;
			set;
        }

        /// <summary>
        ///     Optional. The expected result type.
        /// </summary>
        [DataMember(Name = "expectedResultType", EmitDefaultValue = false, IsRequired = false)]
        public ExpressionType ExpectedResultType
        {
            get;
            set;
        }

        /// <summary>
        ///		Should the parameters be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
		private bool ShouldSerializeParameters( )
	    {
		    return Parameters != null;
	    }
	}
}