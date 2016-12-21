// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using EDC.Monitoring;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.Request;
using EDC.ReadiNow.Diagnostics.Response;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Monitoring.Workflow;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Security;

using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using EDC.ReadiNow.Common.Workflow;
using EDC.SoftwarePlatform.Activities.Engine.Context;
using EDC.ReadiNow.Core;
using EDC.SoftwarePlatform.Activities.Engine;
using Autofac;
using EDC.ReadiNow.Model.Interfaces;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using EDC.ReadiNow.BackgroundTasks;


namespace EDC.SoftwarePlatform.Activities
{
    public class WorkflowRunner : IWorkflowRunner
    {
        const long SuspenedTimeoutMsDefault = 10000;

        public static IWorkflowRunner Instance
        {
            get
            {
                return Factory.Current.Resolve<IWorkflowRunner>();
            }
        }

        private static ISingleInstancePerformanceCounterCategory perfCounters = new SingleInstancePerformanceCounterCategory(WorkflowPerformanceCounters.CategoryName);


        /// <summary>
        /// The workflow metadata factory. (Typically the caching one).
        /// </summary>
        internal IWorkflowMetadataFactory MetadataFactory
        {
            get;
            private set;
        }

        /// <summary>
        /// How long to let a workflow run until we suspend it.
        /// </summary>
        public long SuspendTimeoutMs { get; set; }

        /// <summary>
        /// Initializes the <see cref="WorkflowRunner.Instance"/> class.
        /// </summary>
        public WorkflowRunner(IWorkflowMetadataFactory metadataFactory, long suspendTimeoutMs = SuspenedTimeoutMsDefault)
        {
            if (metadataFactory == null)
                throw new ArgumentNullException("metadataFactory");

            MetadataFactory = metadataFactory;

            WorkflowRequest.WorkflowDiagnosticsRequestReceived += WorkflowRequest_WorkflowDiagnosticsRequestReceived;

            DiagnosticsEnabled = WorkflowRequest.IsEnabled;
            DiagnosticsLastUpdated = DateTime.UtcNow;

            SuspendTimeoutMs = suspendTimeoutMs;
        }



        /// <summary>
        /// Handles the WorkflowDiagnosticsRequestReceived event of the WorkflowRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ReadiNow.Diagnostics.Request.WorkflowRequest.WorkflowRequestEventArgs"/> instance containing the event data.</param>
        void WorkflowRequest_WorkflowDiagnosticsRequestReceived( object sender, WorkflowRequest.WorkflowRequestEventArgs e )
		{
			DiagnosticsEnabled = e.Enabled;

			DiagnosticsLastUpdated = e.Enabled ? DateTime.UtcNow : DateTime.MinValue;
		}

	    /// <summary>
		/// Gets or sets a value indicating whether diagnostics are enabled.
		/// </summary>
		/// <value>
		/// <c>true</c> if diagnostics are enabled; otherwise, <c>false</c>.
		/// </value>
	    private bool DiagnosticsEnabled
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Gets or sets the diagnostics last updated.
		/// </summary>
		/// <value>
		/// The diagnostics last updated.
		/// </value>
	    private DateTime DiagnosticsLastUpdated
	    {
		    get;
		    set;
	    }


