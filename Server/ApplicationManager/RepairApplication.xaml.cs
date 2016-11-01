// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Application = ApplicationManager.Support.Application;

namespace ApplicationManager
{
	/// <summary>
	/// Interaction logic for RepairApplication.xaml
	/// </summary>
	public partial class RepairApplication
	{
		/// <summary>
		///     View model.
		/// </summary>
		private readonly RepairApplicationViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="RepairApplication" /> class.
		/// </summary>
		/// <param name="application">The application.</param>
		public RepairApplication( Application application )
			: this( )
		{
			_viewModel = new RepairApplicationViewModel( application );

			DataContext = _viewModel;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RepairApplication"/> class.
		/// </summary>
		public RepairApplication( )
		{
			InitializeComponent( );
		}
	}
}
