// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Cache key for CachingBulkRequestRunner.
    /// </summary>
    [DataContract]
    class CachingBulkRequestRunnerKey : IEquatable<CachingBulkRequestRunnerKey>
    {
        private int _hashCode;

		/// <summary>
		/// Create a new <see cref="CachingBulkRequestRunnerKey" />.
		/// </summary>
		/// <param name="requestCacheKey">The request cache key.</param>
		/// <param name="entityIdList">The entity identifier list.</param>
		/// <param name="filter">The filter.</param>
        private CachingBulkRequestRunnerKey( string requestCacheKey, string entityIdList, string filter )
        {
            RequestCacheKey = requestCacheKey;
            EntityIdList = entityIdList;
            Filter = filter;
            _hashCode = GenerateHashCode();
        }

        /// <summary>
        /// Parameterless constructor used for serialization only.
        /// </summary>
        private CachingBulkRequestRunnerKey( )
        {
            // Do nothing
        }

		/// <summary>
		/// Called after deserialization.
		/// </summary>
		[OnDeserialized]
		private void OnAfterDeserialization( )
		{
			_hashCode = GenerateHashCode( );
		}

        /// <summary>
        /// The request cache key.
        /// </summary>
        /// 
        [DataMember( Order = 1 )]
        public string RequestCacheKey { get; private set; }

        /// <summary>
        /// The entity ID list.
        /// </summary>
        [DataMember( Order = 2 )]
        public string EntityIdList { get; private set; }

        /// <summary>
        /// The request filter.
        /// </summary>
        [DataMember( Order = 3 )]
        public string Filter { get; private set; }


        /// <summary>
        /// Build a key for caching request results.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static CachingBulkRequestRunnerKey Create( EntityRequest request )
        {
            string requestCacheKey = null;

            if ( !string.IsNullOrEmpty( request.RequestString ) )
            {
                requestCacheKey = request.RequestString;
            }
            else if ( request.Request != null )
            {
                requestCacheKey = request.Request.GetCacheKey( );
            }
            else
            {
                throw new ArgumentException( "Neither request string nor request object were provided." );
            }

            // Canonical form: converted to longs, sorted, comma-separated.
            string canonicalList =
                string.Join( ",", request.EntityIDsCanonical );

            return new CachingBulkRequestRunnerKey( requestCacheKey, canonicalList, request.Filter );
        }

        public bool Equals( CachingBulkRequestRunnerKey other )
        {
            if ( ReferenceEquals( null, other ) ) return false;
            if ( ReferenceEquals( this, other ) ) return true;
            return string.Equals( RequestCacheKey, other.RequestCacheKey ) && string.Equals( EntityIdList, other.EntityIdList ) && string.Equals( Filter, other.Filter );
        }

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) ) return false;
            if ( ReferenceEquals( this, obj ) ) return true;
            if ( obj.GetType( ) != this.GetType( ) ) return false;
            return Equals( ( CachingBulkRequestRunnerKey ) obj );
        }

        public override int GetHashCode( )
        {
            return _hashCode;
        }

        private int GenerateHashCode()
        {
			unchecked
			{
				int hash = 17;

				if ( RequestCacheKey != null )
				{
					hash = hash * 92821 + RequestCacheKey.GetHashCode( );
				}

				if ( EntityIdList != null )
				{
					hash = hash * 92821 + EntityIdList.GetHashCode( );
				}

				if ( Filter != null )
				{
					hash = hash * 92821 + Filter.GetHashCode( );
				}

				return hash;
			}
        }

        public static bool operator ==( CachingBulkRequestRunnerKey left, CachingBulkRequestRunnerKey right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( CachingBulkRequestRunnerKey left, CachingBulkRequestRunnerKey right )
        {
            return !Equals( left, right );
        }
    }
}
