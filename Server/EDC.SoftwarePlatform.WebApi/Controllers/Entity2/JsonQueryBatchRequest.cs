// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Top level request object.
	/// </summary>
	[DataContract]
	public class JsonQueryBatchRequest
	{
		/// <summary>
		///     Array of entity request query strings.
		///     Defined separately to requests so that they can be shared/reused.
		/// </summary>
		[DataMember( Name = "queries", EmitDefaultValue = false )]
		public string[ ] Queries
		{
			get;
			set;
		}

		/// <summary>
		///		Should the queries be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeQueries( )
	    {
			return Queries != null;
	    }

		/// <summary>
		///     Individual requests that were made in the batch.
		/// </summary>
		[DataMember( Name = "requests", EmitDefaultValue = false )]
		public JsonQuerySingleRequest[ ] Requests
		{
			get;
			set;
		}

		/// <summary>
		///		Should the requests be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRequests( )
	    {
			return Requests != null;
	    }
	}
}