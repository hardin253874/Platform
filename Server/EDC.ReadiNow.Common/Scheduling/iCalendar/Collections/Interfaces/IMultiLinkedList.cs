// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     IMultiLinkedList interface
	/// </summary>
	/// <typeparam name="TType">The type of the type.</typeparam>
	public interface IMultiLinkedList<TType> : IList<TType>
	{
		/// <summary>
		///     Gets the exclusive end.
		/// </summary>
		/// <value>
		///     The exclusive end.
		/// </value>
		int ExclusiveEnd
		{
			get;
		}

		/// <summary>
		///     Gets the start index.
		/// </summary>
		/// <value>
		///     The start index.
		/// </value>
		int StartIndex
		{
			get;
		}

		/// <summary>
		///     Sets the next.
		/// </summary>
		/// <param name="next">The next.</param>
		void SetNext( IMultiLinkedList<TType> next );

		/// <summary>
		///     Sets the previous.
		/// </summary>
		/// <param name="previous">The previous.</param>
		void SetPrevious( IMultiLinkedList<TType> previous );
	}
}