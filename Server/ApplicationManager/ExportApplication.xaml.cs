// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Windows.Input;
using Application = ApplicationManager.Support.Application;

namespace ApplicationManager
{
	/// <summary>
	///     Interaction logic for ExportApplication.xaml
	/// </summary>
	public partial class ExportApplication
	{
		/// <summary>
		///     View model.
		/// </summary>
		private readonly ExportApplicationViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="ExportApplication" /> class.
		/// </summary>
		/// <param name="application">The application.</param>
		public ExportApplication( Application application )
			: this( )
		{
			_viewModel = new ExportApplicationViewModel( application );

			DataContext = _viewModel;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ExportApplication" /> class.
		/// </summary>
		private ExportApplication( )
		{
			InitializeComponent( );

			Loaded += ( sender, e ) => MoveFocus( new TraversalRequest( FocusNavigationDirection.Next ) );
		}
	}
}