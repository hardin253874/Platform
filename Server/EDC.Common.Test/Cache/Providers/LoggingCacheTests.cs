// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.Cache.Providers.MetricRepositories;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Cache.Providers
{
    [TestFixture]
    public class LoggingCacheTests
    {
        [Test]
        public void Ctor_CacheName()
        {
            LoggingCache<int, string> loggingCache;
            ICache<int, string> cache;
            const string cacheName = "foo";

            cache = new Mock<ICache<int, string>>().Object;

            loggingCache = new LoggingCache<int, string>(cache, cacheName);

            Assert.That(loggingCache, Has.Property("InnerCache").EqualTo(cache));
            Assert.That(loggingCache, Has.Property("Name").EqualTo(cacheName));
            Assert.That(loggingCache, Has.Property("MetricReporter").Not.Null);
        }

        [Test]
        public void Ctor_CacheNameCategorySync()
        {
            LoggingCache<int, string> loggingCache;
            ICache<int, string> cache;
            const string cacheName = "foo";
            ILoggingCacheMetricReporter metricReporter;

            cache = new Mock<ICache<int, string>>().Object;
            metricReporter = new Mock<ILoggingCacheMetricReporter>().Object;

            loggingCache = new LoggingCache<int, string>(cache, cacheName, metricReporter);

            Assert.That(loggingCache, Has.Property("InnerCache").EqualTo(cache));
            Assert.That(loggingCache, Has.Property("Name").EqualTo(cacheName));
            Assert.That(loggingCache, Has.Property("MetricReporter").EqualTo(metricReporter));
        }

        [Test]
        public void Ctor_NullCache()
        {
            ILoggingCacheMetricReporter metricReporter;
            const string cacheName = "foo";

            metricReporter = new Mock<ILoggingCacheMetricReporter>().Object;

            Assert.That(() => new LoggingCache<int, string>(null, cacheName, metricReporter),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("innerCache"));
        }

        [Test]
        public void Ctor_NullName()
        {
            ICache<int, string> cache;
            ILoggingCacheMetricReporter metricReporter;

            cache = new Mock<ICache<int, string>>().Object;
            metricReporter = new Mock<ILoggingCacheMetricReporter>().Object;

            Assert.That(() => new LoggingCache<int, string>(cache, null, metricReporter),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
        }

        [Test]
        public void Ctor_NullMetricRepository()
        {
            ICache<int, string> cache;
            const string cacheName = "foo";

            cache = new Mock<ICache<int, string>>().Object;

            Assert.That(() => new LoggingCache<int, string>(cache, cacheName, null),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("metricReporter"));
        }

        [Test]
        public void Indexer_CacheHit()
        {
            LoggingCache<int, int> loggingCache;
            DictionaryCache<int, int> dictionaryCache;
            const string cacheName = "Cache";
            const int testKey = 1;
            const int testValue = 2;
            Mock<ILoggingCacheMetricReporter> metricReporterMock;

            metricReporterMock = new Mock<ILoggingCacheMetricReporter>(MockBehavior.Strict);
            metricReporterMock.Setup(lcmr => lcmr.AddSizeCallback(cacheName, It.IsAny<Func<long>>()));
            metricReporterMock.Setup(lcmr => lcmr.AddHitsAndMissesCallback(cacheName, It.IsAny<Func<HitsAndMisses>>()));
            metricReporterMock.Setup(lcmr => lcmr.NotifyHitsAndMissesChange(cacheName));

            dictionaryCache = new DictionaryCache<int, int>();
            dictionaryCache[testKey] = testValue;
            loggingCache = new LoggingCache<int, int>(dictionaryCache, cacheName, metricReporterMock.Object);

            Assert.That(loggingCache[testKey], Is.EqualTo(testValue));
            metricReporterMock.VerifyAll();
        }

        [Test]
        public void Indexer_CacheMiss()
        {
            LoggingCache<int, int> loggingCache;
            DictionaryCache<int, int> dictionaryCache;
            const string cacheName = "Cache";
            const int testKey = 1;
            const int testValue = 2;
            Mock<ILoggingCacheMetricReporter> metricReporterMock;

            metricReporterMock = new Mock<ILoggingCacheMetricReporter>(MockBehavior.Strict);
            metricReporterMock.Setup(lcmr => lcmr.AddSizeCallback(cacheName, It.IsAny<Func<long>>()));
            metricReporterMock.Setup(lcmr => lcmr.AddHitsAndMissesCallback(cacheName, It.IsAny<Func<HitsAndMisses>>()));
            metricReporterMock.Setup(lcmr => lcmr.NotifyHitsAndMissesChange(cacheName));

            dictionaryCache = new DictionaryCache<int, int>();
            dictionaryCache[testKey] = testValue;
            loggingCache = new LoggingCache<int, int>(dictionaryCache, cacheName, metricReporterMock.Object);

            Assert.That(() => loggingCache[testKey + 1], Is.EqualTo(default(int)));
            metricReporterMock.VerifyAll();
        }

        [Test]
        public void TryGetValue_CacheHit()
        {
            LoggingCache<int, int> loggingCache;
            DictionaryCache<int, int> dictionaryCache;
            const string cacheName = "Cache";
            const int testKey = 1;
            const int testValue = 2;
            int result;
            Mock<ILoggingCacheMetricReporter> metricReporterMock;

            metricReporterMock = new Mock<ILoggingCacheMetricReporter>(MockBehavior.Strict);
            metricReporterMock.Setup(lcmr => lcmr.AddSizeCallback(cacheName, It.IsAny<Func<long>>()));
            metricReporterMock.Setup(lcmr => lcmr.AddHitsAndMissesCallback(cacheName, It.IsAny<Func<HitsAndMisses>>()));
            metricReporterMock.Setup(lcmr => lcmr.NotifyHitsAndMissesChange(cacheName));

            dictionaryCache = new DictionaryCache<int, int>();
            dictionaryCache[testKey] = testValue;
            loggingCache = new LoggingCache<int, int>(dictionaryCache, cacheName, metricReporterMock.Object);

            Assert.That(loggingCache.TryGetValue(testKey, out result), Is.True);
            Assert.That(result, Is.EqualTo(testValue));
            metricReporterMock.VerifyAll();            
        }

        [Test]
        public void TryGetValue_CacheMiss()
        {
            LoggingCache<int, int> loggingCache;
            DictionaryCache<int, int> dictionaryCache;
            const string cacheName = "Cache";
            const int testKey = 1;
            const int testValue = 2;
            int result;
            Mock<ILoggingCacheMetricReporter> metricReporterMock;

            metricReporterMock = new Mock<ILoggingCacheMetricReporter>(MockBehavior.Strict);
            metricReporterMock.Setup(lcmr => lcmr.AddSizeCallback(cacheName, It.IsAny<Func<long>>()));
            metricReporterMock.Setup(lcmr => lcmr.AddHitsAndMissesCallback(cacheName, It.IsAny<Func<HitsAndMisses>>()));
            metricReporterMock.Setup(lcmr => lcmr.NotifyHitsAndMissesChange(cacheName));

            dictionaryCache = new DictionaryCache<int, int>();
            dictionaryCache[testKey] = testValue;
            loggingCache = new LoggingCache<int, int>(dictionaryCache, cacheName, metricReporterMock.Object);

            Assert.That(loggingCache.TryGetValue(testKey + 1, out result), Is.False);
            metricReporterMock.VerifyAll();
        }

        [Test]
        [Explicit]
        public void BasicTest()
        {
            LoggingCache<int, int> loggingCache;
            DictionaryCache<int, int> dictionaryCache;
            const int maxCached = 200;

            using (AsynchronousLoggingCacheMetricReporter metricReporter = new AsynchronousLoggingCacheMetricReporter())
            {
                dictionaryCache = new DictionaryCache<int, int>();
                loggingCache = new LoggingCache<int, int>(dictionaryCache, "test", metricReporter);
                for (int i = 0; i < maxCached; i++)
                {
                    loggingCache.Add(i, i);
                }

                // Hits
                int b = loggingCache[20];
                int c = loggingCache[21];

                // Misses
                int d = loggingCache[maxCached + 1];
                int e = loggingCache[maxCached + 2];
            }
        }
    }
}
