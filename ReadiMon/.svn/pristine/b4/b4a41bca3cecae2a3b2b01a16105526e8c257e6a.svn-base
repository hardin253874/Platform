// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiMon.Shared;

namespace ReadiMon.Plugin.Application
{
	/// <summary>
	///     Interaction logic for TenantApplications.xaml
	/// </summary>
	public partial class TenantApplications
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TenantApplications" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public TenantApplications( IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new TenantApplicationsViewModel( settings );
			DataContext = viewModel;
		}
	}
}