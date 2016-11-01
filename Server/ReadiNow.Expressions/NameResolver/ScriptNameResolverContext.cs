// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Remoting.Messaging;

namespace ReadiNow.Expressions.NameResolver
{
	/// <summary>
	///     Class representing the ScriptNameResolverContext type.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class ScriptNameResolverContext : IDisposable
	{
		/// <summary>
		///     The script name resolver context key
		/// </summary>
		private const string ScriptNameResolverContextKey = "ScriptNameResolverContext";

		/// <summary>
		///     Indicates whether this object has been disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Gets or sets the reason.
		/// </summary>
		/// <value>
		///     The reason.
		/// </value>
		public static NullMemberNameReason Reason
		{
			get
			{
				object reason = CallContext.LogicalGetData( ScriptNameResolverContextKey );

				if ( reason == null )
				{
					return NullMemberNameReason.Unknown;
				}

				return ( NullMemberNameReason ) reason;
			}
			set
			{
				CallContext.LogicalSetData( ScriptNameResolverContextKey, value );
			}
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
			if ( !_disposed )
			{
				/////
				// If called by the finalizer, dont bother restoring the context as you
				// are currently running on a GC Finalizer thread.
				/////
				if ( disposing )
				{
					CallContext.FreeNamedDataSlot( ScriptNameResolverContextKey );
				}

				/////
				// Dispose complete.
				/////
				_disposed = true;
			}
		}

		/// <summary>
		///     Object destructor. This will only get called if Dispose has not been called.
		/// </summary>
		~ScriptNameResolverContext( )
		{
			Dispose( false );
		}
	}
}