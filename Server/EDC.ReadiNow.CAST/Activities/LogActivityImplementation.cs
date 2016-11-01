// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Contracts;
using EDC.SoftwarePlatform.Activities;

namespace EDC.ReadiNow.CAST.Activities
{
    /// <summary>
    /// CAST workflow activity that logs a message on a remote platform installation.
    /// </summary>
    public class LogActivityImplementation : CastActivityImplementation<LogRequest, LogResponse>
    {
        /// <summary>
        /// The alias of the input argument to the activity that will receive the database id value.
        /// </summary>
        protected override string DatabaseIdInputAlias
        {
            get { return "cast:inLogActivityDatabaseId"; }
        }

        /// <summary>
        /// The alias of the exit point that will be used in case of any failure.
        /// </summary>
        protected override string FailureExitPointAlias
        {
            get { return "cast:exitPointLogFailure"; }
        }

        /// <summary>
        /// Builds a request from the current state of the workflow run to initiate the appropriate action remotely.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="inputs">The activity input arguments.</param>
        /// <returns>The request object.</returns>
        protected override LogRequest GetRequest(IRunState context, ActivityInputs inputs)
        {
            var message = GetArgumentValue<string>(inputs, MessageArgumentAlias);

            return new LogRequest
            {
                Message = message
            };
        }

        /// <summary>
        /// Handles the response that was received from a request this activity made.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="request">The original request object.</param>
        /// <param name="response">The object that was received in response to the request.</param>
        protected override void OnResponse(IRunState context, LogRequest request, LogResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            if (response.Time == DateTime.MinValue)
                throw new ArgumentException("response.time value was not set.");

            var logTimeKey = GetArgumentKey(LogTimeArgumentAlias);

            context.SetArgValue(ActivityInstance, logTimeKey, response.Time.ToLocalTime());
        }

        #region Internals

        internal static string MessageArgumentAlias { get { return "cast:inLogActivityMessage"; } }

        internal static string LogTimeArgumentAlias { get { return "cast:outLogActivityLogTime"; } }

        #endregion
    }
}
