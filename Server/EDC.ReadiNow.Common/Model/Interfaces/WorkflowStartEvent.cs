// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace EDC.ReadiNow.Model.Interfaces
{
    /// <summary>
    /// Event indicating that a workflow is to be started.
    /// NOT CURRENTLY QUEUEABLE
    /// </summary>
    public class WorkflowStartEvent: IWorkflowEvent
    {
        /// <summary>
        /// the arguments to start the workflow with. 
        /// </summary>
        public Dictionary<string, object> Arguments { get; set; }

       
        /// <summary>
        /// The workflow to be run
        /// </summary>
        public Workflow Workflow { get; }

        /// <summary>
        /// The trigger that the workflow was started from - may be null.
        /// </summary>
        public WfTrigger Trigger { get; }

        /// <summary>
        /// Is the workflow to be run in trace mode.
        /// </summary>
        public bool Trace { get; set; }

        /// <summary>
        /// Set if this run was triggered from another run.
        /// </summary>
        public WorkflowRun ParentRun { get; set; }

        /// <summary>
        /// Create the event for a workflow
        /// </summary>
        /// <param name="workflow">The workflow to run</param>
        public WorkflowStartEvent(Workflow workflow)
        {
            Workflow = workflow;
            Arguments = new Dictionary<string, object>();
        }

        /// <summary>
        /// Create the event using a trigger.
        /// </summary>
        /// <param name="trigger">the trigger to use as the basis of a run</param>
        public WorkflowStartEvent(WfTrigger trigger)
        {
            Trigger = trigger;
            Workflow = trigger.WorkflowToRun;
            Arguments = new Dictionary<string, object>();
        }
    }
}
