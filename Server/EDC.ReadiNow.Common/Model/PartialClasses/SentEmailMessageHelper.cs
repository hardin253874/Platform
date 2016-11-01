// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Net.Mail;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     SentEmailMessage class.
	/// </summary>
	public static class SentEmailMessageHelper
	{
		/// <summary>
		///     Converts the current SentEmailMessage to a MailMessage instance.
		/// </summary>
		/// <returns>
		///     A new MailMessage instance that corresponds to this SentEmailMessage instance.
		/// </returns>
		public static MailMessage ToMailMessage( this SentEmailMessage sentEmailMessage )
		{
            if ( sentEmailMessage == null )
                throw new ArgumentNullException( "sentEmailMessage" );

			MailMessage message = sentEmailMessage.As<EmailMessage>( ).ToMailMessage( );

			message.Headers[ EmailHelper.SoftwarePlatformSequenceIdHeader ] = sentEmailMessage.SemSequenceNumber;

			return message;
		}
	}
}