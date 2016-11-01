// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Threading;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     Retry handler.
	/// </summary>
	public static class RetryHandler
	{
		/// <summary>
		///     Retries the specified action.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action">The action.</param>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="delay">The delay.</param>
		/// <param name="exceptionHandler">The exception handler.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">
		///     Invalid retry count. Must be between 0 and 100.
		///     or
		///     Invalid delay. Must be between 0 and 1000.
		/// </exception>
		public static T Retry<T>( Func<T> action, int retryCount = 5, int delay = 100, Action<Exception> exceptionHandler = null )
		{
			if ( retryCount < 0 || retryCount > 100 )
			{
				throw new InvalidOperationException( "Invalid retry count. Must be between 0 and 100." );
			}

			if ( delay < 0 || delay > 1000 )
			{
				throw new InvalidOperationException( "Invalid delay. Must be between 0 and 1000." );
			}

			int count = 0;

			while ( count <= retryCount )
			{
				try
				{
					return action( );
				}
				catch ( Exception exception )
				{
					if ( count < retryCount )
					{
						count++;
						Thread.Sleep( delay );
					}
					else
					{
						if ( exceptionHandler != null )
						{
							exceptionHandler( exception );
						}

						throw;
					}
				}
			}

			return default( T );
		}

		/// <summary>
		///     Retries the specified action.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="delay">The delay.</param>
		/// <param name="exceptionHandler">The exception handler.</param>
		/// <exception cref="System.InvalidOperationException">
		///     Invalid retry count. Must be between 0 and 100.
		///     or
		///     Invalid delay. Must be between 0 and 1000.
		/// </exception>
		public static void Retry( Action action, int retryCount = 5, int delay = 100, Action<Exception> exceptionHandler = null )
		{
			if ( retryCount < 0 || retryCount > 100 )
			{
				throw new InvalidOperationException( "Invalid retry count. Must be between 0 and 100." );
			}

			if ( delay < 0 || delay > 1000 )
			{
				throw new InvalidOperationException( "Invalid delay. Must be between 0 and 1000." );
			}

			int count = 0;

			while ( count <= retryCount )
			{
				try
				{
					action( );
					break;
				}
				catch ( Exception exception )
				{
					if ( count < retryCount )
					{
						count++;
						Thread.Sleep( delay );
					}
					else
					{
						if ( exceptionHandler != null )
						{
							exceptionHandler( exception );
						}

						throw;
					}
				}
			}
		}
	}
}