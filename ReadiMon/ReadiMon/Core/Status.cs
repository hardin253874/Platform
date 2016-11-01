// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace ReadiMon.Core
{
	/// <summary>
	///     Status Text class.
	/// </summary>
	public sealed class Status : IDisposable
	{
		/// <summary>
		///     The stack of status text instances.
		/// </summary>
		private static readonly Stack<Status> Stack = new Stack<Status>( );

		/// <summary>
		///     Prevents a default instance of the <see cref="Status" /> class from being created.
		/// </summary>
		private Status( string text )
		{
			Text = text;
		}

		/// <summary>
		///     Gets or sets the text.
		/// </summary>
		/// <value>
		///     The text.
		/// </value>
		private string Text
		{
			get;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [revert to ready].
		/// </summary>
		/// <value>
		///   <c>true</c> if [revert to ready]; otherwise, <c>false</c>.
		/// </value>
		private bool RevertToReady
		{
			get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( Stack.Peek( ) != this )
			{
				throw new InvalidOperationException( "StatusText stack corruption" );
			}

			Stack.Pop( );

			if ( Stack.Count > 0 )
			{
				Status status = Stack.Peek( );

				status.OnStatusChanged( status.Text );
			}
			else
			{
				if ( RevertToReady )
				{
					OnStatusChanged( "Ready..." );

					System.Windows.Application.Current.Dispatcher.Invoke( ( ) =>
					{
					}, DispatcherPriority.Render );
				}
			}
		}

		/// <summary>
		///     Called when the status changes.
		/// </summary>
		/// <param name="e">The e.</param>
		private void OnStatusChanged( string e )
		{
			var handler = StatusChanged;

			handler?.Invoke( this, e );
		}

		/// <summary>
		/// Sets the specified text.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="revertToReady">if set to <c>true</c> [revert to ready].</param>
		/// <returns></returns>
		public static Status Set( string text, bool revertToReady = true )
		{
			var status = new Status( text )
			{
				RevertToReady = revertToReady
			};

			Stack.Push( status );

			status.OnStatusChanged( text );

			System.Windows.Application.Current.Dispatcher.Invoke( ( ) =>
			{
			}, DispatcherPriority.Render );

			return status;
		}

		/// <summary>
		///     Occurs when the status changes.
		/// </summary>
		public static event EventHandler<string> StatusChanged;
	}
}