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

    public class StartImportJobTest
    {

        [Test]
        [RunWithTransaction]
        public void TestBadProtocolGeneratesRunInfo()
        {
            var url = "ftp://ThisShouldntWork/bob.csv";
                
            var job = new StartImportJob();

            var config = CreateConfig(url);

            var mockFetcher = new Mock<IRemoteFileFetcher>(MockBehavior.Strict);
            mockFetcher.Setup(f => f.GetToTemporaryFile(url, "username", "password")).Throws(new ConnectionException("dummy"));

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => mockFetcher.Object).As<IRemoteFileFetcher>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                job.Execute(config);
            }

            var runs = config.SicImportConfig.ImportRuns;
            Assert.That(runs.Count(), Is.EqualTo(1));
            Assert.That(runs.First().ImportRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunFailed));
            Assert.That(runs.First().ImportMessages, Contains.Substring("failed"));
            Assert.That(runs.First().ImportMessages, Contains.Substring(url));

            mockFetcher.VerifyAll();


        }


        [RunWithTransaction]
        [TestCase("ftps://wibble/file.csv")]
        [TestCase("sftp://wibble/file.csv")]
        public void TestUrl(string url)
        {
            var job = new StartImportJob();

            var config = CreateConfig(url);

            var mockFetcher = new Mock<IRemoteFileFetcher>(MockBehavior.Strict);
            mockFetcher.Setup(f => f.GetToTemporaryFile(url, "username", "password")).Returns(() => "123");

            var mockRunner = new Mock<IAsyncRunner>(MockBehavior.Strict);
            mockRunner.Setup(r => r.Start(It.IsAny<Action>(), It.IsAny<AsyncRunnerSettings>()));

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => mockFetcher.Object).As<IRemoteFileFetcher>();
                builder.Register(ctx => mockRunner.Object).As<IAsyncRunner>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                job.Execute(config);
            }

            mockFetcher.VerifyAll();
            mockRunner.VerifyAll();

            var runs = config.SicImportConfig.ImportRuns;

            Assert.That(runs.Count, Is.EqualTo(1));
            Assert.That(runs.First().ImportFileId, Is.EqualTo("123"));
        }


        [Test]
        [RunWithTransaction]
        public void ConnectionExceptionGeneratesRunInfo()
        {
            var job = new StartImportJob();

            var url = "ftps://dontcare.com/test.csv";
            var config = CreateConfig(url);

            var mockFetcher = new Mock<IRemoteFileFetcher>(MockBehavior.Strict);
            mockFetcher.Setup(f => f.GetToTemporaryFile(url, "username", "password")).Returns(() =>
            {
                throw new ConnectionException("Dummy");
            });

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => mockFetcher.Object).As<IRemoteFileFetcher>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                
                job.Execute(config);
            }

            mockFetcher.VerifyAll();

            var runs = config.SicImportConfig.ImportRuns;

            Assert.That(runs.Count, Is.EqualTo(1));
            Assert.That(runs.First().ImportMessages, Contains.Substring("Dummy"));
        }

        ScheduledImportConfig CreateConfig(string url)
        {
            var secureId = Factory.SecuredData.Create(RequestContext.TenantId, "dummy", "password");

            return new ScheduledImportConfig
            {
                SicImportConfig = new ImportConfig { },
                SicUrl = url,
                SicUsername = "username",
                SicPassword = null,
                SicPasswordSecureId = secureId
            };
        }
    }
}
