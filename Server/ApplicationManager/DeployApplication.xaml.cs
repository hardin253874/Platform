// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Application = ApplicationManager.Support.Application;

namespace ApplicationManager
{
	/// <summary>
	///     Interaction logic for DeployApplication.xaml
	/// </summary>
	public partial class DeployApplication
	{
		/// <summary>
		///     View model.
		/// </summary>
		private readonly DeployApplicationViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="DeployApplication" /> class.
		/// </summary>
		/// <param name="application">The application.</param>
		public DeployApplication( Application application )
			: this( )
		{
			_viewModel = new DeployApplicationViewModel( application );

			DataContext = _viewModel;
		}

		/// <summary>
		///     Prevents a default instance of the <see cref="DeployApplication" /> class from being created.
		/// </summary>
		private DeployApplication( )
		{
			InitializeComponent( );
		}
	}
}