// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.AddIn;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using ReadiMon.AddinView;
using ReadiMon.AddinView.Configuration;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Entity browser plugin
	/// </summary>
	[AddIn( "Entity Browser Plugin", Version = "1.0.0.0" )]
	public class EntityBrowserPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The sync root.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     The balloon timeout
		/// </summary>
		private int _balloonTimeout = 7500;

		/// <summary>
		///     The notify icon
		/// </summary>
		private TaskbarIcon _notifyIcon;

		/// <summary>
		///     The browser
		/// </summary>
		private EntityBrowser _userInterface;

		/// <summary>
		/// Is the first process.
		/// </summary>
		private bool _isFirstProcess;

		/// <summary>
		/// The first process identifier.
		/// </summary>
		private int _firstProcessId;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityBrowserPlugin" /> class.
		/// </summary>
		public EntityBrowserPlugin( )
		{
			SectionOrdinal = 1;
			SectionName = "Entity";
			EntryName = "Entity Browser";
			EntryOrdinal = 0;
			HasOptionsUserInterface = false;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the double left click command.
		/// </summary>
		/// <value>
		///     The double left click command.
		/// </value>
		public ICommand DoubleClickCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the exit click command.
		/// </summary>
		/// <value>
		///     The exit click command.
		/// </value>
		public ICommand ExitClickCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [hide when minimized].
		/// </summary>
		/// <value>
		///     <c>true</c> if [hide when minimized]; otherwise, <c>false</c>.
		/// </value>
		private bool HideWhenMinimized
		{
			get
			{
				return GetConfigurationValue<bool>( );
			}
			set
			{
				SetConfigurationValue( value );

				Settings.Channel.SendMessage( value ? new HideWhenMinimizedMessage( ).ToString( ) : new ShowWhenMinimizedMessage( ).ToString( ) );
			}
		}

		/// <summary>
		///     Gets the hide when minimized command.
		/// </summary>
		/// <value>
		///     The hide when minimized command.
		/// </value>
		public ICommand HideWhenMinimizedCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the next view model.
		/// </summary>
		/// <value>
		///     The next view model.
		/// </value>
		private FancyBalloonViewModel NextViewModel
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new EntityBrowser( Settings ) );
		}

		/// <summary>
		///     Sends the message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public override bool OnMessageReceived( string message )
		{
			if ( GetUserInterface( ) != null )
			{
				if ( !_isFirstProcess && _firstProcessId > 0 )
				{
					try
					{
						var process = Process.GetProcessById( _firstProcessId );

						if ( process.HasExited )
						{
							_isFirstProcess = true;
							_firstProcessId = 0;
						}
					}
					catch ( Exception )
					{
						_isFirstProcess = true;
						_firstProcessId = 0;
					}
				}

				if ( _isFirstProcess )
				{
					var deserializeObject = Serializer.DeserializeObject<object>( message );

					var entityMessage = deserializeObject as EntityBrowserMessage;

					if ( entityMessage != null )
					{
						if ( ShowBalloon( entityMessage ) )
						{
							return true;
						}
					}

					var balloonSettingsMessage = deserializeObject as BalloonSettingsMessage;

					if ( balloonSettingsMessage != null )
					{
						_balloonTimeout = balloonSettingsMessage.BalloonTimeout;
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		///     Called when the plugin first initializes.
		/// </summary>
		public override void OnStartup( )
		{
			base.OnStartup( );

			InitializeNotifyIcon( );

			_notifyIcon.Visibility = Visibility.Visible;

			Settings.Channel.SendMessage( HideWhenMinimized ? new HideWhenMinimizedMessage( ).ToString( ) : new ShowWhenMinimizedMessage( ).ToString( ) );

			_isFirstProcess = IsFirstProcess( );
		}

		/// <summary>
		///     Called when the plugin shuts down.
		/// </summary>
		public override void OnShutdown( )
		{
			base.OnShutdown( );

			if ( _notifyIcon != null )
			{
				_notifyIcon.Visibility = Visibility.Collapsed;

				_notifyIcon.Dispose( );
			}
		}

		/// <summary>
		///     Checks for existing instance.
		/// </summary>
		private bool IsFirstProcess( )
		{
			var currentProcess = Process.GetCurrentProcess( );

			var name = currentProcess.ProcessName.Replace( ".vshost", "" );

			var processes = Process.GetProcesses( );

			try
			{
				foreach ( Process process in processes )
				{
					try
					{
						if ( process.Id != currentProcess.Id && process.ProcessName == name && !process.HasExited && currentProcess.StartTime > process.StartTime )
						{
							_firstProcessId = process.Id;
							return false;
						}
					}
					catch
					{
						Debug.WriteLine( "Failed to open process " + process.ProcessName );
					}
				}
			}
			catch ( Exception exc )
			{
				MessageBox.Show( exc.ToString( ) );
			}

			return true;
		}

		/// <summary>
		///     Creates the configuration section.
		/// </summary>
		/// <returns></returns>
		protected override PluginConfigurationBase CreateConfigurationSection( )
		{
			return new EntityBrowserPluginConfiguration( );
		}

		/// <summary>
		///     Doubles the click.
		/// </summary>
		private void DoubleClick( )
		{
			Settings.Channel.SendMessage( new RestoreUiMessage( false ).ToString( ) );
		}

		/// <summary>
		///     Exits the click.
		/// </summary>
		private void ExitClick( )
		{
			Settings.Channel.SendMessage( new ExitMessage( ).ToString( ) );
		}

		/// <summary>
		///     Hides the when minimized click.
		/// </summary>
		private void HideWhenMinimizedClick( MenuItem menuItem )
		{
			menuItem.IsChecked = !menuItem.IsChecked;
			HideWhenMinimized = menuItem.IsChecked;
		}

		/// <summary>
		///     Initializes the notify icon.
		/// </summary>
		private void InitializeNotifyIcon( )
		{
			DoubleClickCommand = new DelegateCommand( DoubleClick );
			ExitClickCommand = new DelegateCommand( ExitClick );
			HideWhenMinimizedCommand = new DelegateCommand<MenuItem>( HideWhenMinimizedClick );

			var menu = new ContextMenu( );

			var openMenuItem = new MenuItem
			{
				Header = "Open",
				Command = DoubleClickCommand,
				FontWeight = FontWeights.Bold
			};

			var hideMenuItem = new MenuItem
			{
				Header = "Hide When Minimized",
				IsChecked = HideWhenMinimized,
				Command = HideWhenMinimizedCommand
			};

			hideMenuItem.CommandParameter = hideMenuItem;

			var exitMenuItem = new MenuItem
			{
				Header = "Exit",
				Command = ExitClickCommand
			};

			menu.Items.Add( openMenuItem );
			menu.Items.Add( new Separator( ) );
			menu.Items.Add( hideMenuItem );
			menu.Items.Add( new Separator( ) );
			menu.Items.Add( exitMenuItem );

			_notifyIcon = new TaskbarIcon
			{
				ToolTipText = "ReadiMon",
				Icon = Resources.Resources.icon,
				DoubleClickCommand = DoubleClickCommand,
				ContextMenu = menu
			};
		}

		/// <summary>
		///     Called when updating the settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			if ( _userInterface != null )
			{
				var viewModel = _userInterface.DataContext as EntityBrowserViewModel;

				if ( viewModel != null )
				{
					viewModel.PluginSettings = Settings;
				}
			}
		}

		/// <summary>
		///     Shows the balloon.
		/// </summary>
		/// <param name="entityMessage">The entity message.</param>
		/// <returns></returns>
		private bool ShowBalloon( EntityBrowserMessage entityMessage )
		{
			lock ( _syncRoot )
			{
				var entityBrowserViewModel = _userInterface.DataContext as EntityBrowserViewModel;

				if ( entityBrowserViewModel != null )
				{
					long id;
					long tenantId;
					Guid upgradeId;
					string name;
					string description;
					string tenant;
					string solution;
					string type;

					if ( entityBrowserViewModel.GetEntityDetails( entityMessage.Entity, out id, out tenantId, out upgradeId, out name, out description, out tenant, out solution, out type ) )
					{
						if ( FancyBalloon.CurrentEntityId == id )
						{
							return true;
						}

						FancyBalloon fancyBalloon = null;

						if ( _notifyIcon.CustomBalloon != null && _notifyIcon.CustomBalloon.Child != null )
						{
							fancyBalloon = _notifyIcon.CustomBalloon.Child as FancyBalloon;
						}

						FancyBalloon.CurrentEntityId = id;

						var viewModel = new FancyBalloonViewModel
						{
							BalloonTitle = "ReadiMon Entity detected...",
							EntityId = id.ToString( CultureInfo.InvariantCulture ),
							TenantId = entityBrowserViewModel.GetTenantName( tenantId ),
							UpgradeId = upgradeId.ToString( "B" ),
							Name = name,
							Description = description,
							Tenant = tenant,
							Solution = solution,
							Type = type
						};

						viewModel.ShowEntity += viewModel_ShowEntity;

						if ( fancyBalloon != null )
						{
							NextViewModel = viewModel;

							fancyBalloon.Closed += fancyBalloon_Closed;

							_notifyIcon.CloseBalloon( );

							return true;
						}

						var balloon = new FancyBalloon
						{
							DataContext = viewModel,
							EventLog = Settings.EventLog
						};

						_notifyIcon.ShowCustomBalloon( balloon, PopupAnimation.Scroll, _balloonTimeout );

						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		///     Handles the Closed event of the fancyBalloon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void fancyBalloon_Closed( object sender, EventArgs e )
		{
			var fancyBalloon = sender as FancyBalloon;

			if ( fancyBalloon != null )
			{
				fancyBalloon.Closed -= fancyBalloon_Closed;
			}

			if ( NextViewModel != null )
			{
				var balloon = new FancyBalloon
				{
					DataContext = NextViewModel,
					EventLog = Settings.EventLog
				};

				long id;

				if ( long.TryParse( NextViewModel.EntityId, out id ) )
				{
					FancyBalloon.CurrentEntityId = id;
				}

				NextViewModel = null;

				_notifyIcon.ShowCustomBalloon( balloon, PopupAnimation.Scroll, _balloonTimeout );
			}
		}

		/// <summary>
		///     Views the model_ show entity.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void viewModel_ShowEntity( object sender, string e )
		{
			var entityBrowserViewModel = _userInterface.DataContext as EntityBrowserViewModel;

			if ( entityBrowserViewModel != null )
			{
				entityBrowserViewModel.SetEntity( e );
			}

			Settings.Channel.SendMessage( new RestoreUiMessage( true ).ToString( ) );
		}
	}
}