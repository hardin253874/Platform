// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Collections.Generic
{
    [TestFixture]
    public class EnumeratorEqualityComparerTests
    {
        [Test]
        public void Test_Ctor_NoArgs()
        {
            EnumeratorEqualityComparer<int> enumeratorEqualityComparer;

            enumeratorEqualityComparer = new EnumeratorEqualityComparer<int>();
            Assert.That(enumeratorEqualityComparer.EqualityComparer, Is.EqualTo(EqualityComparer<int>.Default));
        }

        [Test]
        public void Test_Ctor_Arg()
        {
            Mock<IEqualityComparer<int>> mockEqualityComparer;
            IEqualityComparer<int> elementEqualityComparer;
            EnumeratorEqualityComparer<int> enumeratorEqualityComparer;

            mockEqualityComparer = new Mock<IEqualityComparer<int>>(MockBehavior.Strict);
            elementEqualityComparer = mockEqualityComparer.Object;

            enumeratorEqualityComparer = new EnumeratorEqualityComparer<int>(elementEqualityComparer);
            Assert.That(enumeratorEqualityComparer.EqualityComparer, Is.EqualTo(elementEqualityComparer));
            mockEqualityComparer.VerifyAll();
        }

        [Test]
        [TestCaseSource("Test_Equal_Source")]
        public void Test_Equal(IEnumerable<string> a, IEnumerable<string> b, bool expectedResult)
        {
            Assert.That(
                new EnumeratorEqualityComparer<string>().Equals(a != null ? a.GetEnumerator() : null, b != null ? b.GetEnumerator() : null),
                Is.EqualTo(expectedResult));
        }

        private IEnumerable<TestCaseData> Test_Equal_Source
        {
            get
            {
                yield return new TestCaseData(null, null, true);
                yield return new TestCaseData(null, new string[0], false);
                yield return new TestCaseData(new string[0], null, false);
                yield return new TestCaseData(new [] { "foo" }, null, false);
                yield return new TestCaseData(null, new [] { "foo" }, false);
                yield return new TestCaseData(new[] { "foo" }, new[] { "foo" }, true);
                yield return new TestCaseData(new[] { "foo" }, new[] { "bar" }, false);
                yield return new TestCaseData(new[] { "foo", "bar" }, new[] { "foo" }, false);
                yield return new TestCaseData(new[] { "foo" }, new[] { "foo", "bar" }, false);
                yield return new TestCaseData(new[] { "foo", "bar" }, new[] { "foo", "bar" }, true);
                yield return new TestCaseData(new[] { "foo", "bar" }, new[] { "bar", "foo" }, false);
                yield return new TestCaseData(new[] { "foo", "foo" }, new[] { "foo" }, false);
                yield return new TestCaseData(new[] { "foo", "foo" }, new[] { "foo", "bar" }, false);
                yield return new TestCaseData(new[] { "foo", "foo" }, new[] { "bar", "foo" }, false);
            }
        }
    }
}
