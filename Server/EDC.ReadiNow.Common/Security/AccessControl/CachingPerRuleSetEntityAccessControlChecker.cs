// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
	/// An entity access control checker that provides a cache for <see cref="RuleSetEntityPermissionTuple"/>.
    /// </summary>
    public class CachingPerRuleSetEntityAccessControlChecker : CachingEntityAccessControlCheckerBase<RuleSetEntityPermissionTuple>
    {
		/// <summary>
		/// Create a new <see cref="CachingPerRuleSetEntityAccessControlChecker" />.
		/// </summary>
		/// <param name="entityAccessControlChecker">The <see cref="IEntityAccessControlChecker" /> used to actually perform checks.
		/// This cannot be null.</param>
		/// <param name="userRuleSetProvider">The user rule set provider.</param>
		/// <param name="cacheName">(Optional) Cache name. If supplied, this cannot be null, empty or whitespace.</param>
		/// <exception cref="System.ArgumentNullException">userRuleSetProvider</exception>
		/// <exception cref="ArgumentNullException"><paramref name="entityAccessControlChecker" /> cannot be null. <paramref name="cacheName" /> cannot be null, empty or whitespace.</exception>
        internal CachingPerRuleSetEntityAccessControlChecker( IEntityAccessControlChecker entityAccessControlChecker, IUserRuleSetProvider userRuleSetProvider, string cacheName = "Access control" )
            : base( entityAccessControlChecker, cacheName )
        {
            if ( userRuleSetProvider == null )
                throw new ArgumentNullException( "userRuleSetProvider" );

            UserRuleSetProvider = userRuleSetProvider;
        }

        /// <summary>
        /// Service to calculate user-rule-set objects for userIds.
        /// </summary>
        public IUserRuleSetProvider UserRuleSetProvider { get;  }

        /// <summary>
        /// Create a per-user cache key.
        /// </summary>
        protected override RuleSetEntityPermissionTuple CreateKey( long userId, long entityId, IEnumerable<long> permissionIds )
        {
            long mostSpecificPermission = Permissions.MostSpecificPermission( permissionIds );

            UserRuleSet userRuleSet;

            using ( new CacheContext( ContextType.None ) )
            {
                userRuleSet = UserRuleSetProvider.GetUserRuleSet( userId, new EntityRef( mostSpecificPermission ) );
            }

            return new RuleSetEntityPermissionTuple( userRuleSet, entityId, permissionIds );
        }
    }
}
