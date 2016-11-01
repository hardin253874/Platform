// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarObject interface
	/// </summary>
	public interface ICalendarObject : IGroupedObject<string>, ILoadable, ICopyable, IServiceProvider
	{
		/// <summary>
		///     Returns the iCalendar that this object
		///     is associated with.
		/// </summary>
		IICalendar Calendar
		{
			get;
		}

		/// <summary>
		///     Returns a collection of children of this object.
		/// </summary>
		ICalendarObjectList<ICalendarObject> Children
		{
			get;
		}

		/// <summary>
		///     Returns the column number where this calendar
		///     object was found during parsing.
		/// </summary>
		int Column
		{
			get;
			set;
		}

		/// <summary>
		///     Returns the line number where this calendar
		///     object was found during parsing.
		/// </summary>
		int Line
		{
			get;
			set;
		}

		/// <summary>
		///     The name of the calendar object.
		///     Every calendar object can be assigned
		///     a name.
		/// </summary>
		string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Returns the parent of this object.
		/// </summary>
		ICalendarObject Parent
		{
			get;
			set;
		}

// ReSharper disable InconsistentNaming
		/// <summary>
		///     Gets the i calendar.
		/// </summary>
		/// <value>
		///     The i calendar.
		/// </value>
		IICalendar iCalendar
// ReSharper restore InconsistentNaming
		{
			get;
		}
	}
}