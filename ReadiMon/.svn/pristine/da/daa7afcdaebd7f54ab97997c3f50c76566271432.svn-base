// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ReadiMon.Shared.Controls.TreeListView
{
	/// <summary>
	///     TreeList class.
	/// </summary>
	public class TreeList : ListView
	{
		#region Properties

		/// <summary>
		///     The root
		/// </summary>
		private readonly TreeNode _root;

		/// <summary>
		///     The model
		/// </summary>
		private ITreeModel _model;

		/// <summary>
		///     Gets or sets the model.
		/// </summary>
		/// <value>
		///     The model.
		/// </value>
		public ITreeModel Model
		{
			get
			{
				return _model;
			}
			set
			{
				if ( _model != value )
				{
					_model = value;
					if ( _root != null )
						_root.Children.Clear( );
					try
					{
						if ( Rows != null )
							Rows.Clear( );
					}
					catch
					{
					}
					if ( _root != null )
						CreateChildrenNodes( _root );
				}
			}
		}

		/// <summary>
		///     Gets the nodes.
		/// </summary>
		/// <value>
		///     The nodes.
		/// </value>
		public ReadOnlyCollection<TreeNode> Nodes
		{
			get
			{
				return Root.Nodes;
			}
		}

		/// <summary>
		///     Gets or sets the pending focus node.
		/// </summary>
		/// <value>
		///     The pending focus node.
		/// </value>
		internal TreeNode PendingFocusNode
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the root.
		/// </summary>
		/// <value>
		///     The root.
		/// </value>
		internal TreeNode Root
		{
			get
			{
				return _root;
			}
		}

		/// <summary>
		///     Internal collection of rows representing visible nodes, actually displayed in the ListView
		/// </summary>
		internal ObservableCollectionAdv<TreeNode> Rows
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the selected node.
		/// </summary>
		/// <value>
		///     The selected node.
		/// </value>
		public TreeNode SelectedNode
		{
			get
			{
				if ( SelectedItems.Count > 0 )
				{
					return SelectedItems[ 0 ] as TreeNode;
				}

				return null;
			}
		}

		/// <summary>
		///     Gets the selected nodes.
		/// </summary>
		/// <value>
		///     The selected nodes.
		/// </value>
		public ICollection<TreeNode> SelectedNodes
		{
			get
			{
				return SelectedItems.Cast<TreeNode>( ).ToArray( );
			}
		}

		#endregion

		/// <summary>
		///     Initializes a new instance of the <see cref="TreeList" /> class.
		/// </summary>
		public TreeList( )
		{
			Rows = new ObservableCollectionAdv<TreeNode>( );

			_root = new TreeNode( this, null )
			{
				IsExpanded = true
			};

			_root.PropertyChanged += TreeNode_PropertyChanged;

			ItemsSource = Rows;
			ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
		}

		void TreeNode_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			if ( e.PropertyName == "IsExpanded" )
			{
				var node = sender as TreeNode;

				if ( node != null )
				{
					var expandable = node.Tag as IExpandable;

					if ( expandable != null )
					{
						if ( node.IsExpanded )
						{
							expandable.OnExpand( );
						}
						else
						{
							expandable.OnCollapse( );
						}
					}
				}
			}
		}

		/// <summary>
		///     Creates the children nodes.
		/// </summary>
		/// <param name="node">The node.</param>
		internal void CreateChildrenNodes( TreeNode node )
		{
			var children = GetChildren( node );
			if ( children != null )
			{
				int rowIndex = Rows.IndexOf( node );

				node.Children.Clear( ); // To not have two exemplaries of the first child.

				node.ChildrenSource = children as INotifyCollectionChanged;

				foreach ( object obj in children )
				{
					var child = new TreeNode( this, obj );
					child.PropertyChanged += TreeNode_PropertyChanged;
					child.HasChildren = HasChildren( child );
					
					if ( child.ChildrenSource == null )
						child.ChildrenSource = GetChildren( child ) as INotifyCollectionChanged; // If not, this branch will not be notified of the addition of a child.
					node.Children.Add( child );
				}

				if ( node.IsExpanded )
				{
					Rows.InsertRange( rowIndex + 1, node.Children.ToArray( ) );
				}
			}
			node.HasChildren = HasChildren( node );
		}

		/// <summary>
		///     Creates the children rows.
		/// </summary>
		/// <param name="node">The node.</param>
		private void CreateChildrenRows( TreeNode node )
		{
			int index = Rows.IndexOf( node );

			if ( index >= 0 || node == _root ) // ignore invisible nodes
			{
				var nodes = node.AllVisibleChildren.ToArray( );
				Rows.InsertRange( index + 1, nodes );
			}
		}

		/// <summary>
		///     Drops the children rows.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="removeParent">if set to <c>true</c> [remove parent].</param>
		internal void DropChildrenRows( TreeNode node, bool removeParent )
		{
			int start = Rows.IndexOf( node );

			if ( start >= 0 || node == _root ) // ignore invisible nodes
			{
				int count = node.VisibleChildrenCount;

				if ( removeParent )
				{
					count++;
				}
				else
				{
					start++;
				}

				Rows.RemoveRange( start, count );
			}
		}

		/// <summary>
		///     Gets the children.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <returns></returns>
		private IEnumerable GetChildren( TreeNode parent )
		{
			if ( Model != null )
			{
				return Model.GetChildren( parent.Tag );
			}

			return null;
		}

		/// <summary>
		///     Creates and returns a new <see cref="T:System.Windows.Controls.ListViewItem" /> container.
		/// </summary>
		/// <returns>
		///     A new <see cref="T:System.Windows.Controls.ListViewItem" /> control.
		/// </returns>
		protected override DependencyObject GetContainerForItemOverride( )
		{
			return new TreeListItem( );
		}

		/// <summary>
		///     Determines whether the specified parent has children.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <returns></returns>
		private bool HasChildren( TreeNode parent )
		{
			if ( parent == Root )
			{
				return true;
			}

			if ( Model != null )
			{
				return Model.HasChildren( parent.Tag );
			}

			return false;
		}

		/// <summary>
		///     Inserts the new node.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="rowIndex">Index of the row.</param>
		/// <param name="index">The index.</param>
		internal void InsertNewNode( TreeNode parent, object tag, int rowIndex, int index )
		{
			int visibleChildCount = 0;
			bool insert = index >= 0 && index < parent.Children.Count;

			int row = 0;
			if ( insert && ( parent.VisibleChildrenCount > 0 ) )
			{
				if ( index < parent.Children.Count )
					row = Rows.IndexOf( parent.Children [ index ] ); // we insert in an existing location, so we use its row.
				else
					row = Rows.IndexOf( parent.Children [ parent.Children.Count - 1 ] ) + 1; // we add at the end of the list, just after the last item, so we use its row+1.
			}

			var node = new TreeNode( this, tag );
			node.PropertyChanged += TreeNode_PropertyChanged;
			if ( insert )
				parent.Children.Insert( index, node );
			else
			{
				parent.HasChildren = HasChildren( parent );
				visibleChildCount += parent.Children.Sum( item => item.VisibleChildrenCount );
				parent.Children.Add( node );
			}

			if ( parent.VisibleChildrenCount > 0 )
			{
				if ( !insert )	// added at the end of the list of children
					row = rowIndex + index + 1 + visibleChildCount;
				Rows.Insert( row, node );
			}

			node.HasChildren = HasChildren( node );
		}

		/// <summary>
		///     Determines whether an object is a <see cref="T:System.Windows.Controls.ListViewItem" />.
		/// </summary>
		/// <param name="item">The object to evaluate.</param>
		/// <returns>
		///     true if the <paramref name="item" /> is a <see cref="T:System.Windows.Controls.ListViewItem" />; otherwise, false.
		/// </returns>
		protected override bool IsItemItsOwnContainerOverride( object item )
		{
			return item is TreeListItem;
		}

		/// <summary>
		///     Items the container generator status changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void ItemContainerGeneratorStatusChanged( object sender, EventArgs e )
		{
			if ( ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && PendingFocusNode != null )
			{
				var item = ItemContainerGenerator.ContainerFromItem( PendingFocusNode ) as TreeListItem;

				if ( item != null )
				{
					item.Focus( );
				}

				PendingFocusNode = null;
			}
		}

		/// <summary>
		///     Sets the styles, templates, and bindings for a <see cref="T:System.Windows.Controls.ListViewItem" />.
		/// </summary>
		/// <param name="element">
		///     An object that is a <see cref="T:System.Windows.Controls.ListViewItem" /> or that can be
		///     converted into one.
		/// </param>
		/// <param name="item">The object to use to create the <see cref="T:System.Windows.Controls.ListViewItem" />.</param>
		protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
		{
			var ti = element as TreeListItem;
			var node = item as TreeNode;

			if ( ti != null && node != null )
			{
				ti.Node = item as TreeNode;
				base.PrepareContainerForItemOverride( element, node.Tag );
			}
		}

		/// <summary>
		///     Sets the is expanded.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		internal void SetIsExpanded( TreeNode node, bool value )
		{
			if ( value )
			{
				if ( !node.IsExpandedOnce )
				{
					node.IsExpandedOnce = true;
					node.AssignIsExpanded( true );
					CreateChildrenNodes( node );
				}
				else
				{
					node.AssignIsExpanded( true );
					CreateChildrenRows( node );
				}
			}
			else
			{
				DropChildrenRows( node, false );
				node.AssignIsExpanded( false );
			}
		}
	}
}