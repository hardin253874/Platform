// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Net.Mail;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Messaging.Mail
{
	/// <summary>
	///     MailMessage formatter interface.
	/// </summary>
	/// <typeparam name="T">Type of entity to format the mail message(s) to.</typeparam>
	public interface IMailMessageFormatter<T>
		where T : IEntity
	{
		/// <summary>
		///     Formats the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="formattedMessage">The formatted message.</param>
		/// <param name="inbox">The inbox.</param>
		/// <returns>A MailMessageFormatterResult value.</returns>
		MailMessageFormatterResult Format( MailMessage message, T formattedMessage, IEntity inbox );
	}
}