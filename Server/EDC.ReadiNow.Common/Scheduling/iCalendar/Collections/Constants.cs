// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{

	#region EventArgs

	/// <summary>
	///     ObjectEventArgs type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ObjectEventArgs<T> : EventArgs
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ObjectEventArgs{T}" /> class.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public ObjectEventArgs( T obj )
		{
			Object = obj;
		}

		/// <summary>
		///     Gets or sets the object.
		/// </summary>
		/// <value>
		///     The object.
		/// </value>
		public T Object
		{
			get;
			set;
		}
	}

	/// <summary>
	///     ObjectEventArgs type.
	/// </summary>
	/// <typeparam name="TFirst"></typeparam>
	/// <typeparam name="TSecond"></typeparam>
	public class ObjectEventArgs<TFirst, TSecond> :
		EventArgs
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ObjectEventArgs{TFirst, TSecond}" /> class.
		/// </summary>
		/// <param name="first">The first.</param>
		/// <param name="second">The second.</param>
		public ObjectEventArgs( TFirst first, TSecond second )
		{
			First = first;
			Second = second;
		}

		/// <summary>
		///     Gets or sets the first.
		/// </summary>
		/// <value>
		///     The first.
		/// </value>
		public TFirst First
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the second.
		/// </summary>
		/// <value>
		///     The second.
		/// </value>
		public TSecond Second
		{
			get;
			set;
		}
	}

	/// <summary>
	///     ValueChangedEventArgs type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ValueChangedEventArgs<T> :
		EventArgs
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ValueChangedEventArgs{T}" /> class.
		/// </summary>
		/// <param name="removedValues">The removed values.</param>
		/// <param name="addedValues">The added values.</param>
		public ValueChangedEventArgs( IEnumerable<T> removedValues, IEnumerable<T> addedValues )
		{
			RemovedValues = removedValues ?? new T[0];
			AddedValues = addedValues ?? new T[0];
		}

		/// <summary>
		///     Gets or sets the added values.
		/// </summary>
		/// <value>
		///     The added values.
		/// </value>
		public IEnumerable<T> AddedValues
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets or sets the removed values.
		/// </summary>
		/// <value>
		///     The removed values.
		/// </value>
		public IEnumerable<T> RemovedValues
		{
			get;
			protected set;
		}
	}

	#endregion
}