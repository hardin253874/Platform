// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReadiMon.Core;
using ReadiMon.Properties;
using ReadiMon.Shared.Core;

namespace ReadiMon
{
	/// <summary>
	///     Database properties.
	/// </summary>
	public class DatabasePropertiesViewModel : ViewModelBase
	{
		/// <summary>
		///     The scan constant.
		/// </summary>
		private const string ScanNetwork = "Scan network...";

		/// <summary>
		///     The scanning constant.
		/// </summary>
		private const string Scanning = "Scanning network...";

		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     Databases.
		/// </summary>
		private IList<string> _databases;

		/// <summary>
		///     The discovered servers
		/// </summary>
		private DataTable _discoveredServers;

		/// <summary>
		///     Existing servers.
		/// </summary>
		private IList<string> _existingServers;

		/// <summary>
		///     Integrated security.
		/// </summary>
		private bool _integratedSecurity = true;

		/// <summary>
		///     The OK enabled value.
		/// </summary>
		private bool _okEnabled;

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
		///     The selected server text item
		/// </summary>
		private string _selectedServerTextItem;

		/// <summary>
		///     The task
		/// </summary>
		private Task<IList<string>> _task;

		/// <summary>
		///     The token source
		/// </summary>
		private CancellationTokenSource _tokenSource;


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
				DatabaseInfo = new DatabaseSettings( SelectedServerText, SelectedDatabase, Username, Password, IntegratedSecurity );

