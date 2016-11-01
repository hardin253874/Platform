// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ReadiMon.Shared.Controls.TreeListView
{
	/// <summary>
	///     TreeNode class.
	/// </summary>
	public sealed class TreeNode : INotifyPropertyChanged
	{
		#region NodeCollection

		/// <summary>
		///     Node Collection
		/// </summary>
		private class NodeCollection : Collection<TreeNode>
		{
			private readonly TreeNode _owner;

			/// <summary>
			///     Initializes a new instance of the <see cref="NodeCollection" /> class.
			/// </summary>
			/// <param name="owner">The owner.</param>
			public NodeCollection( TreeNode owner )
			{
				_owner = owner;
			}

			/// <summary>
			///     Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
			/// </summary>
			protected override void ClearItems( )
			{
				while ( Count != 0 )
				{
					RemoveAt( Count - 1 );
				}
			}

			/// <summary>
			///     Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1" /> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
			/// <param name="item">The object to insert. The value can be null for reference types.</param>
			/// <exception cref="System.ArgumentNullException">item</exception>
			protected override void InsertItem( int index, TreeNode item )
			{
				if ( item == null )
				{
					throw new ArgumentNullException( "item" );
				}

				if ( item.Parent != _owner )
				{
					if ( item.Parent != null )
					{
						item.Parent.Children.Remove( item );
					}

					item._parent = _owner;
					item._index = index;

					for ( int i = index; i < Count; i++ )
					{
						this[ i ]._index++;
					}

					base.InsertItem( index, item );
				}
			}

			/// <summary>
			///     Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
			/// </summary>
			/// <param name="index">The zero-based index of the element to remove.</param>
			protected override void RemoveItem( int index )
			{
				TreeNode item = this[ index ];
				item._parent = null;
				item._index = -1;

				for ( int i = index + 1; i < Count; i++ )
				{
					this[ i ]._index--;
				}

				base.RemoveItem( index );
			}

			/// <summary>
			///     Replaces the element at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the element to replace.</param>
			/// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
			/// <exception cref="System.ArgumentNullException">item</exception>
			protected override void SetItem( int index, TreeNode item )
			{
				if ( item == null )
				{
					throw new ArgumentNullException( "item" );
				}

				RemoveAt( index );
				InsertItem( index, item );
			}
		}

		#endregion

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

		#region Properties

		private readonly Collection<TreeNode> _children;

		/// <summary>
		///     The nodes
		/// </summary>
		private readonly ReadOnlyCollection<TreeNode> _nodes;

		/// <summary>
		///     The tag
		/// </summary>
		private readonly object _tag;

		/// <summary>
		///     The tree
		/// </summary>
		private readonly TreeList _tree;

		/// <summary>
		///     The children source
		/// </summary>
		private INotifyCollectionChanged _childrenSource;

		/// <summary>
		///     The index
		/// </summary>
		private int _index = -1;

		/// <summary>
		///     The is expanded
		/// </summary>
		private bool _isExpanded;

		/// <summary>
		///     The is selected
		/// </summary>
		private bool _isSelected;


		/// <summary>
		///     The parent
		/// </summary>
		private TreeNode _parent;

		/// <summary>
		///     Gets all visible children.
		/// </summary>
		/// <value>
		///     All visible children.
		/// </value>
		public IEnumerable<TreeNode> AllVisibleChildren
		{
			get
			{
				int level = Level;
				TreeNode node = this;

				while ( true )
				{
					node = node.NextVisibleNode;

					if ( node != null && node.Level > level )
					{
						yield return node;
					}
					else
					{
						break;
					}
				}
			}
		}

		/// <summary>
		///     Gets the bottom node.
		/// </summary>
		/// <value>
		///     The bottom node.
		/// </value>
		internal TreeNode BottomNode
		{
			get
			{
				TreeNode parent = Parent;

				if ( parent != null )
				{
					if ( parent.NextNode != null )
					{
						return parent.NextNode;
					}
					return parent.BottomNode;
				}
				return null;
			}
		}

		internal Collection<TreeNode> Children
		{
			get
			{
				return _children;
			}
		}

		/// <summary>
		///     Gets or sets the children source.
		/// </summary>
		/// <value>
		///     The children source.
		/// </value>
		internal INotifyCollectionChanged ChildrenSource
		{
			get
			{
				return _childrenSource;
			}
			set
			{
				if ( _childrenSource != null )
				{
					_childrenSource.CollectionChanged -= ChildrenChanged;
				}

				_childrenSource = value;

				if ( _childrenSource != null )
				{
					_childrenSource.CollectionChanged += ChildrenChanged;
				}
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance has children.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has children; otherwise, <c>false</c>.
		/// </value>
		public bool HasChildren
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets the index.
		/// </summary>
		/// <value>
		///     The index.
		/// </value>
		public int Index
		{
			get
			{
				return _index;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance is expandable.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is expandable; otherwise, <c>false</c>.
		/// </value>
		public bool IsExpandable
		{
			get
			{
				return ( HasChildren && !IsExpandedOnce ) || Nodes.Count > 0;
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is expanded.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
		/// </value>
		public bool IsExpanded
		{
			get
			{
				return _isExpanded;
			}
			set
			{
				if ( value != IsExpanded )
				{
					Tree.SetIsExpanded( this, value );
					OnPropertyChanged( "IsExpanded" );
					OnPropertyChanged( "IsExpandable" );
				}
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance is expanded once.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is expanded once; otherwise, <c>false</c>.
		/// </value>
		public bool IsExpandedOnce
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is selected.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is selected; otherwise, <c>false</c>.
		/// </value>
		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if ( value != _isSelected )
				{
					_isSelected = value;
					OnPropertyChanged( "IsSelected" );
				}
			}
		}

		/// <summary>
		///     Returns true if all parent nodes of this node are expanded.
		/// </summary>
		internal bool IsVisible
		{
			get
			{
				TreeNode node = _parent;

				while ( node != null )
				{
					if ( !node.IsExpanded )
					{
						return false;
					}

					node = node.Parent;
				}

				return true;
			}
		}

		/// <summary>
		///     Gets the level.
		/// </summary>
		/// <value>
		///     The level.
		/// </value>
		public int Level
		{
			get
			{
				if ( _parent == null )
				{
					return -1;
				}
				return _parent.Level + 1;
			}
		}

		/// <summary>
		///     Gets the next node.
		/// </summary>
		/// <value>
		///     The next node.
		/// </value>
		public TreeNode NextNode
		{
			get
			{
				if ( _parent != null )
				{
					int index = Index;

					if ( index < _parent.Nodes.Count - 1 )
					{
						return _parent.Nodes[ index + 1 ];
					}
				}

				return null;
			}
		}

		/// <summary>
		///     Gets the next visible node.
		/// </summary>
		/// <value>
		///     The next visible node.
		/// </value>
		internal TreeNode NextVisibleNode
		{
			get
			{
				if ( IsExpanded && Nodes.Count > 0 )
				{
					return Nodes[ 0 ];
				}
				TreeNode nn = NextNode;

				if ( nn != null )
				{
					return nn;
				}
				return BottomNode;
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
				return _nodes;
			}
		}

		/// <summary>
		///     Gets the parent.
		/// </summary>
		/// <value>
		///     The parent.
		/// </value>
		public TreeNode Parent
		{
			get
			{
				return _parent;
			}
		}

		/// <summary>
		///     Gets the previous node.
		/// </summary>
		/// <value>
		///     The previous node.
		/// </value>
		public TreeNode PreviousNode
		{
			get
			{
				if ( _parent != null )
				{
					int index = Index;

					if ( index > 0 )
					{
						return _parent.Nodes[ index - 1 ];
					}
				}
				return null;
			}
		}

		/// <summary>
		///     Gets the tag.
		/// </summary>
		/// <value>
		///     The tag.
		/// </value>
		public object Tag
		{
			get
			{
				return _tag;
			}
		}

		/// <summary>
		///     Gets the tree.
		/// </summary>
		/// <value>
		///     The tree.
		/// </value>
		internal TreeList Tree
		{
			get
			{
				return _tree;
			}
		}

		/// <summary>
		///     Gets the visible children count.
		/// </summary>
		/// <value>
		///     The visible children count.
		/// </value>
		public int VisibleChildrenCount
		{
			get
			{
				return AllVisibleChildren.Count( );
			}
		}

		/// <summary>
		///     Assigns the is expanded.
		/// </summary>
		/// <param name="value">if set to <c>true</c> [value].</param>
		internal void AssignIsExpanded( bool value )
		{
			_isExpanded = value;
		}

		#endregion

		/// <summary>
		///     Initializes a new instance of the <see cref="TreeNode" /> class.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="tag">The tag.</param>
		/// <exception cref="System.ArgumentNullException">tree</exception>
		internal TreeNode( TreeList tree, object tag )
		{
			if ( tree == null )
			{
				throw new ArgumentNullException( "tree" );
			}

			_tree = tree;
			_children = new NodeCollection( this );
			_nodes = new ReadOnlyCollection<TreeNode>( _children );
			_tag = tag;
		}

		/// <summary>
		///     Children have changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
		private void ChildrenChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					if ( e.NewItems != null )
					{
						int index = e.NewStartingIndex;
						int rowIndex = Tree.Rows.IndexOf( this );

						foreach ( object obj in e.NewItems )
						{
							Tree.InsertNewNode( this, obj, rowIndex, index );
							index++;
						}
					}

					break;

				case NotifyCollectionChangedAction.Remove:

					if ( Children.Count > e.OldStartingIndex )
					{
						RemoveChildAt( e.OldStartingIndex );
					}
					break;

				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					while ( Children.Count > 0 )
					{
						RemoveChildAt( 0 );
					}

					Tree.CreateChildrenNodes( this );

					break;
			}

			HasChildren = Children.Count > 0;
			OnPropertyChanged( "IsExpandable" );
		}

		/// <summary>
		///     Clears the children source.
		/// </summary>
		/// <param name="node">The node.</param>
		private void ClearChildrenSource( TreeNode node )
		{
			node.ChildrenSource = null;

			foreach ( var n in node.Children )
			{
				ClearChildrenSource( n );
			}
		}

		/// <summary>
		///     Removes the child at.
		/// </summary>
		/// <param name="index">The index.</param>
		private void RemoveChildAt( int index )
		{
			var child = Children[ index ];
			Tree.DropChildrenRows( child, true );
			ClearChildrenSource( child );
			Children.RemoveAt( index );
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			if ( Tag != null )
			{
				return Tag.ToString( );
			}
			return base.ToString( );
		}

		/// <summary>
		/// Gets the object.
		/// </summary>
		/// <value>
		/// The object.
		/// </value>
		public object Object
		{
			get
			{
				return this.Tag;
			}
		}
	}
}