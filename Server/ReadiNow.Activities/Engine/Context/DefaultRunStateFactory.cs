// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities
{
    public class DefaultRunStateFactory: IRunStateFactory
    {
        static readonly DefaultRunStateFactory _singleton = new DefaultRunStateFactory();

        public static DefaultRunStateFactory Singleton { get { return _singleton; } }

        /// <summary>
        /// Create a run state
        /// </summary>
        /// <returns>A run state - note the security information will be missing if it has been set incorrectly in the workflow</returns>
        public IRunState CreateRunState(WorkflowMetadata metaData, WorkflowRun workflowRun)
        {
            using (Profiler.Measure("DefaultRunStateFactory.CreateRunState"))
            {
                var run = workflowRun;

                if (!run.IsTemporaryId)
                    run = run.AsWritable<WorkflowRun>();

                //var effectiveUser = GetEffectiveUser(run, metaData);
                var effectiveSecurityContext = GetEffectiveSecurityContext(run, metaData);
                
                return new RunState(metaData, run, effectiveSecurityContext);
            }
        }

        
        /// <summary>
        /// Get the request context this workflow will be running as, taking into account triggering user and owner.
        /// </summary>
        private RequestContextData GetEffectiveSecurityContext(WorkflowRun run, WorkflowMetadata metaData)
        {
            var effectiveUser = GetEffectiveUser(run, metaData);
            var triggeringUser = GetTriggeringUser(run);

            // Error! The caller will deal with the missing info. We can't throw becasue RunState is needed for the error reporting
            if (effectiveUser == null)
                return null;

            if (metaData.WfRunAsOwner && metaData.WfSecurityOwner == null)
                return null;

            var identityInfo = new IdentityInfo(effectiveUser.Id, effectiveUser.Name);
                       

            var context = RequestContext.GetContext();

            var effectiveSecurityContext = new RequestContextData(context);
            effectiveSecurityContext.Identity = identityInfo;

            // If we are running as someone other than the triggerer, set the secondary identity to the triggerer.
            // NOTE: This could potentially cause a problem in the case where a wf triggering another wf scenario. 
            // It's possible that the user will see stale data. The risk should be quite low.
            if (triggeringUser != null && triggeringUser.Id != effectiveUser.Id)
            {
                effectiveSecurityContext.SecondaryIdentity = new IdentityInfo(triggeringUser.Id, triggeringUser.Name);
            }

            return effectiveSecurityContext;
        }


        private UserAccount GetEffectiveUser(WorkflowRun run, WorkflowMetadata metaData)
        {
            if (metaData != null && metaData.WfRunAsOwner)
            {
                return metaData.WfSecurityOwner;
            }

            return GetTriggeringUser(run);
        }

        private UserAccount GetTriggeringUser(WorkflowRun run)
        {
            if (run != null && run.TriggeringUser == null)
            {
                var deferredRun = run as WorkflowRunDeferred;
                if (deferredRun != null)
                    return deferredRun.GetTriggeringUser();
            }

            if (run != null)
                return run.TriggeringUser;

            return null;
        }

    }
}