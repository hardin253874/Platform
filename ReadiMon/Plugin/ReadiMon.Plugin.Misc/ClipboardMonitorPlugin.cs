// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.AddIn;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ReadiMon.AddinView;
using ReadiMon.AddinView.Configuration;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Misc
{
	/// <summary>
	///     Clipboard Monitor
	/// </summary>
	[AddIn( "Clipboard Monitor Plugin", Version = "1.0.0.0" )]
	public class ClipboardMonitorPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     Next clipboard viewer window
		/// </summary>
		private IntPtr _hWndNextViewer;

		/// <summary>
		///     The <see cref="HwndSource" /> for this window.
		/// </summary>
		private HwndSource _hWndSource;

		/// <summary>
		///     The options view model.
		/// </summary>
		private ClipboardMonitorOptionsViewModel _optionsViewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="ClipboardMonitorPlugin" /> class.
		/// </summary>
		public ClipboardMonitorPlugin( )
		{
			SectionName = "Miscellaneous";
			EntryName = "Clipboard Monitor";
			HasOptionsUserInterface = true;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [monitor clipboard].
		/// </summary>
		/// <value>
		///     <c>true</c> if [monitor clipboard]; otherwise, <c>false</c>.
		/// </value>
		public bool MonitorClipboard
		{
			get
			{
				return GetConfigurationValue<bool>( );
			}
			set
			{
				SetConfigurationValue( value );

				if ( value )
				{
					InitializeClipboardMonitor( );
				}
				else
				{
					ShutdownClipboardMonitor( );
				}
			}
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <value>
		///     The options user interface.
		/// </value>
		private ClipboardMonitorOptionsViewModel OptionsViewModel
		{
			get
			{
				return _optionsViewModel ?? ( _optionsViewModel = new ClipboardMonitorOptionsViewModel( ) );
			}
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetOptionsUserInterface( )
		{
			return new ClipboardMonitorOptions( OptionsViewModel );
		}

		/// <summary>
		///     Saves the options.
		/// </summary>
		public override void SaveOptions( )
		{
			if ( _optionsViewModel != null )
			{
				int oldBalloonTimeout = Misc.Settings.Default.BalloonTimeout;

				_optionsViewModel.OnSave( );

				int newBalloonTimeout = Misc.Settings.Default.BalloonTimeout;

				if ( newBalloonTimeout != oldBalloonTimeout )
				{
					Settings.Channel.SendMessage( new BalloonSettingsMessage( newBalloonTimeout ).ToString( ) );
				}
			}
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public override void OnStartupComplete( )
		{
			Settings.Channel.SendMessage( new BalloonSettingsMessage( Misc.Settings.Default.BalloonTimeout ).ToString( ) );
		}

		/// <summary>
		///     Called when the plugin first initializes.
		/// </summary>
		public override void OnStartup( )
		{
			base.OnStartup( );
			InitializeClipboardMonitor( );
		}

		/// <summary>
		///     Called when the plugin shuts down.
		/// </summary>
		public override void OnShutdown( )
		{
			base.OnShutdown( );
			ShutdownClipboardMonitor( );
		}

		/// <summary>
		///     Gets the tool bar.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetToolBar( )
		{
			var toolBar = new ToolBar( );

			var monitorClipboardButton = new ToggleButton
			{
				Content = new Image
				{
					Source = new BitmapImage( new Uri( @"pack://application:,,,/ReadiMon.Plugin.Misc;component/Resources/viewmag.png" ) ),
					Height = 16,
					Width = 16
				},
				ToolTip = "Monitor Clipboard",
				IsThreeState = false,
				Focusable = false,
				IsChecked = MonitorClipboard,
				Command = new DelegateCommand<ToggleButton>( MonitorClipboardClick )
			};

			monitorClipboardButton.CommandParameter = monitorClipboardButton;

			toolBar.Items.Add( monitorClipboardButton );

			return toolBar;
		}

		/// <summary>
		///     Creates the configuration section.
		/// </summary>
		/// <returns></returns>
		protected override PluginConfigurationBase CreateConfigurationSection( )
		{
			return new ClipboardMonitorPluginConfiguration( );
		}

		/// <summary>
		///     Draws the content.
		/// </summary>
		private void DrawContent( )
		{
			RetryHandler.Retry( ( ) =>
			{
				if ( Clipboard.ContainsText( ) )
				{
					string text = Clipboard.GetText( );

					if ( ! string.IsNullOrEmpty( text ) )
					{
						Guid guid;
						if ( Guid.TryParse( text, out guid ) && !Misc.Settings.Default.MonitorGuid )
						{
							return;
						}

						long id;
						if ( long.TryParse( text, out id ) && !Misc.Settings.Default.MonitorLong )
						{
							return;
						}
                        
						if ( !Misc.Settings.Default.MonitorAlias && !Misc.Settings.Default.MonitorPerfLog )
						{
							return;
						}

					    var msg  = IsPerfLog(text) ? new PerfGraphMessage(text).ToString() : new EntityBrowserMessage(text).ToString();

                        Settings.Channel.SendMessage(msg);
					}
				}
			}, exceptionHandler: e => Settings.EventLog.WriteException( e ) );
		}

        /// <summary>
        ///     Checks if the text resembles a perf log.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>True if this is a perf log.</returns>
	    private static bool IsPerfLog(string text)
	    {
	        if (string.IsNullOrEmpty(text))
	            return false;

            var n = text.IndexOf('{');
            if (n >= 0)
            {
                return ((text.IndexOf("label:", n, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                        (text.IndexOf("\"label\":", n, StringComparison.InvariantCultureIgnoreCase) >= 0)) &&
                        ((text.IndexOf("start:", n, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                        ((text.IndexOf("\"start\":", n, StringComparison.InvariantCultureIgnoreCase) >= 0)));
            }

            return false;
	    }

		/// <summary>
		///     Initializes the clipboard monitor.
		/// </summary>
		private void InitializeClipboardMonitor( )
		{
			Monitor( );
		}

		/// <summary>
		///     Monitors this instance.
		/// </summary>
		private void Monitor( )
		{
			var window = new Window
			{
				Title = "ReadiMon Clipboard Monitor",
				Width = 0,
				Height = 0,
				WindowStyle = WindowStyle.None,
				ShowInTaskbar = false,
				ShowActivated = false,
				ResizeMode = ResizeMode.NoResize
			};

			window.Show( );
			window.Hide( );

			var wih = new WindowInteropHelper( window );
			_hWndSource = HwndSource.FromHwnd( wih.Handle );

			if ( _hWndSource != null )
			{
				_hWndSource.AddHook( WinProc ); // start processing window messages
				_hWndNextViewer = Win32.SetClipboardViewer( _hWndSource.Handle ); // set this window as a viewer
			}
		}

		/// <summary>
		///     Monitors the clipboard click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void MonitorClipboardClick( ToggleButton obj )
		{
			MonitorClipboard = obj.IsChecked ?? true;
		}

		/// <summary>
		///     Shutdowns the clipboard monitor.
		/// </summary>
		private void ShutdownClipboardMonitor( )
		{
			StopMonitoring( );
		}

		/// <summary>
		///     Stops the monitoring.
		/// </summary>
		private void StopMonitoring( )
		{
			/////
			// remove this window from the clipboard viewer chain
			/////
			if ( _hWndSource != null )
			{
				Win32.ChangeClipboardChain( _hWndSource.Handle, _hWndNextViewer );

				_hWndNextViewer = IntPtr.Zero;
				_hWndSource.RemoveHook( WinProc );
			}
		}

		/// <summary>
		///     WinProc.
		/// </summary>
		/// <param name="hwnd">The HWND.</param>
		/// <param name="msg">The MSG.</param>
		/// <param name="wParam">The w parameter.</param>
		/// <param name="lParam">The l parameter.</param>
		/// <param name="handled">if set to <c>true</c> [handled].</param>
		/// <returns></returns>
		private IntPtr WinProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
		{
			switch ( msg )
			{
				case Win32.WM_CHANGECBCHAIN:
					if ( wParam == _hWndNextViewer )
					{
						/////
						// clipboard viewer chain changed, need to fix it.
						/////
						_hWndNextViewer = lParam;
					}
					else if ( _hWndNextViewer != IntPtr.Zero )
					{
						/////
						// pass the message to the next viewer.
						/////
						Win32.SendMessage( _hWndNextViewer, msg, wParam, lParam );
					}
					break;

				case Win32.WM_DRAWCLIPBOARD:
					/////
					// clipboard content changed
					/////
					DrawContent( );

					/////
					// pass the message to the next viewer.
					/////
					Win32.SendMessage( _hWndNextViewer, msg, wParam, lParam );

					break;
			}

			if ( msg == Settings.InteropMessageId )
			{
				Settings.Channel.SendMessage( new RestoreUiMessage( true ).ToString( ) );
			}

			return IntPtr.Zero;
		}
	}
}