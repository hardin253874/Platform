// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using JetBrains.Annotations;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     General Status Plugin
	/// </summary>
	[AddIn( "General Status Plugin", Version = "1.0.0.0" )]
	[UsedImplicitly]
	public class GeneralStatusPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The user interface
		/// </summary>
		private GeneralStatus _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="GeneralStatusPlugin" /> class.
		/// </summary>
		public GeneralStatusPlugin( )
		{
			SectionOrdinal = 0;
			SectionName = "General";
			EntryName = "Status";
			EntryOrdinal = 0;
			HasOptionsUserInterface = true;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new GeneralStatus( Settings ) );
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			var viewModel = _userInterface?.DataContext as GeneralSettingsViewModel;

			if ( viewModel != null )
			{
				viewModel.PluginSettings = Settings;
			}
		}
	}
}