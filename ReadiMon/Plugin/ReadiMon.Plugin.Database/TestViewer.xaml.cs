// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;
using ReadiMon.Shared.Controls;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Interaction logic for TestViewer.xaml
	/// </summary>
	public partial class TestViewer : Window
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TestViewer" /> class.
		/// </summary>
		public TestViewer( )
		{
			InitializeComponent( );
		}

		/// <summary>
		///     Handles the Loaded event of the listView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private void listView_Loaded( object sender, RoutedEventArgs e )
		{
			var gridView = ListView.View as GridView;

			if ( gridView == null )
			{
				return;
			}

			var viewModel = ListView.DataContext as TestViewerViewModel;

			if ( viewModel == null )
			{
				return;
			}

			if ( string.IsNullOrEmpty( viewModel.Resolution ) )
			{
				var col = gridView.Columns[ 0 ] as ReadiMonGridViewColumn;

				if ( col != null )
				{
					col.Hidden = true;
				}
			}

			for ( int i = 1; i < gridView.Columns.Count; i++ )
			{
				if ( viewModel.Columns.Count >= i )
				{
					var columnName = viewModel.Columns[ i - 1 ];

					gridView.Columns[ i ].Header = columnName;
				}
				else
				{
					gridView.Columns.RemoveAt( i );
					i--;
				}
			}
		}

		/// <summary>
		///     Handles the OnClick event of the ListView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private void ListView_OnClick( object sender, RoutedEventArgs e )
		{
			ReadiMonListView list = sender as ReadiMonListView;

			GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;

			if ( list != null && column != null && list.ItemsSource != null )
			{
				GridView view = list.View as GridView;

				if ( view?.Columns.IndexOf( column.Column ) == 0 )
				{
					foreach ( FailureRow row in list.ItemsSource )
					{
						row.RowSelected = !row.RowSelected;
					}
				}
			}
		}
	}
}