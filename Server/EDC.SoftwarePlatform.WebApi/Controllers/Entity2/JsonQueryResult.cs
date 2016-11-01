// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Top-level result object for the entire batch.
	/// </summary>
	[DataContract]
	public class JsonQueryResult
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JsonQueryResult" /> class.
		/// </summary>
		public JsonQueryResult( )
		{
			Ids = new List<long>( );
			Results = new List<JsonSingleQueryResult>( );
			Entities = new Dictionary<long, Dictionary<string, object>>( );
			Members = new Dictionary<long, JsonMember>( );
		}

		/// <summary>
		///     Gets or sets the results.
		/// </summary>
		/// <value>
		///     The results.
		/// </value>
		[DataMember( Name = "results" )]
		public List<JsonSingleQueryResult> Results
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the ids.
		/// </summary>
		/// <value>
		///     The ids.
		/// </value>
		/// <remarks>Deprecated</remarks>
		[DataMember( Name = "ids" )]
		public List<long> Ids
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entities.
		/// </summary>
		/// <value>
		///     The entities.
		/// </value>
		[DataMember( Name = "entities" )]
		public Dictionary<long, Dictionary<string, object>> Entities
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the members.
		/// </summary>
		/// <value>
		///     The members.
		/// </value>
		[DataMember( Name = "members" )]
		public Dictionary<long, JsonMember> Members
		{
			get;
			set;
		}
	}
}