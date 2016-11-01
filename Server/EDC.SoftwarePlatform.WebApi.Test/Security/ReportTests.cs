// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Security
{
    [TestFixture]
    public class ReportTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_SimpleReport()
        {
            UserAccount userAccount = null;
            EntityType reportType;
            EntityType conditionalFormatIcon;
            ReadiNow.Model.Report report;
            HttpWebResponse response;

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                reportType = Entity.Get<EntityType>("core:report");
                conditionalFormatIcon = Entity.Get<EntityType>("core:conditionalFormatIcon");

                report = Entity.Get<ReadiNow.Model.Report>("k:reportsReport");
                Assert.That(report, Is.Not.Null, "Test report not found");
                Assert.That(report, Has.Property("RootNode").Not.Null, "Test report not converted");

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), reportType.As<SecurableEntity>(),
                    TestQueries.Entities().ToReport());

                new AccessRuleFactory().AddAllowReadQuery(userAccount.As<Subject>(), conditionalFormatIcon.As<SecurableEntity>(),
                    TestQueries.Entities().ToReport());

                // Sanity check
                using (new SetUser(userAccount))
                {
                    IDictionary<long, bool> checkResult = Factory.EntityAccessControlService.Check(
                        new[] { new EntityRef(report), },
                        new[] { Permissions.Read });

                    Assert.That(checkResult, Has.Exactly(1).Property("Key").EqualTo(report.Id).And.Property("Value").True);
                }

                // Load the first row of the report
                using (var request = new PlatformHttpRequest(@"data/v1/report/" + report.Id + "?page=0,1", PlatformHttpMethod.Get, userAccount))
                {
                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.OK),
                        "Web service call failed");
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try { userAccount.Delete(); }
                    catch (Exception) { }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_NoAccess()
        {
            UserAccount userAccount = null;
            ReadiNow.Model.Report report = null;
            HttpWebResponse response;

            try
            {
                userAccount = new UserAccount();
                userAccount.Name = "Test user " + Guid.NewGuid();
                userAccount.Save();

                report = new ReadiNow.Model.Report();
                report.Save();

                // Sanity check
                using (new SetUser(userAccount))
                {
                    IDictionary<long, bool> checkResult = Factory.EntityAccessControlService.Check(
                        new[] { new EntityRef(report), },
                        new[] { Permissions.Read });

                    Assert.That(checkResult, Has.Exactly(1).Property("Key").EqualTo(report.Id).And.Property("Value").False);
                }

                // Load a report that the user cannot access
                using (var request = new PlatformHttpRequest(@"data/v1/report/" + report.Id + "?page=0,1", PlatformHttpMethod.Get, userAccount))
                {
                    response = request.GetResponse();

                    Assert.That(response, Has.Property("StatusCode").EqualTo(HttpStatusCode.Forbidden),
                        "Web service call failed");
                }
            }
            finally
            {
                if (userAccount != null)
                {
                    // Will cascade delete and remove the access rule
                    try { userAccount.Delete(); }
                    catch (Exception) { }
                }
                if (report != null)
                {
                    try { report.Delete(); }
                    catch (Exception) { }
                }
            }
        }

    }
}
