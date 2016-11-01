// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Sets a specific context for the duration of this objects lifetime on the current thread.
	///     Once disposed, the original calling context is restored.
	/// </summary>
	public class ContextBlock : IDisposable
	{
		/// <summary>
		///     The original context data.
		/// </summary>
		private RequestContext _originalContextData;

		/// <summary>
		///     Indicates whether this object has been disposed or not.
		/// </summary>
		private bool disposed;

		/// <summary>
		///     Default constructor for the ContextBlock object.
		/// </summary>
		public ContextBlock( Action setContextAction )
		{
			if ( RequestContext.IsSet )
			{
				/////
				// Cache the original context.
				/////
				_originalContextData = RequestContext.GetContext( );
			}

			/////
			// Set the specific context.
			/////
			setContextAction( );
		}

		/// <summary>
		///     Dispose this object instance.
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
		///     Virtual Dispose method.
		/// </summary>
		/// <param name="disposing">
		///     Whether this was called from Dispose of the Finalizer.
		/// </param>
		protected virtual void Dispose( bool disposing )
		{
			if ( !disposed )
			{
				/////
				// If called by the finalizer, dont bother restoring the context as you
				// are currently running on a GC Finalizer thread.
				/////
				if ( disposing )
				{
					/////
					// Restore the original request context
					/////
					if ( ( _originalContextData != null ) && ( _originalContextData.IsValid ) )
					{
						RequestContext.SetContext( _originalContextData );
					}
					else
					{
						RequestContext.FreeContext( );
					}
				}

				_originalContextData = null;

				/////
				// Dispose complete.
				/////
				disposed = true;
			}
		}

		/// <summary>
		///     Object destructor. This will only get called if Dispose has not been called.
		/// </summary>
		~ContextBlock( )
		{
			Dispose( false );
		}
	}
}