        /// <summary>
        /// Factory to create a workflow run , the workflow starts async
        /// </summary>
        /// <param name="workflowToRun"></param>
        /// <param name="args"></param>
        /// <param name="trace"></param>
        /// <param name="invoker"></param>
        /// <returns>The workflow run task ID</returns>
        public string RunWorkflowAsync(WorkflowStartEvent startEvent)
        {
            if (startEvent == null)
                throw new ArgumentNullException(nameof(startEvent));

            if (startEvent.Workflow == null)
                throw new ArgumentException("Missing mandatory argument workflow to on WorkflowStartEvent.");

            using (Profiler.Measure("WorkflowRunner.Instance.StartWorkflowAsync"))
            {
                using (new SecurityBypassContext())
                {
                    if (startEvent.Workflow.WfNewerVersion != null)
                        throw new ArgumentException("Attempted to run a workflow that is not the newest version.");
                }

                // create a wf run then pass into the workflow
                var run = new WorkflowRunDeferred(startEvent.Workflow)
                { 
                    RunTrace = startEvent.Trace
                };

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                Factory.WorkflowRunTaskManager.RegisterStart(run.TaskId);

				HandleDiagnostics( run, WorkflowRunState_Enumeration.WorkflowRunStarted.ToString( ) );

                WorkflowRunContext.Current.QueueAction(() =>
                {
                    stopWatch.Stop();

                    perfCounters.GetPerformanceCounter<AverageTimer32PerformanceCounter>(WorkflowPerformanceCounters.QueueDurationCounterName).AddTiming(stopWatch);

                    using (new WorkflowRunContext(true) { RunTriggersInCurrentThread = true }) // need to ensure that all deferred saves occur before we register complete.
                    {
                        ProcessWorkflowInContext(run, startEvent);
                    }
                });

                return run.TaskId;
                
            }
        }

       
        /// <summary>
        /// Start a workflow and and wait for it to complete or pause.
        /// </summary>
        /// <returns></returns>
        public WorkflowRun RunWorkflow(WorkflowStartEvent startEvent)
        {
            if (startEvent.Workflow == null)
                throw new ArgumentException($"{nameof(startEvent)} missing mandatory field Workflow.");

            // deny workflow run when tenant is disabled
            if (TenantHelper.IsDisabled())
            {
                EventLog.Application.WriteWarning("Workflow run denied, tenant is disabled. \"{0\"({1})", startEvent.Workflow.Name, startEvent.Workflow.Id);
                return null;
            }

            using (Profiler.Measure("WorkflowRunner.Instance.RunWorkflow"))
            {
                using (new SecurityBypassContext())
                {
                    if (startEvent.Workflow.WfNewerVersion != null)
                        throw new ArgumentException("Attempted to run a workflow that is not the newest version.");


                    // create a wf run then pass into the workflow
                    var run = new WorkflowRunDeferred(startEvent.Workflow, startEvent.Trigger)
                    {
                        RunTrace = startEvent.Trace,
                        ParentRun = startEvent.ParentRun
                    };

                    return ProcessWorkflowInContext(run, startEvent);
                }
            }
        }

        private Stopwatch StartWorkflowTimer()
        {
            var stopWatch = new Stopwatch();
            perfCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(WorkflowPerformanceCounters.RunRateCounterName).Increment();
            perfCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(WorkflowPerformanceCounters.RunCountCounterName).Increment();
            stopWatch.Start();

            return stopWatch;
        }

        private void EndWorkflowTimer(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            perfCounters.GetPerformanceCounter<AverageTimer32PerformanceCounter>(WorkflowPerformanceCounters.RunDurationCounterName).AddTiming(stopWatch);
        }

        private WorkflowRun ProcessWorkflowInContext(WorkflowRun run, IWorkflowEvent wfEvent)
        {
            var workflow = run.WorkflowBeingRun;

            using (Profiler.Measure("WorkflowRunner.Instance.StartWorkflowInContext " + workflow.Id))
            {
                var stopWatch = StartWorkflowTimer();

                IRunState runState = null;

                try
                {
                    if (!run.IsTemporaryId)
                        PrecacheWorkflow(run.Id);

                    using (new SecurityBypassContext())
                    {
                        var metadata = MetadataFactory.Create(workflow);

                        runState = CreateRunState(metadata, run); 

                        if (runState.EffectiveSecurityContext == null)
                            throw new WorkflowMissingOwnerException();
                    }

                    // Wrap a Security bypass with the effective context. This less us "Pop" to run as the effective context. 
                    using (CustomContext.SetContext(runState.EffectiveSecurityContext))
                    {
                        using (new SecurityBypassContext())
                        {
                            if (runState.Metadata.HasViolations)
                            {
                                MarkRunFailedHasErrors(runState);
                            }
                            else if (run.TriggerDepth > WorkflowTriggerHelper.MaxTriggerDepth)
                            {
                                MarkRunFailedTriggerDepth(runState);
                            }
                            else
                            {
                                var isCompleted = ProcessWorkflow(workflow, runState, wfEvent);
                            }
                        }
                    }
                }
                catch (WorkflowRunException ex)
                {
                    MarkRunFailed(runState, ex);

                    if (runState != null)
                        runState.FlushInternalArgs();

                }
                catch (Exception ex)
                {
                    MarkRunInternalError(runState, ex);

                    if (runState != null)
                        runState.FlushInternalArgs();

                }
                finally
                {
                    if (!Factory.WorkflowRunTaskManager.HasCancelled(runState.RunTaskId))
                    {
                        run = FinalizeRun(runState);
                    }
                }

                EndWorkflowTimer(stopWatch);
            }

            return run;
        }

