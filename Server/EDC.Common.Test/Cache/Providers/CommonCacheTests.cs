// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core.Cache.Providers;
using NUnit.Framework;
using EDC.Cache;
using EDC.Cache.Providers;

namespace EDC.Test.Cache.Providers
{
    /// <summary>
    /// Cache tests that should be passed by all cache providers.
    /// </summary>
    [TestFixture]
    public class CommonCacheTests
    {
        public string CacheName = "Test cache";

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Clear( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            cache.Clear( );
            Assert.That( cache.Count, Is.EqualTo( 0 ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Add_Get( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            string value = cache.Get( 1 );
            Assert.That( value, Is.EqualTo( "Test" ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Add_ReAdd( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            cache.Add( 1, "Test2" );
            string value = cache.Get( 1 );
            Assert.That( value, Is.EqualTo( "Test2" ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Add_Null( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, null );
            string value = cache.Get( 1 );
            Assert.That( value, Is.Null );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_SetIndexer_Get( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache [ 1 ] = "Test";
            string value = cache.Get( 1 );
            Assert.That( value, Is.EqualTo( "Test" ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_SetIndexer_ReAdd( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache [ 1 ] = "Test";
            cache [ 1 ] = "Test2";
            string value = cache.Get( 1 );
            Assert.That( value, Is.EqualTo( "Test2" ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Get_Hit( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            string value = cache.Get( 1 );
            Assert.That( value, Is.EqualTo( "Test" ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Get_Miss( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            string value = cache.Get( 1 );
            // Should this throw?
            Assert.That( value, Is.EqualTo( null ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_GetIndexer_Hit( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            string value = cache [ 1 ];
            Assert.That( value, Is.EqualTo( "Test" ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_GetIndexer_Miss( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            string value = cache [ 1 ];
            // Should this throw?
            Assert.That( value, Is.EqualTo( null ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Count( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            cache.Add( 2, "Test2" );
            Assert.That( cache.Count, Is.EqualTo( 2 ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Count_Empty( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            Assert.That( cache.Count, Is.EqualTo( 0 ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Remove_Existing( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            cache.Add( 2, "Test2" );
            cache.Remove( 1 );
            Assert.That( cache.ContainsKey( 1 ), Is.False );
            Assert.That( cache.ContainsKey( 2 ), Is.True );
            Assert.That( cache.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_Remove_NonExisting( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            cache.Remove( 2 );
            Assert.That( cache.ContainsKey( 1 ), Is.True );
            Assert.That( cache.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_RemoveBatch_NullKeys( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            Assert.Throws<ArgumentNullException>( ( ) => cache.Remove( (IEnumerable<int>)null ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_RemoveBatch_EmptyKeys( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            var removed = cache.Remove( new List<int>() );
            Assert.That( removed, Is.Empty );
            Assert.That( cache.ContainsKey( 1 ), Is.True );
            Assert.That( cache.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_RemoveBatch_Existing( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            cache.Add( 2, "Test" );
            cache.Add( 3, "Test" );
            var removed = cache.Remove( new[] { 1, 2 } );
            Assert.That( removed, Is.EquivalentTo( new [ ] { 1, 2 } ) );
            Assert.That( cache.Keys(), Is.EquivalentTo( new [ ] { 3 } ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_RemoveBatch_NonExisting( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            cache.Add( 2, "Test" );
            cache.Add( 3, "Test" );
            var removed = cache.Remove( new[] { 4, 5 } );
            Assert.That( removed, Is.EquivalentTo( new int[ 0 ] ) );
            Assert.That( cache.Keys(), Is.EquivalentTo( new [ ] { 1, 2, 3 } ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_RemoveBatch_Mixed( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            cache.Add( 2, "Test" );
            cache.Add( 3, "Test" );
            var removed = cache.Remove( new[] { 3, 4 } );
            Assert.That( removed, Is.EquivalentTo( new [ ] { 3 } ) );
            Assert.That( cache.Keys(), Is.EquivalentTo( new [ ] { 1, 2 } ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_TryGetValue_Hit( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            string value;
            bool result = cache.TryGetValue( 1, out value );
            Assert.That( result, Is.True );
            Assert.That( value, Is.EqualTo( "Test" ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_TryGetValue_Miss( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            string value;
            bool result = cache.TryGetValue( 2, out value );
            Assert.That( result, Is.False );
            Assert.That( value, Is.Null );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_TryGetOrAdd_NullValueFactory( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            string value;
            Assert.Throws<ArgumentNullException>( () => cache.TryGetOrAdd( 1, out value, null ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_TryGetOrAdd_Hit( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "Test" );
            string value;
            bool result = cache.TryGetOrAdd( 1, out value, key =>
            {
                Assert.Fail( );
                return null;
            } );
            Assert.That( result, Is.True, "Result" );
            Assert.That( value, Is.EqualTo( "Test" ), "Value" );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_TryGetOrAdd_Miss( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            string value;
            bool result = cache.TryGetOrAdd( 1, out value, key => "Test" );
            Assert.That( result, Is.False, "Result" );
            Assert.That( value, Is.EqualTo( "Test" ), "Value" );
            Assert.That( cache.Count, Is.EqualTo( 1 ), "Count" );
            Assert.That( cache.ContainsKey( 1 ), Is.True, "Contains Key" );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_TryGetOrAdd_Retry( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            string value;
            bool result = cache.TryGetOrAdd( 1, out value, key => "Test" );
            Assert.That( result, Is.False, "Result1" );
            Assert.That( value, Is.EqualTo( "Test" ), "Value1" );

            result = cache.TryGetOrAdd( 1, out value, key =>
            {
                Assert.Fail( );
                return null;
            } );
            Assert.That( result, Is.True, "Result2" );
            Assert.That( value, Is.EqualTo( "Test" ), "Value2" );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_IEnumerable_Empty( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            var values = cache.ToList( );
            Assert.That( values, Has.Count.EqualTo( 0 ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_IEnumerable_NonEmpty( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            cache.Add( 1, "a" );
            cache.Add( 2, "b" );
            var values = cache.ToList( );
            Assert.That( values, Has.Count.EqualTo( 2 ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_ItemsRemoved_SingleRemove( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            List<int> itemsRemoved;
            const int testKey = 42;
            const string testValue = "foo";

            itemsRemoved = new List<int>( );

            cache.ItemsRemoved += ( sender, args ) => itemsRemoved.AddRange( args.Items );

            cache.Add( testKey, testValue );

            Assert.That( itemsRemoved, Is.Empty, "Not empty" );

            cache.Remove( testKey );

            Assert.That( itemsRemoved, Is.Not.Null.And.EquivalentTo( new [ ] { testKey } ), "Not removed" );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_ItemsRemoved_MultipleRemoves( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            List<int> itemsRemoved;
            int [ ] testKeys;

            testKeys = new [ ] { 42, 54, 1, 100, 123456 };

            itemsRemoved = new List<int>( );

            cache.ItemsRemoved += ( sender, args ) => itemsRemoved.AddRange( args.Items );

            foreach ( int testKey in testKeys )
            {
                cache.Add( testKey, testKey.ToString( ) );
            }

            Assert.That( itemsRemoved, Is.Empty, "Not Empty" );

            foreach ( int testKey in testKeys )
            {
                cache.Remove( testKey );
                Assert.That( itemsRemoved, Has.Exactly( 1 ).EqualTo( testKey ), "Not removed" );
            }

            Assert.That( itemsRemoved, Is.EquivalentTo( testKeys ) );
        }

        [Test]
        [TestCaseSource( "Caches" )]
        public void Test_ItemsRemoved_Clear( string cacheName, ICacheFactory factory )
        {
            var cache = factory.Create<int, string>( CacheName );

            List<int> itemsRemoved;
            int [ ] testKeys;

            testKeys = new [ ] { 42, 54, 1, 100, 123456 };

            itemsRemoved = new List<int>( );

            cache.ItemsRemoved += ( sender, args ) => itemsRemoved.AddRange( args.Items );

            foreach ( int testKey in testKeys )
            {
                cache.Add( testKey, testKey.ToString( ) );
            }

            Assert.That( itemsRemoved, Is.Empty, "Not Empty" );

            cache.Clear( );
            Assert.That( itemsRemoved, Is.EquivalentTo( testKeys ) );
        }

        public IEnumerable<TestCaseData> Caches( )
        {
            ICacheFactory inner = new DictionaryCacheFactory( );

            // Factories to test
            ICacheFactory [ ] factories = new ICacheFactory [ ]
            {
                new BlockIfPendingCacheFactory { Inner = inner },
                new DictionaryCacheFactory( ),
                new LoggingCacheFactory { Inner = inner },
                new LruCacheFactory { Inner = inner, MaxSize = 100 },
                new ThreadSafeCacheFactory { Inner = inner },
                new TimeoutCacheFactory { Inner = inner, Expiration = TimeSpan.FromMinutes( 1 ) },
                new TransactionAwareCacheFactory { Inner = inner, Private = inner },
                new StochasticCacheFactory { Inner = inner, MaxSize=100 }
            };

            // Create test data
            // Note: We return factories instead of caches, because NUnit reuses the same instances if the test is rerun. :p
            foreach ( ICacheFactory factory in factories )
            {
                string name = factory.GetType( ).Name.Replace( "Factory", "" );

                yield return new TestCaseData( name, factory );
            }
        }

    }
}
