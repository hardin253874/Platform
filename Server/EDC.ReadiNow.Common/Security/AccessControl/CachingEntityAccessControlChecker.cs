// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
	/// An entity access control checker that provides a cache for <see cref="UserEntityPermissionTuple"/>.
    /// </summary>
    public class CachingEntityAccessControlChecker : CachingEntityAccessControlCheckerBase<UserEntityPermissionTuple>
    {
        /// <summary>
        /// Create a new <see cref="CachingEntityAccessControlChecker"/>.
        /// </summary>
        public CachingEntityAccessControlChecker()
            : this(new EntityAccessControlChecker())
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="CachingEntityAccessControlChecker"/>.
        /// </summary>
        /// <param name="entityAccessControlChecker">
        /// The <see cref="IEntityAccessControlChecker"/> used to actually perform checks.
        /// This cannot be null.
        /// </param>
        /// <param name="cacheName">
        /// (Optional) Cache name. If supplied, this cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityAccessControlChecker"/> cannot be null. <paramref name="cacheName"/> cannot be null, empty or whitespace.
        /// </exception>
        internal CachingEntityAccessControlChecker(IEntityAccessControlChecker entityAccessControlChecker, string cacheName = "Access control")
            : base(entityAccessControlChecker, cacheName)
        {
        }

        /// <summary>
        /// Create a per-user cache key.
        /// </summary>
        protected override UserEntityPermissionTuple CreateKey( long userId, long entityId, IEnumerable<long> permissionIds )
        {
            return new UserEntityPermissionTuple( userId, entityId, permissionIds );
        }
    }
}