        private IRunState CreateRunState(WorkflowMetadata metadata, WorkflowRun run)
        {
            var factory = Factory.Current.Resolve<IRunStateFactory>();
            return factory.CreateRunState(metadata, run);
        }

        private WorkflowRun FinalizeRun(IRunState runState)
        {
            WorkflowRun run;

            using (Profiler.Measure("WorkflowRunner.Instance.FinalizeRun"))
            {
                using (new SecurityBypassContext())
                {
                    try
                    {
                        run = runState.WorkflowRun;
                        runState.CompletedAt = DateTime.UtcNow;
                        if (!run.IsTemporaryId)
                        {
                            run = Entity.Get<WorkflowRun>(runState.WorkflowRun,
                                true,
                                WorkflowRun.WorkflowRunExitPoint_Field,
                                WorkflowRun.HasTimeout_Field,
                                WorkflowRun.PendingActivity_Field,
                                WorkflowRun.RunStepCounter_Field,
                                WorkflowRun.WorkflowRunStatus_Field,
                                WorkflowRun.RunCompletedAt_Field,
                                WorkflowRun.StateInfo_Field);
                        }

                        var deferredRun = run as WorkflowRunDeferred;
                        if (deferredRun != null)
                            deferredRun.Sync();

                       

                        runState.SyncToRun(run);

                        WorkflowRunContext.Current.DeferSave(run);

                        // 
                        // Raise a completed child event
                        //
                        if (run != null && run.ParentRun != null && IsRunCompleted(run))
                        {
                            // This should be hooked into an eventing system. As we don't have one, just run the resume async. 
                            runState.WorkflowInvoker.PostEvent(new ChildWorkflowCompletedEvent(run));
                        }

                        // 
                        // Add a restore message to the queue if we are suspended
                        //
                        if (run != null && run.WorkflowRunStatus_Enum  == WorkflowRunState_Enumeration.WorkflowRunSuspended)
                        {
                            WorkflowRunContext.Current.DeferAction(() =>
                            {
                                // This should be hooked into an eventing system. As we don't have one, just run the resume async.
                                var restoreTask = ResumeWorkflowHandler.CreateBackgroundTask(run, new WorkflowRestoreEvent());
                                Factory.BackgroundTaskManager.EnqueueTask(restoreTask);
                            });
                        }


                        //
                        // Let the world know we have finished
                        WorkflowRunContext.Current.DeferAction(() =>
                        {
                            if (run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunPaused ||
                                run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCompleted ||
                                run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunFailed)
                            {
                                Factory.WorkflowRunTaskManager.RegisterComplete(run.TaskId, run.Id.ToString(CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                Factory.WorkflowRunTaskManager.SetResult(run.TaskId, run.Id.ToString(CultureInfo.InvariantCulture));

                            }
                            HandleDiagnostics(run, run.WorkflowRunStatus_Enum.ToString());
                        });



                    }
                    catch (Exception ex)
                    {
                        Workflow workflow = runState != null ? Entity.Get<Workflow>(runState.WorkflowRunId) : null;

                        var msg = string.Format("Workflow: {0}. Unexpected error when finalizing the workflow run ({1}), version ({2}).",
                            runState != null ? runState.GetSafeWorkflowDescription() : "",
                            runState != null ? runState.WorkflowRunId : -1L,
                            workflow != null ? workflow.WorkflowVersion : 0);

                        var log = msg + Environment.NewLine + ex.Message;

#if DEBUG
                        log += Environment.NewLine + ex.StackTrace;
#endif

                        EventLog.Application.WriteError(log);

                        throw new Exception(msg, ex);
                    }
                }
            }

            return run;
        }


        bool IsRunCompleted(WorkflowRun run)
        {
           return run.WorkflowRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunPaused 
                && run.WorkflowRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunSuspended
                && run.WorkflowRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunStarted
                && run.WorkflowRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunQueued;
        }

        /// <summary>
        /// Resume workflow, the workflow runs immediatly in the current thread.
        /// </summary>
        /// <param name="run"></param>
        /// <param name="resumeEvent"></param>
        /// <param name="invoker"></param>
        /// <returns></returns>
        public WorkflowRun ResumeWorkflow(WorkflowRun run, IWorkflowEvent resumeEvent)
        {
            if (run == null)
                throw new ArgumentNullException(nameof(run));

            if (resumeEvent == null)
                throw new ArgumentNullException(nameof(resumeEvent));

            HandleDiagnostics(run, "Resumed");
            Factory.WorkflowRunTaskManager.RegisterStart(run.TaskId);

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                run = ProcessWorkflowInContext(run, resumeEvent);
            }
            
            return run;
        }

		/// <summary>
		/// Sends the diagnostics.
		/// </summary>
		/// <param name="workflowRun">The workflow run.</param>
		/// <param name="status">The status.</param>
	    private void SendDiagnostics( WorkflowRun workflowRun, string status )
	    {
            var response = new WorkflowResponse
            {
                TenantName = RequestContext.GetContext().Tenant.Name,
                Id = workflowRun.Id,
                TaskId = workflowRun.TaskId,
                WorkflowName = workflowRun?.WorkflowBeingRun?.Name,
                WorkflowRunName = workflowRun?.Name,
                Status = status ?? (workflowRun.WorkflowRunStatus_Enum != null ? workflowRun.WorkflowRunStatus_Enum.ToString() : "Unknown"),
                Date = DateTime.Now,
                TriggeredBy = workflowRun.TriggeringUser != null ? workflowRun.TriggeringUser.Name : "Unknown",
                Server = Environment.MachineName,
                Process = Process.GetCurrentProcess().MainModule.ModuleName,
                StepCount = workflowRun.RunStepCounter ?? -1,
            };


		    DiagnosticChannel.Publish( response );
	    }

		/// <summary>
		/// Handles the diagnostics.
		/// </summary>
		/// <param name="workflowRun">The workflow run.</param>
		/// <param name="status">The status.</param>
	    private void HandleDiagnostics( WorkflowRun workflowRun, string status )
	    {
			if ( DiagnosticsEnabled )
			{
				if ( DateTime.UtcNow - DiagnosticsLastUpdated > TimeSpan.FromMinutes( 5 ) )
				{
					DiagnosticsEnabled = false;
				}
				else
				{
					SendDiagnostics( workflowRun, status );
				}
			}
	    }

        /// <summary>
        /// Resume a workflow. The workflow will run async unless it is in a RunTriggersInCurrentThread WorkflowContext.
        /// </summary>
        /// <param name="workflowRun"></param>
        /// <param name="resumeEvent"></param>
        public string ResumeWorkflowAsync(WorkflowRun workflowRun, IWorkflowEvent resumeEvent)
        {
            if (Factory.FeatureSwitch.Get("longRunningWorkflow"))
            {
                return ResumeWorkflowAsync_new(workflowRun, resumeEvent);
            }
            else
            {
                return ResumeWorkflowAsync_old(workflowRun, resumeEvent);

            }
        }

        private string ResumeWorkflowAsync_old(WorkflowRun workflowRun, IWorkflowEvent resumeEvent)
        {
            Action runAction = () =>
            {
                    using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
                    {
                        ResumeWorkflow(workflowRun, resumeEvent);
                    }
            };

            Factory.WorkflowRunTaskManager.RegisterStart(workflowRun.TaskId);
            HandleDiagnostics(workflowRun, "Queued");

            if (WorkflowRunContext.Current.RunTriggersInCurrentThread)
            {
                // Ignore the Asyn and process it syncronously
                runAction();
            }
            else // Async
            {
                WorkflowRunContext.Current.QueueAction(() =>
                {
                    runAction();
                });
            }

            return workflowRun.TaskId;
        }

        public string ResumeWorkflowAsync_new(WorkflowRun workflowRun, IWorkflowEvent resumeEvent)
        {
            Factory.WorkflowRunTaskManager.RegisterStart(workflowRun.TaskId);

            bool runInForeground = WorkflowRunContext.Current.RunTriggersInCurrentThread;
            
            var queueEvent = resumeEvent as IWorkflowQueuedEvent;
            
            if (queueEvent == null)
            {
                EventLog.Application.WriteError("Attempted to queue a resume task that cannot be queued, running it on the foreground thread instead. EventType: {resumeEvent.GetType()}");
                runInForeground = true;
            }

            if (runInForeground)
            {
                ResumeWorkflow(workflowRun, resumeEvent);
            }
            else
            {
                var task = ResumeWorkflowHandler.CreateBackgroundTask(workflowRun, queueEvent);

                Factory.BackgroundTaskManager.EnqueueTask(task);

                HandleDiagnostics(workflowRun, "Queued");
            }

            return workflowRun.TaskId;
        }

           
        /// <summary>
        /// Run activity using its own context. The activity can not have any pending actions when it completes)
        /// </summary>
        /// <returns>True if the run completed.</returns>
        bool ProcessWorkflow(Workflow wf, IRunState runState, IWorkflowEvent wfEvent)
        {
            var activityInst = wf.Cast<WfActivity>();
            var activityImp = activityInst.CreateWindowsActivity();

            var metadata = runState.Metadata;
            var invoker = runState.WorkflowInvoker;

            if (!metadata.HasViolations)
            {
                var startEvent = wfEvent as WorkflowStartEvent;

                if (startEvent != null)
                {
                    var inputArgs = activityInst.GetInputArguments();
                    var inputs = new ActivityInputs(inputArgs, startEvent.Arguments);

                    invoker.ScheduleActivity(runState, activityImp, inputs, null, null);
                }
                else
                {
                    invoker.ScheduleResume(runState, activityImp, wfEvent, null, null);
                }

                try
                {
                    var result = RunTillCompletion(invoker, runState);
                    return result;
                }
                catch (PlatformSecurityException ex)
                {
                    throw new WorkflowRunException(ex.Message, ex);
                }
                catch (WorkflowRunException_Internal ex)
                {
                    throw new WorkflowRunException(ex.Message, ex.InnerException);
                }
            }

            var act = GetActivityForError(runState, activityInst);

            throw new WorkflowValidationException(runState.WorkflowRunId, act.ContainingWorkflow, act, metadata.ValidationMessages);
        }


        /// <summary>
        /// Keep running outstanding activities until completion.
        /// </summary>
        /// <returns>True, workflow has completed, False if there are pending activities.</returns>
        bool RunTillCompletion(WorkflowInvoker invoker, IRunState runState)
        {
            bool hasCompleted = invoker.RunTillCompletion(runState);

            return hasCompleted;
        }


        /// <summary>
        /// Get an activity to be used in the error report.
        /// </summary>
        private WfActivity GetActivityForError(IRunState runState, WfActivity activity)
        {
            var act = runState.WorkflowRun != null ? runState.WorkflowRun.PendingActivity : runState.PendingActivity;
            return act ?? activity;
        }


        void MarkRunFailedHasErrors(IRunState runState)
        {
            using (new SecurityBypassContext())
            {
                string msg = $"Failed to start because it is misconfigured: \n{string.Join(", \n", runState.Metadata.ValidationMessages)}";

                var logEntry = new WorkflowRunFailedLogEntry
                {
                    Name = "Workflow misconfigured",
                    Description = msg,
                };

                // error in how the workflow is configured by the user.
                EventLog.Application.WriteInformation(string.Format("Workflow: {0}. {1}", runState.GetSafeWorkflowDescription(), msg));

                if (runState != null)
                {
                    runState.RunStatus = WorkflowRunState_Enumeration.WorkflowRunFailed;
                    runState.LogError(logEntry);
                }
            }
        }

        void MarkRunFailedTriggerDepth(IRunState runState)
        {
            using (new SecurityBypassContext())
            {
                const string msg = "Failed to start because the trigger depth has been exceeded. Check triggers on workflow for looping.";
                var logEntry = new WorkflowRunFailedLogEntry
                {
                    Name = "Workflow exceeded trigger depth",
                    Description = msg,
                };

                // error in how the workflow is configured by the user.
                EventLog.Application.WriteInformation(string.Format("Workflow: {0}. {1}", runState.GetSafeWorkflowDescription(), msg));

                if (runState != null)
                {
                    runState.RunStatus = WorkflowRunState_Enumeration.WorkflowRunFailed;
                    runState.LogError(logEntry);
                }
            }
        }

        void MarkRunFailed(IRunState runState, WorkflowRunException ex)
        {
            using (new SecurityBypassContext())
            {
                var stateString = GetStateString(runState);

                var sb = new StringBuilder("");
                sb.Append(ex.InnerException is PlatformSecurityException ? "Security violation " : "Failed ");
                sb.Append("at ");
                sb.Append(GetSafeWfDescription(runState));
                sb.Append(Environment.NewLine + "With message: ");
                sb.Append(Environment.NewLine + ex.Message);
                sb.AppendFormat("\nState: {0}", stateString);
                
                var log = sb.ToString();

                var shortMessage = ex.Message ?? "";
                var maxNameLength = (int)Resource.Name_Field.MaxLength; // why bother?

                if (shortMessage.Length > maxNameLength)            // The name field is a max of 200 characters
                    shortMessage = shortMessage.Substring(0, maxNameLength);

                var logEntry = new WorkflowRunFailedLogEntry
                {
                    Name = shortMessage,        
                    Description = log,
                    ActivityBeingRun = runState.CurrentActivity,
                };
                
#if DEBUG
                log += Environment.NewLine + ex.StackTrace;
#endif

                // error in how the workflow is configured by the user.
                EventLog.Application.WriteInformation("Workflow: {0}", log);

                if (runState != null)
                {
                    runState.RunStatus = WorkflowRunState_Enumeration.WorkflowRunFailed;
                    runState.LogError(logEntry);
                }
            }
        }

        private void MarkRunInternalError(IRunState runState, Exception ex)
        {
            using (new SecurityBypassContext())
            {
                var safeDescription = GetSafeWfDescription(runState);
                var stateString = GetStateString(runState);

                var logEntry = new WorkflowRunLogEntry
                {
                    Name = "Workflow run internal error",
                    Description = string.Format("Workflow failed with an internal error at {0}.\nState: {1}", safeDescription, stateString)
                };

              
                var msg = string.Format("Workflow run ({0}): {1}. Exception occurred on run.\n{2}\n{3}\nState:\n{4}",                   
                    runState != null ? runState.WorkflowRunId : -1L,
                    safeDescription,
                    ex.Message,
                    ex.StackTrace,
                    stateString);

                EventLog.Application.WriteError(msg);

                if (runState != null)
                {
                    runState.RunStatus = WorkflowRunState_Enumeration.WorkflowRunFailed;
                    runState.LogError(logEntry);
                }
            }
        }

        private string GetStateString(IRunState runState)
        {
            try {
                var sb = new StringBuilder();
                runState.PrettyPrint(sb);
                return sb.ToString();
            }
            catch(Exception ex)
            {
                EventLog.Application.WriteError(ex.ToString());
                return "Unable to retrieve workflow state information";
            }
        }

        private string GetSafeWfDescription(IRunState runState)
        {
            return runState != null ? runState.GetSafeWorkflowDescription() : "";
        }

        

        /// <summary>
        /// Prefetch the workflow run and related workflow an activities.
        /// </summary>
        /// <param name="workflowRunId"></param>
        private void PrecacheWorkflow(long workflowRunId)
        {
            var stateInfoRequest = @"
                {stateInfoArgument, stateInfoActivity}.{isOfType.id, name},  
                stateInfoValue.isOfType.id";

            var request = @"
                isOfType.id,
                workflowRunStatus.id, workflowHasErrors, errorLogEntry.isOfType.id, runCompletedAt, triggerDepth,
                parentRun.isOfType.id,
                workflowRunExitPoint.isOfType.id, stateInfo.{" + stateInfoRequest + @"}, 
                workflowBeingRun.id";       // no need to prefetch the workflow as it is already fetched during metadata creation

            BulkPreloader.Preload(new EntityRequest(workflowRunId, request, "Preload workflow run " + workflowRunId.ToString()));
        }
              
    }
}
