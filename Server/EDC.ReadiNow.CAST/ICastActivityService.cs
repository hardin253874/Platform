// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// Defines behavior that a CAST server may request be performed by a <see cref="ManagedPlatform"/>.
    /// </summary>
    public interface ICastActivityService
    {
        /// <summary>
        /// Performs a simple write to the event log.
        /// </summary>
        /// <param name="logRequest">The log request.</param>
        /// <returns>The response to the request.</returns>
        LogResponse Log(LogRequest logRequest);

        /// <summary>
        /// Defines a contract for initiating <see cref="Tenant"/> based operations remotely.
        /// </summary>
        /// <param name="tenantRequest">The request.</param>
        /// <returns>The response.</returns>
        TenantInfoResponse TenantOperation(TenantOperationRequest tenantRequest);

        /// <summary>
        /// Defines a contract for initiating <see cref="UserAccount"/> based operations remotely.
        /// </summary>
        /// <param name="userRequest">The request.</param>
        /// <returns>The response.</returns>
        UserInfoResponse UserOperation(UserOperationRequest userRequest);

        /// <summary>
        /// Defines a contract for initiating <see cref="App"/> based operations remotely.
        /// </summary>
        /// <param name="appRequest">The request.</param>
        /// <returns>The response.</returns>
        ApplicationInfoResponse ApplicationOperation(ApplicationOperationRequest appRequest);
    }
}
