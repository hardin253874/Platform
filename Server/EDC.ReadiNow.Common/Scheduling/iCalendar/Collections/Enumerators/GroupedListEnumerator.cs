// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     GroupedListEnumerator type.
	/// </summary>
	/// <typeparam name="TType">The type of the type.</typeparam>
	public class GroupedListEnumerator<TType> : IEnumerator<TType>
	{
		/// <summary>
		///     List of lists.
		/// </summary>
		private readonly IList<IMultiLinkedList<TType>> _lists;

		/// <summary>
		///     List enumerator.
		/// </summary>
		private IEnumerator<TType> _listEnumerator;

		/// <summary>
		///     Lists enumerator.
		/// </summary>
		private IEnumerator<IMultiLinkedList<TType>> _listsEnumerator;

		/// <summary>
		///     Initializes a new instance of the <see cref="GroupedListEnumerator{TType}" /> class.
		/// </summary>
		/// <param name="lists">The lists.</param>
		public GroupedListEnumerator( IList<IMultiLinkedList<TType>> lists )
		{
			_lists = lists;
		}

		/// <summary>
		///     Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <returns>
		///     The element in the collection at the current position of the enumerator.
		/// </returns>
		public virtual TType Current
		{
			get
			{
				if ( _listEnumerator != null )
				{
					return _listEnumerator.Current;
				}
				return default( TType );
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose( )
		{
			Reset( );
		}

		/// <summary>
		///     Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <returns>
		///     The element in the collection at the current position of the enumerator.
		/// </returns>
		object IEnumerator.Current
		{
			get
			{
				if ( _listEnumerator != null )
				{
					return _listEnumerator.Current;
				}
				return default( TType );
			}
		}

		/// <summary>
		///     Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
		/// </returns>
		public virtual bool MoveNext( )
		{
			if ( _listEnumerator != null )
			{
				if ( _listEnumerator.MoveNext( ) )
				{
					return true;
				}

				DisposeListEnumerator( );

				if ( MoveNextList( ) )
				{
					return MoveNext( );
				}
			}
			else
			{
				if ( MoveNextList( ) )
				{
					return MoveNext( );
				}
			}
			return false;
		}

		/// <summary>
		///     Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		public virtual void Reset( )
		{
			if ( _listsEnumerator != null )
			{
				_listsEnumerator.Dispose( );
				_listsEnumerator = null;
			}
		}

		/// <summary>
		///     Disposes the list enumerator.
		/// </summary>
		private void DisposeListEnumerator( )
		{
			if ( _listEnumerator != null )
			{
				_listEnumerator.Dispose( );
				_listEnumerator = null;
			}
		}

		/// <summary>
		///     Moves the next list.
		/// </summary>
		/// <returns></returns>
		private bool MoveNextList( )
		{
			if ( _listsEnumerator == null )
			{
				_listsEnumerator = _lists.GetEnumerator( );
			}

			if ( _listsEnumerator != null )
			{
				if ( _listsEnumerator.MoveNext( ) )
				{
					DisposeListEnumerator( );
					if ( _listsEnumerator.Current != null )
					{
						_listEnumerator = _listsEnumerator.Current.GetEnumerator( );
						return true;
					}
				}
			}

			return false;
		}
	}
}