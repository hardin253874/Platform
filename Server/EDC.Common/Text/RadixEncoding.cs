// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Numerics;
using System.Text;

namespace EDC.Text
{
	/// <summary>
	///     Radix encoding base class.
	/// </summary>
	public class RadixEncoding
	{
		/// <summary>
		///     Base 36 encoding.
		/// </summary>
		private static volatile RadixEncoding _base36Encoding;

		/// <summary>
		///     Encoding alphabet.
		/// </summary>
		private readonly string _alphabet;

		/// <summary>
		///     Radix value.
		/// </summary>
		private readonly int _radix;

		/// <summary>
		///     Initializes a new instance of the <see cref="RadixEncoding" /> class.
		/// </summary>
		/// <param name="alphabet">The alphabet.</param>
		/// <param name="radix">The radix.</param>
		public RadixEncoding( string alphabet, int radix )
		{
			_alphabet = alphabet;
			_radix = radix;
		}

		/// <summary>
		///     Gets the alphabet.
		/// </summary>
		/// <value>
		///     The alphabet.
		/// </value>
		public string Alphabet
		{
			get
			{
				return _alphabet;
			}
		}

		/// <summary>
		///     Gets the base36.
		/// </summary>
		/// <value>
		///     The base36.
		/// </value>
		public static RadixEncoding Base36
		{
			get
			{
				return _base36Encoding ?? ( _base36Encoding = new Base36Encoding( ) );
			}
		}

		/// <summary>
		///     Gets the radix.
		/// </summary>
		/// <value>
		///     The radix.
		/// </value>
		public int Radix
		{
			get
			{
				return _radix;
			}
		}

		/// <summary>
		///     Gets the bytes.
		/// </summary>
		/// <param name="inputString">The input string.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Input string is not in correct radix.</exception>
		public byte[] GetBytes( string inputString )
		{
			BigInteger value = BigInteger.Zero;

			foreach ( char c in inputString )
			{
				int index = Alphabet.IndexOf( c );

				if ( index < 0 )
				{
					throw new ArgumentException( "Input string is not in correct radix." );
				}

				value = ( value * Radix ) + index;
			}

			byte[] byteArray = value.ToByteArray( );

			if ( !BitConverter.IsLittleEndian )
			{
				Array.Reverse( byteArray );
			}

			return byteArray;
		}

		/// <summary>
		///     Gets the string.
		/// </summary>
		/// <param name="inputArray">The input array.</param>
		/// <returns></returns>
		public string GetString( byte[] inputArray )
		{
			if ( !BitConverter.IsLittleEndian )
			{
				Array.Reverse( inputArray );
			}

			var dividend = new BigInteger( inputArray );

			var builder = new StringBuilder( );

			while ( dividend != 0 )
			{
				BigInteger remainder;

				dividend = BigInteger.DivRem( dividend, Radix, out remainder );

				builder.Insert( 0, Alphabet[ Math.Abs( ( ( int ) remainder ) ) ] );
			}

			return builder.ToString( );
		}
	}
}