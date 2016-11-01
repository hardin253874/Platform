// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Test.Model.AccessControl
{
    [TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]    
    public class ProtectedAppTests
    {
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestCanUpdateModifiableAppProtectedType(bool canUpdate)
        {
            Definition entityType;
            UserAccount userAccount;

            using (new SecurityBypassContext())
            {
                // Create protected app
                var solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = true
                };
                solution.Save();

                // Add protected type to app.
                // As the app is protected the entityType.CanModifyProtectedResource is not set
                // so the entityType will not be modifiable and is considered part of the app
                entityType = new Definition {InSolution = solution};
                entityType.Save();
                
                solution.CanModifyApplication = canUpdate;
                solution.Save();

                userAccount = Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();
            }

            using (new SetUser(userAccount))
            {
                // Modify the type
                entityType.Description = "New Description";

                Assert.That(() => entityType.Save(),
                    canUpdate
                        ? (Constraint) Throws.Nothing
                        : Throws.TypeOf<PlatformSecurityException>());
            }
        }


        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestCanUpdateModifiableAppType_ViaApi(bool canUpdate)
        {
            Definition entityType;
            UserAccount userAccount;
            string solutionName = "TestSolution" + Guid.NewGuid();

            using (new SecurityBypassContext())
            {
                var solution = new Solution
                {
                    Name = solutionName,
                    CanModifyApplication = true
                };
                solution.Save();

                entityType = new Definition {InSolution = solution};
                entityType.Save();

                userAccount = Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();

                var context = RequestContext.GetContext();

                ApplicationAccess.ChangeTenantApplicationCanModify(context.Tenant.Name, solutionName, canUpdate);

                CacheManager.ClearCaches();
            }            

            using (new SetUser(userAccount))
            {
                // Modify the type
                entityType.Description = "New Description";

                Assert.That(() => entityType.Save(),
                    canUpdate
                        ? (Constraint)Throws.Nothing
                        : Throws.TypeOf<PlatformSecurityException>());
            }
        }


        [Test]
        [TestCase(false, "Save")]
        [TestCase(true, "Save")]
        [TestCase(false, "Delete")]
        [TestCase(true, "Delete")]
        public void TestCanUpdateModifiableAppNonProtectedType(bool canUpdate, string action)
        {
            Report report;
            UserAccount userAccount;

            using (new SecurityBypassContext())
            {
                var solution = new Solution
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
                if (action == "Save")
                {
                    // Modify the report
                    report.Description = "New Description";
                    Assert.That(() => report.Save(), Throws.Nothing);
                }                
                else if (action == "Delete")
                {
                    report.Delete();
                }
            }
        }


        [Test]
        [TestCase(false, "Save")]
        [TestCase(true, "Save")]
        [TestCase(false, "Delete")]
        [TestCase(true, "Delete")]
        public void TestCanUpdateProtectedAppProtectedType(bool canUpdate, string action)
        {
            Definition entityType;
            UserAccount userAccount;

            using (new SecurityBypassContext())
            {
                var solution = new Solution
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
                if (action == "Save")
                {
                    // Modify the report
                    entityType.Description = "New Description";
                    Assert.That(() => entityType.Save(), Throws.Nothing);
                }
                else if (action == "Delete")
                {                    
                    Assert.That(() => entityType.Delete(), Throws.Nothing);
                }
            }
        }


        [Test]        
        public void TestSaveFormDefinitionFieldProtectedApp()
        {
            EntityType definition;
            CustomEditForm editForm;
            UserAccount userAccount;

            using (new SecurityBypassContext())
            {
                var solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = true
                };
                solution.Save();

                definition = new EntityType();
                definition.Inherits.Add(UserResource.UserResource_Type);
                definition.InSolution = solution;

                editForm = new CustomEditForm
                {
                    TypeToEditWithForm = definition,
                    InSolution = solution
                };
                editForm.Save();

                // Protect the app
                solution.CanModifyApplication = false;
                solution.Save();

                userAccount = Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();
            }

            using (new SetUser(userAccount))
            {
                // Adding new field to the definition.
                // This should succeed
                var sField = new StringField();
                definition.Fields.Add(sField.As<Field>());
                Entity.Save(new IEntity[] {editForm, definition, sField});

                // Update the new field
                // This should succeed
                sField.Name = Guid.NewGuid().ToString();
                Entity.Save(new IEntity[] { editForm, definition, sField });

                // Remove the field from the definition
                // This should succeed
                sField = Entity.Get<StringField>(sField.Id);
                definition.Fields.Remove(sField.As<Field>());
                Entity.Save(new IEntity[] { editForm, definition, sField });

                // Delete the field
                Entity.Delete(sField.Id);
            }
        }

        [Test]
        public void TestRemoveProtectedFieldFromDefinitionProtectedApp()
        {
            EntityType definition;
            CustomEditForm editForm;
            UserAccount userAccount;
            StringField protectedField;

            using (new SecurityBypassContext())
            {
                var solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = true
                };
                solution.Save();

                definition = new EntityType();
                definition.Inherits.Add(UserResource.UserResource_Type);
                definition.InSolution = solution;

                protectedField = new StringField {InSolution = solution};
                definition.Fields.Add(protectedField.As<Field>());

                editForm = new CustomEditForm
                {
                    TypeToEditWithForm = definition,
                    InSolution = solution
                };
                editForm.Save();

                // Protect the app
                solution.CanModifyApplication = false;
                solution.Save();

                userAccount = Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();
            }

            using (new SetUser(userAccount))
            {
                // Adding new field to the definition.
                // This should fail                
                definition.Fields.Remove(protectedField.As<Field>());
                Assert.That(() => Entity.Save(new IEntity[] {editForm, definition}), Throws.TypeOf<PlatformSecurityException>());
                
                // Delete the field
                Assert.That(() => Entity.Delete(protectedField.Id), Throws.TypeOf<PlatformSecurityException>());
            }
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestCanRemoveProtectedTypeFromProtectedApp(bool canUpdate)
        {
            Definition entityType;
            UserAccount userAccount;

            using (new SecurityBypassContext())
            {
                // Create protected app
                var solution = new Solution
                {
                    Name = "TestSolution" + Guid.NewGuid(),
                    CanModifyApplication = true
                };
                solution.Save();

                // Add protected type to app.
                // As the app is protected the entityType.CanModifyProtectedResource is not set
                // so the entityType will not be modifiable and is considered part of the app
                entityType = new Definition { InSolution = solution };
                entityType.Save();

                solution.CanModifyApplication = canUpdate;
                solution.Save();

                userAccount = Entity.GetByField<UserAccount>(SpecialStrings.TenantAdministratorUser, false, new EntityRef("core", "name")).FirstOrDefault();
            }

            using (new SetUser(userAccount))
            {
                // Modify the type
                entityType.InSolution = null;

                Assert.That(() => entityType.Save(),
                    canUpdate
                        ? (Constraint)Throws.Nothing
                        : Throws.TypeOf<PlatformSecurityException>());
            }
        }
    }
}