// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.Interfaces;
using EDC.Security;
using Quartz;
using S22.Imap;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Model
{
    public class ImapEmailProviderHelper : IInboxProviderHelper
    {
        private ImapEmailProvider _provider;

        internal ImapEmailProviderHelper(ImapEmailProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            _provider = provider;
        }

        private const int DefaultNonSslPort = 143;
        private const int DefaultSslPort = 993;

        static readonly Regex ValidMailboxNameRegex = new Regex("[a-zA-Z][a-zA-Z0-9-_]*");

        private const string EmailAddressFormat = "{0}<{1}@{2}>";

        public bool IsValidInboxName(string name)
        {
            return ValidMailboxNameRegex.IsMatch(name);
        }


        public string NameToEmailAddress(string tenantName, string mailboxName)
        {
            if (_provider.OaEmailAddress != null)
            {
                return _provider.OaEmailAddress;
            }
            else
            {
                return string.Format(EmailAddressFormat, CleanName(mailboxName), CleanName(tenantName), _provider.OaDomainName);
            }
            
        }

        string CleanName(String s)
        {
            return Regex.Replace(s, @"[^A-Za-z0-9_]+", "-");    
        }

        public void CreateInbox(string tenantName, Inbox inbox)
        {
            if (!_provider.OaUseFolder ?? false)
            {
                // TODO: implement
            }
        }

        public void DeleteInbox(string tenantName, Inbox inbox)
        {
            if (!_provider.OaUseFolder ?? false)
            {
                // TODO: implement
            }
        }
         
        public IEnumerable<MailMessage> GetMessages( string tenantName, string inboxName)
        {
            var useSSL = _provider.OaUseSSL ?? false;
            var port = _provider.OaPort ?? (useSSL ? DefaultSslPort : DefaultNonSslPort);
            var folderName = (_provider.OaUseFolder ?? false) ? inboxName : null;

            using (var Client = new ImapClient(_provider.OaServer, port, useSSL))
            {
                EventLog.Application.WriteTrace(
                    string.Format("EmailListenerJob: Starting scan of mail box '{0}':'{1}'/'{2}' on '{3}'.",
                        inboxName, _provider.OaAccount, folderName ?? "default",
                        _provider.OaServer
                        ));

                var authMethod = _provider.OaUseIntegratedAuth ?? false ? AuthMethod.Windows : AuthMethod.Auto;
                try
                {
                    var secureId = _provider.OaPasswordSecureId;
                    var password = secureId != null ? Factory.SecuredData.Read((Guid)secureId) : String.Empty;

                    Client.Login(_provider.OaAccount, password, authMethod);
                }
                catch (InvalidCredentialsException ex)
                {
                    throw new JobExecutionException(
                        string.Format(
                            "EmailListenerJob: The email server rejected the supplied credentials: server:'{0}'  account:'{1}'   message:{2}",
                            _provider.OaServer, _provider.OaAccount, ex.ToString()));
                }

                var mbInfo = Client.GetMailboxInfo(folderName);

                EventLog.Application.WriteTrace(
                    string.Format(string.Format("EmailListenerJob for mailbox '{0}': {1} messages", inboxName,
                                                mbInfo.Messages)));

                //
                // fetch the headers for mail messages 
                //
                var uids = Client.Search(SearchCondition.All(), folderName);

                EventLog.Application.WriteTrace(string.Format("EmailListenerJob: Headers to process:'{0}'",
                                                                    uids.Length));
                //
                // fetch the messages and create a message to uid map to facilitate deletes.
                //
                MailMessage[] messages = Client.GetMessages(uids, FetchOptions.Normal, true, folderName);

                foreach (var uid in uids)
                    Client.DeleteMessage(uid, folderName);

                Client.Expunge(folderName);

                return messages;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="tenantName"></param>
        /// <param name="inboxName"></param>
        /// <returns>true on success</returns>
        public void SendMessages(IEnumerable<MailMessage> messages, string tenantName, string inboxName)
        {
	        /////
	        // Stop multiple enumerations.
	        /////
	        var mailMessages = messages as IList<MailMessage> ?? messages.ToList( );

	        foreach ( var message in mailMessages )
	        {
		        message.From = new MailAddress( NameToEmailAddress( tenantName, inboxName ) );

		        string sequenceId = message.Headers[ EmailHelper.SoftwarePlatformSequenceIdHeader ];

		        if ( !string.IsNullOrEmpty( sequenceId ) )
		        {
			        message.Headers[ EmailHelper.MessageIdHeader ] = EmailHelper.GenerateMessageId( sequenceId, _provider.OaServer );
		        }
	        }

            string password = _provider.OaPasswordSecureId != null ? Factory.SecuredData.Read((Guid) _provider.OaPasswordSecureId) : string.Empty;

	        /////
	        // Send the messages.
	        /////
            EmailHelper.Send(mailMessages, _provider.OaServer, _provider.OaPort ?? EmailHelper.DefaultSmtpPort, _provider.OaUseSSL ?? true, _provider.OaPostInDirectory ?? true, _provider.OaAccount, password);
        }

        public string GetNoReplyAddress()
        {
            return string.Format("no-reply@{0}", _provider.OaDomainName);
        }
    }
}
