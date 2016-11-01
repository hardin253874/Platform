// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using ProtoBuf.Meta;
using ReadiMon.AddinView;
using ReadiMon.AddinView.Configuration;
using ReadiMon.Plugin.Entity.Diagnostics;
using ReadiMon.Shared.Diagnostics.Request;
using ReadiMon.Shared.Diagnostics.Response;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	/// </summary>
	/// <seealso cref="ReadiMon.AddinView.PluginBase" />
	/// <seealso cref="ReadiMon.AddinView.IPlugin" />
	[AddIn( "Workflow Plugin", Version = "1.0.0.0" )]
	public class WorkflowPlugin : PluginBase, IPlugin
	{
		private WorkflowMonitorOptionsViewModel _optionsViewModel;

		/// <summary>
		///     The user interface
		/// </summary>
		private WorkflowMonitor _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="WorkflowPlugin" /> class.
		/// </summary>
		public WorkflowPlugin( )
		{
			SectionName = "Workflow";
			EntryName = "Active Workflows";
			EntryOrdinal = 0;
			SectionOrdinal = 10;
			HasUserInterface = true;
			HasOptionsUserInterface = true;
		}

		/// <summary>
		///     Gets the options view model.
		/// </summary>
		/// <value>
		///     The options view model.
		/// </value>
		private WorkflowMonitorOptionsViewModel OptionsViewModel
		{
			get
			{
				return _optionsViewModel ?? ( _optionsViewModel = new WorkflowMonitorOptionsViewModel( ) );
			}
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public override void OnStartupComplete( )
		{
			RuntimeTypeModel.Default[ typeof ( DiagnosticRequest ) ].AddSubType( 101, typeof ( WorkflowRequest ) );
			RuntimeTypeModel.Default[ typeof ( DiagnosticResponse ) ].AddSubType( 101, typeof ( WorkflowResponse ) );

			base.OnStartupComplete( );
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new WorkflowMonitor( Settings ) );
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
				var viewModel = _userInterface.DataContext as WorkflowMonitorViewModel;

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
			return new WorkflowMonitorOptions( OptionsViewModel );
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
				var viewModel = _userInterface.DataContext as WorkflowMonitorViewModel;

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
				var viewModel = _userInterface.DataContext as WorkflowMonitorViewModel;

				if ( viewModel != null )
				{
					viewModel.PluginSettings = Settings;
				}
			}
		}
	}
}