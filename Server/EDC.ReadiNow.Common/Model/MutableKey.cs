// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Mutable Id Key
	/// </summary>
	[Serializable]
	public class MutableIdKey : MutableKey<long>, IMutableIdKey
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MutableIdKey" /> class.
		/// </summary>
		/// <param name="id">The id.</param>
		public MutableIdKey( long id )
			: base( id )
		{
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
			if ( obj == null )
			{
				return false;
			}

			var key = obj as MutableIdKey;

			if ( key == null )
			{
				return false;
			}

			return ImmutableKey.Equals( key.ImmutableKey );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			return ImmutableKey.GetHashCode( );
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( MutableIdKey a, MutableIdKey b )
		{
			return !( a == b );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( MutableIdKey a, MutableIdKey b )
		{
			if ( ReferenceEquals( a, b ) )
			{
				return true;
			}

			if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
			{
				return false;
			}

			return a.Equals( b );
		}
	}

	/// <summary>
	///     Mutable key for use in dictionaries etc.
	/// </summary>
	/// <typeparam name="T">Mutable key value.</typeparam>
	[Serializable]
	public class MutableKey<T> : IMutableKey<T>
	{
		/// <summary>
		///     Immutable key value.
		/// </summary>
		private Guid? _immutableKey;

		/// <summary>
		///     Initializes a new instance of the <see cref="MutableKey&lt;T&gt;" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public MutableKey( T value )
		{
			Key = value;
		}

		/// <summary>
		///     Gets or sets the mutable key.
		/// </summary>
		/// <value>
		///     The mutable key.
		/// </value>
		public T Key
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the immutable key.
		/// </summary>
		protected Guid ImmutableKey
		{
			get
			{
				if ( _immutableKey == null )
				{
					_immutableKey = Guid.NewGuid( );
				}

				return _immutableKey.Value;
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
			if ( obj == null )
			{
				return false;
			}

			var key = obj as MutableKey<T>;

			if ( key == null )
			{
				return false;
			}

			return ImmutableKey.Equals( key.ImmutableKey );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			return ImmutableKey.GetHashCode( );
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( MutableKey<T> a, MutableKey<T> b )
		{
			return !( a == b );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( MutableKey<T> a, MutableKey<T> b )
		{
			if ( ReferenceEquals( a, b ) )
			{
				return true;
			}

			if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
			{
				return false;
			}

			return a.ImmutableKey == b.ImmutableKey;
		}
	}
}