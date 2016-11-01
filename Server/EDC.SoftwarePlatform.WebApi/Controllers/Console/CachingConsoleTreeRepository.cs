// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Core;
using EDC.Cache;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Cache;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    /// <summary>
    /// A cached repository for console navigation trees.
    /// The repository is only invalidated when metadata changes. The caching is per tenant and user rule set.
    /// </summary>
    public class CachingConsoleTreeRepository : IConsoleTreeRepository, ICacheService
    {
        readonly ICache<long, EntityData> _securedResultCache;       // Note that this cache is already tenant isolated and per security rule set.
        readonly IConsoleTreeRepository _innerRepository;
        readonly CachingConsoleTreeRepositoryInvalidator _cacheInvalidator;

        /// <summary>
        /// Returns the cache invalidator
        /// </summary>
        public ICacheInvalidator CacheInvalidator => _cacheInvalidator;

        /// <summary>
        /// Create a new <see cref="ConsoleTreeRepository"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="treeRepository"/> cannot be null.
        /// </exception>
        public CachingConsoleTreeRepository(ConsoleTreeRepository treeRepository)
        {
            if (treeRepository == null)
            {
                throw new ArgumentNullException("treeRepository");
            }

            _innerRepository = treeRepository;
            _securedResultCache = Factory.Current.ResolveNamed<ICache<long, EntityData>>("TreeRequest Secured Result");
            _cacheInvalidator = new CachingConsoleTreeRepositoryInvalidator(_securedResultCache);
        }


        /// <summary>
        /// Build and return the console navigation tree. This will be isolated by tenant and rule-set.
        /// </summary>
        /// <returns>
        /// A <see cref="EntityData"/> containing the tree structure.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ReadiNow.IO.RequestContext"/> is not set.
        /// </exception>
        public EntityData GetTree()
        {
            if (!ReadiNow.IO.RequestContext.IsSet)
            {
                throw new InvalidOperationException("Request context not set");
            }

            EntityData result;
            var key = ReadiNow.IO.RequestContext.GetContext().Identity.Id;
            _securedResultCache.TryGetOrAdd(key, out result, innerKey =>
            {
                // Add invalidation
                using (CacheContext cacheContext = new CacheContext())
                {
                    cacheContext.RelationshipTypes.Add(WellKnownAliases.CurrentTenant.AllowDisplay);                    
                    _cacheInvalidator.AddInvalidations(cacheContext, innerKey);
                }

                return _innerRepository.GetTree();
            });            

            return result;
           
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            _securedResultCache.Clear();            
        }
    }
}