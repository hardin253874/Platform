// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows.Input;

namespace ReadiMon.Shared.Support
{
	/// <summary>
	///     Mous cursor class
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class MouseCursor : IDisposable
	{
		/// <summary>
		///     The previous cursor
		/// </summary>
		private readonly Cursor _previousCursor;

		/// <summary>
		///     Initializes a new instance of the <see cref="MouseCursor" /> class.
		/// </summary>
		/// <param name="cursor">The cursor.</param>
		public MouseCursor( Cursor cursor )
		{
			_previousCursor = Mouse.OverrideCursor;

			Mouse.OverrideCursor = cursor;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Mouse.OverrideCursor = _previousCursor;
		}

		/// <summary>
		///     Sets the specified cursor.
		/// </summary>
		/// <param name="cursor">The cursor.</param>
		/// <returns></returns>
		public static MouseCursor Set( Cursor cursor )
		{
			return new MouseCursor( cursor );
		}
	}
}