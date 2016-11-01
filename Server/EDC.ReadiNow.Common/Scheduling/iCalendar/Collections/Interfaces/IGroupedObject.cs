// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     IGroupedObject interface.
	/// </summary>
	/// <typeparam name="TGroup">The type of the group.</typeparam>
	public interface IGroupedObject<TGroup>
	{
		/// <summary>
		///     Gets or sets the group.
		/// </summary>
		/// <value>
		///     The group.
		/// </value>
		TGroup Group
		{
			get;
			set;
		}

		/// <summary>
		///     Occurs when a group changes.
		/// </summary>
		event EventHandler<ObjectEventArgs<TGroup, TGroup>> GroupChanged;
	}
}