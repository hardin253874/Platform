// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Metadata.Query.Structured;
using System;
using System.Collections.Generic;
using ReadiNow.Reporting.Request;

namespace ReadiNow.Reporting
{
    /// <summary>
    /// Cache key for report runs.
    /// </summary>
    [DataContract]
    public class ReportResultCacheKey : IEquatable<ReportResultCacheKey>
    {
        private int _hashCode;

        internal ReportResultCacheKey( ReportSettings reportSettings, IQueryRunnerCacheKey reportQueryKey, IQueryRunnerCacheKey rollupQueryKey )
        {
            if ( reportSettings == null )
                throw new ArgumentNullException( "reportSettings" );
            if ( reportQueryKey == null )
                throw new ArgumentNullException( "reportQueryKey" );
            // rollupQueryKey may be null

            ReportQueryKey = reportQueryKey;
            RollupQueryKey = rollupQueryKey;
            Bools = PackBools( reportSettings );
            ColumnCount = reportSettings.ColumnCount;

            // The following reportSettings do not need to be covered in this key:
            // - SupportPaging, PageSize, InitialRow (denied)
            // - ReportParameters, Timezone, ReportOnType, QuickSearch, FilteredEntityIdentifiers, RelatedEntityFilters
            // - UseStructuredQueryCache, RefreshCachedResult, RefreshCachedSql, RefreshCachedStructuredQuery

            _hashCode = GenerateHashCode();
        }

        /// <summary>
        /// Parameter-less constructor used for serialization.
        /// </summary>
        private ReportResultCacheKey( )
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
        /// CachingQuerySqlBuilderKey.
        /// </summary>
        [DataMember( Order = 1 )]
        public IQueryRunnerCacheKey ReportQueryKey { get; private set; }

        /// <summary>
        /// CachingQuerySqlBuilderKey.
        /// </summary>
        [DataMember( Order = 2 )]
        public IQueryRunnerCacheKey RollupQueryKey { get; private set; }

        /// <summary>
        /// Bools.
        /// </summary>
        [DataMember( Order = 3 )]
        public int Bools { get; private set; }

        /// <summary>
        /// ColumnCount.
        /// </summary>
        [DataMember( Order = 4 )]
        public int? ColumnCount { get; private set; }

        /// <summary>
        /// Store various boolean settings into a bit field for convenient comparison and storage.
        /// Note: these only have meaning within this class.
        /// </summary>
        private static int PackBools( ReportSettings settings )
        {
            if ( settings == null )
                throw new ArgumentNullException( "settings" );

            int flags = 0;
            if ( settings.RequireBasicMetadata )
                flags += 1;
            if ( settings.RequireColumnBasicMetadata )
                flags += 2;
            if ( settings.RequireFullMetadata )
                flags += 4;
            if ( settings.RequireSchemaMetadata )
                flags += 8;

            return flags;
        }

        #region IEquatable

        public bool Equals( ReportResultCacheKey other )
        {
            if ( ReferenceEquals( null, other ) ) return false;
            if ( ReferenceEquals( this, other ) ) return true;
            return Equals(ReportQueryKey, other.ReportQueryKey) && Equals(RollupQueryKey, other.RollupQueryKey) && Bools == other.Bools && Equals(ColumnCount, other.ColumnCount);
        }

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) ) return false;
            if ( ReferenceEquals( this, obj ) ) return true;
            if ( obj.GetType( ) != this.GetType( ) ) return false;
            return Equals( ( ReportResultCacheKey ) obj );
        }

        public override int GetHashCode( )
        {           
            return _hashCode;
        }

        private int GenerateHashCode()
        {
	        unchecked
	        {
		        int hashCode = 17;

		        hashCode = hashCode * 92821 + EqualityComparer<IQueryRunnerCacheKey>.Default.GetHashCode( ReportQueryKey );

		        hashCode = hashCode * 92821 + EqualityComparer<IQueryRunnerCacheKey>.Default.GetHashCode( RollupQueryKey );

		        hashCode = hashCode * 92821 + Bools.GetHashCode( );

		        hashCode = hashCode * 92821 + EqualityComparer<int?>.Default.GetHashCode( ColumnCount );

		        return hashCode;
	        }
        }

        public static bool operator ==( ReportResultCacheKey left, ReportResultCacheKey right )
        {
            return EqualityComparer<ReportResultCacheKey>.Default.Equals( left, right );
        }

        public static bool operator !=( ReportResultCacheKey left, ReportResultCacheKey right )
        {
            return !EqualityComparer<ReportResultCacheKey>.Default.Equals( left, right );
        }
        #endregion

    }
}
