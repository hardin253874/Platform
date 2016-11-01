// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Structured Query class.
	/// </summary>
	[DataContract]
	public class JsonStructuredQuery
	{
		/// <summary>
		///     Gets or sets the root.
		/// </summary>
		/// <value>
		///     The root.
		/// </value>
		[DataMember( Name = "root" )]
		public JsonEntityInQuery Root
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the selects.
		/// </summary>
		/// <value>
		///     The selects.
		/// </value>
		[DataMember( Name = "selects" )]
		public List<JsonSelectInQuery> Selects
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the conds.
		/// </summary>
		/// <value>
		///     The conds.
		/// </value>
		[DataMember( Name = "conds" )]
		public List<JsonCondition> Conds
		{
			get;
			set;
		}
	}
}