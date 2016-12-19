// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Mail;
using S22.Imap;
using EDC.Diagnostics;

namespace EDC.ReadiNow.Email
{
    public interface IEmailReceiver
    {
        List<MailMessage> GetAllMessages(IReadOnlyCollection<string> toAddressFilters);
    }

    public class ImapEmailReceiver : IEmailReceiver
    {
        public static readonly long MaxEmailSize = 20000000;  // All emails greater than this size are ignored
        public static readonly int DefaultNonSslPort = 143;
        public static readonly int DefaultSslPort = 993;

        public string Hostname { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Folder { get; set; }
        public bool UseIntegratedAuthentication { get; set; }
        public IEventLog EventLog { get; set; }

        public ImapEmailReceiver()
        {
            UseSSL = true;
            Port = DefaultSslPort;
        }

        public ImapEmailReceiver(string hostname, int port, bool useSSL, string userId, string password, string folder,
            IEventLog eventLog)
        {
            Hostname = hostname;
            Port = port;
            UseSSL = useSSL;
            UserId = userId;
            Password = password;
            Folder = folder;
            EventLog = eventLog;
            if (Folder?.Trim().Length == 0)
                Folder = null;
        }

        public List<MailMessage> GetAllMessages(IReadOnlyCollection<string> toAddressFilters)
        {
            if (toAddressFilters == null)
                throw new ArgumentNullException(nameof(toAddressFilters));

            ImapClient imapClient = null;
            try
            {
                imapClient = new ImapClient(Hostname, Port, UseSSL);

                EventLog.WriteTrace($"ImapEmailReceiver : Fetching emails from IMAP server for User Id : {UserId}, Port : {Port}, Server : {Hostname}.");

                var authMethod = UseIntegratedAuthentication ? AuthMethod.Login : AuthMethod.Auto;
                try
                {
                    imapClient.Login(UserId, Password, authMethod);
                }
                catch (InvalidCredentialsException ex)
                {
                    throw new Exception($"ImapEmailReceiver: Failed to connect to the IMAP server rejected the supplied credentials. User Id : {UserId}, Server : {Hostname}, Message : {ex.ToString()}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"ImapEmailReceiver: An error occurred connecting to IMAP Server. User Id : {UserId}, Server : {Hostname}, Message : {ex.ToString()}");
                }

                /*
                 This implementation of the email fetching is far from ideal however this is the only implementation that works reliably since the 
                 SearchCondition.To() option does not appear to work reliably. Hence we need to fetch and examin each email indivdually to determine
                 if we are interested in it.
                */

                var yesterday = DateTime.Now.AddDays(-1);
                var uids = imapClient.Search(SearchCondition.SentSince(yesterday).And(SearchCondition.Smaller(MaxEmailSize)), Folder);   // SearchCondition.SentSince has resolution of days (not hours, minutes or seconds)

                var matchedUids = new List<uint>();
                foreach (var uid in uids.Distinct())
                {
                    // fetch just the headers and check the To address
                    var message = imapClient.GetMessage(uid, FetchOptions.HeadersOnly);
                    if (!ContainsAddress(message.To, toAddressFilters))
                        continue;

                    matchedUids.Add(uid);
                }

                if (matchedUids.Count == 0)
                    return new List<MailMessage>();
                
                // now fetch the full email for those that we have matched.
                var messages = imapClient.GetMessages(matchedUids, FetchOptions.Normal).ToList();
#if !DEBUG
                // delete these emails
                imapClient.DeleteMessages(matchedUids);
                imapClient.Expunge(Folder);
#endif
                EventLog.WriteTrace($"ImapEmailReceiver : Fetched {messages.Count} emails from IMAP server.");

                return messages;
                
            }
            finally
            {
                imapClient.Logout();
                imapClient.Dispose();
            }
        }

        bool ContainsAddress(MailAddressCollection mailAddresses, IReadOnlyCollection<string> filterStrings)
        {
            foreach (var mailAddress in mailAddresses.Select(x => x.Address.ToLower()))
            {
                if (filterStrings.Any(x => mailAddress.Contains(x.ToLower())))
                    return true;
            }
            return false;
        }
    }
}
