// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using JetBrains.Annotations;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     User Account Plugin
	/// </summary>
	[AddIn( "User Account Plugin", Version = "1.0.0.0" )]
	[UsedImplicitly]
	public class UserAccountPlugin : PluginBase, IPlugin
	{
		private UserAccounts _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="UserAccountPlugin" /> class.
		/// </summary>
		public UserAccountPlugin( )
		{
			SectionOrdinal = 10;
			SectionName = "Security";
			EntryName = "User Accounts";
			EntryOrdinal = 10;
			HasOptionsUserInterface = false;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new UserAccounts( Settings ) );
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			var viewModel = _userInterface?.DataContext as UserAccountsViewModel;

			if ( viewModel != null )
			{
				viewModel.PluginSettings = Settings;
			}
		}
	}
}