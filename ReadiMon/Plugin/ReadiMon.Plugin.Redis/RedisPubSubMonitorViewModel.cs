// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Plugin.Redis.Contracts;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;
using StackExchange.Redis;
using Serializer = ProtoBuf.Serializer;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Redis pub sub monitor view model.
	/// </summary>
	public class RedisPubSubMonitorViewModel : ViewModelBase
	{
		/// <summary>
		///     The payload regex
		/// </summary>
		private static readonly Regex PayloadRegex = new Regex( "^<msg><src>(?<mac><mac>.*?</mac>)(?<pro><pro>.*?</pro>)(?<proId><proId>.*?</proId>)(?<app><app>.*?</app>)(?<appId><appId>.*?</appId>)(?<dt><dt>.*?</dt>)</src>(?<dat><dat>.*?</dat>)(?<pubOrig><pubOrig>.*?</pubOrig>)</msg>$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled );

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
		private ObservableCollection<RedisMessage> _messages = new ObservableCollection<RedisMessage>( );

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
		private RedisMessage _selectedMessage;

		/// <summary>
		///     The server
		/// </summary>
		private string _server = "Not connected";

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPubSubMonitorViewModel" /> class.
		/// </summary>
		public RedisPubSubMonitorViewModel( IPluginSettings settings )
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
						Connect( );
					}
					else
					{
						if ( _multiplexer != null )
						{
							var subscriber = _multiplexer.GetSubscriber( );

							subscriber.UnsubscribeAll( );

							_multiplexer.Close( );

							_multiplexer.Dispose( );
							_multiplexer = null;
						}

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
		public ObservableCollection<RedisMessage> Messages
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
		public RedisMessage SelectedItem
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
			Messages.Clear( );

			SelectedItem = null;
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

				subscriber.Subscribe( "*", OnMessage );

				Server = PluginSettings.RedisSettings.ServerName;

				if ( _dispatcher != null )
				{
					_dispatcher.Invoke( ( ) => Messages.Clear( ) );
				}
			}
			else
			{
				IsEnabled = false;
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

		/// <summary>
		///     Deserializes the specified stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stream">The stream.</param>
		/// <param name="channel">The channel.</param>
		/// <returns></returns>
		private RedisMessage Deserialize<T>( MemoryStream stream, string channel )
		{
			var obj = Serializer.Deserialize<ChannelMessage<T>>( stream );

			if ( obj != null )
			{
				return new RedisMessage( channel, obj.MachineName, obj.ProcessName, obj.ProcessId, obj.AppDomainName, obj.AppDomainId, obj.Date, stream.Length, obj.HostTenantId, obj.Message.ToString( ) );
			}

			return null;
		}

		/// <summary>
		///     Gets the encoded regex group argument.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="match">The match.</param>
		/// <param name="groupName">Name of the group.</param>
		/// <returns></returns>
		private static T GetEncodedRegexGroupArgument<T>( Match match, string groupName )
		{
			string decoded = null;

			string encoded = match.Groups[ groupName ].Value;

			if ( !string.IsNullOrEmpty( encoded ) )
			{
				SecurityElement element = SecurityElement.FromString( encoded );

				if ( element != null )
				{
					decoded = element.Text;
				}
			}

			/////
			// Convert the value to the return type
			/////
			var converter = TypeDescriptor.GetConverter( typeof ( T ) );

			return ( T ) converter.ConvertFromString( decoded );
		}

		/// <summary>
		///     Gets the entity details.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="upgradeId">The upgrade identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="tenant">The tenant.</param>
		/// <param name="solution">The solution.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public bool GetEntityDetails( long id, out long tenantId, out Guid upgradeId, out string name, out string description, out string tenant, out string solution, out string type )
		{
			tenantId = -1;
			upgradeId = Guid.Empty;
			name = null;
			description = null;
			tenant = null;
			solution = null;
			type = null;

			if ( id < 0 )
			{
				return false;
			}

			const string commandText = @"--ReadiMon - GetEntityDetails
DECLARE @tenantId BIGINT

SELECT @tenantId = TenantId FROM Entity WHERE Id = @id

DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )

SELECT
	e.Id,
	e.TenantId,
	e.UpgradeId,
	Name = dbo.fnName( @id ),
	Description = dbo.fnFieldNVarChar( @id, @description ),
	Tenant = dbo.fnName( e.TenantId ),
	Solution = dbo.fnNameAlias( r.ToId, r.TenantId ),
	Type = dbo.fnNameAlias( t.ToId, t.TenantId )
FROM
	Entity e
LEFT JOIN
	Relationship r ON
		r.TenantId = e.TenantId
		AND r.FromId = e.Id
		AND r.TypeId = @inSolution
LEFT JOIN
	Relationship t ON
		t.TenantId = e.TenantId
		AND t.FromId = e.Id
		AND t.TypeId = @isOfType
WHERE
	e.Id = @id
	AND e.TenantId = @tenantId
";

			try
			{
				var manager = new DatabaseManager( PluginSettings.DatabaseSettings );

				using ( var command = manager.CreateCommand( commandText ) )
				{
					manager.AddParameter( command, "@id", id );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						if ( reader.Read( ) )
						{
							tenantId = reader.GetInt64( 1 );
							upgradeId = reader.GetGuid( 2 );
							name = reader.GetString( 3, "<Unnamed>", "<Unnamed>" );
							description = reader.GetString( 4, string.Empty );
							tenant = reader.GetString( 5, "<Unnamed>" );
							solution = reader.GetString( 6, "<Unnamed>" );
							type = reader.GetString( 7, "<Unnamed>" );

							return true;
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			return false;
		}

		/// <summary>
		///     Gets the entity identifier by alias.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="enforceTenantId">if set to <c>true</c> [enforce tenant identifier].</param>
		/// <returns></returns>
		protected long GetEntityIdByAlias( string value, bool enforceTenantId = false )
		{
			string nameSpace;
			string alias;

			if ( string.IsNullOrWhiteSpace( value ) )
			{
				return -1;
			}

			int lineFeed = value.IndexOf( '\n' );

			if ( lineFeed > 0 )
			{
				value = value.Substring( 0, lineFeed - 1 );
			}

			if ( value.GetNamespaceAlias( out nameSpace, out alias ) )
			{
				string commandText = enforceTenantId ? @"--ReadiMon - GetEntityIdByAlias
SELECT TOP 1 EntityId FROM Data_Alias WHERE Data = @alias AND Namespace = @namespace AND TenantId = @tenantId" : @"--ReadiMon - GetEntityIdByAlias
SELECT TOP 1 EntityId FROM Data_Alias WHERE Data = @alias AND Namespace = @namespace";

				var manager = new DatabaseManager( PluginSettings.DatabaseSettings );

				try
				{
					using ( var command = manager.CreateCommand( commandText ) )
					{
						manager.AddParameter( command, "@alias", alias );
						manager.AddParameter( command, "@namespace", nameSpace );

						var scalar = command.ExecuteScalar( );

						if ( scalar != null && scalar != DBNull.Value )
						{
							var id = ( long ) scalar;

							return id;
						}
					}
				}
				catch ( Exception exc )
				{
					PluginSettings.EventLog.WriteException( exc );
				}
			}

			return -1;
		}

		/// <summary>
		///     Gets the entity identifier by unique identifier.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="enforceTenantId">if set to <c>true</c> [enforce tenant identifier].</param>
		/// <returns></returns>
		public long GetEntityIdByGuid( Guid value, bool enforceTenantId = false )
		{
			string commandText = enforceTenantId ? @"--ReadiMon - GetEntityIdByGuid
SELECT TOP 1 Id FROM Entity WHERE UpgradeId = @guid AND TenantId = @tenantId" : @"--ReadiMon - GetEntityIdByGuid
SELECT TOP 1 Id FROM Entity WHERE UpgradeId = @guid";

			try
			{
				var manager = new DatabaseManager( PluginSettings.DatabaseSettings );

				using ( var command = manager.CreateCommand( commandText ) )
				{
					manager.AddParameter( command, "@guid", value );

					var scalar = command.ExecuteScalar( );

					if ( scalar != null && scalar != DBNull.Value )
					{
						var id = ( long ) scalar;

						return id;
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			return -1;
		}

		/// <summary>
		///     Determines whether the specified identifier is entity.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="details">The details.</param>
		/// <returns></returns>
		public bool IsEntity( string identifier, out string details )
		{
			bool valid = false;
			details = string.Empty;

			if ( !string.IsNullOrEmpty( identifier ) )
			{
				long id;

				Guid guid;

				if ( Guid.TryParse( identifier, out guid ) )
				{
					id = GetEntityIdByGuid( guid );
				}
				else
				{
					if ( !long.TryParse( identifier, out id ) )
					{
						id = GetEntityIdByAlias( identifier );
					}
				}

				valid = id >= 0;

				if ( valid )
				{
					long tenantId;
					Guid upgradeId;
					string name;
					string description;
					string tenant;
					string solution;
					string type;

					if ( GetEntityDetails( id, out tenantId, out upgradeId, out name, out description, out tenant, out solution, out type ) )
					{
						details = string.Format( "Entity Id: {0}\nName: {1}\nDescription: {2}\nType: {3}\nTenant: {4}\nSolution: {5}", id, name, description, type, tenant, solution );
					}
					else
					{
						valid = false;
					}
				}
			}

			return valid;
		}

		/// <summary>
		///     Called when [message].
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="message">The message.</param>
		private void OnMessage( RedisChannel channel, RedisValue message )
		{
			byte[ ] messageBytes = Decompress( message );

			RedisMessage msg;

			if ( TryDeserializeXml( channel, messageBytes, out msg ) )
			{
			}
			else if ( TryDeserializeBinary( channel, messageBytes, out msg ) )
			{
			}
			else
			{
				msg = new RedisMessage( channel, "Unknown", "Unknown", 0, "Unknown", 0, DateTime.MinValue, 0, -1, "Message format not recognised." );
			}

			float originalLength = ( ( byte[ ] ) message ).Length;
			float decompressedLength = messageBytes.Length;

			float savings = 1 - ( originalLength / decompressedLength );

			msg.CompressionRate = savings.ToString( "0.00%" );
			msg.TransmissionSize = ( ( byte[ ] ) message ).Length;

			_dispatcher.Invoke( ( ) => _messages.Add( msg ) );
		}

		/// <summary>
		///     Tries the deserialize binary.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="messageBytes">The message bytes.</param>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		private bool TryDeserializeBinary( string channel, byte[ ] messageBytes, out RedisMessage message )
		{
			try
			{
				using ( var stream = new MemoryStream( messageBytes ) )
				{
					switch ( channel )
					{
						case "Entity":
							message = Deserialize<EntityCacheMessage>( stream, channel );
							return true;
						case "EntityField":
							message = Deserialize<EntityFieldCacheMessage>( stream, channel );
							return true;
						case "FieldEntity":
							message = Deserialize<FieldEntityCacheMessage>( stream, channel );
							return true;
						case "EntityRelationship":
							message = Deserialize<EntityRelationshipCacheMessage>( stream, channel );
							return true;
						case "Profiling":
							message = Deserialize<string>( stream, channel );
							return true;
					}
				}
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc.Message );
			}

			message = null;
			return false;
		}

		/// <summary>
		///     Tries the deserialize XML.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="messageBytes">The message bytes.</param>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		private bool TryDeserializeXml( string channel, byte[ ] messageBytes, out RedisMessage message )
		{
			string messageText = Encoding.UTF8.GetString( messageBytes );

			Match match = PayloadRegex.Match( messageText );

			if ( match.Success )
			{
				message = new RedisMessage( channel, GetEncodedRegexGroupArgument<string>( match, "mac" ), GetEncodedRegexGroupArgument<string>( match, "pro" ), GetEncodedRegexGroupArgument<int>( match, "proId" ), GetEncodedRegexGroupArgument<string>( match, "app" ), GetEncodedRegexGroupArgument<int>( match, "appId" ), new DateTime( GetEncodedRegexGroupArgument<long>( match, "dt" ) ), messageText.Length, -1, GetEncodedRegexGroupArgument<string>( match, "dat" ) );
				return true;
			}

			message = null;
			return false;
		}
	}
}