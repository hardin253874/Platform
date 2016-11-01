// Copyright 2011-2016 Global Software Innovation Pty Ltd
using ApplicationManager.Properties;

namespace ApplicationManager
{
	/// <summary>
	///     Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow
	{
		/// <summary>
		///     View model.
		/// </summary>
		private readonly SettingsWindowViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="Settings" /> class.
		/// </summary>
		public SettingsWindow( )
		{
			_viewModel = new SettingsWindowViewModel( );

			DataContext = _viewModel;

			InitializeComponent( );
		}
	}
}