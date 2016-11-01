// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Linq;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     An iCalendar status code.
	/// </summary>
	[Serializable]
	public class StatusCode : EncodableDataType, IStatusCode
	{
		/// <summary>
		///     Parts.
		/// </summary>
		private int[] _parts;

		/// <summary>
		///     Gets or sets the parts.
		/// </summary>
		/// <value>
		///     The parts.
		/// </value>
		public int[] Parts
		{
			get
			{
				return _parts;
			}
			set
			{
				_parts = value;
			}
		}

		/// <summary>
		///     Gets the primary.
		/// </summary>
		/// <value>
		///     The primary.
		/// </value>
		public int Primary
		{
			get
			{
				if ( _parts.Length > 0 )
				{
					return _parts[ 0 ];
				}
				return 0;
			}
		}

		/// <summary>
		///     Gets the secondary.
		/// </summary>
		/// <value>
		///     The secondary.
		/// </value>
		public int Secondary
		{
			get
			{
				if ( _parts.Length > 1 )
				{
					return _parts[ 1 ];
				}
				return 0;
			}
		}

		/// <summary>
		///     Gets the tertiary.
		/// </summary>
		/// <value>
		///     The tertiary.
		/// </value>
		public int Tertiary
		{
			get
			{
				if ( _parts.Length > 2 )
				{
					return _parts[ 2 ];
				}
				return 0;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StatusCode" /> class.
		/// </summary>
		public StatusCode( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StatusCode" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public StatusCode( string value )
			: this( )
		{
			var serializer = new StatusCodeSerializer( );
			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );
			var statusCode = obj as IStatusCode;
			if ( statusCode != null )
			{
				IStatusCode sc = statusCode;
				Parts = new int[sc.Parts.Length];
				sc.Parts.CopyTo( Parts, 0 );
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
			var serializer = new StatusCodeSerializer( );
			return serializer.SerializeToString( this );
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
			var sc = obj as IStatusCode;
			if ( sc != null )
			{
				if ( _parts != null &&
				     sc.Parts != null &&
				     _parts.Length == sc.Parts.Length )
				{
					return !_parts.Where( ( t, i ) => !Equals( t, sc.Parts[ i ] ) ).Any( );
				}
				return false;
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
			int hash = 13;

// ReSharper disable NonReadonlyFieldInGetHashCode
			int[] mParts = _parts;
// ReSharper restore NonReadonlyFieldInGetHashCode

			if ( mParts != null )
			{
				for ( int i = 0; i < mParts.Length; i++ )
				{
					hash = ( hash * 7 ) + mParts[ i ].GetHashCode( );
				}
			}

			return hash;
		}
	}
}