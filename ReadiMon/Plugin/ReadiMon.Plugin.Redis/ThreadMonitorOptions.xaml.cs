// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows.Controls;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Interaction logic for ThreadMonitorOptions.xaml
	/// </summary>
	public partial class ThreadMonitorOptions : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadMonitorOptions" /> class.
		/// </summary>
		/// <param name="viewModel">The view model.</param>
		public ThreadMonitorOptions( ThreadMonitorOptionsViewModel viewModel )
		{
			InitializeComponent( );

			viewModel.OnLoad( );

			DataContext = viewModel;
		}
	}
}