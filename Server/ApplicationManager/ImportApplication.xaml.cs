// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Windows;

namespace ApplicationManager
{
	/// <summary>
	///     Interaction logic for ImportApplication.xaml
	/// </summary>
	public partial class ImportApplication
	{
		/// <summary>
		///     View model.
		/// </summary>
		private readonly ImportApplicationViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="ImportApplication" /> class.
		/// </summary>
		public ImportApplication( )
		{
			InitializeComponent( );

			_viewModel = new ImportApplicationViewModel( );

			DataContext = _viewModel;

			Loaded += ImportApplication_Loaded;
		}

		/// <summary>
		///     Handles the Loaded event of the ImportApplication control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="RoutedEventArgs" /> instance containing the event data.
		/// </param>
		private void ImportApplication_Loaded( object sender, RoutedEventArgs e )
		{
			_viewModel.DialogLoaded( );
		}
	}
}