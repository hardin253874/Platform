// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Collections.Generic
{
	/// <summary>
	///     Interface that allows access to a typed change tracker instance.
	/// </summary>
	/// <typeparam name="T">Type of change tracker to be accessed.</typeparam>
	public interface IChangeTrackerAccessor<T>
	{
		/// <summary>
		///     Gets the tracker.
		/// </summary>
		IChangeTracker<T> Tracker
		{
			get;
		}
	}
}