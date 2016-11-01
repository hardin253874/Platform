// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Compressed content.
	/// </summary>
	public class CompressedContent : HttpContent
	{
		/// <summary>
		///     The encoding type
		/// </summary>
		private readonly string _encodingType;

		/// <summary>
		///     The original content
		/// </summary>
		private readonly HttpContent _originalContent;

		/// <summary>
		///     Initializes a new instance of the <see cref="CompressedContent" /> class.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="encodingType">Type of the encoding.</param>
		/// <exception cref="System.ArgumentNullException">
		///     content
		///     or
		///     encodingType
		/// </exception>
		/// <exception cref="System.InvalidOperationException"></exception>
		public CompressedContent( HttpContent content, string encodingType )
		{
			if ( content == null )
			{
				return;
			}

			if ( encodingType == null )
			{
				throw new ArgumentNullException( "encodingType" );
			}

			_originalContent = content;
			_encodingType = encodingType.ToLowerInvariant( );

			if ( _encodingType != "gzip" && _encodingType != "deflate" )
			{
				throw new InvalidOperationException( string.Format( "Encoding '{0}' is not supported. Only supports gzip or deflate encoding.", _encodingType ) );
			}

			// copy the headers from the original content
			foreach ( var header in _originalContent.Headers )
			{
				Headers.TryAddWithoutValidation( header.Key, header.Value );
			}

			Headers.ContentEncoding.Add( encodingType );
		}

		/// <summary>
		///     Determines whether the HTTP content has a valid length in bytes.
		/// </summary>
		/// <param name="length">The length in bytes of the HHTP content.</param>
		/// <returns>
		///     Returns <see cref="T:System.Boolean" />.true if <paramref name="length" /> is a valid length; otherwise, false.
		/// </returns>
		protected override bool TryComputeLength( out long length )
		{
			length = -1;

			return false;
		}

		/// <summary>
		///     Serialize the HTTP content to a stream as an asynchronous operation.
		/// </summary>
		/// <param name="stream">The target stream.</param>
		/// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
		/// <returns>
		///     Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.
		/// </returns>
		protected override Task SerializeToStreamAsync( Stream stream, TransportContext context )
		{
			Stream compressedStream = null;

			if ( _originalContent == null )
			{
				return Task.FromResult( ( object ) null );
			}

			if ( _encodingType == "gzip" )
			{
				compressedStream = new GZipStream( stream, CompressionMode.Compress, true );
			}
			else if ( _encodingType == "deflate" )
			{
				compressedStream = new DeflateStream( stream, CompressionMode.Compress, true );
			}

			return _originalContent.CopyToAsync( compressedStream ).ContinueWith( tsk =>
			{
				if ( compressedStream != null )
				{
					compressedStream.Dispose( );
				}
			} );
		}
	}
}