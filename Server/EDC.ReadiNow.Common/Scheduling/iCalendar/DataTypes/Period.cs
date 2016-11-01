// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Represents an iCalendar period of time.
	/// </summary>
	[Serializable]
	public class Period : EncodableDataType, IPeriod
	{
		/// <summary>
		///     Start time.
		/// </summary>
		private IDateTime _startTime;

		/// <summary>
		///     End time.
		/// </summary>
		private IDateTime _endTime;

		/// <summary>
		///     Duration.
		/// </summary>
		private TimeSpan _duration;

		/// <summary>
		///     Matches date only.
		/// </summary>
		private bool _matchesDateOnly;

		/// <summary>
		///     Initializes a new instance of the <see cref="Period" /> class.
		/// </summary>
		public Period( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Period" /> class.
		/// </summary>
		/// <param name="occurs">The occurs.</param>
		public Period( IDateTime occurs )
			: this( occurs, default( TimeSpan ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Period" /> class.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		public Period( IDateTime start, IDateTime end )
			: this( )
		{
			StartTime = start;
			if ( end != null )
			{
				EndTime = end;
				Duration = end.Subtract( start );
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Period" /> class.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <param name="duration">The duration.</param>
		public Period( IDateTime start, TimeSpan duration )
			: this( )
		{
			StartTime = start;
			if ( duration != default( TimeSpan ) )
			{
				Duration = duration;
				EndTime = start.Add( duration );
			}
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var p = obj as IPeriod;
			if ( p != null )
			{
				StartTime = p.StartTime;
				EndTime = p.EndTime;
				Duration = p.Duration;
				MatchesDateOnly = p.MatchesDateOnly;
			}
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
			var period = obj as IPeriod;

			if ( period != null )
			{
				IPeriod p = period;
				if ( MatchesDateOnly || p.MatchesDateOnly )
				{
					return
						StartTime.Value.Date == p.StartTime.Value.Date &&
						(
							EndTime == null ||
							p.EndTime == null ||
							EndTime.Value.Date.Equals( p.EndTime.Value.Date )
						);
				}

				return
					StartTime.Equals( p.StartTime ) &&
					(
						EndTime == null ||
						p.EndTime == null ||
						EndTime.Equals( p.EndTime )
					);
			}
			return false;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			if ( StartTime != null )
			{
				if ( EndTime != null )
				{
					return ( StartTime.GetHashCode( ) * 23 ) + EndTime.GetHashCode( );
				}
				return StartTime.GetHashCode( );
			}
			return 0;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			var periodSerializer = new PeriodSerializer( );
			return periodSerializer.SerializeToString( this );
		}

		/// <summary>
		///     Extrapolates the times.
		/// </summary>
		private void ExtrapolateTimes( )
		{
			if ( EndTime == null && StartTime != null && Duration != default( TimeSpan ) )
			{
				EndTime = StartTime.Add( Duration );
			}
			else if ( Duration == default( TimeSpan ) && StartTime != null && EndTime != null )
			{
				Duration = EndTime.Subtract( StartTime );
			}
			else if ( StartTime == null && Duration != default( TimeSpan ) && EndTime != null )
			{
				StartTime = EndTime.Subtract( Duration );
			}
		}

		#region IPeriod Members

		/// <summary>
		///     Gets or sets the start time.
		/// </summary>
		/// <value>
		///     The start time.
		/// </value>
		public IDateTime StartTime
		{
			get
			{
				return _startTime;
			}
			set
			{
				_startTime = value;
				ExtrapolateTimes( );
			}
		}

		/// <summary>
		///     Gets or sets the end time.
		/// </summary>
		/// <value>
		///     The end time.
		/// </value>
		public IDateTime EndTime
		{
			get
			{
				return _endTime;
			}
			set
			{
				_endTime = value;
				ExtrapolateTimes( );
			}
		}

		/// <summary>
		///     Gets or sets the duration.
		/// </summary>
		/// <value>
		///     The duration.
		/// </value>
		public TimeSpan Duration
		{
			get
			{
				return _duration;
			}
			set
			{
				if ( !Equals( _duration, value ) )
				{
					_duration = value;
					ExtrapolateTimes( );
				}
			}
		}

		/// <summary>
		///     When true, comparisons between this and other <see cref="Period" />
		///     objects are matched against the date only, and
		///     not the date-time combination.
		/// </summary>
		public bool MatchesDateOnly
		{
			get
			{
				return _matchesDateOnly;
			}
			set
			{
				_matchesDateOnly = value;
			}
		}

		/// <summary>
		///     Determines whether [contains] [the specified dt].
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified dt]; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains( IDateTime dt )
		{
			// Start time is inclusive
			if ( dt != null &&
			     StartTime != null &&
			     StartTime.LessThanOrEqual( dt ) )
			{
				// End time is exclusive
				if ( EndTime == null || EndTime.GreaterThan( dt ) )
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		///     Collideses the with.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		public bool CollidesWith( IPeriod period )
		{
			if ( period != null &&
			     (
				     ( period.StartTime != null && Contains( period.StartTime ) ) ||
				     ( period.EndTime != null && Contains( period.EndTime ) )
			     ) )
			{
				return true;
			}
			return false;
		}

		#endregion

		#region IComparable Members

		/// <summary>
		///     Compares to.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">p</exception>
		/// <exception cref="System.Exception">An error occurred while comparing Period values.</exception>
		public int CompareTo( IPeriod p )
		{
			if ( p == null )
			{
				throw new ArgumentNullException( "p" );
			}

			if ( Equals( p ) )
			{
				return 0;
			}

			if ( StartTime.LessThan( p.StartTime ) )
			{
				return -1;
			}

			if ( StartTime.GreaterThanOrEqual( p.StartTime ) )
			{
				return 1;
			}

			throw new Exception( "An error occurred while comparing Period values." );
		}

		#endregion
	}
}