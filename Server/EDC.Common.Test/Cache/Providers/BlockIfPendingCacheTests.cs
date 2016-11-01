// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using EDC.ReadiNow.Core.Cache;

namespace EDC.Test.Cache.Providers
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class BlockIfPendingCacheTests
    {
        [TestCase( 1, 1 )]
        [TestCase( 500, 1 )]
        [TestCase( 500, 10 )]
        public void Test_MultipleThreads( int taskCount, int keyCount )
        {
            var factory = new CacheFactory { CacheName = "BlockIfPending", BlockIfPending = true };
            var cache = factory.Create<int, string>( );
            int callCount = 0;
            int cacheCount = 0;

            // Value function
            Func<int, string> valueFactory = key =>
            {
                Interlocked.Increment( ref callCount );
                Thread.Sleep( 200 );    //ms
                return key.ToString();
            };

            // Task performed by each thread
            Action<int> testTask = key =>
            {
                string value;
                bool result = cache.TryGetOrAdd( key, out value, valueFactory );
                if ( value != key.ToString() )
                {
                    throw new Exception( "Incorrect result" );    // note: I don't want to call the NUnit stuff inside this tight thread loop
                }
                if ( result )
                {
                    Interlocked.Increment( ref cacheCount );
                }
            };

            // Run tasks
            Task [ ] tasks = Enumerable.Repeat( "", taskCount )
                .Select( (x, pos) => Task.Factory.StartNew( () => testTask( pos % keyCount ) ) ).ToArray( );

            Task.WaitAll( tasks );

            // Check
            Assert.That( callCount, Is.EqualTo( keyCount ) );
            Assert.That( cacheCount, Is.EqualTo( taskCount - keyCount ) );
        }

        [Test]
        public void Test_Reentrant_DifferentKey( )
        {
            var factory = new CacheFactory { CacheName = "BlockIfPending", BlockIfPending = true };
            var cache = factory.Create<int, string>( );

            // Value function
            Func<int, string> valueFactory = null;
            valueFactory = key =>
            {
                if (key == 1)
                {
                    string value2;
                    cache.TryGetOrAdd(2, out value2, valueFactory);
                    Assert.That(value2, Is.EqualTo("Test2"));
                    return "Test1";
                }
                if (key == 2)
                {
                    return "Test2";
                }
                throw new Exception( );
            };

            string value1;
            cache.TryGetOrAdd( 1, out value1, valueFactory );
            Assert.That( value1, Is.EqualTo( "Test1" ) );            
        }

        [Test]
        public void Test_Reentrant_SameKey( )
        {
            var factory = new CacheFactory { CacheName = "BlockIfPending", BlockIfPending = true };
            var cache = factory.Create<int, string>( );

            // Value function
            Func<int, string> valueFactory = null;
            valueFactory = key =>
            {
                string value2;
                cache.TryGetOrAdd( key, out value2, valueFactory ); // throws
                Assert.Fail( );
                return "";
            };

            string value1;
            Assert.Throws<InvalidOperationException>(
                ( ) => cache.TryGetOrAdd( 1, out value1, valueFactory ) );
        }
    }
}
