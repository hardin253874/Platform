// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Graphs.LicensingMetrics
{
	/// <summary>
	///     LicensingMetricsViewModel class.
	/// </summary>
	/// <seealso cref="ViewModelBase" />
	public class LicensingMetricsViewModel : ViewModelBase
	{
		private readonly Dispatcher _dispatcher;

		private readonly DispatcherTimer _dispatcherTimer;

		private bool? _canRun;
		private string _error;
		private bool _isRunning;

		private Guid? _jobIdentifier;

		private string _message;
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The database manager
		/// </summary>
		protected DatabaseManager DatabaseManager;

		/// <summary>
		///     Initializes a new instance of the <see cref="LicensingMetricsViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public LicensingMetricsViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			_dispatcher = Dispatcher.CurrentDispatcher;

			RunCommand = new DelegateCommand( ( ) => _dispatcher.Invoke( Run ) );

			_jobIdentifier = GetJobId( );

			if ( _dispatcherTimer == null )
			{
				_dispatcherTimer = new DispatcherTimer( );
				_dispatcherTimer.Tick += DispatcherTimerOnTick;
				_dispatcherTimer.Interval = new TimeSpan( 0, 0, 1 ); // 1 secs
				if ( _jobIdentifier.HasValue )
					_dispatcherTimer.Start( );
			}

			Jobs = new ObservableCollection<IndexData>( );

			Load( );
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance can run.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can run; otherwise, <c>false</c>.
		/// </value>
		public bool CanRun
		{
			get
			{
				return _canRun.HasValue && _canRun.Value;
			}
			set
			{
				var reload = _canRun.HasValue && !_canRun.Value && value;
				SetProperty( ref _canRun, value );
				if ( reload )
				{
					_dispatcher.Invoke( Load );

					OnGatherComplete( );
				}
			}
		}

		/// <summary>
		///     Gets the jobs.
		/// </summary>
		/// <value>
		///     The jobs.
		/// </value>
		public ObservableCollection<IndexData> Jobs
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the message.
		/// </summary>
		/// <value>
		///     The message.
		/// </value>
		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				SetProperty( ref _message, value );
			}
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

				DatabaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );
			}
		}

		/// <summary>
		///     Gets or sets the run command.
		/// </summary>
		/// <value>
		///     The run command.
		/// </value>
		public ICommand RunCommand
		{
			get;
			set;
		}

		private void CheckJob( )
		{
			if ( !_jobIdentifier.HasValue )
			{
				Message = "Job not found.";
				CanRun = false;
				return;
			}

			try
			{
				const string sql = @"--ReadiMon - CheckJob
SELECT COUNT(1) FROM msdb.dbo.sysjobactivity WHERE job_id = @jobIdentifier AND start_execution_date IS NOT NULL AND stop_execution_date IS NULL";
				using ( var cmd = DatabaseManager.CreateCommand( sql ) )
				{
					DatabaseManager.AddParameter( cmd, "@jobIdentifier", _jobIdentifier );

					var running = ( int? ) cmd.ExecuteScalar( );

					if ( !running.HasValue || running.Value == 0 )
					{
						Message = _error ?? "Job is ready to run.";
						CanRun = true;
					}
					else
					{
						Message = "Job is running.";
						CanRun = false;
					}
				}
			}
			catch ( Exception e )
			{
				PluginSettings.EventLog.WriteException( e );
			}
		}

		private void DispatcherTimerOnTick( object sender, EventArgs eventArgs )
		{
			CheckJob( );
		}

		private Guid? GetJobId( )
		{
			try
			{
				const string commandText = @"--ReadiMon - GetJobId
SELECT job_id FROM msdb.dbo.sysjobs WHERE name = @job";
				using ( var cmd = DatabaseManager.CreateCommand( commandText ) )
				{
					DatabaseManager.AddParameter( cmd, "@job", string.Format( "{0} Licensing Metrics Update", PluginSettings.DatabaseSettings.CatalogName ) );

					return ( Guid? ) cmd.ExecuteScalar( );
				}
			}
			catch ( Exception e )
			{
				PluginSettings.EventLog.WriteException( e );
			}

			return null;
		}

		private void Load( )
		{
			try
			{
				Jobs.Clear( );

				using ( var cmd = DatabaseManager.CreateCommand( "SELECT i.[Id], i.[Timestamp] FROM [dbo].[Lic_Index] i ORDER BY i.[Timestamp] DESC" ) )
				{
					using ( IDataReader reader = cmd.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							Jobs.Add( new IndexData
							{
								Id = reader.GetInt64( 0 ),
								TimeStamp = reader.GetDateTime( 1 ).ToLocalTime( )
							} );
						}
					}
				}
			}
			catch ( Exception e )
			{
				PluginSettings.EventLog.WriteException( e );
			}
		}

		private void OnGatherComplete( )
		{
			if ( GatherComplete != null )
			{
				GatherComplete( this, new EventArgs( ) );
			}
		}

		private void Run( )
		{
			try
			{
				if ( _isRunning )
					return;

				_isRunning = true;

				if ( !_jobIdentifier.HasValue )
					return;

				CheckJob( );

				if ( !CanRun )
					return;

				using ( var cmd = DatabaseManager.CreateCommand( "msdb.dbo.sp_start_job @job_id = @jobIdentifier" ) )
				{
					DatabaseManager.AddParameter( cmd, "@jobIdentifier", _jobIdentifier.Value );
					cmd.ExecuteNonQuery( );

					_error = null;
				}
			}
			catch ( Exception e )
			{
				PluginSettings.EventLog.WriteException( e );

				_error = e.Message;
			}
			finally
			{
				_isRunning = false;
			}
		}

		/// <summary>
		///     Occurs when [gather complete].
		/// </summary>
		public event EventHandler GatherComplete;
	}
}