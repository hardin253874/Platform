// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	[DataContract]
	public class AnalyserColumnCondition
	{
		/// <summary>
		///     Gets or sets the expression unique identifier.
		/// </summary>
		/// <value>The expression unique identifier.</value>
		[DataMember( Name = "expid", EmitDefaultValue = false, IsRequired = true )]
		public string ExpressionId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the expression identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
	    private bool ShouldSerializeExpressionId( )
	    {
			return ExpressionId != null;
	    }

		/// <summary>
		///     Gets or sets the data type for the column.
		/// </summary>
		/// <value>The data type.</value>
		[DataMember( Name = "type", EmitDefaultValue = false, IsRequired = false )]
		public string Type
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
	    private bool ShouldSerializeType( )
	    {
			return Type != null;
	    }

		/// <summary>
		///     Gets or sets the default operator.
		/// </summary>
		/// <value>The default operator.</value>
		[DataMember( Name = "oper", EmitDefaultValue = false, IsRequired = false )]
		public string Operator
		{
			get;
			set;
		}

		/// <summary>
		///		Should the operator be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
	    private bool ShouldSerializeOperator( )
	    {
			return Operator != null;
	    }

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[DataMember( Name = "value", EmitDefaultValue = false, IsRequired = false )]
		public string Value
		{
			get;
			set;
		}

		/// <summary>
		///		Should the value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
	    private bool ShouldSerializeValue( )
	    {
			return Value != null;
	    }

		/// <summary>
		///     Gets or sets the entity identifiers.
		/// </summary>
		/// <value>The entity identifiers.</value>
		[DataMember( Name = "values", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, string> EntityIdentifiers
		{
			get;
			set;
		}

		/// <summary>
		///		Should the entity identifiers be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
	    private bool ShouldSerializeEntityIdentifiers( )
	    {
			return EntityIdentifiers != null;
	    }
	}
}