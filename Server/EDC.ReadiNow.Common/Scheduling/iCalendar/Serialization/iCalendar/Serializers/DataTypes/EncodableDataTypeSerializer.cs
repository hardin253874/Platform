// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     EncodableDataTypeSerializer class.
	/// </summary>
	public abstract class EncodableDataTypeSerializer : DataTypeSerializer
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EncodableDataTypeSerializer" /> class.
		/// </summary>
		protected EncodableDataTypeSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EncodableDataTypeSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		protected EncodableDataTypeSerializer( ISerializationContext ctx )
			: base( ctx )
		{
		}

		/// <summary>
		///     Decodes the specified date time.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected string Decode( IEncodableDataType dt, string value )
		{
			byte[] data = DecodeData( dt, value );
			if ( data != null )
			{
				// Default to the current encoding
				var encodingStack = GetService<IEncodingStack>( );
				return encodingStack.Current.GetString( data );
			}
			return null;
		}

		/// <summary>
		///     Decodes the data.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected byte[] DecodeData( IEncodableDataType dt, string value )
		{
			if ( value != null )
			{
				if ( dt == null || dt.Encoding == null )
				{
					// Default to the current encoding
					var encodingStack = GetService<IEncodingStack>( );
					return encodingStack.Current.GetBytes( value );
				}

				var encodingProvider = GetService<IEncodingProvider>( );
				if ( encodingProvider != null )
				{
					return encodingProvider.DecodeData( dt.Encoding, value );
				}
			}
			return null;
		}

		/// <summary>
		///     Encodes the specified date time.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected string Encode( IEncodableDataType dt, string value )
		{
			if ( value != null )
			{
				if ( dt == null || dt.Encoding == null )
				{
					return value;
				}

				// Return the value in the current encoding
				var encodingStack = GetService<IEncodingStack>( );
				return Encode( dt, encodingStack.Current.GetBytes( value ) );
			}
			return null;
		}

		/// <summary>
		///     Encodes the specified date time.
		/// </summary>
		/// <param name="dt">The date time.</param>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		protected string Encode( IEncodableDataType dt, byte[] data )
		{
			if ( data != null )
			{
				if ( dt == null || dt.Encoding == null )
				{
					// Default to the current encoding
					var encodingStack = GetService<IEncodingStack>( );
					return encodingStack.Current.GetString( data );
				}

				var encodingProvider = GetService<IEncodingProvider>( );
				if ( encodingProvider != null )
				{
					return encodingProvider.Encode( dt.Encoding, data );
				}
			}
			return null;
		}
	}
}