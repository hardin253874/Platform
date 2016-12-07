// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Utc;
using System.Reflection;
using System.IO;
using EDC.ReadiNow.IO;
using EDC.Security;
using EDC.SoftwarePlatform.Services.ExportData;
using ReadiNow.ExportData;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    [Category("ExtendedTests")]
    [Category("WorkflowTests")]
    public class ExportToTest : TestBase
    {
        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestExportCsv()
        {
            var report = Entity.Get<Report>(new EntityRef("test", "personReport"));
            var name = "testName" + DateTime.UtcNow.Ticks.ToString();
            var description = "testDescription" + DateTime.UtcNow.Ticks.ToString();

            var doc = ExportToImplementation.ExportTo(report, name, description, TimeZoneHelper.SydneyTimeZoneName, ExportFormat.Csv);

            Assert.That(doc, Is.Not.Null);
            Assert.That(doc.Name, Is.EqualTo(name));
            Assert.That(doc.Description, Is.EqualTo(description));
            Assert.That(doc.CurrentDocumentRevision, Is.Not.Null);
            Assert.That(doc.CurrentDocumentRevision.Name, Is.Not.Null);
            Assert.That(doc.CurrentDocumentRevision.FileDataHash, Is.Not.Null);
            using (var stream = FileRepositoryHelper.GetFileDataStreamForEntity(new EntityRef(doc.Id)))
            {
                Assert.That(stream, Is.Not.Null);
                Assert.That(stream.Length, Is.Not.EqualTo(0));
            }
        }
    }
}