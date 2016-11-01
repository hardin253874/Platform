// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Application = ApplicationManager.Support.Application;

namespace ApplicationManager
{
	/// <summary>
	///     Interaction logic for DeleteApplication.xaml
	/// </summary>
	public partial class DeleteApplication
	{
		/// <summary>
		///     View model.
		/// </summary>
		private readonly DeleteApplicationViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="DeleteApplication" /> class.
		/// </summary>
		/// <param name="application">The application.</param>
		public DeleteApplication( Application application )
			: this( )
		{
			_viewModel = new DeleteApplicationViewModel( application );

			DataContext = _viewModel;
		}

		/// <summary>
		///     Prevents a default instance of the <see cref="DeleteApplication" /> class from being created.
		/// </summary>
		private DeleteApplication( )
		{
			InitializeComponent( );
		}

		/// <summary>
		///     Gets the view model.
		/// </summary>
		/// <value>
		///     The view model.
		/// </value>
		public DeleteApplicationViewModel ViewModel
		{
			get
			{
				return _viewModel;
			}
		}
	}
}