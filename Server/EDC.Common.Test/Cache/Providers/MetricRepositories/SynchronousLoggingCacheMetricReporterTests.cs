// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Cache.Providers.MetricRepositories;
using EDC.Monitoring;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Cache.Providers.MetricRepositories
{
    [TestFixture]
    public class SynchronousLoggingCacheMetricReporterTests
    {
        [Test]
        public void Ctor()
        {
            SynchronousLoggingCacheMetricReporter metricRepository;
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;

            metricRepository = new SynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategory);
            Assert.That(metricRepository, Has.Property("Category").EqualTo(multiInstancePerformanceCounterCategory));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void NotifyHitsAndMissesChange_InvalidName(string name)
        {
            SynchronousLoggingCacheMetricReporter metricReporter;
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;

            metricReporter = new SynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategory);
            Assert.That(() => metricReporter.NotifyHitsAndMissesChange(name),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void NotifySizeChange_InvalidName(string name)
        {
            SynchronousLoggingCacheMetricReporter metricReporter;
            IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory;

            multiInstancePerformanceCounterCategory = new Mock<IMultiInstancePerformanceCounterCategory>().Object;

            metricReporter = new SynchronousLoggingCacheMetricReporter(multiInstancePerformanceCounterCategory);
            Assert.That(() => metricReporter.NotifySizeChange(name),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
        }
    }
}
