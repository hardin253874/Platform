// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Plugin.Entity.Diagnostics;
using ReadiMon.Shared;
using ReadiMon.Shared.Contracts;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;
using ReadiMon.Shared.Diagnostics.Response;
using StackExchange.Redis;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Workflow monitor view model.
	/// </summary>
	public class WorkflowMonitorViewModel : ViewModelBase
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
		///     The id map
		/// </summary>
		private readonly Dictionary<long, Workflow> _idMap = new Dictionary<long, Workflow>( );

		/// <summary>
		///     The task map
		/// </summary>
		private readonly Dictionary<string, Workflow> _taskMap = new Dictionary<string, Workflow>( );

		/// <summary>
		///     Whether capturing of Pub/Sub messages is enabled.
		/// </summary>
		private bool _isEnabled;

		/// <summary>
		///     The messages
		/// </summary>
		private ObservableCollection<Workflow> _messages = new ObservableCollection<Workflow>( );

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
		private Workflow _selectedMessage;

		/// <summary>
		///     The server
		/// </summary>
		private string _server = "Not connected";

		/// <summary>
		///     The timer.
		/// </summary>
		private Timer _timer;

		/// <summary>
		///     Initializes a new instance of the <see cref="WorkflowMonitorViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public WorkflowMonitorViewModel( IPluginSettings settings )
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
		public ObservableCollection<Workflow> Messages
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
		public Workflow SelectedItem
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
		///     Called when [message].
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="message">The message.</param>
		private void OnMessage( RedisChannel channel, RedisValue message )
		{
			byte[ ] messageBytes = ChannelHelper.Decompress( message );

			var response = ChannelHelper.Deserialize<ChannelMessage<DiagnosticResponse>>( messageBytes );

			var workflowResponse = response.Message as WorkflowResponse;

			if ( workflowResponse != null )
			{
				Workflow wf;

				workflowResponse.Status = workflowResponse.Status.Replace( "WorkflowRun", "" );

				if ( ! string.IsNullOrEmpty( workflowResponse.TaskId ) && _taskMap.TryGetValue( workflowResponse.TaskId, out wf ) )
				{
					if ( wf.Id != workflowResponse.Id )
					{
						_idMap.Remove( wf.Id );
					}

					wf.Id = workflowResponse.Id;
					wf.InstanceName = workflowResponse.WorkflowRunName;
					wf.WorkflowName = workflowResponse.WorkflowName;
					wf.Date = workflowResponse.Date;
					wf.TriggeredBy = workflowResponse.TriggeredBy;
					wf.Status = workflowResponse.Status;
					wf.TaskId = workflowResponse.TaskId;
                    wf.Server = workflowResponse.Server;
                    wf.Process = workflowResponse.Process;

                    if ( workflowResponse.Status == "Completed" )
					{
						_taskMap.Remove( workflowResponse.TaskId );
						wf.CompletedAt = DateTime.UtcNow;

						if ( !Settings.Default.WorkflowShowCompletedRuns )
						{
							_dispatcher.Invoke( ( ) => _messages.Remove( wf ) );
						}
					}
				}
				else
				{
					if ( _idMap.TryGetValue( workflowResponse.Id, out wf ) )
					{
						wf.Id = workflowResponse.Id;
						wf.InstanceName = workflowResponse.WorkflowRunName;
						wf.WorkflowName = workflowResponse.WorkflowName;
						wf.Date = workflowResponse.Date;
						wf.TriggeredBy = workflowResponse.TriggeredBy;
						wf.Status = workflowResponse.Status;
						wf.TaskId = workflowResponse.TaskId;
                        wf.Server = workflowResponse.Server;
                        wf.Process = workflowResponse.Process;
                    }
					else
					{
						wf = new Workflow
						{
							Id = workflowResponse.Id,
							InstanceName = workflowResponse.WorkflowRunName,
							WorkflowName = workflowResponse.WorkflowName,
							Date = workflowResponse.Date,
							TriggeredBy = workflowResponse.TriggeredBy,
							Status = workflowResponse.Status,
							TaskId = workflowResponse.TaskId,
                            Server = workflowResponse.Server,
                            Process = workflowResponse.Process
                        };

						if ( !Settings.Default.WorkflowShowCompletedRuns && wf.Status == "Completed" )
						{
							_dispatcher.Invoke( ( ) => _messages.Remove( wf ) );
						}
						else
						{
							_dispatcher.Invoke( ( ) => _messages.Insert( 0, wf ) );

							if ( ! string.IsNullOrEmpty( wf.TaskId ) )
							{
								_taskMap[ wf.TaskId ] = wf;
							}

							_idMap[ wf.Id ] = wf;
						}
					}
				}
			}
		}

		/// <summary>
		///     Settings have been updated.
		/// </summary>
		public void OnSettingsUpdate( )
		{
			if ( _timer != null )
			{
				_timer.Change( 500, Settings.Default.WorkflowRefreshDuration );
			}
		}

		/// <summary>
		///     Called when shutting down.
		/// </summary>
		public void OnShutdown( )
		{
			Stop( );
		}

		private void Query( )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"-- ReadiMon - GetWorkflows
SET NOCOUNT ON

DECLARE @tenantId BIGINT

DECLARE cur CURSOR FORWARD_ONLY FOR
SELECT 0
UNION
SELECT Id FROM _vTenant

OPEN cur

FETCH NEXT FROM cur
INTO @tenantId

