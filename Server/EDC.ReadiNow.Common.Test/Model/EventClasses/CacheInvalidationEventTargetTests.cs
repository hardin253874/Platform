// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Model.EventClasses;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
    [TestFixture]
	[RunWithTransaction]
    public class CacheInvalidationEventTargetTests
    {
        [Test]
        public void Test_Creation()
        {
            CacheInvalidationEventTarget target;

            target = new CacheInvalidationEventTarget();
            Assert.That(target, 
                Has.Property("Invalidators").InstanceOf<IEnumerable<ICacheInvalidator>>()
                    .And.Property("Invalidators").Not.Null);
        }

        [Test]
        public void Test_Creation_Null()
        {
            Assert.That(() => new CacheInvalidationEventTarget(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("cacheInvalidators"));
        }

        [Test]
        public void Test_Creation_ContainsNull()
        {
            Assert.That(() => new CacheInvalidationEventTarget(new ICacheInvalidator[] { null }),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("cacheInvalidators"));
        }

        [Test]
        public void Test_Creation_SuppliedFactory()
        {
            CacheInvalidationEventTarget target;
            IEnumerable<ICacheInvalidator> cacheInvalidators;

            cacheInvalidators = new List<ICacheInvalidator>();

            target = new CacheInvalidationEventTarget(cacheInvalidators);
            Assert.That(target, Has.Property("Invalidators").EqualTo(cacheInvalidators));
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void Test_OnAfterSave()
        {
            CacheInvalidationEventTarget target;
            MockRepository mockRepository;
            IEnumerable<ICacheInvalidator> cacheInvalidators;
            Mock<ICacheInvalidator> mockCacheInvalidator;
            IEntity[] entities;
            const int numEntities = 3;

            entities = new IEntity[numEntities];
            for (int i = 0; i < numEntities; i++)
            {
                entities[i] = Entity.Create<Resource>().As<Entity>();
                entities[i].Save();
            }

            mockRepository = new MockRepository(MockBehavior.Strict);

            mockCacheInvalidator = mockRepository.Create<ICacheInvalidator>();
            mockCacheInvalidator.SetupGet(ci => ci.Name).Returns("foo");
            mockCacheInvalidator.Setup(ci => ci.OnEntityChange(It.Is<IList<IEntity>>(x => x.SequenceEqual(entities)), InvalidationCause.Save, null));

            cacheInvalidators = new List<ICacheInvalidator>
                {
                    mockCacheInvalidator.Object
                };

            target = new CacheInvalidationEventTarget(cacheInvalidators);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                target.OnAfterSave(entities, new Dictionary<string, object>());
                ctx.CommitTransaction();
            }

            mockRepository.VerifyAll();
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void Test_OnAfterDelete()
        {
            CacheInvalidationEventTarget target;
            MockRepository mockRepository;
            IEnumerable<ICacheInvalidator> cacheInvalidators;
            Mock<ICacheInvalidator> mockCacheInvalidator;
            IEntity[] entities;
            const int numEntities = 3;

            entities = new IEntity[numEntities];
            for (int i = 0; i < numEntities; i++)
            {
                entities[i] = Entity.Create<Resource>().As<Entity>();
                entities[i].Save();
            }

            mockRepository = new MockRepository(MockBehavior.Strict);

            mockCacheInvalidator = mockRepository.Create<ICacheInvalidator>();

            cacheInvalidators = new List<ICacheInvalidator>
                {
                    mockCacheInvalidator.Object
                };

            target = new CacheInvalidationEventTarget(cacheInvalidators);
            target.OnAfterDelete(entities.Select(e => e.Id), null);

            mockRepository.VerifyAll();
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void Test_OnBeforeSave()
        {
            CacheInvalidationEventTarget target;
            MockRepository mockRepository;
            IEnumerable<ICacheInvalidator> cacheInvalidators;
            Mock<ICacheInvalidator> mockCacheInvalidator;
            IEntity[] entities;
            const int numEntities = 3;

            entities = new IEntity[numEntities];
            for (int i = 0; i < numEntities; i++)
            {
                entities[i] = Entity.Create<Resource>().As<Entity>();
                entities[i].Save();
            }

            mockRepository = new MockRepository(MockBehavior.Strict);

            mockCacheInvalidator = mockRepository.Create<ICacheInvalidator>();
            mockCacheInvalidator.SetupGet(ci => ci.Name).Returns("foo");

            cacheInvalidators = new List<ICacheInvalidator>
                {
                    mockCacheInvalidator.Object
                };

            target = new CacheInvalidationEventTarget(cacheInvalidators);
            Assert.That(target.OnBeforeSave(entities, new Dictionary<string, object>()), Is.False,
                "OnBeforeSave returned incorrect value");

            mockRepository.VerifyAll();
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void Test_OnBeforeDelete()
        {
            CacheInvalidationEventTarget target;
            MockRepository mockRepository;
            IEnumerable<ICacheInvalidator> cacheInvalidators;
            Mock<ICacheInvalidator> mockCacheInvalidator;
            IEntity[] entities;
            const int numEntities = 3;

            entities = new IEntity[numEntities];
            for (int i = 0; i < numEntities; i++)
            {
                entities[i] = Entity.Create<Resource>().As<Entity>();
                entities[i].Save();
            }

            mockRepository = new MockRepository(MockBehavior.Strict);

            mockCacheInvalidator = mockRepository.Create<ICacheInvalidator>();
            mockCacheInvalidator.Setup(ci => ci.OnEntityChange(It.Is<IList<IEntity>>(x => x.SequenceEqual(entities)), 
                InvalidationCause.Delete, It.IsAny<Func<long, EntityChanges>>()));

            cacheInvalidators = new List<ICacheInvalidator>
                {
                    mockCacheInvalidator.Object
                };


            target = new CacheInvalidationEventTarget(cacheInvalidators);
            target.OnBeforeDelete(entities, null);

            mockRepository.VerifyAll();
        }
    }
}
