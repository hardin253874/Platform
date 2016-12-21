// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.EventTarget
{
    public class ReplyRecordEventTarget : IEntityEventSave
    {
        const string NewRecordsKey = "ReplyRecordEventTarget_NewRecords";
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            state[NewRecordsKey] = entities.Where(e => e.IsTemporaryId).ToList();

            return false;
        }


        /// <summary>
        /// Check if there is a pending workflow related to the notification and let it know that a reply was received
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="state"></param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // Trigger any reply workflows 
            var runner = Factory.Current.Resolve<IWorkflowRunner>();
            var newRecords = (List<IEntity>) state[NewRecordsKey];
            var newRecordIds = newRecords.Select(r => r.Id);

            //
            // Check for notifications to complete
            var notificationsToCheck = new List<Notification>();

            foreach (var entity in entities)
            {
                var reply = entity.As<ReplyRecord>();

                var notification = reply?.RrToSend?.SendToNotification;

                if (notification != null && notification.NPendingRun != null)
                    notificationsToCheck.Add(notification);

                TestAndRunReplyMap(runner, notification.NReplyMapCopy, reply);
            }

            var distinctNotifications = notificationsToCheck.Distinct<Notification>(new EntityEqualityComparer());


            distinctNotifications.CheckAndResume();
        }

        /// <summary>
        /// Match the replies against the replyMap and trigger a workflow if there is a match
        /// </summary>
        void TestAndRunReplyMap(IWorkflowRunner runner, IEnumerable<ReplyMapEntry> replyMap, ReplyRecord reply)
        {
            var message = reply?.RrReply?.ToLower();

            if (message == null)
                return;

            var entry = ReplyRecordEventTarget.SelectReplyMapEntry(replyMap, message);

            if (entry != null)
            {
                RunReplyWorkflow(runner, entry.RmeWorkflow, reply);
            }
        }

        /// <summary>
        /// Select a reply map entry that best matches the reply. Null if none matches.
        /// </summary>
        /// <param name="replyMap">The reply map</param>
        /// <param name="reply">Teh reply string</param>
        /// <returns></returns>
        public static ReplyMapEntry SelectReplyMapEntry(IEnumerable<ReplyMapEntry> replyMap, string reply)
        {
            var message = reply.ToLower();

            if (String.IsNullOrEmpty(message))
                return null;

            var orderedMap = replyMap.OrderBy(rm => rm.RmeOrder ?? 0);

            return orderedMap.FirstOrDefault(entry =>
            {
                var name = entry.Name?.Trim()?.ToLower();
                return message.Contains(name);
            });
        }



        /// <summary>
        /// Run the workflow associated with notification reply
        /// </summary>
            void RunReplyWorkflow(IWorkflowRunner runner, Workflow workflow, ReplyRecord reply)
        {
            if (workflow.WorkflowRunAsOwner != true)
                throw new ArgumentException($"The provided workflow is not marked as run as owner. This should never occur. Workflow: ${workflow.Name}");
            
            var input = new Dictionary<string, object>();

            var inputArg = workflow?.InputArgumentForAction?.As<ResourceArgument>();

            if (inputArg != null)
            {
                if (inputArg.ConformsToType.Id == ReplyRecord.ReplyRecord_Type.Id)
                {
                    input.Add(inputArg.Name, reply);
                }
            }

            // Set the context to the owner and start the workflow.
            var wfOwner = workflow.SecurityOwner;
            var contextData = new RequestContextData(RequestContext.GetContext());
            contextData.Identity = new ReadiNow.Security.IdentityInfo(wfOwner.Id, wfOwner.Name);

            using (CustomContext.SetContext(contextData))
            {
                runner.RunWorkflowAsync(new WorkflowStartEvent(workflow) { Arguments = input });
            }
        }
    }
}
