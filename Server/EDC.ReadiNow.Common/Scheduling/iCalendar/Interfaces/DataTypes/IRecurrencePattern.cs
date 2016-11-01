// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IRecurrencePattern interface.
	/// </summary>
	public interface IRecurrencePattern : IEncodableDataType
	{
		/// <summary>
		///     Gets or sets the by day.
		/// </summary>
		/// <value>
		///     The by day.
		/// </value>
		IList<IWeekDay> ByDay
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the by hour.
		/// </summary>
		/// <value>
		///     The by hour.
		/// </value>
		IList<int> ByHour
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the by minute.
		/// </summary>
		/// <value>
		///     The by minute.
		/// </value>
		IList<int> ByMinute
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the by month.
		/// </summary>
		/// <value>
		///     The by month.
		/// </value>
		IList<int> ByMonth
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the by month day.
		/// </summary>
		/// <value>
		///     The by month day.
		/// </value>
		IList<int> ByMonthDay
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the by second.
		/// </summary>
		/// <value>
		///     The by second.
		/// </value>
		IList<int> BySecond
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the by set position.
		/// </summary>
		/// <value>
		///     The by set position.
		/// </value>
		IList<int> BySetPosition
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the by week no.
		/// </summary>
		/// <value>
		///     The by week no.
		/// </value>
		IList<int> ByWeekNo
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the by year day.
		/// </summary>
		/// <value>
		///     The by year day.
		/// </value>
		IList<int> ByYearDay
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the count.
		/// </summary>
		/// <value>
		///     The count.
		/// </value>
		int Count
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the evaluation mode.
		/// </summary>
		/// <value>
		///     The evaluation mode.
		/// </value>
		RecurrenceEvaluationModeType EvaluationMode
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the first day of week.
		/// </summary>
		/// <value>
		///     The first day of week.
		/// </value>
		DayOfWeek FirstDayOfWeek
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the frequency.
		/// </summary>
		/// <value>
		///     The frequency.
		/// </value>
		FrequencyType Frequency
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the interval.
		/// </summary>
		/// <value>
		///     The interval.
		/// </value>
		int Interval
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type of the restriction.
		/// </summary>
		/// <value>
		///     The type of the restriction.
		/// </value>
		RecurrenceRestrictionType RestrictionType
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the until.
		/// </summary>
		/// <value>
		///     The until.
		/// </value>
		DateTime Until
		{
			get;
			set;
		}
	}
}