// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Core.Cache.Providers
{
    /// <summary>
    /// Interface for invalidating a per-tenant cache.
    /// </summary>
    public interface IPerTenantCache
    {
        void InvalidateTenant( long tenantId );
    }
}
