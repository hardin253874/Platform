// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities;

namespace EDC.ReadiNow.CAST.Activities
{
    /// <summary>
    /// CAST workflow activity that creates a user within a tenant on a remote platform installation.
    /// </summary>
    public class CreateUserImplementation : CastActivityImplementation<UserOperationRequest, UserInfoResponse>
    {
        private IPlatformService PlatformService { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public CreateUserImplementation()
        {
            PlatformService = Factory.Current.Resolve<IPlatformService>();
        }

        /// <summary>
        /// The alias of the input argument to the activity that will receive the database id value.
        /// </summary>
        protected override string DatabaseIdInputAlias
        {
            get { return "cast:inCreateUserActivityDatabaseId"; }
        }

        /// <summary>
        /// The alias of the exit point that will be used in case of any failure.
        /// </summary>
        protected override string FailureExitPointAlias
        {
            get { return "cast:exitPointCreateUserFailure"; }
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
            var userPassword = GetArgumentValue<string>(inputs, UserPasswordArgumentAlias);
            var userTenant = GetArgumentValue<string>(inputs, UserTenantArgumentAlias);
            var userRoles = GetArgumentValue<IEnumerable<IEntity>>(inputs, UserRolesArgumentAlias);

            var roles = new RoleNames();
            if (userRoles != null)
            {
                var managedRoles = userRoles.Select(u => u.As<ManagedRole>());
                roles.AddRange(managedRoles.Where(m => m != null).Select(m => m.Name));
            }

            return new UserOperationRequest
            {
                Operation = Operation.Create,
                User = userName,
                Password = userPassword,
                Tenant = userTenant,
                Roles = roles
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

            var userInfo = response.Users.FirstOrDefault();
            if (userInfo == null)
            {
                throw new WorkflowRunException("No information about the user was received.");
            }

            var user = PlatformService.CreateOrUpdateUser(databaseId, request.Tenant, userInfo);
            if (user == null)
            {
                throw new WorkflowRunException("Failed to create or update this user entity.");
            }

            var userKey = GetArgumentKey(UserArgumentAlias);

            context.SetArgValue(ActivityInstance, userKey, user);
        }

        #region Internals

        internal static string UserNameArgumentAlias { get { return "cast:inCreateUserActivityUserName"; } }

        internal static string UserPasswordArgumentAlias { get { return "cast:inCreateUserActivityUserPassword"; } }

        internal static string UserTenantArgumentAlias { get { return "cast:inCreateUserActivityUserTenant"; } }

        internal static string UserRolesArgumentAlias { get { return "cast:inCreateUserActivityUserRoles"; } }

        internal static string UserArgumentAlias { get { return "cast:outCreateUserActivityUser"; } }

        #endregion
    }
}
