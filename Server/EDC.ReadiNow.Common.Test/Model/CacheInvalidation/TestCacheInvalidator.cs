// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Cache;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    /// <summary>
    /// Test cache invalidator.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class TestCacheInvalidator<TKey, TValue>: CacheInvalidator<TKey, TValue>
    {
        public TestCacheInvalidator(ICache<TKey, TValue> cache, string name, Func<bool> traceCacheInvalidationSetting = null)
            : base(cache, name, traceCacheInvalidationSetting)
        {
            // Do nothing
        }
    }
}