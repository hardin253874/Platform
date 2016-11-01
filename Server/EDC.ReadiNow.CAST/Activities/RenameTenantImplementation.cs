// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using Autofac;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Core;
using EDC.SoftwarePlatform.Activities;

namespace EDC.ReadiNow.CAST.Activities
{
    /// <summary>
    /// CAST workflow activity that renames a tenant on a remote platform installation.
    /// </summary>
    public class RenameTenantImplementation : CastActivityImplementation<TenantOperationRequest, TenantInfoResponse>
    {
        private IPlatformService PlatformService { get; set; }

        /// <summary>
        /// The alias of the input argument to the activity that will receive the database id value.
        /// </summary>
        protected override string DatabaseIdInputAlias
        {
            get { return "cast:inRenameTenantActivityDatabaseId"; }
        }

        /// <summary>
        /// The alias of the exit point that will be used in case of any failure.
        /// </summary>
        protected override string FailureExitPointAlias
        {
            get { return "cast:exitPointRenameTenantFailure"; }
        }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public RenameTenantImplementation()
        {
            PlatformService = Factory.Current.Resolve<IPlatformService>();
        }

        /// <summary>
        /// Builds a request from the current state of the workflow run to initiate the appropriate action remotely.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="inputs">The activity input arguments.</param>
        /// <returns>The request object.</returns>
        protected override TenantOperationRequest GetRequest(IRunState context, ActivityInputs inputs)
        {
            var tenantId = GetArgumentValue<string>(inputs, TenantRemoteIdArgumentAlias);
            var tenantName = GetArgumentValue<string>(inputs, TenantNameArgumentAlias);

            long id;
            long.TryParse(tenantId, out id);

            return new TenantOperationRequest
            {
                Operation = Operation.Rename,
                Id = id,
                Name = tenantName
            };
        }

        /// <summary>
        /// Handles the response that was received from a request this activity made.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="request">The original request object.</param>
        /// <param name="response">The object that was received in response to the request.</param>
        protected override void OnResponse(IRunState context, TenantOperationRequest request, TenantInfoResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            var databaseId = request.DatabaseId;
            if (string.IsNullOrEmpty(databaseId))
            {
                throw new WorkflowRunException("The platform context for this tenant was unknown.");
            }

            var tenantInfo = response.Tenants.FirstOrDefault();
            if (tenantInfo == null)
            {
                throw new WorkflowRunException("No information about the tenant was received.");
            }

            var tenant = PlatformService.CreateOrUpdateTenant(databaseId, tenantInfo);
            if (tenant == null)
            {
                throw new WorkflowRunException("Failed to create or update this tenant entity.");
            }

            var tenantKey = GetArgumentKey(TenantArgumentAlias);

            context.SetArgValue(ActivityInstance, tenantKey, tenant);
        }

        #region Internals

        internal static string TenantRemoteIdArgumentAlias { get { return "cast:inRenameTenantActivityTenantRemoteId"; } }

        internal static string TenantNameArgumentAlias { get { return "cast:inRenameTenantActivityTenantName"; } }

        internal static string TenantArgumentAlias { get { return "cast:outRenameTenantActivityTenant"; } }

        #endregion
    }
}
