// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Workflow
{
	[DataContract]
	public class ParameterValue
	{
		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember( Name = "name" )]
		public string Name
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
		[DataMember( Name = "value" )]
		public string Value
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the type.
		/// </summary>
		/// <value>
		///     The name of the type.
		/// </value>
		[DataMember( Name = "typeName" )]
		public string TypeName
		{
			get;
			set;
		}
	}
}