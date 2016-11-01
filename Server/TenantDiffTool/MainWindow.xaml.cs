// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows;

namespace TenantDiffTool
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		/// <summary>
		///     View model.
		/// </summary>
		private readonly MainWindowViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="MainWindow" /> class.
		/// </summary>
		public MainWindow( )
		{
			InitializeComponent( );

			_viewModel = new MainWindowViewModel( this );

			DataContext = _viewModel;

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		/// <summary>
		///     Handles the UnhandledException event of the CurrentDomain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="UnhandledExceptionEventArgs" /> instance containing the event data.
		/// </param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			MessageBox.Show( "An unhandled exception has occurred. " + e.ExceptionObject );
		}
	}
}