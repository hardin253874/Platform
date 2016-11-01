// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EDC.Cache;
using ProtoBuf;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Diagnostics;
using System.Diagnostics;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core.Cache.Providers;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Provides a cache of UserRuleSets
    /// I.e. a cached lookup from a userId to an object that represents the hash of the security rules that apply to the user.
    /// </summary>
    public class CachingUserRuleSetProvider : IUserRuleSetProvider, ICacheService
    {
        /// <summary>
        /// Create a new <see cref="CachingUserRuleSetProvider"/>.
        /// </summary>
        /// <param name="innerProvider">
        /// The <see cref="IUserRuleSetProvider"/> that will actually calculate the result. This cannot be null. 
        /// </param>
        public CachingUserRuleSetProvider( IUserRuleSetProvider innerProvider )
        {
            if ( innerProvider == null )
            {
                throw new ArgumentNullException( "innerProvider" );
            }

            InnerProvider = innerProvider;            
            Cache = new CacheFactory()
            {
                BlockIfPending = true,
                CacheName = "UserRuleSet",
                MaxCacheEntries = CacheFactory.DefaultMaximumCacheSize,
                Distributed = false
            }.Create<CachingUserRuleSetProviderKey, UserRuleSet>();                            

            _cacheInvalidator = new CachingUserRuleSetProviderInvalidator( Cache );
        }

        private readonly CachingUserRuleSetProviderInvalidator _cacheInvalidator;


        /// <summary>
        /// The inner <see cref="IUserRuleSetProvider"/> that actually calculates results.
        /// </summary>
        internal IUserRuleSetProvider InnerProvider { get; private set; }

        /// <summary>
        /// The cache itself.
        /// </summary>
        internal ICache<CachingUserRuleSetProviderKey, UserRuleSet> Cache { get; private set; }

        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        public ICacheInvalidator CacheInvalidator
        {
            get { return _cacheInvalidator; }
        }


        /// <summary>
        /// Gets a value that represents a hash of the IDs of a set of security rules.
        /// That is, if two users have equatable UserRuleSets then the same security rules apply to both.
        /// </summary>
        /// <param name="userId">
        /// The ID of the user <see cref="UserAccount"/>.
        /// This cannot be negative.
        /// </param>
        /// <param name="permission">
        /// The permission to get the query for. This cannot be null and should be one of <see cref="Permissions.Read"/>,
        /// <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>.
        /// </param>
        public UserRuleSet GetUserRuleSet( long userId, EntityRef permission)
        {
            // Validate
            if ( userId <= 0 )
            {
                throw new ArgumentNullException( "userId" );
            }
            if ( permission == null )
            {
                throw new ArgumentNullException( "permission" );
            }
            if ( permission.Id <= 0 )
            {
                throw new ArgumentException( "permission" );
            }

            // Create cache key
            CachingUserRuleSetProviderKey key = new CachingUserRuleSetProviderKey( userId, permission.Id );
            UserRuleSet result;

            Func<CachingUserRuleSetProviderKey, UserRuleSet> valueFactory = (k) =>
            {
                UserRuleSet innerResult;

                using (CacheContext cacheContext = new CacheContext())
                {
                    innerResult = InnerProvider.GetUserRuleSet(k.SubjectId, k.PermissionId);

                    // Add the cache context entries to the appropriate CacheInvalidator
                    _cacheInvalidator.AddInvalidations(cacheContext, key);
                }

                return innerResult;
            };
            
            // Check cache
            if ( Cache.TryGetOrAdd( key, out result, valueFactory) &&
                CacheContext.IsSet())
            {
                // Add the already stored changes that should invalidate this cache
                // entry to any outer or containing cache contexts.
                using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                {
                    cacheContext.AddInvalidationsFor( _cacheInvalidator, key );
                }
            }

            return result;
        }


        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear( )
        {
            Trace.WriteLine( "UserRuleSet: Cache cleared" );
            Cache.Clear( );
        }

    }


    internal class CachingUserRuleSetProviderInvalidator : SecurityCacheInvalidatorBase<CachingUserRuleSetProviderKey, UserRuleSet>
    {
        /// <summary>
        /// Create a new <see cref="CachingUserRuleSetProviderInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate at the appropriate time. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cache"/> cannot be null.
        /// </exception>
        public CachingUserRuleSetProviderInvalidator( ICache<CachingUserRuleSetProviderKey, UserRuleSet> cache )
            : base( cache, "UserRuleSet" )
        {
            // Do nothing
        }
    }

    [DataContract]
    internal class CachingUserRuleSetProviderKey : IEquatable<CachingUserRuleSetProviderKey>
    {
        private int _hashCode;

        /// <summary>
        /// Create a new <see cref="CachingUserRuleSetProviderKey"/>.
        /// </summary>
        /// <param name="subjectId">
        /// The subject ID.
        /// </param>
        /// <param name="permissionId">
        /// The permission ID.
        /// </param>
        public CachingUserRuleSetProviderKey( long subjectId, long permissionId )
        {
            SubjectId = subjectId;
            PermissionId = permissionId;
            _hashCode = GenerateHashCode();
        }

        /// <summary>
        /// Parameterless constructor used for serialization only.
        /// </summary>
        private CachingUserRuleSetProviderKey()
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
        /// The Subject ID.
        /// </summary>
        [DataMember(Order = 1)]
        public long SubjectId { get; private set; }

        /// <summary>
        /// The permission ID.
        /// </summary>
        [DataMember(Order = 2)]
        public long PermissionId { get; private set; }

        public bool Equals(CachingUserRuleSetProviderKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SubjectId == other.SubjectId && PermissionId == other.PermissionId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CachingUserRuleSetProviderKey) obj);
        }

        public override int GetHashCode()
        {           
            return _hashCode;
        }

        private int GenerateHashCode()
        {
			unchecked
			{
				int hash = 17;

				hash = hash * 92821 + SubjectId.GetHashCode( );

				hash = hash * 92821 + PermissionId.GetHashCode( );

				return hash;
			}
        }

        public static bool operator ==(CachingUserRuleSetProviderKey left, CachingUserRuleSetProviderKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CachingUserRuleSetProviderKey left, CachingUserRuleSetProviderKey right)
        {
            return !Equals(left, right);
        }
    }
}
