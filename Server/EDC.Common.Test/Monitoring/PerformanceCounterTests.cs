// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using EDC.Monitoring;
using NUnit.Framework;

namespace EDC.Test.Monitoring
{
    [TestFixture]
    public class PerformanceCounterTests
    {
        public const string TimerName = "Timer";
        public const string RateName = "Rate";
        public const string PercentageName = "Percentage";
        public const string NumberOfItemsName = "NumberOfItems";
        public const string CategoryName = "Category";
        public const string Help = "Help";

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            new PerformanceCounterCategoryFactory()        
                .AddAverageTimer32(TimerName, Help)
                .AddRatePerSecond32(RateName, Help)
                .AddPercentageRate(PercentageName, Help)
                .AddNumberOfItems64(NumberOfItemsName, Help)
                .CreateCategory(CategoryName, Help, PerformanceCounterCategoryType.SingleInstance);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (PerformanceCounterCategory.Exists(CategoryName))
            {
                PerformanceCounterCategory.Delete(CategoryName);
            }
        }

        [Test]
        [TestCaseSource("IncorrectCounterTypeDataSource")]
        public void TestIncorrectCounterType(Func<string, object> method, string counterName)
        {
            method(counterName);
        }

        public IEnumerable<TestCaseData> IncorrectCounterTypeDataSource()
        {
            // There must be an easier way of handling method groups. *sigh*
            SingleInstancePerformanceCounterCategory singleInstancePerformanceCounterCategory = new SingleInstancePerformanceCounterCategory(CategoryName);

            yield return
                new TestCaseData((Func<string, object>) singleInstancePerformanceCounterCategory.GetPerformanceCounter<AverageTimer32PerformanceCounter>, RateName).Throws(
                        typeof (InvalidOperationException));
            yield return
                new TestCaseData((Func<string, object>)singleInstancePerformanceCounterCategory.GetPerformanceCounter<RatePerSecond32PerformanceCounter>, TimerName).Throws(
                        typeof(InvalidOperationException));
            yield return
                new TestCaseData((Func<string, object>) singleInstancePerformanceCounterCategory.GetPerformanceCounter<PercentageRatePerformanceCounter>, RateName).Throws(
                        typeof(InvalidOperationException));
            yield return
                new TestCaseData((Func<string, object>)singleInstancePerformanceCounterCategory.GetPerformanceCounter<NumberOfItems64PerformanceCounter>, RateName).Throws(
                        typeof(InvalidOperationException));
        }

        [Test]
        [TestCase(typeof(AverageTimer32PerformanceCounter))]
        [TestCase(typeof(RatePerSecond32PerformanceCounter))]
        [TestCase(typeof(PercentageRatePerformanceCounter))]
        [TestCase(typeof(NumberOfItems64PerformanceCounter))]
        public void TestNonExistantCounter(Type performanceCounterType)
        {
            MethodInfo methodInfo;
            SingleInstancePerformanceCounterCategory singleInstancePerformanceCounterCategory;

            singleInstancePerformanceCounterCategory  = new SingleInstancePerformanceCounterCategory(CategoryName);
            methodInfo = typeof(SingleInstancePerformanceCounterCategory)
                .GetMethod("GetPerformanceCounter").MakeGenericMethod(performanceCounterType);
            Assert.That(() => methodInfo.Invoke(singleInstancePerformanceCounterCategory, BindingFlags.Default, null,
                new object[] { "Counter does not exist" }, null),
                Throws.TargetInvocationException.And.Property("InnerException").TypeOf<InvalidOperationException>());
        }

        [Test]
        public void TestDuplicateCounter()
        {
            const string duplicateCategoryName = "Duplicate Category Name";

            try
            {
                Assert.That(() => new PerformanceCounterCategoryFactory()
                                 .AddAverageTimer32("Counter", "Help")
                                 .AddAverageTimer32("Counter", "Help")
                                 .CreateCategory(duplicateCategoryName, "Help", PerformanceCounterCategoryType.SingleInstance), 
                                 Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                if (PerformanceCounterCategory.Exists(duplicateCategoryName))
                {
                    PerformanceCounterCategory.Delete(duplicateCategoryName);
                }
            }
        }
    }
}
