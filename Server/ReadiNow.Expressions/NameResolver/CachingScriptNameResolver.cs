// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac.Extras.AttributeMetadata;
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;

namespace ReadiNow.Expressions.NameResolver
{
    /// <summary>
    /// Caching layer for resolving indentifiers names that appear in scripts.
    /// (i.e. names of fields, relationships and types).
    /// </summary>
    /// <remarks>
    /// For implementation convenience, the same internal cache is used for both types and members, although they accept different arguments.
    /// </remarks>
    class CachingScriptNameResolver : GenericCacheService<CachingScriptNameKey, MemberInfo>, IScriptNameResolver
    {
        /// <summary>
        /// Create a new <see cref="CachingScriptNameResolver"/>.
        /// </summary>
        /// <param name="innerProvider">
        /// The <see cref="IScriptNameResolver"/> that will actually resolve script names. This cannot be null. 
        /// </param>
        public CachingScriptNameResolver([WithKey(Factory.NonCachedKey)] IScriptNameResolver innerProvider)
            : base("ScriptNameKey", new CacheFactory { MetadataCache = true })
        {
            if (innerProvider == null)
            {
                throw new ArgumentNullException("innerProvider");
            }

            InnerProvider = innerProvider;
        }


        /// <summary>
        /// The CalculatedFieldMetadataProvider that performs the actual work.
        /// </summary>
        internal IScriptNameResolver InnerProvider { get; private set; }


        /// <summary>
        /// Given a type and a name, return any instances of that type that have that name.
        /// </summary>
        public IEntity GetInstance(string instanceName, long typeId)
        {
            // No caching support for accessing resources by name
            return InnerProvider.GetInstance(instanceName, typeId);
        }


        /// <summary>
        /// Given a type (e.g. Person), resolve a name that could be a field name, or a relationship name.
        /// </summary>
        public MemberInfo GetMemberOfType(string memberScriptName, long typeId, MemberType memberTypeFilter)
        {
            // Validate
            if (string.IsNullOrEmpty(memberScriptName))
                throw new ArgumentNullException("memberScriptName");
            if (typeId == 0)
                throw new ArgumentNullException("typeId");
            if (memberTypeFilter == MemberType.Type)
                throw new ArgumentException("memberTypeFilter");

            // Create key
            CachingScriptNameKey key = new CachingScriptNameKey(typeId, memberScriptName, memberTypeFilter);

            // Check cache
            MemberInfo cacheResult = GetOrAdd(key, k =>
            {
                var res = InnerProvider.GetMemberOfType(memberScriptName, typeId, memberTypeFilter);
                return res;
            });

            return cacheResult;
        }


        /// <summary>
        /// Given the script name of a type or object, returns its ID.
        /// </summary>
        public long GetTypeByName(string typeScriptName)
        {
            // Validate
            if (string.IsNullOrEmpty(typeScriptName))
                throw new ArgumentNullException("memberScriptName");

            // Create key
            CachingScriptNameKey key = new CachingScriptNameKey(0, typeScriptName, MemberType.Type);

            // Check cache
            MemberInfo cacheResult = GetOrAdd(key, k =>
            {
                long typeId = InnerProvider.GetTypeByName(typeScriptName);
                return new MemberInfo { MemberId = typeId, MemberType = MemberType.Type };
            });

            return cacheResult.MemberId;
        }
    }
}
