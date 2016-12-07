// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Model;

using EDC.SoftwarePlatform.Activities.Notify;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model.Interfaces;
using System.Collections.Generic;
using System.Collections;
using Autofac;
using EDC.ReadiNow.Notify;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Common.Workflow;
using EDC.SoftwarePlatform.Activities.EventTarget;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Distributes a survey for completion.
    /// </summary>
    public class NotifyImplementation : ResumableActivityImplementationBase
    {
        private const decimal DefaultTimeOutMins = 0;     // no time out


        public override bool OnStart(IRunState context, ActivityInputs inputs)
        {
            var users = GetArgumentEntityList(inputs, "inPeople");

            var message = GetArgumentValue<String>(inputs, "inMessage");
            var waitForReplies = GetArgumentValueStruct<bool>(inputs, "inWaitForReplies");
            var timeoutDays = GetArgumentValue<decimal>(inputs, "inNotifyTimeOut", 0);
            var acceptRepliesDays = GetArgumentValue<decimal>(inputs, "inAcceptRepliesFor", 1);

            var linkToRecord = GetArgumentEntity<UserResource>(inputs, "inLinkToRecord");

            if (!Factory.Current.Resolve<INotifier>().IsConfigured)
                throw new WorkflowRunException("No tenant notifier has been configured in tenant general settings.");

            //
            // Notification needs to be created first to ensure that a fast response will not be processed before the notification is created
            //
            var effectiveUser = Entity.Get<UserAccount>(context.EffectiveSecurityContext.Identity.Id);

            var notification = Entity.Create<Notification>();
            notification.NMessage = message;
            notification.SecurityOwner = effectiveUser;                 // The workflow runner can delete and update the notification and the send and replies.
            notification.NRelatedRecord = linkToRecord;
            notification.NAcceptRepliesUntil = DateTime.UtcNow.AddDays(Convert.ToDouble(acceptRepliesDays));

            CopyReplyMap(context, notification);

            notification.Save();

            context.SetArgValue(ActivityInstance, GetArgumentKey("core:outNotificationRecord"), notification);

            if (waitForReplies)
            {
                SetTimeoutIfNeeded(context, timeoutDays);    // Note that there is no timeout exit. 
            }

            SendMessagesInBackground(context, notification.Id, users.Select(u => u.Id).ToList(), waitForReplies);

            return false;
        }

        /// <summary>
        /// Copy all the map entries from the activity to the notification.
        /// This ensures that changes to teh workflow after the notification is sent will not effect replies.
        /// </summary>
        void CopyReplyMap(IRunState context, Notification notification)
        {
            var replyMap = context.CurrentActivity.As<NotifyActivity>().NReplyMap;

            notification.NReplyMapCopy.AddRange(replyMap.Select(e => new ReplyMapEntry { Name = e.Name, RmeWorkflow = e.RmeWorkflow }));
        }


        public override bool OnResume(IRunState context, IWorkflowEvent resumeEvent)
        {
            if (resumeEvent is NotifyResumeFailedEvent)
                throw new WorkflowRunException("Workflow failed with an internal error.");

            var notification = context.GetArgValue<IEntity>(ActivityInstance, GetArgumentKey("core:outNotificationRecord"));

            if (notification == null)
                throw new ArgumentException("notification");

            var writable = notification.AsWritable<Notification>();

            if (writable.NPendingRun != null)
            {
                writable.NPendingRun = null;
                writable.Save();
            }

            HandleTimeout(context, resumeEvent);

            return true;       // keep going
        }

        /// <summary>
        /// Validate that the reply workflow is marked as RunAsOwner.
        /// </summary>
        /// <param name="metadata"></param>
        public override void Validate(WorkflowMetadata metadata)
        {
            base.Validate(metadata);

            var activityAs = ActivityInstance.Cast<NotifyActivity>();

            foreach (var entry in activityAs.NReplyMap)
            {
                if (String.IsNullOrWhiteSpace(entry.Name))
                {
                    metadata.AddValidationError($"An all entries in the reply mapping must contain a value: '{activityAs.Name}'");
                }
                else if (entry.RmeWorkflow == null)
                {
                    metadata.AddValidationError($"Mapping entry '{entry.Name}' does not point to a workflow: '{activityAs.Name}'");
                }
                else if (entry.RmeWorkflow.WorkflowRunAsOwner != true)
                {
                    metadata.AddValidationError($"Mapping entry '{entry.Name}' points to a workflow that is not set to 'Run as owner': '{activityAs.Name}'");
                }
            }
        }


        /// <summary>
        /// Send the notifications on a background thread.
        /// </summary>
        void SendMessagesInBackground(IRunState context, long notificationId, List<long> userIds, bool waitForReplies)
        {
            context.SetPostRunAction(() =>
            {
                var runId = context.WorkflowRunId;

                WorkflowRunContext.Current.QueueAction(() =>
                SecurityBypassContext.Elevate(() =>
                {
                    var workflowRunner = Factory.Current.Resolve<IWorkflowRunner>();
                    WorkflowRun run = Entity.Get<WorkflowRun>(runId); ;

                    try
                    {
                        var notification = Entity.Get<Notification>(notificationId);
                        var users = Entity.Get(userIds);

                        Factory.Current.Resolve<INotifier>().Send(notification, users, true);
                        var sends = notification.SendRecords;
                        var sentWithoutError = sends.Where(r => r.SrErrorMessage == null);

                        // update the notification 

                        bool resume = true;
                        if (waitForReplies)
                        {
                            notification = notification.AsWritable<Notification>();
                            notification.NPendingRun = run;
                            notification.Save();

                            resume = notification.Complete(); // Just in case all the replies were completed while we were sending.
                        }

                        if (resume && run != null)
                            workflowRunner.ResumeWorkflow(run, new NotifyResumeEvent());
                    }
                    catch
                    {
                        if (run != null)
                        {
                            workflowRunner.ResumeWorkflow(run, new NotifyResumeFailedEvent());
                        }

                        throw;
                    }
                }));
            });
        }
    }

    public class NotifyResumeEvent : IWorkflowEvent
    {
    }

    public class NotifyResumeFailedEvent : IWorkflowEvent
    {
    }
}