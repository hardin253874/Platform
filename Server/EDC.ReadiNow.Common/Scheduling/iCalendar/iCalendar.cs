// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Utility;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents an iCalendar object.  To load an iCalendar object, generally a
	///     static LoadFromXXX method is used.
	///     <example>
	///         For example, use the following code to load an iCalendar object from a URL:
	///         <code>
	///          IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
	///       </code>
	///     </example>
	///     Once created, an iCalendar object can be used to gathers relevant information about
	///     events, to-dos, time zones, journal entries, and free/busy time.
	/// </summary>
	/// <remarks>
	///     <para>
	///         The following is an example of loading an iCalendar and displaying a text-based calendar.
	///         <code>
	/// 				//
	/// 				// The following code loads and displays an iCalendar 
	/// 				// with US Holidays for 2006.
	/// 				//
	/// 				IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://www.applegatehomecare.com/Calendars/USHolidays.ics"));
	/// 				
	/// 				IList&lt;Occurrence&gt; occurrences = iCal.GetOccurrences(
	/// 				    new iCalDateTime(2006, 1, 1, "US-Eastern", iCal),
	/// 				    new iCalDateTime(2006, 12, 31, "US-Eastern", iCal));
	/// 				
	/// 				foreach (Occurrence o in occurrences)
	/// 				{
	/// 				    IEvent iEvent = o.Component as IEvent;
	/// 				    if (iEvent != null)
	/// 				    {
	/// 				        // Display the date of the event
	/// 				        Console.Write(o.Period.StartTime.Local.Date.ToString("MM/dd/yyyy") + " -\t");
	///  
	/// 				        // Display the event summary
	/// 				        Console.Write(evt.Summary);
	///  
	/// 				        // Display the time the event happens (unless it's an all-day event)
	/// 				        if (evt.Start.HasTime)
	/// 				        {
	/// 				            Console.Write(" (" + evt.Start.Local.ToShortTimeString() + " - " + evt.End.Local.ToShortTimeString());
	/// 				            if (evt.Start.TimeZoneInfo != null)
	/// 				                Console.Write(" " + evt.Start.TimeZoneInfo.TimeZoneName);
	/// 				            Console.Write(")");
	/// 				        }
	///  
	/// 				        Console.Write(Environment.NewLine);
	/// 				    }
	/// 				}
	///          </code>
	///     </para>
	///     <para>
	///         The following example loads all active to-do items from an iCalendar:
	///         <code>
	/// 				//
	/// 				// The following code loads and displays active todo items from an iCalendar
	/// 				// for January 6th, 2006.    
	/// 				//
	/// 				IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));    
	/// 				
	/// 				iCalDateTime dateTime = new iCalDateTime(2006, 1, 6, "US-Eastern", iCal);
	/// 				foreach(Todo todo in iCal.Todos)
	/// 				{
	/// 				    if (todo.IsActive(dateTime))
	/// 				    {
	/// 				        // Display the todo summary
	/// 				        Console.WriteLine(todo.Summary);
	/// 				    }
	/// 				}
	///          </code>
	///     </para>
	///     <para>
	///         The DDay.iCal library, its documentation, and its source code
	///         are Copyright � 2010 Douglas Day
	///         &lt;Doug@ ddaysoftware.com&gt;
	///         .
	///         All rights reserved.
	///         Redistribution and use in source and binary forms, with or
	///         without modification, are permitted provided that the following
	///         conditions are met:
	///         * Redistributions of source code must retain the above
	///         copyright notice, this list of conditions and the
	///         following disclaimer.
	///         * Redistributions in binary form must reproduce the above
	///         copyright notice, this list of conditions and the
	///         following disclaimer in the documentation and/or other
	///         materials provided with the distribution.
	///         * Neither the name "DDay.iCal" nor the names of
	///         its contributors may be used to endorse or promote
	///         products derived from this software without specific
	///         prior written permission.
	///         THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
	///         CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
	///         INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
	///         MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	///         DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
	///         CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
	///         SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
	///         NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	///         LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
	///         HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
	///         CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
	///         OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
	///         EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	///     </para>
	/// </remarks>
	[Serializable]
// ReSharper disable InconsistentNaming
	public class iCalendar : CalendarComponent, IICalendar, IDisposable
