// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarParameter interface.
	/// </summary>
	public interface ICalendarParameter : ICalendarObject, IValueObject<string>
	{
		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		string Value
		{
			get;
			set;
		}
	}
}