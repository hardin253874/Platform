// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     CalendarObjectListProxy class.
	/// </summary>
	/// <typeparam name="TType">The type of the type.</typeparam>
	public class CalendarObjectListProxy<TType> : GroupedCollectionProxy<string, ICalendarObject, TType>, ICalendarObjectList<TType>
		where TType : class, ICalendarObject
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarObjectListProxy{TType}" /> class.
		/// </summary>
		/// <param name="list">The list.</param>
		public CalendarObjectListProxy( IGroupedCollection<string, ICalendarObject> list )
			: base( list )
		{
		}

		/// <summary>
		///     Gets the <see cref="TType" /> at the specified index.
		/// </summary>
		/// <value>
		///     The <see cref="TType" />.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public virtual TType this[ int index ]
		{
			get
			{
				return this.Skip( index ).FirstOrDefault( );
			}
		}
	}
}