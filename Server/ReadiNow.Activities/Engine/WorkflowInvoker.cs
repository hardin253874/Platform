// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using System.Diagnostics;
using EDC.ReadiNow.Core;
using Autofac;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Common.Workflow;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// An invoker is used to run a workflow. 
    /// there exists a default invokler for the thread, which is the normal way in which it is used.
    /// </summary>
    public class WorkflowInvoker
    {


        /// <summary>
        /// The activities waiting to be run
        /// </summary>
        Queue<Func<bool>> _pendingActivities = new Queue<Func<bool>>();

        // 
        // Events waiting to be processed. 
        // Events are processed after all other processing is completed, in the order they are raised.
        //
        private Queue<IWorkflowEvent> _pendingEvents = new Queue<IWorkflowEvent>();



        public WorkflowInvoker()
        {
        }


        /// <summary>
        /// Run an activity on the invoker
        /// </summary>
        /// <param name="runState">The runState for the workflow</param>
        /// <param name="windowsActivity">The activity to run</param>
        /// <param name="inputs">The inputs</param>
        /// <returns></returns>
        public bool Run(IRunState runState, ActivityImplementationBase windowsActivity, ActivityInputs inputs)
        {
            ScheduleActivity(runState, windowsActivity, inputs, null, null);
            return RunTillCompletion(runState);
        }

        /// <summary>
        /// Resume a paused activity on the invoker
        /// </summary>
        /// <param name="runState">The runstate</param>
        /// <param name="windowsActivity">The activity</param>
        /// <param name="resumeEvent">The trigger event for the resume</param>
        /// <returns></returns>
        public bool Resume(IRunState runState, ActivityImplementationBase windowsActivity, IWorkflowEvent resumeEvent)
        {
            ScheduleResume(runState, windowsActivity, resumeEvent, null, null);
            return RunTillCompletion(runState);
        }

        /// <summary>
        /// Add an activity to the pending list. NOTE! This does not run the activity.
        /// </summary>
        public void ScheduleActivity(IRunState runState, ActivityImplementationBase windowsActivity, ActivityInputs inputs, 
            Action<IRunState, ActivityImplementationBase, EntityRef> actionOnCompletion,
            Action<IRunState, ActivityImplementationBase> actionOnPause)
        {
            Func<bool> step = () =>
            {
                runState.StepsTakenInSession++;

                runState.CurrentActivity = windowsActivity.ActivityInstance;
                runState.RecordTrace(windowsActivity.ActivityInstance.Name);

                return windowsActivity.RunInContext(runState, inputs);
            };

            ProcessStep(runState, windowsActivity, actionOnCompletion, actionOnPause, step);
           
        }


        /// <summary>
        /// Add a resume activity to the pending list. NOTE! This does not resume the activity.
        /// 
        /// </summary>
        public void ScheduleResume(IRunState runState, ActivityImplementationBase windowsActivity, IWorkflowEvent resumeEvent,
            Action<IRunState, ActivityImplementationBase, EntityRef> actionOnCompletion,
            Action<IRunState, ActivityImplementationBase> actionOnPause)
        {
            Func<bool> step = () =>
            {
                runState.RecordTrace(windowsActivity.ActivityInstance.Name);
                return windowsActivity.ResumeInContext(runState, resumeEvent);
            };

            ProcessStep(runState, windowsActivity, actionOnCompletion, actionOnPause, step);
        }

        void ProcessStep(
            IRunState runState, 
            ActivityImplementationBase windowsActivity, 
            Action<IRunState, ActivityImplementationBase, EntityRef> actionOnCompletion,
            Action<IRunState, ActivityImplementationBase> actionOnPause, 
            Func<bool> step
            )
        {
            //TODO: Retire this mess. This is all overly complicated.

            _pendingActivities.Enqueue(
                () =>
                {
                    var result = step();
                    return CompleteActivityRun(
                        result,
                        runState,
                        exitPoint => // on completed
                            {
                            if (actionOnCompletion != null)
                                actionOnCompletion(runState, windowsActivity, exitPoint);
                        },
                        () =>                   // on paused
                            {
                            if (actionOnPause != null)
                                actionOnPause(runState, windowsActivity);
                        });
                }
                );
        }




        private bool CompleteActivityRun(bool isComplete, IRunState runState, Action<EntityRef > completionAction, Action pauseAction)
        {
            bool result;
            if (isComplete)
            {
                if (completionAction != null)
                {
                    var exitPoint = runState.ExitPointId;

                    completionAction(exitPoint);
                }

                result =  true;
            }
            else
            {
                if (pauseAction != null)
                {
                    pauseAction();
                }
                
                result = false;
            }

            return result;
        }

       

        /// <summary>
        /// Run till everything that can finish has.
        /// </summary>
        /// <returns>True if everything completed, false if there are outstanding resumable activities.</returns>
        virtual public bool RunTillCompletion(IRunState runState)
        {
            bool hasCompletedEverything = true;

            var stopWatch = runState.TimeTakenInSession;
            stopWatch.Reset();
            stopWatch.Start();

            var maxTimeInSessionMs = WorkflowTriggerHelper.MaxRunTimeSeconds * 1000 - runState.WorkflowRun.TotalTimeMs;
            var maxStepsInSession = WorkflowTriggerHelper.MaxSteps - runState.WorkflowRun.RunStepCounter;

            try
            {
                while (_pendingActivities.Any())
                {
                    var next = _pendingActivities.Dequeue();

                    var hasCompleted = next();

                    if (!hasCompleted)
                        hasCompletedEverything = false;

                    if (stopWatch.ElapsedMilliseconds  > maxTimeInSessionMs)
                        throw new WorkflowRunException("The workflow ran for too long. It must pause or complete within {0} seconds", WorkflowTriggerHelper.MaxRunTimeSeconds);

                    if (runState.StepsTakenInSession > maxStepsInSession)
                        throw new WorkflowRunException("The workflow ran for too long. It must pause or complete within {0} steps", WorkflowTriggerHelper.MaxSteps);

                }
            }
            finally
            {
                stopWatch.Stop();
            }
                    
            return hasCompletedEverything;
        }

        /// <summary>
        /// Raise an event. Events are handled in the order raised after the other processing has completed
        /// </summary>
        /// <param name="nextEvent"></param>
        public void PostEvent(IWorkflowEvent nextEvent)
        {
            Action action = null;

            var asChildRunStart = nextEvent as ChildWorkflowStartEvent;

            if (asChildRunStart != null)
            {
                var parentRun = asChildRunStart.ParentRun;
                if (parentRun != null)
                {
                    var triggerDepth = WorkflowRunContext.Current.TriggerDepth;
                    action = () => {
                        // Need to handle deferred here otherwise "double-resume" fails (BUG #27863)
                        using (new WorkflowRunContext(true) { TriggerDepth = triggerDepth + 1 })
                        {
                            var startEvent = new WorkflowStartEvent(asChildRunStart.WorkflowToStart)
                            {
                                Arguments = asChildRunStart.Inputs,
                                ParentRun = parentRun,
                                Trace = parentRun.RunTrace ?? false
                            };

                            // TODO: Change this into a call into the BackgroundTaskManager
                            WorkflowRunner.Instance.RunWorkflow(startEvent);
                        }
                    };
                }
            }
            else
            {
                var asChildRunCompleted = nextEvent as ChildWorkflowCompletedEvent;

                if (asChildRunCompleted != null)
                {
                    var completedRun = asChildRunCompleted.CompletedRun;
                    if (completedRun != null)
                    {
                        // Need to decrement the trigger depth otherwise "child workflow in a foreach" fails (BUG #27863)
                        if (WorkflowRunContext.Current.TriggerDepth > 0)
                        {
                            WorkflowRunContext.Current.TriggerDepth--;
                        }
                        action = () => WorkflowRunner.Instance.ResumeWorkflow(completedRun.ParentRun, asChildRunCompleted);
                    }
                }
            }

            if (action != null)
            {
                // NOTE: runBeforeSave was passed as "true" here, however, this led to issues where previously
                // unsaved copies of the same run were already present in the queue, causing endless looping (BUG #27863)
                WorkflowRunContext.Current.DeferAction(action, false);
            }
        }
    }
}
