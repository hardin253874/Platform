// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A factory for creating access control classes.
    /// </summary>
    public interface IEntityAccessControlFactory
    {
        /// <summary>
        /// Get an <see cref="IEntityAccessControlService"/>, used for high-level access control operations.
        /// </summary>
        IEntityAccessControlService Service { get; }

        /// <summary>
        /// The caches (used for invalidation).
        /// </summary>
        IEnumerable<ICacheInvalidator> CacheInvalidators { get; }
    }
}