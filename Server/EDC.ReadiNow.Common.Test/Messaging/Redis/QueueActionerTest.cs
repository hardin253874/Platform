// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Messaging;
using NUnit.Framework;
using System;
using EDC.ReadiNow.Messaging.Redis;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
    [TestFixture]
    public class QueueActionerTest
    {
		/// <summary>
		/// The timeout (30 seconds)
		/// </summary>
		private static readonly int Timeout = 30000;

		[TestCase( 1 )]
		[TestCase( 5 )]
		public void GoldenPath( int concurrency )
		{
			string result = "";

			object syncRoot = new object( );

			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			using ( CountdownEvent evt = new CountdownEvent( 1 ) )
			{
				Action<string> action = ( s ) =>
				{
					lock ( syncRoot )
					{
						result += s;
					}

					// ReSharper disable once AccessToDisposedClosure
					evt.Signal( );
				};

				using ( var actioner = new QueueActioner<string>( q, action, concurrency ) )
				{
					Assert.That( actioner.State, Is.EqualTo( ActionerState.Stopped ) );

					q.Enqueue( "a" );                 // wont be processed until started

					Thread.Sleep( 50 );
					Assert.That( result, Is.Empty );

					actioner.Start( );

					evt.Wait( Timeout );
					evt.Reset( );

					Assert.That( result, Is.EqualTo( "a" ) );

					q.Enqueue( "b" );

					evt.Wait( Timeout );
					evt.Reset( );

					Assert.That( result, Is.EqualTo( "ab" ) );

					q.Enqueue( "c" );

					evt.Wait( Timeout );
					evt.Reset( );
					Assert.That( result, Is.EqualTo( "abc" ) );

					var stopped = actioner.Stop( 500 );

					Assert.That( stopped, Is.True );

					Assert.That( actioner.State, Is.EqualTo( ActionerState.Stopped ) );

					q.Enqueue( "d" );             // wont be processed

					Thread.Sleep( 50 );
					Assert.That( result, Is.EqualTo( "abc" ) );

				}
			}
		}


		[TestCase( 1 )]
		[TestCase( 5 )]
		public void ProcessInitialEntries( int concurrency )
		{
			string result = "";

			object syncRoot = new object( );

			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			using ( CountdownEvent evt = new CountdownEvent( 6 ) )
			{
				Action<string> action = ( s ) =>
				{
					lock ( syncRoot )
					{
						result += s;
					}

						// ReSharper disable once AccessToDisposedClosure
						evt.Signal( );
				};

				using ( var actioner = new QueueActioner<string>( q, action, concurrency ) )
				{

					q.Enqueue( "a" );
					q.Enqueue( "b" );
					q.Enqueue( "c" );
					q.Enqueue( "d" );
					q.Enqueue( "e" );
					q.Enqueue( "f" );

					actioner.Start( );

					evt.Wait( );

					Assert.That( q.Length, Is.EqualTo( 0 ) );
					Assert.That( result.Length, Is.EqualTo( 6 ) );
					Assert.That( result, Is.StringContaining( "a" ) );
					Assert.That( result, Is.StringContaining( "b" ) );
					Assert.That( result, Is.StringContaining( "c" ) );
					Assert.That( result, Is.StringContaining( "d" ) );
					Assert.That( result, Is.StringContaining( "e" ) );
					Assert.That( result, Is.StringContaining( "f" ) );
				}
			}
		}

		[TestCase( 1 )]
		[TestCase( 5 )]
		public void CanRestart( int concurrency )
		{
			string result = "";

			object syncRoot = new object( );

			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			using ( CountdownEvent evt = new CountdownEvent( 1 ) )
			{
				Action<string> action = ( s ) =>
				{
					lock ( syncRoot )
					{
						Debug.WriteLine( $"result: {result}, s: {s}" );
						result += s;
					}

						// ReSharper disable once AccessToDisposedClosure
						evt.Signal( );
				};

				using ( var actioner = new QueueActioner<string>( q, action, concurrency ) )
				{


					actioner.Start( );
					q.Enqueue( "a" );

					evt.Wait( Timeout );
					evt.Reset( );

					var stopped = actioner.Stop( 500 );
					Assert.That( stopped, Is.True );

					q.Enqueue( "b" );

					actioner.Start( );

					evt.Wait( Timeout );
					evt.Reset( );

					q.Enqueue( "c" );

					evt.Wait( Timeout );
					evt.Reset( );

					Assert.That( result, Is.EqualTo( "abc" ) );
				}
			}
		}

		[TestCase( 1 )]
		[TestCase( 5 )]
		public void StoppingStateCorrect( int concurrency )
		{
			string result = "";

			object syncRoot = new object( );

			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			using ( CountdownEvent evt = new CountdownEvent( 1 ) )
			{
				Action<string> action = ( s ) =>
				{
					lock ( syncRoot )
					{
						result += s;
					}

						// ReSharper disable once AccessToDisposedClosure
						evt.Signal( );
				};

				using ( var actioner = new QueueActioner<string>( q, action, concurrency ) )
				{
					actioner.Start( );
					q.Enqueue( "a" );

					evt.Wait( Timeout );
					evt.Reset( );

					var retVal = actioner.Stop( 0 );

					if ( !retVal )
					{
						/////
						// By the time we evaluate this statement, the state may have transitioned from Stopping to Stopped.
						/////
						Assert.IsTrue( actioner.State == ActionerState.Stopping || actioner.State == ActionerState.Stopped );
					}

					Assert.That( actioner.Stop( 200 ), Is.True );

					Assert.That( actioner.State, Is.EqualTo( ActionerState.Stopped ) );

					Assert.That( result, Is.EqualTo( "a" ) );

				}
			}
		}

		[TestCase( 1 )]
		[TestCase( 5 )]
		public void AbortsCleanly( int concurrency )
		{
			string result = "";

			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			using ( CountdownEvent evt = new CountdownEvent( 1 ) )
			{
				Action<string> action = ( s ) =>
				{
					lock ( this )
					{
						result += s;
					}

						// ReSharper disable once AccessToDisposedClosure
						evt.Signal( );
				};

				using ( var actioner = new QueueActioner<string>( q, action, concurrency ) )
				{
					actioner.Start( );
					q.Enqueue( "a" );

					evt.Wait( Timeout );
					evt.Reset( );

					actioner.Stop( );

					Assert.That( result, Is.EqualTo( "a" ) );
				}
			}
		}

		[TestCase( 1 )]
		[TestCase( 5 )]
		public void DoubleStartDoesNothing( int concurrency )
		{
			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			{
				Action<string> action = ( s ) =>
				{
				};

				using ( var actioner = new QueueActioner<string>( q, action, concurrency ) )
				{
					actioner.Start( );
					actioner.Start( );
				}
			}
		}


		[TestCase( 1 )]
		[TestCase( 5 )]
		public void DoubleStopDoesNothing( int concurrency )
		{
			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			{
				Action<string> action = ( s ) =>
				{
					Thread.Sleep( 100 );
				};

				using ( var actioner = new QueueActioner<string>( q, action, concurrency ) )
				{
					actioner.Start( );
					Thread.Sleep( 50 );
					actioner.Stop( 1 );
					actioner.Stop( 1 );
				}
			}
		}

		[TestCase( 1 )]
		[TestCase( 5 )]
		public void EnqueuingStoppedDoesNothing( int concurrency )
		{
			string test = null;

			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			{
				Action<string> action = ( s ) =>
				{
					test = s;
				};

				using ( var actioner = new QueueActioner<string>( q, action, concurrency ) )
				{
					actioner.Queue.Enqueue( "dummy" );
					Thread.Sleep( 50 );
					Assert.That( test == null );
				}
			}
		}


		[Test]
		public void ExceptionInActionOk( )
		{
			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			using ( CountdownEvent evt = new CountdownEvent( 2 ) )
			{
				Action<string> action = ( s ) =>
				{
						// ReSharper disable once AccessToDisposedClosure
						evt.Signal( );
				};

				using ( var actioner = new QueueActioner<string>( q, action, 1 ) )
				{
					actioner.Start( );
					q.Enqueue( "a" );
					q.Enqueue( "b" );

					evt.Wait( Timeout );
					evt.Reset( );

					Assert.That( q.Length, Is.EqualTo( 0 ) );
				}
			}
		}



		[Test( Description = "A faster concurrency test that does not use the redis queue so can process more events" )]
		public void ConcurrencyTest_Listener( )
		{
			var mgr = Factory.DistributedMemoryManager;

			using ( var q = CreateQueue( mgr ) )
			{
				ConcurrencyQueueTester( q, 1, 10, 500 );
			}
		}

        [Test]
        [Timeout(30000)]
        public void ConcurrencyTest_Full()
        {
	        var dmMgr = Factory.DistributedMemoryManager;

	        var mgr = dmMgr as RedisManager;

			if ( mgr != null )
			{
				var innerQ = mgr.GetQueue<string>( "FullConcurrencyTest" + Guid.NewGuid( ) );

				using ( var q = new ListeningQueue<string>( innerQ, mgr ) )
				{
					ConcurrencyQueueTester( q, 5, 10, 100 );
				}
			}
        }

        public void ConcurrencyQueueTester(IListeningQueue<string> q, int machineCount, int threadCount, int messagesPerThread)
        { 
            int runCount = 0;
            int sentCount = 0;

            var actionerList = new List<QueueActioner<string>>(threadCount);
	        var counter = new CountdownEvent( threadCount * messagesPerThread );

            try
            {
				for ( int i = 0; i < machineCount; i++ )
				{
					var actioner = new QueueActioner<string>( q, ( s ) =>
					{
						counter.Signal( );
						Interlocked.Increment( ref runCount );
					}, threadCount );

					actionerList.Add( actioner );
					actioner.Start( );
					Thread.Sleep( 500 );
				}

                for (int i=0; i< messagesPerThread * threadCount; i++)
                {
                    q.Enqueue("");
                    sentCount++;
                }

	            counter.Wait( );

				foreach (var actioner in actionerList)
                    actioner.Stop(1000);

				if ( runCount != sentCount )
				{
					Debugger.Launch( );
				}

                Assert.That(runCount, Is.EqualTo(sentCount));
            }
            finally
            {
	            Debug.WriteLine( $"SendCount: {sentCount} - RunCount: {runCount} - Counter: {counter.CurrentCount}" );

                foreach (var actioner in actionerList)
                    actioner.Dispose();
            }
        }

        IListeningQueue<string> CreateQueue( IDistributedMemoryManager mgr )
        {
            var q = new InMemoryTestQueue<string>();
            return new ListeningQueue<string>(q, mgr);
        }
    }

   
}
