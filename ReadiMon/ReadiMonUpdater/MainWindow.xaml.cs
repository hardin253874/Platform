// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Threading;
using System.Windows;

namespace ReadiMonUpdater
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		///     The view model
		/// </summary>
		private readonly MainWindowViewModel _viewModel = new MainWindowViewModel( );

		/// <summary>
		///     Initializes a new instance of the <see cref="MainWindow" /> class.
		/// </summary>
		public MainWindow( )
		{
			InitializeComponent( );

			DataContext = _viewModel;
		}

		/// <summary>
		///     Handles the Loaded event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			var dispatcher = Application.Current.Dispatcher;

			var timer = new Timer( o => dispatcher.Invoke(async () => await _viewModel.Update()), null, 1000, Timeout.Infinite );
		}
	}
}