// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class AllowAllEntityAccessControlCheckerTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_NullEntities()
        {
            Assert.That(
                () =>
                new AllowAllEntityAccessControlChecker().CheckAccess(null, new Collection<EntityRef>(),
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_EntitiesContainsNull()
        {
            Assert.That(
                () =>
                new AllowAllEntityAccessControlChecker().CheckAccess(new Collection<EntityRef>() { null },
                                                             new Collection<EntityRef>(),
                                                             new EntityRef(RequestContext.GetContext().Identity.Id)),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_EmptyEntities()
        {
            AllowAllEntityAccessControlChecker checker;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;
            IDictionary<long, bool> result;
            
            entities = new EntityRef[] {  };
            permissions = new EntityRef[] { 2 };
            user = new EntityRef(100);

            checker = new AllowAllEntityAccessControlChecker();
            result = checker.CheckAccess(entities, permissions, user);

            Assert.That(result, Is.Empty);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_Entities()
        {
            AllowAllEntityAccessControlChecker checker;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;
            IDictionary<long, bool> result;

            entities = new EntityRef[] { 1, 2, 3, 4, 5 };
            permissions = new EntityRef[] { 2 };
            user = new EntityRef(100);

            checker = new AllowAllEntityAccessControlChecker();
            result = checker.CheckAccess(entities, permissions, user);

            Assert.That(result, Has.Property("Count").EqualTo(entities.Length));
            foreach (EntityRef entityRef in entities)
            {
                Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entityRef.Id).And.Property("Value").True);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckAccess_DuplicateEntities()
        {
            AllowAllEntityAccessControlChecker checker;
            EntityRef[] entities;
            EntityRef[] permissions;
            EntityRef user;
            IDictionary<long, bool> result;

            entities = new EntityRef[] { 1, 1 };
            permissions = new EntityRef[] { 2 };
            user = new EntityRef(100);

            checker = new AllowAllEntityAccessControlChecker();
            result = checker.CheckAccess(entities, permissions, user);

            Assert.That(result, Has.Property("Count").EqualTo(1));
            Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entities[0].Id).And.Property("Value").True);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullEntityTypes()
        {
            Assert.That(
                () =>
                new AllowAllEntityAccessControlChecker().CheckTypeAccess( (IList<EntityType>)null, Permissions.Create, new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_EntityTypesContainsNull( )
        {
            Assert.That(
                () =>
                new AllowAllEntityAccessControlChecker().CheckTypeAccess( new EntityType[]{ null }, Permissions.Create, new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityTypes"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullUser( )
        {
            Assert.That(
                ( ) =>
                new AllowAllEntityAccessControlChecker( ).CheckTypeAccess( new EntityType [ 0 ], Permissions.Create, null ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "user" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess_NullPermission( )
        {
            Assert.That(
                () =>
                new AllowAllEntityAccessControlChecker().CheckTypeAccess( new EntityType[0], null, new EntityRef( ) ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permission"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CheckTypeAccess( )
        {
            Assert.That(
                new AllowAllEntityAccessControlChecker().CheckTypeAccess( new EntityType[]{ new EntityType() }, Permissions.Create, new EntityRef()),
                Has.All.Property("Value").True);
        }
    }
}
