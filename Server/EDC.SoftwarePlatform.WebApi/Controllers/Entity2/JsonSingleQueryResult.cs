// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Result of a single query within a batch
	/// </summary>
	[DataContract]
	public class JsonSingleQueryResult
	{
        /// <summary>
        ///     Gets or sets the HTTP response code.
        /// </summary>
        /// <value>
        ///     The code enum.
        /// </value>
        [IgnoreDataMember]
        public HttpStatusCode Code
        {
            get;
            set;
        }

        /// <summary>
		///     Gets the HTTP response code.
		/// </summary>
		/// <value>
		///     The code number.
		/// </value>
		[DataMember( Name = "code" )]
		public int CodeNumber
		{
            get { return ( int ) Code; }
            set { Code = ( HttpStatusCode ) value; }
		}

		/// <summary>
		///     Gets or sets the i ds.
		/// </summary>
		/// <value>
		///     The i ds.
		/// </value>
		[DataMember( Name = "ids" )]
		public List<long> Ids
		{
			get;
			set;
		}

		/// <summary>
		///     If set then the query has a name
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember( Name = "name", EmitDefaultValue = true )]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is single value.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is single value; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "single", EmitDefaultValue = false )]
		public bool IsSingleValue
		{
			get;
			set;
		}

		/// <summary>
		///		Should the is single value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeIsSingleValue( )
	    {
			return IsSingleValue;
	    }

		/// <summary>
		///     Returns the hint that was passed in.
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
		///		Should the hint be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeHint( )
	    {
			return Hint != null;
	    }

		/// <summary>
		///     If true, indicates that this response came from cache.
		/// </summary>
		/// <value>
		///     <c>true</c> if cached; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "cached", EmitDefaultValue = false )]
		public bool Cached
		{
			get;
			set;
		}

		/// <summary>
		///		Should the cached value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeCached( )
	    {
			return Cached;
	    }
	}
}