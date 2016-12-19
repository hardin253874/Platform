// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Remoting.Messaging;

namespace EDC.Database
{
	/// <summary>
	///     Database override.
	/// </summary>
	public class DatabaseOverride : IDisposable
	{
		/// <summary>
		///     The key
		/// </summary>
		private const string Key = "DatabaseOverride";

		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseOverride" /> class.
		/// </summary>
		/// <param name="databaseInfo">The database information.</param>
		public DatabaseOverride( DatabaseInfo databaseInfo )
		{
			DatabaseInfo = databaseInfo;

			var existingOverride = CallContext.LogicalGetData( Key ) as DatabaseInfo;

			if ( existingOverride != null )
			{
				PreviousOverride = existingOverride;
			}

			CallContext.LogicalSetData( Key, databaseInfo );
		}

		/// <summary>
		///     Gets the current.
		/// </summary>
		/// <value>
		///     The current.
		/// </value>
		public static DatabaseInfo Current
		{
			get
			{
				return CallContext.LogicalGetData( Key ) as DatabaseInfo;
			}
		}

		/// <summary>
		///     Gets the database information.
		/// </summary>
		/// <value>
		///     The database information.
		/// </value>
		public DatabaseInfo DatabaseInfo
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets a value indicating whether this instance is active.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is active; otherwise, <c>false</c>.
		/// </value>
		public static bool IsActive
		{
			get
			{
				return CallContext.LogicalGetData( Key ) is DatabaseInfo;
			}
		}

		/// <summary>
		///     Gets or sets the previous override.
		/// </summary>
		/// <value>
		///     The previous override.
		/// </value>
		private DatabaseInfo PreviousOverride
		{
			get;
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
		protected virtual void Dispose( bool disposing )
		{
			if ( _disposed )
			{
				return;
			}

			if ( disposing )
			{
				if ( PreviousOverride != null )
				{
					CallContext.LogicalSetData( Key, PreviousOverride );
				}
				else
				{
					CallContext.FreeNamedDataSlot( Key );
				}
			}

			_disposed = true;
		}
	}
}