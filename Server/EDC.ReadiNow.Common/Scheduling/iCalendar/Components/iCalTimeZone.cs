// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents an RFC 5545 VTIMEZONE component.
	/// </summary>
	[Serializable]
// ReSharper disable InconsistentNaming
	public class iCalTimeZone : CalendarComponent, ITimeZone
// ReSharper restore InconsistentNaming
	{
		/// <summary>
		///     Converts from the local time zone.
		/// </summary>
		/// <returns></returns>
		public static iCalTimeZone FromLocalTimeZone( )
		{
			return FromSystemTimeZone( TimeZoneInfo.Local );
		}

		/// <summary>
		///     Converts from the local time zone.
		/// </summary>
		/// <param name="earliestDateTimeToSupport">The earliest date time to support.</param>
		/// <param name="includeHistoricalData">
		///     if set to <c>true</c> [include historical data].
		/// </param>
		/// <returns></returns>
		public static iCalTimeZone FromLocalTimeZone( DateTime earliestDateTimeToSupport, bool includeHistoricalData )
		{
			return FromSystemTimeZone( TimeZoneInfo.Local, earliestDateTimeToSupport, includeHistoricalData );
		}

		/// <summary>
		///     Populates the cal time zone info.
		/// </summary>
		/// <param name="tzi">The time zone info.</param>
		/// <param name="transition">The transition.</param>
		private static void PopulateiCalTimeZoneInfo( ITimeZoneInfo tzi, TimeZoneInfo.TransitionTime transition )
		{
			var recurrence = new RecurrencePattern
				{
					Frequency = FrequencyType.Yearly
				};
			recurrence.ByMonth.Add( transition.Month );
			recurrence.ByHour.Add( transition.TimeOfDay.Hour );
			recurrence.ByMinute.Add( transition.TimeOfDay.Minute );

			if ( transition.IsFixedDateRule )
			{
				recurrence.ByMonthDay.Add( transition.Day );
			}
			else
			{
				recurrence.ByDay.Add( transition.Week != 5 ? new WeekDay( transition.DayOfWeek, transition.Week ) : new WeekDay( transition.DayOfWeek, -1 ) );
			}

			tzi.RecurrenceRules.Add( recurrence );
		}

		/// <summary>
		///     Converts from the system time zone.
		/// </summary>
		/// <param name="tzinfo">The time zone info.</param>
		/// <returns></returns>
		public static iCalTimeZone FromSystemTimeZone( TimeZoneInfo tzinfo )
		{
			// Support date/times for January 1st of the previous year by default.
			return FromSystemTimeZone( tzinfo, new DateTime( DateTime.Now.Year, 1, 1 ).AddYears( -1 ), false );
		}

		/// <summary>
		///     Converts from the system time zone.
		/// </summary>
		/// <param name="tzinfo">The time zone info.</param>
		/// <param name="earliestDateTimeToSupport">The earliest date time to support.</param>
		/// <param name="includeHistoricalData">
		///     if set to <c>true</c> [include historical data].
		/// </param>
		/// <returns></returns>
		public static iCalTimeZone FromSystemTimeZone( TimeZoneInfo tzinfo, DateTime earliestDateTimeToSupport, bool includeHistoricalData )
		{
			TimeZoneInfo.AdjustmentRule[] adjustmentRules = tzinfo.GetAdjustmentRules( );

			TimeSpan utcOffset = tzinfo.BaseUtcOffset;

			var tz = new iCalTimeZone
				{
					TzId = tzinfo.Id
				};

			IDateTime earliest = new iCalDateTime( earliestDateTimeToSupport );

			foreach ( TimeZoneInfo.AdjustmentRule adjustmentRule in adjustmentRules )
			{
				// Only include historical data if asked to do so.  Otherwise,
				// use only the most recent adjustment rule available.
				if ( !includeHistoricalData && adjustmentRule.DateEnd < earliestDateTimeToSupport )
				{
					continue;
				}

				TimeSpan delta = adjustmentRule.DaylightDelta;

				var tzinfoStandard = new iCalTimeZoneInfo
					{
						Name = "STANDARD",
						TimeZoneName = tzinfo.StandardName,
						Start = new iCalDateTime( new DateTime( adjustmentRule.DateStart.Year, adjustmentRule.DaylightTransitionEnd.Month, adjustmentRule.DaylightTransitionEnd.Day, adjustmentRule.DaylightTransitionEnd.TimeOfDay.Hour, adjustmentRule.DaylightTransitionEnd.TimeOfDay.Minute, adjustmentRule.DaylightTransitionEnd.TimeOfDay.Second ).AddDays( 1 ) )
					};

				if ( tzinfoStandard.Start.LessThan( earliest ) )
				{
					tzinfoStandard.Start = tzinfoStandard.Start.AddYears( earliest.Year - tzinfoStandard.Start.Year );
				}

				tzinfoStandard.OffsetFrom = new UtcOffset( utcOffset + delta );
				tzinfoStandard.OffsetTo = new UtcOffset( utcOffset );

				PopulateiCalTimeZoneInfo( tzinfoStandard, adjustmentRule.DaylightTransitionEnd );

				// Add the "standard" time rule to the time zone
				tz.AddChild( tzinfoStandard );

				if ( tzinfo.SupportsDaylightSavingTime )
				{
					var tzinfoDaylight = new iCalTimeZoneInfo
						{
							Name = "DAYLIGHT",
							TimeZoneName = tzinfo.DaylightName,
							Start = new iCalDateTime( new DateTime( adjustmentRule.DateStart.Year, adjustmentRule.DaylightTransitionStart.Month, adjustmentRule.DaylightTransitionStart.Day, adjustmentRule.DaylightTransitionStart.TimeOfDay.Hour, adjustmentRule.DaylightTransitionStart.TimeOfDay.Minute, adjustmentRule.DaylightTransitionStart.TimeOfDay.Second ) )
						};

					if ( tzinfoDaylight.Start.LessThan( earliest ) )
					{
						tzinfoDaylight.Start = tzinfoDaylight.Start.AddYears( earliest.Year - tzinfoDaylight.Start.Year );
					}

					tzinfoDaylight.OffsetFrom = new UtcOffset( utcOffset );
					tzinfoDaylight.OffsetTo = new UtcOffset( utcOffset + delta );

					PopulateiCalTimeZoneInfo( tzinfoDaylight, adjustmentRule.DaylightTransitionStart );

					// Add the "daylight" time rule to the time zone
					tz.AddChild( tzinfoDaylight );
				}
			}

			// If no time zone information was recorded, at least
			// add a STANDARD time zone element to indicate the
			// base time zone information.
			if ( tz.TimeZoneInfos.Count == 0 )
			{
				var tzinfoStandard = new iCalTimeZoneInfo
					{
						Name = "STANDARD",
						TimeZoneName = tzinfo.StandardName,
						Start = earliest,
						OffsetFrom = new UtcOffset( utcOffset ),
						OffsetTo = new UtcOffset( utcOffset )
					};

				// Add the "standard" time rule to the time zone
				tz.AddChild( tzinfoStandard );
			}

			return tz;
		}

		/// <summary>
		///     Evaluator.
		/// </summary>
		private TimeZoneEvaluator _evaluator;

		/// <summary>
		///     TimeZone info
		/// </summary>
		private ICalendarObjectList<ITimeZoneInfo> _timeZoneInfos;

		/// <summary>
		///     Initializes a new instance of the <see cref="iCalTimeZone" /> class.
		/// </summary>
		public iCalTimeZone( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			Name = Components.TIMEZONE;

			_evaluator = new TimeZoneEvaluator( this );
			_timeZoneInfos = new CalendarObjectListProxy<ITimeZoneInfo>( Children );
			Children.ItemAdded += Children_ItemAdded;
			Children.ItemRemoved += Children_ItemRemoved;
			SetService( _evaluator );
		}

		/// <summary>
		///     Children_s the item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void Children_ItemRemoved( object sender, ObjectEventArgs<ICalendarObject, int> e )
		{
			_evaluator.Clear( );
		}

		/// <summary>
		///     Children_s the item added.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void Children_ItemAdded( object sender, ObjectEventArgs<ICalendarObject, int> e )
		{
			_evaluator.Clear( );
		}

		/// <summary>
		///     Called when [deserializing].
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserializing( StreamingContext context )
		{
			base.OnDeserializing( context );

			Initialize( );
		}

		#region ITimeZone Members

		/// <summary>
		///     Gets or sets the ID.
		/// </summary>
		/// <value>
		///     The ID.
		/// </value>
		public virtual string Id
		{
			get
			{
				return Properties.Get<string>( "TZID" );
			}
			set
			{
				Properties.Set( "TZID", value );
			}
		}

		/// <summary>
		///     Gets or sets the TZID.
		/// </summary>
		/// <value>
		///     The TZID.
		/// </value>
		public virtual string TzId
		{
			get
			{
				return Id;
			}
			set
			{
				Id = value;
			}
		}

		/// <summary>
		///     Gets or sets the last modified.
		/// </summary>
		/// <value>
		///     The last modified.
		/// </value>
		public virtual IDateTime LastModified
		{
			get
			{
				return Properties.Get<IDateTime>( "LAST-MODIFIED" );
			}
			set
			{
				Properties.Set( "LAST-MODIFIED", value );
			}
		}

		/// <summary>
		///     Gets or sets the URL.
		/// </summary>
		/// <value>
		///     The URL.
		/// </value>
		public virtual Uri Url
		{
			get
			{
				return Properties.Get<Uri>( "TZURL" );
			}
			set
			{
				Properties.Set( "TZURL", value );
			}
		}

		/// <summary>
		///     Gets or sets the TZ URL.
		/// </summary>
		/// <value>
		///     The TZ URL.
		/// </value>
		public virtual Uri TzUrl
		{
			get
			{
				return Url;
			}
			set
			{
				Url = value;
			}
		}

		/// <summary>
		///     Gets or sets the time zone info's.
		/// </summary>
		/// <value>
		///     The time zone info's.
		/// </value>
		public virtual ICalendarObjectList<ITimeZoneInfo> TimeZoneInfos
		{
			get
			{
				return _timeZoneInfos;
			}
			set
			{
				_timeZoneInfos = value;
			}
		}

		/// <summary>
		///     Retrieves the iCalTimeZoneInfo object that contains information
		///     about the TimeZone, with the name of the current time zone,
		///     offset from UTC, etc.
		/// </summary>
		/// <param name="dt">The iCalDateTime object for which to retrieve the iCalTimeZoneInfo.</param>
		/// <returns>A TimeZoneInfo object for the specified iCalDateTime</returns>
		public virtual TimeZoneObservance? GetTimeZoneObservance( IDateTime dt )
		{
			Trace.TraceInformation( "Getting time zone for '" + dt + "'...", "Time Zone" );

			foreach ( ITimeZoneInfo tzi in TimeZoneInfos )
			{
				TimeZoneObservance? observance = tzi.GetObservance( dt );
				if ( observance != null )
				{
					return observance;
				}
			}

			return null;
		}

		#endregion
	}
}