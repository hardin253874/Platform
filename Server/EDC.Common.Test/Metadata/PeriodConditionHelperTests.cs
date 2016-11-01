// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Query.Structured;
using NUnit.Framework;

namespace EDC.Test.Metadata
{
    [TestFixture]
    public class PeriodConditionHelperTests
    {
        [Test]
        [TestCase("01/01/0001", Result = 0 /*2013x4 => 2013 years since 0001*/ )] //first day
        [TestCase("25 Feb 0001", Result = 0 /*2013x4 => 2013 years since 0001*/ )] //first quarter
        [TestCase("25 Feb 0002", Result = 4 /*2013x4 => 2013 years since 0001*/ )] //first quarter of 2nd year
        [TestCase("2014 Jan 2", Result = 8052 /*2013x4 => 2013 years since 0001*/ )] //a recent quarter
        [TestCase("2014 Dec 31", Result = 8055 /*2013x4 => 2013 years since 0001*/ )] //last day-of-year
        public int GetQuarterIndexSinceBc(string dateAsString)
        {
            var date = DateTime.Parse(dateAsString);
            return PeriodConditionHelper.GetQuarterIndexSinceBc(date);
        }

        [Test]
        [TestCase(0, Result = 1)] //Q1 of 0001
        [TestCase(3, Result = 10)] //Q4 of 0001
        [TestCase(8052, Result = 1)] //Q1 of 2014
        [TestCase(8055, Result = 10)] //Q4 of 2014
        public int GetFirstMonthOfQuarter(int quarter)
        {
            return PeriodConditionHelper.GetFirstMonthOfQuarter(quarter);
        }

        [Test]
        [TestCase(0, Result = 1)] //Q1 of 0001
        [TestCase(3, Result = 1)] //Q4 of 0001
        [TestCase(8052, Result = 2014)] //Q1 of 2014
        [TestCase(8055, Result = 2014)] //Q4 of 2014
        public int GetYearFromQuarter(int quarter)
        {
            return PeriodConditionHelper.GetYearFromQuarter(quarter);
        }

        [Test]
        public void GetStartAndEndDatesOfQuarterRange()
        {
            //arrange
            var date = DateTime.Parse("25 Feb 2014");
            var startQuarterIndex = PeriodConditionHelper.GetQuarterIndexSinceBc(date);
            var endQuarterIndex = startQuarterIndex + 1;

            //act
            DateTime startDate, startDateOfNextQuarter;
            PeriodConditionHelper.GetStartAndEndDateOfQuarterRange(startQuarterIndex, endQuarterIndex, out startDate, out startDateOfNextQuarter);

            //assert
            Assert.That(startDate, Is.EqualTo(DateTime.Parse("01 Jan 2014")));
            Assert.That(startDateOfNextQuarter, Is.EqualTo(DateTime.Parse("01 Jul 2014")));
        }

        [Test]
        public void GetFirstDayOfWeekReturnsSameDayForFirstDayOfWeek()
        {
            //arrange
            var monday = DateTime.Parse("6 Apr 2015");

            //act
            var actual = PeriodConditionHelper.GetFirstDayOfWeek(monday);

            //assert
            Assert.That(actual, Is.EqualTo(monday));
        }

        [Test]
        public void GetFirstDayOfWeekReturnsMondayDateForDayInMiddleOfWeek()
        {
            //arrange
            var wednesday = DateTime.Parse("8 Apr 2015");

            //act
            var actual = PeriodConditionHelper.GetFirstDayOfWeek(wednesday);

            //assert
            var monday = DateTime.Parse("6 Apr 2015");
            Assert.That(actual, Is.EqualTo(monday));
        }
    }
}
