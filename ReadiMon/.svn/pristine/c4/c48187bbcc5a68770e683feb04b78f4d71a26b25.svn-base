// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows.Controls;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for WorkflowMonitorOptions.xaml
	/// </summary>
	public partial class WorkflowMonitorOptions : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="WorkflowMonitorOptions" /> class.
		/// </summary>
		/// <param name="viewModel">The view model.</param>
		public WorkflowMonitorOptions( WorkflowMonitorOptionsViewModel viewModel )
		{
			InitializeComponent( );

			viewModel.OnLoad( );

			DataContext = viewModel;
		}
	}
}