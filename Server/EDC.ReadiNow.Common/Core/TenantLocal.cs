// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Core
{
    /// <summary>
    /// A static-per-tenant.
    /// </summary>
    public class TenantLocal<T>
    {
        /// <summary>
        /// Cache of values for each tenant.
        /// </summary>
        private ConcurrentDictionary<long, T> _cache = new ConcurrentDictionary<long, T>( );

        /// <summary>
        /// Value factory.
        /// </summary>
        private Func<long, T> _valueFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="valueFactory">Factory to create a value for the current tenant. Callback receives the current tenantId.</param>
        public TenantLocal( Func<long, T> valueFactory )
        {
            if ( valueFactory == null )
                throw new ArgumentNullException( "valueFactory" );

            _valueFactory = valueFactory;
        }

        /// <summary>
        /// Return an instance that relates to the current tenant.
        /// </summary>
        public T Value
        {
            get
            {
                T result;
                long currentTenant = RequestContext.TenantId;

                result = _cache.GetOrAdd( currentTenant, _valueFactory );

                return result;
            }
            set
            {
                long currentTenant = RequestContext.TenantId;

                _cache.AddOrUpdate( currentTenant, value, (key, oldValue) => value );
            }
        }

        /// <summary>
        /// Clears all cached values.
        /// </summary>
        public void ClearAll()
        {
            _cache.Clear( );
        }
    }
}
