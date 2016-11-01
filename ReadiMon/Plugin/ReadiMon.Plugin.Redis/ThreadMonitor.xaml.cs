// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Interaction logic for Threads.xaml
	/// </summary>
	public partial class ThreadMonitor
	{
		/// <summary>
		///     The view model
		/// </summary>
		private readonly ThreadMonitorViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadMonitor" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public ThreadMonitor( IPluginSettings settings )
		{
			InitializeComponent( );

			_viewModel = new ThreadMonitorViewModel( settings );

			DataContext = _viewModel;
		}
	}
}