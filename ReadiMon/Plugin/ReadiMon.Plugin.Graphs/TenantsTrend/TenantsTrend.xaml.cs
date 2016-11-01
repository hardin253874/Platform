// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Graphs.TenantsTrend
{
	/// <summary>
	///     Interaction logic for TenantsTrend.xaml
	/// </summary>
	public partial class TenantsTrend : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TenantsTrend" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public TenantsTrend( IPluginSettings settings )
		{
			var viewModel = new TenantsTrendViewModel( settings );
			DataContext = viewModel;

			InitializeComponent( );
		}
	}
}