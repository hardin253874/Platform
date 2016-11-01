// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// Is the context new or attaching to an existing entry
    /// in the call context?
    /// </summary>
    public enum ContextType
    {
        /// <summary>
        /// This <see cref="CacheContext"/> is new, meaning it allocated a new
        /// entry in the call context. When disposed, removes the entry.
        /// </summary>
        New,
        /// <summary>
        /// This <see cref="CacheContext"/> is attached to an existing context.
        /// When disposed, the call context is untouched.
        /// </summary>
        Attached,
        /// <summary>
        /// This <see cref="CacheContext"/> is not attached to any context, 
        /// meaning adding entities, relationship types and fields to it
        /// does nothing.
        /// </summary>
        Detached,
        /// <summary>
        /// This <see cref="CacheContext"/> effectively hides the parent context
        /// without re-establishing a new current one.
        /// </summary>
        None
    }
}