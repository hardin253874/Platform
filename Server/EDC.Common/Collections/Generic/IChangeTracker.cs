// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.ComponentModel;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     IChangeTracker interface.
	/// </summary>
	/// <typeparam name="T">Type of element being tracked.</typeparam>
	public interface IChangeTracker<T> : ICollection<T>, IChangeTracking, IChangeTrackerChanges<T>
	{
	}
}