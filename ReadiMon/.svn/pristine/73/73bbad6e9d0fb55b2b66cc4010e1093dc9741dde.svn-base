// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     Interaction logic for UserAccounts.xaml
	/// </summary>
	public partial class UserAccounts : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UserAccounts" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public UserAccounts( IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new UserAccountsViewModel( settings );
			DataContext = viewModel;
		}
	}
}