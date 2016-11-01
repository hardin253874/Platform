// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    ///     Class IdentityProviderContextCacheKey.
    /// </summary>
    internal class IdentityProviderContextCacheKey : IEquatable<IdentityProviderContextCacheKey>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IdentityProviderContextCacheKey" /> class.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="identityProviderId">The identity provider identifier.</param>
        /// <param name="identityProviderUserName">The identity provider user name.</param>
        /// <exception cref="System.ArgumentException">
        ///     @The tenant is invalid;tenantId
        ///     or
        ///     @The identity provider is invalid;identityProviderId
        /// </exception>
        /// <exception cref="System.ArgumentNullException">identityProviderUserIdentity</exception>
        public IdentityProviderContextCacheKey(long tenantId, long identityProviderId, string identityProviderUserName)
        {
            if (tenantId <= 0)
            {
                throw new ArgumentException(@"The tenant is invalid", nameof(tenantId));
            }

            if (identityProviderId <= 0)
            {
                throw new ArgumentException(@"The identity provider is invalid", nameof(identityProviderId));
            }

            if (string.IsNullOrWhiteSpace(identityProviderUserName))
            {
                throw new ArgumentNullException(nameof(identityProviderUserName));
            }

            TenantId = tenantId;
            IdentityProviderId = identityProviderId;
            IdentityProviderUserName = identityProviderUserName;
        }

        /// <summary>
        ///     Gets the tenant identifier.
        /// </summary>
        /// <value>The tenant identifier.</value>
        public long TenantId { get; }


        /// <summary>
        ///     Gets the identity provider identifier.
        /// </summary>
        /// <value>The identity provider identifier.</value>
        public long IdentityProviderId { get; }


        /// <summary>
        ///     Gets the identity provider user identity.
        /// </summary>
        /// <value>The identity provider user identity.</value>
        public string IdentityProviderUserName { get; }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(IdentityProviderContextCacheKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TenantId == other.TenantId &&
                   IdentityProviderId == other.IdentityProviderId &&
                   IdentityProviderUserName == other.IdentityProviderUserName;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IdentityProviderContextCacheKey) obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = hash*92821 + TenantId.GetHashCode();

                hash = hash*92821 + IdentityProviderId.GetHashCode();

                hash = hash*92821 + IdentityProviderUserName.GetHashCode();

                return hash;
            }
        }

        /// <summary>
        ///     Implements the ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(IdentityProviderContextCacheKey left, IdentityProviderContextCacheKey right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Implements the !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(IdentityProviderContextCacheKey left, IdentityProviderContextCacheKey right)
        {
            return !Equals(left, right);
        }
    }
}