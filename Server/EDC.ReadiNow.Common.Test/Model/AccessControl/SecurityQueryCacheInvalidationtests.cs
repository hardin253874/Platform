// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.AccessControl
{
    [TestFixture]
    public class SecurityQueryCacheInvalidationTests
    {
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction(false)]
        [Ignore("Failing until implementation complete.")]
        public void Test_AuthorizationDeletion()
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            Authorization authorization;
            CachingQueryRepository cachingQueryRepository;

            cachingQueryRepository = (CachingQueryRepository) new EntityAccessControlFactory().Caches.First(c => c is CachingQueryRepository);

            userAccount = new UserAccount();
            userAccount.Name = Guid.NewGuid().ToString();
            userAccount.Save();

            entityType = new EntityType();
            entityType.Save();

            entity = Entity.Create(entityType);
            entity.Save();

            authorization = new AccessControlHelper().AddAllowReadQuery(userAccount.As<Subject>(), 
                entityType.As<SecurableEntity>(), TestQueries.GetAllEntitiesReport());

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permission.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType.Id),
                    "Entry initially present in cache");

            using (new SetUser(userAccount))
            {
                Assert.That(() => Entity.Get(entity.Id), Throws.Nothing);
            }

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(1)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permission.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType.Id),
                    "Entry not added to cache");

            authorization.Delete();

            Assert.That(cachingQueryRepository.Cache,
                Has.Exactly(0)
                    .Property("Key").Property("SubjectId").EqualTo(userAccount.Id)
                    .And.Property("Key").Property("PermissionId").EqualTo(Permission.Read.Id)
                    .And.Property("Key").Property("EntityTypes").Contains(entityType.Id),
                    "Entry not removed from cache");
        }
    }
}
