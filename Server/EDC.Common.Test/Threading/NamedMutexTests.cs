// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading;
using EDC.Threading;
using NUnit.Framework;

namespace EDC.Test.Threading
{
	/// <summary>
	///     This class is responsible for unit testing the NamedMutex class.
	/// </summary>
	[TestFixture]
	public class NamedMutexTests
	{
		/// <summary>
		///     This test tests acquiring and auto releasing the mutex by multiple threads.
		///     The test does the following:
		///     - creates a list of objects. Each object is designated an owner thread either Thread1 or Thread2
		///     - Thread1 and Thread2 are created and they process the list and remove the objects that belong to them
		///     - Each time an object is removed it is done so within a mutex
		/// </summary>
		/// <param name="autoRelease">True to auto release the mutex, false to explicitly release it.</param>
		[Test]
        [TestCase(true)]
        [TestCase(false)]
        public void MultipleThreads_AcquireRelease(bool autoRelease)
		{
			// Create the shared list of objects
			var sharedObjectsList = new List<SharedObject>( );
			var sharedObjectsListCopy = new List<SharedObject>( );

			for ( int i = 0; i < 1000; i++ )
			{
				sharedObjectsList.Add( new SharedObject( Guid.NewGuid( ), "Thread1" ) );
				sharedObjectsList.Add( new SharedObject( Guid.NewGuid( ), "Thread2" ) );
			}

			// Create a copy of the list and the original one is modified
			// by the threads
			sharedObjectsListCopy.AddRange( sharedObjectsList );

			// This event will start the threads
			var startEvent = new ManualResetEvent( false );

			// Create thread1
			var thread1Tester = new MutexThreadTester( "Thread1", sharedObjectsList, autoRelease, startEvent );
			thread1Tester.Start( );

			// Create thread2
			var thread2Tester = new MutexThreadTester( "Thread2", sharedObjectsList, autoRelease, startEvent );
			thread2Tester.Start( );

			// Start
			startEvent.Set( );

			// Wait for the threads to complete
			thread1Tester.Join( );
			thread2Tester.Join( );

			// Validation     
			// The sum of the objects held by the threads should equal the total shared objects
			Assert.That( sharedObjectsListCopy.Count, 
                Is.EqualTo(thread1Tester.LocalObjectsDictionary.Count + thread2Tester.LocalObjectsDictionary.Count),
                string.Format("The number of objects held by the threads is invalid. Thread 1: {0}, Thread 2: {1}",
                    thread1Tester.LocalObjectsDictionary.Count,
                    thread2Tester.LocalObjectsDictionary.Count));

			foreach ( SharedObject so in sharedObjectsListCopy )
			{
				switch ( so.OwnerThread )
				{
					case "Thread1":
						Assert.That( thread1Tester.LocalObjectsDictionary, Has.Exactly(1).Property("Key").EqualTo( so.Id ), "An object designated for Thread1 was not taken" );
						break;

					case "Thread2":
						Assert.That( thread2Tester.LocalObjectsDictionary, Has.Exactly(1).Property("Key").EqualTo( so.Id ), "An object designated for Thread2 was not taken" );
						break;
				}
			}
		}

		/// <summary>
		///     Shared object used to test mutex with multiple threads
		/// </summary>
		private class SharedObject
		{
			/// <summary>
			///     Constructor
			/// </summary>
			/// <param name="id"></param>
			/// <param name="ownerThread"></param>
			public SharedObject( Guid id, string ownerThread )
			{
				Id = id;
				OwnerThread = ownerThread;
			}

			/// <summary>
			///     The id of the object
			/// </summary>
			public Guid Id
			{
				get;
				private set;
			}


			/// <summary>
			///     The name of the thread that owns this object
			/// </summary>
			public string OwnerThread
			{
				get;
				private set;
			}
		}


		/// <summary>
		///     This class wraps a thread and is used to test multiple Threads accessing the mutex
		/// </summary>
		private class MutexThreadTester
		{
			// The thread itself

			// True to auto release the mutex, false to explicitly remove it.
			private readonly bool _autoRelease;

			private readonly Dictionary<Guid, SharedObject> _localObjectsDictionary = new Dictionary<Guid, SharedObject>( );
			private readonly List<SharedObject> _sharedObjectsList;
			private readonly ManualResetEvent _startEvent;
			private readonly string _threadName;
			private Thread _thread;


