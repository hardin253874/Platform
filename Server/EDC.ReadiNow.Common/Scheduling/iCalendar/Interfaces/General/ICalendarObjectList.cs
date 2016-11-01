// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarObjectList interface
	/// </summary>
	/// <typeparam name="TType">The type of the type.</typeparam>
	public interface ICalendarObjectList<TType> : IGroupedCollection<string, TType>
		where TType : class, ICalendarObject
	{
		/// <summary>
		///     Gets the <see cref="TType" /> at the specified index.
		/// </summary>
		/// <value>
		///     The <see cref="TType" />.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		TType this[ int index ]
		{
			get;
		}
	}
}