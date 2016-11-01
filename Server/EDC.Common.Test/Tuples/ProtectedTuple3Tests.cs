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
    public class ProtectedTuple3Tests
    {
        [Test]
        public void Equals()
        {
            var t1 = new TestTuple("abc", 123, 4);
            var t2 = new TestTuple("abc", 120 + "abc".Length, 4);

            Assert.That(t1.Equals(t2), Is.True);
        }

        [Test]
        public void Equals_Null()
        {
            var t1 = new TestTuple("abc", 123, 4);

            Assert.That(t1.Equals(null), Is.False);
        }

        [TestCase("0", 2, 3)]
        [TestCase("a", 0, 3)]
        [TestCase("a", 2, 0)]
        public void Different(string a, long b, int c)
        {
            var t1 = new TestTuple("a", 2, 3);
            var t2 = new TestTuple(a, b, c);

            Assert.That(t1.Equals(t2), Is.False);
        }

        [Test]
        public void GetHashcode()
        {
            var t1 = new TestTuple("abc", 123, 4);
            var t2 = new TestTuple("abc", 120 + "abc".Length, 4);

            Assert.That(t1.GetHashCode(), Is.EqualTo(t2.GetHashCode()));
        }

        [Test]
        public void Values()
        {
            var t1 = new TestTuple("abc", 123, 4);

            Assert.That(t1.A, Is.EqualTo("abc"));
            Assert.That(t1.B, Is.EqualTo(123));
            Assert.That(t1.C, Is.EqualTo(4));
        }

        [Test]
        public void TupleToString()
        {
            var t1 = new TestTuple("abc", 123, 4);

            Assert.That(t1.ToString(), Is.EqualTo("(abc, 123, 4)"));
        }

        class TestTuple : ProtectedTuple<string, long, int>
        {
            public TestTuple(string a, long b, int c) : base(a, b, c)
            {
            }

            public string A { get { return Item1; } }

            public long B { get { return Item2; } }

            public int C { get { return Item3; } }
        }
    }
}
