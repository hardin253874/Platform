// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security
{
    [TestFixture]
	[RunWithTransaction]
    public class PlatformSecurityExceptionTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_Ctor()
        {
            PlatformSecurityException platformSecurityException;
            string userName;
            EntityRef[] permissions;
            EntityRef[] entities;

            userName = "foo";
            permissions = new []{ new EntityRef("read"), new EntityRef("modify"), new EntityRef("create")};
            entities = new []{new EntityRef("folder"), new EntityRef("report")};

            platformSecurityException = new PlatformSecurityException(userName, permissions, entities);
            Assert.That(platformSecurityException, Has.Property("UserName").EqualTo(userName));
            Assert.That(platformSecurityException, Has.Property("PermissionAliases").EqualTo(permissions.Select(x => x.Alias)));
            Assert.That(platformSecurityException, Has.Property("EntityIds").EqualTo(entities.Select(x => x.Id)));
            Assert.That(platformSecurityException,
                Has.Property("Message").StringMatching("foo does not have view, edit access to Folder \\(\\d+\\), Report \\(\\d+\\). foo cannot create Type \\(\\d+\\) objects. "));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Ctor_NullUser()
        {
            Assert.That(() => new PlatformSecurityException(null, new EntityRef[0], new EntityRef[0]),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("userName"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Ctor_NullPermissions()
        {
            Assert.That(() => new PlatformSecurityException("foo", null, new EntityRef[0]),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Ctor_UnknownPermissions()
        {
            Assert.That(() => new PlatformSecurityException("foo", new [] { new EntityRef("notAPermission") }, new EntityRef[0]),
                Throws.TypeOf<ArgumentException>().And.Property("ParamName").EqualTo("permissions"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Ctor_NullEntities()
        {
            Assert.That(() => new PlatformSecurityException("foo", new EntityRef[0], null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entities"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Serialization()
        {
            PlatformSecurityException platformSecurityException;
            string userName;
            EntityRef[] permissions;
            EntityRef[] entities;
            PlatformSecurityException deserializedPlatformSecurityException;

            userName = "foo";
            permissions = new[] { new EntityRef("core:read"), new EntityRef("core:modify") };
            entities = new[] { new EntityRef(3), new EntityRef(4) };

            platformSecurityException = new PlatformSecurityException(userName, permissions, entities);
            using(MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, platformSecurityException);
                memoryStream.Seek(0, SeekOrigin.Begin);
                deserializedPlatformSecurityException = (PlatformSecurityException) binaryFormatter.Deserialize(memoryStream);
            }

            Assert.That(platformSecurityException, Is.EqualTo(deserializedPlatformSecurityException));
        }

        [Test]
        [TestCase(null, null, null)]
        [TestCase("foo", null, null)]
        [RunAsDefaultTenant]
        public void Test_CreateMessage_Nulls(string userName, IEnumerable<EntityRef> permissions, IEnumerable<EntityRef> entities)
        {
            Assert.That(
                PlatformSecurityException.CreateMessage(null, null, null),
                Is.EqualTo("No permissions requested"));
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("a", "core:read", "", "a does not have view access to . ")]
        [TestCase("a", "core:read", "core:folder", "a does not have view access to Folder \\(\\d+\\). ")]
        [TestCase("a", "core:modify", "core:folder", "a does not have edit access to Folder \\(\\d+\\). ")]
        [TestCase("a", "core:delete", "core:folder", "a does not have delete access to Folder \\(\\d+\\). ")]
        [TestCase("a", "core:create", "core:folder", "a cannot create Type \\(\\d+\\) objects. ")]
        [TestCase("a", "core:read, core:modify", "core:folder", "a does not have view, edit access to Folder \\(\\d+\\). ")]
        [TestCase("a", "core:read, core:modify", "core:folder,core:report", "a does not have view, edit access to Folder \\(\\d+\\), Report \\(\\d+\\). ")]
        [TestCase("a", "core:create, core:read", "core:folder", "a does not have view access to Folder \\(\\d+\\). a cannot create Type \\(\\d+\\) objects. ")]
        public void Test_CreateMessage(string userName, string permissionsList, string entities, string expectedMessage)
        {
            Assert.That(
                PlatformSecurityException.CreateMessage(
                    userName,
                    permissionsList
                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(pa => new EntityRef(pa)),
                    entities
                        .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(pa => new EntityRef(pa))),
                Is.StringMatching(expectedMessage));
        }
    }
}
