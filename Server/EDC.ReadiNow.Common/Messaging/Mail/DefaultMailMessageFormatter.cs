// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Messaging.Mail;
using EDC.ReadiNow.Email;
using EDC.ReadiNow.IO;
using System.Text.RegularExpressions;
using System.IO;

namespace EDC.ReadiNow.Messaging.Mail
{
	/// <summary>
	///     Default MailMessage formatter.
	/// </summary>
	public class DefaultMailMessageFormatter : IMailMessageFormatter<ReceivedEmailMessage>
	{
        long MaxAttachmentSize = 20000000;  // All attachments greater than this size are ignored

        static string EnsureValidFileName(string fileName)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(fileName, invalidRegStr, "_");
        }

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

			formattedMessage.EmFrom = message.From?.ToString( );
			formattedMessage.EmTo = message.To.ToString( );
			formattedMessage.EmSubject = message.Subject;
			formattedMessage.EmCC = message.CC.ToString( );
		    formattedMessage.EmBody = message.Body;
            var parser = new EmailBodyParser(message.Body);
            formattedMessage.EmReplyFragment = parser.GetReplySectionFromEmailBodyAsText();
			formattedMessage.EmReferences = message.Headers[ EmailHelper.ReferencesHeader ];
			formattedMessage.EmIsHtml = message.IsBodyHtml;
			formattedMessage.EmReceivedDate = DateTime.UtcNow;
			formattedMessage.FromInbox = inbox.As<Inbox>( );

		    foreach (var emailAttachment in message.Attachments)
		    {
                if (emailAttachment.ContentStream.Length > MaxAttachmentSize)
                {
                    EDC.ReadiNow.Diagnostics.EventLog.Application.WriteWarning("Skipping attachment in email from '{0}' to '{1}'. Attachment is greater than maximum allowed size", message.From, message.To); 
                    continue;
                }

                var tempHash = FileRepositoryHelper.AddTemporaryFile(emailAttachment.ContentStream);

                var file = Entity.Create<FileType>();
                file.Name = EnsureValidFileName(emailAttachment.Name);
                file.Description = "Email Attachment";
                file.FileDataHash = tempHash;
                file.Size = (int)emailAttachment.ContentStream.Length;
                file.FileExtension = Path.GetExtension(emailAttachment.Name);
                file.Save();
		        formattedMessage.EmAttachments.Add(file);

                EDC.ReadiNow.Diagnostics.EventLog.Application.WriteTrace("Created file '{0}' for attachment in email from '{1}' to {2}.", file.Name, message.From, message.To);
            }

		    return MailMessageFormatterResult.Ok;
		}
	}
}