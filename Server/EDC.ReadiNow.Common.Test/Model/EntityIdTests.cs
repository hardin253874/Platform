// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
    [TestFixture]
	[RunWithTransaction]
    public class EntityIdTests
    {
        [Test]
        [TestCaseSource("TemporaryTests_Source")]
        public void TemporaryTests(long id, bool expectedResult)
        {
            Assert.That(EntityId.IsTemporary(id), Is.EqualTo(expectedResult));
        }

        protected IEnumerable TemporaryTests_Source()
        {
            yield return new TestCaseData(long.MinValue, false);
            yield return new TestCaseData(-1, false);
            yield return new TestCaseData(0, false);
            yield return new TestCaseData(1, false);
            yield return new TestCaseData(EntityId.MinTemporary - 1, false);
            yield return new TestCaseData(EntityId.MinTemporary, true);
            yield return new TestCaseData(EntityId.MinTemporary + 1, true);
            yield return new TestCaseData(EntityId.Max - 1, true);
            yield return new TestCaseData(EntityId.Max, true);
            yield return new TestCaseData(EntityId.Max + 1, false);
            yield return new TestCaseData(long.MaxValue, false);
        }
    }
}
