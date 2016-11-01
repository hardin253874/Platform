// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ICalendarComponentFactory interface.
	/// </summary>
	public interface ICalendarComponentFactory
	{
		/// <summary>
		///     Builds the specified object name.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="uninitialized">
		///     if set to <c>true</c> [uninitialized].
		/// </param>
		/// <returns></returns>
		ICalendarComponent Build( string objectName, bool uninitialized );
	}
}