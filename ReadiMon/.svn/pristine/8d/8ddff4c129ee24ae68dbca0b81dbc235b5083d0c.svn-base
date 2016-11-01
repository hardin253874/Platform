// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ReadiMon.Shared.Controls.TreeListView
{
	/// <summary>
	///     Observable Collection class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ObservableCollectionAdv<T> : ObservableCollection<T>
	{
		/// <summary>
		///     Inserts the range.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="collection">The collection.</param>
		public void InsertRange( int index, IEnumerable<T> collection )
		{
			CheckReentrancy( );

			var items = Items as List<T>;

			if ( items != null )
			{
				items.InsertRange( index, collection );
			}

			OnReset( );
		}

		/// <summary>
		///     Called when [property changed].
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		private void OnPropertyChanged( string propertyName )
		{
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		/// <summary>
		///     Called when [reset].
		/// </summary>
		private void OnReset( )
		{
			OnPropertyChanged( "Count" );
			OnPropertyChanged( "Item[]" );
			OnCollectionChanged( new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Reset ) );
		}

		/// <summary>
		///     Removes the range.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		public void RemoveRange( int index, int count )
		{
			CheckReentrancy( );

			var items = Items as List<T>;

			if ( items != null )
			{
				items.RemoveRange( index, count );
			}

			OnReset( );
		}
	}
}