// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IICalendarCollection interface.
	/// </summary>
	public interface IICalendarCollection : IGetOccurrencesTyped, IList<IICalendar>
	{
	}
}