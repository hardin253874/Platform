// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Diagnostics;
using Jil;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Jil formatter.
	/// </summary>
	/// <remarks>
	///     Replaces the default JSON serializer.
	/// </remarks>
	public class JilFormatter : MediaTypeFormatter
	{
		/// <summary>
		///     The application JSON media type.
		/// </summary>
		public static readonly MediaTypeHeaderValue ApplicationJsonMediaType = new MediaTypeHeaderValue( "application/json" );

		/// <summary>
		///     Cached done task.
		/// </summary>
		private static readonly Task<bool> Done = Task.FromResult( true );

		/// <summary>
		///     The text JSON media type.
		/// </summary>
		public static readonly MediaTypeHeaderValue TextJsonMediaType = new MediaTypeHeaderValue( "text/json" );

		/// <summary>
		///     The jil options.
		/// </summary>
		private readonly Options _options;

		/// <summary>
		///     Initializes a new instance of the <see cref="JilFormatter" /> class.
		/// </summary>
		public JilFormatter( )
			: this( GetDefaultOptions( ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="JilFormatter" /> class.
		/// </summary>
		/// <param name="options">The options.</param>
		public JilFormatter( Options options )
		{
			_options = options;

			SupportedMediaTypes.Add( ApplicationJsonMediaType );
			SupportedMediaTypes.Add( TextJsonMediaType );

			SupportedEncodings.Add( new UTF8Encoding( false, true ) );
			SupportedEncodings.Add( new UnicodeEncoding( false, true, true ) );
		}

		/// <summary>
		///     Queries whether this <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> can deserialize an object of
		///     the
		///     specified type.
		/// </summary>
		/// <param name="type">The type to deserialize.</param>
		/// <returns>
		///     true if the <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> can deserialize the type; otherwise,
		///     false.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">type</exception>
		public override bool CanReadType( Type type )
		{
			if ( type == null )
			{
				throw new ArgumentNullException( "type" );
			}
			return true;
		}

		/// <summary>
		///     Queries whether this <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> can serialize an object of the
		///     specified type.
		/// </summary>
		/// <param name="type">The type to serialize.</param>
		/// <returns>
		///     true if the <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> can serialize the type; otherwise,
		///     false.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">type</exception>
		public override bool CanWriteType( Type type )
		{
			if ( type == null )
			{
				throw new ArgumentNullException( "type" );
			}
			return true;
		}

		/// <summary>
		///     Gets the default options.
		/// </summary>
		/// <returns></returns>
		public static Options GetDefaultOptions( )
		{
			return new Options( dateFormat: DateTimeFormat.ISO8601, excludeNulls: false, includeInherited: true );
		}

		/// <summary>
		///     Asynchronously deserializes an object of the specified type.
		/// </summary>
		/// <param name="type">The type of the object to deserialize.</param>
		/// <param name="readStream">The <see cref="T:System.IO.Stream" /> to read.</param>
		/// <param name="content">The <see cref="T:System.Net.Http.HttpContent" />, if available. It may be null.</param>
		/// <param name="formatterLogger">The <see cref="T:System.Net.Http.Formatting.IFormatterLogger" /> to log events to.</param>
		/// <returns>
		///     A <see cref="T:System.Threading.Tasks.Task" /> whose result will be an object of the given type.
		/// </returns>
		public override Task<object> ReadFromStreamAsync( Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger )
		{
			try
			{
				using ( var reader = new StreamReader( readStream ) )
				{
					var deserialize = TypedDeserializers.GetTyped( type );
					var result = deserialize( reader, _options );
					return Task.FromResult( result );
				}
			}
			catch ( DeserializationException exc )
			{
				EventLog.Application.WriteError( string.Format( "Failed to deserialize message.\nType: {0}\nException: {1}\nSnippetAfter: {2}", type.FullName, exc.Message, exc.SnippetAfterError ) );
				throw;
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( string.Format( "Failed to deserialize message.\nType: {0}\nException: {1}", type.FullName, exc.Message ) );
				throw;
			}
		}

		/// <summary>
		///     Asynchronously writes an object of the specified type.
		/// </summary>
		/// <param name="type">The type of the object to write.</param>
		/// <param name="value">The object value to write.  It may be null.</param>
		/// <param name="writeStream">The <see cref="T:System.IO.Stream" /> to which to write.</param>
		/// <param name="content">The <see cref="T:System.Net.Http.HttpContent" /> if available. It may be null.</param>
		/// <param name="transportContext">The <see cref="T:System.Net.TransportContext" /> if available. It may be null.</param>
		/// <returns>
		///     A <see cref="T:System.Threading.Tasks.Task" /> that will perform the write.
		/// </returns>
		public override Task WriteToStreamAsync( Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext )
		{
			using ( TextWriter streamWriter = new StreamWriter( writeStream ) )
			{
				JSON.Serialize( value, streamWriter, _options );

				streamWriter.Flush( );

				return Done;
			}
		}
	}
}