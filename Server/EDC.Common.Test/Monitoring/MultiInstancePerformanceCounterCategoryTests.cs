// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using EDC.Monitoring;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Monitoring
{
    /// <summary>
    /// These tests require the user to have local administrator privileges.
    /// </summary>
    [TestFixture]
    public class MultiInstancePerformanceCounterCategoryTests
    {
        [Test]
        public void TestCreate_NullName()
        {
            Assert.That(() => new MultiInstancePerformanceCounterCategory(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("categoryName"));
        }

        [Test]
        public void TestCreate()
        {
            const string categoryName = "Category Name";

            using (MultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory =
                    new MultiInstancePerformanceCounterCategory(categoryName))
            {
                Assert.That(multiInstancePerformanceCounterCategory.CategoryName, Is.EqualTo(categoryName));
            }
        }

        [Test]
        public void TestGetPerformanceCounter_NullCounter()
        {
            const string categoryName = "Category Name";

            using (MultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory =
                    new MultiInstancePerformanceCounterCategory(categoryName))
            {
                Assert.That(() => multiInstancePerformanceCounterCategory.GetPerformanceCounter<AverageTimer32PerformanceCounter>(null, "instance name"), 
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("counterName"));
            }            
        }

        [Test]
        public void TestGetPerformanceCounter_NullInstance()
        {
            const string categoryName = "Category Name";

            using (MultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory =
                    new MultiInstancePerformanceCounterCategory(categoryName))
            {
                Assert.That(() => multiInstancePerformanceCounterCategory.GetPerformanceCounter<AverageTimer32PerformanceCounter>("counter name", null),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("instanceName"));
            }
        }

        [Test]
        public void TestGetPerformanceCounter_AverageTimer()
        {
            const string categoryName = "..category name"; // Leading '.'s place it at the front of the list. Useful for testing.
            const string categoryHelp = "category help";
            const string timerCounterName = "timer counter";
            const string timerCounterHelp = "timer help";
            string baseCounterName = "timer counter" + PerformanceCounterConstants.BaseSuffix;
            const string instanceName = "instance name";

            AverageTimer32PerformanceCounter averageTimerPerformanceCounter = null;

            try
            {
                new PerformanceCounterCategoryFactory()
                    .AddAverageTimer32(timerCounterName, timerCounterHelp)
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.MultiInstance);

                using (MultiInstancePerformanceCounterCategory singleInstancePerformanceCounterCategory =
                    new MultiInstancePerformanceCounterCategory(categoryName))
                {
                    averageTimerPerformanceCounter =
                        singleInstancePerformanceCounterCategory.GetPerformanceCounter<AverageTimer32PerformanceCounter>(
                        timerCounterName, instanceName);

                    Assert.That(averageTimerPerformanceCounter.Timer.CategoryName, Is.EqualTo(categoryName));
                    Assert.That(averageTimerPerformanceCounter.Timer.CounterName, Is.EqualTo(timerCounterName));
                    Assert.That(averageTimerPerformanceCounter.Timer.CounterHelp, Is.EqualTo(timerCounterHelp));
                    Assert.That(averageTimerPerformanceCounter.Timer.InstanceName,
                                Is.EqualTo(instanceName));

                    Assert.That(averageTimerPerformanceCounter.Base.CategoryName, Is.EqualTo(categoryName));
                    Assert.That(averageTimerPerformanceCounter.Base.CounterName, Is.EqualTo(baseCounterName));
                    Assert.That(averageTimerPerformanceCounter.Base.InstanceName,
                                Is.EqualTo(instanceName));
                }
            }
            finally
            {
                if (averageTimerPerformanceCounter != null)
                {
                    averageTimerPerformanceCounter.Dispose();
                    averageTimerPerformanceCounter = null;
                }

                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                }
            }
        }

        [Test]
        public void TestGetPerformanceCounter_RatePerSecond32()
        {
            const string categoryName = "..category name"; // Leading '.'s place it at the front of the list. Useful for testing.
            const string categoryHelp = "category help";
            const string timerCounterName = "rate of counts 32 counter";
            const string timerCounterHelp = "rate of counts 32 counter help";
            const string instanceName = "instance name";

            RatePerSecond32PerformanceCounter ratePerSecond32PerformanceCounter = null;

            try
            {
                new PerformanceCounterCategoryFactory()
                    .AddRatePerSecond32(timerCounterName, timerCounterHelp)
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.MultiInstance);

                using (MultiInstancePerformanceCounterCategory singleInstancePerformanceCounterCategory =
                    new MultiInstancePerformanceCounterCategory(categoryName))
                {
                    ratePerSecond32PerformanceCounter =
                        singleInstancePerformanceCounterCategory.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                        timerCounterName, instanceName);

                    Assert.That(ratePerSecond32PerformanceCounter.Rate.CategoryName, Is.EqualTo(categoryName));
                    Assert.That(ratePerSecond32PerformanceCounter.Rate.CounterName, Is.EqualTo(timerCounterName));
                    Assert.That(ratePerSecond32PerformanceCounter.Rate.CounterHelp, Is.EqualTo(timerCounterHelp));
                    Assert.That(ratePerSecond32PerformanceCounter.Rate.InstanceName,
                                Is.EqualTo(instanceName));
                }
            }
            finally
            {
                if (ratePerSecond32PerformanceCounter != null)
                {
                    ratePerSecond32PerformanceCounter.Dispose();
                    ratePerSecond32PerformanceCounter = null;
                }

                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                }
            }
        }

        [Test]
        public void TestGetPerformanceCounter_PercentageRate()
        {
            const string categoryName = "..category name";
            // Leading '.'s place it at the front of the list. Useful for testing.
            const string categoryHelp = "category help";
            const string percentageCounterName = "percentage counter";
            const string percentageCounterHelp = "percentage help";
            string baseCounterName = percentageCounterName + PerformanceCounterConstants.BaseSuffix;
            const string instanceName = "instance name";

            PercentageRatePerformanceCounter percentageRatePerformanceCounter = null;

            try
            {
                new PerformanceCounterCategoryFactory()
                    .AddPercentageRate(percentageCounterName, percentageCounterHelp)
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.MultiInstance);

                using (MultiInstancePerformanceCounterCategory singleInstancePerformanceCounterCategory =
                    new MultiInstancePerformanceCounterCategory(categoryName))
                {
                    percentageRatePerformanceCounter =
                        singleInstancePerformanceCounterCategory.GetPerformanceCounter<PercentageRatePerformanceCounter>(
                        percentageCounterName, instanceName);

                    Assert.That(percentageRatePerformanceCounter.SampleFraction.CategoryName, Is.EqualTo(categoryName));
                    Assert.That(percentageRatePerformanceCounter.SampleFraction.CounterName,
                                Is.EqualTo(percentageCounterName));
                    Assert.That(percentageRatePerformanceCounter.SampleFraction.CounterHelp,
                                Is.EqualTo(percentageCounterHelp));
                    Assert.That(percentageRatePerformanceCounter.SampleFraction.InstanceName,
                                Is.EqualTo(instanceName));

                    Assert.That(percentageRatePerformanceCounter.SampleBase.CategoryName, Is.EqualTo(categoryName));
                    Assert.That(percentageRatePerformanceCounter.SampleBase.CounterName, Is.EqualTo(baseCounterName));
                    Assert.That(percentageRatePerformanceCounter.SampleBase.InstanceName,
                                Is.EqualTo(instanceName));
                }
            }
            finally
            {
                if (percentageRatePerformanceCounter != null)
                {
                    percentageRatePerformanceCounter.Dispose();
                    percentageRatePerformanceCounter = null;
                }

                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                }
            }
        }

        [Test]
        public void TestGetPerformanceCounter_NumberOfItems64()
        {
            const string categoryName = "..category name";
            // Leading '.'s place it at the front of the list. Useful for testing.
            const string categoryHelp = "category help";
            const string numberOfItems64CounterName = "percentage counter";
            const string numberOfItems64CounterHelp = "percentage help";
            const string instanceName = "instance name";

            NumberOfItems64PerformanceCounter numberOfItems64PerformanceCounter = null;

            try
            {
                new PerformanceCounterCategoryFactory()
                    .AddNumberOfItems64(numberOfItems64CounterName, numberOfItems64CounterHelp)
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.MultiInstance);

                using (MultiInstancePerformanceCounterCategory singleInstancePerformanceCounterCategory =
                    new MultiInstancePerformanceCounterCategory(categoryName))
                {
                    numberOfItems64PerformanceCounter =
                        singleInstancePerformanceCounterCategory.GetPerformanceCounter<NumberOfItems64PerformanceCounter>
                            (numberOfItems64CounterName, instanceName);

                    Assert.That(numberOfItems64PerformanceCounter.Counter.CategoryName,
                                Is.EqualTo(categoryName));
                    Assert.That(numberOfItems64PerformanceCounter.Counter.CounterName,
                                Is.EqualTo(numberOfItems64CounterName));
                    Assert.That(numberOfItems64PerformanceCounter.Counter.CounterHelp,
                                Is.EqualTo(numberOfItems64CounterHelp));
                    Assert.That(numberOfItems64PerformanceCounter.Counter.InstanceName,
                                Is.EqualTo(instanceName));
                }
            }
            finally
            {
                if (numberOfItems64PerformanceCounter != null)
                {
                    numberOfItems64PerformanceCounter.Dispose();
                    numberOfItems64PerformanceCounter = null;
                }

                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                }
            }
        }

        /// <summary>
        /// Ensure <see cref="MultiInstancePerformanceCounterCategory"/> is mockable using Moq.
        /// </summary>
        [Test]
        public void TestMoq()
        {
            Mock<AverageTimer32PerformanceCounter> averageTimer32PerformanceCounterMock;
            Mock<NumberOfItems64PerformanceCounter> numberofItems64PerformanceCounterMock;
            Mock<PercentageRatePerformanceCounter> percentageRatePerformanceCounterMock;
            Mock<RatePerSecond32PerformanceCounter> ratePerSecond32PerformanceCounterMock;
            Mock<IMultiInstancePerformanceCounterCategory> multiInstancePerformanceCounterCategoryMock;
            const string counterName = "counterName";
            const string instanceName = "instanceName";

            averageTimer32PerformanceCounterMock = new Mock<AverageTimer32PerformanceCounter>();
            averageTimer32PerformanceCounterMock.Setup(pc => pc.AddTiming(It.IsAny<Stopwatch>()));
            averageTimer32PerformanceCounterMock.Setup(pc => pc.Dispose());

            numberofItems64PerformanceCounterMock = new Mock<NumberOfItems64PerformanceCounter>();
            numberofItems64PerformanceCounterMock.Setup(pc => pc.Increment());
            numberofItems64PerformanceCounterMock.Setup(pc => pc.IncrementBy(It.IsAny<long>()));
            numberofItems64PerformanceCounterMock.Setup(pc => pc.Dispose());

            percentageRatePerformanceCounterMock = new Mock<PercentageRatePerformanceCounter>();
            percentageRatePerformanceCounterMock.Setup(pc => pc.AddHit());
            percentageRatePerformanceCounterMock.Setup(pc => pc.AddMiss());
            percentageRatePerformanceCounterMock.Setup(pc => pc.Dispose());

            ratePerSecond32PerformanceCounterMock = new Mock<RatePerSecond32PerformanceCounter>();
            ratePerSecond32PerformanceCounterMock.Setup(pc => pc.Increment());
            ratePerSecond32PerformanceCounterMock.Setup(pc => pc.Dispose());

            multiInstancePerformanceCounterCategoryMock = new Mock<IMultiInstancePerformanceCounterCategory>();
            multiInstancePerformanceCounterCategoryMock.Setup(
                pcc => pcc.GetPerformanceCounter<AverageTimer32PerformanceCounter>(It.IsAny<string>(), It.IsAny<string>()))
                                                        .Returns(() => averageTimer32PerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(
                pcc => pcc.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(It.IsAny<string>(), It.IsAny<string>()))
                                                        .Returns(() => numberofItems64PerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(
                pcc => pcc.GetPerformanceCounter<PercentageRatePerformanceCounter>(It.IsAny<string>(), It.IsAny<string>()))
                                                        .Returns(() => percentageRatePerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(
                pcc => pcc.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(It.IsAny<string>(), It.IsAny<string>()))
                                                        .Returns(() => ratePerSecond32PerformanceCounterMock.Object);
            multiInstancePerformanceCounterCategoryMock.Setup(pcc => pcc.Dispose());

            using (IMultiInstancePerformanceCounterCategory multiInstancePerformanceCounterCategory =
                    multiInstancePerformanceCounterCategoryMock.Object)
            {
                multiInstancePerformanceCounterCategory.GetPerformanceCounter<AverageTimer32PerformanceCounter>(
                    counterName, instanceName).AddTiming(new Stopwatch());
                multiInstancePerformanceCounterCategory.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                    counterName, instanceName).Increment();
                multiInstancePerformanceCounterCategory.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                    counterName, instanceName).IncrementBy(0);
                multiInstancePerformanceCounterCategory.GetPerformanceCounter<PercentageRatePerformanceCounter>(
                    counterName, instanceName).AddHit();
                multiInstancePerformanceCounterCategory.GetPerformanceCounter<PercentageRatePerformanceCounter>(
                    counterName, instanceName).AddMiss();
                multiInstancePerformanceCounterCategory.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                    counterName, instanceName).Increment();
            }
        }
    }
}
