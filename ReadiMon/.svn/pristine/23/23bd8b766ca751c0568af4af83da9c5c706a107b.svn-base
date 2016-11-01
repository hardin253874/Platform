// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Plugin.Redis.Contracts;
using ReadiMon.Plugin.Redis.Profiling;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using StackExchange.Redis;
using Serializer = ProtoBuf.Serializer;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     ProfilerMonitorViewModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class ProfilerMonitorViewModel : ViewModelBase
	{
		/// <summary>
		///     The dispatcher
		/// </summary>
		private readonly Dispatcher _dispatcher;

		/// <summary>
		///     The messages
		/// </summary>
		private ObservableCollection<ProfilerMessage> _messages = new ObservableCollection<ProfilerMessage>( );

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
		///     The server
		/// </summary>
		private string _server = "localhost";

		/// <summary>
		///     Initializes a new instance of the <see cref="ProfilerMonitorViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public ProfilerMonitorViewModel( IPluginSettings settings )
		{
			_dispatcher = Dispatcher.CurrentDispatcher;

			PluginSettings = settings;

			ClearCommand = new DelegateCommand( ( ) => _dispatcher.Invoke( Clear ) );

			ProfilerTraceModel = new ProfilerTraceModel( _dispatcher );
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
		///     Gets or sets the messages.
		/// </summary>
		/// <value>
		///     The messages.
		/// </value>
		public ObservableCollection<ProfilerMessage> Messages
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

				Server = PluginSettings.RedisSettings.ServerName;
				Port = PluginSettings.RedisSettings.Port.ToString( CultureInfo.InvariantCulture );

				Connect( );
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
		///     Gets or sets the profiler trace model.
		/// </summary>
		/// <value>
		///     The profiler trace model.
		/// </value>
		public ProfilerTraceModel ProfilerTraceModel
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
		///     Clears this instance.
		/// </summary>
		private void Clear( )
		{
			ProfilerTraceModel.Clear( );
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
			if ( !task.IsFaulted && task.Result != null )
			{
				_multiplexer = task.Result;

				var subscriber = _multiplexer.GetSubscriber( );

				subscriber.Subscribe( "Profiling", OnMessage );

				Server = PluginSettings.RedisSettings.ServerName;

				if ( _dispatcher != null )
				{
					_dispatcher.Invoke( ( ) => Messages.Clear( ) );
				}
			}
			else
			{
				Server = "Not connected";
			}
		}

		/// <summary>
		///     Decompresses the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		private byte[ ] Decompress( byte[ ] message )
		{
			using ( var compressedStream = new MemoryStream( message ) )
			using ( var decompressionStream = new GZipStream( compressedStream, CompressionMode.Decompress ) )
			using ( var memoryStream = new MemoryStream( ) )
			{
				decompressionStream.CopyTo( memoryStream );

				return memoryStream.ToArray( );
			}
		}

		private void Deserialize<T>( MemoryStream stream, string channel )
		{
			var obj = Serializer.Deserialize<ChannelMessage<T>>( stream );

			if ( obj != null )
			{
				var jss = new JavaScriptSerializer( );
				jss.MaxJsonLength = int.MaxValue;
				jss.RecursionLimit = int.MaxValue;

				var jsonObject = jss.Deserialize<dynamic>( obj.Message.ToString( ) ) as IDictionary<string, object>;

				if ( jsonObject != null )
				{
					var root = new ProfilerTrace( jsonObject, "web.png" );

					_dispatcher.Invoke( ( ) => ProfilerTraceModel.Add( root, Guid.Empty ) );

					var child = jsonObject[ "Root" ] as IDictionary<string, object>;

					Parse( child, root.Id );
				}
			}
		}

		private void DeserializeBinary( string channel, byte[ ] messageBytes )
		{
			try
			{
				using ( var stream = new MemoryStream( messageBytes ) )
				{
					Deserialize<string>( stream, channel );
				}
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc.Message );
			}
		}

		private void OnMessage( RedisChannel channel, RedisValue message )
		{
			byte[ ] messageBytes = Decompress( message );

			DeserializeBinary( channel, messageBytes );
		}

		private void Parse( IDictionary<string, object> obj, Guid parent )
		{
			var trace = new ProfilerTrace( obj );

			_dispatcher.Invoke( ( ) => ProfilerTraceModel.Add( trace, parent ) );

			object children;

			if ( obj.TryGetValue( "Children", out children ) )
			{
				if ( children != null )
				{
					var childrenItems = children as object[ ];

					foreach ( var childItem in childrenItems )
					{
						var childDictionary = childItem as IDictionary<string, object>;

						Parse( childDictionary, trace.Id );
					}
				}
			}

			object customTimings;
			if ( obj.TryGetValue( "CustomTimings", out customTimings ) )
			{
				if ( customTimings != null )
				{
					var timingDictionary = customTimings as IDictionary<string, object>;

					if ( timingDictionary != null )
					{
						object sqlTimings;

						if ( timingDictionary.TryGetValue( "sql", out sqlTimings ) )
						{
							if ( sqlTimings != null )
							{
								var sqlArray = sqlTimings as object[ ];

								if ( sqlArray != null )
								{
									foreach ( var sqlObj in sqlArray )
									{
										var sql = sqlObj as IDictionary<string, object>;

										var sqlTrace = new SqlProfilerTrace( sql );

										_dispatcher.Invoke( ( ) => ProfilerTraceModel.Add( sqlTrace, parent ) );
									}
								}
							}
						}

						object redisTimings;

						if ( timingDictionary.TryGetValue( "redis", out redisTimings ) )
						{
							if ( redisTimings != null )
							{
								var redisArray = redisTimings as object[ ];

								if ( redisArray != null )
								{
									foreach ( var redisObj in redisArray )
									{
										var redis = redisObj as IDictionary<string, object>;

										var sqlTrace = new RedisProfilerTrace( redis );

										_dispatcher.Invoke( ( ) => ProfilerTraceModel.Add( sqlTrace, parent ) );
									}
								}
							}
						}
					}
				}
			}
		}
	}
}