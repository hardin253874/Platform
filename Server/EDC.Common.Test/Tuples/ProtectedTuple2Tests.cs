// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;

namespace EDC.Test.Tuples
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ProtectedTuple2Tests
    {
        [Test]
        public void Equals()
        {
            var t1 = new TestTuple("abc", 123);
            var t2 = new TestTuple("abc", 120 + "abc".Length);

            Assert.That(t1.Equals(t2), Is.True);
        }

        [Test]
        public void Equals_Null()
        {
            var t1 = new TestTuple("abc", 123);

            Assert.That(t1.Equals(null), Is.False);
        }

        [TestCase("b", 1)]
        [TestCase("a", 2)]
        public void Different(string a, long b)
        {
            var t1 = new TestTuple("a", 1);
            var t2 = new TestTuple(a, b);

            Assert.That(t1.Equals(t2), Is.False);
        }

        [Test]
        public void GetHashcode()
        {
            var t1 = new TestTuple("abc", 123);
            var t2 = new TestTuple("abc", 120 + "abc".Length);

            Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
        }

        [Test]
        public void Values()
        {
            var t1 = new TestTuple("abc", 123);

            Assert.That(t1.A, Is.EqualTo("abc"));
            Assert.That(t1.B, Is.EqualTo(123));
        }

        [Test]
        public void TupleToString()
        {
            var t1 = new TestTuple("abc", 123);

            Assert.That(t1.ToString(), Is.EqualTo("(abc, 123)"));
        }

        class TestTuple : ProtectedTuple<string, long>
        {
            public TestTuple(string a, long b) : base(a,b)
            {
            }

            public string A { get { return Item1; } }

            public long B { get { return Item2; } }
        }
    }
}
