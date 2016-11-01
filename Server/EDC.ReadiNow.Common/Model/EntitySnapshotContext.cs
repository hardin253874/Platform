// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Remoting.Messaging;
using EDC.ReadiNow.Model.Internal;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	/// </summary>
	public class EntitySnapshotContext : IDisposable
	{
		#region Fields

		/// <summary>
		///     The is owner context
		/// </summary>
		private readonly bool _isOwnerContext;

		/// <summary>
		///     Indicates whether this object has been disposed or not.
		/// </summary>
		private bool _disposed;


        /// <summary>
        ///     The snapshot data.
        /// </summary>
        [ThreadStatic]
        private static EntitySnapshotContextData _snapshotData;

		#endregion Fields

		#region Constructors/Destructor

		/// <summary>
		///     Initializes a new instance of the <see cref="EntitySnapshotContext" /> class.
		/// </summary>
		public EntitySnapshotContext( )
		{
			// Check the context for a current entity snapshot data
            if ( _snapshotData == null )
			{
				// Did not find one, so create one and mark the current class as the owner.
				_isOwnerContext = true;
                _snapshotData = new EntitySnapshotContextData( );
			}
			else
			{
				// Otherwise just borrow the current snapshot data.
				_isOwnerContext = false;
			}
		}


		/// <summary>
		///     Object destructor. This will only get called if Dispose has not been called.
		/// </summary>
		~EntitySnapshotContext( )
		{
			Dispose( false );
		}

		#endregion

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
					// This class is the owner so free the context data
                    _snapshotData = null;
				}

				_disposed = true;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		///     Gets the context data.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		///     This method returns the data instead of a EntitySnapshotContext to avoid
		///     adding objects to the Finalizer queue.
		/// </remarks>
		internal static EntitySnapshotContextData GetContextData( )
		{
			// Get the data from the context
            var snapshotData = _snapshotData;

            if ( _snapshotData != null )
			{
				// Found data. Check that it is valid.
				if ( snapshotData.IsValid )
				{
					return snapshotData;
				}
			}

			return null;
		}

		#endregion Non-Public Methods
	}
}