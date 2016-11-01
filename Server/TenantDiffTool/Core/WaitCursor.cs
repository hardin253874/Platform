// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows.Input;

namespace TenantDiffTool.Core
{
	public class WaitCursor : IDisposable
	{
		/// <summary>
		///     Previous cursor
		/// </summary>
		private readonly Cursor _previousCursor;

		/// <summary>
		///     Initializes a new instance of the <see cref="WaitCursor" /> class.
		/// </summary>
		public WaitCursor( )
		{
			_previousCursor = Mouse.OverrideCursor;

			Mouse.OverrideCursor = Cursors.Wait;
		}

		#region IDisposable Members

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Mouse.OverrideCursor = _previousCursor;
		}

		#endregion
	}

	public class BackgroundCursor : IDisposable
	{
		/// <summary>
		///     Previous cursor
		/// </summary>
		private readonly Cursor _previousCursor;

		/// <summary>
		///     Initializes a new instance of the <see cref="WaitCursor" /> class.
		/// </summary>
		public BackgroundCursor( )
		{
			_previousCursor = Mouse.OverrideCursor;

			Mouse.OverrideCursor = Cursors.AppStarting;
		}

		#region IDisposable Members

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Mouse.OverrideCursor = _previousCursor;
		}

		#endregion
	}
}