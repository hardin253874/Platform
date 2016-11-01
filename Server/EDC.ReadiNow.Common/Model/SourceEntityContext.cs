// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Remoting.Messaging;

namespace EDC.ReadiNow.Model
{
	public class SourceEntityContext : IDisposable
	{
		/// <summary>
		///     Context name.
		/// </summary>
		private const string SourceEntityContextKey = "SourceEntityContext";

		/////
		// Context data.
		/////

		/// <summary>
		///     The is owner context
		/// </summary>
		private readonly bool _isOwnerContext;

		/////
		// Current context data.
		/////
		private SourceEntityContextData _contextData;

		/// <summary>
		///     Indicates whether this object has been disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="SourceEntityContext" /> class.
		/// </summary>
		public SourceEntityContext( )
		{
			/////
			// Check the context for a current entity snapshot data
			/////
			_contextData = CallContext.LogicalGetData( SourceEntityContextKey ) as SourceEntityContextData;

			if ( _contextData == null )
			{
				/////
				// No current context found so create one.
				/////
				_contextData = new SourceEntityContextData( );
				_isOwnerContext = true;

				CallContext.LogicalSetData( SourceEntityContextKey, _contextData );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether the source entity is read only.
		/// </summary>
		/// <value>
		///     <c>true</c> if the source entity is read only; otherwise, <c>false</c>.
		/// </value>
		public bool Writable
		{
			get
			{
				return _contextData.Writable;
			}
			set
			{
				/////
				// Only allow writable to override.
				/////
				if ( value )
				{
					_contextData.Writable = true;
				}
			}
		}

		/// <summary>
		///     Gets a value indicating whether the source entity is writable.
		/// </summary>
		/// <value>
		///     <c>true</c> if the source entity is writable; otherwise, <c>false</c>.
		/// </value>
		public static bool? SourceEntityIsWritable
		{
			get
			{
				var contextData = CallContext.LogicalGetData( SourceEntityContextKey ) as SourceEntityContextData;

				if ( contextData != null )
				{
					return contextData.Writable;
				}

				return null;
			}
		}

		#region IDisposable

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
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose( bool disposing )
		{
			if ( !_disposed )
			{
				if ( _isOwnerContext )
				{
					/////
					// This class is the owner so free the context data
					/////
					CallContext.FreeNamedDataSlot( SourceEntityContextKey );
				}

				_contextData = null;

				_disposed = true;
			}
		}

		#endregion

		/// <summary>
		///     The Source Entity Context Data.
		/// </summary>
		private class SourceEntityContextData
		{
			/// <summary>
			///     Gets or sets a value indicating whether the source entity is writable.
			/// </summary>
			/// <value>
			///     <c>true</c> if the source entity is writable; otherwise, <c>false</c>.
			/// </value>
			public bool Writable
			{
				get;
				set;
			}
		}
	}
}