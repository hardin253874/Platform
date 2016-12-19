// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Resources;
using EDC.ReadiNow.Scheduling.iCalendar;
using EDC.ReadiNow.Scheduling.iCalendar.Email;

namespace EDC.ReadiNow.Model.EventClasses
{
	using iCalEvent = Scheduling.iCalendar.Event;
    using EDC.ReadiNow.Utc;


    public static class AppointmentHelper
    {

		/// <summary>
		///     Converts the specified date time from the source time zone to the destination time zone.
		/// </summary>
		private static DateTime ConvertDateTime( DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone )
		{
			if ( sourceTimeZone == null || destinationTimeZone == null )
			{
				return dateTime;
			}

			/////
			// Converts the specified date time from the source time zone to the destination time zone.
			/////
			return TimeZoneInfo.ConvertTime( dateTime, sourceTimeZone, destinationTimeZone );
		}

		/// <summary>
		///     Creates the time zone info.
		/// </summary>
		private static TimeZoneInfo CreateTimezone( )
		{
			TimeZoneInfo tzi = null;

			RequestContext requestContext = RequestContext.GetContext( );

			if ( requestContext != null )
			{
				/////
				// Get the time zone from the request context.
				/////
				string timeZone = requestContext.TimeZone;

				if ( !string.IsNullOrEmpty( timeZone ) )
				{
				
                    tzi = TimeZoneHelper.GetTimeZoneInfo(timeZone);
				}
			}

			/////
			// Return the time zone if found or Utc by default.
			/////
			return tzi ?? ( TimeZoneInfo.Utc );
		}


		/// <summary>
		///     Generates the and send I cal appointment.
		/// </summary>
		public static void GenerateAndSendICalAppointment( Appointment appointment )
		{
            throw new NotImplementedException("Support for ICAL has been removed. The required Calendar Inbox has been removed");
            /*
            // Are we sending anything?
            if (! (appointment.ApptSendEmail ?? false))     
            {
                // we are no going to delete the iCalAppt as we may want to turn the email udpates 
                // back on.
                return;
            }

            if (appointment.ICalAppt == null)
            {
                EventEmail eventEmail = Entity.Create<EventEmail>();
                eventEmail.Name = string.Format(appointment.Name);
                appointment.ICalAppt = eventEmail;
                eventEmail.Save();
            }

            var emailServerSettings = Entity.Get<TenantEmailSetting>("core:tenantEmailSettingsInstance");


            /////
            // Calendar inbox.
            /////
            var inbox = Entity.Get<Inbox>("core:calendarInbox");
            iCalendar iCal = GenerateICalendar(appointment, inbox.InboxEmailAddress);

			if ( iCal != null )
			{
				/////
				// Create the SentICalEmailMessage instance.
				/////
                SentICalEmailMessage emailEntity = GenerateEmail(appointment, emailServerSettings, iCal);

				if ( appointment.EventEmailAttendees.Count > 0 )
				{
					/////
					// Send the email.
					/////
					SendEmail( emailEntity.As<SentEmailMessage>( ), iCal, emailServerSettings);
				}
			}*/
		}

		/// <summary>
		///     Generates the email.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">
		///     eventEntity
		///     or
		///     inbox
		///     or
		///     iCal
		/// </exception>
		private static SentICalEmailMessage GenerateEmail( Appointment appointment, TenantEmailSetting emailServerSettings, iCalendar iCal )
		{
            if (appointment.ICalAppt == null)
			{
				throw new ArgumentNullException( "eventEntity" );
			}

			if (emailServerSettings == null )
			{
				throw new ArgumentNullException("emailServerSettings");
			}

			if ( iCal == null )
			{
				throw new ArgumentNullException( "iCal" );
			}

			/////
			// Create the new email message.
			/////
			var emailMessage = Entity.Create<SentICalEmailMessage>( );

			emailMessage.Name = GlobalStrings.SoftwarePlatformCalendarInvite;

			/////
			// Body.
			/////
			if ( appointment.Description != null )
			{
                emailMessage.EmBody = appointment.Description;
			}

			/////
			// From. 
			/////
			emailMessage.EmFrom = emailServerSettings.EmailNoReplyAddress;   // This will need to be chagned to something else 

			/////
			// Is Html.
			/////
			emailMessage.EmIsHtml = true;

			/////
			// Send date.
			/////
			emailMessage.EmSentDate = DateTime.UtcNow;

			/////
			// Subject.
			/////
            emailMessage.EmSubject = !string.IsNullOrEmpty(appointment.Name) ? string.Format(GlobalStrings.ValidCalendarEmailSubject, appointment.Name) : GlobalStrings.InvalidCalendarEmailSubject;

			/////
			// Indicate that the event was cancelled on the subject line.
			/////
			if ( iCal.Method == Methods.Cancel )
			{
				emailMessage.EmSubject = string.Format( "{0}: {1}", GlobalStrings.Cancelled, emailMessage.EmSubject );
			}

			/////
			// Attendees.
			/////
			var attendees = new List<string>( );

			/////
			// To add the organizer as a recipient of the email, uncomment the following line.
			/////
			//attendees.Add( "Software Platform " + inbox.InboxEmailAddress );

            var attendeesEmails = GetAttendeesAddresses(appointment.EventEmailAttendees);

            attendees.AddRange(attendeesEmails.Select(a => string.Format("{0} {1}", a.EmailContactDisplayName, a.Name).Trim()));

			/////
			// Process the attendees.
			/////
            if (appointment.EventEmailAttendees != null)
			{
				emailMessage.EmTo = string.Join( ";", attendees.ToArray( ) );
			}

			ICalendarObjectList<ITimeZone> timeZones = iCal.TimeZones;

			/////
			// Store the time zone id so that any new proposed times will exist in the same time zone.
			/////
			if ( timeZones != null )
			{
				ITimeZone timeZone = timeZones.FirstOrDefault( );

				if ( timeZone != null )
				{
					emailMessage.ICalTimeZone = timeZone.TzId;
				}
			}

			/////
			// Store a reference to the original event email entity.
			/////
			emailMessage.OwnerEventEmail = appointment.ICalAppt;

			emailMessage.SentFromEmailServer = emailServerSettings;

			emailMessage.Save( );

			return emailMessage;
		}

