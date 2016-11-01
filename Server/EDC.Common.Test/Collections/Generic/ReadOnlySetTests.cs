// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using EDC.Collections.Generic;
using NUnit.Framework;

namespace EDC.Test.Collections.Generic
{
    [TestFixture]
    public class ReadOnlySetTests
    {
        [Test]
        public void CtrNullTest()
        {
            Assert.Throws<ArgumentNullException>(()=> new ReadOnlySet<long>(null));
        }


        [Test]
        public void CtrTest()
        {
            Assert.DoesNotThrow(() => new ReadOnlySet<long>(new HashSet<long>()));
        }


        [Test]
        public void AddTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.Throws<InvalidOperationException>(() => roSet.Add(5));

            var iCollection = roSet as ICollection<long>;
            Assert.Throws<InvalidOperationException>(() => iCollection.Add(5));
        }


        [Test]
        public void ClearTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.Throws<InvalidOperationException>(roSet.Clear);
        }


        [Test]
        public void ContainsTest()
        {
            var hs = new HashSet<long> {5, 6, 7};
            var roSet = new ReadOnlySet<long>(hs);

            Assert.DoesNotThrow(() => roSet.Contains(5));
        }


        [Test]
        public void CopyToTest()
        {
            var hs = new HashSet<long> { 5, 6, 7 };
            var roSet = new ReadOnlySet<long>(hs);
            var array = new long[3];

            Assert.DoesNotThrow(() => roSet.CopyTo(array, 0));                       
        }


        [Test]
        public void ExceptWithTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.Throws<InvalidOperationException>(() => roSet.ExceptWith(new List<long>()));
        }


        [Test]
        public void GetEnumeratorTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.DoesNotThrow(() => roSet.GetEnumerator());
            
            Assert.DoesNotThrow(() => ((IEnumerable)roSet).GetEnumerator());
        }


        [Test]
        public void IntersectWithTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.Throws<InvalidOperationException>(() => roSet.IntersectWith(new List<long>()));
        }


        [Test]
        public void IsProperSubsetOfTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.DoesNotThrow(() => roSet.IsProperSubsetOf(new List<long>()));
        }


        [Test]
        public void IsProperSupersetOfTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.DoesNotThrow(() => roSet.IsProperSupersetOf(new List<long>()));
        }


        [Test]
        public void IsSubsetOfTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.DoesNotThrow(() => roSet.IsSubsetOf(new List<long>()));
        }


        [Test]
        public void IsSupersetOfTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.DoesNotThrow(() => roSet.IsSupersetOf(new List<long>()));
        }


        [Test]
        public void OverlapsTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.DoesNotThrow(() => roSet.Overlaps(new List<long>()));
        }
        

        [Test]
        public void RemoveTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.Throws<InvalidOperationException>(() => roSet.Remove(5));
        }


        [Test]
        public void SetEqualsTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.DoesNotThrow(() => roSet.SetEquals(new List<long>()));
        }


        [Test]
        public void SymmetricExceptWithTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.Throws<InvalidOperationException>(() => roSet.SymmetricExceptWith(new List<long>()));
        }


        [Test]
        public void UnionWithTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.Throws<InvalidOperationException>(() => roSet.UnionWith(new List<long>()));
        }

        [Test]
        public void CountTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());
            Assert.DoesNotThrow(() => { var x = roSet.Count; });
        }


        [Test]
        public void IsReadOnlyTest()
        {
            var roSet = new ReadOnlySet<long>(new HashSet<long>());            
            Assert.IsTrue(roSet.IsReadOnly);
        }
    }
}