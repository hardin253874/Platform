// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    public static class UserTaskHelper
    {
        public static readonly SequenceIdGenerator SequenceIdGenerator = new SequenceIdGenerator("UTSEQ");

        // TODO: Move these to a tenant specific configuration
        private const string DefaultSubject = "New task assigned ";
        private const string MessageFormat = "{0}<br/><br/><hr/><a href='{1}'>Action Task</a>";
        private const string ActionLinkWithRecordFormat = @"https://{0}/sp/#/{1}/{2}/viewForm?taskId={3}";
        private const string ActionLinkFormat = @"https://{0}/sp/#/{1}/{2}/viewForm";


        /// <summary>
        /// Notify the user that there is a task waiting
        /// </summary>
        /// <param name="userAccount"></param>
        public static void NotifyUser(this DisplayFormUserTask task)
        {
            throw new NotImplementedException("This code needs to be refactored once a decision is made about the structure of email addresses has been made.");
            /*
			/*
            if (task.AssignedToUser != null)
            {
                string emailAddress = string.Empty;
                var emailContacts = task.AssignedToUser.PersonHasEmailContact;
                if (emailContacts != null)
                {
                    var defaultEmailContact = emailContacts.FirstOrDefault(ec => ec.EmailContactIsDefault ?? false);
                    if (defaultEmailContact != null)
                    {
                        emailAddress = defaultEmailContact.Name;
                    }
                }

                if (!String.IsNullOrEmpty(emailAddress))
                {
                    var subject = task.Name ?? DefaultSubject;
                    
                    var body = string.Format(MessageFormat, (task.Description), CreateActionLink(task));    //TODO: consider if this needs to be encoded

                    var emailSettings = Entity.Get<TenantEmailSetting>("tenantEmailSettingsInstance", TenantEmailSetting.AllFields);

                    if (emailSettings.TesApprovalsInbox != null && emailSettings.TesApprovalsInbox.UsesInboxProvider != null)
                    {
                        var mailBox = emailSettings.TesApprovalsInbox;
                        var provider = mailBox.UsesInboxProvider;
                        var tenantName = RequestContext.GetContext().Tenant.Name;

                        var sentMessage = new SentEmailMessage()
                            {
                                EmTo = emailAddress,
                                EmFrom = mailBox.InboxEmailAddress,
                                EmSubject = subject,
                                EmBody = body,
                                EmIsHtml = true,
                                EmSentDate = DateTime.UtcNow,
                                SentFromInbox = mailBox,
                                RelatedTask = task.As<DisplayFormUserTask>(),
                            };

                        sentMessage.Save();

                        var inboxProviderHelper = provider.GetHelper();
                        inboxProviderHelper.SendMessages(sentMessage.ToMailMessage().ToEnumerable(), tenantName, mailBox.Name);
                    }
                }
            }
			*/
        }

        static string CreateActionLink(DisplayFormUserTask task)
        {
            var baseAddress = ConfigurationSettings.GetSiteConfigurationSection().SiteSettings.Address;
            var tenantID = RequestContext.TenantId;
            string tenantName;

            using (new AdministratorContext())
            {
                tenantName = Entity.Get<Tenant>(tenantID, Tenant.Name_Field).Name;
            }

            string url;
            if (task.RecordToPresent != null)
            {
                url = string.Format(ActionLinkWithRecordFormat, baseAddress, tenantName, task.RecordToPresent.Id, task.Id);
            }
            else
            {
                url = string.Format(ActionLinkFormat, baseAddress, tenantName, task.Id);
            }

            if (task.FormToUse != null)
            {
                url += "&formId=" + task.FormToUse.Id;
            }

            return url;
        }

        /// <summary>
        /// Given a string with an embedded user task sequence, get the task
        /// </summary>
        /// <param name="sequenceString"></param>
        /// <returns></returns>
        public static DisplayFormUserTask GetTaskFromEmbededSequenceId(string sequenceString)
        {

                var task =
                    Entity.GetByField<DisplayFormUserTask>(sequenceString, true, DisplayFormUserTask.SequenceId_Field)
                          .FirstOrDefault();
                return task;

        }

        /// <summary>
        /// Given a string with an embedded user task sequence, get the task
        /// </summary>
        /// <param name="sequenceString"></param>
        /// <returns></returns>
        public static DisplayFormUserTask GetTaskFromLinkToken(string linkToken)
        {

            var task =
                Entity.GetByField<DisplayFormUserTask>(linkToken, true, DisplayFormUserTask.DfutLinkToken_Field)
                      .FirstOrDefault();
            return task;

        }

        


        /// <summary>
        /// Process an approval
        /// </summary>
        /// <param name="taskId">The task to approve</param>
        /// <param name="selectedOption">the name of the transition that has been selected</param>
        /// <param name="preSaveAction">An optional presave action</param>
        public static void ProcessApproval(long taskId, string selectedOption, Action<DisplayFormUserTask> preSaveAction = null)
        {
            DisplayFormUserTask task = Entity.Get<DisplayFormUserTask>(
                taskId, true, DisplayFormUserTask.UserResponse_Field,
                BaseUserTask.UserTaskCompletedOn_Field,
                DisplayFormUserTask.RelatedMessages_Field);

            var transitions = task.WorkflowRunForTask.PendingActivity.ForwardTransitions;

            var selectedTransition = transitions.First(
                       t => String.Equals(t.FromExitPoint.Name, selectedOption, StringComparison.OrdinalIgnoreCase));

            ProcessApproval(task, selectedTransition, preSaveAction);
        }

        /// <summary>
        /// Process an approval
        /// </summary>
        /// <param name="taskId">The task to approve</param>
        /// <param name="selectedOption">Teh selected transition</param>
        /// <param name="preSaveAction">An optional presave action</param>
        public static void ProcessApproval(DisplayFormUserTask task, TransitionStart selectedOption, Action<DisplayFormUserTask> preSaveAction = null)
        {
            task = task.AsWritable<DisplayFormUserTask>();

            task.UserResponse = selectedOption;
            task.UserTaskCompletedOn = DateTime.UtcNow;      // Leave this for the Before save hook, it's currently the only way to know what has changed.
            task.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;

            if (preSaveAction != null)
            {
                preSaveAction(task);
            }

            task.Save();
        }
    }
}
