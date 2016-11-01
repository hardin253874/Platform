// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Diagnostics;
using EDC.Text;
using EDC.ReadiNow.IO;
using System.IO;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Helper class for sending mail using the email settings defined for the tenant.
	/// </summary>
	public static class EmailHelper
	{
        public static readonly string MailDropDirectory = SpecialFolder.GetSpecialFolderPath(SpecialMachineFolders.MailDrop);
            

		/// <summary>
		///     Default SMTP port.
		/// </summary>
		public const int DefaultSmtpPort = 25;

		/// <summary>
		///     RFC compliant Message Id header.
		/// </summary>
		public const string MessageIdHeader = "Message-ID";

		/// <summary>
		///     References header.
		/// </summary>
		public const string ReferencesHeader = "References";

		/// <summary>
		///     Software Platform sequence Id header.
		/// </summary>
		public const string SoftwarePlatformSequenceIdHeader = "X-EDC-SoftwarePlatform-SequenceId";

		/// <summary>
		///     Resolves partial host names to FQDN.
		/// </summary>
		private static readonly ConcurrentDictionary<string, string> HostNameResolver = new ConcurrentDictionary<string, string>( );

		/// <summary>
		///     Sha1 hash algorithm.
		/// </summary>
		private static readonly SHA1Managed Sha1 = new SHA1Managed( );

		/// <summary>
		///     Message Id parser.
		/// </summary>
		public static Regex MessageIdParser = new Regex( "<(?<local>.*?)@(?<host>.*?)>", RegexOptions.IgnoreCase | RegexOptions.Compiled );

		/// <summary>
		///     Creates a new mail message.
		/// </summary>
		/// <param name="fromAddress">The 'From' address.</param>
		/// <param name="toAddresses">The 'To' addresses.</param>
		/// <param name="subject">The subject.</param>
		/// <param name="body">The body.</param>
		/// <param name="isBodyHtml">
		///     Set to <c>true</c> if the body of the email contains HTML.
		/// </param>
		/// <param name="headers">Any additional headers.</param>
		/// <returns>
		///     A new MailMessage instance.
		/// </returns>
		public static MailMessage CreateMailMessage( 
            string fromAddress, IEnumerable<string> toAddresses, string subject, 
            string body, Dictionary<string, Stream> attachments, 
            bool isBodyHtml = false, NameValueCollection headers = null)
		{
			/////
			// MailAddress constructor handles null/empty values.
			/////
			var mailMessage = new MailMessage
				{
					From = new MailAddress( fromAddress ),
					Subject = subject,
					Body = body,
					IsBodyHtml = isBodyHtml
				};

			/////
			// Add any 'to' recipients.
			/////
			if ( toAddresses != null )
			{
				foreach ( string toAddress in toAddresses )
				{
                    if (!string.IsNullOrEmpty(toAddress))                    
                        mailMessage.To.Add(toAddress);                    
				}
			}

			/////
			// Add any additional message headers.
			/////
			if ( headers != null )
			{
				mailMessage.Headers.Add( headers );
			}

            /////
            // Add attchaments
            if (attachments != null && attachments.Any())
            {
                foreach (var de in attachments)
                {
                    var attachment = new Attachment(de.Value, de.Key);
                    mailMessage.Attachments.Add(attachment);
                }
            }

			return mailMessage;
		}

		/// <summary>
		///     Creates a new mail message.
		/// </summary>
		/// <param name="fromAddress">The 'From' address.</param>
		/// <param name="toAddresses">The 'To' addresses.</param>
		/// <param name="ccAddresses">The 'CC' addresses.</param>
		/// <param name="subject">The subject.</param>
		/// <param name="body">The body.</param>
		/// <param name="isBodyHtml">
		///     Set to <c>true</c> if the body of the email contains HTML.
		/// </param>
		/// <param name="headers">Any additional headers.</param>
		/// <returns>
		///     A new MailMessage instance.
		/// </returns>
		public static MailMessage CreateMailMessage( string fromAddress, IEnumerable<string> toAddresses, IEnumerable<string> ccAddresses, string subject, string body, Dictionary<string, Stream> attachments = null, bool isBodyHtml = false, NameValueCollection headers = null )
		{
			MailMessage mailMessage = CreateMailMessage( fromAddress, toAddresses, subject, body, attachments, isBodyHtml, headers );

			if ( ccAddresses != null )
			{
				foreach ( string ccAddress in ccAddresses )
				{
					mailMessage.CC.Add( ccAddress );
				}
			}

			return mailMessage;
		}

		/// <summary>
		///     Creates a new mail message.
		/// </summary>
		/// <param name="fromAddress">The 'From' address.</param>
		/// <param name="toAddresses">The 'To' addresses.</param>
		/// <param name="ccAddresses">The 'CC' addresses.</param>
		/// <param name="bccAddresses">The 'BCC' addresses.</param>
		/// <param name="subject">The subject.</param>
		/// <param name="body">The body.</param>
		/// <param name="isBodyHtml">
		///     Set to <c>true</c> if the body of the email contains HTML.
		/// </param>
		/// <param name="headers">Any additional headers.</param>
		/// <returns>
		///     A new MailMessage instance.
		/// </returns>
		public static MailMessage CreateMailMessage( string fromAddress, IEnumerable<string> toAddresses, IEnumerable<string> ccAddresses, IEnumerable<string> bccAddresses, string subject, string body, Dictionary<string, Stream> attachments = null, bool isBodyHtml = false, NameValueCollection headers = null )
		{
			MailMessage mailMessage = CreateMailMessage( fromAddress, toAddresses, ccAddresses, subject, body, attachments, isBodyHtml, headers );

			if ( bccAddresses != null )
			{
				foreach ( string bccAddress in bccAddresses )
				{
					mailMessage.Bcc.Add( bccAddress );
				}
			}

			return mailMessage;
		}

		/// <summary>
		///     Creates a new mail message.
		/// </summary>
		/// <param name="fromAddress">The 'From' address.</param>
		/// <param name="toAddresses">The 'To' addresses.</param>
		/// <param name="ccAddresses">The 'CC' addresses.</param>
		/// <param name="bccAddresses">The 'BCC' addresses.</param>
		/// <param name="replyToAddresses">The 'Reply To' addresses.</param>
		/// <param name="subject">The subject.</param>
		/// <param name="body">The body.</param>
		/// <param name="isBodyHtml">
		///     Set to <c>true</c> if the body of the email contains HTML.
		/// </param>
		/// <param name="headers">Any additional headers.</param>
		/// <returns>
		///     A new MailMessage instance.
		/// </returns>
		public static MailMessage CreateMailMessage( string fromAddress, IEnumerable<string> toAddresses, IEnumerable<string> ccAddresses, IEnumerable<string> bccAddresses, IEnumerable<string> replyToAddresses, string subject, string body, Dictionary<string, Stream> attachments = null, bool isBodyHtml = false, NameValueCollection headers = null )
		{
			MailMessage mailMessage = CreateMailMessage( fromAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, isBodyHtml, headers );

			if ( replyToAddresses != null )
			{
				foreach ( string replyToAddress in replyToAddresses )
				{
					mailMessage.ReplyToList.Add( replyToAddress );
				}
			}

			return mailMessage;
		}

		/// <summary>
		///     Generates the message id as per RFC 2822.
		/// </summary>
		/// <param name="host">The mail server host name.</param>
		/// <returns>
		///     A message id that conforms to RFC 2822.
		/// </returns>
		/// <remarks>
		///     The format is:
		///     &lt;hash@fqdnHost&gt;
		///     where the hash is a base 36 encoded version of the sha1 hashed local part.
		/// </remarks>
		public static string GenerateMessageId( string host )
		{
			return GenerateMessageId( GenerateMessageIdLocalPart( ), GenerateMessageIdHostPart( host ) );
		}

		/// <summary>
		///     Generates the message id as per RFC 2822.
		/// </summary>
		/// <param name="localPart">The local part.</param>
		/// <param name="host">The host.</param>
		/// <returns>
		///     A message id that conforms to RFC 2822.
		/// </returns>
		/// <remarks>
		///     The format is:
		///     &lt;hash@fqdnHost&gt;
		///     where the hash is a base 36 encoded version of the sha1 hashed local part.
		/// </remarks>
		public static string GenerateMessageId( string localPart, string host )
		{
			/////
			// Format is: <hash@fqdnHost>
			/////
			return string.Format( "<{0}@{1}>", localPart, GenerateMessageIdHostPart( host ) );
		}

		/// <summary>
		///     Generates the message id host part as per RFC 2822.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns>
		///     The host part of the message id.
		/// </returns>
		public static string GenerateMessageIdHostPart( string host )
		{
			if ( string.IsNullOrEmpty( host ) )
			{
				return host;
			}

			string fqdn;

			if ( !HostNameResolver.TryGetValue( host, out fqdn ) )
			{
				try
				{
					IPHostEntry ipHostEntry = Dns.GetHostEntry( host );

					fqdn = ipHostEntry.HostName;

					/////
					// Store both the short -> long host name as well as long -> long host name.
					/////
					HostNameResolver[ host ] = fqdn;
					HostNameResolver[ fqdn ] = fqdn;
				}
				catch
				{
					/////
					// If unable to resolve the host name, use the supplied value.
					/////

					return host;
				}
			}

			return fqdn;
		}

		/// <summary>
		///     Generates the message id local part as per RFC 2822.
		/// </summary>
		/// <returns>
		///     The local part of the message id.
		/// </returns>
		public static string GenerateMessageIdLocalPart( )
		{
			/////
			// The hash is comprised of the current time along with some unique value.
			/////
			byte[] hash = Sha1.ComputeHash( Encoding.UTF8.GetBytes( string.Format( "{0}.{1}", DateTime.Now.ToString( "yyyyMMddHHmmss.ff" ), Guid.NewGuid( ).ToString( ) ) ) );

			/////
			// RFC indicates the local part is to be Base36 encoded.
			/////
			return RadixEncoding.Base36.GetString( hash );
		}

		/// <summary>
		///     Gets the message id host part.
		/// </summary>
		/// <param name="messageId">The message id.</param>
		/// <returns>
		///     The host part of the MessageId if found; null otherwise.
		/// </returns>
		public static string GetMessageIdHostPart( string messageId )
		{
			if ( string.IsNullOrEmpty( messageId ) )
			{
				return null;
			}

			Match match = MessageIdParser.Match( messageId );

			if ( !match.Success )
			{
				return null;
			}

			return match.Groups[ "host" ].Value;
		}

		/// <summary>
		///     Gets the message id local part.
		/// </summary>
		/// <param name="messageId">The message id.</param>
		/// <returns>
		///     The local part of the MessageId if found; null otherwise.
		/// </returns>
		public static string GetMessageIdLocalPart( string messageId )
		{
			if ( string.IsNullOrEmpty( messageId ) )
			{
				return null;
			}

			Match match = MessageIdParser.Match( messageId );

			if ( ! match.Success )
			{
				return null;
			}

			return match.Groups[ "local" ].Value;
		}

		/// <summary>
		///     Parses the message ids from the references string.
		/// </summary>
		/// <param name="references">The references.</param>
		/// <returns>
		///     A list of message ids.
		/// </returns>
		public static List<string> GetMessageIds( string references )
		{
			var messageIds = new List<string>( );

			if ( string.IsNullOrEmpty( references ) )
			{
				return messageIds;
			}

			MatchCollection matchCollection = MessageIdParser.Matches( references );

			messageIds.AddRange( from Match match in matchCollection
			                     select match.Value );

			return messageIds;
		}

		/// <summary>
		///     Sends the specified messages.
		/// </summary>
		/// <param name="messages">The messages.</param>
		/// <param name="smtpHost">The SMTP host.</param>
		/// <returns>
		///     True if all the messages were sent successfully.
		/// </returns>
		public static void Send( IEnumerable<MailMessage> messages, string smtpHost )
		{
			Send( messages, smtpHost, DefaultSmtpPort, false, false, null, null );
		}

		/// <summary>
		///     Sends the specified messages.
		/// </summary>
		/// <param name="messages">The messages.</param>
		/// <param name="smtpHost">The SMTP host.</param>
		/// <param name="smtpPort">The SMTP port.</param>
		/// <returns>
		///     True if all the messages were sent successfully.
		/// </returns>
		public static void Send( IEnumerable<MailMessage> messages, string smtpHost, int smtpPort )
		{
			Send( messages, smtpHost, smtpPort, false, false, null, null );
		}

		/// <summary>
		///     Sends the specified messages.
		/// </summary>
		/// <param name="messages">The messages.</param>
		/// <param name="smtpHost">The SMTP host.</param>
		/// <param name="smtpPort">The SMTP port.</param>
		/// <param name="enableSsl">Whether SSL is enabled on the specified port.</param>
		/// <returns>
		///     True if all the messages were sent successfully.
		/// </returns>
		public static void Send( IEnumerable<MailMessage> messages, string smtpHost, int smtpPort, bool enableSsl )
		{
			Send( messages, smtpHost, smtpPort, enableSsl, false, null, null );
		}

		/// <summary>
		///     Sends the specified messages.
		/// </summary>
		/// <param name="messages">The messages.</param>
		/// <param name="smtpHost">The SMTP host.</param>
		/// <param name="smtpPort">The SMTP port.</param>
		/// <param name="enableSsl">Whether SSL is enabled on the specified port.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns>
		///     True if all the messages were sent successfully.
		/// </returns>
		public static void Send( IEnumerable<MailMessage> messages, string smtpHost, int smtpPort, bool enableSsl, string username, string password )
		{
			Send( messages, smtpHost, smtpPort, enableSsl, false, username, password );
		}

		/// <summary>
		///     Sends the specified messages.
		/// </summary>
		/// <param name="messages">The messages.</param>
		/// <param name="smtpHost">The SMTP host.</param>
		/// <param name="smtpPort">The SMTP port.</param>
		/// <param name="enableSsl">Whether SSL is enabled on the specified port.</param>
		/// <param name="useDropDirectory">Whether a drop directory is used rather than the SMTP host.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns>
		///     True if all the messages were sent successfully.
		/// </returns>
		/// <exception cref="System.ArgumentException">Attempted to send email but the SMTP host has not been set for the tenant.</exception>
		public static void Send( IEnumerable<MailMessage> messages, string smtpHost, int smtpPort, bool enableSsl, bool useDropDirectory, string username, string password )
		{
			var smtpClient = new SmtpClient( );

			if ( username == null )
			{
				smtpClient.UseDefaultCredentials = true;
				smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
			}
			else
			{
				smtpClient.UseDefaultCredentials = false;
				smtpClient.Credentials = new NetworkCredential( username, password );
			}

			if ( useDropDirectory )
			{
				smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
				smtpClient.PickupDirectoryLocation = MailDropDirectory;
			}
			else
			{
				if ( string.IsNullOrEmpty( smtpHost ) )
				{
                    throw new MissingSmtpHostException();
				}

				smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
				smtpClient.Host = smtpHost;
				smtpClient.Port = smtpPort;
				smtpClient.EnableSsl = enableSsl;
			}

			int successCount = 0;
			int totalCount = 0;

			foreach ( MailMessage message in messages )
			{
				totalCount++;

				try
				{
					smtpClient.Send( message );

					successCount++;
				}
				catch ( SmtpFailedRecipientException ex )
				{
					EventLog.Application.WriteInformation( "Email send failed with message: {0}", ex.Message );
					throw;
				}
				catch ( SmtpException ex )
				{
					EventLog.Application.WriteError( "Failed to connect to email server, Exception:\n{0}", ex.ToString( ) );
					throw; // internal error, can not communicate with the server
				}
			}

			EventLog.Application.WriteTrace( "Successfully sent {0} email messages of {1} to {2}. Use drop directory = {3}.", successCount, totalCount, smtpHost, useDropDirectory );
		}

		/// <summary>
		///     Sends the specified messages to the specified drop directory.
		/// </summary>
		/// <param name="messages">The messages.</param>
		/// <returns>
		///     True if all messages were send successfully.
		/// </returns>
		public static void SendToDropDirectory( IEnumerable<MailMessage> messages )
		{
			Send( messages, null, DefaultSmtpPort, false, true, null, null );
		}

        /// <summary>
        /// The SMTP host has not been configured for the tenant
        /// </summary>
        public class MissingSmtpHostException : ArgumentException
        {
            public MissingSmtpHostException()
                : base("Attempted to send email but the SMTP host has not been set for the tenant.")
            { }
        }

	}
}