// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Cache;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Provides a cache of calculated field metadata.
    /// </summary>
    internal class CachingCalculatedFieldMetadataProvider : GenericCacheService<long, CalculatedFieldMetadata>, ICalculatedFieldMetadataProvider
    {
        // A separate cache just for holding whether a given ID represents a calculated field.
        ICache<long, bool> _isCalculatedFieldCache;

        private static CacheFactory cacheFactory = new CacheFactory { BlockIfPending = true, MaxCacheEntries = CacheFactory.DefaultMaximumCacheSize };

        /// <summary>
        /// Create a new <see cref="CachingCalculatedFieldMetadataProvider"/>.
        /// </summary>
        /// <param name="innerProvider">
        /// The <see cref="ICalculatedFieldMetadataProvider"/> that will actually calculate the result. This cannot be null. 
        /// </param>
        public CachingCalculatedFieldMetadataProvider(ICalculatedFieldMetadataProvider innerProvider)
            : base("CalculatedFieldMetadata", cacheFactory)
        {
            if (innerProvider == null)
            {
                throw new ArgumentNullException("innerProvider");
            }

            InnerProvider = innerProvider;

            // Note: this class manages two caches:
            // 1. a full cache service for GetCalculatedFieldMetadata
            // 2. a simple unmanaged, non-invalidated, cache for IsCalculatedField.
            _isCalculatedFieldCache = CacheFactory.CreateSimpleCache<long, bool>("IsCalculatedFieldCache", false, 0);
        }


        /// <summary>
        /// The CalculatedFieldMetadataProvider that performs the actual work.
        /// </summary>
        internal ICalculatedFieldMetadataProvider InnerProvider { get; private set; }


        /// <summary>
        /// Returns true if the ID number refers to a calculated field. Otherwise, false.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns>Returns true if the ID number refers to a calculated field. Otherwise, false.</returns>
        public bool IsCalculatedField(long fieldId)
        {
            bool result;
            _isCalculatedFieldCache.TryGetOrAdd(fieldId, out result, f => InnerProvider.IsCalculatedField(f));
            return result;
        }


        /// <summary>
        /// Get the static information about a calculated field, which can be used to more quickly evaluate individual entities.
        /// </summary>
        /// <param name="fieldIDs">The field IDs to load</param>
        /// <param name="settings">Additional settings.</param>
        /// <returns></returns>
        public IReadOnlyCollection<CalculatedFieldMetadata> GetCalculatedFieldMetadata(IReadOnlyCollection<long> fieldIDs, CalculatedFieldSettings settings)
        {
            // Validate
            if (fieldIDs == null)
            {
                throw new ArgumentNullException("fieldIDs");
            }

            IReadOnlyCollection<KeyValuePair<long, CalculatedFieldMetadata>> cacheResult;
            IReadOnlyCollection<CalculatedFieldMetadata> result;

            cacheResult = GetOrAddMultiple( fieldIDs, fields => InnerProvider.GetCalculatedFieldMetadata(fields.ToList(), settings));

            result = cacheResult.Select(pair => pair.Value).ToList();

            return result;
        }

    }
    
}
