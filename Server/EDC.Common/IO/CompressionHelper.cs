// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.IO.Compression;

namespace EDC.IO
{
	/// <summary>
	///     Compress/Decompress helper methods
	/// </summary>
	public static class CompressionHelper
	{
		/// <summary>
		///     Compresses the specified data.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">dataToCompress</exception>
		public static byte[ ] Compress( byte[ ] bytes )
		{
			if ( bytes == null )
			{
				throw new ArgumentNullException( nameof( bytes ) );
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
		///     Decompresses the specified data.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">bytes</exception>
		public static byte[ ] Decompress( byte[ ] bytes )
		{
			if ( bytes == null )
			{
				throw new ArgumentNullException( nameof( bytes ) );
			}

			using ( var compressedStream = new MemoryStream( bytes ) )
			using ( var decompressionStream = new GZipStream( compressedStream, CompressionMode.Decompress ) )
			using ( var memoryStream = new MemoryStream( ) )
			{
				decompressionStream.CopyTo( memoryStream );

				return memoryStream.ToArray( );
			}
		}
	}
}