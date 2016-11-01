// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     ReceivedEmailMessage class.
	/// </summary>
	public static class ReceivedEmailMessageHelper
	{
		/// <summary>
		///     Converts the messages.
		/// </summary>
		/// <param name="messages">The messages.</param>
		/// <param name="inbox">The inbox.</param>
		/// <returns></returns>
		public static List<ReceivedEmailMessage> ConvertMessages( IEnumerable<MailMessage> messages, Inbox inbox )
		{
			return messages.Select( message => new ReceivedEmailMessage
				{
					EmFrom = message.From.ToString( ),
					EmTo = message.To.ToString( ),
					EmSubject = message.Subject,
					EmCC = message.CC.ToString( ),
					EmBody = message.Body,
					EmIsHtml = message.IsBodyHtml,
					EmReceivedDate = DateTime.UtcNow,
					FromInbox = inbox,
				} ).ToList( );
		}
	}
}