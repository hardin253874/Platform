// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Context for a workflow run. Holds the variables values and properties as well as the invoker.
    /// The RunState caches writable values so that a writable reference to the workflowRun is not required until the state is flushed. This reduces the amount of blocking
    /// experienced.
    /// </summary>
    public abstract class RunStateBase : IRunState
    {
        const int MaxParamPrintLength = 50;
        const int MaxTraceSteps = 500;              // the maximum number of trace steps that will be recorded


        /// <summary>
        /// This is set when the context is created or recreated
        /// </summary>
        public long WorkflowRunId { get { return WorkflowRun.Id; } }

        public string RunTaskId { get { return WorkflowRun.TaskId; } }

        public long WorkflowId { get { return WorkflowRun.WorkflowBeingRun.Id; } }

        public int StepsTakenInSession { get; set; }


        public System.Diagnostics.Stopwatch TimeTakenInSession { get; }

        public RequestContextData EffectiveSecurityContext { get; private set; }
        public WorkflowMetadata Metadata { get; private set; }
        public WorkflowRun WorkflowRun { get; private set; }
        public WorkflowInvoker WorkflowInvoker { get; private set; }

        /// <summary>
        /// Write cached values, to be synced back to the workflowRun
        /// </summary>
        public EntityRef ExitPointId {get; set;}
        public bool HasTimeout { get; set;}
        public WfActivity PendingActivity { get; set; }

        /// <summary>
        /// The current activity being run
        /// </summary>
        public WfActivity CurrentActivity { get; set; }

        public WorkflowRunState_Enumeration? RunStatus { get; set; }
        public DateTime? CompletedAt { get; set; }

        public abstract IDictionary<string, object> GetResult(IEnumerable<ActivityArgument> args = null);
        
        private RunStateBase(WorkflowMetadata metadata)
        {
            Metadata = metadata;
            TimeTakenInSession = new System.Diagnostics.Stopwatch();
        }

        public RunStateBase(WorkflowMetadata metaData, WorkflowRun _workflowRun, RequestContextData effectiveSecurityContext)
            : this(metaData)
        {
            WorkflowInvoker = new WorkflowInvoker();
            WorkflowRun = _workflowRun;

            StepsTakenInSession = 0;
            EffectiveSecurityContext = effectiveSecurityContext;
            ExitPointId = _workflowRun.WorkflowRunExitPoint;
            HasTimeout = _workflowRun.HasTimeout ?? false;
            PendingActivity = _workflowRun.PendingActivity;
            RunStatus = _workflowRun.WorkflowRunStatus_Enum;
            CompletedAt = _workflowRun.RunCompletedAt;
        }
         
        /// <summary>
        /// Update the provided run with the cached values in the runstate
        /// </summary>
        /// <param name="writableRun"></param>
        public void SyncToRun(WorkflowRun writableRun) 
        {
            using (Profiler.Measure("RunStateBase.SyncToRun"))
            {
                writableRun.WorkflowRunExitPoint = ExitPointId != null ? Entity.Get<ExitPoint>(ExitPointId) : null;
                writableRun.HasTimeout = HasTimeout;
                writableRun.PendingActivity = PendingActivity;
                writableRun.RunStepCounter = writableRun.RunStepCounter.HasValue
                    ? writableRun.RunStepCounter.Value + StepsTakenInSession
                    : StepsTakenInSession;
                writableRun.TotalTimeMs = (writableRun.TotalTimeMs.HasValue ? writableRun.TotalTimeMs.Value : 0) + (int)TimeTakenInSession.ElapsedMilliseconds;
                writableRun.WorkflowRunStatus_Enum = RunStatus;
                writableRun.RunCompletedAt = CompletedAt;

                StepsTakenInSession = 0;
                TimeTakenInSession.Reset();

                var stateInfo = writableRun.StateInfo;
                var deleteList = stateInfo.Select(e => e.Id).ToList();

                stateInfo.Clear();

                Entity.Delete(deleteList);

                // write a message to the log
                var sb = new StringBuilder();

                sb.AppendLine("Saving state to workflow run. Values: ");

                foreach (var cacheValue in GetArgValues())
                {
                    WfActivity activity = cacheValue.Item1;
                    ActivityArgument arg = cacheValue.Item2;
                    object value = cacheValue.Item3;

                    ActivityArgument valueArg = ActivityArgumentHelper.ConvertArgInstValue(activity, arg, value);

                    stateInfo.Add(new StateInfoEntry
                    {
                        Name = valueArg.Name,
                        StateInfoActivity = activity.Id != writableRun.WorkflowBeingRun.Id ? activity : null,
                        // Don't store the workflow as an activity
                        StateInfoArgument = arg,
                        StateInfoValue = valueArg
                    });

                    sb.Append(valueArg.Name);
                    sb.Append(": \t");
                    if (value is IEntity)
                        sb.AppendFormat("Resource {0}\n", ((IEntity) value).Id);
                    else if (value == null)
                        sb.AppendLine("[Null]");
                    else
                        sb.AppendLine(value.ToString());
                }

                EventLog.Application.WriteInformation(sb.ToString());
            }
        }
        
        /// <summary>
        /// Load up any state from the run.
        /// </summary>
        public void SyncFromRun()
        {
            using (Profiler.Measure("RunStateBase.SyncFromRun"))
            {
                var run = WorkflowRun;

                if (run == null)
                {
                    throw new ArgumentNullException("run");
                }

                var sb = new StringBuilder("");
                sb.AppendFormat("Workflow: {0}. Loading state from workflow run ({1}). Values: ",
                    GetSafeWorkflowDescription(), run.Id);
                sb.Append(Environment.NewLine);

                foreach (var entry in run.StateInfo)
                {
                    var activity = entry.StateInfoActivity;
                    var argument = entry.StateInfoArgument;
                    var stateValue = entry.StateInfoValue;

                    var value = ActivityArgumentHelper.GetArgParameterValue(stateValue);

                    if (value != null)
                    {
                        if (activity == null || (run.WorkflowBeingRun != null && activity.Id == run.WorkflowBeingRun.Id))
                        {
                            SetArgValue(argument, value);
                        }
                        else
                        {
                            SetArgValue(activity, argument, value);
                        }

                        var act = "[Empty]";
                        var arg = "[Empty]";
                        if (activity != null)
                        {
                            if (!string.IsNullOrEmpty(activity.Name))
                            {
                                act = activity.Name;
                            }
                            else
                            {
                                act = "[" + activity.Id + "]";
                            }
                        }
                        if (argument != null)
                        {
                            if (!string.IsNullOrEmpty(argument.Name))
                            {
                                arg = argument.Name;
                            }
                            else
                            {
                                arg = "[" + argument.Id + "]";
                            }
                        }

                        sb.AppendLine(string.Format("{0}.{1} ", act, arg));
                    }
                }

                EventLog.Application.WriteInformation(sb.ToString());
            }
        }

        /// <summary>
        ///Produce a string displaying all the values in the workflow run
        /// </summary>
        /// <returns></returns>
        public void PrettyPrint(StringBuilder sb)
        {
            using (Profiler.Measure("RunStateBase.PrettyPrint"))
            {
                var wf = WorkflowRun.WorkflowBeingRun;
                if (wf == null)
                {
                    sb.Append("Workflow is null.");
                }
                else
                {
                    if (wf.InputArguments.Any())
                    {
                        sb.Append("Inputs:\n");
                        PrintParams(wf.InputArguments, sb);
                        sb.AppendLine();
                    }

                    if (wf.Variables.Any())
                    {
                        sb.Append("Variables:\n");
                        PrintParams(wf.Variables, sb);
                        sb.AppendLine();

                    }

                    if (wf.OutputArguments.Any())
                    {
                        sb.Append("Activity Outputs:\n");
                        PrintParams(wf.OutputArguments, sb);
                        sb.AppendLine();

                    }
                }
            }
        }

        /// <summary>
        /// Ensure that the arge value is an accepted type
        /// </summary>
        public bool IsValueArgValueType(object value)
        {
            return value == null
                   || value is IEntity || value is IEnumerable<IEntity>
                   || value is string
                   || value is int || value is decimal
                   || value is bool
                   || value is DateTime
                   || value is TimeSpan
                   || value is Guid;
        }


        /// <summary>
        /// Given a text expression and runtimeArgs, evaluate the expression. 
        /// Expressions must be compatible with DataColumn.Expression. the result is cast to T
        /// </summary>
        public object EvaluateExpression(WfExpression expression)
        {
            using (Profiler.Measure("RunStateBase.EvaluateExpression"))
            {
                ExprType targetType = null;

            SecurityBypassContext.Elevate(() =>  targetType = Metadata.GetExpressionType(expression));

                var result = expression.EvaluateExpression(this);

                // force any lists to be resolved to prevent lazy evaluation in a different security context and get rid of the nulls
                if (targetType.IsList && result != null)
                    result = ((IEnumerable<IEntity>) result).Where(e => e != null).ToList<IEntity>();

                return result;
            }
        }

        /// <summary>
        /// Generate a trace message in the log
        /// </summary>
        public void RecordTrace(string activityName)
        {
            using (Profiler.Measure("RunStateBase.RecordTrace"))
            {
                if ((WorkflowRun.RunTrace ?? false) && (StepsTakenInSession + (WorkflowRun.RunStepCounter ?? 0)) < MaxTraceSteps)
                {
                    var steps = StepsTakenInSession;
                    if (WorkflowRun != null && WorkflowRun.RunStepCounter.HasValue)
                    {
                        steps += WorkflowRun.RunStepCounter.Value;
                    }

                    var sb = new StringBuilder();
                    sb.AppendFormat("Activity: {0}\n", activityName ?? "[Unnamed]");
                    PrettyPrint(sb);

                    Log(new RunTraceLogEntry
                    {
                        Name = "Trace",
                        WorkflowRunBeingLogged = WorkflowRun,
                        Description = sb.ToString(),
                        WorkflowRunTraceStep = steps
                    });                   
                }
            }
        }

        public void Log(IEntity log)
        {
            using (Profiler.Measure("RunStateBase.Log"))
            {
                var wfName = WorkflowRun.WorkflowBeingRun.Name;
                if (String.IsNullOrEmpty(wfName))
                    wfName = "[Unnamed]";

                // Using set field as it is a little faster than a cast
                log.SetField(WorkflowRunLogEntry.WrleWorkflowName_Field, wfName);
                log.SetField(WorkflowRunLogEntry.WrleStepName_Field, CurrentActivity != null ? CurrentActivity.Name : null);
                log.SetField(WorkflowRunLogEntry.LogEventTime_Field, DateTime.UtcNow);

                // Delay logging until the end to ensure all the temporary Ids have been resolved.
                WorkflowRunContext.Current.DeferAction(() =>
                {
                    using (new SecurityBypassContext())
                    {
                        var entry = log.As<WorkflowRunLogEntry>();
                        entry.WorkflowRunBeingLogged = WorkflowRun;
                        Factory.ActivityLogWriter.WriteLogEntry(entry.As<TenantLogEntry>());
                    }
                });
            }
        }

        public void LogError(IEntity log)
        {
            var logEntity = log.As<WorkflowRunLogEntry>();
            logEntity.WorkflowRunInError = WorkflowRun;
            logEntity.LogEntrySeverity = Entity.Get<LogSeverityEnum>("errorSeverity");

            Log(logEntity);
        }

        public void SetUserTask(BaseUserTask userTask)
        {
            if (userTask == null)
               throw new ArgumentException();

            WorkflowRun.TaskWithinWorkflowRun.Add(userTask);
        }

        public void SetPostRunAction(Action postRunAction)
        {
            using (Profiler.Measure("RunStateBase.SetPostRunAction"))
            {
                if (postRunAction == null)
                    return;

                var deferredRun = WorkflowRun as WorkflowRunDeferred;
                if (WorkflowRun.IsTemporaryId && deferredRun != null)
                {
                    deferredRun.DeferredActions.Add(postRunAction);
                }
                else
                {
                    postRunAction.Invoke();
                }   
            }
        }

        /// <summary>
        /// Get a description of the workflow even if there are nulls.
        /// </summary>
        /// <returns></returns>
        public string GetSafeWorkflowDescription()
        {
            if (WorkflowRun == null)
                return "[Unknown run]";

            if (WorkflowRun.WorkflowBeingRun == null)
                return string.Format("Workflow run {0}", WorkflowRun.Id);

            var wf = WorkflowRun.WorkflowBeingRun;
            var sb = new StringBuilder();

            sb.AppendFormat("Workflow: '{0}' ({1}), version {2}, Run: {3}", wf.Name ?? "[Unnamed]", wf.Id, wf.WorkflowVersion ?? 1, WorkflowRun.Id);

            if (CurrentActivity != null)
                sb.AppendFormat(", Activity: '{0}' ({1}), version {2}", CurrentActivity.Name ?? "[Unnamed]", wf.WorkflowVersion ?? 1, CurrentActivity.Id);

            sb.AppendFormat(", Run: {0}", WorkflowRun.Id);

            return sb.ToString();
        }

        public abstract void SetArgValue(WfActivity activity, ActivityArgument targetArg, object value);
        public abstract void SetArgValue(ActivityArgument targetArg, object value);
        public abstract T GetArgValue<T>(WfActivity activity, ActivityArgument targetArg);
        public abstract T GetArgValue<T>(ActivityArgument targetArg);
        public abstract IEnumerable<Tuple<WfActivity, ActivityArgument, object>> GetArgValues();
        public abstract void FlushInternalArgs();
        public abstract void RemoveReference(EntityRef removedEntity);    
        public abstract object ResolveParameterValue(string name, IDictionary<string, Resource> knownEntities);

        private void PrintParams(IEnumerable<ActivityArgument> args, StringBuilder sb)
        {
            foreach (var arg in args)
            {
                sb.AppendFormat("    {0}: ", arg.Name);
                var value = GetArgValue<object>(arg);
                if (value != null)
                {
                    string s = null;
                    if (value is IEnumerable<IEntity>)
                    {
                        var count = ((IEnumerable<IEntity>)value).Count();
                        s = count > 0 ? string.Format("{0} items", count) : null;
                    }
                    else
                    {
                        if (value is IEntity)
                            s = "Entity " + ((IEntity)value).Id;
                        else
                            s = value.ToString();
                    }

                    s = s ?? "[Null]";

                    if (s.Length > MaxParamPrintLength)
                        s = s.Substring(0, MaxParamPrintLength - 3) + "...";

                    sb.AppendLine(s);
                }
                else
                {
                    sb.AppendLine("[Null]");
                }

            }
        }
    }
}