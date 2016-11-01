// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for WorkflowMonitor.xaml
	/// </summary>
	public partial class WorkflowMonitor : UserControl
	{
		/// <summary>
		///     The view model
		/// </summary>
		private readonly WorkflowMonitorViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="WorkflowMonitor" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public WorkflowMonitor( IPluginSettings settings )
		{
			InitializeComponent( );

			_viewModel = new WorkflowMonitorViewModel( settings );

			DataContext = _viewModel;
		}
	}
}