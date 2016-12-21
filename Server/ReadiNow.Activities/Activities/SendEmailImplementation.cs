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
using EDC.ReadiNow.Email;
using System.Text.RegularExpressions;
using Autofac;

namespace EDC.SoftwarePlatform.Activities
{

    public sealed class SendEmailImplementation : ActivityImplementationBase, IRunNowActivity
    {
        IEmailSender _emailSender;
        TenantEmailSetting _tenantEmailSetting = null;
        ActivityInputs _activityInputs = null;
        string _fromAddress = null;
        string _fromName = null;

        public IEmailSender EmailSender
        {
            get
            {
                if (_emailSender == null)
                {
                    if (Factory.Current.IsRegistered<IEmailSender>())
                        _emailSender = Factory.Current.Resolve<IEmailSender>();
                }
                if (_emailSender == null)
                    _emailSender = new SmtpEmailSender(TenantEmailSetting);
                return _emailSender;
            }
            set { _emailSender = value; }
        }

        private ActivityInputs ActivityInputs
        {
            get { return _activityInputs; }
        }

        private TenantEmailSetting TenantEmailSetting
        {
            get
            {
                if (_tenantEmailSetting == null)
                    _tenantEmailSetting = Entity.Get<TenantEmailSetting>("core:tenantEmailSettingsInstance");
                return _tenantEmailSetting;
            }
        }
        
