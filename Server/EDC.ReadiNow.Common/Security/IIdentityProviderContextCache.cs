// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    ///     Interface IIdentityProviderRequestContextCache
    /// </summary>
    public interface IIdentityProviderRequestContextCache
    {
        /// <summary>
        ///     Gets the request context data.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="identityProviderId"></param>
        /// <param name="identityProviderUserName"></param>
        /// <param name="ensureAccountIsActive"></param>
        /// <returns>RequestContextData.</returns>
        RequestContextData GetRequestContextData(long tenantId, long identityProviderId, string identityProviderUserName, bool ensureAccountIsActive);
    }
}