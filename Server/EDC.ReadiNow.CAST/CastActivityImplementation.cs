// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading.Tasks;
using Autofac;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using EDC.Remote;
using EDC.SoftwarePlatform.Activities;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// A base implementation for any CAST activity that is required to initiate a remote request.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The expected response type.</typeparam>
    public abstract class CastActivityImplementation<TRequest, TResponse> : ActivityImplementationBase,
        IResumableActivity, IRemoteResponseHandler<TRequest, TResponse>
        where TRequest : CastRequest
        where TResponse : CastResponse
    {
        #region Private Properties

        private IRemoteSender Sender { get; set; }

        private IEntityRepository EntityRepository { get; set; }
        
        private IWorkflowRunner WorkflowRunner { get; set; }
        
        private IActivityLogWriter ActivityLogWriter { get; set; }

        private ICastService CastService { get; set; }

        #endregion

        /// <summary>
        /// The alias of the input argument to the activity that will receive the database id value.
        /// </summary>
        protected abstract string DatabaseIdInputAlias { get; }

        /// <summary>
        /// The alias of the exit point that will be used in case of any failure.
        /// </summary>
        protected abstract string FailureExitPointAlias { get; }

        /// <summary>
        /// Protected constructor.
        /// </summary>
        protected CastActivityImplementation()
        {
            Sender = Factory.Current.Resolve<IRemoteSender>();
            CastService = Factory.Current.Resolve<ICastService>();
            EntityRepository = Factory.Current.Resolve<IEntityRepository>();
            WorkflowRunner = Factory.Current.Resolve<IWorkflowRunner>();
            ActivityLogWriter = Factory.Current.Resolve<IActivityLogWriter>();
        }

        /// <summary>
        /// Builds a request from the current state of the workflow run to initiate the appropriate action remotely.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="inputs">The activity input arguments.</param>
        /// <returns>The request object.</returns>
        protected abstract TRequest GetRequest(IRunState context, ActivityInputs inputs);

        /// <summary>
        /// Handles the response that was received from a request this activity made.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="request">The original request object.</param>
        /// <param name="response">The object that was received in response to the request.</param>
        protected abstract void OnResponse(IRunState context, TRequest request, TResponse response);

        #region IResumableActivity

        /// <summary>
        /// Responds to an activity being started.
        /// </summary>
        /// <param name="context">The current running context of the workflow.</param>
        /// <param name="inputs">Any inputs that have been provided to the activity by the workflow.</param>
        /// <returns>True if the activity has completed, false if it is paused.</returns>
        public bool OnStart(IRunState context, ActivityInputs inputs)
        {
            var done = false;

            using (Profiler.Measure("CastActivityImplementation.OnStart"))
            {
                try
                {
                    LogToRun(context, string.Format("CAST Activity '{0}' is starting.", ActivityInstance.Name));

                    var dbid = string.Empty;

                    if (!string.IsNullOrEmpty(DatabaseIdInputAlias))
                    {
                        dbid = GetArgumentValue<string>(inputs, DatabaseIdInputAlias);
                    }

                    if (string.IsNullOrEmpty(dbid))
                    {
                        throw new WorkflowRunException("Unable to determine client communication key.");
                    }

                    var request = GetRequest(context, inputs);

                    request.Type = request.GetType().AssemblyQualifiedName;
                    request.DatabaseId = dbid;
                    request.RunStep = context.StepsTakenInSession;

                    // send the request after the workflow run has been saved
                    context.SetPostRunAction(() =>
                    {
                        request.RunId = context.WorkflowRunId;

                        Sender.Request(SpecialStrings.CastClientKeyPrefix + dbid.ToLowerInvariant(), request, this);
                    });
                }
                catch (Exception e)
                {
                    EventLog.Application.WriteError("An unexpected error occurred when starting a CAST activity. {0}", e.ToString());

                    done = true;

                    LogToRun(context, string.Format("CAST Activity '{0}' failed to start.", ActivityInstance.Name));

                    if (string.IsNullOrEmpty(FailureExitPointAlias))
                    {
                        throw;
                    }

                    context.ExitPointId = new EntityRef(FailureExitPointAlias);
                }
            }

            return done;
        }

        /// <summary>
        /// Responds to a paused activity being resumed.
        /// </summary>
        /// <param name="context">The current running context of the workflow.</param>
        /// <param name="resumeEvent">Information about the event that caused the resumption of the activity.</param>
        /// <returns>True if the activity has completed, false if it is paused.</returns>
        public bool OnResume(IRunState context, IWorkflowEvent resumeEvent)
        {
            TRequest request = null;
            TResponse response = null;

            using (Profiler.Measure("CastActivityImplementation.OnResume"))
            {
                try
                {
                    if (resumeEvent is ICastActivityResponseEvent)
                    {
                        LogToRun(context, string.Format("CAST Activity '{0}' is resuming.", ActivityInstance.Name));

                        var castEvent = resumeEvent as CastActivityResponseEvent<TRequest, TResponse>;
                        if (castEvent != null)
                        {
                            request = castEvent.Request;
                            response = castEvent.Response;
                        }

                        if (response != null && response.IsError)
                        {
                            throw new WorkflowRunException("CAST Activity failed at the remote end. ({0})", response.Error);
                        }

                        OnResponse(context, request, response);
                    }
                }
                catch (Exception e)
                {
                    EventLog.Application.WriteError("An unexpected error occurred when resuming a CAST activity. {0}", e.ToString());

                    LogToRun(context, string.Format("CAST Activity '{0}' failed to resume.", ActivityInstance.Name));

                    if (string.IsNullOrEmpty(FailureExitPointAlias))
                    {
                        throw;
                    }

                    context.ExitPointId = new EntityRef(FailureExitPointAlias);
                }
            }

            return true;
        }

        #endregion

        #region IRemoteResponseHandler

        /// <summary>
        /// Handles the receipt of a response message coming from a remote platform by resuming the workflow run and handing the
        /// messages to the relevant CAST activity implementation.
        /// </summary>
        /// <param name="request">The original request message.</param>
        /// <param name="response">The response message to process.</param>
        public async void Process(TRequest request, TResponse response)
        {
            using (new DeferredChannelMessageContext())
            using (CastService.GetCastContext())
            {
                try
                {
                    var run = EntityRepository.Get<WorkflowRun>(request.RunId, WorkflowRunPreload);
                    if (run == null)
                    {
                        throw new Exception(string.Format("CAST workflow activity could not resume. Workflow run ({0}) not found.", request.RunId));
                    }

                    // let the workflow or caching or whatever, catch up before officially responding to RabbitMQ
                    if (run.RunStepCounter <= request.RunStep)
                    {
                        var state = await WaitForRunStep(run.Id, request.RunStep);
                        if (state < request.RunStep)
                            throw new Exception(string.Format("CAST workflow activity could not resume. Workflow run ({0}) wasn't in step.", request.RunId));
                    }

                    var castEvent = new CastActivityResponseEvent<TRequest, TResponse>(request, response);

                    WorkflowRunner.ResumeWorkflowAsync(run, castEvent);
                }
                catch (Exception err)
                {
                    EventLog.Application.WriteError("Failed to process CAST response. {0}", err);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Logs a message to the workflow run.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="message">The message to log.</param>
        private void LogToRun(IRunState context, string message)
        {
            var activityName = string.Empty;

            var wf = context != null ? context.GetSafeWorkflowDescription() : "";

            EventLog.Application.WriteTrace(wf + message);

            if (context == null || context.WorkflowRun == null)
            {
                return;
            }

            if (context.CurrentActivity != null)
            {
                activityName = context.CurrentActivity.Name;
            }

            var logEntry = new LogActivityLogEntry
            {
                Name = activityName,
                Description = message
            };

            ActivityLogWriter.WriteLogEntry(logEntry.As<TenantLogEntry>());
        }

        /// <summary>
        /// Waits asynchronously for the run step of the 
        /// </summary>
        /// <param name="id">The id of the workflow run.</param>
        /// <param name="stepToWaitFor">Step count to wait for.</param>
        /// <returns>A task that returns the state of the step when waiting stopped.</returns>
        private async Task<int> WaitForRunStep(long id, int stepToWaitFor)
        {
            var attempts = 10;
            var step = -1;

            while (step < stepToWaitFor && attempts > 0)
            {
                attempts--;

                await Task.Delay(200);

                var run = EntityRepository.Get<WorkflowRun>(id, WorkflowRunPreload);
                if (run != null && run.RunStepCounter != null)
                    step = run.RunStepCounter.Value;
            }

            return step;
        }

        private const string WorkflowRunPreload = "name, isOfType.{alias, name}, runStepCounter";

        #endregion
    }
}