        private string FromAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_fromAddress))
                {
                    var noReply = GetArgumentValue<bool>(ActivityInputs, "core:sendEmailNoReply", true);
                    if (noReply)
                    {
                        _fromAddress = TenantEmailSetting.EmailNoReplyAddress;
                        return _fromAddress;
                    }

                    var fromInbox = GetArgumentValue<IEntity>(ActivityInputs, "core:sendEmailFromInbox");
                    if (fromInbox == null)
                    {
                        _fromAddress = TenantEmailSetting.EmailNoReplyAddress;
                        return _fromAddress;
                    }

                    var inbox = fromInbox.As<Inbox>();

                    if (!(inbox.InboxEnabled ?? true))
                        throw new Exception($"Unable to send email from inbox '{inbox.Name}'. This Inbox has been disabled.");

                    if (!string.IsNullOrEmpty(inbox.InboxReplyAddress))
                        _fromAddress = inbox.InboxReplyAddress;
                    else
                        _fromAddress = inbox.InboxEmailAddress;
                }
                return _fromAddress;
            }
        }
        private string FromName
        {
            get
            {
                if (string.IsNullOrEmpty(_fromName))
                {
                    var noReply = GetArgumentValue<bool>(ActivityInputs, "core:sendEmailNoReply", true);
                    if (noReply)
                        return null;

                    var fromInbox = GetArgumentValue<IEntity>(ActivityInputs, "core:sendEmailFromInbox");
                    if (fromInbox == null)
                        return null;

                    var inbox = fromInbox.As<Inbox>();

                    if (!(inbox.InboxEnabled ?? true))
                        throw new Exception($"Unable to send email from inbox '{inbox.Name}'. This Inbox has been disabled.");

                    if (!string.IsNullOrEmpty(inbox.InboxFromName))
                        _fromName = inbox.InboxFromName;
                    else
                        _fromName = inbox.Name;
                }
                return _fromName;
            }
        }
        
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            _activityInputs = inputs;
            try
            {
                if (string.IsNullOrEmpty(TenantEmailSetting.SmtpServer))
                    throw new Exception("smtpServer has not been specified.");
                
                var sendEmailRecipientsType = GetArgumentValue<Resource>(inputs, "core:sendEmailRecipientsType");

                var sentEmailMessages = new List<SentEmailMessage>();
                if (string.IsNullOrEmpty(sendEmailRecipientsType?.Alias) || (sendEmailRecipientsType?.Alias == "core:sendEmailActivityRecipientsAddress"))
                {
                    sentEmailMessages.Add(GenerateEmailToRecipientsExpression());
                }
                else
                {
                    var sendEmailDistributionType = GetArgumentValue<Resource>(inputs, "core:sendEmailDistributionType");
                    if (string.IsNullOrEmpty(sendEmailDistributionType?.Alias) || (sendEmailDistributionType?.Alias == "core:sendEmailActivityGroupDistribution"))
                        sentEmailMessages.Add(GenerateGroupEmailsToRecipientsList());
                    else
                        sentEmailMessages.AddRange(GenerateIndividualEmailsToRecipientsList());
                }

                var mailMessages = new List<MailMessage>();
                sentEmailMessages.RemoveAll(msg => string.IsNullOrEmpty(msg.EmTo) && string.IsNullOrEmpty(msg.EmCC) && string.IsNullOrEmpty(msg.EmBCC));
                sentEmailMessages.RemoveAll(msg => string.IsNullOrEmpty(msg.EmSubject));
                sentEmailMessages.ForEach(msg =>
                {
                    msg.EmSentDate = DateTime.UtcNow;
                    msg.Save();
                    mailMessages.Add(msg.ToMailMessage());
                });

                RecordInfoToUserLog(context, $"Sending {sentEmailMessages.Count} emails");

                if (sentEmailMessages.Count == 0)
                {
                    context.ExitPointId = Entity.GetId("core:sendEmailSucceededSend");
                    return;
                }

                EmailSender.SendMessages(mailMessages);
               
                context.SetArgValue(ActivityInstance, GetArgumentKey("core:outSentEmailMessages"), sentEmailMessages);
                context.ExitPointId = Entity.GetId("core:sendEmailSucceededSend");
            }
            catch (Exception ex)
            {
                RecordErrorToUserLog(context, ex);
                context.ExitPointId = Entity.GetId("core:sendEmailFailedSend");
            }
        }

        SentEmailMessage GenerateEmailToRecipientsExpression()
        {
            var to = GetArgumentValue<string>(ActivityInputs, "core:sendEmailRecipientArgument");
            var cc = GetArgumentValue<string>(ActivityInputs, "core:sendEmailRecipientCCArgument");
            var bcc = GetArgumentValue<string>(ActivityInputs, "core:sendEmailRecipientBCCArgument");

            var mail = new SentEmailMessage()
            {
                EmIsHtml = true,
                SentFromEmailServer = TenantEmailSetting,
                EmFrom = FromAddress,
                EmFromName = FromName,
                EmTo = GetValidEmailAddresses(to),
                EmCC = GetValidEmailAddresses(cc),
                EmBCC = GetValidEmailAddresses(bcc),
                EmSubject = GetArgumentValue<string>(ActivityInputs, "core:sendEmailSubjectArgument"),
                EmBody = GetArgumentValue<string>(ActivityInputs, "core:sendEmailBodyArgument")
            };
            
            var attachments = GetArgumentEntityList(ActivityInputs, "core:sendEmailAttachments");
            mail.EmAttachments.AddRange(Entity.Get<FileType>(attachments.Select(a => a.Id)));
            return mail;
        }

        SentEmailMessage GenerateGroupEmailsToRecipientsList()
        {
            var recipientList = GetArgumentValue<IEnumerable<IEntity>>(ActivityInputs, "core:sendEmailRecipientList");
            var recipientListTOField = GetArgumentValue<IEntity>(ActivityInputs, "core:sendEmailRecipientField");
            var recipientListCCField = GetArgumentValue<IEntity>(ActivityInputs, "core:sendEmailRecipientCCField");
            var recipientListBCCField = GetArgumentValue<IEntity>(ActivityInputs, "core:sendEmailRecipientBCCField");

            if ((recipientListTOField == null) && (recipientListCCField == null) && (recipientListBCCField == null))
                throw new Exception("A TO, CC or BCC recipient field has not been specified");

            var mail = new SentEmailMessage()
            {
                EmIsHtml = true,
                SentFromEmailServer = TenantEmailSetting,
                EmFrom = FromAddress,
                EmFromName = FromName,
                EmTo = (recipientListTOField != null) ? GetEmailAddressesFromRecipientsList(recipientList, recipientListTOField) : null,
                EmCC = (recipientListCCField != null) ? GetEmailAddressesFromRecipientsList(recipientList, recipientListCCField) : null,
                EmBCC = (recipientListBCCField != null) ? GetEmailAddressesFromRecipientsList(recipientList, recipientListBCCField) : null,
                EmSubject = GetArgumentValue<string>(ActivityInputs, "core:sendEmailSubjectArgument"),
                EmBody = GetArgumentValue<string>(ActivityInputs, "core:sendEmailBodyArgument")
            };

            var attachments = GetArgumentEntityList(ActivityInputs, "core:sendEmailAttachments");
            mail.EmAttachments.AddRange(Entity.Get<FileType>(attachments.Select(a => a.Id)));
            return mail;
        }
        
        /// <summary>
        /// This method is incomplete as it does not generate correctly use the Recipient list to generate the email subject and body components.
        /// </summary>
        /// <returns></returns>
        List<SentEmailMessage> GenerateIndividualEmailsToRecipientsList()
        {
            var recipientList = GetArgumentValue<IEnumerable<IEntity>>(ActivityInputs, "core:sendEmailRecipientList");
            var recipientListTOField = GetArgumentValue<IEntity>(ActivityInputs, "core:sendEmailRecipientField");
            var recipientListCCField = GetArgumentValue<IEntity>(ActivityInputs, "core:sendEmailRecipientCCField");
            var recipientListBCCField = GetArgumentValue<IEntity>(ActivityInputs, "core:sendEmailRecipientBCCField");
            var attachments = GetArgumentEntityList(ActivityInputs, "core:sendEmailAttachments");

            if ((recipientListTOField == null) && (recipientListCCField == null) && (recipientListBCCField == null))
                throw new Exception("A TO, CC or BCC recipient field has not been specified");

            //var recipients = Entity.Get<Resource>(recipientList.Select(e => e.Id), recipientField).ToArray();

            var emails = new List<SentEmailMessage>();
            foreach (var recipient in recipientList)
            {
                var mail = new SentEmailMessage()
                {
                    EmIsHtml = true,
                    SentFromEmailServer = TenantEmailSetting,
                    EmFrom = FromAddress,
                    EmFromName = FromName,
                    EmTo = GetEmailAddressFromRecipientEntity(recipient, recipientListTOField),
                    EmCC = GetEmailAddressFromRecipientEntity(recipient, recipientListCCField),
                    EmBCC = GetEmailAddressFromRecipientEntity(recipient, recipientListBCCField),
                    EmSubject = GetArgumentValue<string>(ActivityInputs, "core:sendEmailSubjectArgument"),
                    EmBody = GetArgumentValue<string>(ActivityInputs, "core:sendEmailBodyArgument")
                };

                mail.EmAttachments.AddRange(Entity.Get<FileType>(attachments.Select(a => a.Id)));
                emails.Add(mail);
            }
            return emails;
        }
        
        static string GetValidEmailAddresses(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var validAddresses = str.Split(new char[] { ';', ',', ':' }, StringSplitOptions.RemoveEmptyEntries).Where(s => IsValidEmailAddress(s.Trim()));
            return string.Join(";", validAddresses);
        }
        /// <summary>
        /// Returns a string containing a list of valid email addresses taken from the specified field value on 
        /// each of the entities in the recipients list.
        /// </summary>
        static string GetEmailAddressesFromRecipientsList(IEnumerable<IEntity> recipientList, IEntity field)
        {

            if ((recipientList == null) || (field == null))
                return null;

            var recipients = Entity.Get<Resource>(recipientList.Select(e => e.Id), field).ToArray();

            var addresses = new List<string>();
            foreach (var recipient in recipients)
            {
                var emailAddress = recipient.GetField<string>(field);
                if (IsValidEmailAddress(emailAddress))
                    addresses.Add(emailAddress);
            }

            return string.Join(";", addresses);
        }

        /// <summary>
        /// Gets the specified field value from the entity and returns the value if it is a valid email address, 
        /// otherwise returns null
        /// </summary>
        static string GetEmailAddressFromRecipientEntity(IEntity recipientEntity, IEntity field)
        {
            if (field == null)
                return null;

            var address = recipientEntity.GetField<string>(field);
            if (!IsValidEmailAddress(address))
                return null;

            return address;
        }

        /// <summary>
        /// Return true if strIn is in valid e-mail format.
        /// </summary>
        static bool IsValidEmailAddress(string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;
            
            try
            {
                return Regex.IsMatch(str,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
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

        void RecordInfoToUserLog(IRunState context, string message)
        {
            context.Log(new WorkflowRunLogEntry
            {
                LogEntrySeverity_Enum = LogSeverityEnum_Enumeration.InformationSeverity,
                Name = "Email send information",
                Description = message
            });
        }

    }
}
