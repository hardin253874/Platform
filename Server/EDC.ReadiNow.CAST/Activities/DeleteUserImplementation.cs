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
    /// CAST workflow activity that deletes a user from a tenant on a remote platform installation.
    /// </summary>
    public class DeleteUserImplementation : CastActivityImplementation<UserOperationRequest, UserInfoResponse>
    {
        private IPlatformService PlatformService { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public DeleteUserImplementation()
        {
            PlatformService = Factory.Current.Resolve<IPlatformService>();
        }

        /// <summary>
        /// The alias of the input argument to the activity that will receive the database id value.
        /// </summary>
        protected override string DatabaseIdInputAlias
        {
            get { return "cast:inDeleteUserActivityDatabaseId"; }
        }

        /// <summary>
        /// The alias of the exit point that will be used in case of any failure.
        /// </summary>
        protected override string FailureExitPointAlias
        {
            get { return "cast:exitPointDeleteUserFailure"; }
        }

        /// <summary>
        /// Builds a request from the current state of the workflow run to initiate the appropriate action remotely.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="inputs">The activity input arguments.</param>
        /// <returns>The request object.</returns>
        protected override UserOperationRequest GetRequest(IRunState context, ActivityInputs inputs)
        {
            var userName = GetArgumentValue<string>(inputs, UserNameArgumentAlias);
            var userTenant = GetArgumentValue<string>(inputs, UserTenantArgumentAlias);

            return new UserOperationRequest
            {
                Operation = Operation.Delete,
                User = userName,
                Tenant = userTenant
            };
        }

        /// <summary>
        /// Handles the response that was received from a request this activity made.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="request">The original request object.</param>
        /// <param name="response">The object that was received in response to the request.</param>
        protected override void OnResponse(IRunState context, UserOperationRequest request, UserInfoResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            var databaseId = request.DatabaseId;
            if (string.IsNullOrEmpty(databaseId))
            {
                throw new WorkflowRunException("The platform context for this user was unknown.");
            }

            PlatformService.DeleteUser(databaseId, request.Tenant, request.User);
        }

        #region Internals

        internal static string UserNameArgumentAlias { get { return "cast:inDeleteUserActivityUserName"; } }
        
        internal static string UserTenantArgumentAlias { get { return "cast:inDeleteUserActivityUserTenant"; } }

        #endregion
    }
}
