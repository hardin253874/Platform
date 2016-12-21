// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Email;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.EmailListener
{
	/// <summary>
	/// Matches the Received email to its send email.
	/// </summary>
    public class MatchSentToReceivedEmailsAction: IEmailAction
    {
		/// <summary>
		/// Executed before the message is saved
		/// </summary>
		/// <param name="message"></param>
		/// <param name="postSaveAction">if not null an action run after the save. This happens even if the save is cancelled.</param>
		/// <returns>
		/// True if the save is to be cancelled
		/// </returns>
        public bool BeforeSave(ReceivedEmailMessage message, out Action postSaveAction)
        {
            postSaveAction = null;

			/////
			// Get the references from the received email.
			/////
	        string referencesString = message.EmReferences;

			if ( ! string.IsNullOrEmpty( referencesString ) )
			{
				/////
				// Parse out each reference as there may be many.
				/////
				List<string> messageIds = EmailHelper.GetMessageIds( referencesString );

				foreach ( string messageId in messageIds )
				{
					/////
					// Get the local part of the message Id.
					/////
					string messageIdLocalPart = EmailHelper.GetMessageIdLocalPart( messageId );

					if ( !string.IsNullOrEmpty( messageIdLocalPart ) )
					{
						/////
						// Locate any Sent emails that have the same sequence number.
						/////
						var sentEmail = Entity.GetByField<SentEmailMessage>( messageIdLocalPart, true, SentEmailMessage.SemSequenceNumber_Field ).FirstOrDefault( );

						if ( sentEmail != null )
						{
							/////
							// Link the send email to this received message.
							/////
							message.OriginalMessage = sentEmail;

							break;
						}
					}
				}
			}

            return false;
        }
    }
}