// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;

namespace TenantDiffTool
{
	/// <summary>
	///     Interaction logic for Viewer.xaml
	/// </summary>
	public partial class Viewer : Window
	{
		/// <summary>
		///     View Model.
		/// </summary>
		private readonly ViewerViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="Viewer" /> class.
		/// </summary>
		public Viewer( )
		{
			InitializeComponent( );

			_viewModel = new ViewerViewModel( );

			DataContext = _viewModel;
		}
	}
}