// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarParameterCollection interface.
	/// </summary>
	public interface ICalendarParameterCollection : IGroupedList<string, ICalendarParameter>
	{
		/// <summary>
		///     Adds the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		void Add( string name, string value );

		/// <summary>
		///     Gets the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		string Get( string name );

		/// <summary>
		///     Gets the many.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		IList<string> GetMany( string name );

		/// <summary>
		///     Sets the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		void Set( string name, string value );

		/// <summary>
		///     Sets the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="values">The values.</param>
		void Set( string name, IEnumerable<string> values );

		/// <summary>
		///     Sets the parent.
		/// </summary>
		/// <param name="parent">The parent.</param>
		void SetParent( ICalendarObject parent );
	}
}