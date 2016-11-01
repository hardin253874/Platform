// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;
using EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Messaging.Mail;

namespace EDC.ReadiNow.Scheduling.iCalendar.Formatters
{
	/// <summary>
	///     iCal MailMesssage formatter class.
	/// </summary>
	public class iCalMailMesssageFormatter : IMailMessageFormatter<ReceivedEmailMessage>
	{
		/// <summary>
		///     Formats the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="formattedMessage">The formatted message.</param>
		/// <param name="inbox">The inbox.</param>
		/// <returns>Enumeration value indicating the result of the format.</returns>
		public MailMessageFormatterResult Format( MailMessage message, ReceivedEmailMessage formattedMessage, IEntity inbox )
		{
			if ( message == null || formattedMessage == null || inbox == null )
			{
				return MailMessageFormatterResult.Skip;
			}

			var iCalMessage = formattedMessage.As<ReceivedICalEmailMessage>( );

			if ( iCalMessage == null )
			{
				EventLog.Application.WriteError( "The ReceivedEmailMessage was not of type 'ReceivedICalEmailMessage'" );

				return MailMessageFormatterResult.Error;
			}

			string iCalString;

			MailMessageFormatterResult result = GetICalUpdate( message, iCalMessage, out iCalString );

			if ( result == MailMessageFormatterResult.Ok )
			{
				iCalMessage.ICalUpdate = iCalString;
			}

			return result;
		}

		/// <summary>
		///     Finds the I cal attendee.
		/// </summary>
		/// <param name="iCalEvent">The i cal event.</param>
		/// <param name="fromAddress">From address.</param>
		/// <returns></returns>
		private static Attendee FindICalAttendee( IEvent iCalEvent, string fromAddress )
		{
			return iCalEvent.Attendees.Cast<Attendee>( ).FirstOrDefault( attendee => attendee.Value.ToString( ).ToLowerInvariant( ).Contains( fromAddress.ToLowerInvariant( ) ) );
		}

		/// <summary>
		///     Gets the Ical update.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="receivedICalEmailMessage">The received I cal email message.</param>
		/// <param name="iCal">The i cal.</param>
		/// <returns></returns>
		private static MailMessageFormatterResult GetICalUpdate( MailMessage message, ReceivedICalEmailMessage receivedICalEmailMessage, out string iCal )
		{
			iCal = null;

			if ( message == null )
			{
				return MailMessageFormatterResult.Skip;
			}

			try
			{
				var attachableItems = new List<AttachmentBase>( );

				attachableItems.AddRange( message.AlternateViews );
				attachableItems.AddRange( message.Attachments );

				/////
				// Locate a view that contains a calendar attachment.
				/////
				foreach ( AttachmentBase attachment in attachableItems )
				{
					if ( attachment.ContentType.MediaType == "text/calendar" )
					{
						/////
						// Deserialize the iCalendar attachment.
						/////
						IICalendarCollection iCalendarCollection = iCalendar.LoadFromStream( attachment.ContentStream );

						if ( iCalendarCollection != null )
						{
							IICalendar calendar = iCalendarCollection.FirstOrDefault( );

							if ( calendar != null )
							{
								if ( calendar.Method == Methods.Reply )
								{
									IEvent calendarEvent = calendar.Events.FirstOrDefault( );

									if ( calendarEvent != null && ! string.IsNullOrEmpty( calendarEvent.Uid ) )
									{
										/////
										// Find all iCal UID containers that correlate to the received calendar events UID.
										/////
										IEnumerable<ICalUidContainer> iCalUidContainers = Entity.GetByField<ICalUidContainer>( calendarEvent.Uid, ICalUidContainer.ICalUid_Field );

										if ( iCalUidContainers != null )
										{
											/////
											// Get the first iCal UID container.
											/////
											ICalUidContainer iCalUidContainer = iCalUidContainers.FirstOrDefault( );

											if ( iCalUidContainer != null && iCalUidContainer.CalendarEventEmail != null )
											{
												/////
												// Get the original event email object that was used to calendar UID container.
												/////
												EventEmail eventEntity = iCalUidContainer.CalendarEventEmail;

												if ( eventEntity != null )
												{

                                                    // ***************
                                                    // This needs to be fixed as part of dealing with replies from ical requests. The rel on a rel needs to be turned into something else.
                                                    // ***************
                                                    //IEntityRelationship<EventEmailAttendees, EmailContact> attendeeRelationshipInstance = FindAttendeeRelationshipInstance( eventEntity, message.From.Address.ToLowerInvariant( ) );

                                                    //if ( attendeeRelationshipInstance != null )
                                                    //{
                                                    //    Attendee attendee = FindICalAttendee( calendarEvent, message.From.Address.ToLowerInvariant( ) );

                                                    //    if ( attendee != null )
                                                    //    {
                                                    //        var eventEmailAttendees = attendeeRelationshipInstance.Instance.AsWritable<EventEmailAttendees>( );

                                                    //        switch ( attendee.ParticipationStatus )
                                                    //        {
                                                    //            case ParticipationStatus.Accepted:
                                                    //                eventEmailAttendees.AttendeeStatus_Enum = AttendeeStatusEnum_Enumeration.AttendeeStatusAccepted;
                                                    //                break;
                                                    //            case ParticipationStatus.Declined:
                                                    //                eventEmailAttendees.AttendeeStatus_Enum = AttendeeStatusEnum_Enumeration.AttendeeStatusDeclined;
                                                    //                break;
                                                    //            case ParticipationStatus.Delegated:
                                                    //                eventEmailAttendees.AttendeeStatus_Enum = AttendeeStatusEnum_Enumeration.AttendeeStatusDelegated;
                                                    //                break;
                                                    //            case ParticipationStatus.NeedsAction:
                                                    //                eventEmailAttendees.AttendeeStatus_Enum = AttendeeStatusEnum_Enumeration.AttendeeStatusNeedsAction;
                                                    //                break;
                                                    //            case ParticipationStatus.Tentative:
                                                    //                eventEmailAttendees.AttendeeStatus_Enum = AttendeeStatusEnum_Enumeration.AttendeeStatusTentative;
                                                    //                break;
                                                    //        }

                                                    //        eventEmailAttendees.Save( );
                                                    //    }
                                                    //}

                                                    //*************

													receivedICalEmailMessage.CreatedEventEmail = eventEntity;

													/////
													// Save the message since the process inbox action will discard it.
													/////
													receivedICalEmailMessage.Save( );
												}
											}
										}
									}

									return MailMessageFormatterResult.Reject;
								}

								/////
								// Serialize the iCalendar to a string for storage.
								/////
								var serializer = new iCalendarSerializer( );

								iCal = serializer.SerializeToString( calendar );

								return MailMessageFormatterResult.Ok;
							}
						}

						break;
					}
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( "Failed to process email iCal attachment. " + exc );

				return MailMessageFormatterResult.Error;
			}

			return MailMessageFormatterResult.Skip;
		}
	}
}