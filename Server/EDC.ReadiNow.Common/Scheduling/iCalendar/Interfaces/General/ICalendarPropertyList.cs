// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarPropertyList interface.
	/// </summary>
	public interface ICalendarPropertyList : IGroupedValueList<string, ICalendarProperty, object>
	{
		/// <summary>
		///     Gets the <see cref="ICalendarProperty" /> with the specified name.
		/// </summary>
		/// <value>
		///     The <see cref="ICalendarProperty" />.
		/// </value>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		ICalendarProperty this[ string name ]
		{
			get;
		}
	}
}