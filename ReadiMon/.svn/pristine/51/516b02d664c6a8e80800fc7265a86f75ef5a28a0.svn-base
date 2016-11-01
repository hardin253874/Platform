// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.AddIn;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using ReadiMon.AddinView;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Graphs
{
	/// <summary>
	/// </summary>
	[AddIn( "Perf Graph Plugin", Version = "1.0.0.0" )]
	public class PerfGraphPlugin : PluginBase, IPlugin // IPlugin - explicit is necessary
	{
		private const string FeatureBrowserEmulation = @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
		// Every published build is debug, so this don't work right now
//#if DEBUG
//        private const string App = "ReadiMon.vshost.exe";
//#else
//        private const string App = "ReadiMon.exe";
//#endif

		private readonly object _syncRoot = new object( );
		private TaskbarIcon _notifyIcon;
		private int _popupTimeout = 7500;
		private PerfGraph _userInterface;

		/// <summary>
		/// </summary>
		public PerfGraphPlugin( )
		{
			SectionOrdinal = 7;
			SectionName = "Graphs";
			EntryName = "Perf Graph";
			EntryOrdinal = 0;
			HasOptionsUserInterface = false;
			HasUserInterface = true;
		}

		private PerfGraphPopupViewModel PopupViewModel
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
			return _userInterface ?? ( _userInterface = new PerfGraph( Settings ) );
		}

		/// <summary>
		///     Called when the plugin first initializes.
		/// </summary>
		public override void OnStartup( )
		{
			base.OnStartup( );

			_notifyIcon = new TaskbarIcon
			{
				ToolTipText = "PerfGraph",
				Visibility = Visibility.Collapsed
			};
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public override void OnStartupComplete( )
		{
			base.OnStartupComplete( );

			using ( var key = Registry.CurrentUser.OpenSubKey( FeatureBrowserEmulation, true ) ?? Registry.CurrentUser.CreateSubKey( FeatureBrowserEmulation ) )
			{
				if ( key != null )
				{
					//key.SetValue(App, 9000, RegistryValueKind.DWord);
					// TODO:REM
					key.SetValue( "ReadiMon.vshost.exe", 9000, RegistryValueKind.DWord );
					key.SetValue( "ReadiMon.exe", 9000, RegistryValueKind.DWord );
				}
			}
		}

		/// <summary>
		///     Called when the plugin shuts down.
		/// </summary>
		public override void OnShutdown( )
		{
			using ( var key = Registry.CurrentUser.OpenSubKey( FeatureBrowserEmulation, true ) )
			{
				if ( key != null )
				{
					var valueNames = key.GetValueNames( ).ToList( );

					if ( valueNames.Contains( "ReadiMon.vshost.exe" ) )
					{
						key.DeleteValue( "ReadiMon.vshost.exe" );
					}

					if ( valueNames.Contains( "ReadiMon.exe" ) )
					{
						key.DeleteValue( "ReadiMon.exe" );
					}
				}
			}

			if ( _notifyIcon != null )
			{
				_notifyIcon.Dispose( );
			}

			base.OnShutdown( );
		}

		/// <summary>
		///     Receives the message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public override bool OnMessageReceived( string message )
		{
			if ( GetUserInterface( ) != null )
			{
				var deserializeObject = Serializer.DeserializeObject<object>( message );

				var perfMessage = deserializeObject as PerfGraphMessage;
				if ( perfMessage != null )
				{
					if ( ShowPopup( perfMessage ) )
					{
						return true;
					}
				}

				var balloonSettingsMessage = deserializeObject as BalloonSettingsMessage;
				if ( balloonSettingsMessage != null )
				{
					_popupTimeout = balloonSettingsMessage.BalloonTimeout;
					return true;
				}
			}

			return false;
		}

		private void popup_Closed( object sender, EventArgs e )
		{
			var popup = sender as PerfGraphPopup;
			if ( popup != null )
			{
				popup.Closed -= popup_Closed;
			}

			if ( PopupViewModel != null )
			{
				var perfGraphPopup = new PerfGraphPopup
				{
					DataContext = PopupViewModel
				};

				PopupViewModel = null;

				_notifyIcon.ShowCustomBalloon( perfGraphPopup, PopupAnimation.Scroll, _popupTimeout );
			}
		}

		private void popupViewModel_ShowPerfLog( object sender, string e )
		{
			var perfGraphViewModel = _userInterface.DataContext as PerfGraphViewModel;

			if ( perfGraphViewModel != null )
			{
				perfGraphViewModel.Logs = e;
				perfGraphViewModel.ResetCommand.Execute( null );
			}

			Settings.Channel.SendMessage( new RestoreUiMessage( false )
			{
				Section = "Graphs",
				Entry = "Perf Graph"
			}.ToString( ) );
		}

		private bool ShowPopup( PerfGraphMessage perfMessage )
		{
			lock ( _syncRoot )
			{
				var viewModel = _userInterface.DataContext as PerfGraphViewModel;
				if ( viewModel != null )
				{
					PerfGraphPopup popup = null;
					if ( _notifyIcon.CustomBalloon != null && _notifyIcon.CustomBalloon.Child != null )
					{
						popup = _notifyIcon.CustomBalloon.Child as PerfGraphPopup;
					}

					var popupViewModel = new PerfGraphPopupViewModel
					{
						Message = perfMessage.Content
					};

					popupViewModel.ShowPerfLog += popupViewModel_ShowPerfLog;

					if ( popup != null )
					{
						PopupViewModel = popupViewModel;

						popup.Closed += popup_Closed;

						_notifyIcon.CloseBalloon( );

						return true;
					}

					var perfGraphPopup = new PerfGraphPopup
					{
						DataContext = popupViewModel
					};

					_notifyIcon.ShowCustomBalloon( perfGraphPopup, PopupAnimation.Scroll, _popupTimeout );

					return true;
				}
			}

			return false;
		}
	}
}