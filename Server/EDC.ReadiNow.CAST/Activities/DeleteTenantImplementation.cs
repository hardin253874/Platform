// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using Autofac;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Core;
using EDC.SoftwarePlatform.Activities;

namespace EDC.ReadiNow.CAST.Activities
{
    /// <summary>
    /// CAST workflow activity that deletes a tenant on a remote platform installation.
    /// </summary>
    public class DeleteTenantImplementation : CastActivityImplementation<TenantOperationRequest, TenantInfoResponse>
    {
        private IPlatformService PlatformService { get; set; }

        /// <summary>
        /// The alias of the input argument to the activity that will receive the database id value.
        /// </summary>
        protected override string DatabaseIdInputAlias
        {
            get { return "cast:inDeleteTenantActivityDatabaseId"; }
        }

        /// <summary>
        /// The alias of the exit point that will be used in case of any failure.
        /// </summary>
        protected override string FailureExitPointAlias
        {
            get { return "cast:exitPointDeleteTenantFailure"; }
        }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public DeleteTenantImplementation()
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
            var tenantName = GetArgumentValue<string>(inputs, TenantNameArgumentAlias);

            return new TenantOperationRequest
            {
                Operation = Operation.Delete,
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

            PlatformService.DeleteTenant(databaseId, request.Name);
        }

        #region Internals

        internal static string TenantNameArgumentAlias { get { return "cast:inDeleteTenantActivityTenantName"; } }

        #endregion
    }
}
