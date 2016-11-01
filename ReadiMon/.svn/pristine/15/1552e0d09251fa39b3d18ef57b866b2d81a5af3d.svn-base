// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReadiMon.Shared.AttachedProperties;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Controls
{
	/// <summary>
	///     ReadiMon ListView.
	/// </summary>
	public class ReadiMonListView : ListView
	{
		/// <summary>
		///     The selected items list property
		/// </summary>
		public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.Register( "SelectedItemsList", typeof( IList ), typeof( ReadiMonListView ), new PropertyMetadata( null ) );

		/// <summary>
		///     Initializes a new instance of the <see cref="ReadiMonListView" /> class.
		/// </summary>
		public ReadiMonListView( )
		{
			LayoutUpdated += ReadiMonListView_LayoutUpdated;

			Loaded += ReadiMonListView_Loaded;
			SelectionChanged += ReadiMonListView_SelectionChanged;
		}

		/// <summary>
		///     Gets or sets the selected items list.
		/// </summary>
		/// <value>
		///     The selected items list.
		/// </value>
		public IList SelectedItemsList
		{
			get
			{
				return ( IList ) GetValue( SelectedItemsListProperty );
			}
			set
			{
				SetValue( SelectedItemsListProperty, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [automatic update width].
		/// </summary>
		/// <value>
		///     <c>true</c> if [automatic update width]; otherwise, <c>false</c>.
		/// </value>
		private bool AutoUpdateWidth
		{
			get;
			set;
		}

		/// <summary>
		///     Handles the LayoutUpdated event of the ReadiMonListView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void ReadiMonListView_LayoutUpdated( object sender, EventArgs e )
		{
			var gridView = View as GridView;

			if ( gridView != null )
			{
				var scrollViewer = VisualTreeHelperMethods.GetScrollViewer( this ) as ScrollViewer;

				if ( scrollViewer != null )
				{
					var actualWidth = scrollViewer.ViewportWidth - 12;

					double fixedSize = 0;

					var autoColumns = new List<ReadiMonGridViewColumn>( );

					foreach ( var column in gridView.Columns )
					{
						var col = column as ReadiMonGridViewColumn;

						if ( col == null )
						{
							continue;
						}

						if ( col.Hidden )
						{
							col.MinWidth = 0;
							col.Width = 0;
							continue;
						}

						if ( col.UserResized )
						{
							fixedSize += col.Width;
							continue;
						}

						autoColumns.Add( col );
					}

					double width = ( actualWidth - fixedSize ) / autoColumns.Count;

					if ( width < 50 )
					{
						width = 50;
					}

					if ( width > 0 )
					{
						AutoUpdateWidth = true;

						foreach ( ReadiMonGridViewColumn col in autoColumns )
						{
							double minWidth = col.MinWidth;

							if ( minWidth <= 0 )
							{
								minWidth = 50;
							}

							col.Width = width < minWidth ? minWidth : width;
						}

						AutoUpdateWidth = false;
					}
				}
			}
		}

		/// <summary>
		///     Handles the Loaded event of the ReadiMonListView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private void ReadiMonListView_Loaded( object sender, RoutedEventArgs args )
		{
			var gridView = View as GridView;

			if ( gridView != null )
			{
				foreach ( var column in gridView.Columns )
				{
					( ( INotifyPropertyChanged ) column ).PropertyChanged += ( s, e ) =>
					{
						if ( e.PropertyName == "ActualWidth" )
						{
							if ( !AutoUpdateWidth )
							{
								var col = s as ReadiMonGridViewColumn;

								if ( col != null )
								{
									if ( col.Hidden )
									{
										col.Width = 0;
										col.MinWidth = 0;
										return;
									}

									double minWidth = col.MinWidth;

									if ( minWidth <= 0 )
									{
										minWidth = 50;
									}

									if ( col.Width < minWidth )
									{
										col.Width = minWidth;
									}

									col.UserResized = true;

									int index = gridView.Columns.IndexOf( col );

									for ( int i = 0; i < index; i++ )
									{
										col = gridView.Columns[ i ] as ReadiMonGridViewColumn;

										if ( col != null )
										{
											col.UserResized = true;
										}
									}
								}
							}
						}
					};
				}
			}

			string source = ListViewBackGroundImage.GetImageSourceUri( this );

			if ( !string.IsNullOrEmpty( source ) )
			{
				var bitmapImage = new BitmapImage( new Uri( source ) );

				var brush = new ImageBrush( bitmapImage )
				{
					AlignmentX = ListViewBackGroundImage.GetAlignmentX( this ),
					AlignmentY = ListViewBackGroundImage.GetAlignmentY( this ),
					Opacity = 0.25,
					Stretch = Stretch.None
				};


				Background = brush;
			}
		}

		/// <summary>
		///     Handles the SelectionChanged event of the ReadiMonListView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
		private void ReadiMonListView_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			SelectedItemsList = SelectedItems;
		}
	}
}