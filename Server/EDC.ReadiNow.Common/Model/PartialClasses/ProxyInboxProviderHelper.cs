// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// This provider acts as a proxy to the 
    /// </summary>
    public class ProxyInboxProviderHelper : IInboxProviderHelper
    {
        private ProxyInboxProvider _provider;

        internal ProxyInboxProviderHelper(ProxyInboxProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider"); 
            _provider = provider;
        }

        private static IInboxProviderHelper GetProvider(string alias)
        {
            InboxProvider provider = Entity.Get<InboxProvider>(alias);
            IInboxProviderHelper providerHelper = provider.GetHelper();
            return providerHelper;
        }

        public bool IsValidInboxName(string name)
        {
            bool result = false;

            RunAsAdmin((alias) =>
                {
                    result = GetProvider(alias).IsValidInboxName(name);
                });

            return result;

        }

        public string NameToEmailAddress(string tenantName, string name)
        {
            string result = string.Empty;

            RunAsAdmin((alias) =>
            {
                result = GetProvider(alias).NameToEmailAddress(tenantName, name);
            });

            return result;
        }

        public void CreateInbox(string tenantName, Inbox inbox)
        {
            RunAsAdmin((alias) =>
            {
                GetProvider(alias).CreateInbox(tenantName, inbox);
            });
        }

        public void DeleteInbox(string tenantName, Inbox inbox)
        {
            RunAsAdmin((alias) =>
            {
                GetProvider(alias).DeleteInbox(tenantName, inbox);
            });
        }

        public IEnumerable<MailMessage> GetMessages(string tenantName, string inboxName)
        {
            IEnumerable<MailMessage> result = null;
            RunAsAdmin((alias) =>
            {
                result = GetProvider(alias).GetMessages(tenantName, inboxName);
            });

            return result;
        }

        public void SendMessages(IEnumerable<MailMessage> messages, string tenantName, string inboxName)
        {

            RunAsAdmin((alias) =>
            {
                GetProvider(alias).SendMessages(messages, tenantName, inboxName);
            });

        }


        public string GetNoReplyAddress()
        {
            string result = null;
            RunAsAdmin((alias) =>
            {
                result =  GetProvider(alias).GetNoReplyAddress();
            });

            return result;
        }

        void RunAsAdmin(Action<string> action)
        {
            string alias = _provider.ProviderAlias;

            using (new AdministratorContext())
            {
                action(alias);
            }
        }


    }
}