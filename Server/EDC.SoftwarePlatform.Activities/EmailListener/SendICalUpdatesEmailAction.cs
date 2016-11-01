// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Resources;
using EDC.ReadiNow.Scheduling.iCalendar;
using EDC.ReadiNow.Scheduling.iCalendar.Email;
using Event = EDC.ReadiNow.Scheduling.iCalendar.Event;
using EventStatus = EDC.ReadiNow.Scheduling.iCalendar.EventStatus;

namespace EDC.SoftwarePlatform.Activities.EmailListener
{
	/// <summary>
	///     Action that sends iCal updates to attendees when a proposed new time is made.
	/// </summary>
	public class SendICalUpdatesEmailAction : IEmailAction
	{
		/// <summary>
		///     Executed before the message is saved
		/// </summary>
		/// <param name="message"></param>
		/// <param name="postSaveAction">if not null an action run after the save. This happens even if the save is cancelled.</param>
		/// <returns>
		///     True if the save is to be cancelled
		/// </returns>
		public bool BeforeSave( ReceivedEmailMessage message, out Action postSaveAction )
		{
			postSaveAction = null;

			/////
			// Check the message.
			/////
			if ( message == null )
			{
				return false;
			}

			var iCalMessage = message.As<ReceivedICalEmailMessage>( );

			/////
			// Ensure the message is a received iCal email message.
			/////
			if ( iCalMessage == null )
			{
				return false;
			}

			/////
			// The iCalUpdate field was set by the iCalMailMesssageFormatter that was called as part
			// of the ProcessInboxes action.
			/////
			if ( string.IsNullOrEmpty( iCalMessage.ICalUpdate ) )
			{
				return false;
			}

			/////
			// Read the iCal update.
			/////
			using ( var sr = new StringReader( iCalMessage.ICalUpdate ) )
			{
				/////
				// Deserialize the string into the iCal object model.
				/////
				IICalendarCollection iCalendarCollection = iCalendar.LoadFromStream( sr );

				if ( iCalendarCollection == null )
				{
					return false;
				}

				/////
				// Get the first calendar.
				/////
				IICalendar calendar = iCalendarCollection.FirstOrDefault( );

				if ( calendar == null || calendar.Events == null )
				{
					return false;
				}

				/////
				// Get the first calendar event.
				/////
				IEvent calendarEvent = calendar.Events.FirstOrDefault( );

				if ( calendarEvent == null )
				{
					return false;
				}

				/////
				// Make sure the calendar events UID is set.
				/////
				if ( string.IsNullOrEmpty( calendarEvent.Uid ) )
				{
					return false;
				}

				EventEmail eventEntity = null;
                Appointment appointment = null;

				/////
				// Find all sent iCal UID containers that correlate to the received calendar events UID.
				/////
				IEnumerable<ICalUidContainer> iCalUidContainers = Entity.GetByField<ICalUidContainer>( calendarEvent.Uid, ICalUidContainer.ICalUid_Field );

				if ( iCalUidContainers != null )
				{
					/////
					// Get the first sent message.
					/////
					ICalUidContainer iCalUidContainer = iCalUidContainers.FirstOrDefault( );

					if ( iCalUidContainer != null && iCalUidContainer.CalendarEventEmail != null )
					{
						/////
						// Get the original event email object that was used to create the sent iCal Email Message.
						/////
						eventEntity = iCalUidContainer.CalendarEventEmail.AsWritable<EventEmail>( );
					}
				}

				bool modificationsMade = false;

                if (eventEntity == null)
				{
					/////
					// No existing event email so this is a new request.
					/////
					EntityRef type = GetEventCreationType( message );

					eventEntity = type != null ? Entity.Create( type ).As<EventEmail>( ) : new EventEmail( );

                    appointment = Entity.Create<Appointment>();
                    eventEntity.EventEmailAppt = appointment;                       

					modificationsMade = true;

					eventEntity.Name = calendarEvent.Summary;

					var calUidContainer = new ICalUidContainer
						{
							ICalUid = calendarEvent.Uid
						};

					eventEntity.CalendarId = calUidContainer;

					string creatorEmailAddress = GetEmailAddress( message.EmFrom );

					if ( creatorEmailAddress != null )
					{
						EmailContact creatorEmailContact = FindEmailContact( creatorEmailAddress );

						if ( creatorEmailContact == null )
						{
							var mailAddress = new MailAddress( message.EmFrom );

							creatorEmailContact = CreateEmailContact( creatorEmailAddress, mailAddress.DisplayName ?? creatorEmailAddress );
						}

						eventEntity.EventEmailCreator = creatorEmailContact;
					}

					foreach ( IAttendee attendee in calendarEvent.Attendees )
					{
						string emailAddress = GetEmailAddress( attendee.Value.ToString( ) );

						if ( emailAddress != null )
						{
							EmailContact emailContact = FindEmailContact( emailAddress );

							if ( emailContact == null )
							{
								CreateEmailContact( emailAddress, attendee.CommonName );
							}

                            appointment.EventEmailAttendees.Add(emailContact.EmailContactOwner);
						}
					}

					CreateAndSendAcceptance( calendar, iCalMessage, eventEntity );
				}
				else
				{
                    appointment = eventEntity.EventEmailAppt;

					if ( calendar.Method == Methods.Publish || calendar.Method == Methods.Request )
					{
						/////
						// A REQUEST or PUBLISH means a new event arriving in the system.
						/////
						CreateAndSendAcceptance( calendar, iCalMessage, eventEntity );
					}
				}


				eventEntity.ReceivedEmailMessages.Add( iCalMessage );


				/////
				// Start time.
				/////
				if ( calendarEvent.Start != null )
				{
					DateTime utcTime = calendarEvent.Start.Utc;

                    if (!Equals(utcTime, appointment.EventStart))
					{
						appointment.EventStart = utcTime;
						modificationsMade = true;
					}
				}

				/////
				// End time.
				/////
				if ( calendarEvent.End != null )
				{
					DateTime utcTime = calendarEvent.End.Utc;

                    if (!Equals(utcTime, appointment.EventEnd))
					{
                        appointment.EventEnd = utcTime;
						modificationsMade = true;
					}
				}

				/////
				// All Day Event.
				/////
                if (appointment.EventIsAllDay == null || !Equals(calendarEvent.IsAllDay, appointment.EventIsAllDay.Value))
				{
                    appointment.EventIsAllDay = calendarEvent.IsAllDay;
					modificationsMade = true;
				}

				/////
				// Location.
				/////
				if ( calendarEvent.Location != null )
				{
                    if (!Equals(calendarEvent.Location, appointment.EventLocation))
					{
                        appointment.EventLocation = calendarEvent.Location;
						modificationsMade = true;
					}
				}

				/////
				// Location.
				/////
                if (eventEntity.EventEmailAppt.EventEmailPriority == null || !Equals(calendarEvent.Priority, eventEntity.EventEmailAppt.EventEmailPriority))
				{
					string priorityAlias;

					if ( calendarEvent.Priority <= 0 )
					{
						/////
						// Undefined.
						/////
						priorityAlias = null;
					}
					else if ( calendarEvent.Priority <= 4 )
					{
						/////
						// High priority.
						/////
						priorityAlias = "core:highPriority";
					}
					else if ( calendarEvent.Priority == 5 )
					{
						/////
						// Normal priority.
						/////
						priorityAlias = "core:normalPriority";
					}
					else if ( calendarEvent.Priority <= 9 )
					{
						/////
						// Low priority.
						/////
						priorityAlias = "core:lowPriority";
					}
					else
					{
						/////
						// Invalid priority.
						/////
						priorityAlias = null;
					}

					eventEntity.EventEmailAppt.EventEmailPriority = priorityAlias != null ? Entity.Get<EventEmailPriorityEnum>( priorityAlias ) : null;

					modificationsMade = true;
				}

				/////
				// Status.
				/////
				string statusAlias = null;

				switch ( calendarEvent.Status )
				{
					case EventStatus.Cancelled:
						statusAlias = "core:eventStatusCancelled";
						break;
					case EventStatus.Confirmed:
						statusAlias = "core:eventStatusConfirmed";
						break;
					case EventStatus.Tentative:
						statusAlias = "core:eventStatusTentative";
						break;
				}

				if ( !string.IsNullOrEmpty( statusAlias ) )
				{
                    if (appointment.EventStatus == null || appointment.EventStatus.Alias != statusAlias)
					{
                        appointment.EventStatus = Entity.Get<EventStatusEnum>(statusAlias);
						modificationsMade = true;
					}
				}

				if ( modificationsMade )
				{
					CustomContext cc = null;

					try
					{
						string timeZone = null;

						if ( eventEntity != null )
						{
							/////
							// Find all sent iCal Email Messages that correlate to the received calendar events UID.
							/////
							IEnumerable<SentICalEmailMessage> sentICalEmailMessages = eventEntity.SentEmailMessages;

							if ( sentICalEmailMessages != null )
							{
								SentICalEmailMessage sentICalEmailMessage = sentICalEmailMessages.FirstOrDefault( sent => sent.ICalTimeZone != null );

								if ( sentICalEmailMessage != null )
								{
									timeZone = sentICalEmailMessage.ICalTimeZone;
								}
							}
						}

						if ( string.IsNullOrEmpty( timeZone ) )
						{
							if ( calendar.TimeZones != null )
							{
								ITimeZone calendarTimeZone = calendar.TimeZones.FirstOrDefault( );

								if ( calendarTimeZone != null )
								{
									timeZone = calendarTimeZone.TzId;
								}
							}
						}

						if ( !string.IsNullOrEmpty( timeZone ) )
						{
							/////
							// Set up a custom context just for the duration of this call.
							/////
							RequestContext currentRequestContext = RequestContext.GetContext( );

							var data = new RequestContextData( currentRequestContext )
								{
									TimeZone = timeZone
								};

							cc = new CustomContext( data );
						}

						eventEntity.Save( );
					}
					finally
					{
						/////
						// Ensure the custom context is disposed.
						/////
						if ( cc != null )
						{
							cc.Dispose( );
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		///     Creates the acceptance.
		/// </summary>
		/// <param name="calendar">The calendar.</param>
		/// <param name="iCalMessage">The i cal message.</param>
		/// <returns></returns>
		private static IICalendar CreateAcceptance( IICalendar calendar, ReceivedICalEmailMessage iCalMessage )
		{
			/////
			// Sanity check on the calendar and message.
			/////
			if ( calendar == null || iCalMessage == null )
			{
				return null;
			}

			Inbox inbox = iCalMessage.FromInbox;

			/////
			// Ensure the inbox is set.
			/////
			if ( inbox == null )
			{
				return null;
			}

			/////
			// Create a new calendar.
			/////
			var reply = new iCalendar
				{
					Method = Methods.Reply
				};

			/////
			// Add the same time zones.
			/////
			foreach ( ITimeZone timeZone in calendar.TimeZones )
			{
				reply.TimeZones.Add( timeZone );
			}

			/////
			// Add each event.
			/////
			foreach ( IEvent existingCalendarEvent in calendar.Events )
			{
				var newCalendarEvent = new Event( );

				bool foundAttendee = false;

				/////
				// Search through the attendees looking for one that matches the inbox owner.
				/////
				foreach ( IAttendee existingAttendee in existingCalendarEvent.Attendees )
				{
					/////
					// Regex out the email address from the Uri.
					/////
					string existingEmailAddress = GetEmailAddress( existingAttendee.Value.ToString( ) );

					/////
					// If the address matches the inbox owner...
					/////
					if ( existingEmailAddress != null && existingEmailAddress.ToLowerInvariant( ) == inbox.InboxEmailAddress.ToLowerInvariant( ) )
					{
						/////
						// Create a new attendee and set the participation status to accepted.
						/////
						var newAttendee = new Attendee( existingAttendee.Value )
							{
								ParticipationStatus = ParticipationStatus.Accepted
							};

						newCalendarEvent.Attendees.Add( newAttendee );

						foundAttendee = true;
						break;
					}
				}

				if ( foundAttendee )
				{
					/////
					// Copy the other event properties over.
					/////
					newCalendarEvent.Class = existingCalendarEvent.Class;
					newCalendarEvent.Comments = existingCalendarEvent.Comments;
					newCalendarEvent.DtEnd = existingCalendarEvent.DtEnd;
					newCalendarEvent.DtStamp = existingCalendarEvent.DtStamp;
					newCalendarEvent.Start = existingCalendarEvent.Start;
					newCalendarEvent.Location = existingCalendarEvent.Location;
					newCalendarEvent.Organizer = existingCalendarEvent.Organizer;
					newCalendarEvent.Priority = existingCalendarEvent.Priority;
					newCalendarEvent.Sequence = existingCalendarEvent.Sequence;
					newCalendarEvent.Status = existingCalendarEvent.Status;
					newCalendarEvent.Summary = existingCalendarEvent.Summary;
					newCalendarEvent.Transparency = existingCalendarEvent.Transparency;
					newCalendarEvent.Uid = existingCalendarEvent.Uid;

					/////
					// if there was an attendee found, add the event.
					/////
					reply.Events.Add( newCalendarEvent );
				}
			}

			return reply;
		}

		/// <summary>
		///     Creates the and send acceptance.
		/// </summary>
		/// <param name="calendar">The calendar.</param>
		/// <param name="iCalMessage">The i cal message.</param>
		/// <param name="eventEmail">The event email.</param>
		private static void CreateAndSendAcceptance( IICalendar calendar, ReceivedICalEmailMessage iCalMessage, EventEmail eventEmail )
		{
			if ( calendar == null || iCalMessage == null )
			{
				return;
			}

			IICalendar reply = CreateAcceptance( calendar, iCalMessage );

			if ( reply != null )
			{
				SendAcceptance( reply, iCalMessage, eventEmail );
			}
		}

		/// <summary>
		///     Creates the email contact.
		/// </summary>
		/// <param name="emailAddress">The email address.</param>
		/// <param name="displayName">The display name.</param>
		/// <returns></returns>
		private static EmailContact CreateEmailContact( string emailAddress, string displayName )
		{
			if ( emailAddress == null )
			{
				return null;
			}

			/////
			// Create a new email contact.
			/////
			var emailContact = new EmailContact( );

			if ( displayName != null )
			{
				emailContact.Name = displayName;
				emailContact.EmailContactDisplayName = displayName;
			}
			else
			{
				emailContact.Name = emailAddress;
			}

			emailContact.Name = emailAddress;

			return emailContact;
		}

		/// <summary>
		///     Finds the email contact.
		/// </summary>
		/// <param name="emailAddress">The email address.</param>
		/// <returns></returns>
		private static EmailContact FindEmailContact( string emailAddress )
		{
			if ( string.IsNullOrEmpty( emailAddress ) )
			{
				return null;
			}

			return Entity.GetByField<EmailContact>( emailAddress, EmailContact.Name_Field ).FirstOrDefault( );
		}

		/// <summary>
		///     Gets the email address.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		private static string GetEmailAddress( string text )
		{
			if ( string.IsNullOrEmpty( text ) )
			{
				return null;
			}

			Match match = Regex.Match( text, @"(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds( 250 ) );

			if ( match != null )
			{
				return match.Value;
			}

			return null;
		}

		/// <summary>
		///     Gets the type of the event creation.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>
		///     An EntityRef describing the type to create.
		/// </returns>
		private static EntityRef GetEventCreationType( ReceivedEmailMessage message )
		{
			EntityRef type = null;

			if ( message != null && message.FromInbox != null )
			{
				Inbox inbox = message.FromInbox;

				if ( inbox.InboxCreatedEventType != null )
				{
					type = inbox.InboxCreatedEventType;
				}
			}

			return type ?? new EntityRef( "core:eventEmail" );
		}

		/// <summary>
		///     Sends the acceptance.
		/// </summary>
		/// <param name="calendar">The calendar.</param>
		/// <param name="iCalMessage">The i cal message.</param>
		/// <param name="eventEmail">The event email.</param>
		private static void SendAcceptance( IICalendar calendar, ReceivedICalEmailMessage iCalMessage, EventEmail eventEmail )
		{
			Inbox inbox = iCalMessage.FromInbox;

			string subject = string.Format( "{0}: {1}", GlobalStrings.Accepted, iCalMessage.EmSubject ?? GlobalStrings.MeetingInvitation );

			/////
			// Create the new mail message.
			/////
			var sentMessage = new SentICalEmailMessage
				{
					Name = subject,
					EmFrom = inbox.InboxEmailAddress,
					EmTo = iCalMessage.EmFrom,
					EmSubject = subject,
					EmBody = iCalMessage.EmBody ?? string.Empty,
					EmIsHtml = iCalMessage.EmIsHtml ?? true,
					OwnerEventEmail = eventEmail
				};

			sentMessage.Save( );

			/////
			// Send the message.
			/////
			iCalEmailHelper.SendICalEmail( calendar, sentMessage.As<SentEmailMessage>( ).ToMailMessage( ), inbox );
		}
	}
}