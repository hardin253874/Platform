// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     IGroupedValueListProxy interface.
	/// </summary>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public interface IGroupedValueListProxy<out TItem, TValue> : IList<TValue>
	{
		/// <summary>
		///     Gets the items.
		/// </summary>
		/// <value>
		///     The items.
		/// </value>
		IEnumerable<TItem> Items
		{
			get;
		}
	}
}