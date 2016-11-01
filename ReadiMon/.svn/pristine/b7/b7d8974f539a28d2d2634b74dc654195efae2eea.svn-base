// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using ProtoBuf.Meta;
using ReadiMon.AddinView;
using ReadiMon.AddinView.Configuration;
using ReadiMon.Plugin.Redis.Diagnostics;
using ReadiMon.Shared.Diagnostics.Request;
using ReadiMon.Shared.Diagnostics.Response;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Threads plugin
	/// </summary>
	[AddIn( "Threads Plugin", Version = "1.0.0.0" )]
	public class ThreadsPlugin : PluginBase, IPlugin
	{
		private ThreadMonitorOptionsViewModel _optionsViewModel;

		/// <summary>
		///     The user interface
		/// </summary>
		private ThreadMonitor _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPlugin" /> class.
		/// </summary>
		public ThreadsPlugin( )
		{
			SectionName = "Diagnostics";
			EntryName = "Threads";
			EntryOrdinal = 0;
			SectionOrdinal = 2;
			HasUserInterface = true;
			HasOptionsUserInterface = true;
		}

		/// <summary>
		///     Gets the options view model.
		/// </summary>
		/// <value>
		///     The options view model.
		/// </value>
		private ThreadMonitorOptionsViewModel OptionsViewModel
		{
			get
			{
				return _optionsViewModel ?? ( _optionsViewModel = new ThreadMonitorOptionsViewModel( ) );
			}
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public override void OnStartupComplete( )
		{
			RuntimeTypeModel.Default[ typeof ( DiagnosticRequest ) ].AddSubType( 100, typeof ( ThreadRequest ) );
			RuntimeTypeModel.Default[ typeof ( DiagnosticResponse ) ].AddSubType( 100, typeof ( ThreadResponse ) );

			base.OnStartupComplete( );
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new ThreadMonitor( Settings ) );
		}

		/// <summary>
		///     Gets the tool bar.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetToolBar( )
		{
			return null;
		}

		/// <summary>
		///     Called when the plugin shuts down.
		/// </summary>
		public override void OnShutdown( )
		{
			base.OnShutdown( );

			if ( _userInterface != null )
			{
				var viewModel = _userInterface.DataContext as ThreadMonitorViewModel;

				if ( viewModel != null )
				{
					viewModel.OnShutdown( );
				}
			}
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetOptionsUserInterface( )
		{
			return new ThreadMonitorOptions( OptionsViewModel );
		}

		/// <summary>
		///     Saves the options.
		/// </summary>
		public override void SaveOptions( )
		{
			if ( _optionsViewModel != null )
			{
				_optionsViewModel.OnSave( );
			}

			if ( _userInterface != null )
			{
				var viewModel = _userInterface.DataContext as ThreadMonitorViewModel;

				if ( viewModel != null )
				{
					viewModel.OnSettingsUpdate( );
				}
			}
		}

		/// <summary>
		///     Creates the configuration section.
		/// </summary>
		/// <returns></returns>
		protected override PluginConfigurationBase CreateConfigurationSection( )
		{
			return new PluginConfiguration( );
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			if ( _userInterface != null )
			{
				var viewModel = _userInterface.DataContext as ThreadMonitorViewModel;

				if ( viewModel != null )
				{
					viewModel.PluginSettings = Settings;
				}
			}
		}
	}
}