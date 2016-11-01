// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.Collections.Generic;

namespace EDC.Test.Collections.Generic
{
    [TestFixture]
    class CallbackAtEndTests
    {
        [Test]
        public void Test_CallbackAtEnd()
        {
            bool called = false;
            Action callback = () => { called = true; };
            List<int> inner = new[] { 1, 2 }.ToList();

            var wrapped = inner.CallbackAtEnd(callback);

            var res = wrapped.ToArray();

            Assert.That(called, Is.True);
            Assert.That(res, Is.EquivalentTo(inner));
        }

        [Test]
        public void Test_CallbackAtEnd_ViaEnumerator()
        {
            bool called = false;
            Action callback = () => { called = true; };
            List<int> inner = new[] { 1, 2 }.ToList();

            var wrapped = inner.CallbackAtEnd(callback);
            var enumerator = wrapped.GetEnumerator();

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(1));
            Assert.That(called, Is.False);

            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(2));
            Assert.That(called, Is.False);

            Assert.That(enumerator.MoveNext(), Is.False);
            Assert.That(called, Is.True);
        }

        [Test]
        public void Test_CallbackAtEnd_Empty()
        {
            bool called = false;
            Action callback = () => { called = true; };
            List<int> inner = new List<int>();

            var wrapped = inner.CallbackAtEnd(callback);
            var enumerator = wrapped.GetEnumerator();

            Assert.That(enumerator.MoveNext(), Is.False);
            Assert.That(called, Is.True);
        }
    }
}
