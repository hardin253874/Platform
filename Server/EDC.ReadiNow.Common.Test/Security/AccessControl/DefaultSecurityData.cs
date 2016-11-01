// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Expressions;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    public class DefaultSecurityData
    {
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void SetupEntities()
        {
            using (DatabaseContext context = DatabaseContext.GetContext(true))
            {
                using (new SecurityBypassContext())
                {
                    Role administratorRole;
                    EntityType resourceType;
                    AccessRuleFactory accessControlHelper;

                    administratorRole = Entity.Get<Role>(new EntityRef("core", "administratorRole"), true);
                    resourceType = Entity.Get<EntityType>(new EntityRef("core", "resource"));
                    // resourceType = Entity.Get<EntityType>(new EntityRef("core", "securableEntity"));
                    accessControlHelper = new AccessRuleFactory();

                    accessControlHelper.AddAllowCreate(administratorRole.As<Subject>(), resourceType.As<SecurableEntity>());
                    accessControlHelper.AddAllowByQuery(administratorRole.As<Subject>(), resourceType.As<SecurableEntity>(),
                        new[] { Permissions.Read, Permissions.Modify, Permissions.Delete }, TestQueries.Entities().ToReport());

                    context.CommitTransaction();
                }
            }
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void CleanUpEntities()
        {
            using (DatabaseContext context = DatabaseContext.GetContext(true))
            {
                using (new SecurityBypassContext())
                {
                    Role administratorRole;

                    administratorRole = Entity.Get<Role>(new EntityRef("core", "administratorRole"), true);
                    foreach (AccessRule entity in administratorRole.AllowAccess)
                    {
                        Entity.Delete(entity);
                    }

                    context.CommitTransaction();
                }
            }
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void EnableAdministratorAccessAllRule()
        {
            using (new SecurityBypassContext())
            {
                AccessRule adminFullAuthorizationAccessRule;

                adminFullAuthorizationAccessRule = Entity.Get<AccessRule>(new EntityRef("core", "adminFullAuthorization"), true);
                adminFullAuthorizationAccessRule.AccessRuleEnabled = true;
                adminFullAuthorizationAccessRule.Save();
            }                
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void DisableAdministratorAccessAllRule()
        {
            using (new SecurityBypassContext())
            {
                AccessRule adminFullAuthorizationAccessRule;

                adminFullAuthorizationAccessRule = Entity.Get<AccessRule>(new EntityRef("core", "adminFullAuthorization"), true);
                adminFullAuthorizationAccessRule.AccessRuleEnabled = false;
                adminFullAuthorizationAccessRule.Save();
            }
        }

        public string [ ] SelfServeTypeNames =
        {
            "core:selfServeComponent"
        };

        public string[] FullControlTypeNames =
        {
            "console:navigationElement", 
            "core:definition", 
            "core:userResource", 
            "core:accessRule", 
            "core:subject", 
            "core:entityWithArgsAndExits",
            "core:visualController",
            "core:inbox",
            "core:relationship",
            "core:field",
            "core:enumType",
            "core:argumentType",
            "core:fieldGroup",
            "core:schedule",
            "core:wfTrigger",
            "core:wfExpression",
            "core:swimlane",
            "core:transitionStart",
            "core:wfArgumentInstance",
            "core:argumentValue",
            "console:workflowActionMenuItem",
            "console:consoleBehavior",
            "console:actionMenu",
            "core:structureView",
            "core:structureLevel",
            "core:reportExpression",
            "core:reportRollup",
            "core:exitPoint", 
            "core:wfActivity", 
            "core:inboxEmailAction", 
            "core:activityArgument",
            "console:consoleTheme",
            "console:actionMenuItem",
            "core:displayFormat",
            "core:reportColumn",
            "core:reportNode",
            "core:enumValue",
            "core:reportRowGroup",
            "core:reportOrderBy",
            "core:reportCondition",
            "console:fieldRenderControlType",
            "console:controlOnForm",
            "core:reportTemplate",
            "core:documentType",
            "core:documentFolder"
        };
        public string[] ReadModifyTypeNames =
        {
            "core:auditLogSettings",
            "core:passwordPolicy",
            "core:tenantGeneralSettings",
            "core:tenantEmailSetting",
            "core:tenantImageSettings",
            "core:inboxProvider"
        };
        public string[] ReadOnlyTypeNames =
        {
            "core:permission", 
            "core:auditLogEntry",
            "core:securityQueriesPageType",
            "core:securityCustomiseUIPageType",
            "console:navigationConfigButton",
            "core:parameter",
            "console:uiContext",
            "core:resourceViewer",
            "core:stringPattern",
            "core:tenantLogEntry",
            "console:fieldTypeDisplayName"
        };
        public string SelfServeCreateAccessRuleNameTemplate = "core:selfServe{0}CreateOnlyAccessRule";
        public string AdministratorsFullControlAccessRuleNameTemplate = "core:administrators{0}FullControlAccessRule";
        public string AdministratorsReadModifyAccessRuleNameTemplate = "core:administrators{0}ReadModifyAccessRule";
        public string AdministratorsReadOnlyAccessRuleNameTemplate = "core:administrators{0}ReadyOnlyAccessRule";
        public string EveryoneFullControlAccessRuleNameTemplate = "core:everyone{0}FullControlAccessRule";
        public string EveryoneReadModifyControlAccessRuleNameTemplate = "core:everyone{0}ReadModifyAccessRule";
        public string EveryoneReadOnlyAccessRuleNameTemplate = "core:everyone{0}ReadOnlyAccessRule";

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void HobbleAdministratorsRole()
        {
            IAccessRuleFactory accessRuleFactory;
            IAccessRuleReportFactory accessRuleReportFactory;
            Subject administratorsRole;

            accessRuleReportFactory = new AccessRuleDisplayReportFactory();
            accessRuleFactory = new AccessRuleFactory();
            using (DatabaseContext databaseContext = DatabaseContext.GetContext(true))
            using (new SecurityBypassContext())
            {
                administratorsRole = Entity.Get<Subject>("core:administratorRole", true);

                // Allow the test to be rerun by re-enabling the allow all Administrators access rule
                EnableAdministratorAccessAllRule();

                // Create full control access rules
                DeleteAccessRules(FullControlTypeNames, AdministratorsFullControlAccessRuleNameTemplate);
                CreateAccessRules(
                    accessRuleFactory,
                    accessRuleReportFactory,
                    administratorsRole,
                    FullControlTypeNames,
                    new[] {Permissions.Create, Permissions.Read, Permissions.Modify, Permissions.Delete},
                    AdministratorsFullControlAccessRuleNameTemplate);

                // Create read modify access rules
                DeleteAccessRules(ReadModifyTypeNames, AdministratorsReadModifyAccessRuleNameTemplate);
                CreateAccessRules(
                    accessRuleFactory,
                    accessRuleReportFactory,
                    administratorsRole,
                    ReadModifyTypeNames,
                    new[] { Permissions.Read, Permissions.Modify },
                    AdministratorsReadModifyAccessRuleNameTemplate);

                // Create read only access rules
                DeleteAccessRules(ReadOnlyTypeNames, AdministratorsReadOnlyAccessRuleNameTemplate);
                CreateAccessRules(
                    accessRuleFactory,
                    accessRuleReportFactory,
                    administratorsRole,
                    ReadOnlyTypeNames,
                    new[] { Permissions.Read },
                    AdministratorsReadOnlyAccessRuleNameTemplate);

                // Disable the "allow all" administrator access rule
                DisableAdministratorAccessAllRule();

                databaseContext.CommitTransaction();
            }
        }
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void SelfServeReportGrant( )
        {
            IAccessRuleFactory accessRuleFactory;
            IAccessRuleReportFactory accessRuleReportFactory;
            Subject selfServeRole;
            Solution coreDataSolution;

            accessRuleReportFactory = new AccessRuleDisplayReportFactory( );
            accessRuleFactory = new AccessRuleFactory( );
            using ( DatabaseContext databaseContext = DatabaseContext.GetContext( true ) )
            using ( new SecurityBypassContext( ) )
            {
                selfServeRole = Entity.Get<Subject>( "core:selfServeRole", true );
                coreDataSolution = CodeNameResolver.GetInstance( "ReadiNow Core Data", "Application" ).As<Solution>( );

                // Allow the test to be rerun by re-enabling the allow all Administrators access rule
                //EnableAdministratorAccessAllRule( );

                // Create access rules
                //DeleteAccessRules( ReportTypeNames, SelfServeCreateAccessRuleNameTemplate );
                CreateAccessRules(
                    accessRuleFactory,
                    accessRuleReportFactory,
                    selfServeRole,
                    SelfServeTypeNames,
                    new [ ] { Permissions.Create },
                    "core:createSelfServeComponentsAccessRule",
                    coreDataSolution );

                // Disable the "allow all" administrator access rule
                //DisableAdministratorAccessAllRule( );

                databaseContext.CommitTransaction( );
            }
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void HideCoreEveryoneRoles()
        {
            IList<AccessRule> accessRules;
            Solution consoleSolution;

            using (DatabaseContext databaseContext = DatabaseContext.GetContext(true))
            using (new SecurityBypassContext())
            {
                consoleSolution = Entity.Get<Solution>("core:consoleSolution");

                accessRules = Entity.GetInstancesOfType<AccessRule>(false, "accessRuleHidden, allowAccess.{alias}, inSolution.{name}, controlAccess.{name}")
                                    .ToList();
                foreach (AccessRule accessRule in accessRules)
                {
                    if (accessRule.AllowAccessBy.Alias == "core:everyoneRole"
                        && accessRule.InSolution != null && accessRule.InSolution.Name == "ReadiNow Core Data")
                    {
                        AccessRule writeableAccessRule = accessRule.AsWritable<AccessRule>();
                        writeableAccessRule.AccessRuleHidden = true;

						if ( writeableAccessRule.InSolution == consoleSolution )
						{
							writeableAccessRule.InSolution = null;
						}

                        Console.Out.WriteLine("Making access rule on type {0} hidden", writeableAccessRule.ControlAccess.Name);

                        writeableAccessRule.Save();
                    }
                }

                databaseContext.CommitTransaction();
            }
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void SecurityCheckTest()
        {
            UserAccount userAccount;

            const long testEntityId = 21660;

            userAccount = Entity.GetByName<UserAccount>("Erica.Mcknight").First();
            Assert.That(userAccount, Is.Not.Null, "User not found");

            using (new SetUser(Entity.GetByName<UserAccount>("Erica.Mcknight").First()))
            using (new ForceSecurityTraceContext(testEntityId))
            {
                IEnumerable<IEntity> entities = Entity.GetInstancesOfType(new EntityRef(testEntityId)).ToList();
                foreach (IEntity entity in entities)
                {
                    Console.Out.WriteLine("{0} ({1})", entity.GetField("core:name"), entity.Id);
                }

                //new EntityAccessControlFactory().Service.Check(
                //    new EntityRef(testEntityId),
                //    new [] { Permissions.Delete });
            }
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void AddMissingAccessRules()
        {
            IAccessRuleFactory accessRuleFactory;
            IAccessRuleReportFactory accessRuleReportFactory;
            Subject administratorsRole;
            Subject everyoneRole;
            Solution coreDataSolution;

            accessRuleReportFactory = new AccessRuleDisplayReportFactory();
            accessRuleFactory = new AccessRuleFactory();
            using (DatabaseContext databaseContext = DatabaseContext.GetContext(true))
            using (new SecurityBypassContext())
            {
                administratorsRole = Entity.Get<Subject>("core:administratorRole", true);
                everyoneRole = Entity.Get<Subject>("core:everyoneRole", true);
                coreDataSolution = CodeNameResolver.GetInstance("ReadiNow Core Data", "Application").As<Solution>();

                Console.WriteLine("Create access rule in solution {0}", coreDataSolution.Id);

                CreateAccessRules(
                    accessRuleFactory,
                    accessRuleReportFactory,
                    administratorsRole,
                    new [ ] { "core:importConfig" },
                    new [ ] { Permissions.Create, Permissions.Read, Permissions.Modify, Permissions.Delete },
                    AdministratorsFullControlAccessRuleNameTemplate,
                    coreDataSolution );

                CreateAccessRules(
                    accessRuleFactory,
                    accessRuleReportFactory,
                    administratorsRole,
                    new [ ] { "core:importRun" },
                    new [ ] { Permissions.Read, Permissions.Modify },
                    AdministratorsReadModifyAccessRuleNameTemplate,
                    coreDataSolution );

                //var types = new[] { "core:board", "core:boardDimension" };
                //DeleteAccessRules(types, EveryoneReadModifyControlAccessRuleNameTemplate);
                //CreateAccessRules(
                //    accessRuleFactory,
                //    accessRuleReportFactory,
                //    everyoneRole,
                //    types,
                //    new[] { Permissions.Create, Permissions.Read, Permissions.Modify },
                //    EveryoneReadModifyControlAccessRuleNameTemplate,
                //    coreDataSolution);

                databaseContext.CommitTransaction();
            }
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void UnhobbleAdministratorsRole()
        {
            using (DatabaseContext databaseContext = DatabaseContext.GetContext(true))
            using (new SecurityBypassContext())
            {
                EnableAdministratorAccessAllRule();
                DeleteAccessRules(FullControlTypeNames, AdministratorsFullControlAccessRuleNameTemplate);
                DeleteAccessRules(ReadModifyTypeNames, AdministratorsReadModifyAccessRuleNameTemplate);
                DeleteAccessRules(ReadOnlyTypeNames, AdministratorsReadOnlyAccessRuleNameTemplate);

                databaseContext.CommitTransaction();
            }
        }

        private void CreateAccessRules(IAccessRuleFactory accessRuleFactory, IAccessRuleReportFactory accessRuleReportFactory, 
            Subject subject, IList<string> typeNames, IList<EntityRef> permissions, string aliasTemplate,
            Solution solution = null)
        {
            AccessRule accessRule;

            foreach (string typeName in typeNames)
            {
                SecurableEntity targetType = Entity.Get<SecurableEntity>(typeName);
                Assert.That(targetType, Is.Not.Null,
                    string.Format("Type {0} does not exist", typeName));

                accessRule = accessRuleFactory.AddAllowByQuery(
                    subject,
                    targetType,
                    permissions,
                    accessRuleReportFactory.GetDisplayReportForSecurableEntity(targetType));
                accessRule = accessRule.AsWritable<AccessRule>();
                accessRule.Alias = string.Format(aliasTemplate, new EntityRef(typeName).Alias);
                accessRule.AccessRuleHidden = true;
                if (solution != null)
                    accessRule.InSolution = solution;

                accessRule.Save();

                Console.WriteLine("Create access rule {0} {1}", accessRule.Alias, accessRule.Id);

            }
        }

        private void DeleteAccessRules(IList<string> typeNames, string aliasTemplate)
        {
            foreach (EntityRef entityToDelete in typeNames.Select(
                tn => new EntityRef(string.Format(aliasTemplate, new EntityRef(tn).Alias))))
            {
                if (Entity.Exists(entityToDelete))
                {
                    Entity.Delete(entityToDelete);
                }
            }            
        }


        /// <summary>
        /// Create a super administrator account.
        /// </summary>
        /// <returns>
        /// The new <see cref="UserAccount"/>.
        /// </returns>
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void CreateSuperAdminAccount()
        {
            UserAccount superAdministratorUserAccount;
            IAccessRuleFactory accessRuleFactory;

            superAdministratorUserAccount = new UserAccount
            {
                Name = SpecialStrings.SystemAdministratorUser,
                AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Disabled
            };
            superAdministratorUserAccount.Save();

            accessRuleFactory = new AccessRuleFactory();
            accessRuleFactory.AddAllowByQuery(
                superAdministratorUserAccount.As<Subject>(),
                Entity.Get<SecurableEntity>("core:resource"),
                new[] { Permissions.Create, Permissions.Read, Permissions.Modify, Permissions.Delete },
                TestQueries.Entities().ToReport()
                );
        }

        [TestCase("Everyone accessing Resource", "Allow Access to Resources by Role", "Allow Access to Resources by Role", "core:accessRuleResourcesByRole", true)]
        [TestCase( null, "Self (Person)", "Allow read to own person record", "core:accessRuleReadOwnPerson", false )]
        [TestCase( null, "Self (User Account)", "Allow read to own user account", "core:accessRuleReadOwnUserAccount", false )]
        [TestCase( "'Everyone' accessing 'Resource'", "Resource for Assigned Task", "Resource for Assigned Task", "core:accessRuleResourceForAssignedTask", true )]
        [Explicit]
        [RunAsDefaultTenant]
        public void FixCoreRules( string accessRuleName, string reportName, string newAccessRuleName, string newAlias, bool markAsNonReport )
        {
            // Note: you'll need to manually disable .ReadOnly checks (eg in code)

            AccessRule rule;
            //if ( accessRuleName == null )
            //{
            Report report = Entity.GetByName<Report>( reportName ).Where( r => r.ReportForAccessRule != null && ( r.ReportForAccessRule.Name == accessRuleName || r.ReportForAccessRule.Name == newAccessRuleName ) ).Single( );
                rule = report.ReportForAccessRule.AsWritable<AccessRule>();
            //}
            //else
            //{
            //    rule = Entity.GetByName<AccessRule>( accessRuleName ).SingleOrDefault( );
            //    if ( rule == null )
            //        throw new Exception( "Rule not found" );
            //}

            rule.Name = newAccessRuleName;
            if ( newAlias != null )
            {
                rule.Alias = newAlias;
            }
            if ( markAsNonReport )
            {
                rule.AccessRuleIgnoreForReports = true;
                Assert.That( rule.AccessRuleIgnoreForReports, Is.True );
            }
            long id = rule.Id;
            rule.Save( );
        }
    }
}

