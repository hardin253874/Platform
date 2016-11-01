// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents an RFC 5545 VEVENT component.
	/// </summary>
	/// <note>
	///     TODO: Add support for the following properties:
	///     <list type="bullet">
	///         <item>Add support for the Organizer and Attendee properties</item>
	///         <item>Add support for the Class property</item>
	///         <item>Add support for the Geo property</item>
	///         <item>Add support for the Priority property</item>
	///         <item>Add support for the Related property</item>
	///         <item>Create a TextCollection DataType for 'text' items separated by commas</item>
	///     </list>
	/// </note>
	[Serializable]
	public class Event : RecurringComponent, IEvent
	{
		/// <summary>
		///     The start date/time of the event.
		///     <note>
		///         If the duration has not been set, but
		///         the start/end time of the event is available,
		///         the duration is automatically determined.
		///         Likewise, if the end date/time has not been
		///         set, but a start and duration are available,
		///         the end date/time will be extrapolated.
		///     </note>
		/// </summary>
		public override IDateTime DtStart
		{
			get
			{
				return base.DtStart;
			}
			set
			{
				base.DtStart = value;
				ExtrapolateTimes( );
			}
		}

		/// <summary>
		///     The end date/time of the event.
		///     <note>
		///         If the duration has not been set, but
		///         the start/end time of the event is available,
		///         the duration is automatically determined.
		///         Likewise, if an end time and duration are available,
		///         but a start time has not been set, the start time
		///         will be extrapolated.
		///     </note>
		/// </summary>
		public virtual IDateTime DtEnd
		{
			get
			{
				return Properties.Get<IDateTime>( "DTEND" );
			}
			set
			{
				if ( !Equals( DtEnd, value ) )
				{
					Properties.Set( "DTEND", value );
					ExtrapolateTimes( );
				}
			}
		}

		/// <summary>
		///     The duration of the event.
		///     <note>
		///         If a start time and duration is available,
		///         the end time is automatically determined.
		///         Likewise, if the end time and duration is
		///         available, but a start time is not determined,
		///         the start time will be extrapolated from
		///         available information.
		///     </note>
		/// </summary>
		// NOTE: Duration is not supported by all systems,
		// (i.e. iPhone) and cannot co-exist with DTEnd.
		// RFC 5545 states:
		//
		//      ; either 'dtend' or 'duration' may appear in
		//      ; a 'eventprop', but 'dtend' and 'duration'
		//      ; MUST NOT occur in the same 'eventprop'
		//
		// Therefore, Duration is not serialized, as DTEnd
		// should always be extrapolated from the duration.
		public virtual TimeSpan Duration
		{
			get
			{
				return Properties.Get<TimeSpan>( "DURATION" );
			}
			set
			{
				if ( !Equals( Duration, value ) )
				{
					Properties.Set( "DURATION", value );
					ExtrapolateTimes( );
				}
			}
		}

		/// <summary>
		///     An alias to the DTEnd field (i.e. end date/time).
		/// </summary>
		public virtual IDateTime End
		{
			get
			{
				return DtEnd;
			}
			set
			{
				DtEnd = value;
			}
		}

		/// <summary>
		///     Returns true if the event is an all-day event.
		/// </summary>
		public virtual bool IsAllDay
		{
			get
			{
				return !Start.HasTime;
			}
			set
			{
				// Set whether or not the start date/time
				// has a time value.
				if ( Start != null )
				{
					Start.HasTime = !value;
				}
				if ( End != null )
				{
					End.HasTime = !value;
				}

				if ( value &&
				     Start != null &&
				     End != null &&
				     Equals( Start.Date, End.Date ) )
				{
					Duration = default( TimeSpan );
					End = Start.AddDays( 1 );
				}
			}
		}

		/// <summary>
		///     The geographic location (lat/long) of the event.
		/// </summary>
		public IGeographicLocation GeographicLocation
		{
			get
			{
				return Properties.Get<IGeographicLocation>( "GEO" );
			}
			set
			{
				Properties.Set( "GEO", value );
			}
		}

		/// <summary>
		///     The location of the event.
		/// </summary>
		public string Location
		{
			get
			{
				return Properties.Get<string>( "LOCATION" );
			}
			set
			{
				Properties.Set( "LOCATION", value );
			}
		}

		/// <summary>
		///     Resources that will be used during the event.
		///     <example>Conference room #2</example>
		///     <example>Projector</example>
		/// </summary>
		public IList<string> Resources
		{
			get
			{
				return Properties.GetMany<string>( "RESOURCES" );
			}
			set
			{
				Properties.Set( "RESOURCES", value );
			}
		}

		/// <summary>
		///     The status of the event.
		/// </summary>
		public EventStatus Status
		{
			get
			{
				return Properties.Get<EventStatus>( "STATUS" );
			}
			set
			{
				Properties.Set( "STATUS", value );
			}
		}

		/// <summary>
		///     The transparency of the event.  In other words,
		///     whether or not the period of time this event
		///     occupies can contain other events (transparent),
		///     or if the time cannot be scheduled for anything
		///     else (opaque).
		/// </summary>
		public TransparencyType Transparency
		{
			get
			{
				return Properties.Get<TransparencyType>( "TRANSP" );
			}
			set
			{
				Properties.Set( "TRANSP", value );
			}
		}

		private EventEvaluator _evaluator;

		/// <summary>
		///     Constructs an Event object, with an <see cref="ICalendarObject" />
		///     (usually an iCalendar object) as its parent.
		/// </summary>
		public Event( )
		{
			Initialize( );
		}

		private void Initialize( )
		{
			Name = Components.EVENT;

			_evaluator = new EventEvaluator( this );
			SetService( _evaluator );
		}

		/// <summary>
		///     Use this method to determine if an event occurs on a given date.
		///     <note type="caution">
		///         This event should be called only after the Evaluate
		///         method has calculated the dates for which this event occurs.
		///     </note>
		/// </summary>
		/// <param name="dateTime">The date to test.</param>
		/// <returns>
		///     True if the event occurs on the <paramref name="dateTime" /> provided, False otherwise.
		/// </returns>
		public virtual bool OccursOn( IDateTime dateTime )
		{
			// NOTE: removed UTC from date checks, since a date is a date.
			// It's the start date OR
			// It's after the start date AND
			// an end time was specified, and it's after the test date
			// an end time was not specified, and it's before the end date
			return _evaluator.Periods.Any( p => p.StartTime.Date == dateTime.Date || ( p.StartTime.Date <= dateTime.Date && ( p.EndTime.HasTime && p.EndTime.Date >= dateTime.Date || ( !p.EndTime.HasTime && p.EndTime.Date > dateTime.Date ) ) ) );
		}

		/// <summary>
		///     Use this method to determine if an event begins at a given date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to test.</param>
		/// <returns>True if the event begins at the given date and time</returns>
		public virtual bool OccursAt( IDateTime dateTime )
		{
			return _evaluator.Periods.Any( p => p.StartTime.Equals( dateTime ) );
		}

		/// <summary>
		///     Determines whether or not the <see cref="Event" /> is actively displayed
		///     as an upcoming or occurred event.
		/// </summary>
		/// <returns>True if the event has not been cancelled, False otherwise.</returns>
		public virtual bool IsActive( )
		{
			return ( Status != EventStatus.Cancelled );
		}

		/// <summary>
		///     Gets a value indicating whether [evaluation includes reference date].
		/// </summary>
		/// <value>
		///     <c>true</c> if [evaluation includes reference date]; otherwise, <c>false</c>.
		/// </value>
		protected override bool EvaluationIncludesReferenceDate
		{
			get
			{
				return true;
			}
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

		/// <summary>
		///     Called when deserialized.
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserialized( StreamingContext context )
		{
			base.OnDeserialized( context );

			ExtrapolateTimes( );
		}

		/// <summary>
		///     Extrapolates the times.
		/// </summary>
		private void ExtrapolateTimes( )
		{
			if ( DtEnd == null && DtStart != null && Duration != default( TimeSpan ) )
			{
				DtEnd = DtStart.Add( Duration );
			}
			else if ( Duration == default( TimeSpan ) && DtStart != null && DtEnd != null )
			{
				Duration = DtEnd.Subtract( DtStart );
			}
			else if ( DtStart == null && Duration != default( TimeSpan ) && DtEnd != null )
			{
				DtStart = DtEnd.Subtract( Duration );
			}
		}
	}
}