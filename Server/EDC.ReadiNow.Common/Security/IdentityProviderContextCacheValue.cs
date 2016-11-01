// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Identity provider context cache value.
    /// </summary>
    internal class IdentityProviderContextCacheValue
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestContextData"></param>
        /// <param name="accountStatus"></param>
        public IdentityProviderContextCacheValue(RequestContextData requestContextData, long accountStatus)
        {
            if (requestContextData == null)
            {
                throw new ArgumentNullException(nameof(requestContextData));
            }

            RequestContextData = requestContextData;
            AccountStatus = accountStatus;
        }

        /// <summary>
        /// Request context data.
        /// </summary>
        public RequestContextData RequestContextData { get; }

        /// <summary>
        /// Account status id.
        /// </summary>
        public long AccountStatus { get; }
    }
}