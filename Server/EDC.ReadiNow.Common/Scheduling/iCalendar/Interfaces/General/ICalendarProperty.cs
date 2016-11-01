// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarProperty interface.
	/// </summary>
	public interface ICalendarProperty : ICalendarParameterCollectionContainer, ICalendarObject, IValueObject<object>
	{
		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		object Value
		{
			get;
			set;
		}
	}
}