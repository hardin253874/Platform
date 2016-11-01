// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Condition class.
	/// </summary>
	[DataContract]
	public class JsonCondition
	{
		/// <summary>
		///     Gets or sets the expression.
		/// </summary>
		/// <value>
		///     The expression.
		/// </value>
		[DataMember( Name = "expr" )]
		public JsonExprFieldQuery Expression
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the operation.
		/// </summary>
		/// <value>
		///     The operation.
		/// </value>
		[DataMember( Name = "oper" )]
		public string Operation
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		[DataMember( Name = "val" )]
		public string Value
		{
			get;
			set;
		}
	}
}