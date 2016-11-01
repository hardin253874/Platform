// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CommandLine;
using ReadiMon.Core;
using ReadiMon.Properties;
using EventLog = ReadiMon.Shared.Core.EventLog;

namespace ReadiMon
{
	/// <summary>
	///     Application.
	/// </summary>
	public class Application : System.Windows.Application
	{
		/// <summary>
		///     The broadcast handle.
		/// </summary>
		private const int HwndBroadcast = 0xFFFF;

		/// <summary>
		///     The ReadiMon updater name
		/// </summary>
		private const string ReadiMonUpdaterName = "ReadiMonUpdater.exe";

		/// <summary>
		///     The application instance.
		/// </summary>
		public static Application App;

		/// <summary>
		///     Custom message.
		/// </summary>
		public static readonly int WmMyMsg = RegisterWindowMessage( "WM_MY_MSG" );

		/// <summary>
		/// An update is present
		/// </summary>
		/// <value>
		///   <c>true</c> if [update present]; otherwise, <c>false</c>.
		/// </value>
		public bool UpdatePresent
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [restart readi mon after update].
		/// </summary>
		/// <value>
		/// <c>true</c> if [restart readi mon after update]; otherwise, <c>false</c>.
		/// </value>
		public bool RestartReadiMonAfterUpdate
		{
			get;
			set;
		}

		/// <summary>
		///     Defines the entry point of the application.
		/// </summary>
		[STAThread]
		[DebuggerNonUserCode]
		[LoaderOptimization( LoaderOptimization.MultiDomainHost )]
		public static void Main( )
		{
			if ( Environment.GetCommandLineArgs( ).Length <= 1 )
			{
				var splashScreen = new SplashScreen( "splashscreen.png" );
				splashScreen.Show( true );
			}

			var app = new Application( );
			App = app;
			app.Run( );
		}

		/// <summary>
		///     Sets the theme.
		/// </summary>
		/// <param name="themeName">Name of the theme.</param>
		/// <param name="themeColor">Color of the theme.</param>
		public static void SetTheme( string themeName, string themeColor )
		{
			const BindingFlags staticNonPublic = BindingFlags.Static | BindingFlags.NonPublic;
			var presentationFrameworkAsm = Assembly.GetAssembly( typeof( Window ) );
			var themeWrapper = presentationFrameworkAsm.GetType( "MS.Win32.UxThemeWrapper" );
			var isActiveField = themeWrapper.GetField( "_isActive", staticNonPublic );
			var themeColorField = themeWrapper.GetField( "_themeColor", staticNonPublic );
			var themeNameField = themeWrapper.GetField( "_themeName", staticNonPublic );

			/////
			// Set this to true so WPF doesn't default to classic.
			/////
			if ( isActiveField != null )
			{
				isActiveField.SetValue( null, true );
			}

			if ( themeColorField != null )
			{
				themeColorField.SetValue( null, themeColor );
			}

			if ( themeNameField != null )
			{
				themeNameField.SetValue( null, themeName );
			}
		}

		/// <summary>
		///     Raises the <see cref="E:System.Windows.Application.Exit" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
		protected override void OnExit( ExitEventArgs e )
		{
			try
			{
				if ( UpdatePresent )
				{
					UpdateHelper.StartUpgrade( RestartReadiMonAfterUpdate );
				}
			}
			catch ( Exception exc )
			{
				Trace.WriteLine( exc );
			}

			base.OnExit( e );
		}

		/// <summary>
		///     Raises the <see cref="E:System.Windows.Application.Startup" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			Current.Exit += Application_Exit;

			if ( e.Args.Length > 0 )
			{
				RunConsoleVersion( e.Args );
			}
			else
			{
				if ( Settings.Default.AutoUpdate )
				{
					ThreadPool.QueueUserWorkItem( s =>
					{
						UpdatePresent = UpdateHelper.DoesUpdateExist( );
					} );
				}

				RunGuiVersion( );
			}
		}

		private static void DeleteUpdaterBinary( )
		{
			string currentLocation = Assembly.GetEntryAssembly( ).Location;

			string directoryName = Path.GetDirectoryName( currentLocation );

			if ( !string.IsNullOrEmpty( directoryName ) )
			{
				string updater = Path.Combine( directoryName, ReadiMonUpdaterName );

				if ( File.Exists( updater ) )
				{
					File.Delete( updater );
				}
			}
		}

		/// <summary>
		///     Registers the window message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		[DllImport( "user32" )]
		private static extern int RegisterWindowMessage( string message );

		/// <summary>
		///     Sends the notify message.
		/// </summary>
		/// <param name="hwnd">The HWND.</param>
		/// <param name="msg">The MSG.</param>
		/// <param name="wparam">The w-param.</param>
		/// <param name="lparam">The l-param.</param>
		/// <returns></returns>
		[DllImport( "user32" )]
		private static extern bool SendNotifyMessage( IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam );

