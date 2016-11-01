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
	///     Remote Exec plugin
	/// </summary>
	[AddIn( "Remote Exec Plugin", Version = "1.0.0.0" )]
	public class RemoteExecPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The user interface
		/// </summary>
		private RemoteExecControl _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPlugin" /> class.
		/// </summary>
		public RemoteExecPlugin( )
		{
			SectionName = "Diagnostics";
			EntryName = "Remote Executor";
			EntryOrdinal = 1;
			SectionOrdinal = 2;
			HasUserInterface = true;
			HasOptionsUserInterface = false;
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public override void OnStartupComplete( )
		{
			RuntimeTypeModel.Default[ typeof ( DiagnosticRequest ) ].AddSubType( 102, typeof ( RemoteExecRequest ) );
			RuntimeTypeModel.Default[ typeof ( DiagnosticResponse ) ].AddSubType( 102, typeof ( RemoteExecResponse ) );

			base.OnStartupComplete( );
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new RemoteExecControl( Settings ) );
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
				var viewModel = _userInterface.DataContext as RemoteExecViewModel;

				if ( viewModel != null )
				{
					viewModel.OnShutdown( );
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
	}
}