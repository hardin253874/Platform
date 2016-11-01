// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Remoting.Messaging;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	public class CallData<T> : IDisposable
	{
		/// <summary>
		///     Indicates whether this object has been disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="CallData{T}" /> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public CallData( string key, T value )
		{
			Key = key;
			CallContext.LogicalSetData( key, value );
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static CallData<T> SetValue( string key, T value )
		{
			return new CallData<T>( key, value );
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static T GetValue( string key )
		{
			object value = CallContext.LogicalGetData( key );

			if ( value != null )
			{
				return ( T ) value;
			}

			return default(T);
		}

		/// <summary>
		///     Gets or sets the key.
		/// </summary>
		/// <value>
		///     The key.
		/// </value>
		private string Key
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public T Value
		{
			get
			{
				return GetValue( Key );
			}
		}

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
		///     Gets the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public T Get( string key )
		{
			return ( T ) CallContext.LogicalGetData( key );
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
				// If called by the finalizer, don't bother restoring the context as you
				// are currently running on a GC Finalizer thread.
				/////
				if ( disposing )
				{
					CallContext.FreeNamedDataSlot( Key );
				}

				/////
				// Dispose complete.
				/////
				_disposed = true;
			}
		}
	}
}