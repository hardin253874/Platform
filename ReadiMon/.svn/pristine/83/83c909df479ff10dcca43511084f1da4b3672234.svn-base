// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using JetBrains.Annotations;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     Tenants Plugin
	/// </summary>
	[AddIn( "Tenant Plugin", Version = "1.0.0.0" )]
	[UsedImplicitly]
	public class TenantsPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The user interface
		/// </summary>
		private Tenants _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantsPlugin" /> class.
		/// </summary>
		public TenantsPlugin( )
		{
			SectionOrdinal = 10;
			SectionName = "Security";
			EntryName = "Tenants";
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
			return _userInterface ?? ( _userInterface = new Tenants( Settings ) );
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			var viewModel = _userInterface?.DataContext as TenantsViewModel;

			if ( viewModel != null )
			{
				viewModel.PluginSettings = Settings;
			}
		}
	}
}