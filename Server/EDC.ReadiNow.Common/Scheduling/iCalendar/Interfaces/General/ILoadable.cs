// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ILoadable interface.
	/// </summary>
	public interface ILoadable
	{
		/// <summary>
		///     Gets whether or not the object has been loaded.
		/// </summary>
		bool IsLoaded
		{
			get;
		}

		/// <summary>
		///     Fires the Loaded event.
		/// </summary>
		void OnLoaded( );

		/// <summary>
		///     An event that fires when the object has been loaded.
		/// </summary>
		event EventHandler Loaded;
	}
}