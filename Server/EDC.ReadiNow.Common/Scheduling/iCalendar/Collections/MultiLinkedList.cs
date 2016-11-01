// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     MultiLinkedList type.
	/// </summary>
	/// <typeparam name="TType">The type of the type.</typeparam>
	public class MultiLinkedList<TType> : List<TType>, IMultiLinkedList<TType>
	{
		/// <summary>
		///     Next list.
		/// </summary>
// ReSharper disable NotAccessedField.Local
		private IMultiLinkedList<TType> _next;

// ReSharper restore NotAccessedField.Local

		/// <summary>
		///     Previous list.
		/// </summary>
		private IMultiLinkedList<TType> _previous;

		#region IMultiLinkedList<TType> Members

		/// <summary>
		///     Sets the previous.
		/// </summary>
		/// <param name="previous">The previous.</param>
		public virtual void SetPrevious( IMultiLinkedList<TType> previous )
		{
			_previous = previous;
		}

		/// <summary>
		///     Sets the next.
		/// </summary>
		/// <param name="next">The next.</param>
		public virtual void SetNext( IMultiLinkedList<TType> next )
		{
			_next = next;
		}

		/// <summary>
		///     Gets the start index.
		/// </summary>
		/// <value>
		///     The start index.
		/// </value>
		public virtual int StartIndex
		{
			get
			{
				return _previous != null ? _previous.ExclusiveEnd : 0;
			}
		}

		/// <summary>
		///     Gets the exclusive end.
		/// </summary>
		/// <value>
		///     The exclusive end.
		/// </value>
		public virtual int ExclusiveEnd
		{
			get
			{
				return Count > 0 ? StartIndex + Count : StartIndex;
			}
		}

		#endregion
	}
}