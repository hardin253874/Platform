// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Occurrence.
	/// </summary>
	[Serializable]
	public struct Occurrence : IComparable<Occurrence>
	{
		/// <summary>
		///     Period.
		/// </summary>
		private IPeriod _period;

		/// <summary>
		///     Source.
		/// </summary>
		private IRecurrable _source;

		/// <summary>
		///     Gets or sets the period.
		/// </summary>
		/// <value>
		///     The period.
		/// </value>
		public IPeriod Period
		{
			get
			{
				return _period;
			}
			set
			{
				_period = value;
			}
		}

		/// <summary>
		///     Gets or sets the source.
		/// </summary>
		/// <value>
		///     The source.
		/// </value>
		public IRecurrable Source
		{
			get
			{
				return _source;
			}
			set
			{
				_source = value;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Occurrence" /> structure.
		/// </summary>
		/// <param name="ao">The occurrence.</param>
		public Occurrence( Occurrence ao )
		{
			_period = ao.Period;
			_source = ao.Source;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Occurrence" /> structure.
		/// </summary>
		/// <param name="recurrable">The recurrable.</param>
		/// <param name="period">The period.</param>
		public Occurrence( IRecurrable recurrable, IPeriod period )
		{
			_source = recurrable;
			_period = period;
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
			if ( obj is Occurrence )
			{
				var o = ( Occurrence ) obj;
				return
					Equals( Source, o.Source ) &&
					CompareTo( o ) == 0;
			}
			return base.Equals( obj );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			int hash = 13;

			if ( Source != null )
			{
				hash = ( hash * 7 ) + Source.GetHashCode( );
			}

			return hash;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			string s = "Occurrence";
			if ( Source != null )
			{
				s = Source.GetType( ).Name + " ";
			}

			if ( Period != null )
			{
				s += "(" + Period.StartTime + ")";
			}

			return s;
		}

		#region IComparable<Occurrence> Members

		/// <summary>
		///     Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
		///     Value
		///     Meaning
		///     Less than zero
		///     This object is less than the <paramref name="other" /> parameter.
		///     Zero
		///     This object is equal to <paramref name="other" />.
		///     Greater than zero
		///     This object is greater than <paramref name="other" />.
		/// </returns>
		public int CompareTo( Occurrence other )
		{
			return Period.CompareTo( other.Period );
		}

		#endregion
	}
}