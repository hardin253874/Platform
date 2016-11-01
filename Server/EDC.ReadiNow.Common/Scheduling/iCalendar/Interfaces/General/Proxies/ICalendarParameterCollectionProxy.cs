// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarParameterCollectionProxy interface.
	/// </summary>
	public interface ICalendarParameterCollectionProxy : ICalendarParameterCollection, IGroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>
	{
	}
}