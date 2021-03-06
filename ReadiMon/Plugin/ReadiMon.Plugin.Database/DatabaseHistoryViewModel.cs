﻿// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Plugin.Database.Diagnostics;
using ReadiMon.Shared;
using ReadiMon.Shared.Contracts;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Diagnostics.Response;
using ReadiMon.Shared.Messages;
using ReadiMon.Shared.Support;
using StackExchange.Redis;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     The database history view model class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class DatabaseHistoryViewModel : ViewModelBase
	{
		/// <summary>
		///     The dispatcher
		/// </summary>
		private readonly Dispatcher _dispatcher;

		/// <summary>
		///     The filtered transactions
		/// </summary>
		private List<HistoricalTransaction> _filteredTransactions;

		/// <summary>
		///     The list view height
		/// </summary>
		private double _height;

		/// <summary>
		///     The list view enabled
		/// </summary>
		private bool _listViewEnabled;

		/// <summary>
		///     The login filter open
		/// </summary>
		private bool _loginFilterOpen;

		/// <summary>
		///     The login filter open time
		/// </summary>
		private DateTime _loginFilterOpenTime;

		/// <summary>
		///     The multiplexer
		/// </summary>
		private ConnectionMultiplexer _multiplexer;

		/// <summary>
		///     The plugin settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The program filter open
		/// </summary>
		private bool _programFilterOpen;

		/// <summary>
		///     The program filter open time
		/// </summary>
		private DateTime _programFilterOpenTime;

		/// <summary>
		///     The selected transactions
		/// </summary>
		private IList _selectedTransactions;

		/// <summary>
		///     The spid filter open
		/// </summary>
		private bool _spidFilterOpen;

		/// <summary>
		///     The spid filter open time
		/// </summary>
		private DateTime _spidFilterOpenTime;

		/// <summary>
		///     The tenant filter open
		/// </summary>
		private bool _tenantFilterOpen;

		/// <summary>
		///     The tenant filter open time
		/// </summary>
		private DateTime _tenantFilterOpenTime;

		/// <summary>
		///     The transactions
		/// </summary>
		private List<HistoricalTransaction> _transactions;

		/// <summary>
		///     The user filter open
		/// </summary>
		private bool _userFilterOpen;

		/// <summary>
		///     The user filter open time
		/// </summary>
		private DateTime _userFilterOpenTime;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseHistoryViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public DatabaseHistoryViewModel( IPluginSettings settings )
		{
			_dispatcher = Dispatcher.CurrentDispatcher;

			PluginSettings = settings;

			RefreshCommand = new DelegateCommand( LoadTransactions );

			RevertCommand = new DelegateCommand<HistoricalTransaction>( RevertClick );
			RevertToCommand = new DelegateCommand<HistoricalTransaction>( RevertToClick );
			RevertToTenantCommand = new DelegateCommand<Tuple<long, HistoricalTransaction>>( RevertToTenantClick );
			RevertRangeCommand = new DelegateCommand<IList>( RevertRangeClick );
			RevertRangeTenantCommand = new DelegateCommand<Tuple<long, IList>>( RevertRangeTenantClick );
			RevertSelectedCommand = new DelegateCommand<IList>( RevertSelectedClick );
			RevertSelectedTenantCommand = new DelegateCommand<Tuple<long, IList>>( RevertSelectedTenantClick );

			FilterUserCommand = new DelegateCommand( FilterUserClick );
			FilterTenantCommand = new DelegateCommand( FilterTenantClick );
			FilterSpidCommand = new DelegateCommand( FilterSpidClick );
			FilterProgramCommand = new DelegateCommand( FilterProgramClick );
			FilterLoginCommand = new DelegateCommand( FilterLoginClick );

			SelectUsersCommand = new DelegateCommand<bool>( selected =>
			{
				SelectClick( UserFilters, selected );
			} );
			SelectTenantsCommand = new DelegateCommand<bool>( selected =>
			{
				SelectClick( TenantFilters, selected );
			} );
			SelectSpidsCommand = new DelegateCommand<bool>( selected =>
			{
				SelectClick( SpidFilters, selected );
			} );
			SelectProgramsCommand = new DelegateCommand<bool>( selected =>
			{
				SelectClick( ProgramFilters, selected );
			} );
			SelectLoginsCommand = new DelegateCommand<bool>( selected =>
			{
				SelectClick( LoginFilters, selected );
			} );

			Connect( );
		}

		/// <summary>
		///     Gets or sets the filtered transactions.
		/// </summary>
		/// <value>
		///     The filtered transactions.
		/// </value>
		public List<HistoricalTransaction> FilteredTransactions
		{
			get
			{
				return _filteredTransactions;
			}
			set
			{
				SetProperty( ref _filteredTransactions, value );
			}
		}

		/// <summary>
		///     Gets or sets the filter login command.
		/// </summary>
		/// <value>
		///     The filter login command.
		/// </value>
		public ICommand FilterLoginCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the filter program command.
		/// </summary>
		/// <value>
		///     The filter program command.
		/// </value>
		public ICommand FilterProgramCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the filter spid command.
		/// </summary>
		/// <value>
		///     The filter spid command.
		/// </value>
		public ICommand FilterSpidCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the filter tenant command.
		/// </summary>
		/// <value>
		///     The filter tenant command.
		/// </value>
		public ICommand FilterTenantCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the filter user command.
		/// </summary>
		/// <value>
		///     The filter user command.
		/// </value>
		public ICommand FilterUserCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the height.
		/// </summary>
		/// <value>
		///     The height.
		/// </value>
		public double Height
		{
			get
			{
				return _height;
			}
			set
			{
				SetProperty( ref _height, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [ListView enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [ListView enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool ListViewEnabled
		{
			get
			{
				return _listViewEnabled;
			}
			set
			{
				SetProperty( ref _listViewEnabled, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [login filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [login filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool LoginFilterOpen
		{
			get
			{
				return _loginFilterOpen;
			}
			set
			{
				if ( _loginFilterOpen != value && _loginFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _loginFilterOpen, value );

					_loginFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		///     Gets or sets the login filters.
		/// </summary>
		/// <value>
		///     The login filters.
		/// </value>
		public List<FilterObject> LoginFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		public IPluginSettings PluginSettings
		{
			get
			{
				return _pluginSettings;
			}
			set
			{
				_pluginSettings = value;

				LoadTransactions( );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [program filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [program filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool ProgramFilterOpen
		{
			get
			{
				return _programFilterOpen;
			}
			set
			{
				if ( _programFilterOpen != value && _programFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _programFilterOpen, value );

					_programFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		///     Gets or sets the program filters.
		/// </summary>
		/// <value>
		///     The program filters.
		/// </value>
		public List<FilterObject> ProgramFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the refresh command.
		/// </summary>
		/// <value>
		///     The refresh command.
		/// </value>
		public ICommand RefreshCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the revert command.
		/// </summary>
		/// <value>
		///     The revert command.
		/// </value>
		public ICommand RevertCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the revert range command.
		/// </summary>
		/// <value>
		///     The revert range command.
		/// </value>
		public ICommand RevertRangeCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the revert range tenant command.
		/// </summary>
		/// <value>
		///     The revert range tenant command.
		/// </value>
		public ICommand RevertRangeTenantCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the revert selected command.
		/// </summary>
		/// <value>
		///     The revert selected command.
		/// </value>
		public ICommand RevertSelectedCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the revert selected tenant command.
		/// </summary>
		/// <value>
		///     The revert selected tenant command.
		/// </value>
		public ICommand RevertSelectedTenantCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the revert to command.
		/// </summary>
		/// <value>
		///     The revert to command.
		/// </value>
		public ICommand RevertToCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the revert to tenant command.
		/// </summary>
		/// <value>
		///     The revert to tenant command.
		/// </value>
		public ICommand RevertToTenantCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the selected transactions.
		/// </summary>
		/// <value>
		///     The selected transactions.
		/// </value>
		public IList SelectedTransactions
		{
			get
			{
				return _selectedTransactions;
			}
			set
			{
				SetProperty( ref _selectedTransactions, value );
			}
		}

		/// <summary>
		///     Gets or sets the select logins command.
		/// </summary>
		/// <value>
		///     The select logins command.
		/// </value>
		public ICommand SelectLoginsCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the select programs command.
		/// </summary>
		/// <value>
		///     The select programs command.
		/// </value>
		public ICommand SelectProgramsCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the select spids command.
		/// </summary>
		/// <value>
		///     The select spids command.
		/// </value>
		public ICommand SelectSpidsCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the select tenants command.
		/// </summary>
		/// <value>
		///     The select tenants command.
		/// </value>
		public ICommand SelectTenantsCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the select all users command.
		/// </summary>
		/// <value>
		///     The select all users command.
		/// </value>
		public ICommand SelectUsersCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [spid filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [spid filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool SpidFilterOpen
		{
			get
			{
				return _spidFilterOpen;
			}
			set
			{
				if ( _spidFilterOpen != value && _spidFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _spidFilterOpen, value );

					_spidFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		///     Gets or sets the spid filters.
		/// </summary>
		/// <value>
		///     The spid filters.
		/// </value>
		public List<FilterObject> SpidFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [tenant filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [tenant filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool TenantFilterOpen
		{
			get
			{
				return _tenantFilterOpen;
			}
			set
			{
				if ( _tenantFilterOpen != value && _tenantFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _tenantFilterOpen, value );

					_tenantFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		///     Gets or sets the tenant filters.
		/// </summary>
		/// <value>
		///     The tenant filters.
		/// </value>
		public List<FilterObject> TenantFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [user filter open].
		/// </summary>
		/// <value>
		///     <c>true</c> if [user filter open]; otherwise, <c>false</c>.
		/// </value>
		public bool UserFilterOpen
		{
			get
			{
				return _userFilterOpen;
			}
			set
			{
				if ( _userFilterOpen != value && _userFilterOpenTime.AddMilliseconds( 500 ) < DateTime.UtcNow )
				{
					SetProperty( ref _userFilterOpen, value );

					_userFilterOpenTime = DateTime.UtcNow;
				}
			}
		}

		/// <summary>
		///     Gets or sets the user filters.
		/// </summary>
		/// <value>
		///     The user filters.
		/// </value>
		public List<FilterObject> UserFilters
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
		/// </value>
		private bool IsEnabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the transactions.
		/// </summary>
		/// <value>
		///     The transactions.
		/// </value>
		private List<HistoricalTransaction> Transactions
		{
			get
			{
				return _transactions;
			}
			set
			{
				SetProperty( ref _transactions, value );
			}
		}

		/// <summary>
		///     Called when shutting down.
		/// </summary>
		public void OnShutdown( )
		{
			Stop( );
		}


		/// <summary>
		///     Connects this instance.
		/// </summary>
		private void Connect( )
		{
			if ( _multiplexer != null )
			{
				_multiplexer.Dispose( );
				_multiplexer = null;
			}

			string config = $"{PluginSettings.RedisSettings.ServerName}:{PluginSettings.RedisSettings.Port}";

			Task<ConnectionMultiplexer> task = ConnectionMultiplexer.ConnectAsync( config );

			task.ContinueWith( ConnectComplete );
		}

		/// <summary>
		///     Connects the complete.
		/// </summary>
		/// <param name="task">The task.</param>
		private void ConnectComplete( Task<ConnectionMultiplexer> task )
		{
			if ( !task.IsFaulted && task.Result != null )
			{
				_multiplexer = task.Result;

				ISubscriber subscriber = _multiplexer.GetSubscriber( );

				subscriber.Subscribe( "ReadiNowDiagnosticResponses", OnMessage );

				IsEnabled = true;
			}
		}

		/// <summary>
		///     Filters the login click.
		/// </summary>
		private void FilterLoginClick( )
		{
			LoginFilterOpen = true;
		}

		/// <summary>
		///     Filters the program click.
		/// </summary>
		private void FilterProgramClick( )
		{
			ProgramFilterOpen = true;
		}

		/// <summary>
		///     Filters the spid click.
		/// </summary>
		private void FilterSpidClick( )
		{
			SpidFilterOpen = true;
		}

		/// <summary>
		///     Filters the tenant click.
		/// </summary>
		private void FilterTenantClick( )
		{
			TenantFilterOpen = true;
		}

		private void FilterUpdate( )
		{
			List<HistoricalTransaction> filteredTransactions = new List<HistoricalTransaction>( );

			foreach ( var transaction in Transactions )
			{
				FilterObject userFilterObject = UserFilters.FirstOrDefault( f => f.Value.ToString( ) == transaction.ActiveUserName.ToString( ) );
				FilterObject tenantFilterObject = TenantFilters.FirstOrDefault( f => f.Value.ToString( ) == transaction.TenantName.ToString( ) );
				FilterObject spidFilterObject = SpidFilters.FirstOrDefault( f => f.Value.ToString( ) == transaction.Spid.ToString( ) );
				FilterObject programFilterObject = ProgramFilters.FirstOrDefault( f => f.Value.ToString( ) == transaction.ProgramName );
				FilterObject loginFilterObject = LoginFilters.FirstOrDefault( f => f.Value.ToString( ) == transaction.LoginName );

				if ( ( userFilterObject == null || userFilterObject.IsFiltered ) && ( tenantFilterObject == null || tenantFilterObject.IsFiltered ) && ( spidFilterObject == null || spidFilterObject.IsFiltered ) && ( programFilterObject == null || programFilterObject.IsFiltered ) && ( loginFilterObject == null || loginFilterObject.IsFiltered ) )
				{
					filteredTransactions.Add( transaction );
				}
			}

			FilteredTransactions = filteredTransactions;
		}

		/// <summary>
		///     Filters the user click.
		/// </summary>
		private void FilterUserClick( )
		{
			UserFilterOpen = true;
		}

		/// <summary>
		///     Gets the change counts.
		/// </summary>
		/// <param name="transactionId">The transaction identifier.</param>
		/// <returns></returns>
		private DatabaseHistoryChangeCounts GetChangeCounts( long transactionId )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - LoadTransactionCounts
SET NOCOUNT ON

SELECT
	[Entity Added],
	[Entity Deleted],
	[Relationship Added],
	[Relationship Deleted],
	[Alias Added],
	[Alias Deleted],
	[Bit Added],
	[Bit Deleted],
	[DateTime Added],
	[DateTime Deleted],
	[Decimal Added],
	[Decimal Deleted],
	[Guid Added],
	[Guid Deleted],
	[Int Added],
	[Int Deleted],
	[NVarChar Added],
	[NVarChar Deleted],
	[Xml Added],
	[Xml Deleted]
FROM
	dbgHist_Transaction
WHERE
	TransactionId = @transactionId";

			DatabaseHistoryChangeCounts changeCounts = null;

			using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
			{
				databaseManager.AddParameter( command, "@transactionId", transactionId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					if ( reader.Read( ) )
					{
						changeCounts = new DatabaseHistoryChangeCounts( reader );
					}
				}
			}

			return changeCounts;
		}

		/// <summary>
		///     Loads the transactions.
		/// </summary>
		private void LoadTransactions( )
		{
			BackgroundWorker worker = new BackgroundWorker( );
			worker.DoWork += Worker_DoWork;
			worker.RunWorkerAsync( );
		}

		/// <summary>
		///     Loads the transactions.
		/// </summary>
		private void LoadTransactionsAsynchronous( )
		{
			_dispatcher.Invoke( ( ) =>
			{
				ListViewEnabled = false;
				MouseCursor.Set( Cursors.Wait );
				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Loading transactions..." ).ToString( ) );
			} );

			try
			{
				var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

				HashSet<long> selectedTransactions = null;

				if ( SelectedTransactions != null )
				{
					selectedTransactions = new HashSet<long>( );

					foreach ( HistoricalTransaction transaction in SelectedTransactions )
					{
						selectedTransactions.Add( transaction.TransactionId );
					}
				}

				const string commandText = @"--ReadiMon - LoadTransactions
SET NOCOUNT ON

SELECT
	[TransactionId],
	[UserId],
	[TenantId],
	[Spid],
	[Timestamp],
	[HostName],
	[ProgramName],
	[Domain],
	[Username],
	[LoginName],
	[Context]
FROM
	Hist_Transaction
ORDER BY
	TransactionId DESC";

				HistoricalTransaction.TenantCache.Clear( );
				HistoricalTransaction.UserCache.Clear( );

				HashSet<string> users = new HashSet<string>( );
				HashSet<string> tenants = new HashSet<string>( );
				HashSet<string> spids = new HashSet<string>( );
				HashSet<string> programs = new HashSet<string>( );
				HashSet<string> logins = new HashSet<string>( );

				var transactions = new List<HistoricalTransaction>( );

				using ( IDbCommand command = databaseManager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						HistoricalTransaction previousTransaction = null;

						while ( reader.Read( ) )
						{
							var transaction = new HistoricalTransaction( reader, GetChangeCounts );

							transactions.Add( transaction );

							if ( previousTransaction != null )
							{
								previousTransaction.NextTransaction = transaction;
							}

							previousTransaction = transaction;
						}
					}
				}

				const string tenantCommandText = @"-- ReadiMon - Select Tenant Names
SELECT
	Id, name
FROM
	_vTenant";
				using ( IDbCommand command = databaseManager.CreateCommand( tenantCommandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							HistoricalTransaction.TenantCache[ reader.GetInt64( 0 ) ] = reader.GetString( 1 );
						}
					}
				}

				HistoricalTransaction.TenantCache[ 0 ] = "Global";

				const string userCommandText = @"-- ReadiMon - Select User Name
SELECT
	dbo.fnName( @userId )";

				foreach ( long userId in transactions.Select( t => t.UserId ).Distinct( ).Where( t => t > 0 ) )
				{
					using ( SqlCommand command = databaseManager.CreateCommand( userCommandText ) )
					{
						databaseManager.AddParameter( command, "@userId", userId );

						object userName = command.ExecuteScalar( );

						if ( userName != null && userName != DBNull.Value )
						{
							HistoricalTransaction.UserCache[ userId ] = userName.ToString( );
						}
					}
				}

				HistoricalTransaction.UserCache[ 0 ] = "Administrator";

				Transactions = transactions;

				users.UnionWith( Transactions.Select( t => t.ActiveUserName.ToString( ) ) );
				tenants.UnionWith( Transactions.Select( t => t.TenantName.ToString( ) ) );
				spids.UnionWith( Transactions.Select( t => t.Spid.ToString( ) ) );
				programs.UnionWith( Transactions.Select( t => t.ProgramName ) );
				logins.UnionWith( Transactions.Select( t => t.LoginName ) );

				UserFilters = new List<FilterObject>( );

				foreach ( string user in users.OrderBy( k => k ) )
				{
					UserFilters.Add( new FilterObject( user, string.IsNullOrEmpty( user ) ? "<empty>" : user, true, UserFilterUpdate ) );
				}

				OnPropertyChanged( "UserFilters" );

				TenantFilters = new List<FilterObject>( );

				foreach ( string ten in tenants.OrderBy( k => k ) )
				{
					TenantFilters.Add( new FilterObject( ten, string.IsNullOrEmpty( ten ) ? "<empty>" : ten, true, TenantFilterUpdate ) );
				}

				OnPropertyChanged( "TenantFilters" );

				SpidFilters = new List<FilterObject>( );

				foreach ( string spid in spids.OrderBy( k => k ) )
				{
					SpidFilters.Add( new FilterObject( spid, string.IsNullOrEmpty( spid ) ? "<empty>" : spid, true, SpidFilterUpdate ) );
				}

				OnPropertyChanged( "SpidFilters" );

				ProgramFilters = new List<FilterObject>( );

				foreach ( string prog in programs.OrderBy( k => k ) )
				{
					ProgramFilters.Add( new FilterObject( prog, string.IsNullOrEmpty( prog ) ? "<empty>" : prog, true, ProgramFilterUpdate ) );
				}

				OnPropertyChanged( "ProgramFilters" );

				LoginFilters = new List<FilterObject>( );

				foreach ( string log in logins.OrderBy( k => k ) )
				{
					LoginFilters.Add( new FilterObject( log, string.IsNullOrEmpty( log ) ? "<empty>" : log, true, LoginFilterUpdate ) );
				}

				OnPropertyChanged( "LoginFilters" );

				FilteredTransactions = new List<HistoricalTransaction>( Transactions );

				if ( selectedTransactions != null )
				{
					List<HistoricalTransaction> newlySelectedTransactions = FilteredTransactions.Where( t => selectedTransactions.Contains( t.TransactionId ) ).ToList( );

					SelectedTransactions = newlySelectedTransactions;
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
			finally
			{
				_dispatcher.Invoke( ( ) =>
				{
					ListViewEnabled = true;
					MouseCursor.Set( Cursors.Arrow );
					PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Ready..." ).ToString( ) );
				} );
			}
		}

		/// <summary>
		///     Logins the filter update.
		/// </summary>
		private void LoginFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "LoginFilters" );
		}

		/// <summary>
		///     Called when a message is received.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="message">The message.</param>
		private void OnMessage( RedisChannel channel, RedisValue message )
		{
			byte[ ] messageBytes = ChannelHelper.Decompress( message );

			var response = ChannelHelper.Deserialize<ChannelMessage<DiagnosticResponse>>( messageBytes );

			var flushCachesResponse = response.Message as FlushCachesResponse;

			if ( flushCachesResponse != null )
			{
				/////
				// Message response received.
				/////
			}
		}

		private void ProgramFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "ProgramFilters" );
		}

		/// <summary>
		///     Reverts the click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void RevertClick( HistoricalTransaction obj )
		{
			var messageBoxResult = MessageBox.Show( "Are you sure you wish to revert this transaction?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning );

			if ( messageBoxResult == MessageBoxResult.Yes )
			{
				using ( new MouseCursor( Cursors.Wait ) )
				{
					var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

					try
					{
						using ( SqlCommand command = databaseManager.CreateCommand( "spRevert", CommandType.StoredProcedure ) )
						{
							databaseManager.AddParameter( command, "@transactionId", obj.TransactionId );

							command.ExecuteNonQuery( );

							LoadTransactions( );

							SendRequest( obj.TenantId );
						}
					}
					catch ( Exception exc )
					{
						MessageBox.Show( "Unable to revert the selected transaction(s) due to successive transactions requiring them.", "Unable to revert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
						PluginSettings.EventLog.WriteException( exc );
					}
				}
			}
		}

		/// <summary>
		///     Reverts the range click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void RevertRangeClick( IList obj )
		{
			if ( obj == null || obj.Count <= 0 )
			{
				return;
			}

			HistoricalTransaction start = obj[ 0 ] as HistoricalTransaction;
			HistoricalTransaction end = obj[ obj.Count - 1 ] as HistoricalTransaction;

			if ( start != null && end != null )
			{
				var messageBoxResult = MessageBox.Show( "Are you sure you wish to revert these transactions?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning );

				if ( messageBoxResult == MessageBoxResult.Yes )
				{
					using ( new MouseCursor( Cursors.Wait ) )
					{
						var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

						try
						{
							using ( SqlCommand command = databaseManager.CreateCommand( "spRevertRange", CommandType.StoredProcedure ) )
							{
								databaseManager.AddParameter( command, "@fromTransactionId", start.TransactionId );
								databaseManager.AddParameter( command, "@toTransactionId", end.TransactionId );

								command.ExecuteNonQuery( );

								LoadTransactions( );

								SendRequest( start.TenantId );
							}
						}
						catch ( Exception exc )
						{
							MessageBox.Show( "Unable to revert the selected transaction(s) due to successive transactions requiring them.", "Unable to revert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
							PluginSettings.EventLog.WriteException( exc );
						}
					}
				}
			}
		}

		/// <summary>
		///     Reverts the range tenant click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void RevertRangeTenantClick( Tuple<long, IList> obj )
		{
			if ( obj?.Item2 == null || obj.Item2.Count <= 0 )
			{
				return;
			}

			HistoricalTransaction start = obj.Item2[ 0 ] as HistoricalTransaction;
			HistoricalTransaction end = obj.Item2[ obj.Item2.Count - 1 ] as HistoricalTransaction;

			if ( start != null && end != null )
			{
				var messageBoxResult = MessageBox.Show( "Are you sure you wish to revert these transactions?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning );

				if ( messageBoxResult == MessageBoxResult.Yes )
				{
					using ( new MouseCursor( Cursors.Wait ) )
					{
						var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

						try
						{
							using ( SqlCommand command = databaseManager.CreateCommand( "spRevertRange", CommandType.StoredProcedure ) )
							{
								databaseManager.AddParameter( command, "@fromTransactionId", start.TransactionId );
								databaseManager.AddParameter( command, "@toTransactionId", end.TransactionId );
								databaseManager.AddParameter( command, "@tenantId", obj.Item1 );

								command.ExecuteNonQuery( );

								LoadTransactions( );

								SendRequest( obj.Item1 );
							}
						}
						catch ( Exception exc )
						{
							MessageBox.Show( "Unable to revert the selected transaction(s) due to successive transactions requiring them.", "Unable to revert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
							PluginSettings.EventLog.WriteException( exc );
						}
					}
				}
			}
		}

		/// <summary>
		///     Reverts the selected click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void RevertSelectedClick( IList obj )
		{
			if ( obj == null || obj.Count <= 0 )
			{
				return;
			}

			ISet<long> transactions = new HashSet<long>( );

			HistoricalTransaction firstTransaction = null;

			foreach ( HistoricalTransaction transaction in obj )
			{
				transactions.Add( transaction.TransactionId );

				if ( firstTransaction == null )
				{
					firstTransaction = transaction;
				}
			}

			if ( transactions.Count > 0 )
			{
				var messageBoxResult = MessageBox.Show( "Are you sure you wish to revert the selected transactions?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning );

				if ( messageBoxResult == MessageBoxResult.Yes )
				{
					using ( new MouseCursor( Cursors.Wait ) )
					{
						var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

						var sqlTransaction = databaseManager.BeginTransaction( );

						try
						{
							using ( SqlCommand command = databaseManager.CreateCommand( "spRevert", CommandType.StoredProcedure, 30000, sqlTransaction ) )
							{
								var parameter = databaseManager.AddParameter( command, "@transactionId", SqlDbType.BigInt );
								databaseManager.AddParameter( command, "@context", SqlDbType.VarChar, $"Reverting transactions {string.Join( ",", transactions.OrderByDescending( t => t ) )}" );

								foreach ( long transactionId in transactions.OrderByDescending( t => t ) )
								{
									parameter.Value = transactionId;

									command.ExecuteNonQuery( );
								}

								sqlTransaction.Commit( );

								LoadTransactions( );

								if ( firstTransaction != null )
								{
									SendRequest( firstTransaction.TenantId );
								}
							}
						}
						catch ( Exception exc )
						{
							sqlTransaction.Rollback( );

							MessageBox.Show( "Unable to revert the selected transaction(s) due to successive transactions requiring them.", "Unable to revert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
							PluginSettings.EventLog.WriteException( exc );
						}
					}
				}
			}
		}

		/// <summary>
		///     Reverts the selected tenant click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void RevertSelectedTenantClick( Tuple<long, IList> obj )
		{
			if ( obj?.Item2 == null || obj.Item2.Count <= 0 )
			{
				return;
			}

			ISet<long> transactions = new HashSet<long>( );

			foreach ( HistoricalTransaction transaction in obj.Item2 )
			{
				transactions.Add( transaction.TransactionId );
			}

			if ( transactions.Count > 0 )
			{
				var messageBoxResult = MessageBox.Show( "Are you sure you wish to revert the selected transactions?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning );

				if ( messageBoxResult == MessageBoxResult.Yes )
				{
					using ( new MouseCursor( Cursors.Wait ) )
					{
						var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

						var sqlTransaction = databaseManager.BeginTransaction( );

						try
						{
							using ( SqlCommand command = databaseManager.CreateCommand( "spRevert", CommandType.StoredProcedure, 30000, sqlTransaction ) )
							{
								var parameter = databaseManager.AddParameter( command, "@transactionId", SqlDbType.BigInt );
								databaseManager.AddParameter( command, "@tenantId", obj.Item1 );
								databaseManager.AddParameter( command, "@context", SqlDbType.VarChar, $"Reverting transactions {string.Join( ",", transactions.OrderByDescending( t => t ) )}" );

								foreach ( long transactionId in transactions.OrderByDescending( t => t ) )
								{
									parameter.Value = transactionId;

									command.ExecuteNonQuery( );
								}

								sqlTransaction.Commit( );

								LoadTransactions( );

								SendRequest( obj.Item1 );
							}
						}
						catch ( Exception exc )
						{
							sqlTransaction.Rollback( );

							MessageBox.Show( "Unable to revert the selected transaction(s) due to successive transactions requiring them.", "Unable to revert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
							PluginSettings.EventLog.WriteException( exc );
						}
					}
				}
			}
		}

		/// <summary>
		///     Reverts to click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void RevertToClick( HistoricalTransaction obj )
		{
			var messageBoxResult = MessageBox.Show( "Are you sure you wish to revert to this transaction?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning );

			if ( messageBoxResult == MessageBoxResult.Yes )
			{
				using ( new MouseCursor( Cursors.Wait ) )
				{
					var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

					try
					{
						/////
						// Start at the next transaction since RevertTo reverts upto but not including the selected transaction.
						/////
						HistoricalTransaction transaction = obj.NextTransaction;

						long tenantId = transaction.TenantId;
						bool multipleTenants = tenantId == -1;

						while ( transaction != null )
						{
							if ( transaction.TenantId != tenantId )
							{
								multipleTenants = true;
								break;
							}

							transaction = transaction.NextTransaction;
						}

						using ( SqlCommand command = databaseManager.CreateCommand( "spRevertTo", CommandType.StoredProcedure ) )
						{
							databaseManager.AddParameter( command, "@transactionId", obj.TransactionId );

							if ( !multipleTenants )
							{
								databaseManager.AddParameter( command, "@tenantId", tenantId );
							}

							command.ExecuteNonQuery( );

							LoadTransactions( );

							SendRequest( obj.TenantId );
						}
					}
					catch ( Exception exc )
					{
						MessageBox.Show( "Unable to revert the selected transaction(s) due to successive transactions requiring them.", "Unable to revert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
						PluginSettings.EventLog.WriteException( exc );
					}
				}
			}
		}

		/// <summary>
		///     Reverts to tenant click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void RevertToTenantClick( Tuple<long, HistoricalTransaction> obj )
		{
			var messageBoxResult = MessageBox.Show( "Are you sure you wish to revert to this transaction?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning );

			if ( messageBoxResult == MessageBoxResult.Yes )
			{
				using ( new MouseCursor( Cursors.Wait ) )
				{
					var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

					try
					{
						using ( SqlCommand command = databaseManager.CreateCommand( "spRevertTo", CommandType.StoredProcedure ) )
						{
							databaseManager.AddParameter( command, "@transactionId", obj.Item2.TransactionId );
							databaseManager.AddParameter( command, "@tenantId", obj.Item1 );

							command.ExecuteNonQuery( );

							LoadTransactions( );

							SendRequest( obj.Item1 );
						}
					}
					catch ( Exception exc )
					{
						MessageBox.Show( "Unable to revert the selected transaction(s) due to successive transactions requiring them.", "Unable to revert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
						PluginSettings.EventLog.WriteException( exc );
					}
				}
			}
		}

		/// <summary>
		///     Selects the click.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="selected">if set to <c>true</c> [selected].</param>
		private void SelectClick( List<FilterObject> collection, bool selected )
		{
			foreach ( FilterObject filter in collection )
			{
				filter.IsFiltered = selected;
			}
		}

		/// <summary>
		///     Sends the thread request.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		private void SendRequest( long tenantId = -1 )
		{
			if ( !IsEnabled )
			{
				return;
			}

			ISubscriber subscriber = _multiplexer.GetSubscriber( );

			var request = new FlushCachesRequest
			{
				TenantId = tenantId
			};

			ChannelMessage<FlushCachesRequest> channelMessage = ChannelMessage<FlushCachesRequest>.Create( request );

			byte[ ] serializedObject = ChannelHelper.Serialize( channelMessage );
			byte[ ] compressedObject = ChannelHelper.Compress( serializedObject );

			subscriber.Publish( "ReadiNowDiagnosticRequests", compressedObject, CommandFlags.FireAndForget );
		}

		private void SpidFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "SpidFilters" );
		}

		/// <summary>
		///     Stops this instance.
		/// </summary>
		private void Stop( )
		{
			IsEnabled = false;

			if ( _multiplexer != null )
			{
				ISubscriber subscriber = _multiplexer.GetSubscriber( );

				subscriber.UnsubscribeAll( );

				_multiplexer.Close( );

				_multiplexer.Dispose( );
				_multiplexer = null;
			}
		}

		/// <summary>
		///     Tenants the filter update.
		/// </summary>
		private void TenantFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "TenantFilters" );
		}

		/// <summary>
		///     Users the filter update.
		/// </summary>
		private void UserFilterUpdate( )
		{
			FilterUpdate( );

			OnPropertyChanged( "UserFilters" );
		}

		/// <summary>
		///     Handles the DoWork event of the Worker control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DoWorkEventArgs" /> instance containing the event data.</param>
		private void Worker_DoWork( object sender, DoWorkEventArgs e )
		{
			LoadTransactionsAsynchronous( );
		}
	}
}