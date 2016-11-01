// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     Database manager.
	/// </summary>
	public class DatabaseManager
	{
		/// <summary>
		///     The connection.
		/// </summary>
		private readonly Lazy<SqlConnection> _connection;

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseManager" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="token">The token.</param>
		/// <exception cref="System.ArgumentNullException">settings</exception>
		public DatabaseManager( IDatabaseSettings settings, CancellationToken token )
		{
			if ( settings == null )
			{
				throw new ArgumentNullException( nameof( settings ) );
			}

			Settings = settings;
			Token = token;

			_connection = new Lazy<SqlConnection>( GetConnection );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseManager" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <exception cref="System.ArgumentNullException">settings</exception>
		public DatabaseManager( IDatabaseSettings settings )
		{
			if ( settings == null )
			{
				throw new ArgumentNullException( nameof( settings ) );
			}

			Settings = settings;

			_connection = new Lazy<SqlConnection>( GetConnection );
		}

		/// <summary>
		/// Gets or sets the token.
		/// </summary>
		/// <value>
		/// The token.
		/// </value>
		private CancellationToken Token
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the connection.
		/// </summary>
		/// <value>
		///     The connection.
		/// </value>
		private SqlConnection Connection => _connection.Value;

		/// <summary>
		///     Gets or sets the settings.
		/// </summary>
		/// <value>
		///     The settings.
		/// </value>
		private IDatabaseSettings Settings
		{
			get;
		}

		/// <summary>
		///     Adds the parameter.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public SqlParameter AddParameter( SqlCommand command, string name, object value )
		{
			var parameter = new SqlParameter( name, value );

			command.Parameters.Add( parameter );

			return parameter;
		}

		/// <summary>
		/// Adds the parameter.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public SqlParameter AddParameter( SqlCommand command, string name, SqlDbType type )
		{
			var parameter = new SqlParameter( name, type );

			command.Parameters.Add( parameter );

			return parameter;
		}

		/// <summary>
		/// Adds the parameter.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public SqlParameter AddParameter( SqlCommand command, string name, SqlDbType type, object value )
		{
			var parameter = new SqlParameter( name, type );
			parameter.Value = value;

			command.Parameters.Add( parameter );

			return parameter;
		}

		/// <summary>
		///     Begins the transaction.
		/// </summary>
		/// <returns></returns>
		public SqlTransaction BeginTransaction( )
		{
			return Connection.BeginTransaction( );
		}

		/// <summary>
		/// Creates the command.
		/// </summary>
		/// <param name="commandText">The command text.</param>
		/// <param name="type">The type.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="transaction">The transaction.</param>
		/// <returns></returns>
		public SqlCommand CreateCommand( string commandText, CommandType type = CommandType.Text, int timeout = 30000, SqlTransaction transaction = null )
		{
			var command = Connection.CreateCommand( );

			command.CommandText = commandText;
			command.CommandType = type;
			command.CommandTimeout = timeout;

			if ( transaction != null )
			{
				command.Transaction = transaction;
			}

			return command;
		}
	
		/// <summary>
		///     Tests the connection.
		/// </summary>
		/// <returns></returns>
		public bool TestConnection( )
		{
			try
			{
				using ( SqlConnection connection = GetConnection( ) )
				{
					using ( IDbCommand command = connection.CreateCommand( ) )
					{
						command.CommandText = @"--ReadiMon - TestConnection
SELECT TOP 1 1 FROM Data_Int";
						command.CommandType = CommandType.Text;

						command.ExecuteScalar( );
					}
				}

				return true;
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc );
			}

			return false;
		}

		/// <summary>
		///     Gets the connection.
		/// </summary>
		/// <returns></returns>
		private SqlConnection GetConnection( )
		{
			var builder = new SqlConnectionStringBuilder
			{
				IntegratedSecurity = Settings.UseIntegratedSecurity,
				InitialCatalog = Settings.CatalogName,
				DataSource = Settings.ServerName,
				ApplicationName = "ReadiMon",
				ConnectTimeout = 30
			};

			if ( !Settings.UseIntegratedSecurity )
			{
				builder.UserID = Settings.Username;
				builder.Password = Settings.Password;
			}

			try
			{
				var connection = new SqlConnection( builder.ConnectionString );

				if ( Token != CancellationToken.None )
				{
					var task = connection.OpenAsync( Token );
					task.Wait( Token );
				}
				else
				{
					connection.Open( );
				}

				return connection;
			}
			catch ( OperationCanceledException )
			{
				return null;
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc.Message );

				throw;
			}
		}
	}
}