// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Plugin.Redis.Contracts;
using ReadiMon.Plugin.Redis.Diagnostics;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Diagnostics.Response;
using StackExchange.Redis;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Thread monitor view model.
	/// </summary>
	public class ThreadMonitorViewModel : ViewModelBase
	{
		/// <summary>
		///     The message added
		/// </summary>
		private const string MessageAdded = "PaleGreen";

		/// <summary>
		///     The message default
		/// </summary>
		private const string MessageDefault = "Transparent";

		/// <summary>
		///     The message removed
		/// </summary>
		private const string MessageRemoved = "Salmon";

		/// <summary>
		///     The dispatcher
		/// </summary>
		private readonly Dispatcher _dispatcher;

		/// <summary>
		///     Whether capturing of Pub/Sub messages is enabled.
		/// </summary>
		private bool _isEnabled;

		/// <summary>
		///     The messages
		/// </summary>
		private ObservableCollection<ThreadMessage> _messages = new ObservableCollection<ThreadMessage>( );

		/// <summary>
		///     The multiplexer
		/// </summary>
		private ConnectionMultiplexer _multiplexer;

		/// <summary>
		///     The settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The port
		/// </summary>
		private string _port = "6379";

		/// <summary>
		///     The selected message
		/// </summary>
		private ThreadMessage _selectedMessage;

		/// <summary>
		///     The server
		/// </summary>
		private string _server = "Not connected";

		/// <summary>
		///     The timer.
		/// </summary>
		private Timer _timer;

		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadMonitorViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public ThreadMonitorViewModel( IPluginSettings settings )
		{
			_dispatcher = Dispatcher.CurrentDispatcher;

			PluginSettings = settings;

			ClearCommand = new DelegateCommand( ( ) => _dispatcher.Invoke( Clear ) );
		}

		/// <summary>
		///     Gets or sets the clear command.
		/// </summary>
		/// <value>
		///     The clear command.
		/// </value>
		public ICommand ClearCommand
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
		public bool IsEnabled
		{
			get
			{
				return _isEnabled;
			}
			set
			{
				if ( value != _isEnabled )
				{
					SetProperty( ref _isEnabled, value );

					if ( _isEnabled )
					{
						_dispatcher.Invoke( ( ) => Mouse.OverrideCursor = Cursors.Wait );

						Connect( );
					}
					else
					{
						Stop( );

						Server = "Not connected";
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the messages.
		/// </summary>
		/// <value>
		///     The messages.
		/// </value>
		public ObservableCollection<ThreadMessage> Messages
		{
			get
			{
				return _messages;
			}
			set
			{
				SetProperty( ref _messages, value );
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

				Port = PluginSettings.RedisSettings.Port.ToString( CultureInfo.InvariantCulture );
			}
		}

		/// <summary>
		///     Gets the port.
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
		///     Gets or sets the selected item.
		/// </summary>
		/// <value>
		///     The selected item.
		/// </value>
		public ThreadMessage SelectedItem
		{
			get
			{
				return _selectedMessage;
			}
			set
			{
				SetProperty( ref _selectedMessage, value );
			}
		}

		/// <summary>
		///     Gets the server.
		/// </summary>
		/// <value>
		///     The server.
		/// </value>
		public string Server
		{
			get
			{
				return _server;
			}
			set
			{
				SetProperty( ref _server, value );
			}
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		private void Clear( )
		{
			_dispatcher.Invoke( ( ) => _messages.Clear( ) );
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

			Server = string.Format( "Connecting to {0}...", PluginSettings.RedisSettings.ServerName );

			string config = string.Format( "{0}:{1}", PluginSettings.RedisSettings.ServerName, PluginSettings.RedisSettings.Port );

			var task = ConnectionMultiplexer.ConnectAsync( config );

			task.ContinueWith( ConnectComplete );
		}

		/// <summary>
		///     Connects the complete.
		/// </summary>
		/// <param name="task">The task.</param>
		private void ConnectComplete( Task<ConnectionMultiplexer> task )
		{
			try
			{
				if ( !task.IsFaulted && task.Result != null )
				{
					_multiplexer = task.Result;

					var subscriber = _multiplexer.GetSubscriber( );

					subscriber.Subscribe( "ReadiNowDiagnosticResponses", OnMessage );

					Server = PluginSettings.RedisSettings.ServerName;

					if ( _dispatcher != null )
					{
						_dispatcher.Invoke( ( ) => Messages.Clear( ) );
					}

					StartTimer( );
				}
				else
				{
					Server = "Not connected";
				}
			}
			finally
			{
				if ( _dispatcher != null )
				{
					_dispatcher.Invoke( ( ) => Mouse.OverrideCursor = null );
				}
			}
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

			var threadResponse = response.Message as ThreadResponse;

			if ( threadResponse != null )
			{
				Dictionary<ThreadKey, ThreadMessage> existingMessages = _messages.ToDictionary( m => new ThreadKey( m.Message.SourceProcessId, m.Message.SourceAppDomainId, m.Message.Id ) );

				bool showUnmanagedThreads = Settings.Default.ShowUnmanagedThreads;

				_dispatcher.Invoke( ( ) =>
				{
					foreach ( ThreadInfo thread in threadResponse.Threads )
					{
						if ( !showUnmanagedThreads && thread.OsThreadId <= 0 )
						{
							continue;
						}

						ThreadMessage existingMessage;

						if ( existingMessages.TryGetValue( new ThreadKey( response.ProcessId, response.AppDomainId, thread.Id ), out existingMessage ) )
						{
							existingMessage.Message.BackGround = MessageDefault;
							existingMessage.Message.CallStack = thread.CallStack;
							existingMessage.Message.CpuUsage = thread.CpuUsage;
							existingMessage.Message.OsThreadId = thread.OsThreadId;
						}
						else
						{
							var threadData = ThreadData.FromThreadInfo( thread );
							threadData.SourceProcessId = response.ProcessId;
							threadData.SourceAppDomainId = response.AppDomainId;

							if ( string.IsNullOrEmpty( threadData.AppDomain ) )
							{
								threadData.AppDomain = response.AppDomainName;
							}

							var threadMessage = new ThreadMessage( channel, response.MachineName, response.ProcessName, response.ProcessId, response.AppDomainName, response.AppDomainId, response.Date, messageBytes.Length, response.HostTenantId, threadData );

							if ( existingMessages.Any( m => m.Key.ProcessId == response.ProcessId && m.Key.AppDomainId == response.AppDomainId ) )
							{
								threadMessage.Message.BackGround = MessageAdded;
							}

							_messages.Add( threadMessage );
						}
					}

					Dictionary<ThreadKey, ThreadInfo> newMessages = threadResponse.Threads.ToDictionary( ti => new ThreadKey( response.ProcessId, response.AppDomainId, ti.Id ) );

					foreach ( var existingMessage in existingMessages )
					{
						if ( existingMessage.Key.ProcessId == response.ProcessId && existingMessage.Key.AppDomainId == response.AppDomainId && !newMessages.ContainsKey( existingMessage.Key ) )
						{
							if ( existingMessage.Value.Message.BackGround == MessageDefault )
							{
								existingMessage.Value.Message.BackGround = MessageRemoved;
							}
							else
							{
								_messages.Remove( existingMessage.Value );
							}
						}
					}
				} );
			}
		}

		/// <summary>
		///     Settings have been updated.
		/// </summary>
		public void OnSettingsUpdate( )
		{
			if ( _timer != null )
			{
				_timer.Change( 500, Settings.Default.RefreshDuration );
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
		///     Sends the thread request.
		/// </summary>
		/// <param name="state">The state.</param>
		private void SendRequest( object state )
		{
			var subscriber = _multiplexer.GetSubscriber( );

			var request = new ThreadRequest( );

			var channelMessage = ChannelMessage<ThreadRequest>.Create( request );

			byte[ ] serializedObject = ChannelHelper.Serialize( channelMessage );
			byte[ ] compressedObject = ChannelHelper.Compress( serializedObject );

			subscriber.Publish( "ReadiNowDiagnosticRequests", compressedObject, CommandFlags.FireAndForget );
		}

		/// <summary>
		///     Starts the timer.
		/// </summary>
		private void StartTimer( )
		{
			_timer = new Timer( SendRequest, null, 500, Settings.Default.RefreshDuration );
		}

		/// <summary>
		///     Stops this instance.
		/// </summary>
		private void Stop( )
		{
			if ( _timer != null )
			{
				_timer.Dispose( );
				_timer = null;
			}

			if ( _multiplexer != null )
			{
				var subscriber = _multiplexer.GetSubscriber( );

				subscriber.UnsubscribeAll( );

				_multiplexer.Close( );

				_multiplexer.Dispose( );
				_multiplexer = null;
			}
		}
	}
}