// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using NUnit.Framework;
using ReadiNow.Connector.Scheduled;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO.RemoteFileFetcher;
using EDC.ReadiNow.Core;
using Autofac;
using Moq;
using EDC.ReadiNow.Test;
using ReadiNow.Core;
using System.Linq;
using EDC.ReadiNow.IO;

namespace ReadiNow.Connector.Test.Scheduled
{
    [TestFixture]
    [RunAsDefaultTenant]

    public class StartExportJobTest
    {

        [Test]
        [RunWithTransaction]
        [ExpectedException(typeof(ValidationException))]
        public void TestBadProtocolGeneratesRunInfo()
        {
            var url = "ftp://ThisShouldntWork.com/bob.csv";
                
            var job = new StartExportJob();
            var report = Entity.Get<Report>("console:allTypesReport");

            var config = CreateConfig(url, report, ExportFileTypeEnum_Enumeration.ExportFileTypeCsv );

            var mockFetcher = new Mock<IRemoteFileFetcher>(MockBehavior.Strict);
            mockFetcher.Setup(f => f.PutFromTemporaryFile(It.IsNotNull<string>(), url, "username", "password")).Throws(new ConnectionException("dummy"));

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => mockFetcher.Object).As<IRemoteFileFetcher>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                job.Execute(config);
            }

            var logs = config.GetRelationships<TenantLogEntry>("core:secRunLog");
            
            Assert.That(logs.Count(), Is.EqualTo(1));
            var log = logs.First();
            Assert.That(log.LogEntrySeverity_Enum, Is.EqualTo(LogSeverityEnum_Enumeration.ErrorSeverity));
            Assert.That(log.Description, Is.EqualTo("dummy"));

            mockFetcher.VerifyAll();
        }


        [RunWithTransaction]
        [TestCase("ftps://wibble.com/file.csv", ExportFileTypeEnum_Enumeration.ExportFileTypeCsv)]
        [TestCase("sftp://wibble.com/file.xlss", ExportFileTypeEnum_Enumeration.ExportFileTypeExcel)]
        [TestCase("sftp://wibble.com/file.docx", ExportFileTypeEnum_Enumeration.ExportFileTypeWord)]
        public void TestUrl(string url, ExportFileTypeEnum_Enumeration fileType)
        {
            var job = new StartExportJob();
            var report = Entity.Get<Report>("console:allTypesReport");

            var config = CreateConfig(url, report, fileType);

            var mockFetcher = new Mock<IRemoteFileFetcher>(MockBehavior.Strict);
            mockFetcher.Setup(f => f.PutFromTemporaryFile(It.IsNotNull<string>(), url, "username", "password"));

            var mockRunner = new Mock<IAsyncRunner>(MockBehavior.Strict);

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => mockFetcher.Object).As<IRemoteFileFetcher>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                job.Execute(config);
            }

            var logs = config.GetRelationships<TenantLogEntry>("core:secRunLog");

            Assert.That(logs.Count(), Is.EqualTo(1));
            var log = logs.First();
            Assert.That(log.LogEntrySeverity_Enum, Is.EqualTo(LogSeverityEnum_Enumeration.InformationSeverity));
            Assert.That(log.Description, Is.EqualTo("Success"));

            mockFetcher.VerifyAll();
        }



        ScheduledExportConfig CreateConfig(string url, Report report, ExportFileTypeEnum_Enumeration fileType)
        {
            var secureId = Factory.SecuredData.Create(RequestContext.TenantId, "dummy", "password");

            return new ScheduledExportConfig
            {
                SicUrl = url,
                SicUsername = "username",
                SicPassword = null,
                SicPasswordSecureId = secureId,
                SecFileType_Enum = fileType,
                SecReport = report
            };
        }
    }
}