			/// <summary>
			///     Initializes a new instance of the <see cref="MutexThreadTester" /> class.
			/// </summary>
			/// <param name="threadName">Name of the thread.</param>
			/// <param name="sharedObjectsList">The shared objects list.</param>
			/// <param name="autoRelease">
			///     if set to <c>true</c> [auto release].
			/// </param>
			/// <param name="startEvent">The start event.</param>
			public MutexThreadTester( string threadName, List<SharedObject> sharedObjectsList, bool autoRelease, ManualResetEvent startEvent )
			{
				_threadName = threadName;
				_sharedObjectsList = sharedObjectsList;
				_startEvent = startEvent;
				_autoRelease = autoRelease;
			}

			/// <summary>
			///     The dictionary of objects owned by this thread.
			/// </summary>
			public Dictionary<Guid, SharedObject> LocalObjectsDictionary
			{
				get
				{
					return _localObjectsDictionary;
				}
			}


			/// <summary>
			///     Waits for the thread to complete.
			/// </summary>
			public void Join( )
			{
				_thread.Join( 60000 );
			}

			/// <summary>
			///     Start the thread.
			/// </summary>
			public void Start( )
			{
				_thread = new Thread( ThreadProc )
					{
						IsBackground = true,
						Name = _threadName
					};
				_thread.Start( );
			}


			/// <summary>
			///     Thread proc method
			/// </summary>
			private void ThreadProc( )
			{
				_startEvent.WaitOne( );

				bool finished = false;

				while ( !finished )
				{
					using ( var mutex = new NamedMutex( "TestMutex" ) )
					{
						if ( mutex.Acquire( ) )
						{
							if ( _sharedObjectsList.Count == 0 )
							{
								finished = true;
							}
							else
							{
								// Get the first object from the list
								SharedObject so = _sharedObjectsList[ 0 ];

								// If it belongs to this thread task it and remove it from the list
								if ( so.OwnerThread == Thread.CurrentThread.Name )
								{
									_localObjectsDictionary[ so.Id ] = so;
									_sharedObjectsList.RemoveAt( 0 );
								}
							}
						}

						if ( !_autoRelease )
						{
							mutex.Release( );
						}
					}
				}
			}
		}

		/// <summary>
		///     Test creating a mutex with an invalid name.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ConstructorNameOnly_InvalidName( )
		{
			using ( new NamedMutex( null ) )
			{
			}
		}

		/// <summary>
		///     Test creating a mutex with a valid name.
		/// </summary>
		[Test]
		public void ConstructorNameOnly_ValidName( )
		{
			using ( var mutex = new NamedMutex( "TestMutex" ) )
			{
				Assert.IsNotNull( mutex, "The mutex should not be null" );
			}
		}

		/// <summary>
		///     Test creating a mutex with an invalid name.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void Constructor_InvalidName( )
		{
			using ( new NamedMutex( null, null ) )
			{
			}
		}

		/// <summary>
		///     Test creating a mutex with a valid name.
		/// </summary>
		[Test]
		public void Constructor_ValidName( )
		{
			using ( var mutex = new NamedMutex( "TestMutex", null ) )
			{
				Assert.IsNotNull( mutex, "The mutex should not be null" );
			}
		}

		/// <summary>
		///     This test creates a thread that abandons a mutex
		///     and then verifies that the current thread can
		///     task ownership of the mutex.
		/// </summary>
		[Test]
		public void MultipleThreads_AbandonedMutex( )
		{
			// Create a thread that abandons a mutex
			var t = new Thread( ( ) =>
				{
					//Create a mutex and do not release it.

					var mutex = new NamedMutex( "AbandonedMutex" );
					mutex.Acquire( );
				} );
			t.Start( );
			// Wait for the thread to finish
			t.Join( );

			using ( var mutex = new NamedMutex( "AbandonedMutex" ) )
			{
				Assert.IsTrue( mutex.Acquire( ), "Abandoned mutex should be acquired" );
			}
		}

