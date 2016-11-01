// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     History tracker.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class History<T>
	{
		/// <summary>
		///     The history.
		/// </summary>
		private readonly List<T> _history = new List<T>( );

		/// <summary>
		///     The current index.
		/// </summary>
		private int _index = -1;

		/// <summary>
		///     Gets a value indicating whether the current item represents the end item.
		/// </summary>
		/// <value>
		///     <c>true</c> if at the end; otherwise, <c>false</c>.
		/// </value>
		public bool AtEnd
		{
			get
			{
				return _index >= _history.Count - 1;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the current item represents the first item.
		/// </summary>
		/// <value>
		///     <c>true</c> if at the front; otherwise, <c>false</c>.
		/// </value>
		public bool AtFront
		{
			get
			{
				return _index <= 0;
			}
		}

		/// <summary>
		///     Gets the current.
		/// </summary>
		/// <value>
		///     The current.
		/// </value>
		public T Current
		{
			get
			{
				if ( _index < 0 || _index >= _history.Count )
				{
					return default( T );
				}

				return _history[ _index ];
			}
		}

		/// <summary>
		///     Adds the specified site.
		/// </summary>
		/// <param name="site">The site.</param>
		public void Add( T site )
		{
			if ( _history.Count > 0 )
			{
				if ( EqualityComparer<T>.Default.Equals( site, Current ) )
				{
					return;
				}

				if ( _index != _history.Count - 1 )
				{
					_history.RemoveRange( _index + 1, _history.Count - _index - 1 );
				}
			}

			_history.Add( site );
			_index++;
		}

		/// <summary>
		/// Removes the current.
		/// </summary>
		public void RemoveCurrent( )
		{
			if ( _history.Count > 0 )
			{
				if ( _index >= 0 )
				{
					_history.RemoveRange( _index, _history.Count - _index );
				}
			}
		}

		/// <summary>
		///     Moves the back.
		/// </summary>
		public void MoveBack( )
		{
			if ( _index <= 0 )
			{
				return;
			}

			_index--;
		}

		/// <summary>
		///     Moves the forward.
		/// </summary>
		public void MoveForward( )
		{
			if ( _index >= _history.Count - 1 )
			{
				return;
			}

			_index++;
		}
	}
}