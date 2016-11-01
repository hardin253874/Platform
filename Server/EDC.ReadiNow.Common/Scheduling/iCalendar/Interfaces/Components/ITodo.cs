// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ITodo interface.
	/// </summary>
	public interface ITodo :
		IRecurringComponent
	{
		/// <summary>
		///     The date/time the to-do was completed.
		/// </summary>
		/// <value>
		///     The completed.
		/// </value>
		IDateTime Completed
		{
			get;
			set;
		}

		/// <summary>
		///     The due date of the to-do item.
		/// </summary>
		/// <value>
		///     The due.
		/// </value>
		IDateTime Due
		{
			get;
			set;
		}

		/// <summary>
		///     The duration of the to-do item.
		/// </summary>
		// NOTE: Duration is not supported by all systems,
		// (i.e. iPhone 1st gen) and cannot co-exist with Due.
		// RFC 5545 states:
		//
		//      ; either 'due' or 'duration' may appear in
		//      ; a 'todoprop', but 'due' and 'duration'
		//      ; MUST NOT occur in the same 'todoprop'
		//
		// Therefore, Duration should not be serialized, as 
		// it can be extrapolated from the Due date.
		TimeSpan Duration
		{
			get;
			set;
		}

		/// <summary>
		///     The geographic location (lat/long) of the to-do item.
		/// </summary>
		/// <value>
		///     The geographic location.
		/// </value>
		IGeographicLocation GeographicLocation
		{
			get;
			set;
		}

		/// <summary>
		///     The location of the to-do item.
		/// </summary>
		/// <value>
		///     The location.
		/// </value>
		string Location
		{
			get;
			set;
		}

		/// <summary>
		///     A number between 0 and 100 that represents
		///     the percentage of completion of this item.
		/// </summary>
		/// <value>
		///     The percent complete.
		/// </value>
		int PercentComplete
		{
			get;
			set;
		}

		/// <summary>
		///     A list of resources associated with this to-do item.
		/// </summary>
		/// <value>
		///     The resources.
		/// </value>
		IList<string> Resources
		{
			get;
			set;
		}

		/// <summary>
		///     The current status of the to-do item.
		/// </summary>
		TodoStatus Status
		{
			get;
			set;
		}

		/// <summary>
		///     Returns 'True' if the to-do item is Active as of <paramref name="currentDateTime" />.
		///     An item is Active if it requires action of some sort.
		/// </summary>
		/// <param name="currentDateTime">The date and time to test.</param>
		/// <returns>
		///     True if the item is Active as of <paramref name="currentDateTime" />, False otherwise.
		/// </returns>
		bool IsActive( IDateTime currentDateTime );

		/// <summary>
		///     Returns True if the to-do item was cancelled.
		/// </summary>
		/// <returns>
		///     True if the to-do was cancelled, False otherwise.
		/// </returns>
		bool IsCancelled( );

		/// <summary>
		///     Use this method to determine if a to-do item has been completed.
		///     This takes into account recurrence items and the previous date
		///     of completion, if any.
		///     <note>
		///         This method evaluates the recurrence pattern for this TO-DO
		///         as necessary to ensure all relevant information is taken
		///         into account to give the most accurate result possible.
		///     </note>
		/// </summary>
		/// <param name="currentDateTime">The date and time to test.</param>
		/// <returns>
		///     True if the to-do item has been completed
		/// </returns>
		bool IsCompleted( IDateTime currentDateTime );
	}
}