WHILE @@FETCH_STATUS = 0
BEGIN

	DECLARE @tenantName NVARCHAR(MAX) = ISNULL( dbo.fnNameAlias( @tenantId, 0 ), 'Global' )
	DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
	DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
	DECLARE @workflowRun BIGINT = dbo.fnAliasNsId( 'workflowRun', 'core', @tenantId )
	DECLARE @runCompletedAt BIGINT = dbo.fnAliasNsId( 'runCompletedAt', 'core', @tenantId )
	DECLARE @workflowRunStatus BIGINT = dbo.fnAliasNsId( 'workflowRunStatus', 'core', @tenantId )
	DECLARE @workflowBeingRun BIGINT = dbo.fnAliasNsId( 'workflowBeingRun', 'core', @tenantId )
	DECLARE @triggeringUser BIGINT = dbo.fnAliasNsId( 'triggeringUser', 'core', @tenantId )

	SELECT
		TenantName = @tenantName,
		Id = r.FromId,
		RunName = wfrn.Data,
		WorkflowName = wfbrn.Data,
		CompletedAt = d.Data,
		[Status] = sn.Data,
		TriggeringUser = tun.Data
	FROM
		Relationship r
	LEFT JOIN
		Data_NVarChar wfrn ON
			wfrn.TenantId = r.TenantId
			AND wfrn.EntityId = r.FromId
			AND wfrn.FieldId = @name
	LEFT JOIN
		Relationship wfbr ON
			wfbr.TenantId = r.TenantId 
			AND wfbr.FromId = r.FromId
			AND wfbr.TypeId = @workflowBeingRun
	LEFT JOIN
		Data_NVarChar wfbrn ON
			wfbrn.TenantId = wfbr.TenantId
			AND wfbrn.EntityId = wfbr.ToId
			AND wfbrn.FieldId = @name
	LEFT JOIN
		Data_DateTime d ON
			d.EntityId = r.FromId
			AND d.TenantId = r.TenantId
			AND d.FieldId = @runCompletedAt
	LEFT JOIN
		Relationship s ON
			s.TenantId = r.TenantId 
			AND s.FromId = r.FromId
			AND s.TypeId = @workflowRunStatus
	LEFT JOIN
		Data_NVarChar sn ON
			sn.TenantId = s.TenantId
			AND sn.EntityId = s.ToId
			AND sn.FieldId = @name
	LEFT JOIN
		Relationship tu ON
			tu.TenantId = r.TenantId 
			AND tu.FromId = r.FromId
			AND tu.TypeId = @triggeringUser
	LEFT JOIN
		Data_NVarChar tun ON
			tun.TenantId = tu.TenantId
			AND tun.EntityId = tu.ToId
			AND tun.FieldId = @name
	WHERE
		r.TenantId = @tenantId
		AND r.TypeId = @isOfType
		AND r.ToId = @workflowRun
	ORDER BY
		d.Data DESC

	FETCH NEXT FROM cur 
	INTO @tenantId
END 
CLOSE cur;
DEALLOCATE cur;";

			try
			{
				using ( IDbCommand command = databaseManager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var workflows = new List<Workflow>( );

						do
						{
							while ( reader.Read( ) )
							{
								var tenantName = reader.GetString( 0 );
								var runId = reader.GetInt64( 1 );
								var runName = reader.GetString( 2, "" );
								var workflowName = reader.GetString( 3, "" );
								var completedAt = reader.GetDateTime( 4 );
								var status = reader.GetString( 5, "" );
								var triggeringUser = reader.GetString( 6, "" );

								if ( !Settings.Default.WorkflowShowCompletedRuns && status == "Completed" )
								{
									continue;
								}

								Workflow workflow;
								if ( !_idMap.TryGetValue( runId, out workflow ) )
								{
									workflow = new Workflow( );

									workflows.Add( workflow );

									_idMap[ runId ] = workflow;
								}

								workflow.TenantName = tenantName;
								workflow.Id = runId;
								workflow.InstanceName = runName;
								workflow.WorkflowName = workflowName;
								workflow.CompletedAt = completedAt;
								workflow.Date = completedAt;
								workflow.Status = status;
								workflow.TriggeredBy = triggeringUser;
							}
						}
						while ( reader.NextResult( ) );

						_dispatcher.Invoke( ( ) =>
						{
							foreach ( Workflow wf in workflows )
							{
								_messages.Add( wf );
							}
						} );
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Sends the thread request.
		/// </summary>
		private void SendRequest( )
		{
			if ( _multiplexer != null )
			{
				var subscriber = _multiplexer.GetSubscriber( );

				var request = new WorkflowRequest
				{
					Enabled = IsEnabled
				};

				var channelMessage = ChannelMessage<WorkflowRequest>.Create( request );

				byte[ ] serializedObject = ChannelHelper.Serialize( channelMessage );
				byte[ ] compressedObject = ChannelHelper.Compress( serializedObject );

				subscriber.Publish( "ReadiNowDiagnosticRequests", compressedObject, CommandFlags.FireAndForget );
			}
		}

		/// <summary>
		///     Starts the timer.
		/// </summary>
		private void StartTimer( )
		{
			_timer = new Timer( Update, null, 500, Settings.Default.WorkflowRefreshDuration );
		}

		/// <summary>
		///     Stops this instance.
		/// </summary>
		private void Stop( )
		{
			SendRequest( );

			if ( _timer != null )
			{
				_timer.Dispose( );
				_timer = null;
			}

			if ( _multiplexer != null )
			{
				_multiplexer.Close( );

				_multiplexer.Dispose( );
				_multiplexer = null;
			}
		}

		/// <summary>
		///     Called when a message is received.
		/// </summary>
		private void Update( object state )
		{
			SendRequest( );

			Query( );
		}
	}
}