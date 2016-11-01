// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Net.Mail;

namespace EDC.ReadiNow.Model.Interfaces
{
    public interface IInboxProviderHelper
    {
        /// <summary>
        ///     Is the name of the mailbox valid?
        /// </summary>
        bool IsValidInboxName(string name);

        /// <summary>
        ///     the email address for the inbox
        /// </summary>
        /// <returns></returns>
        string NameToEmailAddress(string tenantName, string name);

        /// <summary>
        ///     Create the inbox on the mail server
        /// </summary>
        /// <param name="inbox"></param>
        /// <param name="tenantName"></param>
        void CreateInbox(string tenantName, Inbox inbox);

        /// <summary>
        ///     Delete the inbox on the mail server
        /// </summary>
        /// <param name="inbox"></param>
        /// <param name="tenantName"></param>
        void DeleteInbox(string tenantName, Inbox inbox);

        /// <summary>
        ///     Get the messages from the mailbox
        /// </summary>
        /// <returns></returns>
        IEnumerable<MailMessage> GetMessages(string tenantName, string inboxName);


        /// <summary>
        ///     Send a message on behalf of the inbox
        /// </summary>
        /// <param name="inboxName"></param>
        /// <param name="messages"></param>
        /// <param name="tenantName"></param>
        /// <returns>true on success</returns>
        void SendMessages(IEnumerable<MailMessage> messages, string tenantName, string inboxName);

        /// <summary>
        /// Get the no reply address
        /// </summary>
        /// <returns></returns>
        string GetNoReplyAddress();
    }
}
