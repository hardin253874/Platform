// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;
using EDC.ReadiNow.EntityRequests;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Individual requests that were made in a batch. Base unit of combination / response, etc.
	/// </summary>
	[DataContract]
	public class JsonQuerySingleRequest
	{
		/// <summary>
		///     IDs or aliases of one or more entities being requested. Optional.
		/// </summary>
		/// <value>
		///     The ids.
		/// </value>
		[DataMember( Name = "ids", EmitDefaultValue = false )]
		public long[ ] Ids
		{
			get;
			set;
		}

		/// <summary>
		///		Should the ids value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIds( )
	    {
			return Ids != null;
	    }

		/// <summary>
		///     Gets or sets the string ids.
		/// </summary>
		/// <value>
		///     The string ids.
		/// </value>
		[DataMember( Name = "aliases", EmitDefaultValue = false )]
		public string[ ] Aliases
		{
			get;
			set;
		}

		/// <summary>
		///		Should the aliases value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAliases( )
	    {
			return Aliases != null;
	    }

		/// <summary>
		///     Zero based index of the query string that this query represents.
		/// </summary>
		/// <value>
		///     The index of the query.
		/// </value>
		[DataMember( Name = "rq", EmitDefaultValue = false )]
		public int QueryIndex
		{
			get;
			set;
		}

		/// <summary>
		///		Should the index of the query value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeQueryIndex( )
	    {
			return QueryIndex != 0;
	    }

		/// <summary>
		///     Zero based index of the query string that this query represents.
		/// </summary>
		/// <value>
		///     The type of the query.
		/// </value>
		[DataMember( Name = "get", EmitDefaultValue = false )]
		public QueryType QueryType
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type of the query value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeQueryType( )
	    {
			return QueryType != QueryType.Basic;
	    }

		/// <summary>
		///     A hint about whats in the request, for debugging purposes.
		/// </summary>
		/// <value>
		///     The hint.
		/// </value>
		[DataMember( Name = "hint", EmitDefaultValue = false )]
		public string Hint
		{
			get;
			set;
		}

		/// <summary>
		///		Should the hint value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeHint( )
	    {
			return Hint != null;
	    }

        /// <summary>
        ///     A calculation string that may be used with Instances or Exact Instances.
        ///     It must evaluate to a bool. (or be implicitly castable to a bool).
        ///     E.g. "[Name]='Peter'"
        /// </summary>
        /// <value>
        ///     The filter calculation.
        /// </value>
        [DataMember(Name = "filter", EmitDefaultValue = false)]
        public string Filter
        {
            get;
            set;
        }

		/// <summary>
		///		Should the filter value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeFilter( )
	    {
			return Filter != null;
	    }

        /// <summary>
        ///     Isolated mode.
        ///     Certain system types can be tagged as metadata (a property on the type).
        ///     By default, for those types only, any entity info requests will get cached and shared between users in the same UserRuleSet. (And the caching is also much more aggressive).
        ///     That means that the security of one may apply to another.
        ///     Use the isolated flag to ignore this behavior, for example if you want perform an entity info request on a system type, but expect only the current user's security rules to apply.
        /// </summary>
        [DataMember( Name = "isolated", EmitDefaultValue = false )]
        public bool Isolated
        {
            get;
            set;
        }

		/// <summary>
		///		Should the isolated value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeIsolated( )
	    {
			return Isolated;
	    }
	}
}