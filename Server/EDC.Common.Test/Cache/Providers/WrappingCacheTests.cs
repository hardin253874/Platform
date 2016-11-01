// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.Cache.Providers.MetricRepositories;
using EDC.Collections.Generic;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Messaging;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Cache.Providers
{
    /// <summary>
    /// Test implementations of <see cref="ICache{TKey, TValue}"/> that wrap another cache.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item><description>
    ///     Each public member has its corresponding test.
    ///     </description></item>
    ///     <item><description>
    ///     Each test creates a mock cache that corresponds to that member and 
    ///     ensures the wrapping class calls to the inner cache version, returning 
    ///     the correct value if applicable.
    ///     </description></item>
    ///     <item><description>
    ///     Each test has a corresponding test case source, another property
    ///     that returns the different types of caches to test. If a particular
    ///     wrapping cache has special requirements, this is where they can be
    ///     added to the mocked inner cache.
    ///     </description></item>
    ///     <item><description>
    ///     The test-specific test cases sources call to a shared source, listing
    ///     the different caches to test.
    ///     </description></item>
    /// </list>
    /// <p/>
    /// There may be a better way of approaching this but NUnit hates inheriting test classes.
    /// </remarks>
    [TestFixture]
    public class WrappingCacheTests
    {
        [Test]
        [TestCaseSource("Test_Count_Source")]
        public void Test_Count(string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache)
        {
            ICache<int, string> cache;
            const int testCount = 47;

            mockCache.SetupGet(c => c.Count).Returns(testCount);

            cache = createCache(mockCache.Object);

            Assert.That(cache.Count, Is.EqualTo(testCount));
            mockCache.VerifyAll();
        }

        public IEnumerable<TestCaseData> Test_Count_Source
        {
            get { return WrappingCaches; }
        }
            
        [Test]
        [TestCaseSource("Test_Add_Source")]
        public void Test_Add(string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache)
        {
            ICache<int, string> cache;
            const int testKey = 42;
            const string testValue = "foo";

            mockCache.Setup(c => c.Add(testKey, testValue)).Returns(true);

            cache = createCache(mockCache.Object);
            cache.Add(testKey, testValue);

            mockCache.VerifyAll();
        }

        public IEnumerable<TestCaseData> Test_Add_Source
        {
            get { return WrappingCaches; }
        }

        [Test]
        [TestCaseSource( "Test_TryGetOrAdd_Source" )]
        public void Test_TryGetOrAdd( string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache )
        {
            ICache<int, string> cache;
            const int testKey = 42;
            const string expectedValue = "hello";
            string value;

            Func<int, string> factory = key =>
            {
                return expectedValue;
            };

            mockCache.Setup( c => c.TryGetOrAdd( testKey, out value, factory ) ).Returns(false);

            cache = createCache( mockCache.Object );
            cache.TryGetOrAdd( testKey, out value, factory );

            mockCache.VerifyAll( );
        }

        public IEnumerable<TestCaseData> Test_TryGetOrAdd_Source
        {
            get { return WrappingCaches; }
        }

        [Test]
        [TestCaseSource("Test_Clear_Source")]
        public void Test_Clear(string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache)
        {
            ICache<int, string> cache;

            mockCache.Setup(c => c.Clear());

            cache = createCache(mockCache.Object);
            cache.Clear();

            mockCache.VerifyAll();
        }

        public IEnumerable<TestCaseData> Test_Clear_Source
        {
            get { return WrappingCaches; }
        }

        [Test]
        [TestCaseSource("Test_Remove_Source")]
        public void Test_Remove(string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache)
        {
            ICache<int, string> cache;
            const int testKey = 42;

            mockCache.Setup(c => c.Remove(testKey)).Returns(true);

            cache = createCache(mockCache.Object);

            Assert.That(cache.Remove(testKey), Is.True);
            mockCache.VerifyAll();
        }

        public IEnumerable<TestCaseData> Test_Remove_Source
        {
            get { return WrappingCaches; }
        }

        [Test]
        [TestCaseSource("Test_TryGetValue_Source")]
        public void Test_TryGetValue(string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache)
        {
            ICache<int, string> cache;
            const int testKey = 42;
            const string testValue = "foo";
            string expectedValue;
            string actualValue;

            expectedValue = testValue;
            mockCache.Setup(c => c.TryGetValue(testKey, out expectedValue)).Returns(true);

            cache = createCache(mockCache.Object);

            Assert.That(cache.TryGetValue(testKey, out actualValue), Is.True);
            Assert.That(actualValue, Is.EqualTo(testValue));
            mockCache.VerifyAll();
        }

        public IEnumerable<TestCaseData> Test_TryGetValue_Source
        {
            get { return WrappingCaches; }
        }

        [Test]
        [TestCaseSource("Test_GetEnumerator_Source")]
        public void Test_GetEnumerator(string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache)
        {
            ICache<int, string> cache;
            IEnumerator<KeyValuePair<int, string>> enumerator;

            enumerator = new List<KeyValuePair<int, string>>
                {
                    new KeyValuePair<int, string>(1, "foo"),
                    new KeyValuePair<int, string>(2, "bar"),
                }.GetEnumerator();
            mockCache.Setup(c => c.GetEnumerator()).Returns(enumerator);

            cache = createCache(mockCache.Object);

            Assert.That(cache.GetEnumerator(), 
                Is.EqualTo(enumerator).Using(new EnumeratorEqualityComparer<KeyValuePair<int, string>>()));
            mockCache.VerifyAll();
        }

        public IEnumerable<TestCaseData> Test_GetEnumerator_Source
        {
            get { return WrappingCaches; }
        }

        [Test]
        [TestCaseSource("Test_IndexerGet_Source")]
        public void Test_IndexerGet(string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache)
        {
            ICache<int, string> cache;
            const int testKey = 42;
            const string testValue = "foo";

            mockCache.Setup(c => c[testKey]).Returns(testValue);

            cache = createCache(mockCache.Object);

            Assert.That(cache[testKey], Is.EqualTo(testValue));
            mockCache.VerifyAll();
        }

        public IEnumerable<TestCaseData> Test_IndexerGet_Source
        {
            get { return WrappingCaches.Where(c => (string) c.Arguments[0] != "LoggingCache"); }
        }

        [Test]
        [TestCaseSource("Test_IndexerSet_Source")]
        public void Test_IndexerSet(string cacheName, Func<ICache<int, string>, ICache<int, string>> createCache, Mock<ICache<int, string>> mockCache)
        {
            ICache<int, string> cache;
            const int testKey = 42;
            const string testValue = "foo";
            
            mockCache.SetupSet(c => c[testKey] = testValue);

            cache = createCache(mockCache.Object);
            cache[testKey] = testValue;

            mockCache.VerifyAll();
        }

        public IEnumerable<TestCaseData> Test_IndexerSet_Source
        {
            get { return WrappingCaches.Where(c => (string)c.Arguments[0] != "LoggingCache"); }
        }

        private IEnumerable<TestCaseData> WrappingCaches
        {
            get
            {
                MockRepository mockRepository;
                const string cacheName = "Cache";
                Mock<ILoggingCacheMetricReporter> metricReporterMock;
                Mock<IDistributedMemoryManager> memoryManagerMock;
                Mock<IChannel<RedisPubSubCacheMessage<int>>> channelMock;
                    
                mockRepository = new MockRepository(MockBehavior.Strict);

                metricReporterMock = mockRepository.Create<ILoggingCacheMetricReporter>(MockBehavior.Loose);

                channelMock = mockRepository.Create<IChannel<RedisPubSubCacheMessage<int>>>();
                channelMock.Setup(c => c.Subscribe());
                channelMock.Setup(c => c.Dispose());
                channelMock.Setup(c => c.Publish(It.IsAny<RedisPubSubCacheMessage<int>>(), It.IsAny<PublishOptions>(), 
                    It.IsAny<bool>(), It.IsAny<Action< RedisPubSubCacheMessage<int>, RedisPubSubCacheMessage<int>>>()));
                memoryManagerMock = mockRepository.Create<IDistributedMemoryManager>();
                memoryManagerMock.Setup(mm => mm.IsConnected).Returns(false);
                memoryManagerMock.Setup(mm => mm.Connect(It.IsAny<bool>()));
                memoryManagerMock.Setup(mm => mm.GetChannel<RedisPubSubCacheMessage<int>>(
                    RedisPubSubCacheHelpers.GetChannelName(cacheName))).Returns(channelMock.Object);

                yield return new TestCaseData(
                    "LoggingCache",
                    (Func<ICache<int, string>, ICache<int, string>>)(c => new LoggingCache<int, string>(c, cacheName, metricReporterMock.Object)), 
                    mockRepository.Create<ICache<int, string>>());
                yield return new TestCaseData(
                    "ThreadSafeCache", 
                    (Func<ICache<int, string>, ICache<int, string>>)(c => new ThreadSafeCache<int, string>(c)), 
                    mockRepository.Create<ICache<int, string>>());
                yield return new TestCaseData(
                    "RedisPubSubCache",
					( Func<ICache<int, string>, ICache<int, string>> ) ( c => new RedisPubSubCache<int, string>( c, cacheName, memoryManagerMock.Object ) ),
                    mockRepository.Create<ICache<int, string>>());

                // Note: for many of the providers, general assumptions can't be made about how they will use their inner cache.
            }
        }

        /// <summary>
        /// For the logging cache, allow calls to Count. This is used by Add, Remove and Clear 
        /// for updating the performance counter.
        /// </summary>
        /// <param name="testCaseData"></param>
        /// <returns></returns>
        private static TestCaseData AddLoggingCacheCountStub(TestCaseData testCaseData)
        {
            if (testCaseData == null)
            {
                throw new ArgumentNullException("testCaseData");
            }

            TestCaseData result;

            if ((string)testCaseData.Arguments[0] == "LoggingCache")
            {
                Mock<ICache<int, string>> mockCache = (Mock<ICache<int, string>>)testCaseData.Arguments[2];
                mockCache.SetupGet(c => c.Count).Returns(0);
                result = new TestCaseData(testCaseData.Arguments[0], testCaseData.Arguments[1], mockCache);
            }
            else
            {
                result = testCaseData;
            }

            return result;
        }
    }
}
