// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using System;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model.CacheInvalidation;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using ReadiNow.Reporting;
using ServiceResult = ReadiNow.Reporting.Result;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
    /// <summary>
    /// A cache that supports caching of reports based on incomplete report runs.
    /// </summary>
    /// <remarks>
    /// A formatter is provided to transform completed results.
    /// </remarks>
    /// <typeparam name="TResult">Type of cache result</typeparam>
    public class ReportResultCache : ICacheService
    {
        private readonly ReportResultCacheInvalidator _cacheInvalidator;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportResultCache()
        {            
            var factory = new CacheFactory
            {
                CacheName = "Report Result",
                BlockIfPending = true,
                MaxCacheEntries = CacheFactory.DefaultMaximumCacheSize,
                Redis = true,
                RedisValueCompression = true,
                RedisKeyExpiry = TimeSpan.FromMinutes( 5 ),
                DelayedInvalidates = true
            };

            Cache = factory.Create<ReportResultCacheKey, string>( );

            _cacheInvalidator = new ReportResultCacheInvalidator( Cache );
        }

        /// <summary>
        /// The cache of JSON results.
        /// </summary>
        public ICache<ReportResultCacheKey, string> Cache { get; private set; }

        /// <summary>
        /// Check the cache.
        /// Run the continuation callback if necessary.
        /// Apply presentation formatting.
        /// Store result in cache if possible.
        /// </summary>
        /// <typeparam name="T">Type of result after presentation formatting.</typeparam>
        /// <param name="reportCompletionData">The completion process for a partially processed report.</param>
        /// <param name="presentationCallback">Presentation formatting, such as conversion to webapi message.</param>
        /// <returns></returns>
        public string GetReportResult( ReportCompletionData reportCompletionData, Func<ServiceResult.ReportResult, string> presentationCallback )
        {
            if ( reportCompletionData == null )
                return null;
            
            ReportResultCacheKey cacheKey;
            string result;

            using ( MessageContext msg = new MessageContext( "Reports" ) )
            {
                cacheKey = reportCompletionData.ResultCacheKey;

                // A null cacheKey indicates that the report is uncacheable
                if ( cacheKey == null )
                {
                    msg.Append( ( ) => "ReportResultCache received no cache key" );

                    // Invoke callback
                    ServiceResult.ReportResult serviceResult = reportCompletionData.PerformRun( );

                    // And format it
                    result = presentationCallback( serviceResult );
                }
                else
                {
                    // Check cache
                    bool fromCache = TryGetOrAdd( cacheKey, msg, out result, key1 =>
                    {
                        string formattedResult;

                        using ( CacheContext cacheContext = new CacheContext( ) )
                        {
                            // Call completion callback to run report
                            ServiceResult.ReportResult serviceResult = reportCompletionData.PerformRun( );

                            // Format result
                            formattedResult = presentationCallback( serviceResult );

                            // Add the cache context entries to the appropriate CacheInvalidator
                            _cacheInvalidator.AddInvalidations( cacheContext, cacheKey );

                            if ( reportCompletionData.CacheContextDuringPreparation != null )
                            {
                                _cacheInvalidator.AddInvalidations( reportCompletionData.CacheContextDuringPreparation, cacheKey );                
                            }
                        }
                        return formattedResult;
                    } );

                    // Note: No call to AddInvalidationsFor because no-one is listening.
                }

            }

            return result;
        }


        /// <summary>
        /// Try to get the value from cache, with logging.
        /// </summary>
        private bool TryGetOrAdd( ReportResultCacheKey key, MessageContext msg, out string result, Func<ReportResultCacheKey, string> valueFactory )
        {
            bool foundValue;

            foundValue = Cache.TryGetOrAdd( key, out result, valueFactory );

            msg.Append( ( ) => "ReportResultCache key:" + key.ToString( ) );
            if ( foundValue )
            {
                var cacheValue = result;
                msg.Append( ( ) => "ReportResultCache cache hit" );
                //msg.Append( ( ) => string.Format( "Entry originally cached at {0}", cacheValue.CacheTime ) );
            }
            else
            {
                msg.Append( ( ) => "ReportResultCache cache miss" );
            }

            return foundValue;
        }

        public ICacheInvalidator CacheInvalidator
        {
            get { return _cacheInvalidator; }
        }

        public void Clear( )
        {
            Cache.Clear( );
        }
    }
}