		/// <summary>
		///     Generates the I calendar.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">
		///     eventEntity
		///     or
		///     inbox
		/// </exception>
		private static iCalendar GenerateICalendar( Appointment appointment, string emailAddress )
		{
            if ( appointment == null )
			{
				throw new ArgumentNullException( "eventEntity" );
			}

            if (string.IsNullOrEmpty(emailAddress))
			{
				throw new ArgumentNullException("emailAddress");
			}

			try
			{
				/////
				// Calendar object
				/////
				var calendar = new iCalendar( );
				var calendarEvent = calendar.Create<iCalEvent>( );

				ICalUidContainer iCalUidContainer = appointment.ICalAppt.CalendarId;

				string uid = null;

				/////
				// If there exists a reference to a previously sent message, obtain it and use the iCal UID from it.
				// Note* This is required to ensure proposed updates modify the existing calendar entry and no create a new one.
				/////
				if ( iCalUidContainer != null )
				{
					uid = iCalUidContainer.ICalUid;

					if ( !string.IsNullOrEmpty( uid ) )
					{
						calendarEvent.Uid = uid;
					}
				}

				/////
				// Create the time zone object.
				/////
				TimeZoneInfo tzi = CreateTimezone( );
				ITimeZone tz = null;

				if ( tzi != null )
				{
					tz = calendar.AddTimeZone( tzi );
				}

				/////
				// Start time.
				/////
                if (appointment.EventStart != null)
				{
                    calendarEvent.Start = tz != null ? new iCalDateTime(ConvertDateTime(appointment.EventStart.Value, TimeZoneInfo.Utc, tzi), tz.TzId) : new iCalDateTime(appointment.EventStart.Value);
				}

				/////
				// End time.
				/////
                if (appointment.EventEnd != null)
				{
                    calendarEvent.End = tz != null ? new iCalDateTime(ConvertDateTime(appointment.EventEnd.Value, TimeZoneInfo.Utc, tzi), tz.TzId) : new iCalDateTime(appointment.EventEnd.Value);
				}

				/////
				// All day event.
				/////
                if (appointment.EventIsAllDay != null)
				{
                    calendarEvent.IsAllDay = appointment.EventIsAllDay.Value;
				}

				/////
				// Location.
				/////
                if (appointment.EventLocation != null)
				{
                    calendarEvent.Location = appointment.EventLocation;
				}

				/////
				// Priority. (CUA defined)
				/////
				if ( appointment.EventEmailPriority != null )
				{
                    EventEmailPriorityEnum eventEmailPriorityStatus = appointment.EventEmailPriority;

					switch ( eventEmailPriorityStatus.Alias )
					{
						case "core:lowPriority":
							calendarEvent.Priority = 9;
							break;
						case "core:normalPriority":
							calendarEvent.Priority = 5;
							break;
						case "core:highPriority":
							calendarEvent.Priority = 1;
							break;
					}
				}

				/////
				// Summary.
				/////
				calendarEvent.Summary = !string.IsNullOrEmpty( appointment.Name ) ? string.Format( "{0} - {1}", appointment.Name, GlobalStrings.DefaultICalSummary ) : GlobalStrings.DefaultICalSummary;

				/////
				// Status.
				/////
                if (appointment.EventStatus != null)
				{
                    EventStatusEnum eventStatusEnum = appointment.EventStatus;

					switch ( eventStatusEnum.Alias )
					{
						case "core:eventStatusTentative":
							calendarEvent.Status = Scheduling.iCalendar.EventStatus.Tentative;
							calendar.Method = Methods.Request;
							break;
						case "core:eventStatusConfirmed":
							calendarEvent.Status = Scheduling.iCalendar.EventStatus.Confirmed;
							calendar.Method = Methods.Publish;
							break;
						case "core:eventStatusCancelled":

							/////
							// If there is no previous iCal UID, ignore this since it is invalid.
							/////
							if ( uid == null )
							{
								return null;
							}

							calendarEvent.Status = Scheduling.iCalendar.EventStatus.Cancelled;
							calendar.Method = Methods.Cancel;
							break;
					}
				}
				else
				{
					/////
					// New and Updated events must be Requests.
					/////
					calendar.Method = Methods.Request;
				}

				/////
				// Process the organizer.
				/////
				ProcessOrganizer( calendarEvent, emailAddress );

				/////
				// Process attendees.
				/////
				ProcessAttendees( calendarEvent, appointment );

                // if there are no attendees there is no iCal appointment
				if ( calendarEvent.Attendees.Count <= 0 )
                    return null;

                if (appointment.ICalAppt.CalendarId == null || appointment.ICalAppt.CalendarId.ICalUid != calendarEvent.Uid)
				{
					/////
					// Create an iCal UID Container.
					/////
					var container = new ICalUidContainer
						{
							ICalUid = calendarEvent.Uid,
							CalendarEventEmail = appointment.ICalAppt
						};

					/////
					// Store the iCal UID in the email message so we can correlate any new proposed times.
					/////

					/////
					// Save the container.
					/////
					container.Save( );
				}

				return calendar;
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( "Failed to generate iCalendar attachment." + exc );
			}

			return null;
		}



