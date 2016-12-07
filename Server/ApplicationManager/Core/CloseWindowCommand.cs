// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows;
using System.Windows.Input;

namespace ApplicationManager.Core
{
	/// <summary>
	///     Close Window command.
	/// </summary>
	public class CloseWindowCommand : ICommand
	{
		/// <summary>
		/// </summary>
		public static readonly ICommand Instance = new CloseWindowCommand( );

		/// <summary>
		///     Prevents a default instance of the <see cref="CloseWindowCommand" /> class from being created.
		/// </summary>
		private CloseWindowCommand( )
		{
		}

		#region ICommand Members

		/// <summary>
		///     Occurs when [can execute changed].
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		///     Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">
		///     Data used by the command.  If the command does not require data to be passed, this object can
		///     be set to null.
		/// </param>
		/// <returns>
		///     true if this command can be executed; otherwise, false.
		/// </returns>
		public bool CanExecute( object parameter )
		{
			return parameter is Window;
		}

		/// <summary>
		///     Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		///     Data used by the command.  If the command does not require data to be passed, this object can
		///     be set to null.
		/// </param>
		public void Execute( object parameter )
		{
			if ( CanExecute( parameter ) )
			{
				( ( Window ) parameter ).Close( );
			}
		}

		/// <summary>
		///     Raises the can execute changed.
		/// </summary>
		public void RaiseCanExecuteChanged( )
		{
			EventHandler handler = CanExecuteChanged;

			handler?.Invoke( this, EventArgs.Empty );
		}

		#endregion ICommand Members
	}
}