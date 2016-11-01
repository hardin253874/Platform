// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using EDC.ReadiNow.Scheduling;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ApplicationManager
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

			_viewModel = new MainWindowViewModel( );

			DataContext = _viewModel;

			Activated += MainWindow_Activated;
			Closing += MainWindow_Closing;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		/// <summary>
		///     Handles the UnhandledException event of the CurrentDomain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="UnhandledExceptionEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			MessageBox.Show( "An unhandled exception has occurred. " + e.ExceptionObject );
		}

		/// <summary>
		///     Handles the Activated event of the MainWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void MainWindow_Activated( object sender, EventArgs e )
		{
			List<Window> windows = Application.Current.Windows.OfType<Window>( ).ToList( );

			Window window = windows.FirstOrDefault( p => !p.Equals( this ) && !p.IsActive );

			if ( window != null )
			{
				window.Activate( );
			}
		}

		/// <summary>
		///     Handles the Closing event of the MainWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.</param>
		private void MainWindow_Closing( object sender, CancelEventArgs e )
		{
			SchedulingHelper.Instance.Shutdown( true ); // shutdown the instance so that no threads are left running
		}
	}
}