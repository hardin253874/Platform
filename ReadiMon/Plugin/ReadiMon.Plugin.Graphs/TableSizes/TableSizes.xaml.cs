// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Graphs.TableSizes
{
	/// <summary>
	///     Interaction logic for TableSizes.xaml
	/// </summary>
	public partial class TableSizes : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TableSizes" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public TableSizes( IPluginSettings settings )
		{
			var viewModel = new TableSizesViewModel( settings );
			DataContext = viewModel;

			InitializeComponent( );
		}
	}
}