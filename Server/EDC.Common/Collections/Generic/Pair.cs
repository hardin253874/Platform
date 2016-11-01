// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     Represents a generic pair.
	/// </summary>
	[Serializable]
	[Immutable]
	public struct Pair<TFirst, TSecond>
	{
		private readonly TFirst _first;
		private readonly TSecond _second;

		/////
		// ReSharper disable CompareNonConstrainedGenericWithNull
		/////

		/// <summary>
		///     Initializes a new instance of the Pair class.
		/// </summary>
		/// <param name="first">
		///     The first object in the pair.
		/// </param>
		/// <param name="second">
		///     The second object in the pair.
		/// </param>
		public Pair( TFirst first, TSecond second )
		{
			_first = first;
			_second = second;
		}

		/// <summary>
		///     Gets or sets the first object in the pair.
		/// </summary>
		public TFirst First
		{
			[DebuggerStepThrough]
			get
			{
				return _first;
			}
		}

		/// <summary>
		///     Gets or sets the second object in the pair.
		/// </summary>
		public TSecond Second
		{
			[DebuggerStepThrough]
			get
			{
				return _second;
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
			return obj is Pair<TFirst, TSecond> && this == ( Pair<TFirst, TSecond> ) obj;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			unchecked
			{
				int hash = 17;

				if ( First != null )
				{
					hash = hash * 92821 + First.GetHashCode( );
				}

				if ( Second != null )
				{
					hash = hash * 92821 + Second.GetHashCode( );
				}

				return hash;
			}
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( Pair<TFirst, TSecond> a, Pair<TFirst, TSecond> b )
		{
			if ( a.First == null )
			{
				if ( b.First != null )
				{
					/////
					// a.First null b.First not-null
					/////
					return false;
				}
			}
			else
			{
				if ( b.First == null )
				{
					/////
					// a.First not null, b.First null
					/////
					return false;
				}

				if ( !a.First.Equals( b.First ) )
				{
					/////
					// a.First != b.First
					/////
					return false;
				}
			}

			if ( a.Second == null )
			{
				if ( b.Second != null )
				{
					/////
					// a.Second null b.Second not-null
					/////
					return false;
				}
			}
			else
			{
				if ( b.Second == null )
				{
					/////
					// a.Second not-null b.Second null
					/////
					return false;
				}

				if ( !a.Second.Equals( b.Second ) )
				{
					/////
					// a.Second != b.Second
					/////
					return false;
				}
			}

			/////
			// Equal
			/////
			return true;
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( Pair<TFirst, TSecond> a, Pair<TFirst, TSecond> b )
		{
			return !( a == b );
		}

        /// <summary>
        /// Show a human readable version.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0},{1}", First, Second);
        }

		/////
		// ReSharper restore CompareNonConstrainedGenericWithNull
		/////
	}
}