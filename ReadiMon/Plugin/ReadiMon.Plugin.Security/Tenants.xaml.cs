// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     Interaction logic for Tenants.xaml
	/// </summary>
	public partial class Tenants : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="Tenants" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public Tenants( IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new TenantsViewModel( settings );
			DataContext = viewModel;
		}
	}
}