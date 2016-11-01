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
    public class QueryRepositoryTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_InvalidSubjectId()
        {
            EntityType testSecurableEntityType;

            using (DatabaseContext.GetContext(true))
            {
                testSecurableEntityType = Entity.Create<EntityType>();
                testSecurableEntityType.Save();

                Assert.That(() => new QueryRepository().GetQueries(testSecurableEntityType.Id, Permissions.Read, new[] { testSecurableEntityType.Id }),
                    Throws.ArgumentException.And.Property("ParamName").EqualTo("subjectId"));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_EmptySecurableEntityType()
        {
            Role testSubject;
            EntityType testSecurableEntityType;
            IEnumerable<AccessRuleQuery> result;

            using (DatabaseContext.GetContext(true))
            {
                testSubject = Entity.Create<Role>();
                testSubject.Save();

                testSecurableEntityType = Entity.Create<EntityType>();
                testSecurableEntityType.Save();

                result = null;
                Assert.That(() => result = new QueryRepository().GetQueries(testSubject.Id, Permissions.Read, new long[0]),
                    Throws.Nothing);
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_NullPermission( )
        {
            Role testSubject;
            EntityType testSecurableEntityType;
            IEnumerable<AccessRuleQuery> result;
            AccessRule accessRule;
            Report report;
            QueryRepository queryRepository;

            using ( DatabaseContext.GetContext( true ) )
            {

                testSecurableEntityType = Entity.Create<EntityType>( );
                testSecurableEntityType.Save( );

                report = TestQueries.Entities( ).ToReport( );
                report.Save( );

                accessRule = Entity.Create<AccessRule>( );
                accessRule.AccessRuleEnabled = true;
                accessRule.PermissionAccess.Add( Entity.Get<Permission>(Permissions.Read ) );
                accessRule.ControlAccess = testSecurableEntityType.As<SecurableEntity>( );
                accessRule.AccessRuleReport = report;
                accessRule.Save( );

                testSubject = Entity.Create<Role>( );
                testSubject.AllowAccess.Add( accessRule.As<AccessRule>( ) );
                testSubject.Save( );

                queryRepository = new QueryRepository( );
                result = new List<AccessRuleQuery>( queryRepository.GetQueries( testSubject.Id,
                    null, new [ ] { testSecurableEntityType.Id } ) );

                Assert.That( result, Is.Not.Empty );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_NullSecurableEntityType( )
        {
            Role testSubject;
            EntityType testSecurableEntityType;
            IEnumerable<AccessRuleQuery> result;
            AccessRule accessRule;
            Report report;
            QueryRepository queryRepository;

            using ( DatabaseContext.GetContext( true ) )
            {

                testSecurableEntityType = Entity.Create<EntityType>( );
                testSecurableEntityType.Save( );

                report = TestQueries.Entities( ).ToReport( );
                report.Save( );

                accessRule = Entity.Create<AccessRule>( );
                accessRule.AccessRuleEnabled = true;
                accessRule.PermissionAccess.Add( Entity.Get<Permission>( Permissions.Read ) );
                accessRule.ControlAccess = testSecurableEntityType.As<SecurableEntity>( );
                accessRule.AccessRuleReport = report;
                accessRule.Save( );

                testSubject = Entity.Create<Role>( );
                testSubject.AllowAccess.Add( accessRule.As<AccessRule>( ) );
                testSubject.Save( );

                queryRepository = new QueryRepository( );
                result = new List<AccessRuleQuery>( queryRepository.GetQueries( testSubject.Id,
                    new EntityRef("core:read"), null ) );

                Assert.That( result, Is.Not.Empty );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_SubjectWithoutAccessRules()
        {
            QueryRepository queryRepository;
            EntityType testSecurableEntityType;
            Role testSubject;
            IEnumerable<AccessRuleQuery> result;

            using (DatabaseContext.GetContext(true))
            {
                testSecurableEntityType = Entity.Create<EntityType>();
                testSecurableEntityType.Save();

                testSubject = Entity.Create<Role>();
                testSubject.Save();

                queryRepository = new QueryRepository();
                result = queryRepository.GetQueries(testSubject.Id, Permissions.Read, new[] { testSecurableEntityType.Id });

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
            IEnumerable<AccessRuleQuery> result;
            Report authReport;

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType = Entity.Create<EntityType>().As<SecurableEntity>();
                securableEntityType.Save();

                authReport = Entity.Create<Report>();
                authReport.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = true;
                accessRule.PermissionAccess.Add(Permissions.Read.Entity.As<Permission>());
                accessRule.AccessRuleReport = authReport;
                accessRule.Save();

                subject = Entity.Create<Role>();
                subject.AllowAccess.Add(accessRule.As<AccessRule>());
                subject.Save();

                queryRepository = new QueryRepository();
                result = queryRepository.GetQueries(subject.Id, Permissions.Read, new[] { securableEntityType.Id });

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
            IEnumerable<AccessRuleQuery> result;
            Report authReport;

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType = Entity.Create<EntityType>().As<SecurableEntity>();
                securableEntityType.Save();

                authReport = Entity.Create<Report>();
                authReport.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = true;
                accessRule.ControlAccess = securableEntityType;
                accessRule.AccessRuleReport = authReport;
                accessRule.Save();

                subject = Entity.Create<Role>();
                subject.AllowAccess.Add(accessRule.As<AccessRule>());
                subject.Save();

                queryRepository = new QueryRepository();
                result = queryRepository.GetQueries(subject.Id, Permissions.Read, new[] { securableEntityType.Id });

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }
        }

        [Test]
        [TestCase("read")]
        [TestCase("modify")]
        [TestCase("delete")]
        [RunAsDefaultTenant]
        public void TestSinglePermissionQuery(string permissionAlias)
        {
            QueryRepository queryRepository;
            SecurableEntity securableEntityType;
            AccessRule accessRule;
            Role subject;
            List<AccessRuleQuery> result;
            Permission permission;
            Report report;

            using (DatabaseContext.GetContext(true))
            {
                permission = Entity.Get<Permission>(permissionAlias);

                securableEntityType = Entity.Create<EntityType>().As<SecurableEntity>();
                securableEntityType.Save();

                report = TestQueries.Entities().ToReport();
                report.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = true;
                accessRule.PermissionAccess.Add(permission);
                accessRule.ControlAccess = securableEntityType;
                accessRule.AccessRuleReport = report;
                accessRule.Save();

                subject = Entity.Create<Role>();
                subject.AllowAccess.Add(accessRule.As<AccessRule>());
                subject.Save();

                queryRepository = new QueryRepository();
                result = new List<AccessRuleQuery>(queryRepository.GetQueries(subject.Id,
                    permission, new[] { securableEntityType.Id }));

                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(TestQueries.IsCreatedFrom(result[0].Query, report), Is.True);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_SubjectWith2SEs2SQs()
        {
            QueryRepository queryRepository;
            EntityType securableEntityType1;
            EntityType securableEntityType2;
            Role subject;
            AccessRule accessRule1;
            AccessRule accessRule2;
            List<AccessRuleQuery> result;
            Report authReport1;
            Report authReport2;

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

                authReport1 = TestQueries.Entities().ToReport();
                authReport1.Save();

                accessRule1 = Entity.Create<AccessRule>();
                accessRule1.AccessRuleEnabled = true;
                accessRule1.PermissionAccess.Add(Permissions.Read.Entity.As<Permission>());
                accessRule1.ControlAccess = securableEntityType1.As<SecurableEntity>();
                accessRule1.AccessRuleReport = authReport1;
                accessRule1.Save();

                securableEntityType2 = Entity.Create<EntityType>();
                securableEntityType2.Save();

                authReport2 = TestQueries.Entities().ToReport();
                authReport2.Save();

                accessRule2 = Entity.Create<AccessRule>();
                accessRule2.AccessRuleEnabled = true;
                accessRule2.PermissionAccess.Add(Permissions.Read.Entity.As<Permission>());
                accessRule2.ControlAccess = securableEntityType2.As<SecurableEntity>();
                accessRule2.AccessRuleReport = authReport2;
                accessRule2.Save();

                subject = Entity.Create<Role>();
                subject.AllowAccess.Add(accessRule1.As<AccessRule>());
                subject.AllowAccess.Add(accessRule2.As<AccessRule>());
                subject.Save();

                queryRepository = new QueryRepository();
                result = new List<AccessRuleQuery>(queryRepository.GetQueries(subject.Id, Permissions.Read,
                    new[] { securableEntityType1.Id, securableEntityType2.Id }));

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result.Select(q=>q.Query),
                            Is.EquivalentTo( new [ ] {
                                ReportToQueryConverter.Instance.Convert( authReport1 ),
                                ReportToQueryConverter.Instance.Convert( authReport2 ) } )
                              .Using(new StructuredQueryEqualityComparer()));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetQueries_SubjectWith2SEs2SQsDifferentPermissions()
        {
            QueryRepository queryRepository;
            EntityType securableEntityType1;
            EntityType securableEntityType2;
            Role subject;
            List<StructuredQuery> result;
            Report authReport1;
            Report authReport2;

            // Test:
            //
            //                           |--------------> Read
            //                           |
            //  Subject -------> Access Rule -----------> SE1
            //           |                      
            //           |-----> Access Rule -----------> SE2
            //                           |
            //                           |--------------> Modify

            using (DatabaseContext.GetContext(true))
            {
                securableEntityType1 = Entity.Create<EntityType>();
                securableEntityType1.Save();

                authReport1 = TestQueries.Entities().ToReport();
                authReport1.Save();

                securableEntityType2 = Entity.Create<EntityType>();
                securableEntityType2.Save();

                authReport2 = TestQueries.Entities().ToReport();
                authReport2.Save();

                subject = Entity.Create<Role>();
                subject.Save();

                new AccessRuleFactory().AddAllowReadQuery(subject.As<Subject>(), securableEntityType1.As<SecurableEntity>(), authReport1);
                new AccessRuleFactory().AddAllowModifyQuery(subject.As<Subject>(), securableEntityType1.As<SecurableEntity>(), authReport2);

                queryRepository = new QueryRepository();

                result = new List<StructuredQuery>(queryRepository.GetQueries(subject.Id, Permissions.Read,
                    new[] { securableEntityType1.Id, securableEntityType2.Id }).Select(q=>q.Query));
                Assert.That(result,
                            Is.EquivalentTo( new [ ] { ReportToQueryConverter.Instance.Convert( authReport1 ) } )
                              .Using(new StructuredQueryEqualityComparer()), "Incorrect read queries");

                result = new List<StructuredQuery>(queryRepository.GetQueries(subject.Id, Permissions.Modify,
                    new[] { securableEntityType1.Id, securableEntityType2.Id }).Select(q=>q.Query));
                Assert.That(result,
                            Is.EquivalentTo( new [ ] { ReportToQueryConverter.Instance.Convert( authReport2 ) } )
                              .Using(new StructuredQueryEqualityComparer()), "Incorrect modify queries");

            }
        }
    }
}
