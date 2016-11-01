// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IRecurrable interface.
	/// </summary>
	public interface IRecurrable : IGetOccurrences, IServiceProvider
	{
		/// <summary>
		///     Gets or sets the date time start.
		/// </summary>
		/// <value>
		///     The date time start.
		/// </value>
		[Obsolete( "Use the Start property instead." )]
		IDateTime DtStart
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the exception dates.
		/// </summary>
		/// <value>
		///     The exception dates.
		/// </value>
		IList<IPeriodList> ExceptionDates
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the exception rules.
		/// </summary>
		/// <value>
		///     The exception rules.
		/// </value>
		IList<IRecurrencePattern> ExceptionRules
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the recurrence dates.
		/// </summary>
		/// <value>
		///     The recurrence dates.
		/// </value>
		IList<IPeriodList> RecurrenceDates
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the recurrence id.
		/// </summary>
		/// <value>
		///     The recurrence id.
		/// </value>
		IDateTime RecurrenceId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the recurrence rules.
		/// </summary>
		/// <value>
		///     The recurrence rules.
		/// </value>
		IList<IRecurrencePattern> RecurrenceRules
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets the start date/time of the component.
		/// </summary>
		/// <value>
		///     The start.
		/// </value>
		IDateTime Start
		{
			get;
			set;
		}
	}
}