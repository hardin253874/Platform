// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Cache;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter
{
    /// <summary>
    /// Interface for a cache of ResourceTriggerFilters
    /// </summary>
    public interface IResourceTriggerFilterPolicyCache
    {
        /// <summary>
        /// Gets the policy type handler map.
        /// </summary>
        ICache<long, IFilteredSaveEventHandler> PolicyTypeHandlerMap { get; }

        /// <summary>
        /// Gets the policy to fields map.
        /// </summary>
        ICache<long, HashSet<long>> PolicyToFieldsMap { get; }

        /// <summary>
        /// The policies that apply to a type including derived types
        /// </summary>
        ICache<long, List<ResourceTriggerFilterDef>> TypePolicyMap { get; }

        /// <summary>
        /// Update the cache for the given policy
        /// </summary>
        void UpdateOrAddPolicy(ResourceTriggerFilterDef policy);

        /// <summary>
        /// Remove the given policy from the cache.
        /// </summary>
        void RemovePolicy(long policyId);
    }
}