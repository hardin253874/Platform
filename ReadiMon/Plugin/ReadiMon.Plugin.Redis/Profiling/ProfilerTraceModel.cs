// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using ReadiMon.Shared.Controls.TreeListView;

namespace ReadiMon.Plugin.Redis.Profiling
{
	/// <summary>
	///     The ProfilerTraceModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Controls.TreeListView.ITreeModel" />
	public class ProfilerTraceModel : ITreeModel
	{
		/// <summary>
		///     The dispatcher
		/// </summary>
		private readonly Dispatcher _dispatcher;

		/// <summary>
		///     Initializes a new instance of the <see cref="ProfilerTraceModel" /> class.
		/// </summary>
		public ProfilerTraceModel( Dispatcher dispatcher )
		{
			_dispatcher = dispatcher;

			Map = new Dictionary<Guid, ObservableCollection<ProfilerTrace>>( );
			Root = new ObservableCollection<ProfilerTrace>( );
		}

		/// <summary>
		///     Gets or sets the map.
		/// </summary>
		/// <value>
		///     The map.
		/// </value>
		private Dictionary<Guid, ObservableCollection<ProfilerTrace>> Map
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the root.
		/// </summary>
		/// <value>
		///     The root.
		/// </value>
		public ObservableCollection<ProfilerTrace> Root
		{
			get;
			set;
		}

		/// <summary>
		///     Get list of children of the specified parent
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Invalid parent</exception>
		public IEnumerable GetChildren( object parent )
		{
			if ( parent == null )
			{
				return Root;
			}

			var message = parent as ProfilerTrace;

			if ( message == null )
			{
				throw new ArgumentException( "Invalid parent" );
			}

			ObservableCollection<ProfilerTrace> children;

			if ( Map.TryGetValue( message.Id, out children ) )
			{
				return children;
			}

			return null;
		}

		/// <summary>
		///     returns whether specified parent has any children or not.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public bool HasChildren( object parent )
		{
			var message = parent as ProfilerTrace;

			if ( message == null )
			{
				return false;
			}

			return Map.ContainsKey( message.Id );
		}

		/// <summary>
		///     Adds the specified entry.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="parentId">The parent identifier.</param>
		public void Add( ProfilerTrace entry, Guid parentId )
		{
			_dispatcher.Invoke( ( ) =>
			{
				try
				{
					ObservableCollection<ProfilerTrace> children;

					if ( !Map.TryGetValue( parentId, out children ) )
					{
						children = new ObservableCollection<ProfilerTrace>( );
						Map[ parentId ] = children;
					}

					ObservableCollection<ProfilerTrace> grandChildren;
					if ( !entry.IsLeaf && !Map.TryGetValue( entry.Id, out grandChildren ) )
					{
						grandChildren = new ObservableCollection<ProfilerTrace>( );
						Map[ entry.Id ] = grandChildren;
					}

					children.Add( entry );

					if ( parentId == Guid.Empty )
					{
						Root.Add( entry );
					}
				}
				catch ( Exception exc )
				{
					Console.WriteLine( exc.Message );
				}
			} );
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		public void Clear( )
		{
			Map.Clear( );
			Root.Clear( );
		}

		/// <summary>
		///     Removes the children.
		/// </summary>
		/// <param name="parentId">The parent identifier.</param>
		public void RemoveChildren( Guid parentId )
		{
			_dispatcher.Invoke( ( ) =>
			{
				try
				{
					ObservableCollection<ProfilerTrace> children;

					if ( Map.TryGetValue( parentId, out children ) )
					{
						children.Clear( );
					}
				}
				catch ( Exception exc )
				{
					Console.WriteLine( exc.Message );
				}
			} );
		}
	}
}