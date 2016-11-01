// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IWeekDay interface.
	/// </summary>
	public interface IWeekDay : IEncodableDataType, IComparable
	{
		/// <summary>
		///     Gets or sets the day of week.
		/// </summary>
		/// <value>
		///     The day of week.
		/// </value>
		DayOfWeek DayOfWeek
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the offset.
		/// </summary>
		/// <value>
		///     The offset.
		/// </value>
		int Offset
		{
			get;
			set;
		}
	}
}