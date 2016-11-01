// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace EDC.Database
{
	/// <summary>
	///     The database context information class.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class DatabaseContextInfo : IDisposable
	{
		/// <summary>
		///     The call context key
		/// </summary>
		private static readonly string CallContextKey = "DatabaseContextInfo";

		/// <summary>
		///     The ellipsis
		/// </summary>
		private static readonly string Ellipsis = "...";

		/// <summary>
		///     Whether this instance has been disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     The original context information
		/// </summary>
		private DatabaseContextInfo _originalContextInfo;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseContextInfo" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public DatabaseContextInfo( string message )
		{
			DatabaseContextInfo existingContextInfo = CallContext.LogicalGetData( CallContextKey ) as DatabaseContextInfo;

			if ( existingContextInfo != null )
			{
				_originalContextInfo = existingContextInfo;
				TransactionId = _originalContextInfo.TransactionId;
			}
			else
			{
				TransactionId = -1;
			}

			/////
			// Compact messages
			/////
			if ( existingContextInfo == null || !string.Equals( existingContextInfo.Message, message, StringComparison.OrdinalIgnoreCase ) )
			{
				Message = message;
			}

			CallContext.LogicalSetData( CallContextKey, this );
		}

		/// <summary>
		///     Gets the message.
		/// </summary>
		/// <value>
		///     The message.
		/// </value>
		public string Message
		{
			get;
		}

		/// <summary>
		///     Gets or sets the transaction identifier.
		/// </summary>
		/// <value>
		///     The transaction identifier.
		/// </value>
		public long TransactionId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the parent.
		/// </summary>
		/// <value>
		///     The parent.
		/// </value>
		private DatabaseContextInfo Parent => _originalContextInfo;

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );

			/////
			// No need to call the finalizer.
			/////
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Gets the context information.
		/// </summary>
		/// <returns></returns>
		public static DatabaseContextInfo GetContextInfo( )
		{
			DatabaseContextInfo contextInfo = CallContext.LogicalGetData( CallContextKey ) as DatabaseContextInfo;

			return contextInfo;
		}

		/// <summary>
		/// Gets the message chain.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="separator">The separator.</param>
		/// <param name="maxLength">The maximum length.</param>
		/// <param name="caller">The caller.</param>
		/// <returns></returns>
		/// <value>
		/// The message chain.
		/// </value>
		public static string GetMessageChain( long userId, string separator = "->", int maxLength = 128, [CallerMemberName] string caller = null )
		{
			DatabaseContextInfo ctx = GetContextInfo( );

			if ( ctx != null && ctx.TransactionId >= 0 )
			{
				return $"u:{userId},{ctx.TransactionId}";
			}

			IList<string> messageChain = ctx == null ? BuildDebugMessageChain( ) : BuildContextInfoMessageChain( ctx );

			string message = TruncateMessageChain( userId, messageChain, separator, maxLength );

			message = message ?? caller ?? "No context available";

			return $"u:{userId},{message}";
		}

		/// <summary>
		///     Sets the context information.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		public static DatabaseContextInfo SetContextInfo( string message )
		{
			return new DatabaseContextInfo( message );
		}

		/// <summary>
		/// Truncates the message chain.
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="messageChain">The message chain.</param>
		/// <param name="separator">The separator.</param>
		/// <param name="maxLength">The maximum length.</param>
		/// <returns></returns>
		public static string TruncateMessageChain( long userId, IList<string> messageChain, string separator = "->", int maxLength = 128 )
		{
			if ( maxLength < 20 )
			{
				/////
				// Support strings of the form 'a...->...->b...' at a minimum.
				/////
				throw new ArgumentException( @"Length must be at least 16 characters long.", nameof( maxLength ) );
			}

			if ( messageChain == null || messageChain.Count == 0 || string.IsNullOrEmpty( separator ) || maxLength <= 0 )
			{
				return null;
			}

			string message;

			maxLength = maxLength - ( userId.ToString( ).Length + 3 );

			/////
			// Current length is the sum of all components plus the separators between them
			/////
			int length = messageChain.Sum( m => m.Length ) + ( messageChain.Count - 1 ) * separator.Length;

			int ellipsePosition = -1;

			while ( length > maxLength && messageChain.Count > 2 )
			{
				ellipsePosition = messageChain.Count / 2;
				messageChain.RemoveAt( ellipsePosition );

				length = messageChain.Sum( m => m.Length ) + messageChain.Count * separator.Length + Ellipsis.Length;
			}

			if ( length <= maxLength )
			{
				if ( ellipsePosition >= 0 )
				{
					messageChain.Insert( ellipsePosition, Ellipsis );
				}

				message = string.Join( separator, messageChain );
			}
			else
			{
				if ( messageChain.Count == 1 )
				{
					/////
					// Simply append the ellipse to the end
					/////
					message = messageChain [ 0 ].Substring( 0, maxLength - Ellipsis.Length ) + Ellipsis;
				}
				else
				{
					/////
					// Trim one or both components to get under the limit
					/////
					string first = messageChain [ 0 ];
					string second = messageChain [ 1 ];

					int maxComponentLength = ( int ) Math.Floor( ( double ) maxLength / 2 ) - separator.Length;

					if ( ellipsePosition >= 0 )
					{
						maxComponentLength -= ( int ) Math.Ceiling( ( double ) Ellipsis.Length / 2 );
					}

					if ( first.Length >= maxComponentLength && second.Length >= maxComponentLength )
					{
						message = $"{first.Substring( 0, maxComponentLength - Ellipsis.Length )}{Ellipsis}{separator}{( ellipsePosition >= 0 ? $"{Ellipsis}{separator}" : string.Empty )}{second.Substring( 0, maxComponentLength - Ellipsis.Length )}{Ellipsis}";
					}
					else if ( first.Length > maxComponentLength )
					{
						int availableLength = maxLength - ( separator.Length + Ellipsis.Length ) - second.Length;

						if ( ellipsePosition >= 0 )
						{
							availableLength -= separator.Length + Ellipsis.Length;
						}

						message = $"{first.Substring( 0, availableLength )}{Ellipsis}{separator}{( ellipsePosition >= 0 ? $"{Ellipsis}{separator}" : string.Empty )}{second}";
					}
					else if ( second.Length > maxComponentLength )
					{
						int availableLength = maxLength - ( separator.Length + Ellipsis.Length ) - first.Length;

						if ( ellipsePosition >= 0 )
						{
							availableLength -= separator.Length + Ellipsis.Length;
						}

						message = $"{first}{separator}{( ellipsePosition >= 0 ? $"{Ellipsis}{separator}" : string.Empty )}{second.Substring( 0, availableLength )}{Ellipsis}";
					}
					else
					{
						message = null;
					}
				}
			}

			return message;
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
			if ( !_disposed )
			{
				/////
				// If called by the finalizer, dont bother restoring the context as you
				// are currently running on a GC Finalizer thread.
				/////
				if ( disposing )
				{
					/////
					// Restore the original context
					/////
					if ( _originalContextInfo != null )
					{
						_originalContextInfo.TransactionId = TransactionId;

						CallContext.LogicalSetData( CallContextKey, _originalContextInfo );
					}
					else
					{
						CallContext.FreeNamedDataSlot( CallContextKey );
					}
				}

				_originalContextInfo = null;

				/////
				// Dispose complete.
				/////
				_disposed = true;
			}
		}

		/// <summary>
		///     Builds the context information message chain.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		private static IList<string> BuildContextInfoMessageChain( DatabaseContextInfo context )
		{
			List<string> messageChain = new List<string>( );

			DatabaseContextInfo currentContext = context;

			while ( currentContext != null )
			{
				if ( !string.IsNullOrEmpty( currentContext.Message ) )
				{
					messageChain.Insert( 0, currentContext.Message );
				}

				currentContext = currentContext.Parent;
			}

			return messageChain;
		}

		/// <summary>
		///     Builds the debug message chain.
		/// </summary>
		/// <returns></returns>
		private static IList<string> BuildDebugMessageChain( )
		{
			StackTrace stackTrace = new StackTrace( 2, false );

			var frames = stackTrace.GetFrames( );

			List<string> messageChain = new List<string>( );

			if ( frames != null )
			{
				foreach ( StackFrame frame in frames )
				{
					var method = frame.GetMethod( );

					if ( method != null && method.DeclaringType != null )
					{
						var declaringTypeFullName = method.DeclaringType.FullName;

						if ( declaringTypeFullName.StartsWith( "EDC", StringComparison.OrdinalIgnoreCase ) || declaringTypeFullName.StartsWith( "ReadiNow", StringComparison.OrdinalIgnoreCase ) )
						{
							string methodName = frame.GetMethod( ).Name;
							string typeName = method.DeclaringType.Name;

							if ( !string.IsNullOrEmpty( methodName ) )
							{
								messageChain.Insert( 0, $"{typeName}.{methodName}" );
							}
						}
					}
				}
			}

			return messageChain;
		}
	}
}