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
    /// CAST workflow activity that installs an application to a tenant on a remote platform installation.
    /// </summary>
    public class DeployApplicationImplementation : CastActivityImplementation<ApplicationOperationRequest, ApplicationInfoResponse>
    {
        private IPlatformService PlatformService { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public DeployApplicationImplementation()
        {
            PlatformService = Factory.Current.Resolve<IPlatformService>();
        }

        /// <summary>
        /// The alias of the input argument to the activity that will receive the database id value.
        /// </summary>
        protected override string DatabaseIdInputAlias
        {
            get { return "cast:inDeployApplicationActivityDatabaseId"; }
        }

        /// <summary>
        /// The alias of the exit point that will be used in case of any failure.
        /// </summary>
        protected override string FailureExitPointAlias
        {
            get { return "cast:exitPointDeployApplicationFailure"; }
        }

        /// <summary>
        /// Builds a request from the current state of the workflow run to initiate the appropriate action remotely.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="inputs">The activity input arguments.</param>
        /// <returns>The request object.</returns>
        protected override ApplicationOperationRequest GetRequest(IRunState context, ActivityInputs inputs)
        {
            var appId = Guid.Empty;
            var appVersion = GetArgumentValue<string>(inputs, ApplicationVersionArgumentAlias);
            var appTenant = GetArgumentValue<string>(inputs, ApplicationTenantArgumentAlias);

            object o;
            if (inputs.TryGetValue(GetArgumentKey(ApplicationIdArgumentAlias), out o))
            {
                if (o != null)
                    appId = (Guid)o;
            }

            if (appId == Guid.Empty)
                throw new WorkflowRunException("Application identifier not found.");

            return new ApplicationOperationRequest
            {
                Operation = ApplicationOperation.Install,
                Tenant = appTenant,
                Id = appId,
                Version = appVersion
            };
        }

        /// <summary>
        /// Handles the response that was received from a request this activity made.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="request">The original request object.</param>
        /// <param name="response">The object that was received in response to the request.</param>
        protected override void OnResponse(IRunState context, ApplicationOperationRequest request, ApplicationInfoResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            var databaseId = request.DatabaseId;
            if (string.IsNullOrEmpty(databaseId))
            {
                throw new WorkflowRunException("The platform context for this user was unknown.");
            }

            var tenant = PlatformService.UpdateInstalledApplications(databaseId, request.Tenant, response.Installed);
            if (tenant == null)
            {
                throw new WorkflowRunException("Cannot update the tenant with apps installed.");
            }

            var installedKey = GetArgumentKey(ApplicationsInstalledArgumentAlias);

            context.SetArgValue(ActivityInstance, installedKey, tenant.HasAppsInstalled.ToList());
        }

        #region Internals

        internal static string ApplicationTenantArgumentAlias { get { return "cast:inDeployApplicationActivityApplicationTenant"; } }

        internal static string ApplicationIdArgumentAlias { get { return "cast:inDeployApplicationActivityApplicationId"; } }

        internal static string ApplicationVersionArgumentAlias { get { return "cast:inDeployApplicationActivityApplicationVersion"; } }

        internal static string ApplicationsInstalledArgumentAlias { get { return "cast:outDeployApplicationApplicationsInstalled"; } }

        #endregion
    }
}
