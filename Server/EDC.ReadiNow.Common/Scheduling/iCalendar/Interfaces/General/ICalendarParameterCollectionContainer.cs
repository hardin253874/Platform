// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarParameterCollectionContainer interface.
	/// </summary>
	public interface ICalendarParameterCollectionContainer
	{
		/// <summary>
		///     Gets the parameters.
		/// </summary>
		/// <value>
		///     The parameters.
		/// </value>
		ICalendarParameterCollection Parameters
		{
			get;
		}
	}
}