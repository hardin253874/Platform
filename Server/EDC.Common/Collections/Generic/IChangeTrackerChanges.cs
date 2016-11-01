// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     The changes that have been made to the change tracker.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IChangeTrackerChanges<out T>
	{
		/// <summary>
		///     Gets the added elements.
		/// </summary>
		IEnumerable<T> Added
		{
			get;
		}

		/// <summary>
		///     Gets the removed elements.
		/// </summary>
		IEnumerable<T> Collection
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this <see cref="IChangeTrackerChanges&lt;T&gt;" /> is flushed.
		/// </summary>
		/// <value>
		///     <c>True</c> to indicate that the tracker has been flushed prior to tracking new changes; otherwise, <c>False</c>.
		/// </value>
		bool Flushed
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the removed elements.
		/// </summary>
		IEnumerable<T> Removed
		{
			get;
		}
	}
}