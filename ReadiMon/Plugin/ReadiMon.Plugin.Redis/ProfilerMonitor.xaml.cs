// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Interaction logic for ProfilerMonitor.xaml
	/// </summary>
	public partial class ProfilerMonitor
	{
		/// <summary>
		///     The view model
		/// </summary>
		private readonly ProfilerMonitorViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="ProfilerMonitor" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public ProfilerMonitor( IPluginSettings settings )
		{
			InitializeComponent( );

			_viewModel = new ProfilerMonitorViewModel( settings );

			DataContext = _viewModel;

			_tree.Model = _viewModel.ProfilerTraceModel;
		}
	}
}