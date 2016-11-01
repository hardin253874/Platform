// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Data;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     Null command
	/// </summary>
	public class NullCommand : IDbCommand
	{
		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
		}

		/// <summary>
		///     Creates a prepared (or compiled) version of the command on the data source.
		/// </summary>
		public void Prepare( )
		{
		}

		/// <summary>
		///     Attempts to cancels the execution of an <see cref="T:System.Data.IDbCommand" />.
		/// </summary>
		public void Cancel( )
		{
		}

		/// <summary>
		///     Creates a new instance of an <see cref="T:System.Data.IDbDataParameter" /> object.
		/// </summary>
		/// <returns>
		///     An IDbDataParameter object.
		/// </returns>
		public IDbDataParameter CreateParameter( )
		{
			return null;
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
			return 0;
		}

		/// <summary>
		///     Executes the <see cref="P:System.Data.IDbCommand.CommandText" /> against the
		///     <see
		///         cref="P:System.Data.IDbCommand.Connection" />
		///     and builds an <see cref="T:System.Data.IDataReader" />.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Data.IDataReader" /> object.
		/// </returns>
		public IDataReader ExecuteReader( )
		{
			return null;
		}

		/// <summary>
		///     Executes the <see cref="P:System.Data.IDbCommand.CommandText" /> against the
		///     <see
		///         cref="P:System.Data.IDbCommand.Connection" />
		///     , and builds an <see cref="T:System.Data.IDataReader" /> using one of the
		///     <see
		///         cref="T:System.Data.CommandBehavior" />
		///     values.
		/// </summary>
		/// <param name="behavior">
		///     One of the <see cref="T:System.Data.CommandBehavior" /> values.
		/// </param>
		/// <returns>
		///     An <see cref="T:System.Data.IDataReader" /> object.
		/// </returns>
		public IDataReader ExecuteReader( CommandBehavior behavior )
		{
			return null;
		}

		/// <summary>
		///     Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra
		///     columns or rows are ignored.
		/// </summary>
		/// <returns>
		///     The first column of the first row in the resultset.
		/// </returns>
		public object ExecuteScalar( )
		{
			return null;
		}

		/// <summary>
		///     Gets or sets the <see cref="T:System.Data.IDbConnection" /> used by this instance of the
		///     <see
		///         cref="T:System.Data.IDbCommand" />
		///     .
		/// </summary>
		/// <returns>The connection to the data source.</returns>
		public IDbConnection Connection
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
		/// </summary>
		/// <returns>the Command object of a .NET Framework data provider executes. The default value is null.</returns>
		public IDbTransaction Transaction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the text command to run against the data source.
		/// </summary>
		/// <returns>The text command to execute. The default value is an empty string ("").</returns>
		public string CommandText
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
		/// </summary>
		/// <returns>The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</returns>
		public int CommandTimeout
		{
			get;
			set;
		}

		/// <summary>
		///     Indicates or specifies how the <see cref="P:System.Data.IDbCommand.CommandText" /> property is interpreted.
		/// </summary>
		/// <returns>
		///     One of the <see cref="T:System.Data.CommandType" /> values. The default is Text.
		/// </returns>
		public CommandType CommandType
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the <see cref="T:System.Data.IDataParameterCollection" />.
		/// </summary>
		/// <returns>The parameters of the SQL statement or stored procedure.</returns>
		public IDataParameterCollection Parameters
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow" /> when used by the
		///     <see
		///         cref="M:System.Data.IDataAdapter.Update(System.Data.DataSet)" />
		///     method of a
		///     <see
		///         cref="T:System.Data.Common.DbDataAdapter" />
		///     .
		/// </summary>
		/// <returns>
		///     One of the <see cref="T:System.Data.UpdateRowSource" /> values. The default is Both unless the command is
		///     automatically generated. Then the default is None.
		/// </returns>
		public UpdateRowSource UpdatedRowSource
		{
			get;
			set;
		}
	}
}