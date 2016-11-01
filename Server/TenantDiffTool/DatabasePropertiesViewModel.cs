// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Linq;
using System.Windows.Input;
using TenantDiffTool.Core;

namespace TenantDiffTool
{
	/// <summary>
	///     Database properties.
	/// </summary>
	public class DatabasePropertiesViewModel : ViewModelBase
	{
		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     Databases.
		/// </summary>
		private IList<string> _databases;

		/// <summary>
		///     Existing servers.
		/// </summary>
		private IList<string> _existingServers;

		/// <summary>
		///     Integrated security.
		/// </summary>
		private bool _integratedSecurity;

		/// <summary>
		///     Password.
		/// </summary>
		private string _password;

		/// <summary>
		///     Selected database.
		/// </summary>
		private string _selectedDatabase;

		/// <summary>
		///     Selected server text.
		/// </summary>
		private string _selectedServerText;

		/// <summary>
		///     Username.
		/// </summary>
		private string _username;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabasePropertiesViewModel" /> class.
		/// </summary>
		public DatabasePropertiesViewModel( )
		{
			OkCommand = new DelegateCommand( ( ) =>
			{
				DatabaseInfo = DatabaseContext.GetContext( SelectedServerText, SelectedDatabase, IntegratedSecurity, Username, Password );

				OkClicked = true;
				CloseWindow = true;
			} );

			CloseCommand = new DelegateCommand( ( ) =>
			{
				DatabaseInfo = null;

				OkClicked = false;
				CloseWindow = true;
			} );

			using ( new WaitCursor( ) )
			{
				LoadAvailableDatabaseServers( );
			}
		}

		public ICommand CloseCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the close window.
		/// </summary>
		/// <value>
		///     The close window.
		/// </value>
		public bool? CloseWindow
		{
			get
			{
				return _closeWindow;
			}
			set
			{
				if ( _closeWindow != value )
				{
					_closeWindow = value;
					RaisePropertyChanged( "CloseWindow" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the database info.
		/// </summary>
		/// <value>
		///     The database info.
		/// </value>
		public DatabaseContext DatabaseInfo
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the databases.
		/// </summary>
		/// <value>
		///     The databases.
		/// </value>
		public IList<string> Databases
		{
			get
			{
				return _databases;
			}
			set
			{
				if ( !Equals( _databases, value ) )
				{
					_databases = value;
					RaisePropertyChanged( "Databases" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the existing servers.
		/// </summary>
		/// <value>
		///     The existing servers.
		/// </value>
		public IList<string> ExistingServers
		{
			get
			{
				return _existingServers;
			}
			set
			{
				if ( !Equals( _existingServers, value ) )
				{
					_existingServers = value;
					RaisePropertyChanged( "ExistingServers" );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [integrated security].
		/// </summary>
		/// <value>
		///     <c>true</c> if [integrated security]; otherwise, <c>false</c>.
		/// </value>
		public bool IntegratedSecurity
		{
			get
			{
				return _integratedSecurity;
			}
			set
			{
				if ( _integratedSecurity != value )
				{
					_integratedSecurity = value;
					RaisePropertyChanged( "IntegratedSecurity" );

					LoadDatabases( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [ok clicked].
		/// </summary>
		/// <value>
		///     <c>true</c> if [ok clicked]; otherwise, <c>false</c>.
		/// </value>
		public bool OkClicked
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the ok command.
		/// </summary>
		/// <value>
		///     The ok command.
		/// </value>
		public ICommand OkCommand
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
			get
			{
				return _password;
			}
			set
			{
				if ( _password != value )
				{
					_password = value;
					RaisePropertyChanged( "Password" );

					LoadDatabases( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected database.
		/// </summary>
		/// <value>
		///     The selected database.
		/// </value>
		public string SelectedDatabase
		{
			get
			{
				return _selectedDatabase;
			}
			set
			{
				if ( _selectedDatabase != value )
				{
					_selectedDatabase = value;
					RaisePropertyChanged( "SelectedDatabase" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected server text.
		/// </summary>
		/// <value>
		///     The selected server text.
		/// </value>
		public string SelectedServerText
		{
			get
			{
				return _selectedServerText;
			}
			set
			{
				if ( _selectedServerText != value )
				{
					_selectedServerText = value;
					RaisePropertyChanged( "SelectedServerText" );

					LoadDatabases( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the username.
		/// </summary>
		/// <value>
		///     The username.
		/// </value>
		public string Username
		{
			get
			{
				return _username;
			}
			set
			{
				if ( _username != value )
				{
					_username = value;
					RaisePropertyChanged( "Username" );

					LoadDatabases( );
				}
			}
		}

		/// <summary>
		///     Sets the existing server.
		/// </summary>
		/// <param name="dbInfo">The db info.</param>
		public void SetExistingServer( DatabaseContext dbInfo )
		{
			DatabaseInfo = dbInfo;
			IntegratedSecurity = dbInfo.IntegratedSecurity;
			Username = dbInfo.Username;
			Password = dbInfo.Password;
			SelectedServerText = dbInfo.Server;
		}

		/// <summary>
		///     Loads the available database servers.
		/// </summary>
		private void LoadAvailableDatabaseServers( )
		{
			DataTable dataSources = SqlDataSourceEnumerator.Instance.GetDataSources( );

			ExistingServers = ( from DataRow row in dataSources.Rows
				select ( string ) row.ItemArray[ 0 ] ).ToList( );
		}

		/// <summary>
		///     Loads the databases.
		/// </summary>
		private void LoadDatabases( )
		{
			Databases = null;

			if ( string.IsNullOrEmpty( SelectedServerText ) )
			{
				return;
			}

			using ( new BackgroundCursor( ) )
			{
				try
				{
					if ( !IntegratedSecurity && string.IsNullOrEmpty( Username ) )
					{
						return;
					}

					using ( DatabaseContext ctx = DatabaseContext.GetContext( SelectedServerText, "master", IntegratedSecurity, Username, Password ) )
					{
						using ( IDbCommand command = ctx.CreateCommand( "SELECT name FROM master..sysdatabases ORDER BY name" ) )
						{
							using ( IDataReader reader = command.ExecuteReader( ) )
							{
								var databases = new List<string>( );

								while ( reader.Read( ) )
								{
									databases.Add( reader.GetString( 0 ) );
								}

								Databases = databases;
							}
						}
					}
				}
					// ReSharper disable EmptyGeneralCatchClause
				catch ( Exception )
					// ReSharper restore EmptyGeneralCatchClause
				{
				}
			}
		}
	}
}