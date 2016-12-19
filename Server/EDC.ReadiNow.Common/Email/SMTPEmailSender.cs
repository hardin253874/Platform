// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Email
{
    public interface IEmailSender
    {
        void SendMessages(IReadOnlyCollection<MailMessage> messages);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly TenantEmailSetting _emailServerSettings;

        public SmtpEmailSender(TenantEmailSetting emailServerSettings)
        {
            _emailServerSettings = emailServerSettings;
        }

        public void SendMessages(IReadOnlyCollection<MailMessage> messages)
        {
            foreach ( var message in messages)
	        {

                if (message.From == null)
                    message.From = new MailAddress(_emailServerSettings.EmailNoReplyAddress);

                if (!string.IsNullOrEmpty(_emailServerSettings.TestingOverrideToAddress))
                {
                    message.To.Clear();
                    message.To.Add(_emailServerSettings.TestingOverrideToAddress);
                }


                string sequenceId = message.Headers[EmailHelper.SoftwarePlatformSequenceIdHeader];

                if (!string.IsNullOrEmpty(sequenceId))
                    message.Headers[EmailHelper.MessageIdHeader] = EmailHelper.GenerateMessageId(sequenceId, _emailServerSettings.SmtpServer);

	        }

            string password = _emailServerSettings.SmtpPasswordSecureId != null ? Factory.SecuredData.Read((Guid)_emailServerSettings.SmtpPasswordSecureId) : string.Empty;
            EmailHelper.Send(messages, _emailServerSettings.SmtpServer, _emailServerSettings.SmtpPort ?? EmailHelper.DefaultSmtpPort, _emailServerSettings.SmtpUseSSL ?? true, _emailServerSettings.PostInDirectory ?? true, _emailServerSettings.SmtpAccount, password);
        }

    }
}
