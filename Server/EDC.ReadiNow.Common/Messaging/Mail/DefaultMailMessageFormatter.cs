// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Messaging.Mail;

namespace EDC.ReadiNow.Messaging.Mail
{
	/// <summary>
	///     Default MailMessage formatter.
	/// </summary>
	public class DefaultMailMessageFormatter : IMailMessageFormatter<ReceivedEmailMessage>
	{
		/// <summary>
		/// Formats the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="formattedMessage">The formatted message.</param>
		/// <param name="inbox">The inbox.</param>
		public MailMessageFormatterResult Format( MailMessage message, ReceivedEmailMessage formattedMessage, IEntity inbox )
		{
			if ( message == null || formattedMessage == null || inbox == null )
			{
				return MailMessageFormatterResult.Skip;
			}

			formattedMessage.EmFrom = message.From.ToString( );
			formattedMessage.EmTo = message.To.ToString( );
			formattedMessage.EmSubject = message.Subject;
			formattedMessage.EmCC = message.CC.ToString( );
			formattedMessage.EmBody = message.Body;
			formattedMessage.EmReferences = message.Headers[ EmailHelper.ReferencesHeader ];
			formattedMessage.EmIsHtml = message.IsBodyHtml;
			formattedMessage.EmReceivedDate = DateTime.UtcNow;
			formattedMessage.FromInbox = inbox.As<Inbox>( );

			return MailMessageFormatterResult.Ok;
		}
	}
}