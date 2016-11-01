// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class SystemEntityAccessControlCheckerTests
    {
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Test_CheckTypeAccess_CreateSingleInheritedType(bool grant)
        {
            var derivedEntityType = new EntityType {Name = "Derived Type " + Guid.NewGuid()};
            derivedEntityType.Save();

            var baseEntityType = new EntityType {Name = "Base Type " + Guid.NewGuid()};
            baseEntityType.DerivedTypes.Add(derivedEntityType);
            baseEntityType.Save();

            var userAccount = Entity.Create<UserAccount>();
            userAccount.Save();

            IDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>> rules = new Dictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>();

            if (grant)
            {
                // Grant access to base type
                rules.Add(new SubjectPermissionTuple(userAccount.Id, Permissions.Create.Id),
                    new List<AccessRuleQuery> {new AccessRuleQuery(100, 101, baseEntityType.Id, new StructuredQuery(), false)});
            }

            var mockRepo = new MockRepository(MockBehavior.Strict);
            var accessQueryFactoryMock = mockRepo.Create<IAccessRuleQueryFactory>();
            accessQueryFactoryMock.Setup(f => f.GetQueries()).Returns(rules);

            var checker = new SystemEntityAccessControlChecker(new UserRoleRepository(), new SystemAccessRuleQueryRepository(accessQueryFactoryMock.Object),
                new EntityTypeRepository());

            // Check derived type

            var derivedEntityTypes = new [ ] { derivedEntityType };
            var baseEntityTypes = new [ ] { baseEntityType };

            Assert.AreEqual(grant, checker.CheckTypeAccess( derivedEntityTypes, Permissions.Create, userAccount)[ derivedEntityType.Id ], "CheckTypeAccess returned incorrect result" );
            Assert.AreEqual(grant, checker.CheckTypeAccess( baseEntityTypes, Permissions.Create, userAccount )[ baseEntityType.Id ], "CheckTypeAccess returned incorrect result" );
        }

        [Test]
        public void Test_CheckTypeAccess_SingleInheritedTypeNoAccess( )
        {
            var derivedEntityType = new EntityType {Name = "Derived Type " + Guid.NewGuid()};
            derivedEntityType.Save();

            var baseEntityType = new EntityType {Name = "Base Type " + Guid.NewGuid()};
            baseEntityType.DerivedTypes.Add(derivedEntityType);
            baseEntityType.Save();

            var userAccount = Entity.Create<UserAccount>();
            userAccount.Save();

            // Grant access to derived type
            IDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>> rules = new Dictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>
            {
                {
                    new SubjectPermissionTuple(userAccount.Id, Permissions.Create.Id),
                    new List<AccessRuleQuery> {new AccessRuleQuery(100, 101, derivedEntityType.Id, new StructuredQuery(), false)}
                }
            };

            var mockRepo = new MockRepository(MockBehavior.Strict);
            var accessQueryFactoryMock = mockRepo.Create<IAccessRuleQueryFactory>();
            accessQueryFactoryMock.Setup(f => f.GetQueries()).Returns(rules);

            var checker = new SystemEntityAccessControlChecker(new UserRoleRepository(), new SystemAccessRuleQueryRepository(accessQueryFactoryMock.Object),
                new EntityTypeRepository());

            // Check derived type      
            var baseEntityTypes = new [ ] { baseEntityType };
            var results = checker.CheckTypeAccess( baseEntityTypes, Permissions.Create, userAccount );
            Assert.AreEqual(false, results[ baseEntityType.Id ], "CheckTypeAccess returned incorrect result" );
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Test_CheckTypeAccess_SingleSpecificType( bool grant)
        {
            var entityType = Entity.Create<EntityType>();
            entityType.Save();

            var userAccount = Entity.Create<UserAccount>();
            userAccount.Save();

            IDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>> rules = new Dictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>();

            if (grant)
            {
                rules.Add(new SubjectPermissionTuple(userAccount.Id, Permissions.Create.Id),
                    new List<AccessRuleQuery> {new AccessRuleQuery(100, 101, entityType.Id, new StructuredQuery(), false)});
            }

            var mockRepo = new MockRepository(MockBehavior.Strict);
            var accessQueryFactoryMock = mockRepo.Create<IAccessRuleQueryFactory>();
            accessQueryFactoryMock.Setup(f => f.GetQueries()).Returns(rules);

            var checker = new SystemEntityAccessControlChecker(new UserRoleRepository(), new SystemAccessRuleQueryRepository(accessQueryFactoryMock.Object),
                new EntityTypeRepository());

            var entityTypes = new [ ] { entityType };
            Assert.AreEqual(grant, checker.CheckTypeAccess( entityTypes, Permissions.Create, userAccount)[ entityType.Id ], "CheckTypeAccess returned incorrect result" );
        }

        [Test]
        public void Test_Creation()
        {
            SystemEntityAccessControlChecker entityAccessControlChecker;

            entityAccessControlChecker = null;
            Assert.That(() => entityAccessControlChecker = new SystemEntityAccessControlChecker(), Throws.Nothing);
            Assert.That(entityAccessControlChecker, Has.Property("RoleRepository").Not.Null);
            Assert.That(entityAccessControlChecker, Has.Property("QueryRepository").Not.Null);
            Assert.That(entityAccessControlChecker, Has.Property("EntityTypeRepository").Not.Null);
        }

        [Test]
        public void Test_Creation_NullEntityTypeRepo()
        {
            Assert.That(() => new SystemEntityAccessControlChecker(new UserRoleRepository(), new SystemAccessRuleQueryRepository(new SystemAccessRuleQueryFactory()), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypeRepository"));
        }

        [Test]
        public void Test_Creation_NullQueryRepo()
        {
            Assert.That(() => new SystemEntityAccessControlChecker(new UserRoleRepository(), null, new EntityTypeRepository()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("queryRepository"));
        }

        [Test]
        public void Test_Creation_NullRoleRepo()
        {
            Assert.That(() => new SystemEntityAccessControlChecker(null, new SystemAccessRuleQueryRepository(new SystemAccessRuleQueryFactory()), new EntityTypeRepository()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("roleRepository"));
        }
    }
}