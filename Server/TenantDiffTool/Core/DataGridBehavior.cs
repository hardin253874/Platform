// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     DataGrid Behaviour
	/// </summary>
	public class DataGridBehavior
	{
		#region AutoScrollIntoView

		/// <summary>
		/// </summary>
		public static readonly DependencyProperty AutoScrollIntoViewProperty =
			DependencyProperty.RegisterAttached(
				"AutoScrollIntoView",
				typeof( bool ),
				typeof( DataGridBehavior ),
				new UIPropertyMetadata( false, OnAutoScrollIntoViewWhenSelectionChanged ) );

		/// <summary>
		///     Gets the auto scroll into view.
		/// </summary>
		/// <param name="dataGrid">The data grid.</param>
		/// <returns></returns>
		public static bool GetAutoScrollIntoView( DataGrid dataGrid )
		{
			return ( bool ) dataGrid.GetValue( AutoScrollIntoViewProperty );
		}

		/// <summary>
		///     Sets the auto scroll into view.
		/// </summary>
		/// <param name="dataGrid">The data grid.</param>
		/// <param name="value">
		///     if set to <c>true</c> [value].
		/// </param>
		public static void SetAutoScrollIntoView( DataGrid dataGrid, bool value )
		{
			dataGrid.SetValue( AutoScrollIntoViewProperty, value );
		}

		/// <summary>
		///     Called when [auto scroll into view when selection changed].
		/// </summary>
		/// <param name="depObj">The dependency obj.</param>
		/// <param name="e">
		///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
		/// </param>
		private static void OnAutoScrollIntoViewWhenSelectionChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs e )
		{
			var dataGrid = depObj as DataGrid;
			if ( dataGrid == null )
			{
				return;
			}

			if ( !( e.NewValue is bool ) )
			{
				return;
			}

			if ( ( bool ) e.NewValue )
			{
				dataGrid.SelectionChanged += OnDataGridSelectionChanged;
			}
			else
			{
				dataGrid.SelectionChanged -= OnDataGridSelectionChanged;
			}
		}

		/// <summary>
		///     Called when [data grid selection changed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">
		///     The <see cref="RoutedEventArgs" /> instance containing the event data.
		/// </param>
		private static void OnDataGridSelectionChanged( object sender, RoutedEventArgs e )
		{
			if ( !ReferenceEquals( sender, e.OriginalSource ) )
			{
				return;
			}

			var dataGrid = e.OriginalSource as DataGrid;
			if ( dataGrid != null && dataGrid.SelectedItem != null )
			{
				dataGrid.ScrollIntoView( dataGrid.SelectedItem );
			}
		}

		#endregion // AutoScrollIntoView
	}
}