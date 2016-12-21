// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Threading;

namespace EDC.Threading
{
	/// <summary>
	///     The class represents a named Mutex object.
	/// </summary>
	/// <remarks>
	///     This class is not thread-safe. It is expected that
	///     each thread create its own NamedMutex instance using a common
	///     name.
	/// </remarks>
	[DebuggerStepThrough]
	public class NamedMutex : IDisposable
	{
		#region Fields

		/// <summary>
		///     True if the object has been disposed, false otherwise.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     The mutex object itself.
		/// </summary>
		private Mutex _mutex;

		#endregion

		#region Properties

		#endregion

		#region Constructors

		/// <summary>
		///     Create a mutex with the given name and default access security.
		/// </summary>
		/// <param name="name">The name of the mutex.</param>
		public NamedMutex( string name )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				throw new ArgumentNullException( nameof( name ) );
			}

			var mutexSecurity = new MutexSecurity( );
			mutexSecurity.AddAccessRule( new MutexAccessRule( "BUILTIN\\Administrators", MutexRights.FullControl, AccessControlType.Allow ) );
			mutexSecurity.AddAccessRule( new MutexAccessRule( "BUILTIN\\Users", MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow ) );

			bool createdNew;
			_mutex = new Mutex( false, name, out createdNew, mutexSecurity );
		}


		/// <summary>
		///     Create a mutex with the given name and the specified access security.
		/// </summary>
		/// <param name="name">The name of the mutex.</param>
		/// <param name="mutexSecurity">The security of the mutex.</param>
		public NamedMutex( string name, MutexSecurity mutexSecurity )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				throw new ArgumentNullException( nameof( name ) );
			}

			bool createdNew;
			_mutex = new Mutex( false, name, out createdNew, mutexSecurity );
		}

		#endregion

		#region Synchronisation Methods                

		/// <summary>
		///     Wait indefinitely to acquire the mutex.
		/// </summary>
		/// <returns>True if the mutex was acquired, false otherwise.</returns>
		public bool Acquire( )
		{
			return Acquire( Timeout.Infinite );
		}


		/// <summary>
		///     Wait the specified number of milliseconds to acquire the mutex.
		/// </summary>
		/// <param name="millsecondsTimeout">The number of milliseconds to wait to acquire the mutex.</param>
		/// <returns>True if the mutex was acquired successfully, false otherwise.</returns>
		public bool Acquire( int millsecondsTimeout )
		{
			//if ( _isAcquired )
			//{
			//	// The mutex is already acquired by the current thread. So return.
			//	return true;
			//}

			if ( millsecondsTimeout < -1 )
			{
				throw new ArgumentOutOfRangeException( nameof( millsecondsTimeout ) );
			}

			try
			{
				// Wait for the specified timeout to acquire the mutex
				return _mutex.WaitOne( millsecondsTimeout, false );
			}
			catch ( AbandonedMutexException )
			{
				// Ignore the abandoned mutex exception.
				// This means another process\thread with the mutex has exited before releasing the mutex and
				// this thread now has ownership.
				return true;
			}
		}


		/// <summary>
		///     Releases the mutex.
		/// </summary>
		public void Release( )
		{
			_mutex.ReleaseMutex( );
		}

        /// <summary>
        ///     Return an IDisposable block that acquires and releases the mutex.
        /// </summary>
        public IDisposable AcquireRelease( out bool acquired )
        {
            bool acquiredLocal = Acquire( );
            acquired = acquiredLocal;

            return ContextHelper.Create( () =>
                {
                    if ( acquiredLocal )
                        Release();
                });
        }

		#endregion

		#region IDisposable Methods        

		/// <summary>
		///     Dispose the mutex object.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Dispose the mutex object.
		/// </summary>
		/// <param name="disposing">True if Dispose is called from user code.</param>
		protected virtual void Dispose( bool disposing )
		{
			if ( !_disposed )
			{
				if ( disposing )
				{
					// Disposed of any managed objects
					if ( _mutex != null )
					{
						// If the mutex was acquired release it.
                        try
                        {
                            _mutex.ReleaseMutex( );
                        }
                        catch { }

						_mutex.Close( );
					}
					_mutex = null;
				}
				_disposed = true;
			}
		}

		#endregion
	}
}