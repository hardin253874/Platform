// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Expression Field Query class.
	/// </summary>
	[DataContract]
	public class JsonExprFieldQuery
	{
		/// <summary>
		///     Gets or sets the field.
		/// </summary>
		/// <value>
		///     The field.
		/// </value>
		[DataMember( Name = "field" )]
		public string Field
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the on.
		/// </summary>
		/// <value>
		///     The on.
		/// </value>
		[DataMember( Name = "on" )]
		public string On
		{
			get;
			set;
		}
	}
}