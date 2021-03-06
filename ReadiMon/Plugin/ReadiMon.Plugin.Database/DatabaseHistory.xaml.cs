﻿// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ReadiMon.Shared;
using ReadiMon.Shared.Controls;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Interaction logic for DatabaseHistory.xaml
	/// </summary>
	public partial class DatabaseHistory : UserControl
	{
		/// <summary>
		///     The view model
		/// </summary>
		private readonly DatabaseHistoryViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseHistory" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public DatabaseHistory( IPluginSettings settings )
		{
			InitializeComponent( );

			_viewModel = new DatabaseHistoryViewModel( settings );
			DataContext = _viewModel;
		}

		/// <summary>
		///     Handles the ToolTipOpening event of the lv control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ToolTipEventArgs" /> instance containing the event data.</param>
		private void lv_ToolTipOpening( object sender, ToolTipEventArgs e )
		{
			ListViewItem item = e.Source as ListViewItem;

			HistoricalTransaction transaction = item?.Content as HistoricalTransaction;

			if ( transaction != null )
			{
				transaction.LoadTooltip( );

				item.ToolTip = transaction.Tooltip;
			}
		}

		/// <summary>
		///     Handles the MouseDoubleClick event of the ReadiMonListView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the event data.</param>
		private void ReadiMonListView_MouseDoubleClick( object sender, MouseButtonEventArgs e )
		{
			ReadiMonListView listView = sender as ReadiMonListView;

			if ( e.ChangedButton != MouseButton.Left )
			{
				return;
			}

			DependencyObject source = e.OriginalSource as DependencyObject;

			while ( source != null )
			{
				FrameworkElement element = source as FrameworkElement;

				ListViewItem item = element?.TemplatedParent as ListViewItem;

				if ( item?.Content is HistoricalTransaction )
				{
					break;
				}

				source = VisualTreeHelper.GetParent( source );
			}

			if ( source == null )
			{
				return;
			}

			HistoricalTransaction transaction = listView?.SelectedItem as HistoricalTransaction;

			if ( transaction != null )
			{
				HistoryViewer viewer = new HistoryViewer( transaction.TransactionId, _viewModel.PluginSettings );
				var helper = new WindowInteropHelper( viewer );
				helper.Owner = Process.GetCurrentProcess( ).MainWindowHandle;
				viewer.ShowDialog( );
			}
		}

		/// <summary>
		///     Handles the SelectionChanged event of the ReadiMonListView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
		private void ReadiMonListView_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			ReadiMonListView listView = sender as ReadiMonListView;

			if ( listView == null )
			{
				return;
			}

			HistoryContextMenu.Items.Clear( );

			if ( listView.SelectedItems == null || listView.SelectedItems.Count <= 0 )
			{
				return;
			}

			if ( listView.SelectedItems.Count == 1 )
			{
				MenuItem revertItem = new MenuItem
				{
					Header = "Revert",
					Command = _viewModel.RevertCommand,
					CommandParameter = listView.SelectedItems[ 0 ]
				};
				HistoryContextMenu.Items.Add( revertItem );

				HistoricalTransaction transaction = listView.SelectedItems[ 0 ] as HistoricalTransaction;

				/////
				// Only show RevertTo if the selected transaction is NOT the last transaction in the list
				/////
				if ( transaction?.NextTransaction != null )
				{
					MenuItem revertToItem = new MenuItem
					{
						Header = "Revert To",
						Command = _viewModel.RevertToCommand,
						CommandParameter = listView.SelectedItems[ 0 ]
					};
					HistoryContextMenu.Items.Add( revertToItem );
				}

				Dictionary<string, long> tenantMap = new Dictionary<string, long>( );

				HashSet<string> tenantNames = new HashSet<string>( );

				transaction = transaction?.NextTransaction;

				while ( transaction != null )
				{
					/////
					// If any transaction is for tenant -1 (multiple tenants) do not show specific RevertTo entries.
					/////
					if ( transaction.TenantId == -1 )
					{
						tenantNames.Clear( );
						tenantMap.Clear( );
						break;
					}

					tenantNames.Add( transaction.TenantName );

					tenantMap[ transaction.TenantName ] = transaction.TenantId;

					transaction = transaction.NextTransaction;
				}

				if ( tenantNames.Count > 1 )
				{
					foreach ( string tenantName in tenantNames.OrderBy( t => t ) )
					{
						MenuItem revertToTenantItem = new MenuItem
						{
							Header = $"Revert To ({tenantName} tenant only)",
							Command = _viewModel.RevertToTenantCommand,
							CommandParameter = new Tuple<long, HistoricalTransaction>( tenantMap[ tenantName ], listView.SelectedItems[ 0 ] as HistoricalTransaction )
						};
						HistoryContextMenu.Items.Add( revertToTenantItem );
					}
				}
			}
			else
			{
				List<HistoricalTransaction> sortedList = new List<HistoricalTransaction>( );

				foreach ( HistoricalTransaction transaction in listView.SelectedItems )
				{
					sortedList.Add( transaction );
				}

				sortedList.Sort( ( a, b ) => a.TransactionId.CompareTo( b.TransactionId ) );

				bool consecutive = true;

				for ( int i = 0; i < sortedList.Count - 1; i++ )
				{
					HistoricalTransaction transactionA = sortedList[ i ];
					HistoricalTransaction transactionB = sortedList[ i + 1 ];

					if ( transactionA != null && transactionA.NextTransaction != transactionB )
					{
						consecutive = false;
						break;
					}
				}

				if ( consecutive )
				{
					MenuItem revertRangeItem = new MenuItem
					{
						Header = "Revert Range",
						Command = _viewModel.RevertRangeCommand,
						CommandParameter = listView.SelectedItems
					};
					HistoryContextMenu.Items.Add( revertRangeItem );

					HashSet<string> tenantNames = new HashSet<string>( );

					Dictionary<string, long> tenantMap = new Dictionary<string, long>( );

					foreach ( HistoricalTransaction transaction in listView.SelectedItems )
					{
						tenantNames.Add( transaction.TenantName );

						tenantMap[ transaction.TenantName ] = transaction.TenantId;
					}

					if ( tenantNames.Count > 1 )
					{
						foreach ( string tenantName in tenantNames.OrderBy( t => t ) )
						{
							MenuItem revertToTenantItem = new MenuItem
							{
								Header = $"Revert Range ({tenantName} tenant only)",
								Command = _viewModel.RevertRangeTenantCommand,
								CommandParameter = new Tuple<long, IList>( tenantMap[ tenantName ], listView.SelectedItems )
							};
							HistoryContextMenu.Items.Add( revertToTenantItem );
						}
					}
				}
				else
				{
					MenuItem revertSelectedItem = new MenuItem
					{
						Header = "Revert Selected",
						Command = _viewModel.RevertSelectedCommand,
						CommandParameter = listView.SelectedItems
					};
					HistoryContextMenu.Items.Add( revertSelectedItem );

					HashSet<string> tenantNames = new HashSet<string>( );

					Dictionary<string, long> tenantMap = new Dictionary<string, long>( );

					foreach ( HistoricalTransaction transaction in listView.SelectedItems )
					{
						tenantNames.Add( transaction.TenantName );

						tenantMap[ transaction.TenantName ] = transaction.TenantId;
					}

					if ( tenantNames.Count > 1 )
					{
						foreach ( string tenantName in tenantNames.OrderBy( t => t ) )
						{
							MenuItem revertToTenantItem = new MenuItem
							{
								Header = $"Revert Selected ({tenantName} tenant only)",
								Command = _viewModel.RevertSelectedTenantCommand,
								CommandParameter = new Tuple<long, IList>( tenantMap[ tenantName ], listView.SelectedItems )
							};
							HistoryContextMenu.Items.Add( revertToTenantItem );
						}
					}
				}
			}
		}

		/// <summary>
		///     Handles the SizeChanged event of the UserControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SizeChangedEventArgs" /> instance containing the event data.</param>
		private void UserControl_SizeChanged( object sender, SizeChangedEventArgs e )
		{
			if ( e.HeightChanged && e.NewSize.Height > 85 )
			{
				_viewModel.Height = e.NewSize.Height - 85;
			}
		}
	}
}