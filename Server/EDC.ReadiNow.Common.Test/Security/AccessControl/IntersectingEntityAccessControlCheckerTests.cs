// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class IntersectingEntityAccessControlCheckerTests
    {
        [Test]
        public void Test_CheckTypeAccess_EmptyEntityTypes()
        {
            var result = new IntersectingEntityAccessControlChecker(new IEntityAccessControlChecker[0]).CheckTypeAccess( new Collection<EntityType>(), new EntityRef( ), new EntityRef(1));

            Assert.AreEqual(0, result.Count, "The result should be empty");
        }

        [Test]
        public void Test_CheckTypeAccess_NullEntityTypes( )
        {
            Assert.That(
                () =>
                    new IntersectingEntityAccessControlChecker(new IEntityAccessControlChecker[0]).CheckTypeAccess( null, new EntityRef( ), new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        public void Test_CheckTypeAccess_NullUser( )
        {
            Assert.That(
                ( ) =>
                    new IntersectingEntityAccessControlChecker( new IEntityAccessControlChecker [ 0 ] ).CheckTypeAccess( new Collection<EntityType>( ), new EntityRef( ), null ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "user" ) );
        }

        [Test]
        public void Test_CheckTypeAccess_NullPermission( )
        {
            Assert.That(
                () =>
                    new IntersectingEntityAccessControlChecker(new IEntityAccessControlChecker[0]).CheckTypeAccess( new Collection<EntityType>(), null, new EntityRef( ) ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permission"));
        }

        [Test]
        public void Test_CheckAccess_EmptyEntityTypes()
        {
            var result = new IntersectingEntityAccessControlChecker(new IEntityAccessControlChecker[0]).CheckAccess(new Collection<EntityRef>(), new Collection<EntityRef>(),
                new EntityRef(1));

            Assert.AreEqual(0, result.Count, "The result should be empty");
        }

        [Test]
        public void Test_CheckAccess_NullEntities()
        {
            Assert.That(
                () =>
                    new IntersectingEntityAccessControlChecker(new IEntityAccessControlChecker[0]).CheckAccess(null, new Collection<EntityRef>(),
                        new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        public void Test_CheckAccess_NullPermissions()
        {
            Assert.That(
                () =>
                    new IntersectingEntityAccessControlChecker(new IEntityAccessControlChecker[0]).CheckAccess(new Collection<EntityRef>(), null, new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        public void Test_CheckAccess_NullUser()
        {
            Assert.That(
                () =>
                    new IntersectingEntityAccessControlChecker(new IEntityAccessControlChecker[0]).CheckAccess(new Collection<EntityRef>(), new Collection<EntityRef>(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("user"));
        }

        [Test]
        public void Test_Creation_NullCheckers()
        {
            Assert.That(() => new IntersectingEntityAccessControlChecker(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityAccessControlCheckers"));
        }

        [Test]
        [TestCase(false, false, false, false, false, false)]
        [TestCase(false, false, false, true, false, false)]
        [TestCase(false, false, true, false, false, false)]
        [TestCase(false, false, true, true, false, false)]
        [TestCase(false, true, false, false, false, false)]
        [TestCase(false, true, false, true, false, true)]
        [TestCase(false, true, true, false, false, false)]
        [TestCase(false, true, true, true, false, true)]
        [TestCase(true, false, false, false, false, false)]
        [TestCase(true, false, false, true, false, false)]
        [TestCase(true, false, true, false, true, false)]
        [TestCase(true, false, true, true, true, false)]
        [TestCase(true, true, false, false, false, false)]
        [TestCase(true, true, false, true, false, true)]
        [TestCase(true, true, true, false, true, false)]
        [TestCase(true, true, true, true, true, true)]
        [TestCase(null, null, null, null, false, false)]
        [TestCase(null, null, null, true, false, false)]
        [TestCase(null, null, true, null, false, false)]
        [TestCase(null, null, true, true, false, false)]
        [TestCase(null, true, null, null, false, false)]
        [TestCase(null, true, null, true, false, true)]
        [TestCase(null, true, true, null, false, false)]
        [TestCase(null, true, true, true, false, true)]
        [TestCase(true, null, null, null, false, false)]
        [TestCase(true, null, null, true, false, false)]
        [TestCase(true, null, true, null, true, false)]
        [TestCase(true, null, true, true, true, false)]
        [TestCase(true, true, null, null, false, false)]
        [TestCase(true, true, null, true, false, true)]
        [TestCase(true, true, true, null, true, false)]
        public void TestCheckTypeAccessList(
            bool? checker1Entity1, bool? checker1Entity2,
            bool? checker2Entity1, bool? checker2Entity2,
            bool resultEntity1, bool resultEntity2)
        {
            var et1 = new EntityType();
            var et2 = new EntityType();

            var entitiesToCheck = new List<EntityType> {et1, et2};
            var userToCheck = new EntityRef(1);
            var permission = new EntityRef( 2 );

            var result1 = new Dictionary<long, bool>();

            if (checker1Entity1 != null)
            {
                result1[et1.Id] = checker1Entity1.Value;
            }

            if (checker1Entity2 != null)
            {
                result1[et2.Id] = checker1Entity2.Value;
            }

            var result2 = new Dictionary<long, bool>();

            if (checker2Entity1 != null)
            {
                result2[et1.Id] = checker2Entity1.Value;
            }

            if (checker2Entity2 != null)
            {
                result2[et2.Id] = checker2Entity2.Value;
            }

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var checker1 = mockRepo.Create<IEntityAccessControlChecker>();
            checker1.Setup(f => f.CheckTypeAccess(
                It.Is<IList<EntityType>>(e => ReferenceEquals(e, entitiesToCheck)),
                It.Is<EntityRef>( e => Equals( permission, e ) ),
                It.Is<EntityRef>(e => Equals(userToCheck, e)))).Returns(result1);

            var checker2 = mockRepo.Create<IEntityAccessControlChecker>();
            checker2.Setup(f => f.CheckTypeAccess(
                It.Is<IList<EntityType>>(e => ReferenceEquals(e, entitiesToCheck)),
                It.Is<EntityRef>( e => Equals( permission, e ) ),
                It.Is<EntityRef>(e => Equals(userToCheck, e)))).Returns(result2);

            var intersectingChecker = new IntersectingEntityAccessControlChecker(new List<IEntityAccessControlChecker> {checker1.Object, checker2.Object});

            var result = intersectingChecker.CheckTypeAccess(entitiesToCheck, permission, userToCheck);

            Assert.AreEqual(2, result.Count, "The count is invalid");
            Assert.AreEqual(resultEntity1, result[et1.Id], "Access to entity 1 is invalid");
            Assert.AreEqual(resultEntity2, result[et2.Id], "Access to entity 2 is invalid");

            mockRepo.VerifyAll();
        }


        [Test]
        public void TestCanCreateList_ByPassSecurity()
        {
            var et1 = new EntityType();
            var et2 = new EntityType();

            var entitiesToCheck = new List<EntityType> {et1, et2};
            var userToCheck = new EntityRef(1);
            var permission = new EntityRef( 2 );

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var checker1 = mockRepo.Create<IEntityAccessControlChecker>();
            var checker2 = mockRepo.Create<IEntityAccessControlChecker>();

            var intersectingChecker = new IntersectingEntityAccessControlChecker(new List<IEntityAccessControlChecker> {checker1.Object, checker2.Object});

            IDictionary<long, bool> result;

            using (new SecurityBypassContext())
            {
                result = intersectingChecker.CheckTypeAccess(entitiesToCheck, permission, userToCheck );
            }

            Assert.AreEqual(2, result.Count, "The count is invalid");
            Assert.AreEqual(true, result[et1.Id], "Access to entity 1 is invalid");
            Assert.AreEqual(true, result[et2.Id], "Access to entity 2 is invalid");

            mockRepo.VerifyAll();
        }

        [Test]
        [TestCase(false, false, false, false, false, false)]
        [TestCase(false, false, false, true, false, false)]
        [TestCase(false, false, true, false, false, false)]
        [TestCase(false, false, true, true, false, false)]
        [TestCase(false, true, false, false, false, false)]
        [TestCase(false, true, false, true, false, true)]
        [TestCase(false, true, true, false, false, false)]
        [TestCase(false, true, true, true, false, true)]
        [TestCase(true, false, false, false, false, false)]
        [TestCase(true, false, false, true, false, false)]
        [TestCase(true, false, true, false, true, false)]
        [TestCase(true, false, true, true, true, false)]
        [TestCase(true, true, false, false, false, false)]
        [TestCase(true, true, false, true, false, true)]
        [TestCase(true, true, true, false, true, false)]
        [TestCase(true, true, true, true, true, true)]
        [TestCase(null, null, null, null, false, false)]
        [TestCase(null, null, null, true, false, false)]
        [TestCase(null, null, true, null, false, false)]
        [TestCase(null, null, true, true, false, false)]
        [TestCase(null, true, null, null, false, false)]
        [TestCase(null, true, null, true, false, true)]
        [TestCase(null, true, true, null, false, false)]
        [TestCase(null, true, true, true, false, true)]
        [TestCase(true, null, null, null, false, false)]
        [TestCase(true, null, null, true, false, false)]
        [TestCase(true, null, true, null, true, false)]
        [TestCase(true, null, true, true, true, false)]
        [TestCase(true, true, null, null, false, false)]
        [TestCase(true, true, null, true, false, true)]
        [TestCase(true, true, true, null, true, false)]
        public void TestCheckAccess(
            bool? checker1Entity1, bool? checker1Entity2,
            bool? checker2Entity1, bool? checker2Entity2,
            bool resultEntity1, bool resultEntity2)
        {
            var entitiesToCheck = new List<EntityRef> {new EntityRef(100), new EntityRef(200)};
            var permissionsToCheck = new List<EntityRef> {new EntityRef(5)};
            var userToCheck = new EntityRef(1);

            var result1 = new Dictionary<long, bool>();

            if (checker1Entity1 != null)
            {
                result1[100] = checker1Entity1.Value;
            }

            if (checker1Entity2 != null)
            {
                result1[200] = checker1Entity2.Value;
            }

            var result2 = new Dictionary<long, bool>();

            if (checker2Entity1 != null)
            {
                result2[100] = checker2Entity1.Value;
            }
            if (checker2Entity2 != null)
            {
                result2[200] = checker2Entity2.Value;
            }

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var checker1 = mockRepo.Create<IEntityAccessControlChecker>();
            checker1.Setup(f => f.CheckAccess(
                It.Is<IList<EntityRef>>(e => e.SequenceEqual(entitiesToCheck, EntityRefComparer.Instance)),
                It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissionsToCheck, EntityRefComparer.Instance)),
                It.Is<EntityRef>(e => Equals(userToCheck, e)))).Returns(result1);

            var checker2 = mockRepo.Create<IEntityAccessControlChecker>();
            checker2.Setup(f => f.CheckAccess(
                It.Is<IList<EntityRef>>(e => e.SequenceEqual(entitiesToCheck, EntityRefComparer.Instance)),
                It.Is<IList<EntityRef>>(p => p.SequenceEqual(permissionsToCheck, EntityRefComparer.Instance)),
                It.Is<EntityRef>(e => Equals(userToCheck, e)))).Returns(result2);

            var intersectingChecker = new IntersectingEntityAccessControlChecker(new List<IEntityAccessControlChecker> {checker1.Object, checker2.Object});

            var result = intersectingChecker.CheckAccess(entitiesToCheck, permissionsToCheck, userToCheck);

            Assert.AreEqual(2, result.Count, "The count is invalid");
            Assert.AreEqual(resultEntity1, result[100], "Access to entity 1 is invalid");
            Assert.AreEqual(resultEntity2, result[200], "Access to entity 2 is invalid");

            mockRepo.VerifyAll();
        }

        [Test]
        public void TestCheckAccess_ByPassSecurity()
        {
            var entitiesToCheck = new List<EntityRef> {new EntityRef(100), new EntityRef(200)};
            var permissionsToCheck = new List<EntityRef> {new EntityRef(5)};
            var userToCheck = new EntityRef(1);

            var mockRepo = new MockRepository(MockBehavior.Strict);

            var checker1 = mockRepo.Create<IEntityAccessControlChecker>();
            var checker2 = mockRepo.Create<IEntityAccessControlChecker>();

            var intersectingChecker = new IntersectingEntityAccessControlChecker(new List<IEntityAccessControlChecker> {checker1.Object, checker2.Object});
            IDictionary<long, bool> result;

            using (new SecurityBypassContext())
            {
                result = intersectingChecker.CheckAccess(entitiesToCheck, permissionsToCheck, userToCheck);
            }

            Assert.AreEqual(2, result.Count, "The count is invalid");
            Assert.AreEqual(true, result[100], "Access to entity 1 is invalid");
            Assert.AreEqual(true, result[200], "Access to entity 2 is invalid");

            mockRepo.VerifyAll();
        }
    }
}