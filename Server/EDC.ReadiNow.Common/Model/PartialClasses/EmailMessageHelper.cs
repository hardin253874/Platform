// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.IO;
using System;
using System.Net.Mail;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using EDC.ReadiNow.Email;

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

            if (string.IsNullOrEmpty(emailMessage.EmTo) && 
                string.IsNullOrEmpty(emailMessage.EmCC) && 
                string.IsNullOrEmpty(emailMessage.EmBCC))
                throw new InvalidOperationException( "This email message has no recipient." );


            return EmailHelper.CreateMailMessage(
                emailMessage.EmFrom,
                emailMessage.EmFromName,
                emailMessage.EmTo?.Split(';'),
                emailMessage.EmCC?.Split(';'),
                emailMessage.EmBCC?.Split(';'),
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
            var attachments = documents.ToDictionary(file => GetDocFileName(file), file => GetDocStream(file));
            return attachments;
        }

        static string GetDocFileName(FileType file)
        {
            return $"{file.Name ?? UnnamedFileName}.{file.FileExtension ?? string.Empty}";
        }

        static Stream GetDocStream(FileType file)
        {
            return FileRepositoryHelper.GetFileDataStreamForEntity(file);
        }
	}
}