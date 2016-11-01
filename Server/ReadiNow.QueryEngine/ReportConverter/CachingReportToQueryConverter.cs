// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using Autofac.Extras.AttributeMetadata;
using EDC.Cache;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Diagnostics;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Core.Cache;

namespace ReadiNow.QueryEngine.ReportConverter
{
    /// <summary>
    /// Convert a <see cref="Report"/> to a <see cref="StructuredQuery"/>, caching ones previously converted.
    /// </summary>
    public class CachingReportToQueryConverter : IReportToQueryConverter, ICacheService
    {
        private readonly object _syncRoot;
        private readonly ReportToQueryCacheInvalidator _cacheInvalidator;

        /// <summary>
        /// Create a new <see cref="CachingReportToQueryConverter"/>.
        /// </summary>
        /// <param name="converter">
        /// The wrapped converter.
        /// </param>
        public CachingReportToQueryConverter([WithKey(Factory.NonCachedKey)] IReportToQueryConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            Converter = converter;
            Cache = CacheFactory.CreateSimpleCache<CachingReportToQueryConverterKey, CachingReportToQueryConverterValue>( "Report To Query" );
            _cacheInvalidator = new ReportToQueryCacheInvalidator(Cache);
            _syncRoot = new object();
        }

        /// <summary>
        /// The inner <see cref="IReportToQueryConverter"/>.
        /// </summary>
        public IReportToQueryConverter Converter { get; }

        /// <summary>
        /// The cache.
        /// </summary>
        internal ICache<CachingReportToQueryConverterKey, CachingReportToQueryConverterValue> Cache { get; }

        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        public ICacheInvalidator CacheInvalidator => _cacheInvalidator;

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            System.Diagnostics.Trace.WriteLine("CachingReportToQueryConverter: Cache cleared");
            Cache.Clear();
        }

        /// <summary>
        /// Convert a <see cref="Report" /> to a <see cref="StructuredQuery" />.
        /// </summary>
        /// <param name="report">The <see cref="Report" /> to convert. This cannot be null.</param>
        /// <returns>
        /// The converted report.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">report</exception>
        public StructuredQuery Convert( Report report )
        {
            return Convert( report, null );
        }

        /// <summary>
        /// Convert a <see cref="Report" /> to a <see cref="StructuredQuery" />.
        /// </summary>
        /// <param name="report">The <see cref="Report" /> to convert. This cannot be null.</param>
        /// <param name="settings"></param>
        /// <returns>
        /// The converted report.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">report</exception>
        public StructuredQuery Convert(Report report, ReportToQueryConverterSettings settings)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }
            if ( settings == null )
            {
                settings = ReportToQueryConverterSettings.Default;
            }

            StructuredQuery result;
            CachingReportToQueryConverterValue cacheValue;

            CachingReportToQueryConverterKey key = new CachingReportToQueryConverterKey( report, settings );

            using ( MessageContext msg = new MessageContext( "Reports" ) )
            {
                // Check cache
                bool doConvert = !TryGetValue( key, msg, out cacheValue );

                // Check for force recalculation
                if ( settings.RefreshCachedStructuredQuery )
                {
                    msg.Append( ( ) => "CachingReportToQueryConverter refreshed forced" );
                    doConvert = true;
                }

                if ( doConvert )
                {
                    lock (_syncRoot)
                    {
                        using ( CacheContext cacheContext = new CacheContext( ) )
                        {
                            result = Converter.Convert( report, settings );
                            cacheValue = new CachingReportToQueryConverterValue( result );

                            Cache.Add( key, cacheValue );

                            // Add the cache context entries to the appropriate CacheInvalidator
                            _cacheInvalidator.AddInvalidations( cacheContext, key );
                        }
                    }
                }
                else if ( CacheContext.IsSet( ) )
                {
                    // Add the already stored changes that should invalidate this cache
                    // entry to any outer or containing cache contexts.
                    using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                    {
                        cacheContext.AddInvalidationsFor( _cacheInvalidator, key );
                    }
                }
            }

            result = cacheValue.StructuredQuery;
            return result;
        }

        /// <summary>
        /// Try to get the value from cache, with logging.
        /// </summary>
        private bool TryGetValue( CachingReportToQueryConverterKey key, MessageContext msg, out CachingReportToQueryConverterValue result )
        {
            msg.Append( ( ) => "CachingReportToQueryConverter key:" + key );

            CachingReportToQueryConverterValue cacheValue;
            bool foundValue;

            if ( Cache.TryGetValue( key, out cacheValue ) )
            {
                msg.Append( ( ) => "CachingReportToQueryConverter cache hit" );
                msg.Append( ( ) => $"Entry originally cached at {cacheValue.CacheTime}" );
                result = cacheValue;
                foundValue = true;
            }
            else
            {
                msg.Append( ( ) => "CachingReportToQueryConverter cache miss" );
                result = null;
                foundValue = false;
            }

            return foundValue;
        }
    }
}
