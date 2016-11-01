// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IPeriod interface.
	/// </summary>
	public interface IPeriod : IEncodableDataType, IComparable<IPeriod>
	{
		/// <summary>
		///     Gets or sets the duration.
		/// </summary>
		/// <value>
		///     The duration.
		/// </value>
		TimeSpan Duration
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the end time.
		/// </summary>
		/// <value>
		///     The end time.
		/// </value>
		IDateTime EndTime
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [matches date only].
		/// </summary>
		/// <value>
		///     <c>true</c> if [matches date only]; otherwise, <c>false</c>.
		/// </value>
		bool MatchesDateOnly
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the start time.
		/// </summary>
		/// <value>
		///     The start time.
		/// </value>
		IDateTime StartTime
		{
			get;
			set;
		}

		/// <summary>
		///     Determines if there is a period collision.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		bool CollidesWith( IPeriod period );

		/// <summary>
		///     Determines whether [contains] [the specified date time].
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified date time]; otherwise, <c>false</c>.
		/// </returns>
		bool Contains( IDateTime dateTime );
	}
}