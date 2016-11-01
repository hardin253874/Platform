// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.ReadiNow.Model
{
    public static class InboxProviderHelper
    {
        /// <summary>
        /// Get an appropriate InboxProviderHelper for the provided resource.
        /// </summary>
        /// <param name="inboxProvider">The inbox provider resource.</param>
        /// <returns>A suitable helper class.</returns>
        public static IInboxProviderHelper GetHelper(this InboxProvider inboxProvider)
        {
            var t = inboxProvider.As<ProxyInboxProvider>();
            if (t != null) return new ProxyInboxProviderHelper(t);

            var t2 = inboxProvider.As<ImapEmailProvider>();
            if (t2 != null) return new ImapEmailProviderHelper(t2);

            throw new Exception("Unrecognised Provider Type");
        }
    }    
}