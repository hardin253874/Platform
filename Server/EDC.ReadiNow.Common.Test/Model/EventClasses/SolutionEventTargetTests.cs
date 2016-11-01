// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class SolutionEventTargetTests
    {        
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestCanUpdateModifiableAppProtectedType(bool canUpdate)
        {
            Definition entityType;
            UserAccount userAccount;
            Solution solution;

            using (new SecurityBypassContext())
            {
                solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = true
                };
                solution.Save();

                entityType = new Definition { InSolution = solution };
                entityType.Save();

                solution.CanModifyApplication = canUpdate;
                solution.Save();

                userAccount = Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();
            }

            using (new SetUser(userAccount))
            {
                // Remove the type from the solution
                solution.ApplicationContents.Remove(entityType.As<Resource>());

                Assert.That(() => solution.Save(),
                    canUpdate
                        ? (Constraint)Throws.Nothing
                        : Throws.TypeOf<PlatformSecurityException>());
            }
        }


        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestCanUpdateModifiableAppNonProtectedType(bool canUpdate)
        {
            Report report;
            UserAccount userAccount;
            Solution solution;

            using (new SecurityBypassContext())
            {
                solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = canUpdate
                };
                solution.Save();

                report = new Report { InSolution = solution };
                report.Save();

                userAccount = Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();
            }

            using (new SetUser(userAccount))
            {
                // Remove the report from the solution
                solution.ApplicationContents.Remove(report.As<Resource>());

                Assert.That(() => solution.Save(), Throws.Nothing);
            }
        }


        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestCanUpdateProtectedAppProtectedType(bool canUpdate)
        {
            Definition entityType;
            UserAccount userAccount;
            Solution solution;

            using (new SecurityBypassContext())
            {
                solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = false
                };
                solution.Save();

                // As the type is being added to a protected app the CanModifyProtectedResource flag is set to true
                // which will allow modifications even though the app is protected
                entityType = new Definition { InSolution = solution };
                entityType.Save();

                solution.CanModifyApplication = canUpdate;
                solution.Save();

                userAccount = Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();
            }

            using (new SetUser(userAccount))
            {
                // Remove the type from the solution
                // This should always work
                solution.ApplicationContents.Remove(entityType.As<Resource>());

                Assert.That(() => solution.Save(), Throws.Nothing);
            }
        }
    }
}