		/// <summary>
		///     Processes the attendees.
		/// </summary>
		private static void ProcessAttendees( iCalEvent calendarEvent, Appointment appointment )
		{
            if (appointment == null || calendarEvent == null)
			{
				return;
			}

			/////
			// Get the attendees.
			/////
            var attendees = appointment.EventEmailAttendees;

			if ( attendees == null || attendees.Count <= 0 )
			{
				return;
			}

			/////
			// Attendees.
			/////
            foreach (var emailContact in GetAttendeesAddresses(attendees))
			{
                var attendee = new Attendee(emailContact.Name, emailContact.EmailContactDisplayName, ParticipantRole.RequiredParticipant);
				calendarEvent.Attendees.Add( attendee );

				/////
				// RSVP contact.
				/////
				attendee.Rsvp = true;
			}
		}

        /// <summary>
        /// Get the best email addresses for the attendees that have them
        /// </summary>
        static IList<EmailContact>  GetAttendeesAddresses(IEntityCollection<Person> attendees)
        {
            return attendees.Select(a => GetBestAddress(a)).Where(e => e != null).ToList();
        }

        /// <summary>
        /// Get the best address to contact the person. Starting with Business then other then personal.
        /// </summary>
        static EmailContact GetBestAddress(Person person)
        {
            EmailContact result = person.PersonHasEmailContact.FirstOrDefault(c => (c.EmailContactIsDefault ?? false) && !(c.EmailContactIsInactive ?? false));

            if (result == null)
                result = person.PersonHasEmailContact.FirstOrDefault(c => c.EmailType_Enum == EmailTypeEnum_Enumeration.EmailTypeBusiness && !(c.EmailContactIsInactive ?? false));
            
            if (result == null)
                result = person.PersonHasEmailContact.FirstOrDefault(c => c.EmailType_Enum == EmailTypeEnum_Enumeration.EmailTypeOther && !(c.EmailContactIsInactive ?? false));
            
            if (result == null)
                result = person.PersonHasEmailContact.FirstOrDefault(c => c.EmailType_Enum == EmailTypeEnum_Enumeration.EmailTypePersonal && !(c.EmailContactIsInactive ?? false));

            return result;
        }

		/// <summary>
		///     Processes the organizer.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">
		///     calendarEvent
		///     or
		///     inbox
		/// </exception>
		private static void ProcessOrganizer( iCalEvent calendarEvent, string emailAddress )
		{
			if ( calendarEvent == null )
			{
				throw new ArgumentNullException( "calendarEvent" );
			}

			if ( String.IsNullOrEmpty(emailAddress))
			{
				throw new ArgumentNullException("emailAddress");
			}

			/////
			// Create the organizer attendee.
			/////
			var organizer = new Organizer
				{
					CommonName = "Software Platform",
					Value = new Uri( string.Format( "MAILTO:{0}", emailAddress) )
				};

			calendarEvent.Organizer = organizer;
		}

		/// <summary>
		///     Sends the email.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">
		///     emailEntity
		///     or
		///     iCal
		///     or
		///     inbox
		/// </exception>
		private static void SendEmail( SentEmailMessage emailEntity, iCalendar iCal, TenantEmailSetting emailServerSettings)
		{
			if ( emailEntity == null )
			{
				throw new ArgumentNullException( "emailEntity" );
			}

			if ( iCal == null )
			{
				throw new ArgumentNullException( "iCal" );
			}

			if (emailServerSettings == null )
			{
				throw new ArgumentNullException("emailServerSettings");
			}


			/////
			// Send the email.
			/////
			iCalEmailHelper.SendICalEmail( iCal, emailEntity.ToMailMessage( ), emailServerSettings);
		}
	}
}