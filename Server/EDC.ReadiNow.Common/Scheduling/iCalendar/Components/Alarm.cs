// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents an RFC 2445 VALARM component.
	///     FIXME: move GetOccurrences() logic into an AlarmEvaluator.
	/// </summary>
	[Serializable]
	public class Alarm : CalendarComponent, IAlarm
	{
		/// <summary>
		///     Occurrences.
		/// </summary>
		private List<AlarmOccurrence> _occurrences;

		/// <summary>
		///     Gets or sets the action.
		/// </summary>
		/// <value>
		///     The action.
		/// </value>
		public virtual AlarmAction Action
		{
			get
			{
				return Properties.Get<AlarmAction>( "ACTION" );
			}
			set
			{
				Properties.Set( "ACTION", value );
			}
		}

		/// <summary>
		///     Gets or sets the attachment.
		/// </summary>
		/// <value>
		///     The attachment.
		/// </value>
		public virtual IAttachment Attachment
		{
			get
			{
				return Properties.Get<IAttachment>( "ATTACH" );
			}
			set
			{
				Properties.Set( "ATTACH", value );
			}
		}

		/// <summary>
		///     Gets or sets the attendees.
		/// </summary>
		/// <value>
		///     The attendees.
		/// </value>
		public virtual IList<IAttendee> Attendees
		{
			get
			{
				return Properties.GetMany<IAttendee>( "ATTENDEE" );
			}
			set
			{
				Properties.Set( "ATTENDEE", value );
			}
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public virtual string Description
		{
			get
			{
				return Properties.Get<string>( "DESCRIPTION" );
			}
			set
			{
				Properties.Set( "DESCRIPTION", value );
			}
		}

		/// <summary>
		///     Gets or sets the duration.
		/// </summary>
		/// <value>
		///     The duration.
		/// </value>
		public virtual TimeSpan Duration
		{
			get
			{
				return Properties.Get<TimeSpan>( "DURATION" );
			}
			set
			{
				Properties.Set( "DURATION", value );
			}
		}

		/// <summary>
		///     Gets or sets the repeat.
		/// </summary>
		/// <value>
		///     The repeat.
		/// </value>
		public virtual int Repeat
		{
			get
			{
				return Properties.Get<int>( "REPEAT" );
			}
			set
			{
				Properties.Set( "REPEAT", value );
			}
		}

		/// <summary>
		///     Gets or sets the summary.
		/// </summary>
		/// <value>
		///     The summary.
		/// </value>
		public virtual string Summary
		{
			get
			{
				return Properties.Get<string>( "SUMMARY" );
			}
			set
			{
				Properties.Set( "SUMMARY", value );
			}
		}

		/// <summary>
		///     Gets or sets the trigger.
		/// </summary>
		/// <value>
		///     The trigger.
		/// </value>
		public virtual ITrigger Trigger
		{
			get
			{
				return Properties.Get<ITrigger>( "TRIGGER" );
			}
			set
			{
				Properties.Set( "TRIGGER", value );
			}
		}

		/// <summary>
		///     Gets or sets the occurrences.
		/// </summary>
		/// <value>
		///     The occurrences.
		/// </value>
		protected virtual List<AlarmOccurrence> Occurrences
		{
			get
			{
				return _occurrences;
			}
			set
			{
				_occurrences = value;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Alarm" /> class.
		/// </summary>
		public Alarm( )
		{
			Initialize( );
		}

		private void Initialize( )
		{
			Name = Components.ALARM;
			Occurrences = new List<AlarmOccurrence>( );
		}

		/// <summary>
		///     Gets a list of alarm occurrences for the given recurring component, <paramref name="rc" />
		///     that occur between <paramref name="fromDate" /> and <paramref name="toDate" />.
		/// </summary>
		/// <param name="rc"></param>
		/// <param name="fromDate"></param>
		/// <param name="toDate"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Alarm trigger is relative to the END of the occurrence; however, the occurrence has no discernible end.</exception>
		public virtual IList<AlarmOccurrence> GetOccurrences( IRecurringComponent rc, IDateTime fromDate, IDateTime toDate )
		{
			Occurrences.Clear( );

			if ( Trigger != null )
			{
				// If the trigger is relative, it can recur right along with
				// the recurring items, otherwise, it happens once and
				// only once (at a precise time).
				if ( Trigger.IsRelative )
				{
					// Ensure that "FromDate" has already been set
					if ( fromDate == null )
					{
						fromDate = rc.Start.Copy<IDateTime>( );
					}

					TimeSpan d = default( TimeSpan );
					foreach ( Occurrence o in rc.GetOccurrences( fromDate, toDate ) )
					{
						IDateTime dt = o.Period.StartTime;
						if ( Trigger.Related == TriggerRelation.End )
						{
							if ( o.Period.EndTime != null )
							{
								dt = o.Period.EndTime;
								if ( d == default( TimeSpan ) )
								{
									d = o.Period.Duration;
								}
							}
								// Use the "last-found" duration as a reference point
							else if ( d != default( TimeSpan ) )
							{
								dt = o.Period.StartTime.Add( d );
							}
							else
							{
								throw new ArgumentException( "Alarm trigger is relative to the END of the occurrence; however, the occurrence has no discernible end." );
							}
						}

						if ( Trigger.Duration != null )
						{
							Occurrences.Add( new AlarmOccurrence( this, dt.Add( Trigger.Duration.Value ), rc ) );
						}
					}
				}
				else
				{
					var dt = Trigger.DateTime.Copy<IDateTime>( );
					dt.AssociatedObject = this;
					Occurrences.Add( new AlarmOccurrence( this, dt, rc ) );
				}

				// If a REPEAT and DURATION value were specified,
				// then handle those repetitions here.
				AddRepeatedItems( );
			}

			return Occurrences;
		}

		/// <summary>
		///     Polls the <see cref="Alarm" /> component for alarms that have been triggered
		///     since the provided <paramref name="start" /> date/time.  If <paramref name="start" />
		///     is null, all triggered alarms will be returned.
		/// </summary>
		/// <param name="start">The earliest date/time to poll triggered alarms for.</param>
		/// <param name="end">The end.</param>
		/// <returns>
		///     A list of <see cref="AlarmOccurrence" /> objects, each containing a triggered alarm.
		/// </returns>
		public virtual IList<AlarmOccurrence> Poll( IDateTime start, IDateTime end )
		{
			var results = new List<AlarmOccurrence>( );

			// Evaluate the alarms to determine the recurrences
			var rc = Parent as RecurringComponent;
			if ( rc != null )
			{
				results.AddRange( GetOccurrences( rc, start, end ) );
				results.Sort( );
			}
			return results;
		}

		/// <summary>
		///     Handles the repetitions that occur from the <c>REPEAT</c> and
		///     <c>DURATION</c> properties.  Each recurrence of the alarm will
		///     have its own set of generated repetitions.
		/// </summary>
		protected virtual void AddRepeatedItems( )
		{
			if ( Repeat > 0 )
			{
				int len = Occurrences.Count;
				for ( int i = 0; i < len; i++ )
				{
					AlarmOccurrence ao = Occurrences[ i ];
					var alarmTime = ao.DateTime.Copy<IDateTime>( );

					for ( int j = 0; j < Repeat; j++ )
					{
						alarmTime = alarmTime.Add( Duration );
						Occurrences.Add( new AlarmOccurrence( this, alarmTime.Copy<IDateTime>( ), ao.Component ) );
					}
				}
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
	}
}