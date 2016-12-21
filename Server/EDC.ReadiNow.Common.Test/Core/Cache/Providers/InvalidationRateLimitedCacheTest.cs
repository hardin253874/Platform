// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
    [TestFixture]
    public class DelayedInvalidateCacheTest
    {
        class DummyCacheFactory : ICacheFactory
        {
            public EDC.Cache.ICache<string, string> Inner;

            public bool ThreadSafe { get { return false; } }

            public EDC.Cache.ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
            {
                var result = new DictionaryCacheFactory().Create<TKey, TValue>( cacheName );
                Inner = (EDC.Cache.ICache<string, string>) result;
                return result;
            }
        }


        EDC.Cache.ICache<string, string> CreateCache(DummyCacheFactory innerFactory, TimeSpan expire)
        {
            var factory = new DelayedInvalidateCacheFactory { Inner = innerFactory, ExpirationInterval = expire };
            return factory.Create<string, string>("Test Cache");
        }

        EDC.Cache.ICache<string, string> CreateCache(DummyCacheFactory innerFactory, TimeSpan expire, TimeSpan recent)
        {
            var factory = new DelayedInvalidateCacheFactory {Inner = innerFactory, ExpirationInterval = expire, GlobalThresholdTicks = recent };
            return factory.Create<string, string>("Test Cache");
        }

        [Test]
        public void InvalidatesDontBubble()
        {
            var innerFactory = new DummyCacheFactory();

            var cache = CreateCache(innerFactory, new TimeSpan(1,0,0));

            cache.Add("a", "a");
            cache.Add("b", "b");
            
            string value;

            Assert.That(cache.TryGetValue("a", out value), Is.True);
            Assert.That(cache.TryGetOrAdd("b", out value, (k) => { Assert.Fail(); return ""; }), Is.True);

            innerFactory.Inner.Clear();

            Assert.That(cache.TryGetValue("a", out value), Is.True);
            Assert.That(cache.TryGetOrAdd("b", out value, (k) => { Assert.Fail(); return ""; }), Is.True);
        }


        [Test]
        [RunAsDefaultTenant]
        public void RecentValueTimesOut( )
        {
            var innerFactory = new DummyCacheFactory();

            var cache = CreateCache(innerFactory, new TimeSpan(0, 0, 1));

			using ( ManualResetEvent evt = new ManualResetEvent( false ) )
			{
				ItemsRemovedEventHandler<string> handler = ( sender, args ) =>
				{
					// ReSharper disable once AccessToDisposedClosure
					evt.Set( );
				};

				cache.ItemsRemoved += handler;

				cache.Add( "a", "a" );
				cache.Add( "b", "b" );
				innerFactory.Inner.Clear( );

				evt.WaitOne( 5000 );

				cache.ItemsRemoved -= handler;
			}

            string value;
            Assert.That(cache.TryGetValue("a", out value), Is.False);
            Assert.That(cache.TryGetOrAdd("b", out value, (k) => "c" ), Is.False);

	        
        }


        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [Ignore]
        public void BypassesForRecentUserChanges_Entities( bool delay )
        {
           BypassesForRecentUserChanges((invalidator) =>
           {
               invalidator.OnEntityChange(new List<IEntity>(), ReadiNow.Model.CacheInvalidation.InvalidationCause.Save, (l) => null);
           }, delay );
        }

        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [Ignore]
        public void BypassesForRecentUserChanges_Fields( bool delay )
        {
            BypassesForRecentUserChanges((invalidator) =>
            {
                invalidator.OnFieldChange(new List<long>());
            }, delay );
        }


        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [Ignore]
        public void BypassesForRecentUserChanges_Relationships( bool delay )
        {
            BypassesForRecentUserChanges((invalidator) =>
            {
                invalidator.OnRelationshipChange(new List<EntityRef>());
            }, delay );
        }

        public void BypassesForRecentUserChanges(Action<ICacheInvalidator> act, bool delay)
        {
            var innerFactory = new DummyCacheFactory();

            var cache = CreateCache(innerFactory, new TimeSpan(0, 0, 1));

            var invalidator = ((ICacheService)cache).CacheInvalidator;

            var user = Entity.Create<UserAccount>();

            cache.Add("a", "a");
            innerFactory.Inner.Clear();

            string value;

            Assert.That(cache.TryGetValue("a", out value), Is.True);

            if ( delay )
            {
                Thread.Sleep( 1005 );
            }

            using (new SetUser(user))
            {
                act(invalidator);

                Assert.That(cache.TryGetValue("a", out value), Is.EqualTo(!delay));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [Ignore]
        public void DoesNotBypassesForOtherUsers( )
        {
            var innerFactory = new DummyCacheFactory();

            var cache = CreateCache(innerFactory, new TimeSpan(0, 0, 1));

            var invalidator = ((ICacheService)cache).CacheInvalidator;

            var user1 = Entity.Create<UserAccount>();
            var user2 = Entity.Create<UserAccount>();

            cache.Add("a", "a");
            innerFactory.Inner.Clear();

            string value;

            Assert.That(cache.TryGetValue("a", out value), Is.True);

            using (new SetUser(user1))
            {
                invalidator.OnRelationshipChange(new List<EntityRef>());
            }

            using (new SetUser(user2))
            {
                Assert.That(cache.TryGetValue("a", out value), Is.True);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void DoesBypassesForSecondaryIdentity()
        {
            var innerFactory = new DummyCacheFactory();

            var cache = CreateCache(innerFactory, new TimeSpan(1, 0, 0), TimeSpan.Zero);


            var user1 = Entity.Create<UserAccount>();
            var user2 = Entity.Create<UserAccount>();

            cache.Add("a", "a");
            innerFactory.Inner.Clear();

            string value;

            Assert.That(cache.TryGetValue("a", out value), Is.True);

            using (new SetUser(user1, user2))
            {
                cache.Remove("a");
            }

            using (new SetUser(user2))
            {
                Assert.That(cache.TryGetValue("a", out value), Is.False);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [Ignore]
        public void EnsureCallbackThrottled( )
        {

            var innerFactory = new DummyCacheFactory( );

            EDC.Cache.ICache<string, string> cache = CreateCache( innerFactory, new TimeSpan( 0, 0, 1 ) );

            string key = "1";
            int callCount = 0;

            Action action = ( ) =>
                {
                    string result;
                    cache.TryGetOrAdd( key, out result, key1 =>
                    {
                        Interlocked.Increment( ref callCount );
                        return "A";
                    } );
                    cache.Remove( key );
                };

            Stopwatch sw = new Stopwatch( );
            sw.Start( );
            int runCount = 0;

            while ( sw.ElapsedMilliseconds < 3000 )
            {
                action( );
                Thread.Sleep( 10 );
                Interlocked.Increment( ref runCount );
            }

            Assert.That( runCount, Is.GreaterThan( 250 ).And.LessThan( 350 ) );
            Assert.That( callCount, Is.GreaterThan( 1 ).And.LessThan( 6 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Check1( )
        {
            CacheFactory fact = new CacheFactory
            {
                DelayedInvalidates = true
            };

            var cache = fact.Create<string, string>( "name" );

            Stopwatch sw = new Stopwatch( );
            sw.Start( );

            int callbackCount = 0;
            Func<string, string> callback = key1 =>
            {
                Interlocked.Increment( ref callbackCount );
                Console.WriteLine( sw.ElapsedMilliseconds );
                return "value";
            };

            var user1 = Entity.Create<UserAccount>( );
            var user2 = Entity.Create<UserAccount>( );
            var user3 = Entity.Create<UserAccount>( );
            using ( new SetUser( user1 ) )
            {
                Console.WriteLine( RequestContext.GetContext( ).Identity.Id );
            } 
            using ( new SetUser( user2 ) )
            {
                Console.WriteLine( RequestContext.GetContext( ).Identity.Id );
            }
            UserAccount [ ] users = new [ ] { user1, user2 };
            int userNumber = 0;

            // Fill cache
            string value;
            cache.TryGetOrAdd( "a", out value, callback );
            Assert.That( callbackCount, Is.EqualTo( 1 ) );

            TestHelpers.TestConcurrent( 2, ( ) =>
            {
                int userIndex = Interlocked.Increment( ref userNumber ) - 1;
                var user = users [ userIndex ];
                Console.WriteLine( "Running user {0}", userIndex );

                using ( new SetUser( user ) )
                {
                    cache.Remove( "a" );
                    cache.TryGetOrAdd( "a", out value, callback );
                }
            } );
            Assert.That( callbackCount, Is.EqualTo( 2 ) );
        }
    }
}
