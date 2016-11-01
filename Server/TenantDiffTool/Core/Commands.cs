// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     Commands
	/// </summary>
	public static class Commands
	{
		/// <summary>
		/// </summary>
		public static readonly DependencyProperty DataGridDoubleClickProperty =
			DependencyProperty.RegisterAttached( "DataGridDoubleClickCommand", typeof( ICommand ), typeof( Commands ),
				new PropertyMetadata( AttachOrRemoveDataGridDoubleClickEvent ) );

		/// <summary>
		/// </summary>
		public static readonly DependencyProperty ListViewDoubleClickProperty =
			DependencyProperty.RegisterAttached( "ListViewDoubleClickCommand", typeof( ICommand ), typeof( Commands ),
				new PropertyMetadata( AttachOrRemoveListViewDoubleClickEvent ) );

		public static readonly DependencyProperty PropertyGridSelectedPropertyChangedProperty =
			DependencyProperty.RegisterAttached( "PropertyGridSelectedPropertyChangedCommand", typeof( ICommand ), typeof( Commands ),
				new PropertyMetadata( AttachOrRemovePropertyGridSelectedPropertyChangedEvent ) );

		/// <summary>
		///     Attaches the or remove data grid double click event.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="args">
		///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
		/// </param>
		public static void AttachOrRemoveDataGridDoubleClickEvent( DependencyObject obj, DependencyPropertyChangedEventArgs args )
		{
			var dataGrid = obj as DataGrid;
			if ( dataGrid != null )
			{
				if ( args.OldValue == null && args.NewValue != null )
				{
					dataGrid.MouseDoubleClick += ExecuteDataGridDoubleClick;
				}
				else if ( args.OldValue != null && args.NewValue == null )
				{
					dataGrid.MouseDoubleClick -= ExecuteDataGridDoubleClick;
				}
			}
		}

		/// <summary>
		///     Attaches the or remove data grid double click event.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="args">
		///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
		/// </param>
		public static void AttachOrRemoveListViewDoubleClickEvent( DependencyObject obj, DependencyPropertyChangedEventArgs args )
		{
			var listView = obj as ListView;

			if ( listView != null )
			{
				if ( args.OldValue == null && args.NewValue != null )
				{
					listView.MouseDoubleClick += ExecuteListViewDoubleClick;
				}
				else if ( args.OldValue != null && args.NewValue == null )
				{
					listView.MouseDoubleClick -= ExecuteListViewDoubleClick;
				}
			}
		}

		/// <summary>
		///     Attaches the or remove property grid selected property changed event.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="args">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		public static void AttachOrRemovePropertyGridSelectedPropertyChangedEvent( DependencyObject obj, DependencyPropertyChangedEventArgs args )
		{
			var propertyGrid = obj as PropertyGrid;

			if ( propertyGrid != null )
			{
				if ( args.OldValue == null && args.NewValue != null )
				{
					propertyGrid.SelectedPropertyItemChanged += ExecuteSelectedPropertyItemChanged;
				}
				else if ( args.OldValue != null && args.NewValue == null )
				{
					propertyGrid.SelectedPropertyItemChanged -= ExecuteSelectedPropertyItemChanged;
				}
			}
		}

		/// <summary>
		///     Gets the data grid double click command.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public static ICommand GetDataGridDoubleClickCommand( DependencyObject obj )
		{
			return ( ICommand ) obj.GetValue( DataGridDoubleClickProperty );
		}

		/// <summary>
		///     Gets the data grid double click command.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public static ICommand GetListViewDoubleClickCommand( DependencyObject obj )
		{
			return ( ICommand ) obj.GetValue( ListViewDoubleClickProperty );
		}

		/// <summary>
		///     Gets the type of the parent of.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="current">The current.</param>
		/// <returns></returns>
		public static T GetParentOfType<T>( DependencyObject current )
			where T : DependencyObject
		{
			for ( DependencyObject parent = VisualTreeHelper.GetParent( current );
				parent != null;
				parent = VisualTreeHelper.GetParent( parent ) )
			{
				var result = parent as T;

				if ( result != null )
				{
					return result;
				}
			}

			return null;
		}

		/// <summary>
		///     Gets the property grid selected property changed command.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static ICommand GetPropertyGridSelectedPropertyChangedCommand( DependencyObject obj )
		{
			return ( ICommand ) obj.GetValue( PropertyGridSelectedPropertyChangedProperty );
		}

		/// <summary>
		///     Properties the grid selected property changed command.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static ICommand PropertyGridSelectedPropertyChangedCommand( DependencyObject obj )
		{
			return ( ICommand ) obj.GetValue( PropertyGridSelectedPropertyChangedProperty );
		}

		/// <summary>
		///     Sets the data grid double click command.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="value">The value.</param>
		public static void SetDataGridDoubleClickCommand( DependencyObject obj, ICommand value )
		{
			obj.SetValue( DataGridDoubleClickProperty, value );
		}

		/// <summary>
		///     Sets the data grid double click command.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="value">The value.</param>
		public static void SetListViewDoubleClickCommand( DependencyObject obj, ICommand value )
		{
			obj.SetValue( ListViewDoubleClickProperty, value );
		}

		/// <summary>
		///     Sets the property grid selected property changed command.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="value">The value.</param>
		public static void SetPropertyGridSelectedPropertyChangedCommand( DependencyObject obj, ICommand value )
		{
			obj.SetValue( PropertyGridSelectedPropertyChangedProperty, value );
		}

		/// <summary>
		///     Executes the data grid double click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">
		///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
		/// </param>
		private static void ExecuteDataGridDoubleClick( object sender, MouseButtonEventArgs args )
		{
			var obj = sender as DependencyObject;

			if ( obj != null )
			{
				var original = args.OriginalSource as DependencyObject;

				if ( original != null )
				{
					var sb = GetParentOfType<ScrollBar>( original );

					if ( sb != null )
					{
						return;
					}
				}

				var cmd = ( ICommand ) obj.GetValue( DataGridDoubleClickProperty );

				if ( cmd != null )
				{
					if ( cmd.CanExecute( obj ) )
					{
						cmd.Execute( obj );
					}
				}
			}
		}

		/// <summary>
		///     Executes the data grid double click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">
		///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
		/// </param>
		private static void ExecuteListViewDoubleClick( object sender, MouseButtonEventArgs args )
		{
			var obj = sender as DependencyObject;

			if ( obj != null )
			{
				var original = args.OriginalSource as DependencyObject;

				if ( original != null )
				{
					var sb = GetParentOfType<ScrollBar>( original );

					if ( sb != null )
					{
						return;
					}
				}

				var cmd = ( ICommand ) obj.GetValue( ListViewDoubleClickProperty );

				if ( cmd != null )
				{
					if ( cmd.CanExecute( obj ) )
					{
						cmd.Execute( obj );
					}
				}
			}
		}

		/// <summary>
		///     Executes the selected property item changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="RoutedPropertyChangedEventArgs{PropertyItem}" /> instance containing the event data.</param>
		private static void ExecuteSelectedPropertyItemChanged( object sender, RoutedPropertyChangedEventArgs<PropertyItem> args )
		{
			var obj = sender as PropertyGrid;

			if ( obj != null )
			{
				var cmd = ( ICommand ) obj.GetValue( PropertyGridSelectedPropertyChangedProperty );

				if ( cmd != null )
				{
					if ( cmd.CanExecute( obj.SelectedPropertyItem ) )
					{
						cmd.Execute( obj.SelectedPropertyItem );
					}
				}
			}
		}
	}
}