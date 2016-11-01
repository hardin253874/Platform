// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model.CacheInvalidation;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    [TestFixture]
    public class CacheContextEntryTests
    {
        [Test]
        public void Test_Ctor()
        {
            CacheContextEntry cacheContextEntry;

            cacheContextEntry = new CacheContextEntry();
            Assert.That(cacheContextEntry, Has.Property("Entities").Empty);
            Assert.That(cacheContextEntry, Has.Property("RelationshipTypes").Empty);
            Assert.That(cacheContextEntry, Has.Property("FieldTypes").Empty);
            Assert.That(cacheContextEntry, Has.Property("EntityInvalidatingRelationshipTypes").Empty);
            Assert.That(cacheContextEntry, Has.Property("EntityTypes").Empty);
        }
    }
}
