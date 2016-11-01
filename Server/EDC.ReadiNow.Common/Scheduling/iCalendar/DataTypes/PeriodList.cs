// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     An iCalendar list of recurring dates (or date exclusions)
	/// </summary>
	[Serializable]
	public class PeriodList : EncodableDataType, IPeriodList
	{
		/// <summary>
		///     Periods.
		/// </summary>
		private IList<IPeriod> _periods = new List<IPeriod>( );

		/// <summary>
		///     Time zone id.
		/// </summary>
		private string _tzid;

		/// <summary>
		///     Gets or sets the TZID.
		/// </summary>
		/// <value>
		///     The TZID.
		/// </value>
		public string TzId
		{
			get
			{
				return _tzid;
			}
			set
			{
				_tzid = value;
			}
		}

		/// <summary>
		///     Gets or sets the periods.
		/// </summary>
		/// <value>
		///     The periods.
		/// </value>
		protected IList<IPeriod> Periods
		{
			get
			{
				return _periods;
			}
			set
			{
				_periods = value;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PeriodList" /> class.
		/// </summary>
		public PeriodList( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PeriodList" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public PeriodList( string value )
			: this( )
		{
			var serializer = new PeriodListSerializer( );
			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			SetService( new PeriodListEvaluator( this ) );
		}

		/// <summary>
		///     Called when [deserializing].
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserializing( StreamingContext context )
		{
			base.OnDeserializing( context );

			Initialize( );
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="System.Object" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var periodList = obj as IPeriodList;

			if ( periodList != null )
			{
				IPeriodList r = periodList;

				IEnumerator<IPeriod> p1Enum = GetEnumerator( );
				IEnumerator<IPeriod> p2Enum = r.GetEnumerator( );

				while ( p1Enum.MoveNext( ) )
				{
					if ( !p2Enum.MoveNext( ) )
					{
						return false;
					}

					if ( !Equals( p1Enum.Current, p2Enum.Current ) )
					{
						return false;
					}
				}

				if ( p2Enum.MoveNext( ) )
				{
					return false;
				}

				return true;
			}
// ReSharper disable BaseObjectEqualsIsObjectEquals
			return base.Equals( obj );
// ReSharper restore BaseObjectEqualsIsObjectEquals
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			return this.Aggregate( 0, ( current, p ) => current ^ p.GetHashCode( ) );
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var periodList = obj as IPeriodList;

			if ( periodList != null )
			{
				IPeriodList rdt = periodList;
				foreach ( IPeriod p in rdt )
				{
					Add( p.Copy<IPeriod>( ) );
				}
			}
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			var serializer = new PeriodListSerializer( );
			return serializer.SerializeToString( this );
		}

		/// <summary>
		///     Evaluates the specified start date.
		/// </summary>
		/// <param name="startDate">The start date.</param>
		/// <param name="fromDate">From date.</param>
		/// <param name="endDate">The end date.</param>
		/// <returns></returns>
		public List<Period> Evaluate( iCalDateTime startDate, iCalDateTime fromDate, iCalDateTime endDate )
		{
			var periods = new List<Period>( );

			if ( startDate > fromDate )
			{
				fromDate = startDate;
			}

			if ( endDate < fromDate ||
			     fromDate > endDate )
			{
				return periods;
			}

			foreach ( Period p in Periods )
			{
				if ( !periods.Contains( p ) )
				{
					periods.Add( p );
				}
			}

			return periods;
		}

		#region IPeriodList Members

		/// <summary>
		///     Adds the specified date time.
		/// </summary>
		/// <param name="dt">The date time.</param>
		public virtual void Add( IDateTime dt )
		{
			Periods.Add( new Period( dt ) );
		}

		/// <summary>
		///     Removes the specified date time.
		/// </summary>
		/// <param name="dt">The date time.</param>
		public virtual void Remove( IDateTime dt )
		{
			Periods.Remove( new Period( dt ) );
		}

		/// <summary>
		///     Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public IPeriod this[ int index ]
		{
			get
			{
				return _periods[ index ];
			}
			set
			{
				_periods[ index ] = value;
			}
		}

		#endregion

		#region IList<IPeriod> Members

		/// <summary>
		///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		public virtual void Add( IPeriod item )
		{
			_periods.Add( item );
		}

		/// <summary>
		///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public virtual void Clear( )
		{
			_periods.Clear( );
		}

		/// <summary>
		///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">
		///     The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		/// <returns>
		///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains( IPeriod item )
		{
			return _periods.Contains( item );
		}

		/// <summary>
		///     Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo( IPeriod[] array, int arrayIndex )
		{
			_periods.CopyTo( array, arrayIndex );
		}

		/// <summary>
		///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <returns>
		///     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public int Count
		{
			get
			{
				return _periods.Count;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <returns>
		///     true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		///     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		/// <returns>
		///     true if <paramref name="item" /> was successfully removed from the
		///     <see
		///         cref="T:System.Collections.Generic.ICollection`1" />
		///     ; otherwise, false. This method also returns false if
		///     <paramref
		///         name="item" />
		///     is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public bool Remove( IPeriod item )
		{
			return _periods.Remove( item );
		}

		/// <summary>
		///     Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </param>
		/// <returns>
		///     The index of <paramref name="item" /> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf( IPeriod item )
		{
			return _periods.IndexOf( item );
		}

		/// <summary>
		///     Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">
		///     The zero-based index at which <paramref name="item" /> should be inserted.
		/// </param>
		/// <param name="item">
		///     The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </param>
		public void Insert( int index, IPeriod item )
		{
			_periods.Insert( index, item );
		}

		/// <summary>
		///     Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		public void RemoveAt( int index )
		{
			_periods.RemoveAt( index );
		}

		#endregion

		#region IEnumerable<IPeriod> Members

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<IPeriod> GetEnumerator( )
		{
			return _periods.GetEnumerator( );
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return _periods.GetEnumerator( );
		}

		#endregion
	}
}