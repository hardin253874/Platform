// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Cache.Providers.MetricRepositories;
using EDC.Monitoring;
using EDC.Monitoring.Cache;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Cache.Providers.MetricRepositories
{
    [TestFixture]
    public class PerformanceCounterLoggingCacheMetricTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void UpdateHitCounter_InvalidName(string name)
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;
            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                    new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategory).Object)
            {
                Assert.That(
                    () => metricReporter.UpdateHitCounter(name, 0, 0),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
            }
        }

        [Test]
        public void UpdateHitCounter_NoExistingCounter()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<PercentageRatePerformanceCounter> percentageRatePerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalHitsPerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalMissesPerformanceCounterMock;
            const string name = "foo";
            const long hits = 3;
            const long misses = 5;

            mockRepository = new MockRepository(MockBehavior.Strict);
            percentageRatePerformanceCounterMock = mockRepository.Create<PercentageRatePerformanceCounter>();
            percentageRatePerformanceCounterMock.Setup(prpc => prpc.AddHits(hits));
            percentageRatePerformanceCounterMock.Setup(prpc => prpc.AddMisses(misses));
            totalHitsPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalHitsPerformanceCounterMock.Setup(pc => pc.IncrementBy(hits));
            totalMissesPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalMissesPerformanceCounterMock.Setup(pc => pc.IncrementBy(misses));
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, name))
                                                       .Returns(percentageRatePerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, name))
                                                       .Returns(totalHitsPerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, name))
                                                       .Returns(totalMissesPerformanceCounterMock.Object);

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.UpdateHitCounter(name, hits, misses);

                mockRepository.VerifyAll();
                Assert.That(metricReporter.HitRateCounters.Keys, Is.EquivalentTo(new[] {name}));
                Assert.That(metricReporter.TotalHitsCounters.Keys, Is.EquivalentTo(new[] { name }));
                Assert.That(metricReporter.TotalMissesCounters.Keys, Is.EquivalentTo(new[] { name }));
                Assert.That(metricReporter.SizeCounters, Is.Empty);
            }
        }

        [Test]
        public void UpdateHitCounter_ExistingCounter()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<PercentageRatePerformanceCounter> hitRatePerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalHitsPerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalMissesPerformanceCounterMock;
            const string name = "foo";
            const long hits = 3;
            const long misses = 5;

            mockRepository = new MockRepository(MockBehavior.Strict);
            hitRatePerformanceCounterMock = mockRepository.Create<PercentageRatePerformanceCounter>();
            hitRatePerformanceCounterMock.Setup(pc => pc.AddHits(hits));
            hitRatePerformanceCounterMock.Setup(pc => pc.AddMisses(misses));
            totalHitsPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalHitsPerformanceCounterMock.Setup(pc => pc.IncrementBy(hits));
            totalMissesPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalMissesPerformanceCounterMock.Setup(pc => pc.IncrementBy(misses));
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.HitRateCounters.TryAdd(name, hitRatePerformanceCounterMock.Object);
                metricReporter.TotalHitsCounters.TryAdd(name, totalHitsPerformanceCounterMock.Object);
                metricReporter.TotalMissesCounters.TryAdd(name, totalMissesPerformanceCounterMock.Object);
                metricReporter.UpdateHitCounter(name, hits, misses);

                Assert.That(metricReporter.HitRateCounters.Keys, Is.EquivalentTo(new[] {name}));
                Assert.That(metricReporter.TotalHitsCounters.Keys, Is.EquivalentTo(new[] { name }));
                Assert.That(metricReporter.TotalMissesCounters.Keys, Is.EquivalentTo(new[] { name }));
                Assert.That(metricReporter.SizeCounters, Is.Empty);
            }

            mockRepository.VerifyAll();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void UpdateSizeCounter_InvalidName(string name)
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;
            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategory).Object)
            {
                Assert.That(
                    () => metricReporter.UpdateSizeCounter(name, 0),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
            }
        }

        [Test]
        public void UpdateSizeCounter_NoExistingCounter()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<NumberOfItems64PerformanceCounter> numberOfItems64PerformanceCounterMock;
            const string name = "foo";

            mockRepository = new MockRepository(MockBehavior.Strict);
            numberOfItems64PerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            numberOfItems64PerformanceCounterMock.Setup(prpc => prpc.SetValue(0)); ;
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, name))
                                                       .Returns(numberOfItems64PerformanceCounterMock.Object);

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.UpdateSizeCounter(name, 0);

                mockRepository.VerifyAll();
                Assert.That(metricReporter.SizeCounters.Keys, Is.EquivalentTo(new[] { name }));
                Assert.That(metricReporter.HitRateCounters, Is.Empty);
            }
        }

        [Test]
        public void UpdateSizeCounter_ExistingCounter()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<NumberOfItems64PerformanceCounter> numberOfItems64PerformanceCounterMock;
            const string name = "foo";

            mockRepository = new MockRepository(MockBehavior.Strict);
            numberOfItems64PerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            numberOfItems64PerformanceCounterMock.Setup(prpc => prpc.SetValue(0));;
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.SizeCounters.TryAdd(name, numberOfItems64PerformanceCounterMock.Object);
                metricReporter.UpdateSizeCounter(name, 0);

                mockRepository.VerifyAll();
                Assert.That(metricReporter.SizeCounters.Keys, Is.EquivalentTo(new[] { name }));
                Assert.That(metricReporter.HitRateCounters, Is.Empty);
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void AddSizeCallback_InvalidName(string name)
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;
            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategory).Object)
            {
                Assert.That(
                    () => metricReporter.AddSizeCallback(name, () => 1),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
            }            
        }

        [Test]
        public void AddSizeCallBack_NullFunc()
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;
            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategory).Object)
            {
                Assert.That(
                    () => metricReporter.AddSizeCallback("test", null),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("getSize"));
            }                 
        }

        [Test]
        public void AddSizeCallback_Single()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<NumberOfItems64PerformanceCounter> numberOfItems64PerformanceCounterMock;

            const string cacheName = "test";

            mockRepository = new MockRepository(MockBehavior.Strict);
            numberOfItems64PerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(0));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, cacheName))
                                                       .Returns(numberOfItems64PerformanceCounterMock.Object);

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.AddSizeCallback(cacheName, () => 1);

                Assert.That(metricReporter, Has.Property("SizeCallbacks").Property("Keys").EquivalentTo(new [] { cacheName }));
                Assert.That(metricReporter, Has.Property("HitsAndMissesCallbacks").Empty);
            }              
        }

        [Test]
        public void AddSizeCallback_Mutliple()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<NumberOfItems64PerformanceCounter> numberOfItems64PerformanceCounterMock;

            const string cacheName1 = "test";
            const string cacheName2 = "test2";

            mockRepository = new MockRepository(MockBehavior.Strict);
            numberOfItems64PerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(0));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, cacheName1))
                                                       .Returns(numberOfItems64PerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, cacheName2))
                                                       .Returns(numberOfItems64PerformanceCounterMock.Object);

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.AddSizeCallback(cacheName1, () => 1);
                metricReporter.AddSizeCallback(cacheName2, () => 2);

                Assert.That(metricReporter, Has.Property("SizeCallbacks").Property("Keys").EquivalentTo(new [] { cacheName1, cacheName2 }));
                Assert.That(metricReporter, Has.Property("HitsAndMissesCallbacks").Empty);
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void AddHitsAndMissesCallback_InvalidName(string name)
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;
            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategory).Object)
            {
                Assert.That(
                    () => metricReporter.AddHitsAndMissesCallback(name, () => new HitsAndMisses(1, 1)),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
            }
        }

        [Test]
        public void AddHitsAndMissesCallback_NullFunc()
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;
            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategory).Object)
            {
                Assert.That(
                    () => metricReporter.AddHitsAndMissesCallback("test", null),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("getHitsAndMisses"));
            }
        }

        [Test]
        public void AddHitsAndMissesCallback_Single()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<PercentageRatePerformanceCounter> percentageRatePerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalHitsPerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalMissesPerformanceCounterMock;
            const string cacheName = "test";
            const long hits = 1;
            const long misses = 2;

            mockRepository = new MockRepository(MockBehavior.Strict);
            percentageRatePerformanceCounterMock = mockRepository.Create<PercentageRatePerformanceCounter>();
            percentageRatePerformanceCounterMock.Setup(pc => pc.Zero());
            percentageRatePerformanceCounterMock.Setup(pc => pc.Dispose());
            totalHitsPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalHitsPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalHitsPerformanceCounterMock.Setup(pc => pc.Dispose());
            totalMissesPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalMissesPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalMissesPerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, cacheName))
                                                       .Returns(percentageRatePerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, cacheName))
                                                       .Returns(totalHitsPerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, cacheName))
                                                       .Returns(totalMissesPerformanceCounterMock.Object);

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.AddHitsAndMissesCallback(cacheName, () => new HitsAndMisses(hits, misses));

                Assert.That(metricReporter, Has.Property("HitsAndMissesCallbacks").Property("Keys").EquivalentTo(new[] { cacheName }));
                Assert.That(metricReporter, Has.Property("SizeCallbacks").Empty);
            }
        }

        [Test]
        public void AddHitsAndMissesCallback_Mutliple()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<PercentageRatePerformanceCounter> percentageRatePerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalHitsPerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalMissesPerformanceCounterMock;
            const string cacheName1 = "test1";
            const string cacheName2 = "test2";

            mockRepository = new MockRepository(MockBehavior.Strict);
            percentageRatePerformanceCounterMock = mockRepository.Create<PercentageRatePerformanceCounter>();
            percentageRatePerformanceCounterMock.Setup(pc => pc.Zero());
            percentageRatePerformanceCounterMock.Setup(pc => pc.Dispose());
            totalHitsPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalHitsPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalHitsPerformanceCounterMock.Setup(pc => pc.Dispose());
            totalMissesPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalMissesPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalMissesPerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, cacheName1))
                                                       .Returns(percentageRatePerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, cacheName1))
                                                       .Returns(totalHitsPerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, cacheName1))
                                                       .Returns(totalMissesPerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, cacheName2))
                                                       .Returns(percentageRatePerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, cacheName2))
                                                       .Returns(totalHitsPerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, cacheName2))
                                                       .Returns(totalMissesPerformanceCounterMock.Object);

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.AddHitsAndMissesCallback(cacheName1, () => new HitsAndMisses(1, 2));
                metricReporter.AddHitsAndMissesCallback(cacheName2, () => new HitsAndMisses(3, 4));

                Assert.That(metricReporter, Has.Property("HitsAndMissesCallbacks").Property("Keys").EquivalentTo(new[] { cacheName1, cacheName2 }));
                Assert.That(metricReporter, Has.Property("SizeCallbacks").Empty);
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void RemoveSizeCallback_InvalidName(string name)
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;
            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategory).Object)
            {
                Assert.That(
                    () => metricReporter.RemoveSizeCallback(name),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
            }
        }

        [Test]
        public void RemoveSizeCallback()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<NumberOfItems64PerformanceCounter> numberOfItems64PerformanceCounterMock;

            const string cacheName = "test";

            mockRepository = new MockRepository(MockBehavior.Strict);
            numberOfItems64PerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.SetValue(0));
            numberOfItems64PerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.SizeCounterName, cacheName))
                                                       .Returns(numberOfItems64PerformanceCounterMock.Object);

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.AddSizeCallback(cacheName, () => 1);

                Assert.That(metricReporter, Has.Property("SizeCallbacks").Property("Keys").EquivalentTo(new[] { cacheName }));
                Assert.That(metricReporter, Has.Property("HitsAndMissesCallbacks").Empty);

                metricReporter.RemoveSizeCallback(cacheName);

                Assert.That(metricReporter, Has.Property("SizeCallbacks").Empty);
                Assert.That(metricReporter, Has.Property("HitsAndMissesCallbacks").Empty);
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void RemoveHitsAndMissesCallback_InvalidName(string name)
        {
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;
            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategory).Object)
            {
                Assert.That(
                    () => metricReporter.RemoveHitsAndMissesCallback(name),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
            }
        }

        [Test]
        public void RemoveHitsAndMissesCallback()
        {
            MockRepository mockRepository;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            Mock<PercentageRatePerformanceCounter> percentageRatePerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalHitsPerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> totalMissesPerformanceCounterMock;
            const string cacheName = "test";
            const long hits = 1;
            const long misses = 2;

            mockRepository = new MockRepository(MockBehavior.Strict);
            percentageRatePerformanceCounterMock = mockRepository.Create<PercentageRatePerformanceCounter>();
            percentageRatePerformanceCounterMock.Setup(pc => pc.Zero());
            percentageRatePerformanceCounterMock.Setup(pc => pc.Dispose());
            totalHitsPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalHitsPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalHitsPerformanceCounterMock.Setup(pc => pc.Dispose());
            totalMissesPerformanceCounterMock = mockRepository.Create<NumberOfItems64PerformanceCounter>();
            totalMissesPerformanceCounterMock.Setup(pc => pc.SetValue(0));
            totalMissesPerformanceCounterMock.Setup(pc => pc.Dispose());
            multiInstancePerformanceCounterCategoryMock = mockRepository.Create<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<PercentageRatePerformanceCounter>(CachePerformanceCounters.HitRateCounterName, cacheName))
                                                       .Returns(percentageRatePerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalHitsCounterName, cacheName))
                                                       .Returns(totalHitsPerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(mipcc => mipcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(CachePerformanceCounters.TotalMissesCounterName, cacheName))
                                                       .Returns(totalMissesPerformanceCounterMock.Object);

            using (PerformanceCounterLoggingCacheMetricReporter metricReporter =
                new Mock<PerformanceCounterLoggingCacheMetricReporter>(multiInstancePerformanceCounterCategoryMock.Object).Object)
            {
                metricReporter.AddHitsAndMissesCallback(cacheName, () => new HitsAndMisses(hits, misses));

                Assert.That(metricReporter, Has.Property("HitsAndMissesCallbacks").Property("Keys").EquivalentTo(new[] { cacheName }));
                Assert.That(metricReporter, Has.Property("SizeCallbacks").Empty);

                metricReporter.RemoveHitsAndMissesCallback(cacheName);

                Assert.That(metricReporter, Has.Property("HitsAndMissesCallbacks").Empty);
                Assert.That(metricReporter, Has.Property("SizeCallbacks").Empty);
            }
        }
    }
}
