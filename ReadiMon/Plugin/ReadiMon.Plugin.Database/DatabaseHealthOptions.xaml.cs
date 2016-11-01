// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Interaction logic for DatabaseHealthOptions.xaml
	/// </summary>
	public partial class DatabaseHealthOptions : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseHealthOptions" /> class.
		/// </summary>
		/// <param name="viewModel">The view model.</param>
		public DatabaseHealthOptions( DatabaseHealthOptionsViewModel viewModel )
		{
			InitializeComponent( );

			viewModel.OnLoad( );

			DataContext = viewModel;
		}
	}
}