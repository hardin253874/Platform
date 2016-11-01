// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.Data.SqlClient;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     Database Information.
	/// </summary>
	public class DatabaseContext : IDisposable
	{
		/// <summary>
		///     Database connection.
		/// </summary>
		private IDbConnection _connection;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseContext" /> class.
		/// </summary>
		private DatabaseContext( )
		{
			Server = "localhost";
			IntegratedSecurity = true;
			Catalogue = "SoftwarePlatform";
		}

		/// <summary>
		///     Gets or sets the catalogue.
		/// </summary>
		/// <value>
		///     The catalogue.
		/// </value>
		public string Catalogue
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [integrated security].
		/// </summary>
		/// <value>
		///     <c>true</c> if [integrated security]; otherwise, <c>false</c>.
		/// </value>
		public bool IntegratedSecurity
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the password.
		/// </summary>
		/// <value>
		///     The password.
		/// </value>
		public string Password
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the server.
		/// </summary>
		/// <value>
		///     The server.
		/// </value>
		public string Server
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the username.
		/// </summary>
		/// <value>
		///     The username.
		/// </value>
		public string Username
		{
			get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( _connection != null )
			{
				_connection.Dispose( );
				_connection = null;
			}
		}

		/// <summary>
		///     Gets the context.
		/// </summary>
		/// <returns></returns>
		public static DatabaseContext GetContext( )
		{
			var ctx = new DatabaseContext( );

			ctx.OpenConnection( );

			return ctx;
		}

		/// <summary>
		///     Gets the context.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="catalogue">The catalogue.</param>
		/// <param name="integratedSecurity">
		///     if set to <c>true</c> [integrated security].
		/// </param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public static DatabaseContext GetContext( string server, string catalogue, bool integratedSecurity, string username, string password )
		{
			var ctx = new DatabaseContext
			{
				Server = server,
				Catalogue = catalogue,
				IntegratedSecurity = integratedSecurity
			};

			if ( !string.IsNullOrEmpty( username ) )
			{
				ctx.Username = username;
			}

			if ( !string.IsNullOrEmpty( password ) )
			{
				ctx.Password = password;
			}

			ctx.OpenConnection( );

			return ctx;
		}

		/// <summary>
		///     Adds the parameter.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="name">Name of the parameter.</param>
		/// <param name="type">The type.</param>
		/// <param name="value">The value.</param>
		public IDbDataParameter AddParameter( IDbCommand command, string name, DbType type, object value )
		{
			if ( command == null )
			{
				return null;
			}

			IDbDataParameter parameter = command.CreateParameter( );

			parameter.DbType = type;
			parameter.ParameterName = name;

			if ( value != null )
			{
				parameter.Value = value;
			}

			/////
			// Associate the parameter with the command object
			/////
			command.Parameters.Add( parameter );

			return parameter;
		}

		/// <summary>
		///     Creates the command.
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateCommand( )
		{
			return CreateCommand( null );
		}

		/// <summary>
		///     Creates the command.
		/// </summary>
		/// <param name="commandText">The command text.</param>
		/// <returns></returns>
		public IDbCommand CreateCommand( string commandText )
		{
			IDbCommand command;

			if ( _connection == null || _connection.State != ConnectionState.Open )
			{
				command = new NullCommand( );
			}
			else
			{
				command = _connection.CreateCommand( );
				command.CommandType = CommandType.Text;

				if ( commandText != null )
				{
					command.CommandText = commandText;
				}
			}

			return command;
		}

		/// <summary>
		///     Gets the connection string.
		/// </summary>
		/// <returns></returns>
		private string GetConnectionString( )
		{
			if ( string.IsNullOrEmpty( Server ) )
			{
				Server = "localhost";
			}

			var builder = new SqlConnectionStringBuilder
			{
				ApplicationName = "TenantDiffTool",
				ConnectTimeout = 5,
				DataSource = Server,
				InitialCatalog = Catalogue,
				IntegratedSecurity = IntegratedSecurity
			};

			if ( !IntegratedSecurity )
			{
				if ( string.IsNullOrEmpty( Username ) || string.IsNullOrEmpty( Password ) )
				{
					return null;
				}

				builder.UserID = Username;
				builder.Password = Password;
			}

			return builder.ConnectionString;
		}

		/// <summary>
		///     Opens the connection.
		/// </summary>
		private void OpenConnection( )
		{
			try
			{
				string connectionString = GetConnectionString( );

				if ( connectionString == null )
				{
					throw new InvalidOperationException( "Invalid connection string" );
				}

				var connection = new SqlConnection( connectionString );
				connection.Open( );

				_connection = connection;
			}
// ReSharper disable EmptyGeneralCatchClause
			catch ( Exception )
// ReSharper restore EmptyGeneralCatchClause
			{
			}
		}
	}
}