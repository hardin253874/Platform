// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
    /// <summary>
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class ProtectableTypeEventTargetTests
    {
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestCanModifyResourceFlagSetOnProtectedApp(bool canModifyApplication)
        {
            using (new SecurityBypassContext())
            {
                var solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = canModifyApplication
                };
                solution.Save();

                var entityType = new Definition {InSolution = solution};
                entityType.Save();

                var canModifyProtectedResource = entityType.CanModifyProtectedResource ?? false;

                Assert.AreNotEqual(canModifyApplication, canModifyProtectedResource);
            }
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestCanModifyResourceFlagSetOnProtectedAppClonedResource(bool canModifyApplication)
        {
            using (new SecurityBypassContext())
            {
                var solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = canModifyApplication
                };
                solution.Save();

                var entityType = new Definition { InSolution = solution };
                entityType.Save();

                var clonedEntityType = entityType.Clone(CloneOption.Deep);
                clonedEntityType.Save();

                var canModifyProtectedResource = clonedEntityType.As<ProtectableType>().CanModifyProtectedResource ?? false;

                Assert.AreNotEqual(canModifyApplication, canModifyProtectedResource);
            }
        }
    }
}