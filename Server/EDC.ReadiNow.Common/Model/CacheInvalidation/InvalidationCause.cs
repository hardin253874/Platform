// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// The operation that is causing the invalidation.
    /// </summary>
    public enum InvalidationCause
    {
        /// <summary>
        /// An entity save (includes create).
        /// </summary>
        Save,
        /// <summary>
        /// An entity delete.
        /// </summary>
        Delete
    }
}