// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     IValueObject class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IValueObject<T>
	{
		/// <summary>
		///     Gets the value count.
		/// </summary>
		/// <value>
		///     The value count.
		/// </value>
		int ValueCount
		{
			get;
		}

		/// <summary>
		///     Gets the values.
		/// </summary>
		/// <value>
		///     The values.
		/// </value>
		IEnumerable<T> Values
		{
			get;
		}

		/// <summary>
		///     Adds the value.
		/// </summary>
		/// <param name="value">The value.</param>
		void AddValue( T value );

		/// <summary>
		///     Determines whether the specified value contains value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the specified value contains value; otherwise, <c>false</c>.
		/// </returns>
		bool ContainsValue( T value );

		/// <summary>
		///     Removes the value.
		/// </summary>
		/// <param name="value">The value.</param>
		void RemoveValue( T value );

		/// <summary>
		///     Sets the value.
		/// </summary>
		/// <param name="value">The value.</param>
		void SetValue( T value );

		/// <summary>
		///     Sets the value.
		/// </summary>
		/// <param name="values">The values.</param>
		void SetValue( IEnumerable<T> values );

		/// <summary>
		///     Occurs when the value changes.
		/// </summary>
		event EventHandler<ValueChangedEventArgs<T>> ValueChanged;
	}
}