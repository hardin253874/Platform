// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EDC.Monitoring;
using NUnit.Framework;

namespace EDC.Test.Monitoring
{
    /// <summary>
    /// These tests require the user to have local administrator privileges.
    /// </summary>
    [TestFixture]
    public class PerformanceCounterCategoryFactoryTests
    {
        [Test]
        public void TestCreateCategory_NullName()
        {
            Assert.That(() => new PerformanceCounterCategoryFactory().CreateCategory(null, "category help", PerformanceCounterCategoryType.SingleInstance),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
        }

        [Test]
        public void TestCreateCategory_NullHelp()
        {
            Assert.That(() => new PerformanceCounterCategoryFactory().CreateCategory("category name", null, PerformanceCounterCategoryType.SingleInstance),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("help"));
        }

        [Test]
        public void TestCreateCategory_UnknownCategory()
        {
            Assert.That(() => new PerformanceCounterCategoryFactory().CreateCategory("category name", "category help", PerformanceCounterCategoryType.Unknown),
                        Throws.TypeOf<ArgumentException>().And.Property("ParamName").EqualTo("categoryType"));
        }

        [Test]
        [TestCaseSource("AddCounterMethods")]
        public void TestAddCounter_NullName(Func<string, string, object> addCounterMethod)
        {
            Assert.That(() => addCounterMethod(null, "counter help"),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("name"));
        }

        [Test]
        [TestCaseSource("AddCounterMethods")]
        public void TestAddCounter_NullHelp(Func<string, string, object> addCounterMethod)
        {
            Assert.That(() => addCounterMethod("counter name", null),
                        Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("help"));
        }

        /// <summary>
        /// Methods used to create counters. Add new methods here to quickly test their argument
        /// validation.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Func<string, string, object>> AddCounterMethods()
        {
            PerformanceCounterCategoryFactory factory;

            factory = new PerformanceCounterCategoryFactory();
            return new Func<string, string, object>[]
                {
                    factory.AddAverageTimer32,
                    factory.AddPercentageRate,
                    factory.AddRatePerSecond32,
                    factory.AddNumberOfItems64
                };
        }

        [Test]
        public void TestEmpty()
        {
            const string categoryName = "category name";
            const string categoryHelp = "category help";

            try
            {
                new PerformanceCounterCategoryFactory()
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.SingleInstance);

                Assert.That(new PerformanceCounterCategory(categoryName).GetCounters(), 
                    Is.Empty);
            }
            finally
            {
                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                }
            }
        }

