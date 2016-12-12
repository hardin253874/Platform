// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Security.AccessControl;
using ReadiNow.QueryEngine.CachingBuilder;
using System;
using System.Linq;

namespace ReadiNow.QueryEngine.CachingRunner
{
    /// <summary>
    /// Cache key for CachingQueryRunner.
    /// </summary>
    /// <remarks>
    /// It only needs to be comparable with itself.
    /// </remarks>
    [DataContract]
    internal class CachingQueryRunnerKey : IEquatable<CachingQueryRunnerKey>, IQueryRunnerCacheKey
    {
        private int _hashCode;

        internal CachingQueryRunnerKey( StructuredQuery query, QuerySettings settings, UserRuleSet userRuleSet, long runAsUser )
        {
            Key = new CachingQuerySqlBuilderKey(query, settings, userRuleSet);
            Bools = PackBools(settings);
            PageSize = settings.PageSize;
            User = runAsUser;
            TargetResource = settings.TargetResource;
            _hashCode = GenerateHashCode();
        }

        /// <summary>
        /// Parameter-less constructor used for serialization.
        /// </summary>
        private CachingQueryRunnerKey()
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
        [DataMember(Order = 1)]
        public CachingQuerySqlBuilderKey Key { get; private set; }

        /// <summary>
        /// Bools.
        /// </summary>
        [DataMember(Order = 2)]
        public int Bools { get; private set; }

        /// <summary>
        /// Page size.
        /// </summary>
        [DataMember(Order = 3)]
        public int PageSize { get; private set; }

        /// <summary>
        /// User ID to run as.
        /// </summary>
        [DataMember(Order = 4)]
        public long User { get; private set; }

        /// <summary>
        /// Target resource ID.
        /// </summary>
        [DataMember(Order = 5)]
        public long TargetResource { get; private set; }

        public bool Equals(CachingQueryRunnerKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TargetResource == other.TargetResource && User == other.User && PageSize == other.PageSize && Bools == other.Bools && Equals(Key, other.Key);
        }

        public bool Equals( IQueryRunnerCacheKeyProvider other )
        {
            if ( ReferenceEquals( null, other ) ) return false;
            return Equals( other );
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CachingQueryRunnerKey) obj);
        }

        public override int GetHashCode()
        {
            // _hashCode cannot be readonly due to OnAfterDeserialization
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _hashCode;
        }

        private int GenerateHashCode()
        {
			unchecked
			{
				int hash = 17;

				hash = hash * 92821 + TargetResource.GetHashCode( );

				hash = hash * 92821 + User.GetHashCode( );

				hash = hash * 92821 + PageSize.GetHashCode( );

				hash = hash * 92821 + Bools.GetHashCode( );

				if ( Key != null )
				{
					hash = hash * 92821 + Key.GetHashCode( );
				}

				return hash;
			}
        }

        public static bool operator ==(CachingQueryRunnerKey left, CachingQueryRunnerKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CachingQueryRunnerKey left, CachingQueryRunnerKey right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Store various boolean settings into a bit field for convenient comparison and storage.
        /// Note: these only have meaning within this class.
        /// </summary>
        private static int PackBools( QuerySettings settings )
        {
            if ( settings == null )
                throw new ArgumentNullException( "settings" );

            int flags = 0;
            if ( settings.ResultSchemaOnly )
                flags += 1;

            return flags;
        }

        /// <summary>
        /// Actually build the SQL
        /// </summary>
        internal static bool DoesRequestAllowForCaching( StructuredQuery query, QuerySettings settings )
        {
            if ( query == null )
                throw new ArgumentNullException( "query" );
            if ( settings == null )
                throw new ArgumentNullException( "settings" );

            if ( !CachingQuerySqlBuilderKey.DoesRequestAllowForCaching( query, settings ) )
            {
                // Reason logged within CachingQuerySqlBuilderKey.DoesRequestAllowForCaching
                return false;
            }

            if ( settings.FirstRow > 0 )
            {
                LogReasonForNonCaching( "Only first page gets cached." );
                return false;
            }

            if ( settings.SupportQuickSearch )
            {
                LogReasonForNonCaching( "Quick search results are not cached." );
                return false;
            }

            if ( settings.SupportRootIdFilter )
            {
                // Don't log .. this is normal operation for security, which does its own caching
                return false;
            }

            if ( settings.IncludeResources != null && settings.IncludeResources.Any() )
            {
                LogReasonForNonCaching( "IncludeResources were used." );
                return false;
            }

            if ( settings.ExcludeResources != null && settings.ExcludeResources.Any( ) )
            {
                LogReasonForNonCaching( "ExcludeResources were used." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Record reason for not caching
        /// </summary>
        private static void LogReasonForNonCaching( string reason )
        {
            using ( MessageContext msg = new MessageContext( "Reports" ) )
            {
                msg.Append( ( ) => "Query result is uncacheable: " + reason );
            }
            EventLog.Application.WriteTrace( "Query result is uncacheable: " + reason );
        }
    }
}
