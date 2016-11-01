// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using ReadiMon.Plugin.Redis.Contracts;
using ReadiMon.Plugin.Redis.Diagnostics;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Diagnostics.Response;
using StackExchange.Redis;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     The RemoteExecViewModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class RemoteExecViewModel : ViewModelBase
	{
		private const string InitialScript = @"using System;
using System.Collections.Generic;
// Add more usings here
// Add any assemblies references by adding one line per file reference of the form://ref: <filePath>

public static class RemoteExecutor
{
	public static List<Tuple<string,string>> Execute()
	{   
        var results = new List<Tuple<string,string>>();

		// Add your code here

        return results;
	}
}

// Define other methods and classes here";

		/// <summary>
		///     The remote execute enabled string.
		/// </summary>
		private const string RemoteExecEnabled = "RemoteExecEnabled";


		/// <summary>
		///     The dispatcher
		/// </summary>
		private readonly Dispatcher _dispatcher;


		/// <summary>
		///     Whether capturing of Pub/Sub messages is enabled.
		/// </summary>
		private bool _isEnabled;


		/// <summary>
		///     The exec data
		/// </summary>
		private ObservableCollection<RemoteExecMessage> _messages = new ObservableCollection<RemoteExecMessage>( );


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
		///     Whether the registry was enabled by the plugin.
		/// </summary>
		private bool _registryEnabled;


		/// <summary>
		///     The server
		/// </summary>
		private string _server = "Not connected";


		/// <summary>
		///     The dns name of the target.
		/// </summary>
		private string _targetDnsName;


		/// <summary>
		///     Initializes a new instance of the <see cref="RemoteExecViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public RemoteExecViewModel( IPluginSettings settings )
		{
			_dispatcher = Dispatcher.CurrentDispatcher;

			PluginSettings = settings;

			ClearCommand = new DelegateCommand( ( ) => _dispatcher.Invoke( Clear ) );
			ExecuteCommand = new DelegateCommand( ( ) => _dispatcher.Invoke( SendRequest ) );
			ResetCommand = new DelegateCommand( ( ) => _dispatcher.Invoke( Reset ) );

			Document = new TextDocument
			{
				Text = InitialScript
			};

			Document.UndoStack.ClearAll( );
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
		///     Gets or sets the document.
		/// </summary>
		/// <value>
		///     The document.
		/// </value>
		public TextDocument Document
		{
			get;
			set;
		}


		/// <summary>
		///     Gets of sets the execute command.
		/// </summary>
		public ICommand ExecuteCommand
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
		///     Gets or sets the remote exec message.
		/// </summary>
		/// <value>
		///     The messages.
		/// </value>
		public ObservableCollection<RemoteExecMessage> Messages
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
		///     Gets or sets the reset command.
		/// </summary>
		/// <value>
		///     The reset command.
		/// </value>
		public ICommand ResetCommand
		{
			get;
			set;
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
		///     The dns name of the target.
		/// </summary>
		public string TargetDnsName
		{
			get
			{
				return _targetDnsName;
			}
			set
			{
				SetProperty( ref _targetDnsName, value );
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

			Task<ConnectionMultiplexer> task = ConnectionMultiplexer.ConnectAsync( config );

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

					ISubscriber subscriber = _multiplexer.GetSubscriber( );

					subscriber.Subscribe( "ReadiNowDiagnosticResponses", OnMessage );

					Server = PluginSettings.RedisSettings.ServerName;

					if ( _dispatcher != null )
					{
						_dispatcher.Invoke( ( ) => Messages.Clear( ) );
					}

					try
					{
						if ( IsLocalIpAddress( PluginSettings.RedisSettings.ServerName ) )
						{
							EnableRemoveExec( );
						}
					}
					catch ( Exception exc )
					{
						PluginSettings.EventLog.WriteException( exc );
					}
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
		///     Enables the remove execute.
		/// </summary>
		private void DisableRemoteExec( )
		{
			using ( var baseKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry64 ) )
			using ( var server = baseKey.OpenSubKey( @"SOFTWARE\EDC\ReadiNow\Server", false ) )
			{
				if ( server != null )
				{
					var subKeyNames = server.GetSubKeyNames( );

					foreach ( string subKeyName in subKeyNames )
					{
						using ( var versionkey = server.OpenSubKey( subKeyName, true ) )
						{
							if ( versionkey != null )
							{
								versionkey.SetValue( RemoteExecEnabled, 0 );
							}
						}
					}
				}
			}

			_registryEnabled = false;
		}

		/// <summary>
		///     Enables the remove execute.
		/// </summary>
		private void EnableRemoveExec( )
		{
			using ( var baseKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry64 ) )
			using ( var server = baseKey.OpenSubKey( @"SOFTWARE\EDC\ReadiNow\Server", false ) )
			{
				if ( server != null )
				{
					var subKeyNames = server.GetSubKeyNames( );

					foreach ( string subKeyName in subKeyNames )
					{
						using ( var versionkey = server.OpenSubKey( subKeyName, true ) )
						{
							if ( versionkey != null )
							{
								object enabledObj = versionkey.GetValue( RemoteExecEnabled );

								bool disabled = true;

								if ( enabledObj != null )
								{
									var isEnabled = ( int ) enabledObj;

									if ( isEnabled > 0 )
									{
										disabled = false;
									}
								}

								if ( disabled )
								{
									var result = MessageBox.Show( "Remote execution is currently disabled on the target server.\nDo you wish to enable it now?", "Warning", MessageBoxButton.YesNo );

									if ( result == MessageBoxResult.Yes )
									{
										versionkey.SetValue( RemoteExecEnabled, 1 );

										_registryEnabled = true;
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Determines whether the host name is a local IP address.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <returns></returns>
		private static bool IsLocalIpAddress( string host )
		{
			try
			{
				/////
				// Get host IP addresses
				/////
				IPAddress[ ] hostIPs = Dns.GetHostAddresses( host );

				/////
				// Get local IP addresses
				/////
				IPAddress[ ] localIPs = Dns.GetHostAddresses( Dns.GetHostName( ) );

				/////
				// Test if any host IP equals to any local IP or to localhost
				/////
				foreach ( IPAddress hostIp in hostIPs )
				{
					/////
					// Is localhost
					/////
					if ( IPAddress.IsLoopback( hostIp ) )
					{
						return true;
					}

					/////
					// Is local address
					/////
					if ( localIPs.Contains( hostIp ) )
					{
						return true;
					}
				}
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( "Failed to determine if the specified server is the local host. " + exc );
			}

			return false;
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

			var execResponse = response.Message as RemoteExecResponse;

			if ( execResponse != null )
			{
				_dispatcher.Invoke( ( ) =>
				{
					foreach ( var data in execResponse.Data )
					{
						var execData = new RemoteExecData( data.Item1, data.Item2 );
						var msg = new RemoteExecMessage( channel, response.MachineName, response.ProcessName, response.ProcessId, response.AppDomainName, response.AppDomainId, response.Date, messageBytes.Length, response.HostTenantId, execData );
						_messages.Add( msg );
					}
				} );
			}
		}


		/// <summary>
		///     Settings have been updated.
		/// </summary>
		public void OnSettingsUpdate( )
		{
		}


		/// <summary>
		///     Called when shutting down.
		/// </summary>
		public void OnShutdown( )
		{
			Stop( );

			if ( _registryEnabled )
			{
				DisableRemoteExec( );
			}
		}


		/// <summary>
		///     Clears this instance.
		/// </summary>
		private void Reset( )
		{
			_dispatcher.Invoke( ( ) => Document.Text = InitialScript );
		}


		/// <summary>
		///     Sends the thread request.
		/// </summary>
		private void SendRequest( )
		{
			if ( !IsEnabled )
			{
				return;
			}

			ISubscriber subscriber = _multiplexer.GetSubscriber( );

			var request = new RemoteExecRequest
			{
				Code = Document.Text,
				Id = Guid.NewGuid( ).ToString( ),
				Target = TargetDnsName
			};

			ChannelMessage<RemoteExecRequest> channelMessage = ChannelMessage<RemoteExecRequest>.Create( request );

			byte[ ] serializedObject = ChannelHelper.Serialize( channelMessage );
			byte[ ] compressedObject = ChannelHelper.Compress( serializedObject );

			subscriber.Publish( "ReadiNowDiagnosticRequests", compressedObject, CommandFlags.FireAndForget );
		}


		/// <summary>
		///     Stops this instance.
		/// </summary>
		private void Stop( )
		{
			if ( _multiplexer != null )
			{
				ISubscriber subscriber = _multiplexer.GetSubscriber( );

				subscriber.UnsubscribeAll( );

				_multiplexer.Close( );

				_multiplexer.Dispose( );
				_multiplexer = null;
			}
		}
	}
}