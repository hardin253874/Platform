// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    public class UserEntityPermissionTupleTests
    {
        [Test]
        public void Test_Creation()
        {
            UserEntityPermissionTuple userEntityPermissionTuple;
            const long userId = 1;
            const long entityId = 2;
            long[] permissionIds = new long[] {3};

            userEntityPermissionTuple = new UserEntityPermissionTuple(userId, entityId, permissionIds);

            Assert.That(userEntityPermissionTuple, Has.Property("UserId").EqualTo(userId));
            Assert.That(userEntityPermissionTuple, Has.Property("EntityId").EqualTo(entityId));
            Assert.That(userEntityPermissionTuple, Has.Property("PermissionIds").EquivalentTo(permissionIds));
        }

        [Test]
        public void Test_Creation_Null()
        {
            Assert.That( () => new UserEntityPermissionTuple(1, 2, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissionIds"));
        }

        [Test]
        public void Test_Equals_Equal()
        {
            UserEntityPermissionTuple userEntityPermissionTuple1;
            UserEntityPermissionTuple userEntityPermissionTuple2;

            userEntityPermissionTuple1 = new UserEntityPermissionTuple(1, 2, new long[] { 3 });
            userEntityPermissionTuple2 = new UserEntityPermissionTuple(1, 2, new long[] { 3 });

            Assert.That(userEntityPermissionTuple1, Is.EqualTo(userEntityPermissionTuple2));
        }

        [Test]
        public void Test_Equals_DifferentUserId()
        {
            UserEntityPermissionTuple userEntityPermissionTuple1;
            UserEntityPermissionTuple userEntityPermissionTuple2;

            userEntityPermissionTuple1 = new UserEntityPermissionTuple(1, 2, new long[] { 3 });
            userEntityPermissionTuple2 = new UserEntityPermissionTuple(1000, 2, new long[] { 3 });

            Assert.That(userEntityPermissionTuple1, Is.Not.EqualTo(userEntityPermissionTuple2));
        }

        [Test]
        public void Test_Equals_DifferentEntityId()
        {
            UserEntityPermissionTuple userEntityPermissionTuple1;
            UserEntityPermissionTuple userEntityPermissionTuple2;

            userEntityPermissionTuple1 = new UserEntityPermissionTuple(1, 2, new long[] { 3 });
            userEntityPermissionTuple2 = new UserEntityPermissionTuple(1, 2000, new long[] { 3 });

            Assert.That(userEntityPermissionTuple1, Is.Not.EqualTo(userEntityPermissionTuple2));
        }

        [Test]
        public void Test_Equals_DifferentPermissionId()
        {
            UserEntityPermissionTuple userEntityPermissionTuple1;
            UserEntityPermissionTuple userEntityPermissionTuple2;

            userEntityPermissionTuple1 = new UserEntityPermissionTuple(1, 2, new long[] { 3 });
            userEntityPermissionTuple2 = new UserEntityPermissionTuple(1, 2, new long[] { 3000 });

            Assert.That(userEntityPermissionTuple1, Is.Not.EqualTo(userEntityPermissionTuple2));
        }
    }
}
