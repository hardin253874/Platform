// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.Test.Collections.Generic
{
    [TestFixture]
    public class BatchingEnumeration_Test
    {

        [Test]
        public void Empty()
        {

            var list = new BatchingEnumeration<object, object>(
                new List<object>(), 
                10,
                (l) => {
                    Assert.Fail("Nothing should be fetched");
                    return l; 
                });


            Assert.That(list, Is.Empty);
            Assert.That(list.GetEnumerator().MoveNext(), Is.False);
        }

        [TestCase(5, 0, 0)]
        [TestCase(5, 5, 1)]
        [TestCase(5, 6, 2)]
        [TestCase(5, 10, 2)]
        [TestCase(5, 11, 3)]
        [TestCase(1, 10, 10)]
        public void TestNumberOfFetches(int batchSize, int listSize, int expectedFetches)
        {
            var fetches = 0;
            var list = CreateList(listSize);

            var enumeration = new BatchingEnumeration<int, int>(
                list, 
                batchSize, 
                (l) => {
                    fetches++;
                    return l; 
                });

            var doAll = enumeration.All(t => true);

            Assert.That(fetches, Is.EqualTo(expectedFetches));
        }

        [Test]
        public void TestListOrder()
        {
            var fetches = 0;
            var batchSize = 10;
            var list = CreateList(100);

            var enumeration = new BatchingEnumeration<int, int>(
                list,
                batchSize,
                (l) =>
                {
                    Assert.That(l.First(), Is.EqualTo(batchSize * fetches));
                    fetches++;
                    return l;
                });
        }

        List<int> CreateList(int count)
        {
            var result = new List<int>(count);

            for (int i = 0; i < count; i++)
                result.Add(i);

            return result;
        }

    }
}
