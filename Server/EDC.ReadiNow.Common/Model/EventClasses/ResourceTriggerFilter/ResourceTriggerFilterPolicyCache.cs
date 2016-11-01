// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter
{
    /// <summary>
    /// A cache of filtered trigger policies along with the handlers that service them.
    /// </summary>
    public class ResourceTriggerFilterPolicyCache : IResourceTriggerFilterPolicyCache
    {
		/// <summary>
		/// The type policy map
		/// </summary>
		private readonly ICache<long, List<ResourceTriggerFilterDef>> _typePolicyMap;

		/// <summary>
		/// The policy to fields map
		/// </summary>
		private readonly ICache<long, HashSet<long>> _policyToFieldsMap;

	    /// <summary>
	    /// The policy type handler map
	    /// </summary>
	    private readonly ICache<long, IFilteredSaveEventHandler> _policyTypeHandlerMap;

        /// <summary>
        /// This 'Cache' is per tenant and thread safe and contains a single entry if precaching occurs. We are using this rather than a hashSet keyed on tenant to ensure that
        /// when a tenant wide cache clear occurs we will trigger a precache on the next request.
        /// </summary>
        private readonly ICache<long, bool> _areWePrecachedYet;

		/// <summary>
		/// Thread synchronization
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		/// The policies that apply to a type including derived types
		/// </summary>
		/// <value>
		/// The type policy map.
		/// </value>
		public ICache<long, List<ResourceTriggerFilterDef>> TypePolicyMap
        {
			get
            {
				PreCache( );

				return _typePolicyMap;
			}
		}

	    /// <summary>
		/// Gets the policy to fields map.
		/// </summary>
		/// <value>
		/// The policy to fields map.
		/// </value>
		public ICache<long, HashSet<long>> PolicyToFieldsMap
                {
		    get
                    {
				PreCache( );

				return _policyToFieldsMap;
			}
	    }

		/// <summary>
		/// Gets the policy type handler map.
		/// </summary>
		/// <value>
		/// The policy type handler map.
		/// </value>
		public ICache<long, IFilteredSaveEventHandler> PolicyTypeHandlerMap
                        {
			get
			{
				PreCache( );

				return _policyTypeHandlerMap;
                        }
                    }

		/// <summary>
		/// Pre fill the cache for the tenant.
		/// </summary>
		private void PreCache( )
		{
            long dummyKey = 999;
            bool dummyValue;

            // We are using a per tenant thread safe cache to determine is the tenant has already been precached.
            // The key is unimportant as we will only have one entry. (Each tenant is isolated) 
            // The value is also unimportant.
            _areWePrecachedYet.TryGetOrAdd(dummyKey, out dummyValue, (ignoredKey) =>
            {
                using (new SecurityBypassContext())
                {
                    CachePolicies(HandlerFactories);
                }

                return true;    // the returned value does not matter
            });
		}

        /// <summary>
		/// Gets or sets the handler factories.
		/// </summary>
		/// <value>
		/// The handler factories.
		/// </value>
		private IEnumerable<IFilteredTargetHandlerFactory> HandlerFactories
	    {
		    get;
		    set;
	    }

	    public ResourceTriggerFilterPolicyCache(IEnumerable<IFilteredTargetHandlerFactory> handlerFactories)
        {
            using (Profiler.Measure("ResourceTriggerFilterPolicyCache Init"))
            {
                var cacheFactory = new CacheFactory() { IsolateTenants = true, ThreadSafe = true, CacheName = "ResourceTriggerFilterPolicyCache" }; // SHOULD BE METADATA?

	            HandlerFactories = handlerFactories;

                _typePolicyMap = cacheFactory.Create<long, List<ResourceTriggerFilterDef>>();       // The policies that apply to a type including derived types
                _policyToFieldsMap = cacheFactory.Create<long, HashSet<long>>();
                _policyTypeHandlerMap = cacheFactory.Create<long, IFilteredSaveEventHandler>();
                _areWePrecachedYet = cacheFactory.Create<long, bool>();
            }
        }

        /// <summary>
        /// Find all the handler factories and cache the policies they refer to.
        /// </summary>
        /// <param name="handlerFactories"></param>
        void CachePolicies(IEnumerable<IFilteredTargetHandlerFactory> handlerFactories)
        {
            // Deal with the upgrade situation where trigger filter hasn't been laid down yet - 2016/04/06 - should be able to be removed safely after version 2.67 
            if (!Entity.Exists(new EntityRef("core:resourceTriggerFilterDef")))
                return;

            foreach (var def in handlerFactories)
            {
                var handler = def.CreateSaveEventHandler();

                EntityType trackedType; 

                try // The type may not be in the current tenant.
                {
                    trackedType = def.GetHandledType();
                }
                catch(ArgumentException)                         // TODO: FIX this is ugly
                {
                    trackedType = null;
                }

                if (trackedType != null)  
                {
                    var policies = Entity.GetInstancesOfType(trackedType).Select(t => t.Cast<ResourceTriggerFilterDef>());

                    _policyTypeHandlerMap.Add(trackedType.Id, handler);

                    foreach (var policy in policies)
                    {
                        if (policy.TriggerEnabled == true && policy.TriggeredOnType != null)
                        {
                            UpdateOrAddPolicy(policy);
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Update the cache for the given policy
        /// </summary>
        /// <param name="policy"></param>
        public void UpdateOrAddPolicy(ResourceTriggerFilterDef policy)
        {
            using (Profiler.Measure("ResourceTriggerFilterPolicyCache UpdateOrAddPolicy"))
            {
                RemovePolicy(policy.Id);

                if (policy.TriggerEnabled == true && policy.TriggeredOnType != null)
                {
                    var allFieldRelIds = new HashSet<long>(policy.UpdatedFieldsToTriggerOn.Select(f => f.Id).Union(policy.UpdatedRelationshipsToTriggerOn.Select(r => r.Id)));
                    _policyToFieldsMap.Add(policy.Id, allFieldRelIds);

                    var handler = _policyTypeHandlerMap[policy.TypeIds.First()];

                    var targetTypes = policy.TriggeredOnType.GetDescendantsAndSelf();
                    foreach (var targetType in targetTypes)
                    {
                        List<ResourceTriggerFilterDef> syncDefList;
                        if (!_typePolicyMap.TryGetValue(targetType.Id, out syncDefList))
                        {
                            syncDefList = new List<ResourceTriggerFilterDef>();
                            _typePolicyMap.Add(targetType.Id, syncDefList);
                        }

                        syncDefList.Add(policy);


                    }
                }
            }
        }


        /// <summary>
        /// Remove the given policy from the cache.
        /// </summary>
        /// <param name="policyId"></param>
        public void RemovePolicy(long policyId)
        {
            using (Profiler.Measure("ResourceTriggerFilterPolicyCache RemovePolicy"))
            {
                foreach (var kvp in _typePolicyMap)
                {
                    kvp.Value.RemoveAll(p => p.Id == policyId);
                    // Note that we are not removing the entry. This is because it is most common for a policy to be replaced rather than actually removed.
                }

                _policyToFieldsMap.Remove(policyId);
            }
        }


    }
}
