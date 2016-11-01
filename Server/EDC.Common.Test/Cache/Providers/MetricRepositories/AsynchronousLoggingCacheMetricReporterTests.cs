// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache.Providers.MetricRepositories;
using EDC.Monitoring;
using EDC.Monitoring.Cache;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Cache.Providers.MetricRepositories
{
    [TestFixture]
    public class AsynchronousLoggingCacheMetricReporterTests
    {
        [Test]
        public void Ctor()
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;

            // ReSharper disable once SuggestUseVarKeywordEvident
            using (AsynchronousLoggingCacheMetricReporter metricRepository =
                new AsynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategory, false))
            {
                Assert.That(metricRepository, Has.Property("Category").EqualTo(multiInstancePerformanceCounterCategory));
                Assert.That(metricRepository, Has.Property("Stopping").False);
                Assert.That(metricRepository, Has.Property("SizeCounters").Empty);
                Assert.That(metricRepository, Has.Property("HitRateCounters").Empty);
                Assert.That(metricRepository, Has.Property("LogHitRates").False);
            }
        }

        [Test]
        public void UpdateSizes_Empty()
        {
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;

            multiInstancePerformanceCounterCategoryMock = new Mock<IMultiInstancePerformanceCounterCategory>();

            using (AsynchronousLoggingCacheMetricReporter metricRepository = new AsynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategoryMock.Object, true))
            {
                AsynchronousLoggingCacheMetricReporter.UpdateSizes(metricRepository);

                Assert.That(metricRepository, Has.Property("SizeCounters").Empty);
                Assert.That(metricRepository, Has.Property("HitRateCounters").Empty);
            }
        }

        [Test]
        public void UpdateSizes_EmptyWithOneUpdate()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<NumberOfItems64PerformanceCounter> numberOfItems64PerformanceCounterMock;
            const string cacheName = "foo";
            const long cacheSize = 42;
            Queue<long> queue;

            mockRepository = new MockRepository(MockBehavior.Strict);
            numberOfItems64PerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(0));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(cacheSize));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, cacheName))
                                                       .Returns(numberOfItems64PerformanceCounterMock.Object);

            queue = new Queue<long>();
            queue.Enqueue(cacheSize);

            using (AsynchronousLoggingCacheMetricReporter metricRepository = new AsynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategoryMock.Object, false))
            {
                metricRepository.AddSizeCallback(cacheName, queue.Dequeue);

                AsynchronousLoggingCacheMetricReporter.UpdateSizes(metricRepository);

                Assert.That(metricRepository,
                    Has.Property("SizeCounters").Property("Keys").EquivalentTo(new[] {cacheName}));
                Assert.That(metricRepository, Has.Property("HitRateCounters").Empty);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateSizes_ExistsWithOneUpdate()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<NumberOfItems64PerformanceCounter> numberOfItems64PerformanceCounterMock;
            const string cacheName = "foo";
            const long cacheSize = 42;
            Queue<long> queue;

            mockRepository = new MockRepository(MockBehavior.Strict);
            numberOfItems64PerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(0));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(cacheSize));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, cacheName))
                                                       .Returns(numberOfItems64PerformanceCounterMock.Object);

            queue = new Queue<long>();
            queue.Enqueue(cacheSize);

            using (AsynchronousLoggingCacheMetricReporter metricRepository = new AsynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategoryMock.Object,
                false))
            {
                metricRepository.AddSizeCallback(cacheName, queue.Dequeue);
                metricRepository.SizeCounters.TryAdd(cacheName, numberOfItems64PerformanceCounterMock.Object);

                AsynchronousLoggingCacheMetricReporter.UpdateSizes(metricRepository);

                Assert.That(metricRepository,
                    Has.Property("SizeCounters").Property("Keys").EquivalentTo(new[] {cacheName}));
                Assert.That(metricRepository, Has.Property("HitRateCounters").Empty);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateSizes_MultipleUpdates()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<NumberOfItems64PerformanceCounter> numberOfItems64PerformanceCounterMock;
            const string cacheName = "foo";
            const long cacheSize1 = 42;
            const long cacheSize2 = 54;
            Queue<long> queue;

            mockRepository = new MockRepository(MockBehavior.Strict);
            numberOfItems64PerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(0));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(cacheSize1));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(cacheSize2));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, cacheName))
                                                       .Returns(numberOfItems64PerformanceCounterMock.Object);

            queue = new Queue<long>();
            queue.Enqueue(cacheSize1);
            queue.Enqueue(cacheSize2);

            using (AsynchronousLoggingCacheMetricReporter metricRepository = new AsynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategoryMock.Object, false))
            {
                metricRepository.AddSizeCallback(cacheName, queue.Dequeue);

                AsynchronousLoggingCacheMetricReporter.UpdateSizes(metricRepository);
                AsynchronousLoggingCacheMetricReporter.UpdateSizes(metricRepository);

                Assert.That(metricRepository,
                    Has.Property("SizeCounters").Property("Keys").EquivalentTo(new[] {cacheName}));
                Assert.That(metricRepository, Has.Property("HitRateCounters").Empty);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateHits_Empty()
        {
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;

            multiInstancePerformanceCounterCategoryMock = new Mock<IMultiInstancePerformanceCounterCategory>();

            using (AsynchronousLoggingCacheMetricReporter metricRepository = new AsynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategoryMock.Object, false))
            {
                AsynchronousLoggingCacheMetricReporter.UpdateHitRates(metricRepository);

                Assert.That(metricRepository, Has.Property("SizeCounters").Empty);
                Assert.That(metricRepository, Has.Property("HitRateCounters").Empty);
            }
        }

        [Test]
        public void UpdateHits_EmptyWithOneUpdate()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<PercentageRatePerformanceCounter> percentageRatePerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalHitsPerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalMissesPerformanceCounterMock;
            const string cacheName = "foo";
            const long hits = 42;
            const long misses = 54;
            Queue<HitsAndMisses> queue;

            mockRepository = new MockRepository(MockBehavior.Strict);
            percentageRatePerformanceCounterMock = mockRepository.Create<PercentageRatePerformanceCounter>();
            percentageRatePerformanceCounterMock.Setup(pc => pc.Zero());
            percentageRatePerformanceCounterMock.Setup(pc => pc.AddHits(hits));
            percentageRatePerformanceCounterMock.Setup(pc => pc.AddMisses(misses));
            percentageRatePerformanceCounterMock.Setup(pc => pc.Dispose());
            totalHitsPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalHitsPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalHitsPerformanceCounterMock.Setup(pc => pc.IncrementBy(hits));
            totalHitsPerformanceCounterMock.Setup(pc => pc.Dispose());
            totalMissesPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalMissesPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalMissesPerformanceCounterMock.Setup(pc => pc.IncrementBy(misses));
            totalMissesPerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, cacheName))
                                                       .Returns(percentageRatePerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, cacheName))
                                                       .Returns(totalHitsPerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, cacheName))
                                                       .Returns(totalMissesPerformanceCounterMock.Object);

            queue = new Queue<HitsAndMisses>();
            queue.Enqueue(new HitsAndMisses(hits, misses));

            using (AsynchronousLoggingCacheMetricReporter metricRepository = new AsynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategoryMock.Object, false, true))
            {
                metricRepository.AddHitsAndMissesCallback(cacheName, queue.Dequeue);

                AsynchronousLoggingCacheMetricReporter.UpdateHitRates(metricRepository);

                Assert.That(metricRepository, Has.Property("SizeCounters").Empty);
                Assert.That(metricRepository, Has.Property("TotalHitsCounters").Property("Keys").EquivalentTo(new[] { cacheName }));
                Assert.That(metricRepository, Has.Property("TotalMissesCounters").Property("Keys").EquivalentTo(new[] { cacheName }));
                Assert.That(metricRepository,
                    Has.Property("HitRateCounters").Property("Keys").EquivalentTo(new[] {cacheName}));
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void UpdateHits_EmptyWithMultipleUpdates()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<PercentageRatePerformanceCounter> hitRatePerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalHitsPerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalMissesPerformanceCounterMock;
            const string cacheName = "foo";
            const long hits1 = 42;
            const long misses1 = 54;
            const long hits2 = 420;
            const long misses2 = 540;
            Queue<HitsAndMisses> queue;
            
            mockRepository = new MockRepository(MockBehavior.Strict);
            hitRatePerformanceCounterMock = mockRepository.Create<PercentageRatePerformanceCounter>();
            hitRatePerformanceCounterMock.Setup(pc => pc.Zero());
            hitRatePerformanceCounterMock.Setup(pc => pc.AddHits(hits1));
            hitRatePerformanceCounterMock.Setup(pc => pc.AddHits(hits2));
            hitRatePerformanceCounterMock.Setup(pc => pc.AddMisses(misses1));
            hitRatePerformanceCounterMock.Setup(pc => pc.AddMisses(misses2));
            hitRatePerformanceCounterMock.Setup(pc => pc.Dispose());
            totalHitsPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalHitsPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalHitsPerformanceCounterMock.Setup(pc => pc.IncrementBy(hits1));
            totalHitsPerformanceCounterMock.Setup(pc => pc.IncrementBy(hits2));
            totalHitsPerformanceCounterMock.Setup(pc => pc.Dispose());
            totalMissesPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalMissesPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalMissesPerformanceCounterMock.Setup(pc => pc.IncrementBy(misses1));
            totalMissesPerformanceCounterMock.Setup(pc => pc.IncrementBy(misses2));
            totalMissesPerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, cacheName))
                                                       .Returns(hitRatePerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, cacheName))
                                                       .Returns(totalHitsPerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, cacheName))
                                                       .Returns(totalMissesPerformanceCounterMock.Object);

            queue = new Queue<HitsAndMisses>();
            queue.Enqueue(new HitsAndMisses(hits1, misses1));
            queue.Enqueue(new HitsAndMisses(hits2, misses2));

            using (AsynchronousLoggingCacheMetricReporter metricRepository = new AsynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategoryMock.Object, false, true))
            {
                metricRepository.AddHitsAndMissesCallback(cacheName, queue.Dequeue);

                AsynchronousLoggingCacheMetricReporter.UpdateHitRates(metricRepository);
                AsynchronousLoggingCacheMetricReporter.UpdateHitRates(metricRepository);

                Assert.That(metricRepository, Has.Property("SizeCounters").Empty);
                Assert.That(metricRepository, Has.Property("HitRateCounters").Property("Keys").EquivalentTo(new[] { cacheName }));
                Assert.That(metricRepository, Has.Property("TotalHitsCounters").Property("Keys").EquivalentTo(new[] { cacheName }));
                Assert.That(metricRepository, Has.Property("TotalMissesCounters").Property("Keys").EquivalentTo(new[] { cacheName }));
            }

            mockRepository.VerifyAll();
        }
    }
}
