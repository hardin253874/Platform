// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;

namespace EDC.Database
{
	/// <summary>
	///     Database command object.
	/// </summary>
	public class DatabaseCommand : IDbCommand
	{
        /// <summary>
		///     The inner command.
		/// </summary>
		private readonly IDbCommand _command;

        /// <summary>
        ///     A callback action that may be called when a command is executed.
        /// </summary>
        private readonly Action<IDbCommand, Action> _callbackAction;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseCommand" /> class.
		/// </summary>
        /// <param name="command">The command.</param>
        /// <param name="callbackAction">An action that gets called on Execute. It must call the passed action.</param>
		/// <exception cref="System.ArgumentNullException">command</exception>
        public DatabaseCommand( IDbCommand command, Action<IDbCommand, Action> callbackAction )
		{
			if ( command == null )
			{
				throw new ArgumentNullException( nameof( command ) );
			}

            _callbackAction = callbackAction;
			_command = command;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			_command.Dispose( );
		}

		/// <summary>
		///     Creates a prepared (or compiled) version of the command on the data source.
		/// </summary>
		public void Prepare( )
		{
			_command.Prepare( );
		}

		/// <summary>
		///     Attempts to cancels the execution of an <see cref="T:System.Data.IDbCommand" />.
		/// </summary>
		public void Cancel( )
		{
			_command.Cancel( );
		}

		/// <summary>
		///     Creates a new instance of an <see cref="T:System.Data.IDbDataParameter" /> object.
		/// </summary>
		/// <returns>
		///     An IDbDataParameter object.
		/// </returns>
		public IDbDataParameter CreateParameter( )
		{
			return _command.CreateParameter( );
		}

		/// <summary>
		///     Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number
		///     of rows affected.
		/// </summary>
		/// <returns>
		///     The number of rows affected.
		/// </returns>
		public int ExecuteNonQuery( )
		{
            return RunCommand( _command.ExecuteNonQuery );
		}

		/// <summary>
		///     Executes the <see cref="P:System.Data.IDbCommand.CommandText" /> against the
		///     <see cref="P:System.Data.IDbCommand.Connection" /> and builds an <see cref="T:System.Data.IDataReader" />.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Data.IDataReader" /> object.
		/// </returns>
		public IDataReader ExecuteReader( )
		{
            return RunCommand( _command.ExecuteReader );
		}

		/// <summary>
		///     Executes the <see cref="P:System.Data.IDbCommand.CommandText" /> against the
		///     <see cref="P:System.Data.IDbCommand.Connection" />, and builds an <see cref="T:System.Data.IDataReader" /> using
		///     one of the <see cref="T:System.Data.CommandBehavior" /> values.
		/// </summary>
		/// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values.</param>
		/// <returns>
		///     An <see cref="T:System.Data.IDataReader" /> object.
		/// </returns>
		public IDataReader ExecuteReader( CommandBehavior behavior )
		{
            return RunCommand( ( ) => _command.ExecuteReader( behavior ) );
		}

		/// <summary>
		///     Executes the query, and returns the first column of the first row in the result set returned by the query. Extra
		///     columns or rows are ignored.
		/// </summary>
		/// <returns>
		///     The first column of the first row in the result set.
		/// </returns>
		public object ExecuteScalar( )
		{
            return RunCommand( _command.ExecuteScalar );
		}

		/// <summary>
		///     Gets or sets the <see cref="T:System.Data.IDbConnection" /> used by this instance of the
		///     <see cref="T:System.Data.IDbCommand" />.
		/// </summary>
		public IDbConnection Connection
		{
			get
			{
				return _command.Connection;
			}
			set
			{
				_command.Connection = value;
			}
		}

		/// <summary>
		///     Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
		/// </summary>
		public IDbTransaction Transaction
		{
			get
			{
				return _command.Transaction;
			}
			set
			{
				_command.Transaction = value;
			}
		}

		/// <summary>
		///     Gets or sets the text command to run against the data source.
		/// </summary>
		public string CommandText
		{
			get
			{
				return _command.CommandText;
			}
			set
			{
				_command.CommandText = value;
			}
		}

		/// <summary>
		///     Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
		/// </summary>
		public int CommandTimeout
		{
			get
			{
				return _command.CommandTimeout;
			}
			set
			{
				_command.CommandTimeout = value;
			}
		}

		/// <summary>
		///     Indicates or specifies how the <see cref="P:System.Data.IDbCommand.CommandText" /> property is interpreted.
		/// </summary>
		public CommandType CommandType
		{
			get
			{
				return _command.CommandType;
			}
			set
			{
				_command.CommandType = value;
			}
		}

		/// <summary>
		///     Gets the <see cref="T:System.Data.IDataParameterCollection" />.
		/// </summary>
		public IDataParameterCollection Parameters
		{
			get
			{
				return _command.Parameters;
			}
		}

		/// <summary>
		///     Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow" /> when used by the
		///     <see cref="M:System.Data.IDataAdapter.Update(System.Data.DataSet)" /> method of a
		///     <see cref="T:System.Data.Common.DbDataAdapter" />.
		/// </summary>
		public UpdateRowSource UpdatedRowSource
		{
			get
			{
				return _command.UpdatedRowSource;
			}
			set
			{
				_command.UpdatedRowSource = value;
			}
		}

		/// <summary>
		///     Invokes the database command.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action">The action.</param>
		/// <returns></returns>
        private T RunCommand<T>( Func<T> action )
        {
            if ( _callbackAction == null )
                return action( );

            T result = default( T );
            bool wasCalled = false;

            _callbackAction( _command, ( ) =>
                {
                    wasCalled = true;
                    result = action( );
                } );

            if ( !wasCalled )
                throw new InvalidOperationException( "DatabaseCommand callbackAction did not invoke the passed callback." );

            return result;
        }
	}
}