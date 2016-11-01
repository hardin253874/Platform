// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Security;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.WebApi.Test.Security
{
    [TestFixture]
    [RunWithTransaction]
    public class AccessRuleDisplayReportFactoryTests
    {
        [Test]
        public void Test_GetDisplayReportForSecurableEntity_Null()
        {
            Assert.That(() => new AccessRuleDisplayReportFactory().GetDisplayReportForSecurableEntity(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("securableEntity"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetDisplayReportForSecurableEntity_NonEntityType()
        {
            Assert.That(() => new AccessRuleDisplayReportFactory().GetDisplayReportForSecurableEntity(Entity.Create<SecurableEntity>()),
                Throws.ArgumentException.And.Property("ParamName").EqualTo("securableEntity"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetDisplayReportForSecurableEntity_EnsureDifferent()
        {
            EntityType entityType;
            IAccessRuleReportFactory accessRuleReportFactory;
            ReadiNow.Model.Report report1;
            ReadiNow.Model.Report report2;

            entityType = new EntityType();
            entityType.Save();

            accessRuleReportFactory = new AccessRuleDisplayReportFactory();
            report1 = accessRuleReportFactory.GetDisplayReportForSecurableEntity(entityType.As<SecurableEntity>());
            report2 = accessRuleReportFactory.GetDisplayReportForSecurableEntity(entityType.As<SecurableEntity>());

            Assert.That(report1, Has.Property("Id").Not.EqualTo(report2.Id));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetDisplayReportForSecurableEntity_ValidSecurityQuery()
        {
            EntityType entityType;
            IAccessRuleReportFactory accessRuleReportFactory;
            ReadiNow.Model.Report report;
            StructuredQuery reportStructuredQuery;

            entityType = new EntityType();
            entityType.Save();

            accessRuleReportFactory = new AccessRuleDisplayReportFactory();
            report = accessRuleReportFactory.GetDisplayReportForSecurableEntity(entityType.As<SecurableEntity>());
            reportStructuredQuery = Factory.NonCachedReportToQueryConverter.Convert(report);

            Assert.That(reportStructuredQuery, Has.Property("SelectColumns").Count.EqualTo(reportStructuredQuery.SelectColumns.Count),
                "Select column mismatch");
            Assert.That(reportStructuredQuery.SelectColumns.Any(sc => sc.Expression is IdExpression),
                Is.EqualTo(reportStructuredQuery.SelectColumns.Any(sc => sc.Expression is IdExpression)), "Id Expression mismatch");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CopyReport_NoResourceKeyViolation()
        {
            SecurableEntity entityType;
            IAccessRuleReportFactory accessRuleReportFactory;
            ReadiNow.Model.Report report;

            report = Entity.Get<ReadiNow.Model.Report>("core:resourceReport");
            entityType = Entity.Get<EntityType>("core:report").As<SecurableEntity>();

            accessRuleReportFactory = new AccessRuleDisplayReportFactory();
            Assert.That(() => accessRuleReportFactory.GetDisplayReportForSecurableEntity(entityType),
                Throws.Nothing);
            Assert.That(() => accessRuleReportFactory.GetDisplayReportForSecurableEntity(entityType),
                Throws.Nothing);
            Assert.That(() => accessRuleReportFactory.GetDisplayReportForSecurableEntity(entityType),
                Throws.Nothing);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CopyReport_CorrectEntityType()
        {
            SecurableEntity entityType;
            ReadiNow.Model.Report report;

            entityType = Entity.Get<SecurableEntity>("core:report");
            report = new AccessRuleDisplayReportFactory().GetDisplayReportForSecurableEntity(entityType);

            Assert.That(report.RootNode.As<ResourceReportNode>(),
                Has.Property("ResourceReportNodeType").Property("Alias").EqualTo(entityType.Alias));
            Assert.That(report,
                Has.Property("ReportUsesDefinition").Property("Alias").EqualTo(entityType.Alias));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CopyReport_CorrectNameDescriptionAndColumnNames()
        {
            SecurableEntity entityType;
            ReadiNow.Model.Report report;

            entityType = Entity.Get<SecurableEntity>("core:report");
            report = new AccessRuleDisplayReportFactory().GetDisplayReportForSecurableEntity(entityType);

            Assert.That(report, Has.Property("Name").EqualTo(entityType.Name));
            Assert.That(report, Has.Property("Description").EqualTo(string.Empty));
            Assert.That(report.ReportColumns.Select(rc => rc.Name),
                Is.EquivalentTo(new [] { "Id", entityType.Name, "Description" }),
                "Incorrect column names");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CopyReport_CorrectNameWhenTypeLacksName()
        {
            SecurableEntity entityType;
            ReadiNow.Model.Report report;

            entityType = new EntityType().As<SecurableEntity>();
            report = new AccessRuleDisplayReportFactory().GetDisplayReportForSecurableEntity(entityType);

            Assert.That(report, Has.Property("Name").EqualTo(AccessRuleDisplayReportFactory.DefaultReportName));
            Assert.That(report, Has.Property("Description").EqualTo(string.Empty));
        }
    }
}
