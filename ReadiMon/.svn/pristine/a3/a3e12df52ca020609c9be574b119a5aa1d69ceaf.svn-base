// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using JetBrains.Annotations;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Application
{
	/// <summary>
	///     Tenant Application Plugin.
	/// </summary>
	[AddIn( "Tenant Applications Plugin", Version = "1.0.0.0" )]
	[UsedImplicitly]
	public class TenantApplicationsPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The user interface
		/// </summary>
		private TenantApplications _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantApplicationsPlugin" /> class.
		/// </summary>
		public TenantApplicationsPlugin( )
		{
			SectionOrdinal = 10;
			SectionName = "Application";
			EntryName = "Tenant Applications";
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
			return _userInterface ?? ( _userInterface = new TenantApplications( Settings ) );
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			var viewModel = _userInterface?.DataContext as TenantApplicationsViewModel;

			if ( viewModel != null )
			{
				viewModel.PluginSettings = Settings;
			}
		}
	}
}