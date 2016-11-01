// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
    [TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class ReportEventTargetTests
    {
        [Test]
        public void TestCanSetDefaultPickerAndDisplayReport()
        {
            using (new SecurityBypassContext())
            {
                var entityType = new EntityType();

                var report1 = new Report
                {
                    Name = "TestReport1" + Guid.NewGuid()
                };
                report1.IsDefaultDisplayReportForTypes.Add(entityType);
                report1.IsDefaultPickerReportForTypes.Add(entityType);
                report1.Save();

                Assert.AreEqual(entityType.DefaultDisplayReport.Id, report1.Id);
                Assert.AreEqual(entityType.DefaultPickerReport.Id, report1.Id);

                var report2 = new Report
                {
                    Name = "TestReport2" + Guid.NewGuid()
                };
                report2.Save();

                report2.IsDefaultDisplayReportForTypes.Add(entityType);
                report2.IsDefaultPickerReportForTypes.Add(entityType);
                report2.Save();

                report1 = Entity.Get<Report>(report1.Id);
                Assert.IsTrue(!report1.IsDefaultDisplayReportForTypes.Any());
                Assert.IsTrue(!report1.IsDefaultPickerReportForTypes.Any());

                entityType = Entity.Get<EntityType>(entityType.Id);
                Assert.AreEqual(entityType.DefaultDisplayReport.Id, report2.Id);
                Assert.AreEqual(entityType.DefaultPickerReport.Id, report2.Id);
            }
        }
    }
}