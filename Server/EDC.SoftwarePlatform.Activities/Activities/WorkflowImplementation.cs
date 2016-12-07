// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Diagnostics;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using EDC.ReadiNow.Model.Interfaces;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.Activities
{
    public class WorkflowImplementation: ActivityImplementationBase, IResumableActivity
    {
        private Workflow _activityInstanceAsWf;

        /// <summary>
        /// ONLY TO BE CALLED BY THE TEST HARNESS OR CREATEWINDOWSACTIVITY
        /// </summary>
        /// <param name="instance">The instance of the workflow activity.</param>
        public override void Initialize(EDC.ReadiNow.Model.WfActivity instance)
        {
            base.Initialize(instance);

            _activityInstanceAsWf = ActivityInstance.Cast<Workflow>();
        }

        bool IResumableActivity.OnStart(IRunState context, ActivityInputs inputs)
        {
            bool hasCompleted = true;

            MapInitialInput(context, inputs);

            var nextActivity = _activityInstanceAsWf.FirstActivity;

            ScheduleNextStep(context, nextActivity);

            hasCompleted = context.WorkflowInvoker.RunTillCompletion(context);

            return hasCompleted;
        }

        bool IResumableActivity.OnResume(IRunState context, IWorkflowEvent resumeEvent)
        {
            bool hasCompleted = true;
            context.SyncFromRun();

            var currentActivity = context.PendingActivity;
            context.PendingActivity = null;

            if (currentActivity == null)
                throw new ApplicationException("Current Activity missing from resumed activity. This should never occur.");

            if (resumeEvent is WorkflowRestoreEvent)    
            {
                if (context.RunStatus == WorkflowRunState_Enumeration.WorkflowRunCancelled)
                    return true;

                // Unsuspend a workflow
                Debug.Assert(context.RunStatus == WorkflowRunState_Enumeration.WorkflowRunSuspended);
                ScheduleNextStep(context, currentActivity);
            }
            else    
            {
                // Unpause an activity
                Debug.Assert(context.RunStatus == WorkflowRunState_Enumeration.WorkflowRunPaused);
                ScheduleResumeStep(context, resumeEvent, currentActivity);
            }

            hasCompleted = context.WorkflowInvoker.RunTillCompletion(context);

            return hasCompleted;
        }

        /// <summary>
        /// Run the next step in the workflow by scheduling the activity. This call in turn will schedule other activities.
        /// </summary>
        private void ScheduleNextStep(IRunState context, WfActivity childActivity)
        {
            var nextWindowsActivity = context.Metadata.CachedInstances[childActivity.Id];

            // ensure all child's input expressions are updated 
            var inputExpressions = nextWindowsActivity.GetInputArgumentsExpressions(context, childActivity);

            var resolvedExpressions = ResolveExpressions(context, inputExpressions);

            var inputs = CreateInputDictionaryForNextStep(context, childActivity, resolvedExpressions);

            context.WorkflowInvoker.ScheduleActivity(context, nextWindowsActivity, inputs, NextStepCompletionCallback, NextStepPausedCallback);
        }

        /// <summary>
        /// Run the next step in the workflow by scheduling the activity. This call in turn will schedule other activities.
        /// </summary>
        private void ScheduleResumeStep(IRunState context, IWorkflowEvent resumeEvent, WfActivity childActivity)
        {
            // set the input
            var windowsActivity = (ActivityImplementationBase)context.Metadata.CachedInstances[childActivity.Id];
            var resumable = (IResumableActivity)windowsActivity;

            context.WorkflowInvoker.ScheduleResume(context, windowsActivity, resumeEvent, NextStepCompletionCallback, NextStepPausedCallback);
        }

        private void NextStepCompletionCallback(IRunState context, ActivityImplementationBase completedInstance, EntityRef exitPointRef)
        {
            var childActivity = ((ActivityImplementationBase)completedInstance).ActivityInstance;

            var transition = context.Metadata.GetTransitionForExitPoint(childActivity, exitPointRef);

            if (transition != null)
            {
                if (NeedToSuspend(context))
                {
                    MarkRunSuspended(context, childActivity);
                }
                else
                {
                    var nextActivity = transition.ToActivity;
                    context.ExitPointId = null;
                    ScheduleNextStep(context, nextActivity);
                }
            }
            else
            {
                Termination termination;

                try
                {
                    termination =
                        _activityInstanceAsWf.Terminations.Single(
                            t => t.FromActivity.Id == childActivity.Id && t.FromExitPoint.Id == exitPointRef.Id);
                }
                catch (InvalidOperationException err)
                {
                    throw new WorkflowRunException(string.Format("There should be only one valid termination found for the given activity and exit point. \nWorkflow: {0} \n Last activity: {1}", _activityInstanceAsWf.GetIdString(), childActivity.Name ?? "(Unnamed)"), err);
                }

                context.ExitPointId = termination.WorkflowExitPoint;

                MarkRunCompleted(context);
                
                context.FlushInternalArgs();
            }
        }

        private void NextStepPausedCallback(IRunState context, ActivityImplementationBase pausedInstance)
        {
            MarkRunPaused(context, pausedInstance.ActivityInstance);
        }

        /// <summary>
        /// Update any arguments on the workflow with the resolved expressions
        /// </summary>
        private void UpdateWorkflowArguments(IRunState context, Dictionary<WfExpression, object> resolvedExpressions)
        {
            foreach (var kvp in resolvedExpressions)
            {
                var expression = kvp.Key;

                ActivityArgument arg = context.Metadata.GetArgumentPopulatedByExpression(expression);

                context.SetArgValue(arg, kvp.Value);
            }
        }

        /// <summary>
        /// Copy any input arguments 
        /// </summary>
        public void MapInitialInput(IRunState context, IDictionary<ActivityArgument, object> inputArguments)
        {
            foreach (var kvp in inputArguments)
            {
                context.SetArgValue(kvp.Key, kvp.Value);
            }

            // Do the initial variable initialization
            var variableExpressions = _activityInstanceAsWf.Variables.Select(v => v.PopulatedByExpression.FirstOrDefault()).Where(e => e != null).ToList();
            var resolvedExpressions = ResolveExpressions(context, variableExpressions);

            UpdateWorkflowArguments(context, resolvedExpressions);
        }
        
        ///// <summary>
        ///// Update all the output arguments
        ///// </summary>
        //void UpdateWfOutputs(IRunState context, Dictionary<WfExpression, object> resolvedExpressions)
        //{
        //    UpdateWorkflowArguments(context, resolvedExpressions);
        //}

        /// <summary>
        /// Create an input dictionary using the applicable values in the ArgumentValueStore
        /// </summary>
        ActivityInputs CreateInputDictionaryForNextStep(IRunState context, WfActivity nextActivity, Dictionary<WfExpression, object> evaluatedExpressions)
        {
            var result = new ActivityInputs();

            // add expressions
            foreach (var kvp in evaluatedExpressions)
            {
                var arg = kvp.Key.ArgumentToPopulate;
                result.Add(arg, kvp.Value);
            }

            return result;
        }

        /// <summary>
        /// Update all the the provided expressions. 
        /// </summary>
        private Dictionary<WfExpression, object> ResolveExpressions(IRunState context, ICollection<WfExpression> expressions)
        {
            Dictionary<WfExpression, object> result = null;

            // evaluate the expressions and update the targeted values
            SecurityBypassContext.RunAsUser(() =>
            {
                result = expressions.ToDictionary(e => e, e => EvaluateExpression(context, e));
            });

            return result;
        }

        /// <summary>
        /// Update all the mapping variables associated with the activity. 
        /// </summary>
        private object EvaluateExpression(IRunState context, WfExpression expression)
        {
            try
            {
                object evaluatedValue;
                try
                {
                    evaluatedValue = context.EvaluateExpression(expression);
                }
                catch (System.Data.EvaluateException ex)
                {
                    throw new WorkflowRunException(ex.Message, ex);
                }

                return evaluatedValue;
            }
            catch (WorkflowRunException)
            {
                throw;
            }
            catch (WorkflowRunException_Internal)
            {
                throw;
            }
            catch (Exception e)
            {
                if (expression != null)
                {
                    EventLog.Application.WriteError("Exception in Workflow UpdateExpression for Expression id {0} string \"{1}\" : {2}", expression.Id, expression.ExpressionString ?? "null", e.Message);
                }
                else
                {
                    EventLog.Application.WriteError("Exception in Workflow UpdateExpression for Expression (null): {0}", e.Message);
                }
                throw;
            }
        }


        public override void Validate(WorkflowMetadata metadata)
        {
            using (Profiler.Measure("WorkflowImplementation.Validate"))
            {
                using (new SecurityBypassContext())
                {
                    base.Validate(metadata);

                    // validate first activity
                    if (_activityInstanceAsWf.FirstActivity == null)
                        metadata.AddValidationError("First Activity is missing.");

                    // ensure that all workflow variables are also in the expression parameters
                    foreach (var v in _activityInstanceAsWf.Variables)
                    {
                        if (_activityInstanceAsWf.ExpressionParameters.All(p => p.ArgumentInstanceArgument.Id != v.Id))
                            metadata.AddValidationError(string.Format("Variable '{0}' is missing from the expression parameter list.", v.Name));
                    }

                    // ensure the name, activity and argument are defined for the expression parameters
                    foreach (var param in _activityInstanceAsWf.ExpressionParameters)
                    {
                        if (param.Name == null)
                            metadata.AddValidationError(string.Format("Parameter '{0}' is missing a name.", param.Id));

                        if (param.ArgumentInstanceActivity == null || param.ArgumentInstanceArgument == null)
                            metadata.AddValidationError(string.Format("Parameter '{0}' is missing source information, must have both activity and argument defined.", param.Name));
                    }

                    ValidateExpressions(metadata);

                    ValidateWorkflowVariables(metadata);
                    ValidateTransitions(metadata);
                    ValidateTerminations(metadata);

                    ValidateTransitions2(metadata);


                    //
                    // validate first activity
                    //
                    if (_activityInstanceAsWf.FirstActivity == null)
                        metadata.AddValidationError("The workflow has no first activity.");

                    //
                    // Validate all the contained activities
                    foreach (var activityInstance in metadata.CachedInstances.Values)
                    {
                        if (activityInstance != this)
                            activityInstance.Validate(metadata);
                    }

                    //
                    // Ensure that all activities have unique names. (This ensures that activity outputs cannot have name clashes.)
                    var allNames = _activityInstanceAsWf.ContainedActivities.Select(a => a.Name);
                    var duplicates = allNames.GroupBy(s => s).SelectMany(grp => grp.Skip(1));

                    if (duplicates.Count() > 0)
                    {
                        metadata.AddValidationError(string.Format("All activities in a workflow must have a unique name: {0}", string.Join(", ", duplicates)));
                    }
                }

            }
        }

        private void ValidateExpressions(WorkflowMetadata metadata)
        {
            using (Profiler.Measure("WorkflowImplementation.ValidateExpressions"))
            {

                // Validate the Expressions
                // - all expressions have a target argument
                // - no more than one expression can target an argument

                var allExp = _activityInstanceAsWf.ContainedActivities.SelectMany(a => a.ExpressionMap.Select(e => new { Activity = a, Expression = e }));
                allExp = allExp.Union(ActivityInstance.ExpressionMap.Select(e => new { Activity = ActivityInstance, Expression = e })).ToList();

                foreach (var exp in allExp)
                {
                    var e = exp.Expression;
                    if (e.ArgumentToPopulate == null)
                    {
                        var s = string.Format("Warning: Expression for \"{0}\" ({1}) missing argument to populate: expression name=\"{2}\" ({3}) value=\"{4}\".",
                                                exp.Activity != null ? exp.Activity.Name : "", exp.Activity != null ? exp.Activity.Id.ToString() : "",
                                                e.Name ?? "", e.Id, e.GetExpressionDisplayString());
                        EventLog.Application.WriteWarning(s);
                        //metadata.AddValidationError(s);
                    }
                }
            }
        }

        private void ValidateWorkflowVariables(WorkflowMetadata metadata)
        {
            using (Profiler.Measure("WorkflowImplementation.ValidateWorkflowVariables"))
            {
                // Validate workflow variables
                var inputArgumentsNames = _activityInstanceAsWf.InputArguments.Select(arg => arg.Name);

                foreach (var wfVar in _activityInstanceAsWf.Variables)
                {
                    if (inputArgumentsNames.Contains(wfVar.Name))
                        metadata.AddValidationError(string.Format("Name of variable '{0}' clashed with the input argument of the same name.", wfVar.Name));
                }
            }
        }

        private void ValidateTerminations(WorkflowMetadata metadata)
        {
            using (Profiler.Measure("WorkflowImplementation.ValidateTerminations"))
            {
                foreach (var term in _activityInstanceAsWf.Terminations)
                {
                    if (term.FromActivity == null || term.FromExitPoint == null || term.WorkflowExitPoint == null)
                        metadata.AddValidationError(
                            string.Format(
                                "Workflow '{0}' does not have one of its terminations fully described. It must have a from activity and exit point and a workflow exit point. From '{1}':'{2}'  To:'{3}'",
                                ActivityInstance.Name,
                                term.FromActivity != null ? term.FromActivity.Name ?? "Unnamed activity" : "Activity not specified",
                                term.FromExitPoint != null ? term.FromExitPoint.Name ?? "Unnamed exit" : "Exit not specified",
                                term.WorkflowExitPoint != null ? term.WorkflowExitPoint.Name ?? "Unnamed workflow exit" : "Exit not specified"));
                }
            }
        }

        private void ValidateTransitions(WorkflowMetadata metadata)
        {
            using (Profiler.Measure("WorkflowImplementation.ValidateTransitions"))
            {
                foreach (var tran in metadata.AllTransitions)
                {
                    if (tran.FromActivity == null || tran.FromExitPoint == null || tran.ToActivity == null)
                        metadata.AddValidationError(
                            string.Format("Workflow '{0}' does not have one of its transitions ({1}) fully described. It must have a from activity and exit point and a destination. "
                            + "From {2} {3} to {4}",
                            ActivityInstance.Name, tran.Id,
                            tran.FromActivity != null ? tran.FromActivity.Name ?? tran.FromActivity.Id.ToString() : "null",
                            tran.FromExitPoint != null ? tran.FromExitPoint.Name ?? tran.FromExitPoint.Id.ToString() : "null",
                            tran.ToActivity != null ? tran.ToActivity.Name ?? tran.FromActivity.Id.ToString() : "null"
                            ));

                    // validate that all transitions point to children of the workflow
                    if (tran.ToActivity != null && (tran.ToActivity.ContainingWorkflow == null || tran.ToActivity.ContainingWorkflow.Id != _activityInstanceAsWf.Id))
                        metadata.AddValidationError(string.Format("Activity '{0}' is not a child of the workflow but is part of a transition", tran.FromActivity.Name));
                }
            }
        }

        private void ValidateTransitions2(WorkflowMetadata metadata)
        {
            using (Profiler.Measure("WorkflowImplementation.ValidateTransitions2"))
            {

                //
                // Validate that all the transitions from the activities are in the Transitions list
                var comparer = new EntityIdComparer();

                var wfTransAndTerms = ((IEnumerable<IEntity>)_activityInstanceAsWf.Transitions).Union((IEnumerable<IEntity>)_activityInstanceAsWf.Terminations).ToList();

                using (Profiler.Measure("WorkflowImplementation.ValidateTransitions2.1"))
                {
                    foreach (var activity in _activityInstanceAsWf.ContainedActivities)
                    {
                        // validate that all the transitions are on the workflow

                        var missing = activity.ForwardTransitions.Except(wfTransAndTerms, comparer);

                        if (missing.Any())
                        {
                            metadata.AddValidationError(string.Format("Activity \"{0}\" has a transition that is not listed against the workflow.", activity.Name ?? "(Unnamed)"));
                        }


                        // validate that the transitions have a from exit point on the activity or its type
                        var exitPoints = metadata.GetExitpointsForActivity(activity.Id);
                        foreach (var tran in activity.ForwardTransitions)
                        {
                            if (!exitPoints.Contains(tran.FromExitPoint, comparer))
                            {
                                metadata.AddValidationError(
                               string.Format("Activity \"{0}\" has a transition whose exit point is not from the origin activity.", activity.Name ?? "(Unnamed)"));
                            }
                        }
                    }
                }
            }
        }

        


        void MarkRunCompleted(IRunState runState)
        {
            using (new SecurityBypassContext())
            {
                runState.RunStatus = WorkflowRunState_Enumeration.WorkflowRunCompleted;
            }
        }

        void MarkRunPaused(IRunState runState, WfActivity currentActivity)
        {
            Debug.Assert(currentActivity != null);

            using (new SecurityBypassContext())
            {
                runState.PendingActivity = currentActivity;
                runState.RunStatus = WorkflowRunState_Enumeration.WorkflowRunPaused;
            }
        }

        void MarkRunSuspended(IRunState runState, WfActivity currentActivity)
        {
            Debug.Assert(currentActivity != null);

            using (new SecurityBypassContext())
            {
                runState.PendingActivity = currentActivity;
                runState.RunStatus = WorkflowRunState_Enumeration.WorkflowRunSuspended;
            }
        }

        /// <summary>
        /// Do we need to suspend the workflow?
        /// </summary>
        bool NeedToSuspend(IRunState runState)
        {
            if (!Factory.FeatureSwitch.Get("longRunningWorkflow"))
                return false;

            // TODO: make configurable
            return (runState.TimeTakenInSession.ElapsedMilliseconds > WorkflowRunner.Instance.SuspendTimeoutMs);
        }

        /// <summary>
        /// Thrown when an attempt is made to get the value of an argument or variable that has never been defined.
        /// </summary>
        public class ExceededArgEvaluationDepthException : WorkflowRunException
        {
            public ExceededArgEvaluationDepthException(long workflowRunId, WfExpression expression)
                : base("An expression has exceeded the maximum depth for references when being evaluated. This is usually caused by a circular reference: Expression: '{0}'", expression.ExpressionString)
            { }
        }
    }
}
