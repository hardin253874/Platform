// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows.Controls;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for EntityImportExportOptions.xaml
	/// </summary>
	public partial class EntityImportExportOptions : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityImportExportOptions" /> class.
		/// </summary>
		/// <param name="viewModel">The view model.</param>
		public EntityImportExportOptions( EntityImportExportOptionsViewModel viewModel )
		{
			InitializeComponent( );

			viewModel.OnLoad( );

			DataContext = viewModel;
		}
	}
}