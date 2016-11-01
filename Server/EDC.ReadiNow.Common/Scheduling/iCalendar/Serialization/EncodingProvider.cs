// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     EncodingProvider class.
	/// </summary>
	public class EncodingProvider : IEncodingProvider
	{
		/// <summary>
		///     Decoder delegate.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public delegate byte[] DecoderDelegate( string value );

		/// <summary>
		///     Encoder delegate.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public delegate string EncoderDelegate( byte[] data );

		/// <summary>
		///     Serialization context.
		/// </summary>
		private readonly ISerializationContext _serializationContext;

		/// <summary>
		///     Initializes a new instance of the <see cref="EncodingProvider" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public EncodingProvider( ISerializationContext ctx )
		{
			_serializationContext = ctx;
		}

		/// <summary>
		///     Decode7s the bit.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected byte[] Decode7Bit( string value )
		{
			try
			{
				var utf7 = new UTF7Encoding( );
				return utf7.GetBytes( value );
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		///     Decode8s the bit.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected byte[] Decode8Bit( string value )
		{
			try
			{
				var utf8 = new UTF8Encoding( );
				return utf8.GetBytes( value );
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		///     Decodes the base64.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected byte[] DecodeBase64( string value )
		{
			try
			{
				return Convert.FromBase64String( value );
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		///     Encode7s the bit.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		protected string Encode7Bit( byte[] data )
		{
			try
			{
				var utf7 = new UTF7Encoding( );
				return utf7.GetString( data );
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		///     Encode8s the bit.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		protected string Encode8Bit( byte[] data )
		{
			try
			{
				var utf8 = new UTF8Encoding( );
				return utf8.GetString( data );
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		///     Encodes the base64.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		protected string EncodeBase64( byte[] data )
		{
			try
			{
				return Convert.ToBase64String( data );
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		///     Gets the decoder for.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		protected virtual DecoderDelegate GetDecoderFor( string encoding )
		{
			if ( encoding != null )
			{
				switch ( encoding.ToUpper( ) )
				{
					case "7BIT":
						return Decode7Bit;
					case "8BIT":
						return Decode8Bit;
					case "BASE64":
						return DecodeBase64;
					default:
						return null;
				}
			}
			return null;
		}

		/// <summary>
		///     Gets the encoder for.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		protected virtual EncoderDelegate GetEncoderFor( string encoding )
		{
			if ( encoding != null )
			{
				switch ( encoding.ToUpper( ) )
				{
					case "7BIT":
						return Encode7Bit;
					case "8BIT":
						return Encode8Bit;
					case "BASE64":
						return EncodeBase64;
					default:
						return null;
				}
			}
			return null;
		}

		#region IEncodingProvider Members

		/// <summary>
		///     Encodes the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public string Encode( string encoding, byte[] data )
		{
			if ( encoding != null &&
			     data != null )
			{
				EncoderDelegate encoder = GetEncoderFor( encoding );
				if ( encoder != null )
				{
					return encoder( data );
				}
			}
			return null;
		}

		/// <summary>
		///     Decodes the string.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public string DecodeString( string encoding, string value )
		{
			if ( encoding != null &&
			     value != null )
			{
				byte[] data = DecodeData( encoding, value );
				if ( data != null )
				{
					// Decode the string into the current encoding
					var encodingStack = _serializationContext.GetService( typeof ( IEncodingStack ) ) as IEncodingStack;

					if ( encodingStack != null )
					{
						return encodingStack.Current.GetString( data );
					}
				}
			}
			return null;
		}

		/// <summary>
		///     Decodes the data.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public byte[] DecodeData( string encoding, string value )
		{
			if ( encoding != null &&
			     value != null )
			{
				DecoderDelegate decoder = GetDecoderFor( encoding );
				if ( decoder != null )
				{
					return decoder( value );
				}
			}
			return null;
		}

		#endregion
	}
}