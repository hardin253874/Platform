// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using EDC.Security;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Impersonation Sql File Stream.
	/// </summary>
	public class ImpersonationSqlFileStream : IDisposable
	{
		/// <summary>
		///     The sql file stream.
		/// </summary>
		private readonly SqlFileStream _fileStream;

		/// <summary>
		///     The impersonation context.
		/// </summary>
		private readonly ImpersonationContext _impersonationContext;

		/// <summary>
		///     Whether this instance has been disposed of or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="ImpersonationSqlFileStream" /> class.
		/// </summary>
		public ImpersonationSqlFileStream( )
		{
			NetworkCredential databaseCredential;

			/////
			// Determine if an impersonation context is required.
			/////
			using ( var context = DatabaseContext.GetContext( ) )
			{
				databaseCredential = context.DatabaseInfo.Credentials;
			}

			if ( databaseCredential != null )
			{
				WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent( );

				bool impersonate = false;

				if ( windowsIdentity != null )
				{
					var principal = new WindowsPrincipal( windowsIdentity );

					string account = ( ( WindowsIdentity ) principal.Identity ).Name;

					if ( String.Compare( CredentialHelper.GetFullyQualifiedName( databaseCredential ), account, StringComparison.OrdinalIgnoreCase ) != 0 )
					{
						impersonate = true;
					}
				}

				if ( impersonate )
				{
					_impersonationContext = ImpersonationContext.GetContext( databaseCredential );

					if ( string.IsNullOrEmpty( databaseCredential.Domain ) )
					{
						windowsIdentity = WindowsIdentity.GetCurrent( );

						if ( windowsIdentity != null )
						{
							var principal = new WindowsPrincipal( windowsIdentity );

							string account = ( ( WindowsIdentity ) principal.Identity ).Name;

							var parts = account.Split( new[ ]
							{
								'\\'
							}, StringSplitOptions.RemoveEmptyEntries );

							if ( parts.Length == 2 )
							{
								databaseCredential.Domain = parts[ 0 ];
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ImpersonationSqlFileStream" /> class.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="transactionContext">The transaction context.</param>
		/// <param name="access">The access.</param>
		public ImpersonationSqlFileStream( string path, byte[ ] transactionContext, FileAccess access ) :
			this( path, transactionContext, access, FileOptions.None, 0L )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ImpersonationSqlFileStream" /> class.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="transactionContext">The transaction context.</param>
		/// <param name="access">The access.</param>
		/// <param name="options">The options.</param>
		/// <param name="allocationSize">Size of the allocation.</param>
		public ImpersonationSqlFileStream( string path, byte[ ] transactionContext, FileAccess access, FileOptions options, long allocationSize )
			: this( )
		{
			_fileStream = new SqlFileStream( path, transactionContext, access, options, allocationSize );
		}

		/// <summary>
		///     Gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <returns>
		///     true if the current stream supports reading; otherwise, false.
		/// </returns>
		public bool CanRead
		{
			get
			{
				return _fileStream.CanRead;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <returns>
		///     true if the current stream supports seeking; otherwise, false.
		/// </returns>
		public bool CanSeek
		{
			get
			{
				return _fileStream.CanSeek;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the current stream can time out.
		/// </summary>
		/// <returns>
		///     true if the current stream can time out; otherwise, false.
		/// </returns>
		public bool CanTimeout
		{
			get
			{
				return _fileStream.CanTimeout;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <returns>
		///     true if the current stream supports writing; otherwise, false.
		/// </returns>
		public bool CanWrite
		{
			get
			{
				return _fileStream.CanWrite;
			}
		}

		/// <summary>
		///     Gets a value indicating the length of the current stream in bytes.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Int64" /> indicating the length of the current stream in bytes.
		/// </returns>
		public long Length
		{
			get
			{
				return _fileStream.Length;
			}
		}

		/// <summary>
		///     Gets the logical path of the <see cref="T:EDC.ReadiNow.Database.ImpersonationSqlFileStream" /> passed to the
		///     constructor.
		/// </summary>
		/// <returns>
		///     A string value indicating the name of the <see cref="T:EDC.ReadiNow.Database.ImpersonationSqlFileStream" />.
		/// </returns>
		public string Name
		{
			get
			{
				return _fileStream.Name;
			}
		}

		/// <summary>
		///     Gets or sets the position within the current stream.
		/// </summary>
		/// <returns>
		///     The current position within the <see cref="T:EDC.ReadiNow.Database.ImpersonationSqlFileStream" />.
		/// </returns>
		public long Position
		{
			get
			{
				return _fileStream.Position;
			}
			set
			{
				_fileStream.Position = value;
			}
		}

		/// <summary>
		///     Gets or sets a value, in milliseconds, that determines how long the stream will attempt to read before timing out.
		/// </summary>
		/// <returns>
		///     A value, in milliseconds, that determines how long the stream will attempt to read before timing out.
		/// </returns>
		public int ReadTimeout
		{
			get
			{
				return _fileStream.ReadTimeout;
			}
			set
			{
				_fileStream.ReadTimeout = value;
			}
		}

		/// <summary>
		///     Gets the stream.
		/// </summary>
		/// <value>
		///     The stream.
		/// </value>
		public Stream Stream
		{
			get
			{
				return _fileStream;
			}
		}

		/// <summary>
		///     Gets or sets the transaction context for this <see cref="T:EDC.ReadiNow.Database.ImpersonationSqlFileStream" />
		///     object.
		/// </summary>
		/// <returns>
		/// </returns>
		public byte[ ] TransactionContext
		{
			get
			{
				return _fileStream.TransactionContext;
			}
		}

		/// <summary>
		///     Gets or sets a value, in milliseconds, that determines how long the stream will attempt to write before timing out.
		/// </summary>
		/// <returns>
		///     A value, in milliseconds, that determines how long the stream will attempt to write before timing out.
		/// </returns>
		public int WriteTimeout
		{
			get
			{
				return _fileStream.WriteTimeout;
			}
			set
			{
				_fileStream.WriteTimeout = value;
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Begins an asynchronous read operation.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.IAsyncResult" /> that represents the asynchronous read, which could still be pending.
		/// </returns>
		/// <param name="buffer">The buffer to read the data into. </param>
		/// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data read from the stream. </param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
		/// <param name="state">
		///     A user-provided object that distinguishes this particular asynchronous read request from other
		///     requests
		/// </param>
		/// <exception cref="T:System.NotSupportedException">Reading data is not supported on the stream.</exception>
		public IAsyncResult BeginRead( byte[ ] buffer, int offset, int count, AsyncCallback callback, object state )
		{
			return _fileStream.BeginRead( buffer, offset, count, callback, state );
		}

		/// <summary>
		///     Begins an asynchronous write operation.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.IAsyncResult" /> that represents the asynchronous write, which could still be pending.
		/// </returns>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The byte offset in <paramref name="buffer" /> from which to begin writing.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		/// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
		/// <param name="state">
		///     A user-provided object that distinguishes this particular asynchronous write request from other
		///     requests.
		/// </param>
		/// <exception cref="T:System.NotSupportedException">Writing data is not supported on the stream.</exception>
		public IAsyncResult BeginWrite( byte[ ] buffer, int offset, int count, AsyncCallback callback, object state )
		{
			return _fileStream.BeginWrite( buffer, offset, count, callback, state );
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///     unmanaged resources.
		/// </param>
		protected virtual void Dispose( bool disposing )
		{
			if ( _disposed )
			{
				return;
			}

			if ( disposing )
			{
				if ( _fileStream != null )
				{
					_fileStream.Dispose( );
				}

				if ( _impersonationContext != null )
				{
					_impersonationContext.Dispose( );
				}
			}

			_disposed = true;
		}

		/// <summary>
		///     Waits for the pending asynchronous read to complete.
		/// </summary>
		/// <returns>
		///     The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return
		///     zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.
		/// </returns>
		/// <param name="asyncResult">The reference to the pending asynchronous request to finish. </param>
		/// <exception cref="T:System.ArgumentException">
		///     The <see cref="T:System.IAsyncResult" /> object did not come from the
		///     corresponding BeginRead method.
		/// </exception>
		public int EndRead( IAsyncResult asyncResult )
		{
			return _fileStream.EndRead( asyncResult );
		}

		/// <summary>
		///     Ends an asynchronous write operation.
		/// </summary>
		/// <param name="asyncResult">A reference to the outstanding asynchronous I/O request. </param>
		/// <exception cref="T:System.ArgumentException">
		///     The <see cref="T:System.IAsyncResult" /> object did not come from the
		///     corresponding BeginWrite method.
		/// </exception>
		public void EndWrite( IAsyncResult asyncResult )
		{
			_fileStream.EndWrite( asyncResult );
		}

		/// <summary>
		///     clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		public void Flush( )
		{
			_fileStream.Flush( );
		}

		/// <summary>
		///     Reads a sequence of bytes from the current stream and advances the position within the stream by the number of
		///     bytes read.
		/// </summary>
		/// <returns>
		///     The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
		///     bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <param name="buffer">
		///     An array of bytes. When this method returns, the buffer contains the specified byte array with the
		///     values between offset and (offset + count - 1) replaced by the bytes read from the current source.
		/// </param>
		/// <param name="offset">
		///     The zero-based byte offset in buffer at which to begin storing the data read from the current
		///     stream.
		/// </param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <exception cref="T:System.NotSupportedException">The object does not support reading of data.</exception>
		public int Read( [In, Out] byte[ ] buffer, int offset, int count )
		{
			return _fileStream.Read( buffer, offset, count );
		}

		/// <summary>
		///     Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end
		///     of the stream.
		/// </summary>
		/// <returns>
		///     The unsigned byte cast to an <see cref="T:System.Int32" />, or -1 if at the end of the stream.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The object does not support reading of data.</exception>
		public int ReadByte( )
		{
			return _fileStream.ReadByte( );
		}

		/// <summary>
		///     Sets the position within the current stream.
		/// </summary>
		/// <returns>
		///     The new position within the current stream.
		/// </returns>
		/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter</param>
		/// <param name="origin">
		///     A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to
		///     obtain the new position
		/// </param>
		public long Seek( long offset, SeekOrigin origin )
		{
			return _fileStream.Seek( offset, origin );
		}

		/// <summary>
		///     Sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="T:System.NotSupportedException">The object does not support reading of data.</exception>
		public void SetLength( long value )
		{
			_fileStream.SetLength( value );
		}

		/// <summary>
		///     Writes a sequence of bytes to the current stream and advances the current position within this stream by the number
		///     of bytes written.
		/// </summary>
		/// <param name="buffer">
		///     An array of bytes. This method copies <paramref name="count" /> bytes from
		///     <paramref name="buffer" /> to the current stream.
		/// </param>
		/// <param name="offset">
		///     The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the
		///     current stream.
		/// </param>
		/// <param name="count">The number of bytes to be written to the current stream. </param>
		/// <exception cref="T:System.NotSupportedException">The object does not support writing of data.</exception>
		public void Write( byte[ ] buffer, int offset, int count )
		{
			_fileStream.Write( buffer, offset, count );
		}

		/// <summary>
		///     Writes a byte to the current position in the stream and advances the position within the stream by one byte.
		/// </summary>
		/// <param name="value">The byte to write to the stream. </param>
		/// <exception cref="T:System.NotSupportedException">The object does not support writing of data.</exception>
		public void WriteByte( byte value )
		{
			_fileStream.WriteByte( value );
		}

		/// <summary>
		///     Finalizes an instance of the <see cref="ImpersonationSqlFileStream" /> class.
		/// </summary>
		~ImpersonationSqlFileStream( )
		{
			Dispose( false );
		}
	}
}