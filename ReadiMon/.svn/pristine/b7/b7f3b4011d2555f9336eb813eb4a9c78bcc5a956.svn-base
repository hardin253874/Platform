// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using JetBrains.Annotations;
using ProtoBuf.Meta;
using ReadiMon.AddinView;
using ReadiMon.Plugin.Database.Diagnostics;
using ReadiMon.Shared.Diagnostics.Request;
using ReadiMon.Shared.Diagnostics.Response;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Database History plugin
	/// </summary>
	[AddIn( "Database History Plugin", Version = "1.0.0.0" )]
	[UsedImplicitly]
	public class DatabaseHistoryPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The user interface
		/// </summary>
		private DatabaseHistory _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseHistoryPlugin" /> class.
		/// </summary>
		public DatabaseHistoryPlugin( )
		{
			SectionOrdinal = 5;
			SectionName = "Database";
			EntryName = "Time Machine";
			EntryOrdinal = 5;
			HasOptionsUserInterface = false;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new DatabaseHistory( Settings ) );
		}

		/// <summary>
		///     Called when the plugin shuts down.
		/// </summary>
		public override void OnShutdown( )
		{
			base.OnShutdown( );

			var viewModel = _userInterface?.DataContext as DatabaseHistoryViewModel;

			viewModel?.OnShutdown( );
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public override void OnStartupComplete( )
		{
			RuntimeTypeModel.Default[ typeof( DiagnosticRequest ) ].AddSubType( 103, typeof( FlushCachesRequest ) );
			RuntimeTypeModel.Default[ typeof( DiagnosticResponse ) ].AddSubType( 103, typeof( FlushCachesResponse ) );

			base.OnStartupComplete( );
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			var viewModel = _userInterface?.DataContext as DatabaseHistoryViewModel;

			if ( viewModel != null )
			{
				viewModel.PluginSettings = Settings;
			}
		}
	}
}