// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Data.SqlClient;
using EDC.ReadiNow.Database;

namespace EDC.SoftwarePlatform.Migration.Storage
{
	/// <summary>
	///     Base class for any SQL-based sources or targets.
	/// </summary>
	internal class SqlBase : IDisposable
	{
		/// <summary>
		///     Creates a new database context.
		/// </summary>
		protected SqlBase( )
			: this( false, null )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SqlBase" /> class.
		/// </summary>
		/// <param name="requireTransaction">
		///     if set to <c>true</c> [require transaction].
		/// </param>
		protected SqlBase( bool requireTransaction )
			: this( requireTransaction, null )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SqlBase" /> class.
		/// </summary>
		/// <param name="requireTransaction">
		///     if set to <c>true</c> [require transaction].
		/// </param>
		/// <param name="timeout">The timeout.</param>
		protected SqlBase( bool requireTransaction, int timeout )
			: this( requireTransaction, ( int? ) timeout )
		{
		}

		/// <summary>
		///     Prevents a default instance of the <see cref="SqlBase" /> class from being created.
		/// </summary>
		/// <param name="requireTransaction">
		///     if set to <c>true</c> [require transaction].
		/// </param>
		/// <param name="timeout">The timeout.</param>
		private SqlBase( bool requireTransaction, int? timeout )
		{
            // Note that we have to create the connection immediately to prevent distributed transaction escalation. (see bug 26599)
			DatabaseContext = timeout == null ? DatabaseContext.GetContext( requireTransaction, createConnectionImmediately: true ) : DatabaseContext.GetContext( requireTransaction, commandTimeout: timeout.Value, transactionTimeout: timeout.Value, createConnectionImmediately: true );
		}

		/// <summary>
		///     Database to read the data from.
		/// </summary>
		protected DatabaseContext DatabaseContext
		{
			get;
			set;
		}

		/// <summary>
		///     Clean up
		/// </summary>
		public void Dispose( )
		{
			DatabaseContext.Dispose( );
		}

		/// <summary>
		///     Determines the data type of a data table.
		/// </summary>
		/// <param name="dataTableSuffix">Table suffix, such as NVarChar.</param>
		/// <returns></returns>
		public static Type DataTableType( string dataTableSuffix )
		{
			switch ( dataTableSuffix )
			{
				case "Alias":
				case "NVarChar":
				case "Xml":
					return typeof ( string );
				case "Bit":
					return typeof ( bool );
				case "DateTime":
					return typeof ( DateTime );
				case "Decimal":
					return typeof ( decimal );
				case "Int":
					return typeof ( Int32 );
				case "Guid":
					return typeof ( Guid );
				default:
					throw new Exception( "Unexpected data table: " + dataTableSuffix );
			}
		}

		/// <summary>
		///     Commits this instance.
		/// </summary>
		internal void Commit( )
		{
			DatabaseContext.CommitTransaction( );
		}

		/// <summary>
		///     Creates the command.
		/// </summary>
		/// <returns></returns>
		internal IDbCommand CreateCommand( )
		{
			return DatabaseContext.CreateCommand( );
		}

		/// <summary>
		///     Creates the connection.
		/// </summary>
		/// <returns></returns>
		internal SqlConnection CreateConnection( )
		{
			return DatabaseContext.GetConnection( );
		}
	}
}