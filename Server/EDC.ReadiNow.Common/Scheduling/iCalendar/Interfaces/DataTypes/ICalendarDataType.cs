// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICalendarDataType interface.
	/// </summary>
	public interface ICalendarDataType : ICalendarParameterCollectionContainer, ICopyable, IServiceProvider
	{
		/// <summary>
		///     Gets or sets the associated object.
		/// </summary>
		/// <value>
		///     The associated object.
		/// </value>
		ICalendarObject AssociatedObject
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the calendar.
		/// </summary>
		/// <value>
		///     The calendar.
		/// </value>
		IICalendar Calendar
		{
			get;
		}

		/// <summary>
		///     Gets or sets the language.
		/// </summary>
		/// <value>
		///     The language.
		/// </value>
		string Language
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the type of the value.
		/// </summary>
		/// <returns></returns>
		Type GetValueType( );

		/// <summary>
		///     Sets the type of the value.
		/// </summary>
		/// <param name="type">The type.</param>
		void SetValueType( string type );
	}
}