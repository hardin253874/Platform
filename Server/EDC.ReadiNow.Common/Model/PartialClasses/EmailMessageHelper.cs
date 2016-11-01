// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.IO;
using System;
using System.Net.Mail;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     EmailMessage class.
	/// </summary>
	public static class EmailMessageHelper
	{
        const string UnnamedFileName = "Unanmed";

		/// <summary>
		///     Converts the current EmailMessage to a MailMessage instance.
		/// </summary>
		/// <returns></returns>
		public static MailMessage ToMailMessage( this EmailMessage emailMessage )
		{
            if (emailMessage == null)
                throw new ArgumentNullException("emailMessage");

            if (emailMessage.EmTo == null)
                throw new InvalidOperationException( "This email message has no recipient." );

            return EmailHelper.CreateMailMessage(
                emailMessage.EmFrom,
                emailMessage.EmTo.Split(';'),
                emailMessage.EmSubject,
                emailMessage.EmBody,
                GetAttachments(emailMessage),
                emailMessage.EmIsHtml ?? false
                );
		}

        /// <summary>
        /// Get any attachments
        /// </summary>
        static Dictionary<string, Stream> GetAttachments(EmailMessage emailMessage)
        {
            var documents = emailMessage.EmAttachments.Where(a => a != null);
            var attachments = documents.ToDictionary(doc => GetDocFileName(doc), doc => GetDocStream(doc));
            return attachments;
        }

        static string GetDocFileName(Document doc)
        {
            return $"{doc.Name ?? UnnamedFileName}.{doc.FileExtension ?? string.Empty}";
        }

        static Stream GetDocStream(Document doc)
        {
            return FileRepositoryHelper.GetFileDataStreamForEntity(doc);
        }
	}
}