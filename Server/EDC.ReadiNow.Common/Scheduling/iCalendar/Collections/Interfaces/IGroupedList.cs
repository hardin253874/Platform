// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     IGroupedList interface.
	/// </summary>
	/// <typeparam name="TGroup">The type of the group.</typeparam>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	public interface IGroupedList<TGroup, TItem> : IGroupedCollection<TGroup, TItem>, IList<TItem>
		where TItem : class, IGroupedObject<TGroup>
	{
		/// <summary>
		///     Gets the object at the specified index.
		/// </summary>
		new TItem this[ int index ]
		{
			get;
		}

		/// <summary>
		///     Returns the index of the given item
		///     within the list, or -1 if the item
		///     is not found in the list.
		/// </summary>
		new int IndexOf( TItem obj );
	}
}