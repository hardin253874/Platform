// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging.Mail;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Messaging.Mail;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Email;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.Activities.EmailListener
{
    public class ProcessInboxesAction : ItemBase
    {
        IEmailReceiver _emailReceiver = null;

        public ProcessInboxesAction()
        {
        }

        public ProcessInboxesAction(IEmailReceiver emailReceiver)
        {
            _emailReceiver = emailReceiver;
        }

        IEmailReceiver EmailReceiver
        {
            get
            {
                if (_emailReceiver == null)
                    _emailReceiver = GetIMAPEmailReceiver();
                return _emailReceiver;
            }
        }

        protected override bool RunAsOwner
        {
            get
            {
                return false;       // runs in global context
            }
        }

        /// <summary>
        /// Run the email listener. This opens an email connection to a server and account and processes all the mail headers through the 
        /// configured email scanners.
        /// WARNING! This relies on a global, system only schedule. This is not being configured correctly at installation!
        /// </summary>
        /// <param name="scheduledItemRef"></param>
        public override void Execute(EntityRef scheduledItemRef)
        {
            using (new AdministratorContext())
            {
                // fetch all the emails addressed to any inbox in any tenant on this system
                var toAddressFilters = GetToAddressFilterForAllInboxes();
                var emailMessages = EmailReceiver.GetAllMessages(toAddressFilters);

                foreach (var tenant in Entity.GetInstancesOfType<Tenant>(false))
                {
                    ProcessMessagesForTenant(tenant, emailMessages);
                }
            }
        }

        IEmailReceiver GetIMAPEmailReceiver()
        {
            // Get the IMAP server connection details
            var imapSettings = Entity.Get<ImapServerSettings>("core:imapServerSettingsInstance");
            var useSSL = imapSettings.ImapUseSSL ?? false;
            var port = imapSettings.ImapPort ?? (useSSL ? ImapEmailReceiver.DefaultSslPort : ImapEmailReceiver.DefaultNonSslPort);
            var folderName = string.IsNullOrEmpty(imapSettings.ImapFolder) ? null : imapSettings.ImapFolder;
            var secureId = imapSettings.ImapPasswordSecureId;
            var password = secureId != null ? Factory.SecuredData.Read((Guid)secureId) : String.Empty;

            if (string.IsNullOrEmpty(imapSettings.ImapServer))
                throw new Exception("IMAP Server has not been configured");

            if (string.IsNullOrEmpty(imapSettings.ImapAccount))
                throw new Exception("IMAP Server connection account has not been configured");

            return new ImapEmailReceiver(imapSettings.ImapServer, port, useSSL, imapSettings.ImapAccount, password, folderName, EventLog.Application);
        }

        List<string> GetToAddressFilterForAllInboxes()
        {
            var toFilters = new List<string>();
            foreach (var tenant in Entity.GetInstancesOfType<Tenant>(false))
            {
                using (new TenantAdministratorContext(tenant.Id))
                {
                    foreach (var inbox in Entity.GetInstancesOfType<Inbox>().Where(inbox => inbox.InboxEnabled ?? true))
                    {
                        toFilters.Add(inbox.InboxEmailAddress);
                    }
                }
            }
            return toFilters;
        }

        /// <summary>
        /// Loads and proceses the emails for all Inboxes for the specified tenant
        /// </summary>
        void ProcessMessagesForTenant(Tenant tenant, IReadOnlyCollection<MailMessage> emailMessages)
        {
            EventLog.Application.WriteTrace("Starting scan of inboxes for tenant '{0}' ({1})", tenant.Name, tenant.Id);

            using (new TenantAdministratorContext(tenant.Id))
            {
                var inboxes = Entity.GetInstancesOfType<Inbox>().Where(inbox => inbox.InboxEnabled ?? true).ToList();
                var toEmailAddresses = inboxes.Select(x => x.InboxEmailAddress);
                
                foreach (var inbox in inboxes)
                {
                    try
                    {
                        var inboxesMessages = emailMessages.Where(msg => DoesMessageMatchToAddress(msg, inbox.InboxEmailAddress)).ToList();
                        ProcessEmailsForInbox(inbox, inboxesMessages);
                    }
                    catch (Exception ex)
                    {
                        EventLog.Application.WriteError("Failed to process emails messages for inbox {0} for tenant '{1}' ({2})", inbox.Name, tenant.Name, ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Processes and saves the email messages for the Inbox
        /// </summary>
        /// <param name="inbox">The inbox that the messages are to be loaded for</param>
        /// <param name="emailMessages">List of email messages who's To address matches the Inboxes email address</param>
        void ProcessEmailsForInbox(Inbox inbox, IReadOnlyCollection<MailMessage> emailMessages)
        {
            EventLog.Application.WriteTrace("Processing {0} emails for inbox '{1}'", emailMessages.Count, inbox.Name);

            var formatters = GetEmailMessageFormatters(inbox);
            var messages = new List<ReceivedEmailMessage>();
            
            foreach (MailMessage message in emailMessages)
            {
                var receivedMessage = CreateReceivedEmailMessage(message, inbox.InboxReceivedMessageType);

                bool process = true;
                foreach (IMailMessageFormatter<ReceivedEmailMessage> formatter in formatters)
                {
                    MailMessageFormatterResult mailMessageFormatterResult = formatter.Format(message, receivedMessage, inbox);

                    if (mailMessageFormatterResult == MailMessageFormatterResult.Reject || mailMessageFormatterResult == MailMessageFormatterResult.Error)
                    {
                        receivedMessage.Dispose();
                        process = false;
                        break;
                    }
                }

                if (process)
                    messages.Add(receivedMessage);
            }

            if (messages.Count <= 0)
                return;

            if (messages.Count > 0)
            {
                SaveWithActions(inbox, messages);

                StartWorkflows(inbox, messages);
            }
        }

        #region Email Filter Functions

        bool DoesMessageMatchToAddress(MailMessage message, string matchToAddress)
        {
            return message.To.Select(to => to.Address.ToLower()).Any(address => address == matchToAddress.ToLower());
        }


        #endregion

        /// <summary>
        /// Get the formatters to process the email messages
        /// </summary>
        /// <param name="inbox"></param>
        /// <returns></returns>
        List<IMailMessageFormatter<ReceivedEmailMessage>> GetEmailMessageFormatters(Inbox inbox)
        {
            var formatters = new List<IMailMessageFormatter<ReceivedEmailMessage>>();

            /////
            // Always use the default message formatter.
            /////
            formatters.Add(new DefaultMailMessageFormatter());

            /////
            // Add any additional formatters.
            /////
            IEntityCollection<Class> mailMessageFormatters = inbox.MailMessageFormatter;

            if (mailMessageFormatters != null && mailMessageFormatters.Count > 0)
            {
                foreach (Class formatterClass in mailMessageFormatters)
                {
                    var mailMessageFormatter = formatterClass.Activate<IMailMessageFormatter<ReceivedEmailMessage>>();

                    /////
                    // Only allow one instance of each formatter type.
                    /////
                    if (formatters.All(formatter => formatter.GetType() != mailMessageFormatter.GetType()))
                    {
                        formatters.Add(mailMessageFormatter);
                    }
                }
            }

            return formatters;
        }

        /// <summary>
        /// Create a ReceivedEmailMessage entity from a mail message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="inboxReceivedMessageType"></param>
        /// <returns></returns>
        ReceivedEmailMessage CreateReceivedEmailMessage(MailMessage msg, Class inboxReceivedMessageType)
        {
            ReceivedEmailMessage returnMessage = null;

            if (inboxReceivedMessageType != null)
            {
                var entity = inboxReceivedMessageType.Activate<IEntity>();

                if (entity != null)
                {
                    returnMessage = entity.As<ReceivedEmailMessage>();
                }
            }

            if (returnMessage == null)
            {
                returnMessage = new ReceivedEmailMessage();
            }

            returnMessage.Name = msg.Subject;

            return returnMessage;
        }


        private void SaveWithActions(Inbox inbox, IList<ReceivedEmailMessage> messages)
        {
            var notSaved = new List<ReceivedEmailMessage>();
            var postSaveActions = new List<Action>();

            var actions =
                inbox.InboxEmailActions.OrderBy(a => a.IeaOrdinal ?? 100)
                     .Where(a => a.IeaBackingClass != null)
                     .Select(a => a.IeaBackingClass.Activate<IEmailAction>());

            IList<IEmailAction> emailActions = actions as IList<IEmailAction> ?? actions.ToList();
            
            EventLog.Application.WriteTrace("Processing {0} messages with {1} actions", messages.Count, emailActions.Count);

            foreach (var action in emailActions)
            {
                foreach (var message in messages)
                {
                    Action postSaveAction = null;

                    try
                    {
                        if (action.BeforeSave(message, out postSaveAction))
                            notSaved.Add(message);
                    }
                    catch (Exception ex)
                    {
                        EventLog.Application.WriteError("Action '{0}' failed with exception, ignoring:\n'{1}'", action.GetType().FullName, ex.ToString());
                    }


                    if (postSaveAction != null)
                    {
                        try
                        {
                            postSaveActions.Add(postSaveAction);
                        }
                        catch (Exception ex)
                        {
                            EventLog.Application.WriteError("Action '{0}' failed with exception, ignoring:\n'{1}'",
                                                            action.GetType().FullName, ex.ToString());
                        }
                    }

                }
            }

            var savedMessages = messages.Except(notSaved);

            Entity.Save(savedMessages);


            foreach (var postSaveAction in postSaveActions)
            {
                postSaveAction();
            }

            EventLog.Application.WriteTrace("{0} messages saved", messages.Count);
        }

        private void StartWorkflows(Inbox inbox, IList<ReceivedEmailMessage> messages)
        {
            //
            // Fire off the workflows
            //
            foreach (var workflow in inbox.InboxWorkflows)
            {
                var resourceArg = workflow.InputArgumentForAction;

                if (resourceArg == null)
                {
                    // no argument means the workflow is run once for the set of messages
                    RunWorkflow(workflow);
                }
                else if (resourceArg.Is<ResourceArgument>())
                {
                    // resourceArguments run the workflow once per messages
                    foreach (var message in messages)
                        RunWorkflow(workflow, resourceArg.Name, message);
                }
                else if (resourceArg.Is<ResourceListArgument>())
                {
                    // resourceListArguments run the workflow once per set of messages
                    RunWorkflow(workflow, resourceArg.Name, messages);
                }
            }
        }

        void RunWorkflow(Workflow workflow, string argName, ReceivedEmailMessage message)
        {
            var args = new Dictionary<string, object>
                {{argName, new EntityRef(message)}};

            WorkflowRunner.Instance.RunWorkflow(new WorkflowStartEvent(workflow) { Arguments = args });
        }

        void RunWorkflow(Workflow workflow, string argName, IEnumerable<ReceivedEmailMessage> messages)
        {
            var args = new Dictionary<string, object>
                {{argName, messages.Select(m => new EntityRef(m)).ToList()}};

            WorkflowRunner.Instance.RunWorkflow(new WorkflowStartEvent(workflow) { Arguments = args });
        }

        void RunWorkflow(Workflow workflow)
        {
            var args = new Dictionary<string, object>();

            WorkflowRunner.Instance.RunWorkflow(new WorkflowStartEvent(workflow));
        }

    }
}