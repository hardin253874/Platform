// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.Security;
using ReadiNow.Integration.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Notify
{
    // 
    // IN PROGRESS - Please leave
    // This code is in development and switched off until email and SMS approvals are required by PM.
    //
    public class EmailRouter: INotificationRouter
    {
        const string MissingEmailAddressMessage = "The person does not have an email address entry.";


        public static EmailRouter Instance { get; } = new EmailRouter();      // TODO: switch to DI

        public void Send(IEntity notifier, Notification notification, IEnumerable<IEntity> people, bool expectReply)
        {
            //
            // TODO: Refactor this and SendEmail
            // TODO: Deal with send failures
            // TODO: deal with replies
            //

            var sends = people.Select(p =>
            {
                var sendRecord = Entity.Create<SendRecord>();
                //receipt.NrReceiptToken = CryptoHelper.GetRandomPrintableString(16);               // TODO handle matching email responses
                sendRecord.SrToPerson = p?.Cast<Person>();
                sendRecord.SendToNotification = notification;
                return sendRecord;
            });

            

            var emailNotifier = notifier.Cast<EmailNotifier>();

            var inbox = notifier.GetRelationships("core:emailNotifierInbox").FirstOrDefault()?.As<Inbox>();

            if (inbox == null)
                throw new ArgumentException("core:emailNotifierInbox");

            var mail = new SentEmailMessage() { EmIsHtml = true, EmSentDate = DateTime.UtcNow, };

            var addresses = FetchAddresses(people, emailNotifier.EnEmailAddressExpression);

            sends = sends.Zip<SendRecord, string, SendRecord>(addresses, (send, address) =>    
            {
                if (address == null)
                    send.SrErrorMessage = MissingEmailAddressMessage;
                return send;
            });

            mail.EmTo = String.Join(",", addresses.Where(a => a != null));

            if (!string.IsNullOrEmpty(mail.EmTo))
            {
                mail.EmSubject = notifier.GetField<string>("core:enSubject");

                if (!string.IsNullOrEmpty(notification.NMessage))
                    mail.EmBody = notification.NMessage;


                mail.EmFrom = inbox?.InboxEmailAddress;
                mail.SentFromInbox = inbox;
                mail.EmSentDate = DateTime.UtcNow;

                mail.Save();

                var mailMessages = mail.Cast<SentEmailMessage>().ToMailMessage().ToEnumerable();

                var provider = inbox.UsesInboxProvider;

                var tenantId = RequestContext.GetContext().Tenant.Id;
                string tenantName;
                using (new AdministratorContext())
                {
                    tenantName = Entity.Get<Tenant>(tenantId, Resource.Name_Field).Name;
                }

                var inboxProviderHelper = provider.GetHelper();

                inboxProviderHelper.SendMessages(mailMessages, tenantName, inbox.Name);
            }

            Entity.Save(sends);
        }

        /// <summary>
        /// Take the people and the expression and produce a list of numbers or nulls
        /// </summary>
        /// <param name="people"></param>
        /// <param name="expressionString"></param>
        /// <param name="missing"></param>
        /// <returns></returns>
        IEnumerable<string> FetchAddresses(IEnumerable<IEntity> people, string expressionString)
        {
            if (expressionString == null)
                throw new ArgumentException(nameof(expressionString));

            var bSettings = new BuilderSettings
            {
                ScriptHost = ScriptHostType.Evaluate,
                ExpectedResultType = ExprType.String,
                RootContextType = ExprTypeHelper.EntityOfType(Person.Person_Type)
            };

            var expression = Factory.ExpressionCompiler.Compile(expressionString, bSettings);
            var expressionRunner = Factory.ExpressionRunner;


            var addresses = people.Select(p =>
            {
                var evalSettings = new EvaluationSettings { ContextEntity = p };
                return (string)expressionRunner.Run(expression, evalSettings).Value;
            });

            return addresses;
        }

    }
}
