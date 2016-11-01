// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ReadiMon.Core;
using ReadiMon.Properties;
using ReadiMon.Shared.Core;

namespace ReadiMon
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		/// <summary>
		///     The check for update timer
		/// </summary>
		private DispatcherTimer _whatsNewTimer;

		/// <summary>
		///     The hide when minimized flag.
		/// </summary>
		private bool _hideWhenMinimized = true;

		/// <summary>
		///     The previous state
		/// </summary>
		private WindowState _previousState = WindowState.Normal;

		/// <summary>
		///     Initializes a new instance of the <see cref="MainWindow" /> class.
		/// </summary>
		public MainWindow( )
		{
			InitializeComponent( );

			UpgradeExistingInstallation( );

			var viewModel = new MainWindowViewModel( this, RefreshAccordion );

			viewModel.RestoreUi += viewModel_RestoreUi;
			viewModel.ShowWhenMinimized += viewModel_ShowWhenMinimized;
			viewModel.HideWhenMinimized += viewModel_HideWhenMinimized;

			DataContext = viewModel;

			Activated += MainWindow_Activated;

			DatabaseSettings databaseSettings = GetDatabaseSettings( );

			if ( databaseSettings == null )
			{
				MessageBox.Show( "A valid database server is required.", "ReadiMon" );
				System.Windows.Application.Current.Shutdown( 0 );
				return;
			}

			RedisSettings redisSettings = GetRedisSettings( );

			viewModel.DatabaseSettings = databaseSettings;
			viewModel.RedisSettings = redisSettings;

			viewModel.Start( );

			Loaded += MainWindow_Loaded;
		}

		/// <summary>
		///     Gets the database settings.
		/// </summary>
		/// <returns></returns>
		private static DatabaseSettings GetDatabaseSettings( )
		{
			string username = null;
			string password = null;

			string server = Settings.Default.DatabaseServer;
			string catalog = Settings.Default.DatabaseCatalog;
			bool useIntegratedSecurity = Settings.Default.DatabaseUseIntegratedSecurity;

			if ( ! useIntegratedSecurity )
			{
				username = Settings.Default.DatabaseUsername;
				password = Settings.Default.DatabasePassword;
			}

			if ( string.IsNullOrEmpty( server ) )
			{
				server = "localhost";
			}

			if ( string.IsNullOrEmpty( catalog ) )
			{
				catalog = "SoftwarePlatform";
			}

			var databaseSettings = new DatabaseSettings( server, catalog, username, password, useIntegratedSecurity );

			var manager = new DatabaseManager( databaseSettings );

			bool modified = false;

			while ( ! manager.TestConnection( ) )
			{
				modified = true;
				databaseSettings = null;

				var properties = new DatabaseProperties( );
				properties.ShowDialog( );

				var vm = properties.DataContext as DatabasePropertiesViewModel;

				if ( vm != null && vm.OkClicked )
				{
					databaseSettings = vm.DatabaseInfo;

					manager = new DatabaseManager( databaseSettings );
				}
				else
				{
					break;
				}
			}

			if ( modified && databaseSettings != null )
			{
				Settings.Default.DatabaseServer = databaseSettings.ServerName;
				Settings.Default.DatabaseCatalog = databaseSettings.CatalogName;
				Settings.Default.DatabaseUseIntegratedSecurity = databaseSettings.UseIntegratedSecurity;
				Settings.Default.DatabaseUsername = databaseSettings.Username;
				Settings.Default.DatabasePassword = databaseSettings.Password;
				Settings.Default.Save( );
			}

			return databaseSettings;
		}

		/// <summary>
		///     Gets the redis settings.
		/// </summary>
		/// <returns></returns>
		private static RedisSettings GetRedisSettings( )
		{
			return new RedisSettings( Settings.Default.RedisServer, Settings.Default.RedisPort );
		}

		/// <summary>
		///     Handles the Activated event of the MainWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
		private void MainWindow_Activated( object sender, EventArgs e )
		{
			List<Window> windows = System.Windows.Application.Current.Windows.OfType<Window>( ).ToList( );

			Window window = windows.FirstOrDefault( p => !p.Equals( this ) && !p.IsActive );

			if ( window != null )
			{
				window.Activate( );
			}
		}

		/// <summary>
		///     Handles the Loaded event of the MainWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			_whatsNewTimer = new DispatcherTimer( TimeSpan.FromMilliseconds( 500 ), DispatcherPriority.Background, OnWindowLoaded, System.Windows.Application.Current.Dispatcher );
		}

		/// <summary>
		///     Called when the window is loaded.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void OnWindowLoaded( object source, EventArgs args )
		{
			ShowWhatsNewDialog( );
		}

		/// <summary>
		///     Parses the change log.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		private static string ParseChangeLog( string path )
		{
			var whatsNew = new StringBuilder( );

			using ( var reader = File.OpenText( path ) )
			{
				bool foundVersion = false;

				var version = AssemblyName.GetAssemblyName( Assembly.GetEntryAssembly( ).Location ).Version.ToString( 3 );

				while ( ! foundVersion && !reader.EndOfStream )
				{
					string line = reader.ReadLine( );

					if ( line != null && line.Trim( ) == version )
					{
						bool foundEnd = false;

						while ( ! foundEnd && !reader.EndOfStream )
						{
							line = reader.ReadLine( );

							if ( line != null )
							{
								if ( !line.Trim( ).StartsWith( "* " ) && !line.Trim( ).StartsWith( "- " ) )
								{
									foundEnd = true;
								}
								else
								{
									whatsNew.AppendLine( line.Replace( "\t", "  " ) );
								}
							}
						}
					}

					foundVersion = true;
				}
			}

			return whatsNew.ToString( );
		}

		/// <summary>
		///     Refreshes the accordion.
		/// </summary>
		private void RefreshAccordion( )
		{
			Height = Height + 1;
			Height = Height - 1;
		}

		/// <summary>
		///     Shows the what's new dialog.
		/// </summary>
		private void ShowWhatsNewDialog( )
		{
			try
			{
				if ( !Settings.Default.NeverShowWhatsNew && !Settings.Default.WhatsNew )
				{
					string path = Assembly.GetEntryAssembly( ).Location;

					path = Path.GetDirectoryName( path );

					if ( path != null )
					{
						path = Path.Combine( path, "ChangeLog.txt" );

						if ( ! File.Exists( path ) )
						{
							return;
						}

						string whatsNew = ParseChangeLog( path );

						if ( whatsNew.Length > 0 )
						{
							var changeLog = new ChangeLog( whatsNew )
							{
								Owner = this
							};

							changeLog.ShowDialog( );

							Settings.Default.WhatsNew = true;
							Settings.Default.Save( );
						}
					}
				}
			}
			catch
			{
				Debug.WriteLine( "Failed to show the change log." );
			}
		}

		/// <summary>
		///     Handles the Loaded event of the ToolBar control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private void ToolBar_Loaded( object sender, RoutedEventArgs e )
		{
			var toolBar = sender as ToolBar;

			if ( toolBar != null )
			{
				var overflowGrid = toolBar.Template.FindName( "OverflowGrid", toolBar ) as FrameworkElement;

				if ( overflowGrid != null )
				{
					overflowGrid.Visibility = Visibility.Collapsed;
				}

				var mainPanelBorder = toolBar.Template.FindName( "MainPanelBorder", toolBar ) as FrameworkElement;

				if ( mainPanelBorder != null )
				{
					mainPanelBorder.Margin = new Thickness( 0 );
				}
			}
		}

		/// <summary>
		///     Upgrades the existing installation.
		/// </summary>
		private static void UpgradeExistingInstallation( )
		{
			if ( Settings.Default.UpgradeRequired )
			{
				Settings.Default.Upgrade( );
				Settings.Default.UpgradeRequired = false;
				Settings.Default.PhonedHome = false;
				Settings.Default.WhatsNew = false;
				Settings.Default.Save( );
			}
		}

		/// <summary>
		///     Handles the SizeChanged event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs" /> instance containing the event data.</param>
		private void Window_SizeChanged( object sender, SizeChangedEventArgs e )
		{
			Accordion.UpdateLayout( );
		}

		/// <summary>
		///     Handles the StateChanged event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void Window_StateChanged( object sender, EventArgs e )
		{
			if ( WindowState == WindowState.Minimized )
			{
				if ( _hideWhenMinimized )
				{
					Hide( );
				}
			}
			else
			{
				_previousState = WindowState;
			}
		}

		/// <summary>
		///     Handles the HideWhenMinimized event of the viewModel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void viewModel_HideWhenMinimized( object sender, EventArgs e )
		{
			_hideWhenMinimized = true;

			if ( WindowState == WindowState.Minimized )
			{
				Hide( );
			}
		}

		/// <summary>
		///     Handles the RestoreUi event of the viewModel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void viewModel_RestoreUi( object sender, EventArgs e )
		{
			Show( );

			if ( WindowState == WindowState.Minimized )
			{
				WindowState = _previousState;
			}

			Activate( );
		}

		/// <summary>
		///     Handles the ShowWhenMinimized event of the viewModel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void viewModel_ShowWhenMinimized( object sender, EventArgs e )
		{
			_hideWhenMinimized = false;

			if ( WindowState == WindowState.Minimized )
			{
				Show( );
			}
		}
	}
}