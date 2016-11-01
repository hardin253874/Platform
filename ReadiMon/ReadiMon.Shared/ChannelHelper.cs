// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.IO.Compression;
using ProtoBuf;

namespace ReadiMon.Shared
{
	/// <summary>
	///     Channel Helper.
	/// </summary>
	public static class ChannelHelper
	{
		/// <summary>
		///     Compresses the specified bytes.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">bytes</exception>
		public static byte[ ] Compress( byte[ ] bytes )
		{
			if ( bytes == null )
			{
				throw new ArgumentNullException( "bytes" );
			}

			using ( var memoryStream = new MemoryStream( ) )
			{
				using ( var compressionStream = new GZipStream( memoryStream, CompressionMode.Compress ) )
				{
					compressionStream.Write( bytes, 0, bytes.Length );
				}

				return memoryStream.ToArray( );
			}
		}

		/// <summary>
		///     Decompresses the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		public static byte[ ] Decompress( byte[ ] message )
		{
			using ( var compressedStream = new MemoryStream( message ) )
			using ( var decompressionStream = new GZipStream( compressedStream, CompressionMode.Decompress ) )
			using ( var memoryStream = new MemoryStream( ) )
			{
				decompressionStream.CopyTo( memoryStream );

				return memoryStream.ToArray( );
			}
		}

		/// <summary>
		///     Deserializes the specified bytes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">bytes</exception>
		public static T Deserialize<T>( byte[ ] bytes )
		{
			if ( bytes == null )
			{
				throw new ArgumentNullException( "bytes" );
			}

			using ( var stream = new MemoryStream( bytes ) )
			{
				var deserializedObject = Serializer.Deserialize<T>( stream );

				return deserializedObject;
			}
		}

		/// <summary>
		///     Serializes the specified message.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">message</exception>
		public static byte[ ] Serialize<T>( T message )
		{
			if ( Equals( message, default( T ) ) )
			{
				throw new ArgumentNullException( "message" );
			}

			using ( var stream = new MemoryStream( ) )
			{
				Serializer.Serialize( stream, message );

				return stream.ToArray( );
			}
		}
	}
}