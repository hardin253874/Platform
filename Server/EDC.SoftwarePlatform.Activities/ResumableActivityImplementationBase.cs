// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using System.Diagnostics;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities
{


    /// <summary>
    /// A default implementation of a resumable activity. 
    /// </summary>
    public abstract class ResumableActivityImplementationBase: ActivityImplementationBase, IResumableActivity
    {
        public abstract bool OnStart(IRunState context, ActivityInputs inputs);

        public abstract bool OnResume(IRunState context, IWorkflowEvent resumeEvent);


        /// <summary>
        /// Set a time-out if the due date is greater than zero and there is a timeout transition.
        /// </summary>
        /// <param name="timeoutDays"></param>
        protected void SetTimeoutIfNeeded(IRunState context, decimal timeoutDays)
        {
            if (timeoutDays > 0 && GetTimeoutTransition() != null)
            {
                context.SetPostRunAction(() => TimeoutActivityHelper.Instance.ScheduleTimeout(this, context.WorkflowRun, new decimal(24.0 * 60.0) * timeoutDays));
                context.HasTimeout = true;
            }
        }

        /// <summary>
        /// Cancel a timeout if one has been set. If a timeout has occured set the exit point to the timeout exit
        /// </summary>
        /// <param name="context"></param>
        /// <returns>true is a time-out has occured</returns>
        protected bool HandleTimeout(IRunState context, IWorkflowEvent resumeEvent)
        {
            if (context.HasTimeout)
            {
                TimeoutActivityHelper.Instance.CancelTimeout(context.WorkflowRun);
                context.HasTimeout = false;
            }

            var timeoutEvent = resumeEvent as TimeoutEvent;
            if (timeoutEvent != null)
            {
                var timeoutTrans = GetTimeoutTransition();

                if (timeoutTrans == null)
                    throw new ApplicationException("There should never be a time-out without a time-out exit point.");

                context.ExitPointId = timeoutTrans.FromExitPoint;

                return true;
            }
            else
                return false;
        }

        TransitionStart GetTimeoutTransition()
        {
            return ActivityInstance.ForwardTransitions.FirstOrDefault(t => t.FromExitPoint.IsTimeoutExitPoint ?? false);
        }

        /// <summary>
        /// Deal with an event that includes a transition selection
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resumeEvent"></param>
        protected void HandleResumeTransition(IRunState context, IWorkflowUserTransitionEvent resumeEvent)
        {
            // The user task was already updated by the service
            var selectedTransition = Entity.Get<TransitionStart>(resumeEvent.CompletionStateId);

            if (selectedTransition == null)
                throw new WorkflowRunException("Attempted to take a transition that does not exist.");

            context.ExitPointId = selectedTransition.FromExitPoint.Id;
        }

        protected EntityCollection<TransitionStart> GetAvailableUserTransitions()
        {
           return new EntityCollection<TransitionStart>(
              ActivityInstance.ForwardTransitions.Where(t => ! (t.FromExitPoint.IsTimeoutExitPoint ?? false) && IsValidTransitionStart(t)));
        }

        // 
        // Remove dead transitions (this shouldn't be needed if cascade delete is working correctly)
        bool IsValidTransitionStart(TransitionStart transStart)
        {
            var transition = transStart.As<Transition>();
            if (transition != null && transition.WorkflowForTransition == null)
            {
                return false;
            }

            var termination = transStart.As<Termination>();
            if (termination != null && termination.WorkflowForTermination == null)
            {
                return false;
            }

            return true;
        }
    }
}