		/// <summary>
		///     This test creates a thread that holds a mutex
		///     and verifies that the current thread times out
		/// </summary>
		[Test]
		public void MultipleThreads_TimedOutMutex( )
		{
			using ( AutoResetEvent evt = new AutoResetEvent( false ) )
			using ( AutoResetEvent evt2 = new AutoResetEvent( false ) )
			{
				// Create a thread that abandons a mutex
				var t = new Thread( ( ) =>
					{
						using ( var mutex = new NamedMutex( "TestMutex" ) )
						{
							// Hold the mutex
							mutex.Acquire( );

							// ReSharper disable once AccessToDisposedClosure
							evt2.Set( );

							// ReSharper disable once AccessToDisposedClosure
							evt.WaitOne( );
						}
					} )
				{
					IsBackground = true
				};
				t.Start( );

				evt2.WaitOne( 500 );

				using ( var mutex = new NamedMutex( "TestMutex" ) )
				{
					// Attempt to acquire the mutex after waiting 100 ms
					// This should fail as the thread above is holding the mutex for 10 seconds.
					Assert.IsFalse( mutex.Acquire( 100 ), "Test mutex should not be acquired" );

					evt.Set( );
				}
			}
		}

		/// <summary>
		///     Tests creating a mutex and calling Acquire with an invalid timeout.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentOutOfRangeException ) )]
		public void SingleThread_InvalidAcquireTimeout( )
		{
			using ( var mutex = new NamedMutex( "TestMutex", null ) )
			{
				Assert.IsNotNull( mutex, "The mutex should not be null" );

				mutex.Acquire( -10 );
			}
		}

		/// <summary>
		///     Tests creating a mutex and calling Acquire and then Release once.
		/// </summary>
		[Test]
		public void SingleThread_MultipleAcquireRelease( )
		{
			using ( var mutex = new NamedMutex( "Global\\TestMutex", null ) )
			{
				Assert.IsNotNull( mutex, "The mutex should not be null" );

				bool isAcquired = mutex.Acquire( 0 );

				Assert.IsTrue( isAcquired, "The mutex should be acquired" );

				mutex.Release( );

				isAcquired = mutex.Acquire( 0 );
				Assert.IsTrue( isAcquired, "The mutex should be acquired" );

				isAcquired = mutex.Acquire( 0 );
				Assert.IsTrue( isAcquired, "The mutex should be acquired" );

				mutex.Release( );
				isAcquired = mutex.Acquire( 0 );
				Assert.IsTrue( isAcquired, "The mutex should be acquired" );

				isAcquired = mutex.Acquire( 0 );
				Assert.IsTrue( isAcquired, "The mutex should be acquired" );

				mutex.Release( );
			}
		}


		/// <summary>
		///     Tests creating a mutex and calling Acquire and then Release once.
		/// </summary>
		[Test]
		public void SingleThread_MultipleAcquireThenMultipleRelease( )
		{
			using ( var mutex = new NamedMutex( "TestMutex", null ) )
			{
				Assert.IsNotNull( mutex, "The mutex should not be null" );

				bool isAcquired = mutex.Acquire( );
				Assert.IsTrue( isAcquired, "The mutex should be acquired" );

				isAcquired = mutex.Acquire( );
				Assert.IsTrue( isAcquired, "The mutex should not be acquired" );

				mutex.Release( );
				isAcquired = mutex.Acquire( 0 );
				Assert.IsTrue( isAcquired, "The mutex should be acquired" );

				mutex.Release( );
			}
		}

		/// <summary>
		///     Tests creating a mutex and calling Acquire and having the dispose call Release.
		/// </summary>
		[Test]
		public void SingleThread_SingleAcquireAutoRelease( )
		{
			using ( var mutex = new NamedMutex( "TestMutex", null ) )
			{
				Assert.IsNotNull( mutex, "The mutex should not be null" );

				bool isAcquired = mutex.Acquire( );
				Assert.IsTrue( isAcquired, "The mutex should be acquired" );
			}
		}


		/// <summary>
		///     Tests creating a mutex and calling Acquire and then Release once.
		/// </summary>
		[Test]
		public void SingleThread_SingleAcquireRelease( )
		{
			using ( var mutex = new NamedMutex( "TestMutex", null ) )
			{
				Assert.IsNotNull( mutex, "The mutex should not be null" );

				bool isAcquired = mutex.Acquire( );
				Assert.IsTrue( isAcquired, "The mutex should be acquired" );

				mutex.Release( );
			}
		}
	}
}