// Copyright 2011-2016 Global Software Innovation Pty Ltd

using TenantDiffTool.Core;

namespace TenantDiffTool
{
	/// <summary>
	///     Interaction logic for SourceSelector.xaml
	/// </summary>
	public partial class SourceSelector
	{
		/// <summary>
		///     View Model.
		/// </summary>
		private readonly SourceSelectorViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="SourceSelector" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		public SourceSelector( ISource source )
		{
			InitializeComponent( );

			_viewModel = new SourceSelectorViewModel( source, this );

			DataContext = _viewModel;
		}
	}
}