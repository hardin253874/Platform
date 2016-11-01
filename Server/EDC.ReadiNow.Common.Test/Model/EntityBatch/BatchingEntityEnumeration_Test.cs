// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model.EntityBatch;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Model.EntityBatch
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class BatchingEntityEnumeration_Test
    {


        [Test]
        public void Empty()
        {
            var list = new BatchingEntityEnumeration<Resource>(new List<Resource>(), Resource.Name_Field);

            Assert.That(list, Is.Empty);
            Assert.That(list.GetEnumerator().MoveNext(), Is.False);
        }

        [Test]
        public void ListInOrder()
        {
            var originalList = CreateList(100);
            var list = (new BatchingEntityEnumeration<Resource>(originalList, 5, Resource.Name_Field)).ToList();

            for (int i = 0; i< originalList.Count(); i++)
            {
                Assert.That(originalList[i].Id, Is.EqualTo(list[i].Id));
            }
        }

        List<Resource> CreateList(int count)
        {
            var result = new List<Resource>(count);

            for (int i = 0; i < count; i++)
            {
                var r = Entity.Create<Resource>();
                r.Name = i.ToString();
                result.Add( r);
            }

            return result;
        }
    }
}
