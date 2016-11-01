// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Graphs.LicensingMetrics
{
	/// <summary>
	///     Interaction logic for LicensingMetrics.xaml
	/// </summary>
	public partial class LicensingMetrics : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="LicensingMetrics" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public LicensingMetrics( IPluginSettings settings )
		{
			var viewModel = new LicensingMetricsViewModel( settings );
			viewModel.GatherComplete += ViewModelOnGatherComplete;
			DataContext = viewModel;

			InitializeComponent( );
		}

		private void OnMetricsUpdate( )
		{
			if ( MetricsUpdate != null )
			{
				MetricsUpdate( this, new EventArgs( ) );
			}
		}

		private void ViewModelOnGatherComplete( object sender, EventArgs eventArgs )
		{
			OnMetricsUpdate( );
		}

		/// <summary>
		///     Occurs when [metrics update].
		/// </summary>
		public event EventHandler MetricsUpdate;
	}
}