        [Test]
        public void TestAverageTimerCounter_DiagnosticsAPI()
        {
            const string categoryName = "..category name"; // Leading '.'s place it at the front of the list. Useful for testing.
            const string categoryHelp = "category help";
            const string timerCounterName = "timer counter";
            string baseCounterName = "timer counter" + PerformanceCounterConstants.BaseSuffix;
            const string timerCounterHelp = "timer help";

            PerformanceCounter timerPerformanceCounter = null;
            PerformanceCounter basePerformanceCounter = null;

            try
            {
                new PerformanceCounterCategoryFactory()
                    .AddAverageTimer32(timerCounterName, timerCounterHelp)
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.SingleInstance);

                Assert.DoesNotThrow(() => timerPerformanceCounter = new PerformanceCounter(categoryName, 
                    timerCounterName, true));
                Assert.That(timerPerformanceCounter.CategoryName, Is.EqualTo(categoryName));
                Assert.That(timerPerformanceCounter.CounterName, Is.EqualTo(timerCounterName));
                Assert.That(timerPerformanceCounter.CounterHelp, Is.EqualTo(timerCounterHelp));
                Assert.That(timerPerformanceCounter.CounterType, Is.EqualTo(PerformanceCounterType.AverageTimer32));

                Assert.DoesNotThrow(() => basePerformanceCounter = new PerformanceCounter(categoryName,
                    baseCounterName, true));
                Assert.That(basePerformanceCounter.CategoryName, Is.EqualTo(categoryName));
                Assert.That(basePerformanceCounter.CounterName, Is.EqualTo(baseCounterName));
                Assert.That(basePerformanceCounter.CounterType, Is.EqualTo(PerformanceCounterType.AverageBase));
            }
            finally
            {
                if (timerPerformanceCounter != null)
                {
                    timerPerformanceCounter.Dispose();
                    timerPerformanceCounter = null;
                }
                if (basePerformanceCounter != null)
                {
                    basePerformanceCounter.Dispose();
                    basePerformanceCounter = null;
                }

                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                }
            }
        }

        [Test]
        public void TestRatePerSecond32Counter_DiagnosticsAPI()
        {
            const string categoryName = "..category name"; // Leading '.'s place it at the front of the list. Useful for testing.
            const string categoryHelp = "category help";
            const string timerCounterName = "rate of counts 32 counter";
            const string timerCounterHelp = "rate of counts 32 counter help";

            PerformanceCounter timerPerformanceCounter = null;

            try
            {
                new PerformanceCounterCategoryFactory()
                    .AddRatePerSecond32(timerCounterName, timerCounterHelp)
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.SingleInstance);

                Assert.DoesNotThrow(() => timerPerformanceCounter = new PerformanceCounter(categoryName,
                    timerCounterName, true));
                Assert.That(timerPerformanceCounter.CategoryName, Is.EqualTo(categoryName));
                Assert.That(timerPerformanceCounter.CounterName, Is.EqualTo(timerCounterName));
                Assert.That(timerPerformanceCounter.CounterHelp, Is.EqualTo(timerCounterHelp));
                Assert.That(timerPerformanceCounter.CounterType, Is.EqualTo(PerformanceCounterType.RateOfCountsPerSecond32));
            }
            finally
            {
                if (timerPerformanceCounter != null)
                {
                    timerPerformanceCounter.Dispose();
                    timerPerformanceCounter = null;
                }

                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                }
            }
        }

        [Test]
        public void TestPercentageRateCounter_DiagnosticsAPI()
        {
            const string categoryName = "..category name"; // Leading '.'s place it at the front of the list. Useful for testing.
            const string categoryHelp = "category help";
            const string percentageCounterName = "percentage counter";
            const string percentageCounterHelp = "percentage counter help";
            string baseCounterName = percentageCounterName + PerformanceCounterConstants.BaseSuffix;

            PerformanceCounter sampleFractionPerformanceCounter = null;
            PerformanceCounter sampleBasePerformanceCounter = null;

            try
            {
                new PerformanceCounterCategoryFactory()
                    .AddPercentageRate(percentageCounterName, percentageCounterHelp)
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.SingleInstance);

                Assert.DoesNotThrow(() => sampleFractionPerformanceCounter = new PerformanceCounter(categoryName,
                    percentageCounterName, true));
                Assert.That(sampleFractionPerformanceCounter.CategoryName, Is.EqualTo(categoryName));
                Assert.That(sampleFractionPerformanceCounter.CounterName, Is.EqualTo(percentageCounterName));
                Assert.That(sampleFractionPerformanceCounter.CounterHelp, Is.EqualTo(percentageCounterHelp));
                Assert.That(sampleFractionPerformanceCounter.CounterType, Is.EqualTo(PerformanceCounterType.SampleFraction));

                Assert.DoesNotThrow(() => sampleBasePerformanceCounter = new PerformanceCounter(categoryName,
                    baseCounterName, true));
                Assert.That(sampleBasePerformanceCounter.CategoryName, Is.EqualTo(categoryName));
                Assert.That(sampleBasePerformanceCounter.CounterType, Is.EqualTo(PerformanceCounterType.SampleBase));
            }
            finally
            {
                if (sampleFractionPerformanceCounter != null)
                {
                    sampleFractionPerformanceCounter.Dispose();
                    sampleFractionPerformanceCounter = null;
                }

                if (sampleBasePerformanceCounter != null)
                {
                    sampleBasePerformanceCounter.Dispose();
                    sampleBasePerformanceCounter = null;
                }

                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    PerformanceCounterCategory.Delete(categoryName);
                }
            }
        }

        [Test]
        public void TestNumberOfItems64_DiagnosticsAPI()
        {
            const string categoryName = "..category name"; // Leading '.'s place it at the front of the list. Useful for testing.
            const string categoryHelp = "category help";
            const string numberOfItems64CounterName = "number of items64";
            const string numberOfItems64CounterHelp = "percentage counter help";

            PerformanceCounter numberOfItems64PerformanceCounter = null;

            try
            {
                new PerformanceCounterCategoryFactory()
                    .AddNumberOfItems64(numberOfItems64CounterName, numberOfItems64CounterHelp)
                    .CreateCategory(categoryName, categoryHelp, PerformanceCounterCategoryType.SingleInstance);

                Assert.DoesNotThrow(() => numberOfItems64PerformanceCounter = new PerformanceCounter(categoryName,
                    numberOfItems64CounterName, true));
                Assert.That(numberOfItems64PerformanceCounter.CategoryName, Is.EqualTo(categoryName));
                Assert.That(numberOfItems64PerformanceCounter.CounterName, Is.EqualTo(numberOfItems64CounterName));
                Assert.That(numberOfItems64PerformanceCounter.CounterHelp, Is.EqualTo(numberOfItems64CounterHelp));
                Assert.That(numberOfItems64PerformanceCounter.CounterType, Is.EqualTo(PerformanceCounterType.NumberOfItems64));
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
    }
}
