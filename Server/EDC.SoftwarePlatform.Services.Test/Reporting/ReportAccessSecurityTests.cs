// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using ReadiNow.Reporting;
using ReadiNow.Reporting.Result;
using ReadiNow.Reporting.Request;
using ReadiNow.Reporting.Definitions;
using NUnit.Framework;
using EntityModel = EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Services.Test.Reporting
{
    [TestFixture]
    public class ReportAccessSecurityTests
    {
        private class SecurityTestData
        {
            public SecurityTestData()
            {
                EntityInstances = new List<EntityModel.IEntity>();
            }


            public EntityModel.EntityType EntityType { get; set; }
            public EntityModel.Report Report { get; set; }
            public List<EntityModel.IEntity> EntityInstances { get; private set; }
            public EntityModel.UserAccount UserAccount { get; set; }
        }


        private SecurityTestData SetupSecurityTest(IEnumerable<Tuple<string, string>> nameAndDescription)
        {
            var testData = new SecurityTestData();

            // Create entity type
            var entityType = EntityModel.Entity.Create<EntityModel.EntityType>();
            entityType.Name = "TestType";
            entityType.Inherits.Add(EntityModel.Entity.Get<EntityModel.EntityType>("core:userResource"));
            entityType.Save();

            testData.EntityType = entityType;

            // Create entity instances
            foreach (var nameDesc in nameAndDescription)
            {
                EntityModel.IEntity entity = EntityModel.Entity.Create(entityType);
                entity.SetField("core:name", nameDesc.Item1);
                entity.SetField("core:description", nameDesc.Item2);

                entity.Save();

                testData.EntityInstances.Add(entity);
            }

            // Create query
            Guid resourceGuid = Guid.NewGuid();
            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = entityType.Id,
                    ExactType = false,
                    NodeId = resourceGuid
                },
                SelectColumns = new List<SelectColumn>()
            };

            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression {NodeId = resourceGuid}
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new ResourceDataColumn
                {
                    NodeId = resourceGuid,
                    FieldId = EntityModel.Entity.GetId("core:name")
                }
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new ResourceDataColumn
                {
                    NodeId = resourceGuid,
                    FieldId = EntityModel.Entity.GetId("core:description")
                }
            });

            // Create report
            var report = ReportToEntityModelHelper.ConvertToEntity( structuredQuery );
            report.Save();
            testData.Report = report;

            // Create user
            var userAccount = new EntityModel.UserAccount();
            userAccount.Save();
            testData.UserAccount = userAccount;

            return testData;
        }


        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void RunReportAllAccess()
        {
            var nameDescList = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Name1", "Description1"),
                new Tuple<string, string>("Name2", "Description2")
            };
            SecurityTestData testData = SetupSecurityTest(nameDescList);

            var reportingInterface = new ReportingInterface();

            // Create an access rule
            new AccessRuleFactory().AddAllowReadQuery(testData.UserAccount.As<EntityModel.Subject>(), testData.EntityType.As<EntityModel.SecurableEntity>(),
                TestQueries.Entities(testData.EntityType).ToReport());

            ReportResult reportResult;
            using (new SetUser(testData.UserAccount))
            {
                reportResult = reportingInterface.RunReport(testData.Report, null);                
            }

            foreach (var nameDesc in nameDescList)
            {
                Assert.AreEqual(1, reportResult.GridData.Count(gd => gd.Values[1].Value == nameDesc.Item1 && gd.Values[2].Value == nameDesc.Item2));
            }
            Assert.AreEqual(nameDescList.Count, reportResult.GridData.Count);
        }


        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void RunReportSomeAccessWithRollup()
        {
            // We will group by the name column but secure by the description column
            // Anything with a D description is allowed
            // This should give 2 groups containing enties the test has access to.            
            var nameDescList = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Name1", "D"),
                new Tuple<string, string>("Name1", "X"),
                new Tuple<string, string>("Name1", "X"),
                new Tuple<string, string>("Name2", "D"),
                new Tuple<string, string>("Name2", "D"),
                new Tuple<string, string>("Name2", "X"),
                new Tuple<string, string>("Name3", "X"),
                new Tuple<string, string>("Name3", "X"),
                new Tuple<string, string>("Name3", "X"),
            };

            // Create the entitues
            SecurityTestData testData = SetupSecurityTest(nameDescList);

            var reportingInterface = new ReportingInterface();

            // Create an access rule only for entities whose description is D
            new AccessRuleFactory().AddAllowReadQuery(testData.UserAccount.As<EntityModel.Subject>(), testData.EntityType.As<EntityModel.SecurableEntity>(),
                TestQueries.EntitiesWithDescription("D", testData.EntityType).ToReport());

            // Get the name column id
            long nameColumnId = testData.Report.ReportColumns.ToList()[1].Id;
            long descriptionColumnId = testData.Report.ReportColumns.ToList()[2].Id;

            // Create rollup and aggregate settings
            // Group by the name column and apply a count to the description column            
            var reportSettings = new ReportSettings
            {
                ReportParameters = new ReportParameters
                {
                    GroupAggregateRules = new ReportMetadataAggregate
                    {
                        IncludeRollup = true,
                        ShowCount = true,
                        ShowGrandTotals = true,
                        ShowSubTotals = true,
                        ShowGroupLabel = true,
                        ShowOptionLabel = true,
                        Groups = new List<Dictionary<long, GroupingDetail>>
                        {
                            new Dictionary<long, GroupingDetail>
                            {
                                {nameColumnId, new GroupingDetail {Style = "groupList"}}
                            }
                        },
                        Aggregates = new Dictionary<long, List<AggregateDetail>>
                        {
                            {descriptionColumnId, new List<AggregateDetail> {new AggregateDetail {Style = "aggCount"}}}
                        }
                    }
                }
            };

            ReportResult reportResult;

            // Run the report for the test user
            using (new SetUser(testData.UserAccount))
            {
                reportResult = reportingInterface.RunReport(testData.Report, reportSettings);
            }

            // Verify the results
            Assert.AreEqual(nameDescList.Count(i => i.Item2 == "D"), reportResult.GridData.Count);
            Assert.IsTrue(reportResult.GridData.All(d => d.Values[2].Value == "D"));

            // Verify the aggregate data
            List<ReportDataAggregate> aggregateData = reportResult.AggregateData;

            // Two groups + total
            Assert.AreEqual(3, aggregateData.Count);

            ReportDataAggregate name1Agg = aggregateData.First(ad => ad.GroupHeadings[0][nameColumnId].Value == "Name1");
            ReportDataAggregate name2Agg = aggregateData.First(ad => ad.GroupHeadings[0][nameColumnId].Value == "Name2");
            ReportDataAggregate totalAgg = aggregateData.First(ad => string.IsNullOrEmpty(ad.GroupHeadings[0][nameColumnId].Value));

            Assert.AreEqual("1", name1Agg.Aggregates[descriptionColumnId][0].Value);
            Assert.AreEqual("2", name2Agg.Aggregates[descriptionColumnId][0].Value);
            Assert.AreEqual("3", totalAgg.Aggregates[descriptionColumnId][0].Value);
        }


        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void RunReportNoAccess()
        {
            SecurityTestData testData = SetupSecurityTest(new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Name1", "Description1"),
                new Tuple<string, string>("Name2", "Description2")
            });

            var reportingInterface = new ReportingInterface();

            using (new SetUser(testData.UserAccount))
            {
                ReportResult reportResult = reportingInterface.RunReport(testData.Report, null);
                Assert.AreEqual(0, reportResult.GridData.Count);
            }
        }


        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void RunReportSomeAccess()
        {
            var nameDescList = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Name1", "Description1"),
                new Tuple<string, string>("Name2", "Description2")
            };
            SecurityTestData testData = SetupSecurityTest(nameDescList);

            var reportingInterface = new ReportingInterface();

            // Create an access rule only for entity with Name1
            new AccessRuleFactory().AddAllowReadQuery(testData.UserAccount.As<EntityModel.Subject>(), testData.EntityType.As<EntityModel.SecurableEntity>(),
                TestQueries.EntitiesWithName(testData.EntityType, "Name1").ToReport());

            ReportResult reportResult;

            using (new SetUser(testData.UserAccount))
            {
                reportResult = reportingInterface.RunReport(testData.Report, null);                
            }

            foreach (var nameDesc in nameDescList)
            {
                Assert.AreEqual(nameDesc.Item1 == "Name1" ? 1 : 0, reportResult.GridData.Count(gd => gd.Values[1].Value == nameDesc.Item1 && gd.Values[2].Value == nameDesc.Item2));
            }
            Assert.AreEqual(1, reportResult.GridData.Count);
        }
    }
}