		/// <summary>
		///     Sets the foreground window.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <returns></returns>
		[DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		private static extern bool SetForegroundWindow( IntPtr hWnd );

		//API-declaration

		/// <summary>
		///     Handles the Exit event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ExitEventArgs" /> instance containing the event data.</param>
		private void Application_Exit( object sender, ExitEventArgs e )
		{
			PluginManager.Dispose( );
		}

		/// <summary>
		///     Handles the DispatcherUnhandledException event of the Current control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs" /> instance containing
		///     the event data.
		/// </param>
		private void Current_DispatcherUnhandledException( object sender, DispatcherUnhandledExceptionEventArgs e )
		{
			EventLog.Instance.WriteException( e.Exception );

			var comException = e.Exception as COMException;

			if ( comException != null && comException.ErrorCode == -2147221040 )
			{
				e.Handled = true;
			}

			var worker = new BackgroundWorker( );
			worker.DoWork += ( s, a ) =>
			{
				try
				{
					SendEmail( e.Exception, "ReadiMon Unhandled exception" );
				}
				catch ( Exception exc )
				{
					EventLog.Instance.WriteException( exc );
				}
			};
		}

		/// <summary>
		///     Handles the UnhandledException event of the CurrentDomain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="UnhandledExceptionEventArgs" /> instance containing the event data.</param>
		private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			EventLog.Instance.WriteException( ( Exception ) e.ExceptionObject );

			var worker = new BackgroundWorker( );

			worker.DoWork += ( s, a ) =>
			{
				try
				{
					SendEmail( e.ExceptionObject, "ReadiMon Unhandled exception" );
				}
				catch ( Exception exc )
				{
					EventLog.Instance.WriteException( exc );
				}
			};

			worker.RunWorkerAsync( );
		}

		/// <summary>
		///     Runs the console.
		/// </summary>
		/// <param name="options">The options.</param>
		private void RunConsole( CommandLineOptions options )
		{
			if ( options.RunDatabaseTests )
			{
				var databasePlugin = PluginManager.Plugins.FirstOrDefault( p => p.Token.Name == "Database Health Plugin" );

				if ( databasePlugin != null )
				{
					string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#if DEBUG
					configFile = configFile.Replace( "vshost.", "" );
#endif

					var settings = new PluginSettings
					{
						ConfigurationFile = configFile,
						InteropMessageId = WmMyMsg,
						EventLog = EventLog.Instance,
						DatabaseSettings = new DatabaseSettings( options.DatabaseServer, options.DatabaseCatalog ),
						RedisSettings = new RedisSettings( options.RedisServer, options.RedisPort ),
                        Tenants = options.Tenants
					};

					databasePlugin.Initialize( settings );

					string result = databasePlugin.Plugin.Invoke( "run" );

					if ( options.Output != null && !string.IsNullOrEmpty( result ) )
					{
						using ( var stream = File.CreateText( options.Output ) )
						{
							stream.Write( result );
							stream.Flush( );
						}
					}
				}
			}
		}

		/// <summary>
		///     Runs the console version.
		/// </summary>
		/// <param name="args">The arguments.</param>
		private void RunConsoleVersion( string[ ] args )
		{
			try
			{
				var options = new CommandLineOptions( );

				Parser parser = new Parser( );
				if ( parser.ParseArguments( args, options ) )
				{
					RunConsole( options );

					Environment.Exit( 0 );
				}
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc );
			}
		}

		/// <summary>
		///     Runs the GUI version.
		/// </summary>
		private void RunGuiVersion( )
		{
			DeleteUpdaterBinary( );

			StartupUri = new Uri( "MainWindow.xaml", UriKind.Relative );

			var resourceDictionary = new ResourceDictionary
			{
				Source = new Uri( "/PresentationFramework.Aero, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL;component/themes/aero.normalcolor.xaml", UriKind.Relative )
			};

			Current.Resources.MergedDictionaries.Add( resourceDictionary );
			Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

			SendStartupEmail( );
		}

		/// <summary>
		///     Sends the email.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="subject">The subject.</param>
		private void SendEmail( object data, string subject )
		{
			var client = new SmtpClient
			{
				DeliveryMethod = SmtpDeliveryMethod.Network,
				Host = "mail.readinow.com",
				Port = 25
			};

			ServicePointManager.ServerCertificateValidationCallback = ( sender, certificate, chain, sslPolicyErrors ) => true;

			string body = string.Format( "Date: {0}\nDomain: {1}\nUsername: {2}\nMachine Name: {3}\nVersion: {4}\n\n{5}", DateTime.Now, Environment.UserDomainName, Environment.UserName, Environment.MachineName, Assembly.GetEntryAssembly( ).GetName( ).Version, data );

			using ( var message = new MailMessage( new MailAddress( "readimon@readinow.com", "ReadiMon" ), new MailAddress( "david.quint@readinow.com", "David Quint" ) ) )
			{
				message.Subject = subject;
				message.Body = body;
				message.IsBodyHtml = false;

				client.Send( message );
			}
		}

		/// <summary>
		///     Sends the startup email.
		/// </summary>
		private void SendStartupEmail( )
		{
			string machineName = Environment.MachineName.ToLowerInvariant( );

			if ( !Settings.Default.PhonedHome && machineName != "dev" && machineName != "syd1dev22" && machineName != "rndev22" )
			{
				var worker = new BackgroundWorker( );
				worker.DoWork += ( s, a ) =>
				{
					try
					{
						SendEmail( string.Empty, "ReadiMon First Run" );

						Settings.Default.PhonedHome = true;
						Settings.Default.Save( );
					}
					catch ( Exception exc )
					{
						EventLog.Instance.WriteException( exc );
					}
				};


				worker.RunWorkerAsync( );
			}
		}
	}
}