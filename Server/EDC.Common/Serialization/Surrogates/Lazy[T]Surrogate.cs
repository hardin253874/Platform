// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using ProtoBuf;

namespace EDC.Serialization.Surrogates
{
	/// <summary>
	///     Lazy Surrogate
	/// </summary>
	/// <typeparam name="T">Lazy Type</typeparam>
	[ProtoContract]
	public class LazySurrogate<T>
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="LazySurrogate{T}" /> class from being created.
		/// </summary>
		private LazySurrogate( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="LazySurrogate{T}" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public LazySurrogate( T value )
			: this( )
		{
			Value = value;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		[ProtoMember( 1 )]
		public T Value
		{
			get;
			set;
		}

		/// <summary>
		///     Performs an implicit conversion from <see cref="Lazy{T}" /> to <see cref="LazySurrogate{T}" />.
		/// </summary>
		/// <param name="lazy">The lazy.</param>
		/// <returns>
		///     The result of the conversion.
		/// </returns>
		public static implicit operator LazySurrogate<T>( Lazy<T> lazy )
		{
			return lazy != null ? new LazySurrogate<T>( lazy.Value ) : null;
		}

		/// <summary>
		///     Performs an implicit conversion from <see cref="LazySurrogate{T}" /> to <see cref="Lazy{T}" />.
		/// </summary>
		/// <param name="surrogate">The surrogate.</param>
		/// <returns>
		///     The result of the conversion.
		/// </returns>
		public static implicit operator Lazy<T>( LazySurrogate<T> surrogate )
		{
			return surrogate != null ? new Lazy<T>( ( ) => surrogate.Value ) : null;
		}
	}
}