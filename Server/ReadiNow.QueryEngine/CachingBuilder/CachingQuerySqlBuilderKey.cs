// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Security.AccessControl;
using System;
using System.Linq;
using ProtoBuf;

namespace ReadiNow.QueryEngine.CachingBuilder
{
    /// <summary>
    /// Cache key for CachingQuerySqlBuilderKey.
    /// </summary>
    /// <remarks>
    /// It only needs to be comparable with itself.
    /// </remarks>
    [DataContract]
    internal class CachingQuerySqlBuilderKey : IEquatable<CachingQuerySqlBuilderKey>
    {
        private int _hashCode;

        internal CachingQuerySqlBuilderKey( StructuredQuery query, QuerySqlBuilderSettings settings, UserRuleSet userRuleSet )
        {
            CacheKeyTokens = GetCacheKeyTokens(query, settings); // Uninspired, but adequate
            Bools = PackBools(settings);
            UserRuleSet = userRuleSet;        // Users may share a query plan if they have the same set of security rules
            TimeZoneName = query.TimeZoneName; // TimeZoneName: Ideally this could be removed from the cache key, but it will be some effort for negligable short-term benefit            
            _hashCode = GenerateHashCode();
        }

        /// <summary>
        /// Parameterless constructor used for serialization only.
        /// </summary>
        private CachingQuerySqlBuilderKey()
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
        /// Cache key tokens.
        /// </summary>
        [DataMember(Order = 1)]
        public string CacheKeyTokens { get; private set; }

        /// <summary>
        /// Bools.
        /// </summary>
        [DataMember(Order = 2)]
        public int Bools { get; private set; }

        /// <summary>
        /// User rule set.
        /// </summary>
        [DataMember(Order = 3)]
        public UserRuleSet UserRuleSet { get; private set; }

        /// <summary>
        /// Time zone name.
        /// </summary>
        [DataMember(Order = 4)]
        public string TimeZoneName { get; private set; }

        public bool Equals(CachingQuerySqlBuilderKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CacheKeyTokens, other.CacheKeyTokens) && Equals(UserRuleSet, other.UserRuleSet) && Bools == other.Bools && string.Equals(TimeZoneName, other.TimeZoneName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CachingQuerySqlBuilderKey) obj);
        }

        public override int GetHashCode()
        {           
            return _hashCode;
        }

        private int GenerateHashCode()
        {
            unchecked
            {
	            int hashCode = 17;

	            if ( CacheKeyTokens != null )
	            {
		            hashCode = hashCode * 92821 + CacheKeyTokens.GetHashCode( );
	            }

				if ( UserRuleSet != null )
				{
					hashCode = hashCode * 92821 + UserRuleSet.GetHashCode( );
				}

				hashCode = hashCode * 92821 + Bools.GetHashCode( );

				if ( TimeZoneName != null )
				{
					hashCode = hashCode * 92821 + TimeZoneName.GetHashCode( );
				}

                return hashCode;
            }
        }

        public static bool operator ==(CachingQuerySqlBuilderKey left, CachingQuerySqlBuilderKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CachingQuerySqlBuilderKey left, CachingQuerySqlBuilderKey right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Get cache key hashes for both the query and the settings.
        /// </summary>
        /// <remarks>
        /// Note: we need to do both in the same context, so that a single StructuredQueryHashingContext can do all of the GUID normalizations together.
        /// </remarks>
        /// <returns>A hash string</returns>
        private static string GetCacheKeyTokens( StructuredQuery query, QuerySqlBuilderSettings settings )
        {
            if ( query == null )
                throw new ArgumentNullException( "query" );

            using ( new StructuredQueryHashingContext( ) )
            {
                string queryHash = query.CacheKeyToken();

	            byte[ ] clientAggregateBytes = null;

	            if ( settings.ClientAggregate != null )
	            {
					clientAggregateBytes = settings.ClientAggregate.Serialize( );
	            }

	            byte[ ] sharedParameterBytes = null;

	            if ( settings.SharedParameters != null )
	            {
					using ( var memoryStream = new MemoryStream( ) )
					{
						// Serialize the query                    
						Serializer.Serialize( memoryStream, settings.SharedParameters );
						memoryStream.Flush( );
						sharedParameterBytes = memoryStream.ToArray( );
					}
	            }

	            byte[ ] hashValue = HashValues( clientAggregateBytes, sharedParameterBytes );

	            string settingsHash = string.Empty;

	            if ( hashValue != null )
	            {
		            settingsHash = Convert.ToBase64String( hashValue );
	            }
	            
                string key = string.Concat( queryHash, " / ", settingsHash );
                return key;
            }
        }

		/// <summary>
		/// Hashes the values.
		/// </summary>
		/// <param name="arrays">The arrays.</param>
		/// <returns></returns>
	    private static byte[ ] HashValues( params byte[ ][ ] arrays )
	    {
		    if ( arrays == null || arrays.Length <= 0 )
		    {
			    return null;
		    }

		    byte[ ][ ] input = arrays.Where( array => array != null ).ToArray( );

			if ( input.Length <= 0 )
			{
				return null;
			}

		    MD5 md5 = new MD5CryptoServiceProvider( );

			for ( int i = 0; i < input.Length - 1; i++ )
		    {
				md5.TransformBlock( input [ i ], 0, input [ i ].Length, input [ i ], 0 );
		    }

			md5.TransformFinalBlock( input [ input.Length - 1 ], 0, input [ input.Length - 1 ].Length );

		    return md5.Hash;
	    }

	    /// <summary>
        /// Store various boolean settings into a bit field for convenient comparison and storage.
        /// Note: these only have meaning within this class.
        /// </summary>
        private static int PackBools( QuerySqlBuilderSettings settings )
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            // Don't cache if any are true (for now)
            // DebugMode, CaptureExpressionMetadata

            int flags = 0;
            if ( settings.SecureQuery )
                flags += 1;
            if ( settings.SupportPaging )
                flags += 2;
            if ( settings.SupportQuickSearch )
                flags += 4;
            if ( settings.UseSharedSql )
                flags += 8;
            if ( settings.FullAggregateClustering )
                flags += 16;
            if (settings.SupportClientAggregate)
                flags += 32;
            if (settings.SupportRootIdFilter)
                flags += 64;
            if (settings.SuppressRootTypeCheck)
                flags += 128;            

            return flags;
        }

        /// <summary>
        /// Actually build the SQL
        /// </summary>
        internal static bool DoesRequestAllowForCaching( StructuredQuery query, QuerySqlBuilderSettings settings )
        {
            if ( query == null )
                throw new ArgumentNullException( "query" );
            if ( settings == null )
                throw new ArgumentNullException( "settings" );

            // Note : settings.AdditionalOrderColumns is wholly derived from the query and settings, so we can ignore it
            // (however it should probably therefore be removed from the QuerySqlBuilderSettings altogether)

            // For now .. for safety
            // Note: if we ever want to address this, we need to ensure expression GUIDs get stitched up in QueryResult.ExpressionTypes
            if ( settings.CaptureExpressionMetadata )
            {
                LogReasonForNonCaching( "CaptureExpressionMetadata" );
                return false;
            }

            // For now .. for sanity
            if ( settings.DebugMode )
            {
                LogReasonForNonCaching( "DebugMode" );
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
                msg.Append( ( ) => "Query is uncacheable: " + reason );
            }
            EventLog.Application.WriteTrace( "Query is uncacheable: " + reason );
        }
    }
}