				OkClicked = true;
				CloseWindow = true;
			} );

			CloseCommand = new DelegateCommand( ( ) =>
			{
				DatabaseInfo = null;

				OkClicked = false;
				CloseWindow = true;
			} );

			var server = Settings.Default.DatabaseServer;
			var catalog = Settings.Default.DatabaseCatalog;
			var useIntegratedSecurity = Settings.Default.DatabaseUseIntegratedSecurity;

			ExistingServers = new List<string>
			{
				server,
				ScanNetwork
			};

			SelectedServerText = server;
			SelectedDatabase = catalog;
			IntegratedSecurity = useIntegratedSecurity;

			if ( !useIntegratedSecurity )
			{
				var username = Settings.Default.DatabaseUsername;
				var password = Settings.Default.DatabasePassword;

				Username = username;
				Password = password;
			}
		}

		/// <summary>
		///     Gets or sets the close command.
		/// </summary>
		/// <value>
		///     The close command.
		/// </value>
		public ICommand CloseCommand
		{
			get;
			private set;
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
					SetProperty( ref _closeWindow, value );
				}
			}
		}

		/// <summary>
		///     Gets or sets the database info.
		/// </summary>
		/// <value>
		///     The database info.
		/// </value>
		public DatabaseSettings DatabaseInfo
		{
			get;
			private set;
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
			private set
			{
				SetProperty( ref _databases, value );

				if ( value != null && value.Count > 0 )
				{
                    if ( string.IsNullOrEmpty(SelectedDatabase) || !value.Contains( SelectedDatabase ) )
                    {
                        SelectedDatabase = value.Contains( "SoftwarePlatform" ) ? "SoftwarePlatform" : value [ 0 ];
                    }
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
			private set
			{
				SetProperty( ref _existingServers, value );
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
				SetProperty( ref _integratedSecurity, value );

				LoadDatabases( );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether OK has been clicked.
		/// </summary>
		/// <value>
		///     <c>true</c> if OK has been clicked; otherwise, <c>false</c>.
		/// </value>
		public bool OkClicked
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the OK command.
		/// </summary>
		/// <value>
		///     The OK command.
		/// </value>
		public ICommand OkCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether OK is enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if OK is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool OkEnabled
		{
			get
			{
				return _okEnabled;
			}
			set
			{
				SetProperty( ref _okEnabled, value );
			}
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
				SetProperty( ref _password, value );

				LoadDatabases( );

				TestOkEnabled( );
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
				SetProperty( ref _selectedDatabase, value );

				TestOkEnabled( );
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
				SetProperty( ref _selectedServerText, value );

				if ( value != ScanNetwork && value != Scanning )
				{
					LoadDatabases( );
				}

				TestOkEnabled( );
			}
		}

		/// <summary>
		///     Gets or sets the selected server text item.
		/// </summary>
		/// <value>
		///     The selected server text item.
		/// </value>
		public string SelectedServerTextItem
		{
			get
			{
				return _selectedServerTextItem;
			}
			set
			{
				if ( value == ScanNetwork )
				{
					ExistingServers = new List<string>
					{
						Scanning
					};

					SetProperty( ref _selectedServerTextItem, Scanning );
					SelectedServerText = Scanning;

					var worker = new BackgroundWorker( );
					worker.DoWork += worker_DoWork;
					worker.RunWorkerCompleted += worker_RunWorkerCompleted;

					worker.RunWorkerAsync( );
					return;
				}

				SetProperty( ref _selectedServerTextItem, value );

				TestOkEnabled( );
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
				SetProperty( ref _username, value );

				LoadDatabases( );

				TestOkEnabled( );
			}
		}

		/// <summary>
		///     Loads the available database servers.
		/// </summary>
		private void LoadAvailableDatabaseServers( )
		{
			_discoveredServers = SqlDataSourceEnumerator.Instance.GetDataSources( );
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

			_tokenSource?.Cancel( false );

			_tokenSource = new CancellationTokenSource( );

			var token = _tokenSource.Token;

			_task = new Task<IList<string>>( ( ) => LoadDatabasesAsync( token ), token );
			_task.ContinueWith( databases =>
			{
				if ( databases != null )
				{
					if ( databases.IsCompleted && !databases.IsCanceled && !databases.IsFaulted && databases.Exception == null && databases.Result != null )
					{
						Databases = databases.Result;
					}
				}
			}, token );

			_task.Start( );
		}

		/// <summary>
		///     Loads the databases asynchronous.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns></returns>
		private IList<string> LoadDatabasesAsync( CancellationToken token )
		{
			List<string> databases = new List<string>( );

			try
			{
				if ( !IntegratedSecurity && string.IsNullOrEmpty( Username ) )
				{
					return databases;
				}

				var ctx = new DatabaseManager( new DatabaseSettings( SelectedServerText, "master", Username, Password, IntegratedSecurity ), token );

				const string commandText = @"--ReadiMon - LoadDatabases
SELECT name FROM master..sysdatabases ORDER BY name";

				if ( token.IsCancellationRequested )
				{
					return databases;
				}

				using ( IDbCommand command = ctx.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							databases.Add( reader.GetString( 0 ) );
						}

						if ( token.IsCancellationRequested )
						{
							return databases;
						}
					}
				}
			}
			catch ( Exception exception )
			{
				EventLog.Instance.WriteException( exception );

				if ( !token.IsCancellationRequested )
				{
					throw;
				}
			}

			return databases;
		}

		/// <summary>
		///     Tests the OK enabled.
		/// </summary>
		private void TestOkEnabled( )
		{
			OkEnabled = !string.IsNullOrEmpty( SelectedServerText ) && !string.IsNullOrEmpty( SelectedDatabase ) && ( IntegratedSecurity || !string.IsNullOrEmpty( Username ) );
		}

		/// <summary>
		///     Handles the DoWork event of the worker control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DoWorkEventArgs" /> instance containing the event data.</param>
		private void worker_DoWork( object sender, DoWorkEventArgs e )
		{
			LoadAvailableDatabaseServers( );
		}

		/// <summary>
		///     Handles the RunWorkerCompleted event of the worker control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RunWorkerCompletedEventArgs" /> instance containing the event data.</param>
		private void worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			System.Windows.Application.Current.Dispatcher.Invoke( ( ) =>
			{
				ExistingServers = ( from DataRow row in _discoveredServers.Rows
					select ( string ) row.ItemArray[ 0 ] ).ToList( );
			} );
		}
	}
}