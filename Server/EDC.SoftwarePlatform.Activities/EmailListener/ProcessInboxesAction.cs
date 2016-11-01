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

namespace EDC.SoftwarePlatform.Activities.EmailListener
{
    public class ProcessInboxesAction : ItemBase
    {
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
                var allTenants = Entity.GetInstancesOfType<Tenant>(false);

                foreach (var tenant in allTenants)
                {
                    // need to get tenant info in the admin context
                    var tenantId = tenant.Id;      
                    var tenantName = tenant.Name;

                    using (new TenantAdministratorContext(tenant.Id))
                    {
                        EventLog.Application.WriteTrace("Starting inbox scan for tenant '{0}' ({1})", tenantName, tenantId);
                        var inboxes = Entity.GetInstancesOfType<Inbox>().Where(inbox => inbox.InboxEnabled ?? true);

                        foreach (var inbox in inboxes)
                        {
                            try
                            {
                                ScanInbox(tenantName, inbox);
                            }
                            catch (Exception ex)
                            {
                                EventLog.Application.WriteError("Failed to scan inbox {0} for tenant '{1}' ({2})", inbox.Name, tenantName, ex.ToString());
                            }
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Scans the inbox.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="inbox">The inbox.</param>
        void ScanInbox( string tenantName, Inbox inbox )
        {
            var provider = inbox.UsesInboxProvider;

            if (provider == null)
                return;

			var formatters = new List<IMailMessageFormatter<ReceivedEmailMessage>>( );

	        IEntityCollection<Class> mailMessageFormatters = inbox.MailMessageFormatter;

	        /////
	        // Always use the default message formatter.
	        /////
	        formatters.Add( new DefaultMailMessageFormatter( ) );

	        /////
	        // Add any additional formatters.
	        /////
			if ( mailMessageFormatters != null && mailMessageFormatters.Count > 0 )
	        {
		        foreach ( Class formatterClass in mailMessageFormatters )
		        {
			        var mailMessageFormatter = formatterClass.Activate<IMailMessageFormatter<ReceivedEmailMessage>>( );

			        /////
			        // Only allow one instance of each formatter type.
			        /////
			        if ( formatters.All( formatter => formatter.GetType( ) != mailMessageFormatter.GetType( ) ) )
			        {
				        formatters.Add( mailMessageFormatter );
			        }
		        }
	        }

	        /////
	        // Delegate that creates new instances of received messages.
	        /////
			var createMessage = new Func<MailMessage, ReceivedEmailMessage>( msg =>
				{
					ReceivedEmailMessage returnMessage = null;

					Class inboxReceivedMessageType = inbox.InboxReceivedMessageType;

					if ( inboxReceivedMessageType != null )
					{
						var entity = inboxReceivedMessageType.Activate<IEntity>( );

						if ( entity != null )
						{
							returnMessage = entity.As<ReceivedEmailMessage>( );
						}
					}

					if ( returnMessage == null )
					{
						returnMessage = new ReceivedEmailMessage( );
					}

					returnMessage.Name = msg.Subject;

					return returnMessage;
				} );
	        

	        var messages = new List<ReceivedEmailMessage>( );
            var inboxProviderHelper = provider.GetHelper();
                        
	        /////
	        // Process each message passing them through each formatter.
	        /////
	        foreach ( MailMessage message in inboxProviderHelper.GetMessages( tenantName, inbox.Name ) )
	        {
		        /////
		        // Create the message type. (Type can be defined declaratively).
		        /////
		        var receivedMessage = createMessage( message );

				bool process = true;

		        foreach ( IMailMessageFormatter<ReceivedEmailMessage> formatter in formatters )
		        {
			        /////
			        // Run the formatter.
			        /////
			        MailMessageFormatterResult mailMessageFormatterResult = formatter.Format( message, receivedMessage, inbox );

			        if ( mailMessageFormatterResult == MailMessageFormatterResult.Reject || mailMessageFormatterResult == MailMessageFormatterResult.Error )
			        {
				        receivedMessage.Dispose( );
				        process = false;
						break;
			        }
		        }

		        if ( process )
		        {
			        messages.Add( receivedMessage );
		        }
	        }

	        if ( messages.Count <= 0 )
	        {
		        return;
	        }

			if ( messages.Count > 0 )
            {
				SaveWithActions( inbox, messages );

				StartWorkflows( inbox, messages );
            }
        }

        private void SaveWithActions(Inbox inbox, IList<ReceivedEmailMessage> messages)
        {
            var notSaved = new List<ReceivedEmailMessage>();
            var postSaveActions = new List<Action> ();

            var actions =
                inbox.InboxEmailActions.OrderBy(a => a.IeaOrdinal ?? 100)
                     .Where(a => a.IeaBackingClass != null)
                     .Select(a => a.IeaBackingClass.Activate<IEmailAction>());

	        IList<IEmailAction> emailActions = actions as IList<IEmailAction> ?? actions.ToList( );

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