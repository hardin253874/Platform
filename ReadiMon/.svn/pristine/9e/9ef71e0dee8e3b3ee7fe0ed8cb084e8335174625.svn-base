// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using JetBrains.Annotations;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Application
{
	/// <summary>
	///     Library Applications Plugin
	/// </summary>
	[AddIn( "Library Applications Plugin", Version = "1.0.0.0" )]
	[UsedImplicitly]
	public class LibraryApplicationsPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The user interface
		/// </summary>
		private LibraryApplications _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="LibraryApplicationsPlugin" /> class.
		/// </summary>
		public LibraryApplicationsPlugin( )
		{
			SectionOrdinal = 10;
			SectionName = "Application";
			EntryName = "Library Applications";
			EntryOrdinal = 6;
			HasOptionsUserInterface = false;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new LibraryApplications( Settings ) );
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			var viewModel = _userInterface?.DataContext as LibraryApplicationsViewModel;

			if ( viewModel != null )
			{
				viewModel.PluginSettings = Settings;
			}
		}
	}
}