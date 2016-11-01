// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IFreeBusy interface.
	/// </summary>
	public interface IFreeBusy : IUniqueComponent, IMergeable
	{
		/// <summary>
		///     Gets or sets the date time end.
		/// </summary>
		/// <value>
		///     The date time end.
		/// </value>
		IDateTime DtEnd
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the date time start.
		/// </summary>
		/// <value>
		///     The date time start.
		/// </value>
		IDateTime DtStart
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the end.
		/// </summary>
		/// <value>
		///     The end.
		/// </value>
		IDateTime End
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entries.
		/// </summary>
		/// <value>
		///     The entries.
		/// </value>
		IList<IFreeBusyEntry> Entries
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the start.
		/// </summary>
		/// <value>
		///     The start.
		/// </value>
		IDateTime Start
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the free busy status.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		FreeBusyStatus GetFreeBusyStatus( IPeriod period );

		/// <summary>
		///     Gets the free busy status.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <returns></returns>
		FreeBusyStatus GetFreeBusyStatus( IDateTime dt );
	}
}