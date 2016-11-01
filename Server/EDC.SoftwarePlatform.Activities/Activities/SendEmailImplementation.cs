// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Activities;
using System.Net.Mail;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.Activities
{

    public sealed class SendEmailImplementation : ActivityImplementationBase, IRunNowActivity
    {

		public const string RecipientAddressKey = "Recipient Address";
        public const string SenderAddressKey =      "Sender Address";
        public const string SubjectKey =            "Subject";
        public const string BodyKey =               "Body";

        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var recipientAddress = GetArgumentValue<string>(inputs, "core:sendEmailRecipientArgument");
            var recipientList = GetArgumentValue<IEnumerable<IEntity>>(inputs, "core:sendEmailRecipientList");
            var recipientField = GetArgumentValue<IEntity>(inputs, "core:sendEmailRecipientField");
            var subject = GetArgumentValue<string>(inputs, "core:sendEmailSubjectArgument");
            var body = GetArgumentValue<string>(inputs, "core:sendEmailBodyArgument");
            var attachments = GetArgumentEntityList (inputs, "core:sendEmailAttachments");
            var inbox = GetArgumentEntity<Inbox>(inputs, "core:sendEmailInbox");

            var sendEmailActivity = ActivityInstance.Cast<SendEmailActivity>();

            var mail = new SentEmailMessage() { EmIsHtml = true, EmSentDate = DateTime.UtcNow, };

            mail.EmTo = GenerateToString(recipientAddress, recipientList, recipientField);

            if (string.IsNullOrEmpty(mail.EmTo))
            {
                context.ExitPointId = Entity.GetId("core:sendEmailFailedSend");
                return;
            }

            if (!string.IsNullOrEmpty(subject))
                mail.EmSubject = subject;

            if (!string.IsNullOrEmpty(body))
                mail.EmBody = body;

            mail.EmFrom = inbox != null ? inbox.InboxEmailAddress : null;
            mail.SentFromInbox = inbox;
            mail.EmSentDate = DateTime.UtcNow;

            mail.EmAttachments.AddRange(Entity.Get<Document>(attachments.Select(a => a.Id)));

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


            var success = false;
            try
            {
                inboxProviderHelper.SendMessages(mailMessages, tenantName, inbox.Name);
                success = true;
            }
            catch (SmtpException ex)
            {
                RecordErrorToUserLog(context, ex);
            }   
            catch (EDC.ReadiNow.Model.EmailHelper.MissingSmtpHostException ex)
            {
                RecordErrorToUserLog(context, ex);
            }


            if (!success) 
                context.ExitPointId = Entity.GetId("core:sendEmailFailedSend");

        }

        void RecordErrorToUserLog(IRunState context, Exception ex)
        {
            context.Log(new WorkflowRunLogEntry
            {
                LogEntrySeverity_Enum = LogSeverityEnum_Enumeration.WarningSeverity,
                Name = "Email send failed",
                Description = ex.Message
            });
        }

        static string GenerateToString(string recipientAddress, IEnumerable<IEntity> recipientList, IEntity recipientField)
        {
            var addresses = new List<string>();
            
            if (!String.IsNullOrWhiteSpace(recipientAddress))
                addresses.Add(recipientAddress);

            if (recipientList != null)
            {
                
                var recipients = Entity.Get<Resource>(recipientList.Select(e => e.Id), recipientField).ToArray();

                foreach (var recipient in recipients)
                {
                    var emailAddress = recipient.GetField<string>(recipientField);
                    if (!string.IsNullOrWhiteSpace(emailAddress))
                        addresses.Add(emailAddress);
                }
            }

            return string.Join(";", addresses);
        }

    }
}
