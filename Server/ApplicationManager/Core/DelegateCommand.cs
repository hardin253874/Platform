// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Windows.Input;

namespace ApplicationManager.Core
{
	/// <summary>
	///     DelegateCommand of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DelegateCommand< T > : ICommand
	{
		/// <summary>
		/// </summary>
		private readonly Predicate< T > _canExecute;

		/// <summary>
		/// </summary>
		private readonly Action< T > _execute;

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateCommand{T}" /> class.
		/// </summary>
		/// <param name="execute">The execute.</param>
		public DelegateCommand( Action< T > execute )
			: this( execute, x => true )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateCommand{T}" /> class.
		/// </summary>
		/// <param name="execute">The execute.</param>
		/// <param name="canExecute">The can execute.</param>
		/// <exception cref="System.ArgumentNullException">canExecute</exception>
		public DelegateCommand( Action< T > execute, Predicate< T > canExecute )
		{
			if ( canExecute == null )
			{
				throw new ArgumentNullException( "canExecute" );
			}
			if ( execute == null )
			{
				throw new ArgumentNullException( "execute" );
			}

			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		///     Occurs when [can execute changed].
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		///     Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>
		///     true if this command can be executed; otherwise, false.
		/// </returns>
		public bool CanExecute( object parameter )
		{
			return _canExecute( ( T ) parameter );
		}

		/// <summary>
		///     Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public void Execute( object parameter )
		{
			_execute( ( T ) parameter );
		}

		/// <summary>
		///     Raises the can execute changed.
		/// </summary>
		public void RaiseCanExecuteChanged( )
		{
			CanExecuteChanged( this, new EventArgs( ) );
		}
	}

	/// <summary>
	///     DelegateCommand.
	/// </summary>
	public class DelegateCommand : DelegateCommand< object >
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateCommand" /> class.
		/// </summary>
		/// <param name="execute">The execute.</param>
		public DelegateCommand( Action execute )
			: base( execute != null ? x => execute( ) : ( Action< object > ) null )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateCommand" /> class.
		/// </summary>
		/// <param name="execute">The execute.</param>
		/// <param name="canExecute">The can execute.</param>
		public DelegateCommand( Action execute, Func< bool > canExecute )
			: base( execute != null ? x => execute( ) : ( Action< object > ) null,
			        canExecute != null ? x => canExecute( ) : ( Predicate< object > ) null )
		{
		}
	}
}