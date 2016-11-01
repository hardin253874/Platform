// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Iterator state.
	/// </summary>
	internal enum IteratorState
	{
		/// <summary>
		///     Iterator is disposed.
		/// </summary>
		Disposed = -1,

		/// <summary>
		///     Iterator is uninitialized.
		/// </summary>
		Uninitialized = 0,

		/// <summary>
		///     Iterator is initialized.
		/// </summary>
		Initialized = 1,

		/// <summary>
		///     Iterator is active.
		/// </summary>
		Active = 2,
	}
}