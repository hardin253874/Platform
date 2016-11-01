// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using ReadiMon.Core;

namespace ReadiMon.Behaviors
{
	/// <summary>
	///     Bindable TreeView SelectedItem behavior.
	/// </summary>
	/// <remarks>
	///     http://stackoverflow.com/a/18700099/1227389
	/// </remarks>
	public class BindableTreeViewSelectedItemBehavior : Behavior<TreeView>
	{
		#region SelectedItem Property

		/// <summary>
		///     The selected item property
		/// </summary>
		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register( "SelectedItem", typeof ( object ), typeof ( BindableTreeViewSelectedItemBehavior ), new UIPropertyMetadata( null, OnSelectedItemChanged ) );

		/// <summary>
		///     Gets or sets the selected item.
		/// </summary>
		/// <value>
		///     The selected item.
		/// </value>
		public object SelectedItem
		{
			get
			{
				return GetValue( SelectedItemProperty );
			}
			set
			{
				SetValue( SelectedItemProperty, value );
			}
		}

		/// <summary>
		///     Gets the TreeView item.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		private static TreeViewItem GetTreeViewItem( ItemsControl container, object item )
		{
			if ( container != null )
			{
				if ( container.DataContext == item )
				{
					return container as TreeViewItem;
				}

				/////
				// Expand the current container
				/////
				if ( container is TreeViewItem && !( ( TreeViewItem ) container ).IsExpanded )
				{
					container.SetValue( TreeViewItem.IsExpandedProperty, true );
				}

				container.ApplyTemplate( );

				var itemsPresenter = ( ItemsPresenter ) container.Template.FindName( "ItemsHost", container );

				if ( itemsPresenter != null )
				{
					itemsPresenter.ApplyTemplate( );
				}
				else
				{
					itemsPresenter = container.GetVisualDescendant<ItemsPresenter>( );

					if ( itemsPresenter == null )
					{
						container.UpdateLayout( );
						itemsPresenter = container.GetVisualDescendant<ItemsPresenter>( );
					}
				}

				if ( itemsPresenter != null )
				{
					var itemsHostPanel = ( Panel ) VisualTreeHelper.GetChild( itemsPresenter, 0 );

#pragma warning disable 168
					// ReSharper disable once UnusedVariable
					var children = itemsHostPanel.Children;
#pragma warning restore 168

					for ( int i = 0,
						count = container.Items.Count; i < count; i++ )
					{
						var subContainer = ( TreeViewItem ) container.ItemContainerGenerator.ContainerFromIndex( i );

						subContainer.BringIntoView( );

						var resultContainer = GetTreeViewItem( subContainer, item );

						if ( resultContainer != null )
						{
							return resultContainer;
						}

						//subContainer.IsExpanded = false;
					}
				}
			}

			return null;
		}

		/// <summary>
		///     Called when the selected item has changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		private static void OnSelectedItemChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
		{
			var item = e.NewValue as TreeViewItem;

			if ( item != null )
			{
				item.SetValue( TreeViewItem.IsSelectedProperty, true );
			}

			var behavior = ( BindableTreeViewSelectedItemBehavior ) sender;

			var treeView = behavior.AssociatedObject;

			if ( treeView == null )
			{
				return;
			}

			item = GetTreeViewItem( treeView, e.NewValue );

			if ( item != null )
			{
				item.IsSelected = true;
			}
		}

		#endregion

		/// <summary>
		///     Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>
		///     Override this to hook up functionality to the AssociatedObject.
		/// </remarks>
		protected override void OnAttached( )
		{
			base.OnAttached( );

			AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
		}

		/// <summary>
		///     Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
		/// </summary>
		/// <remarks>
		///     Override this to unhook functionality from the AssociatedObject.
		/// </remarks>
		protected override void OnDetaching( )
		{
			base.OnDetaching( );

			if ( AssociatedObject != null )
			{
				AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
			}
		}

		/// <summary>
		///     Called when the TreeView selected item has changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The instance containing the event data.</param>
		private void OnTreeViewSelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
		{
			SelectedItem = e.NewValue;
		}
	}
}