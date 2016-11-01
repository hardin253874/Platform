// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;
using ReadiMon.Shared;
using ReadiMon.Shared.Controls;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Interaction logic for DatabaseHealth.xaml
	/// </summary>
	public partial class DatabaseHealth : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseHealth" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public DatabaseHealth( IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new DatabaseHealthViewModel( settings );
			DataContext = viewModel;
		}

		/// <summary>
		///     Handles the Click event of the ReadiMonListView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
		private void ReadiMonListView_Click( object sender, RoutedEventArgs e )
		{
			GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;

			if ( header != null && header.Content.ToString( ) == "Enabled" )
			{
				ReadiMonListView list = sender as ReadiMonListView;

				if ( list?.ItemsSource != null )
				{
					foreach ( DatabaseTest test in list.ItemsSource )
					{
						test.SelectedToRun = !test.SelectedToRun;
					}
				}
			}
		}
	}
}