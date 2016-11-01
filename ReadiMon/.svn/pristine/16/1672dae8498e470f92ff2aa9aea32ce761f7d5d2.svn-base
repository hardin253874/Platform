// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReadiMon.Shared.Controls.TreeListView
{
	/// <summary>
	///     TreeList Item class.
	/// </summary>
	public class TreeListItem : ListViewItem, INotifyPropertyChanged
	{
		#region Properties

		/// <summary>
		///     The node
		/// </summary>
		private TreeNode _node;

		/// <summary>
		///     Gets the node.
		/// </summary>
		/// <value>
		///     The node.
		/// </value>
		public TreeNode Node
		{
			get
			{
				return _node;
			}
			internal set
			{
				_node = value;
				OnPropertyChanged( "Node" );
			}
		}

		#endregion

		/// <summary>
		///     Changes the focus.
		/// </summary>
		/// <param name="node">The node.</param>
		private void ChangeFocus( TreeNode node )
		{
			var tree = node.Tree;

			if ( tree != null )
			{
				var item = tree.ItemContainerGenerator.ContainerFromItem( node ) as TreeListItem;

				if ( item != null )
				{
					item.Focus( );
				}
				else
				{
					tree.PendingFocusNode = node;
				}
			}
		}

		/// <summary>
		///     Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached event reaches an element
		///     in its route that is derived from this class. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
		protected override void OnKeyDown( KeyEventArgs e )
		{
			if ( Node != null )
			{
				switch ( e.Key )
				{
					case Key.Right:
						e.Handled = true;

						if ( !Node.IsExpanded )
						{
							Node.IsExpanded = true;
							ChangeFocus( Node );
						}
						else if ( Node.Children.Count > 0 )
						{
							ChangeFocus( Node.Children[ 0 ] );
						}

						break;

					case Key.Left:

						e.Handled = true;

						if ( Node.IsExpanded && Node.IsExpandable )
						{
							Node.IsExpanded = false;
							ChangeFocus( Node );
						}
						else
						{
							ChangeFocus( Node.Parent );
						}

						break;

					case Key.Subtract:
						e.Handled = true;
						Node.IsExpanded = false;
						ChangeFocus( Node );
						break;

					case Key.Add:
						e.Handled = true;
						Node.IsExpanded = true;
						ChangeFocus( Node );
						break;
				}
			}

			if ( !e.Handled )
			{
				base.OnKeyDown( e );
			}
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		///     Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     Called when [property changed].
		/// </summary>
		/// <param name="name">The name.</param>
		private void OnPropertyChanged( string name )
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( name ) );
			}
		}

		#endregion
	}
}