// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A collection of calendar objects.
	/// </summary>
	[Serializable]
	public class CalendarObjectList : GroupedList<string, ICalendarObject>, ICalendarObjectList<ICalendarObject>
	{
		/// <summary>
		///     Parent.
		/// </summary>
		private ICalendarObject _parent;

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarObjectList" /> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public CalendarObjectList( ICalendarObject parent )
		{
			_parent = parent;
		}
	}
}