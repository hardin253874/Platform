// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Entity In Query class.
	/// </summary>
	[DataContract]
	public class JsonEntityInQuery
	{
		/// <summary>
		///     Gets or sets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		[DataMember( Name = "id" )]
		public string Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets as.
		/// </summary>
		/// <value>
		///     As.
		/// </value>
		[DataMember( Name = "as" )]
		public string As
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the related.
		/// </summary>
		/// <value>
		///     The related.
		/// </value>
		[DataMember( Name = "related" )]
		public List<JsonRelatedEntityInQuery> Related
		{
			get;
			set;
		}
	}
}