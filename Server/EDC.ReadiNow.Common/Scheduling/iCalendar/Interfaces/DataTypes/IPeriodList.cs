// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IPeriodList interface.
	/// </summary>
	public interface IPeriodList : IEncodableDataType, IList<IPeriod>
	{
		/// <summary>
		///     Gets or sets the <see cref="IPeriod" /> at the specified index.
		/// </summary>
		/// <value>
		///     The <see cref="IPeriod" />.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		new IPeriod this[ int index ]
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the TZID.
		/// </summary>
		/// <value>
		///     The TZID.
		/// </value>
		string TzId
		{
			get;
			set;
		}

		/// <summary>
		///     Adds the specified date time.
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		void Add( IDateTime dateTime );

		/// <summary>
		///     Removes the specified date time.
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		void Remove( IDateTime dateTime );
	}
}