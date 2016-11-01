// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Remoting.Messaging;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Redis suppression.
	/// </summary>
	public class Suppression : ISuppression
	{
		/// <summary>
		///     The redis suppression call context key.
		/// </summary>
		private const string CallContextKey = "RedisSuppression";

		/// <summary>
		///     Whether this instance is disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="Suppression" /> class.
		/// </summary>
		public Suppression( )
		{
			var state = CallContext.LogicalGetData( CallContextKey ) as SuppressionState;

			if ( state == null )
			{
				state = new SuppressionState( );
				CallContext.LogicalSetData( CallContextKey, state );
			}

			state.Increment( );
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
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///     unmanaged resources.
		/// </param>
		/// <exception cref="System.InvalidOperationException">CallContext corrupt. Expected valid SuppressionState.</exception>
		protected virtual void Dispose( bool disposing )
		{
			if ( !_disposed )
			{
				if ( disposing )
				{
					var state = CallContext.LogicalGetData( CallContextKey ) as SuppressionState;

					if ( state == null )
					{
						throw new InvalidOperationException( "CallContext corrupt. Expected valid SuppressionState." );
					}

					state.Decrement( );

					if ( state.RefCount == 0 )
					{
						CallContext.FreeNamedDataSlot( CallContextKey );
					}
				}
			}

			_disposed = true;
		}

		/// <summary>
		///     Determines whether this instance is active.
		/// </summary>
		/// <returns></returns>
		public static bool IsActive( )
		{
			var state = CallContext.LogicalGetData( CallContextKey ) as SuppressionState;

			if ( state == null )
			{
				return false;
			}

			return state.RefCount > 0;
		}

		/// <summary>
		///     Suppression State object.
		/// </summary>
		private class SuppressionState
		{
			/// <summary>
			///     Gets or sets the reference count.
			/// </summary>
			/// <value>
			///     The reference count.
			/// </value>
			public int RefCount
			{
				get;
				private set;
			}

			/// <summary>
			///     Decrements this instance.
			/// </summary>
			public void Decrement( )
			{
				if ( RefCount > 0 )
				{
					RefCount--;
				}
			}

			/// <summary>
			///     Increments this instance.
			/// </summary>
			public void Increment( )
			{
				RefCount++;
			}
		}
	}
}