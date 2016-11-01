// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using ReadiMon.Core;
using ReadiMon.Properties;
using ReadiMon.Shared.Core;

namespace ReadiMon
{
	/// <summary>
	///     Redis Properties View Model.
	/// </summary>
	public class RedisPropertiesViewModel : ViewModelBase
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
		///     The discovered servers
		/// </summary>
		private DataTable _discoveredServers;

		/// <summary>
		///     Existing servers.
		/// </summary>
		private IList<string> _existingServers;

		/// <summary>
		///     The OK enabled value.
		/// </summary>
		private bool _okEnabled;

		/// <summary>
		///     Username.
		/// </summary>
		private string _port;

		/// <summary>
		///     Selected server text.
		/// </summary>
		private string _selectedServerText;

		/// <summary>
		///     The _selected server text item
		/// </summary>
		private string _selectedServerTextItem;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPropertiesViewModel" /> class.
		/// </summary>
		public RedisPropertiesViewModel( )
		{
			OkCommand = new DelegateCommand( ( ) =>
			{
				int portNum;

				int.TryParse( Port, out portNum );

				if ( portNum <= 0 )
				{
					portNum = 6379;
				}

				RedisInfo = new RedisSettings( SelectedServerText, portNum );

				OkClicked = true;
				CloseWindow = true;
			} );

			CloseCommand = new DelegateCommand( ( ) =>
			{
				RedisInfo = null;

				OkClicked = false;
				CloseWindow = true;
			} );

			var server = Settings.Default.RedisServer;
			var port = Settings.Default.RedisPort;

			ExistingServers = new List<string>
			{
				server,
				ScanNetwork
			};

			SelectedServerText = server;
			Port = port.ToString( CultureInfo.InvariantCulture );

			TestOkEnabled( );
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
					SetProperty( ref _closeWindow, value );
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
				SetProperty( ref _existingServers, value );
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
			set;
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
			set;
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
		///     Gets or sets the port.
		/// </summary>
		/// <value>
		///     The port.
		/// </value>
		public string Port
		{
			get
			{
				return _port;
			}
			set
			{
				SetProperty( ref _port, value );
			}
		}

		/// <summary>
		///     Gets or sets the redis info.
		/// </summary>
		/// <value>
		///     The redis info.
		/// </value>
		public RedisSettings RedisInfo
		{
			get;
			set;
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
						Scanning,
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
		///     Loads the available database servers.
		/// </summary>
		private void LoadAvailableDatabaseServers( )
		{
			_discoveredServers = SqlDataSourceEnumerator.Instance.GetDataSources( );
		}

		/// <summary>
		///     Sets the existing server.
		/// </summary>
		/// <param name="redisInfo">The redis information.</param>
		public void SetExistingServer( RedisSettings redisInfo )
		{
			RedisInfo = redisInfo;
			SelectedServerText = redisInfo.ServerName;
			Port = redisInfo.Port.ToString( CultureInfo.InvariantCulture );
		}

		/// <summary>
		///     Tests the OK enabled.
		/// </summary>
		private void TestOkEnabled( )
		{
			OkEnabled = !string.IsNullOrEmpty( SelectedServerText ) && !string.IsNullOrEmpty( Port );
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