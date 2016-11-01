// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     TimeZoneObservance structure.
	/// </summary>
	[Serializable]
	public struct TimeZoneObservance
	{
		/// <summary>
		///     Gets or sets the period.
		/// </summary>
		/// <value>
		///     The period.
		/// </value>
		public IPeriod Period
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the time zone info.
		/// </summary>
		/// <value>
		///     The time zone info.
		/// </value>
		public ITimeZoneInfo TimeZoneInfo
		{
			get;
			set;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="TimeZoneObservance" /> structure.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="tzi">The time zone info.</param>
		public TimeZoneObservance( IPeriod period, ITimeZoneInfo tzi )
			: this( )
		{
			Period = period;
			TimeZoneInfo = tzi;
		}

		/// <summary>
		///     Determines whether [contains] [the specified date time].
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified date time]; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains( IDateTime dt )
		{
			if ( Period != null )
			{
				return Period.Contains( dt );
			}
			return false;
		}
	}
}