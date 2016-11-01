// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarPropertyListContainer interface.
	/// </summary>
	public interface ICalendarPropertyListContainer : ICalendarObject
	{
		/// <summary>
		///     Gets the properties.
		/// </summary>
		/// <value>
		///     The properties.
		/// </value>
		ICalendarPropertyList Properties
		{
			get;
		}
	}
}