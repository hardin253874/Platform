// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Security;
using ReadiNow.QueryEngine.ReportConverter;
using Moq;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    // Entity model overview:
    //                
    //  +-------+       +--------+      +---------------+                         +------------+ 
    //  | Role  |       |  User  |      |    Report     |                         | EntityType | 
    //  +-------+       +--------+      |               |                         +------------+ 
    //      |               |           +---------------+                               |     
    //      |               |                  ^                                        |     
    //   Inherits -----------                  |                                     Inherits 
    //      |                                  |                                        |
    //      V                                  |                                        V
    //  +-------+                       +---------------+                        +-----------------+ 
    //  |Subject| --- AllowAccess ------|  Access Rule  |--- ControlAccess ----> | SecurableEntity |
    //  +-------+                       +---------------+                        +-----------------+
    //                                         |
    //                                         |                                 +---------------+
    //                                         ------------ PermissionAccess --> |   Permission  |
    //                                                                           +---------------+
    //                          
    // Create uses the relationship only. Read, Modify and Delete use the related report.

    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class RuleRepositoryTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_InvalidSubjectId()
        {
            EntityType testSecurableEntityType;
            IEntityRepository entityRepository = Factory.EntityRepository;

            using (DatabaseContext.GetContext(true))
            {
                testSecurableEntityType = Entity.Create<EntityType>();
                testSecurableEntityType.Save();

                Assert.That( ( ) => new RuleRepository( entityRepository ).GetAccessRules( testSecurableEntityType.Id, Permissions.Read, new List<long> { testSecurableEntityType.Id } ),
                    Throws.ArgumentException.And.Property("ParamName").EqualTo("subjectId"));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_NullSecurableEntityType()
        {
            Role testSubject;
            EntityType testSecurableEntityType;
            AccessRule accessRule;
            SecurableEntity securableEntityType;
            ICollection<AccessRule> result;
            IEntityRepository entityRepository = Factory.EntityRepository;

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType = Entity.Create<EntityType>( ).As<SecurableEntity>( );
                securableEntityType.Save( );

                accessRule = Entity.Create<AccessRule>( );
                accessRule.AccessRuleEnabled = true;
                accessRule.PermissionAccess.Add( Permissions.Read.Entity.As<Permission>( ) );
                accessRule.Save( );

                testSubject = Entity.Create<Role>( );
                testSubject.AllowAccess.Add( accessRule.As<AccessRule>( ) );
                testSubject.Save();

                testSecurableEntityType = Entity.Create<EntityType>();
                testSecurableEntityType.Save();

                result = null;
                Assert.That( ( ) => result = new RuleRepository( entityRepository ).GetAccessRules( testSubject.Id, Permissions.Read, null ),
                    Throws.Nothing );
                Assert.That( result, Is.Not.Null );
                Assert.That( result, Has.Count.EqualTo( 1 ) );
                Assert.That( result.First( ).Id, Is.EqualTo( accessRule.Id ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_EmptySecurableEntityType()
        {
            Role testSubject;
            EntityType testSecurableEntityType;
            ICollection<AccessRule> result;
            IEntityRepository entityRepository = Factory.EntityRepository;

            using (DatabaseContext.GetContext(true))
            {
                testSubject = Entity.Create<Role>();
                testSubject.Save();

                testSecurableEntityType = Entity.Create<EntityType>();
                testSecurableEntityType.Save();

                result = null;
                Assert.That( ( ) => result = new RuleRepository( entityRepository ).GetAccessRules( testSubject.Id, Permissions.Read, new List<long>() ),
                    Throws.Nothing);
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_SubjectWithoutAccessRules()
        {
            QueryRepository queryRepository;
            EntityType testSecurableEntityType;
            Role testSubject;
            ICollection<AccessRule> result;
            IEntityRepository entityRepository = Factory.EntityRepository;

            using (DatabaseContext.GetContext(true))
            {
                testSecurableEntityType = Entity.Create<EntityType>();
                testSecurableEntityType.Save();

                testSubject = Entity.Create<Role>();
                testSubject.Save();

                queryRepository = new QueryRepository();
                result = new RuleRepository( entityRepository ).GetAccessRules( testSubject.Id, Permissions.Read, new [ ] { testSecurableEntityType.Id } );

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_SubjectWithAccessRuleButNoControl()
        {
            QueryRepository queryRepository;
            SecurableEntity securableEntityType;
            AccessRule accessRule;
            Role subject;
            ICollection<AccessRule> result;
            IEntityRepository entityRepository = Factory.EntityRepository;

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType = Entity.Create<EntityType>().As<SecurableEntity>();
                securableEntityType.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = true;
                accessRule.PermissionAccess.Add(Permissions.Read.Entity.As<Permission>());
                accessRule.Save();

                subject = Entity.Create<Role>();
                subject.AllowAccess.Add(accessRule.As<AccessRule>());
                subject.Save();

                queryRepository = new QueryRepository();
                result = new RuleRepository( entityRepository ).GetAccessRules( subject.Id, Permissions.Read, new [ ] { securableEntityType.Id } );

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_SubjectWithAccessRuleButNoPermissions()
        {
            QueryRepository queryRepository;
            SecurableEntity securableEntityType;
            AccessRule accessRule;
            Role subject;
            ICollection<AccessRule> result;
            IEntityRepository entityRepository = Factory.EntityRepository;

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType = Entity.Create<EntityType>().As<SecurableEntity>();
                securableEntityType.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = true;
                accessRule.ControlAccess = securableEntityType;
                accessRule.Save();

                subject = Entity.Create<Role>();
                subject.AllowAccess.Add(accessRule.As<AccessRule>());
                subject.Save();

                queryRepository = new QueryRepository();
                result = new RuleRepository( entityRepository ).GetAccessRules( subject.Id, Permissions.Read, new [ ] { securableEntityType.Id } );

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }
        }

        [Test]
        [TestCase("read")]
        [TestCase("modify")]
        [TestCase("delete")]
        [TestCase(null)]
        [RunAsDefaultTenant]
        public void TestSinglePermissionQuery(string permissionAlias)
        {
            QueryRepository queryRepository;
            SecurableEntity securableEntityType;
            AccessRule accessRule;
            Role subject;
            ICollection<AccessRule> result;
            IEntityRepository entityRepository = Factory.EntityRepository;
            Permission permission;
            Permission permissionToSet;

            using (DatabaseContext.GetContext(true))
            {
                permission = permissionAlias == null ? null : Entity.Get<Permission>(permissionAlias);
                permissionToSet = permission ?? Entity.Get<Permission>( "core:read" );

                securableEntityType = Entity.Create<EntityType>().As<SecurableEntity>();
                securableEntityType.Save();
                
                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = true;
                accessRule.PermissionAccess.Add( permissionToSet );
                accessRule.ControlAccess = securableEntityType;
                accessRule.Save();

                subject = Entity.Create<Role>();
                subject.AllowAccess.Add(accessRule.As<AccessRule>());
                subject.Save();

                queryRepository = new QueryRepository();
                result = new RuleRepository( entityRepository ).GetAccessRules( subject.Id,
                    permission, new[] { securableEntityType.Id });

                Assert.That( result, Has.Count.EqualTo( 1 ) );
                Assert.That( result.First( ).Id, Is.EqualTo( accessRule.Id ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_SubjectWith2SEs2SQs()
        {
            RuleRepository ruleRepository;
            EntityType securableEntityType1;
            EntityType securableEntityType2;
            Role subject;
            AccessRule accessRule1;
            AccessRule accessRule2;
            List<AccessRule> result;
            IEntityRepository entityRepository = Factory.EntityRepository;

            // Test:
            //
            //                           |--------------> Read
            //                           |
            //  Subject -------> Access Rule -----------> SE1
            //           |                         
            //           |-----> Access Rule -----------> SE2
            //                           |
            //                           |--------------> Read

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType1 = Entity.Create<EntityType>();
                securableEntityType1.Save();

                accessRule1 = Entity.Create<AccessRule>();
                accessRule1.AccessRuleEnabled = true;
                accessRule1.PermissionAccess.Add(Permissions.Read.Entity.As<Permission>());
                accessRule1.ControlAccess = securableEntityType1.As<SecurableEntity>();
                accessRule1.Save();

                securableEntityType2 = Entity.Create<EntityType>();
                securableEntityType2.Save();

                accessRule2 = Entity.Create<AccessRule>();
                accessRule2.AccessRuleEnabled = true;
                accessRule2.PermissionAccess.Add(Permissions.Read.Entity.As<Permission>());
                accessRule2.ControlAccess = securableEntityType2.As<SecurableEntity>();
                accessRule2.Save();

                subject = Entity.Create<Role>();
                subject.AllowAccess.Add(accessRule1.As<AccessRule>());
                subject.AllowAccess.Add(accessRule2.As<AccessRule>());
                subject.Save();
                
                ruleRepository = new RuleRepository( entityRepository );
                result = new List<AccessRule>( ruleRepository.GetAccessRules( subject.Id, Permissions.Read,
                    new[] { securableEntityType1.Id, securableEntityType2.Id }));

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Count.EqualTo(2));
            }
        }
    }
}
