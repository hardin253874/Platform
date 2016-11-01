// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar.Email
{
	public static class iCalEmailHelper
	{
		public static void SendICalEmail( IICalendar iCal, MailMessage message, Inbox inbox )
		{
			/////
			// Get the inbox provider.
			/////
			InboxProvider inboxProvider = inbox.UsesInboxProvider;

			if ( inboxProvider == null )
			{
				return;
			}

			/////
			// Create an instance of the iCalendar serializer.
			/////
			var serializer = new iCalendarSerializer( );

			/////
			// Determine whether an alternate view of type HTML is required.
			/////
			if ( message.IsBodyHtml && !string.IsNullOrEmpty( message.Body ) )
			{
				/////
				// Create the html content type.
				/////
				var htmlContentType = new ContentType( MediaTypeNames.Text.Html );

				/////
				// Add the html alternate view.
				/////
				AlternateView htmlAlternateView = AlternateView.CreateAlternateViewFromString( message.Body, htmlContentType );

				message.AlternateViews.Add( htmlAlternateView );
			}

			/////
			// Create the calendar content type.
			/////
			var calendarContentType = new ContentType( "text/calendar" );

			if ( calendarContentType.Parameters != null )
			{
				calendarContentType.Parameters.Add( "method", "REQUEST" );
			}

			/////
			// Add the calendar alternate view.
			/////
			AlternateView calendarAlternateView = AlternateView.CreateAlternateViewFromString( serializer.SerializeToString( iCal ), calendarContentType );

			message.AlternateViews.Add( calendarAlternateView );

			/////
			// Get a list of MailMessage instances.
			/////
			var messages = new List<MailMessage>
				{
					message
				};

			/////
			// Send the messages.
			/////
            var inboxProviderHelper = inboxProvider.GetHelper();
            inboxProviderHelper.SendMessages(messages, RequestContext.GetContext().Tenant.Name, inbox.Name);
		}
	}
}
