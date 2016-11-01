// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;
using System.Linq;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities
{
    public class WorkflowProxyImplementation : ActivityImplementationBase, IResumableActivity
    {
        bool IResumableActivity.OnStart(IRunState context, ActivityInputs inputs)
        {
            Workflow workflowToProxy;

            using (CustomContext.SetContext(context.EffectiveSecurityContext))
            {
                workflowToProxy = ActivityInstance.As<WorkflowProxy>().WorkflowToProxy;
            }
            var wfArgs = workflowToProxy.As<WfActivity>().GetInputArguments().Select(arg=>arg.Name);

            // copy across matching arguments. Arguments that are supplied that are not used are ignored.
            var matchingInputs = inputs.Where(kvp => wfArgs.Contains(kvp.Key.Name)).ToDictionary(kvp => kvp.Key.Name, kvp => kvp.Value);

            if (inputs.Count != matchingInputs.Count)    // We have some mismatched parameters
            {
                var expected = String.Join(", ", wfArgs);
                var actual = String.Join(", ", inputs.Select(i => i.Key.Name));
                throw new WorkflowRunException($"Mismatched inputs to workflow. (Have the proxied workflows inputs changed?)\n   Expected: {expected}\n   Actual: {actual}");
            }
            
            context.WorkflowInvoker.PostEvent(new ChildWorkflowStartEvent(workflowToProxy, matchingInputs, context.WorkflowRun));
            
            // note, we always rely on the resume for handling the completion.
            return false;
        }

        bool IResumableActivity.OnResume(IRunState context, IWorkflowEvent resumeEvent)
        {
            var run = ((ChildWorkflowCompletedEvent)resumeEvent).CompletedRun;

            ProcessResult(context, run);

            return true;
        }
        
        void ProcessResult(IRunState context, WorkflowRun run)
        {
            if (run.WorkflowRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunCompleted)
                throw new InnerWorkflowFailedException(string.Format("Inner workflow '{0}' failed", run.WorkflowBeingRun.Name));

            var proxyExitPoint = run.GetExitPoint();
            var matchingExitPoint = context.Metadata.GetExitpointsForActivity(ActivityInstance.Id).SingleOrDefault(ep => ep.Name == proxyExitPoint.Name);

            if (matchingExitPoint == null)
                throw new WorkflowRunException("Workflow proxy returned using an exit point that doesn't match anything in the parent");

            context.ExitPointId = matchingExitPoint;

            var outputs = run.GetOutput();

            var outArgs = ActivityInstance.GetOutputArguments();

            // copy the values from the results into the arguments witht eh same name
            foreach (var arg in outArgs)
            {
                var name = arg.Name;
                object value = null;

                if (outputs.TryGetValue(name, out value))
                {
                    context.SetArgValue(ActivityInstance, arg, value);
                }
            }
        }

        public override void Validate(WorkflowMetadata metadata)
        {
            base.Validate(metadata);

            var workflowToProxy = ActivityInstance.As<WorkflowProxy>().WorkflowToProxy;

            if (workflowToProxy == null)
            {
                metadata.AddValidationError("Workflow to run has not been set.");
            }
            
            //
            // The proxy workflow will be validated separately
            //
        }

        /// <summary>
        /// Used to hold exception deatils until theere is sufficent context down the stack to create a 
        /// workflow run exception
        /// </summary>
        public class InnerWorkflowFailedException : WorkflowRunException
        {
            public InnerWorkflowFailedException(string message)
                : base(message)
            { }
        }
    }
}