// ReSharper restore InconsistentNaming

	{
		/// <summary>
		///     Loads an <see cref="iCalendar" /> from the file system.
		/// </summary>
		/// <param name="filepath">The file path.</param>
		/// <returns>
		///     An <see cref="iCalendar" /> object
		/// </returns>
		public static IICalendarCollection LoadFromFile( string filepath )
		{
			return LoadFromFile( filepath, Encoding.UTF8, new iCalendarSerializer( ) );
		}

		/// <summary>
		///     Loads from file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="filepath">The file path.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromFile<T>( string filepath ) where T : IICalendar
		{
			return LoadFromFile( typeof ( T ), filepath );
		}

		/// <summary>
		///     Loads from file.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="filepath">The file path.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromFile( Type iCalendarType, string filepath )
		{
			ISerializer serializer = new iCalendarSerializer( );
			serializer.GetService<ISerializationSettings>( ).iCalendarType = iCalendarType;
			return LoadFromFile( filepath, Encoding.UTF8, serializer );
		}

		/// <summary>
		///     Loads from file.
		/// </summary>
		/// <param name="filepath">The file path.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromFile( string filepath, Encoding encoding )
		{
			return LoadFromFile( filepath, encoding, new iCalendarSerializer( ) );
		}

		/// <summary>
		///     Loads from file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="filepath">The file path.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromFile<T>( string filepath, Encoding encoding ) where T : IICalendar
		{
			return LoadFromFile( typeof ( T ), filepath, encoding );
		}

		/// <summary>
		///     Loads from file.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="filepath">The file path.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromFile( Type iCalendarType, string filepath, Encoding encoding )
		{
			ISerializer serializer = new iCalendarSerializer( );
			serializer.GetService<ISerializationSettings>( ).iCalendarType = iCalendarType;
			return LoadFromFile( filepath, encoding, serializer );
		}

		/// <summary>
		///     Loads from file.
		/// </summary>
		/// <param name="filepath">The file path.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="serializer">The serializer.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromFile( string filepath, Encoding encoding, ISerializer serializer )
		{
			// NOTE: Fixes bug #3211934 - Bug in iCalendar.cs - UnauthorizedAccessException
			var fs = new FileStream( filepath, FileMode.Open, FileAccess.Read );

			IICalendarCollection calendars = LoadFromStream( fs, encoding, serializer );
			fs.Close( );
			return calendars;
		}

		/// <summary>
		///     Loads an <see cref="iCalendar" /> from an open stream.
		/// </summary>
		/// <param name="s">
		///     The stream from which to load the <see cref="iCalendar" /> object
		/// </param>
		/// <returns>
		///     An <see cref="iCalendar" /> object
		/// </returns>
		public new static IICalendarCollection LoadFromStream( Stream s )
		{
			return LoadFromStream( s, Encoding.UTF8, new iCalendarSerializer( ) );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromStream<T>( Stream s ) where T : IICalendar
		{
			return LoadFromStream( typeof ( T ), s );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromStream( Type iCalendarType, Stream s )
		{
			ISerializer serializer = new iCalendarSerializer( );
			serializer.GetService<ISerializationSettings>( ).iCalendarType = iCalendarType;
			return LoadFromStream( s, Encoding.UTF8, serializer );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public new static IICalendarCollection LoadFromStream( Stream s, Encoding encoding )
		{
			return LoadFromStream( s, encoding, new iCalendarSerializer( ) );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s">The s.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public new static IICalendarCollection LoadFromStream<T>( Stream s, Encoding encoding ) where T : IICalendar
		{
			return LoadFromStream( typeof ( T ), s, encoding );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="s">The s.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromStream( Type iCalendarType, Stream s, Encoding encoding )
		{
			ISerializer serializer = new iCalendarSerializer( );
			serializer.GetService<ISerializationSettings>( ).iCalendarType = iCalendarType;
			return LoadFromStream( s, encoding, serializer );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="e">The e.</param>
		/// <param name="serializer">The serializer.</param>
		/// <returns></returns>
		public new static IICalendarCollection LoadFromStream( Stream s, Encoding e, ISerializer serializer )
		{
			return serializer.Deserialize( s, e ) as IICalendarCollection;
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public new static IICalendarCollection LoadFromStream( TextReader tr )
		{
			return LoadFromStream( tr, new iCalendarSerializer( ) );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public new static IICalendarCollection LoadFromStream<T>( TextReader tr ) where T : IICalendar
		{
			return LoadFromStream( typeof ( T ), tr );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromStream( Type iCalendarType, TextReader tr )
		{
			ISerializer serializer = new iCalendarSerializer( );
			serializer.GetService<ISerializationSettings>( ).iCalendarType = iCalendarType;
			return LoadFromStream( tr, serializer );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <param name="serializer">The serializer.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromStream( TextReader tr, ISerializer serializer )
		{
			string text = tr.ReadToEnd( );
			var ms = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
			return LoadFromStream( ms, Encoding.UTF8, serializer );
		}

		/// <summary>
		///     Loads an <see cref="iCalendar" /> from a given Uri.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns>
		///     An <see cref="iCalendar" /> object
		/// </returns>
		public static IICalendarCollection LoadFromUri( Uri uri )
		{
			return LoadFromUri( typeof ( iCalendar ), uri, null, null, null );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri<T>( Uri uri ) where T : IICalendar
		{
			return LoadFromUri( typeof ( T ), uri, null, null, null );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri( Type iCalendarType, Uri uri )
		{
			return LoadFromUri( iCalendarType, uri, null, null, null );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="proxy">The proxy.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri( Uri uri, WebProxy proxy )
		{
			return LoadFromUri( typeof ( iCalendar ), uri, null, null, proxy );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uri">The URI.</param>
		/// <param name="proxy">The proxy.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri<T>( Uri uri, WebProxy proxy )
		{
			return LoadFromUri( typeof ( T ), uri, null, null, proxy );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="uri">The URI.</param>
		/// <param name="proxy">The proxy.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri( Type iCalendarType, Uri uri, WebProxy proxy )
		{
			return LoadFromUri( iCalendarType, uri, null, null, proxy );
		}

		/// <summary>
		///     Loads an <see cref="iCalendar" /> from a given Uri, using a
		///     specified <paramref name="username" /> and <paramref name="password" />
		///     for credentials.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns>
		///     an <see cref="iCalendar" /> object
		/// </returns>
		public static IICalendarCollection LoadFromUri( Uri uri, string username, string password )
		{
			return LoadFromUri( typeof ( iCalendar ), uri, username, password, null );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uri">The URI.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri<T>( Uri uri, string username, string password ) where T : IICalendar
		{
			return LoadFromUri( typeof ( T ), uri, username, password, null );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="uri">The URI.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri( Type iCalendarType, Uri uri, string username, string password )
		{
			return LoadFromUri( iCalendarType, uri, username, password, null );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <param name="proxy">The proxy.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri( Uri uri, string username, string password, WebProxy proxy )
		{
			return LoadFromUri( typeof ( iCalendar ), uri, username, password, proxy );
		}

		/// <summary>
		///     Loads from URI.
		/// </summary>
		/// <param name="iCalendarType">Type of the i calendar.</param>
		/// <param name="uri">The URI.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <param name="proxy">The proxy.</param>
		/// <returns></returns>
		public static IICalendarCollection LoadFromUri( Type iCalendarType, Uri uri, string username, string password, WebProxy proxy )
		{
			try
			{
				WebRequest request = WebRequest.Create( uri );

				if ( username != null && password != null )
				{
					request.Credentials = new NetworkCredential( username, password );
				}

				if ( proxy != null )
				{
					request.Proxy = proxy;
				}

				var evt = new AutoResetEvent( false );

				string str = null;
				request.BeginGetResponse( delegate( IAsyncResult result )
					{
						Encoding e = Encoding.UTF8;

						try
						{
							using ( WebResponse resp = request.EndGetResponse( result ) )
							{
								// Try to determine the content encoding
								try
								{
									var keys = new List<string>( resp.Headers.AllKeys );
									if ( keys.Contains( "Content-Encoding" ) )
									{
										e = Encoding.GetEncoding( resp.Headers[ "Content-Encoding" ] );
									}
								}
// ReSharper disable EmptyGeneralCatchClause
								catch
// ReSharper restore EmptyGeneralCatchClause
								{
									// Fail gracefully back to UTF-8
								}

								using ( Stream stream = resp.GetResponseStream( ) )
								{
									if ( stream != null )
									{
										using ( var sr = new StreamReader( stream, e ) )
										{
											str = sr.ReadToEnd( );
										}
									}
								}
							}
						}
						finally
						{
							evt.Set( );
						}
					}, null );

				evt.WaitOne( );

				if ( str != null )
				{
					return LoadFromStream( new StringReader( str ) );
				}
				return null;
			}
			catch ( WebException )
			{
				return null;
			}
		}

		/// <summary>
		///     Unique components.
		/// </summary>
		private IUniqueComponentList<IUniqueComponent> _uniqueComponents;

		/// <summary>
		///     Events.
		/// </summary>
		private IUniqueComponentList<IEvent> _events;

		/// <summary>
		///     To-dos.
		/// </summary>
		private IUniqueComponentList<ITodo> _todos;

		/// <summary>
		///     Journals.
		/// </summary>
		private ICalendarObjectList<IJournal> _journals;

		/// <summary>
		///     FreeBusy.
		/// </summary>
		private IUniqueComponentList<IFreeBusy> _freeBusy;

		/// <summary>
		///     TimeZones.
		/// </summary>
		private ICalendarObjectList<ITimeZone> _timeZones;

		/// <summary>
		///     To load an existing an iCalendar object, use one of the provided LoadFromXXX methods.
		///     <example>
		///         For example, use the following code to load an iCalendar object from a URL:
		///         <code>
		/// IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
		/// </code>
		///     </example>
		/// </summary>
		public iCalendar( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			Name = Components.CALENDAR;

			_uniqueComponents = new UniqueComponentListProxy<IUniqueComponent>( Children );
			_events = new UniqueComponentListProxy<IEvent>( Children );
			_todos = new UniqueComponentListProxy<ITodo>( Children );
			_journals = new CalendarObjectListProxy<IJournal>( Children );
			_freeBusy = new UniqueComponentListProxy<IFreeBusy>( Children );
			_timeZones = new CalendarObjectListProxy<ITimeZone>( Children );
		}

		/// <summary>
		///     Called when deserializing.
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserializing( StreamingContext context )
		{
			base.OnDeserializing( context );

			Initialize( );
		}

		#region IICalendar Members

		/// <summary>
		///     Gets a list of unique components contained in the calendar.
		/// </summary>
		public virtual IUniqueComponentList<IUniqueComponent> UniqueComponents
		{
			get
			{
				return _uniqueComponents;
			}
		}

		/// <summary>
		///     Gets the recurring items.
		/// </summary>
		/// <value>
		///     The recurring items.
		/// </value>
		public virtual IEnumerable<IRecurrable> RecurringItems
		{
			get
			{
				return Children.OfType<IRecurrable>( );
			}
		}

		/// <summary>
		///     A collection of <see cref="Event" /> components in the iCalendar.
		/// </summary>
		public virtual IUniqueComponentList<IEvent> Events
		{
			get
			{
				return _events;
			}
		}

		/// <summary>
		///     A collection of <see cref="EDC.ReadiNow.Scheduling.iCalendar.FreeBusy" /> components in the iCalendar.
		/// </summary>
		public virtual IUniqueComponentList<IFreeBusy> FreeBusy
		{
			get
			{
				return _freeBusy;
			}
		}

		/// <summary>
		///     A collection of <see cref="Journal" /> components in the iCalendar.
		/// </summary>
		public virtual ICalendarObjectList<IJournal> Journals
		{
			get
			{
				return _journals;
			}
		}

		/// <summary>
		///     A collection of <see cref="TimeZone" /> components in the iCalendar.
		/// </summary>
		public virtual ICalendarObjectList<ITimeZone> TimeZones
		{
			get
			{
				return _timeZones;
			}
		}

		/// <summary>
		///     A collection of <see cref="Todo" /> components in the iCalendar.
		/// </summary>
		public virtual IUniqueComponentList<ITodo> Todos
		{
			get
			{
				return _todos;
			}
		}

		/// <summary>
		///     Gets/sets the calendar version.  Defaults to "2.0".
		/// </summary>
		public virtual string Version
		{
			get
			{
				return Properties.Get<string>( "VERSION" );
			}
			set
			{
				Properties.Set( "VERSION", value );
			}
		}

		/// <summary>
		///     Gets/sets the product ID for the calendar.
		/// </summary>
		public virtual string ProductId
		{
			get
			{
				return Properties.Get<string>( "PRODID" );
			}
			set
			{
				Properties.Set( "PRODID", value );
			}
		}

		/// <summary>
		///     Gets/sets the scale of the calendar.
		/// </summary>
		public virtual string Scale
		{
			get
			{
				return Properties.Get<string>( "CALSCALE" );
			}
			set
			{
				Properties.Set( "CALSCALE", value );
			}
		}

		/// <summary>
		///     Gets/sets the calendar method.
		/// </summary>
		public virtual Methods Method
		{
			get
			{
				var method = Properties.Get<string>( "METHOD" );

				if ( !string.IsNullOrEmpty( method ) )
				{
					return method.FromDescription<Methods>( );
				}

				return Methods.NotSpecified;
			}
			set
			{
				if ( value == Methods.NotSpecified )
				{
					Properties.Remove( "METHOD" );
				}
				else
				{
					Properties.Set( "METHOD", value.ToDescription( ) );
				}
			}
		}

		/// <summary>
		///     Gets/sets the restriction on how evaluation of
		///     recurrence patterns occurs within this calendar.
		/// </summary>
		public virtual RecurrenceRestrictionType RecurrenceRestriction
		{
			get
			{
				return Properties.Get<RecurrenceRestrictionType>( "X-SOFTWAREPLATFORM-ICAL-RECURRENCE-RESTRICTION" );
			}
			set
			{
				Properties.Set( "X-SOFTWAREPLATFORM-ICAL-RECURRENCE-RESTRICTION", value );
			}
		}

		/// <summary>
		///     Gets/sets the evaluation mode during recurrence
		///     evaluation.  Default is ThrowException.
		/// </summary>
		public virtual RecurrenceEvaluationModeType RecurrenceEvaluationMode
		{
			get
			{
				return Properties.Get<RecurrenceEvaluationModeType>( "X-SOFTWAREPLATFORM-ICAL-RECURRENCE-EVALUATION-MODE" );
			}
			set
			{
				Properties.Set( "X-SOFTWAREPLATFORM-ICAL-RECURRENCE-EVALUATION-MODE", value );
			}
		}

		/// <summary>
		///     Adds a time zone to the iCalendar.  This time zone may
		///     then be used in date/time objects contained in the
		///     calendar.
		/// </summary>
		/// <param name="tz">The time zone.</param>
		/// <returns>
		///     The time zone added to the calendar.
		/// </returns>
		public ITimeZone AddTimeZone( ITimeZone tz )
		{
			this.AddChild( tz );
			return tz;
		}

		/// <summary>
		///     Adds a system time zone to the iCalendar.  This time zone may
		///     then be used in date/time objects contained in the
		///     calendar.
		/// </summary>
		/// <param name="tzi">A System.TimeZoneInfo object to add to the calendar.</param>
		/// <returns>
		///     The time zone added to the calendar.
		/// </returns>
		public ITimeZone AddTimeZone( TimeZoneInfo tzi )
		{
			ITimeZone tz = iCalTimeZone.FromSystemTimeZone( tzi );
			this.AddChild( tz );
			return tz;
		}

		/// <summary>
		///     Adds the time zone.
		/// </summary>
		/// <param name="tzi">The time zone info.</param>
		/// <param name="earliestDateTimeToSupport">The earliest date time to support.</param>
		/// <param name="includeHistoricalData">
		///     if set to <c>true</c> [include historical data].
		/// </param>
		/// <returns></returns>
		public ITimeZone AddTimeZone( TimeZoneInfo tzi, DateTime earliestDateTimeToSupport, bool includeHistoricalData )
		{
			ITimeZone tz = iCalTimeZone.FromSystemTimeZone( tzi, earliestDateTimeToSupport, includeHistoricalData );
			this.AddChild( tz );
			return tz;
		}

		/// <summary>
		///     Adds the local system time zone to the iCalendar.
		///     This time zone may then be used in date/time
		///     objects contained in the calendar.
		/// </summary>
		/// <returns>
		///     The time zone added to the calendar.
		/// </returns>
		public ITimeZone AddLocalTimeZone( )
		{
			ITimeZone tz = iCalTimeZone.FromLocalTimeZone( );
			this.AddChild( tz );
			return tz;
		}

		/// <summary>
		///     Adds the local time zone.
		/// </summary>
		/// <param name="earliestDateTimeToSupport">The earliest date time to support.</param>
		/// <param name="includeHistoricalData">
		///     if set to <c>true</c> [include historical data].
		/// </param>
		/// <returns></returns>
		public ITimeZone AddLocalTimeZone( DateTime earliestDateTimeToSupport, bool includeHistoricalData )
		{
			ITimeZone tz = iCalTimeZone.FromLocalTimeZone( earliestDateTimeToSupport, includeHistoricalData );
			this.AddChild( tz );
			return tz;
		}

		/// <summary>
		///     Retrieves the <see cref="TimeZone" /> object for the specified
		///     <see>
		///         <cref>TZID</cref>
		///     </see>
		///     (Time Zone Identifier).
		/// </summary>
		/// <param name="tzid">
		///     A valid
		///     <see>
		///         <cref>TZID</cref>
		///     </see>
		///     object, or a valid
		///     <see>
		///         <cref>TZID</cref>
		///     </see>
		///     string.
		/// </param>
		/// <returns>
		///     A <see cref="TimeZone" /> object for the
		///     <see>
		///         <cref>TZID</cref>
		///     </see>
		///     .
		/// </returns>
		public ITimeZone GetTimeZone( string tzid )
		{
			return TimeZones.FirstOrDefault( tz => Equals( tz.TzId, tzid ) );
		}

		/// <summary>
		///     Clears recurrence evaluations for recurring components.
		/// </summary>
		public void ClearEvaluation( )
		{
			foreach ( IRecurrable recurrable in RecurringItems )
			{
				recurrable.ClearEvaluation( );
			}
		}

		/// <summary>
		///     Returns a list of occurrences of each recurring component
		///     for the date provided (<paramref name="dt" />).
		/// </summary>
		/// <param name="dt">The date for which to return occurrences. Time is ignored on this parameter.</param>
		/// <returns>
		///     A list of occurrences that occur on the given date (<paramref name="dt" />).
		/// </returns>
		public virtual IList<Occurrence> GetOccurrences( IDateTime dt )
		{
			return GetOccurrences<IRecurringComponent>(
				new iCalDateTime( dt.Local.Date ),
				new iCalDateTime( dt.Local.Date.AddDays( 1 ).AddSeconds( -1 ) ) );
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public virtual IList<Occurrence> GetOccurrences( DateTime dt )
		{
			return GetOccurrences<IRecurringComponent>(
				new iCalDateTime( dt.Date ),
				new iCalDateTime( dt.Date.AddDays( 1 ).AddSeconds( -1 ) ) );
		}

		/// <summary>
		///     Returns a list of occurrences of each recurring component
		///     that occur between <paramref name="startTime" /> and <paramref name="endTime" />.
		/// </summary>
		/// <param name="startTime">The starting date range</param>
		/// <param name="endTime">The ending date range</param>
		/// <returns>
		///     A list of occurrences that fall between the dates provided.
		/// </returns>
		public virtual IList<Occurrence> GetOccurrences( IDateTime startTime, IDateTime endTime )
		{
			return GetOccurrences<IRecurringComponent>( startTime, endTime );
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		public virtual IList<Occurrence> GetOccurrences( DateTime startTime, DateTime endTime )
		{
			return GetOccurrences<IRecurringComponent>( new iCalDateTime( startTime ), new iCalDateTime( endTime ) );
		}

		/// <summary>
		///     Returns all occurrences of components of type T that start on the date provided.
		///     All components starting between 12:00:00AM and 11:59:59 PM will be
		///     returned.
		///     <note>
		///         This will first Evaluate() the date range required in order to
		///         determine the occurrences for the date provided, and then return
		///         the occurrences.
		///     </note>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dt">The date for which to return occurrences.</param>
		/// <returns>
		///     A list of Periods representing the occurrences of this object.
		/// </returns>
		public virtual IList<Occurrence> GetOccurrences<T>( IDateTime dt ) where T : IRecurringComponent
		{
			return GetOccurrences<T>(
				new iCalDateTime( dt.Local.Date ),
				new iCalDateTime( dt.Local.Date.AddDays( 1 ).AddTicks( -1 ) ) );
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		public virtual IList<Occurrence> GetOccurrences<T>( DateTime dt ) where T : IRecurringComponent
		{
			return GetOccurrences<T>(
				new iCalDateTime( dt.Date ),
				new iCalDateTime( dt.Date.AddDays( 1 ).AddTicks( -1 ) ) );
		}

		/// <summary>
		///     Returns all occurrences of components of type T that start within the date range provided.
		///     All components occurring between <paramref name="startTime" /> and <paramref name="endTime" />
		///     will be returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startTime">The starting date range</param>
		/// <param name="endTime">The ending date range</param>
		/// <returns></returns>
		public virtual IList<Occurrence> GetOccurrences<T>( IDateTime startTime, IDateTime endTime ) where T : IRecurringComponent
		{
			var occurrences = new List<Occurrence>( );
			foreach ( IRecurrable recurrable in RecurringItems )
			{
				if ( recurrable is T )
				{
					occurrences.AddRange( recurrable.GetOccurrences( startTime, endTime ) );
				}
			}

			occurrences.Sort( );
			return occurrences;
		}

		/// <summary>
		///     Gets the occurrences.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <returns></returns>
		public virtual IList<Occurrence> GetOccurrences<T>( DateTime startTime, DateTime endTime ) where T : IRecurringComponent
		{
			return GetOccurrences<T>( new iCalDateTime( startTime ), new iCalDateTime( endTime ) );
		}

		/// <summary>
		///     Creates a typed object that is a direct child of the iCalendar itself.  Generally,
		///     you would invoke this method to create an Event, Todo, Journal, TimeZone, FreeBusy,
		///     or other base component type.
		/// </summary>
		/// <typeparam name="T">The type of object to create</typeparam>
		/// <returns>
		///     An object of the type specified
		/// </returns>
		/// <example>
		///     To create an event, use the following:
		///     <code>
		/// IICalendar iCal = new iCalendar();
		/// Event evt = iCal.Create&lt;Event&gt;();
		///   </code>
		///     This creates the event, and adds it to the Events list of the iCalendar.
		/// </example>
		public T Create<T>( ) where T : ICalendarComponent
		{
			var obj = Activator.CreateInstance( typeof ( T ) ) as ICalendarObject;
			if ( obj is T )
			{
				this.AddChild( obj );
				return ( T ) obj;
			}
			return default( T );
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Children.Clear( );
		}

		#endregion

		#region IMergeable Members

		/// <summary>
		///     Merges this object with another.
		/// </summary>
		/// <param name="obj"></param>
		public virtual void MergeWith( IMergeable obj )
		{
			var c = obj as IICalendar;
			if ( c != null )
			{
				if ( Name == null )
				{
					Name = c.Name;
				}

				foreach ( ICalendarProperty p in c.Properties )
				{
					if ( !Properties.ContainsKey( p.Name ) )
					{
						Properties.Add( p.Copy<ICalendarProperty>( ) );
					}
				}
				foreach ( ICalendarObject child in c.Children )
				{
					var uniqueComponent = child as IUniqueComponent;

					if ( uniqueComponent != null )
					{
						IUniqueComponent component = UniqueComponents[ uniqueComponent.Uid ];
						if ( component == null )
						{
							this.AddChild( uniqueComponent.Copy<ICalendarObject>( ) );
						}
					}
					else
					{
						var timeZone = child as ITimeZone;

						if ( timeZone != null )
						{
							ITimeZone tz = GetTimeZone( timeZone.TzId );
							if ( tz == null )
							{
								this.AddChild( timeZone.Copy<ICalendarObject>( ) );
							}
						}
						else
						{
							this.AddChild( child.Copy<ICalendarObject>( ) );
						}
					}
				}
			}
		}

		#endregion

		#region IGetFreeBusy Members

		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="freeBusyRequest">The free busy request.</param>
		/// <returns></returns>
		public virtual IFreeBusy GetFreeBusy( IFreeBusy freeBusyRequest )
		{
			return Scheduling.iCalendar.FreeBusy.Create( this, freeBusyRequest );
		}

		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="fromInclusive">From inclusive.</param>
		/// <param name="toExclusive">To exclusive.</param>
		/// <returns></returns>
		public virtual IFreeBusy GetFreeBusy( IDateTime fromInclusive, IDateTime toExclusive )
		{
			return Scheduling.iCalendar.FreeBusy.Create( this, Scheduling.iCalendar.FreeBusy.CreateRequest( fromInclusive, toExclusive, null, null ) );
		}

		/// <summary>
		///     Gets the free busy.
		/// </summary>
		/// <param name="organizer">The organizer.</param>
		/// <param name="contacts">The contacts.</param>
		/// <param name="fromInclusive">From inclusive.</param>
		/// <param name="toExclusive">To exclusive.</param>
		/// <returns></returns>
		public virtual IFreeBusy GetFreeBusy( IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive )
		{
			return Scheduling.iCalendar.FreeBusy.Create( this, Scheduling.iCalendar.FreeBusy.CreateRequest( fromInclusive, toExclusive, organizer, contacts ) );
		}

		#endregion
	}
}