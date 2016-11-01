// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Processing;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.ReadiNow.CAST.Test
{
    [TestFixture]
    public class ApplicationServiceTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void TestGetApps()
        {
            try
            {
                // Arrange
                var appService = new ApplicationService();

                // Act
                var apps = appService.GetApps().ToList();

                // Assert
                apps.Should().NotBeNull().And.NotBeEmpty();
                apps.Select(a => a.ApplicationVersionId).Should().NotContain(EmptyAppVersionId);

                var n = apps.Count;

                ImportEmptyApp();

                apps = appService.GetApps().ToList();
                apps.Count.Should().Be(n + 1);
                apps.Select(a => a.ApplicationVersionId).Should().Contain(EmptyAppVersionId);

                var appInfo = apps.FirstOrDefault(a => a.ApplicationVersionId == EmptyAppVersionId);
                appInfo.Should().NotBeNull();
                appInfo.Name.Should().Be("Empty App");
                appInfo.PackageVersion.Should().Be("1.0.0.0");
                appInfo.ApplicationId.Should().Be(_emptyAppId);

	            using ( new GlobalAdministratorContext( ) )
	            {
		            Entity.Exists( new EntityRef( appInfo.ApplicationEntityId ) ).Should( ).BeTrue( );
	            }

	            Entity.Get(new EntityRef(appInfo.ApplicationEntityId)).Should().BeNull();
            }
            finally
            {
                DeleteEmptyApp();
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestGetInstalledApps()
        {
            try
            {
                // Arrange
                var appService = new ApplicationService();

                ImportEmptyApp();

                // Act
                var apps = appService.GetInstalledApps(RunAsDefaultTenant.DefaultTenantName).ToList();

                // Assert
                apps.Should().NotBeNull().And.NotBeEmpty();
                apps.Select(a => a.ApplicationVersionId).Should().NotContain(EmptyAppVersionId);

                var n = apps.Count;

                InstallEmptyApp();

                apps = appService.GetInstalledApps(RunAsDefaultTenant.DefaultTenantName).ToList();
                apps.Count.Should().Be(n + 1);
                apps.Select(a => a.ApplicationVersionId).Should().Contain(EmptyAppVersionId);

                var appInfo = apps.FirstOrDefault(a => a.ApplicationVersionId == EmptyAppVersionId);
                appInfo.Should().NotBeNull();
                appInfo.Name.Should().Be("Empty App");
                appInfo.PackageVersion.Should().Be("1.0.0.0");
                appInfo.ApplicationId.Should().Be(_emptyAppId);
                appInfo.SolutionVersion.Should().Be("1.0.0.0");
                appInfo.SolutionEntityId.Should().BeGreaterThan(0);

	            using ( new GlobalAdministratorContext( ) )
	            {
		            Entity.Exists( new EntityRef( appInfo.ApplicationEntityId ) ).Should( ).BeTrue( );
	            }

	            Entity.Get(new EntityRef(appInfo.ApplicationEntityId)).Should().BeNull();
                Entity.Exists(new EntityRef(appInfo.SolutionEntityId)).Should().BeTrue();

                var solution = Entity.Get<Solution>(appInfo.SolutionEntityId);
                solution.Should().NotBeNull();
                solution.Name.Should().Be("Empty App");

                Action a1 = () => appService.GetInstalledApps(null);
                a1.ShouldThrow<ArgumentException>().WithMessage("The specified tenantName parameter is invalid.");

                var notTenantName = Guid.NewGuid().ToString();
                Action a2 = () => appService.GetInstalledApps(notTenantName);
                a2.ShouldThrow<EntityNotFoundException>().WithMessage("Unable to locate Tenant with name '" + notTenantName + "'.");
            }
            finally
            {
                DeleteEmptyApp();
            }
        }
        
        [Test]
        [RunAsDefaultTenant]
        public void TestInstall()
        {
            try
            {
                // Arrange
                var appService = new ApplicationService();

                ImportEmptyApp();

                Entity.GetByName<Solution>("Empty App").Should().BeEmpty();

                // Act
                appService.Install(RunAsDefaultTenant.DefaultTenantName, _emptyAppId);

                // Assert
                var solutions = Entity.GetByName<Solution>("Empty App").ToList();
                solutions.Should().NotBeEmpty();
                solutions.Count.Should().Be(1);

                var solution = solutions.First();
                solution.Should().NotBeNull();
                solution.PackageId.Should().Be(EmptyAppVersionId);

                UninstallEmptyApp();

                Action a1 = () => appService.Install(null, Guid.Empty);
                a1.ShouldThrow<ArgumentException>().WithMessage("Value cannot be null.\r\nParameter name: tenantName");

                var notAppId = Guid.Empty;
                Action a2 = () => appService.Install(RunAsDefaultTenant.DefaultTenantName, notAppId);
                a2.ShouldThrow<ApplicationException>().WithMessage("Could not load package: " + notAppId.ToString("B"));

                var notAppVersion = "notappversion";
                Action a3 = () => appService.Install(RunAsDefaultTenant.DefaultTenantName, _emptyAppId, notAppVersion);
                a3.ShouldThrow<ApplicationException>()
                    .WithMessage("Could not load package: " + _emptyAppId.ToString("B") + " version: " + notAppVersion);
            }
            finally
            {
                DeleteEmptyApp();
            }
        }
        
        [Test]
        [RunAsDefaultTenant]
        public void TestUninstall()
        {
            try
            {
                // Arrange
                var appService = new ApplicationService();

                ImportEmptyApp();
                InstallEmptyApp();

                var solutions = Entity.GetByName<Solution>("Empty App").ToList();
                solutions.Should().NotBeEmpty();
                solutions.Count.Should().Be(1);

                // Act
                appService.Uninstall(RunAsDefaultTenant.DefaultTenantName, _emptyAppId);

                // Assert
                Entity.GetByName<Solution>("Empty App").Should().BeEmpty();

                Action a1 = () => appService.Install(null, Guid.Empty);
                a1.ShouldThrow<ArgumentException>().WithMessage("Value cannot be null.\r\nParameter name: tenantName");

                var notAppId = Guid.Empty;
                Action a2 = () => appService.Install(RunAsDefaultTenant.DefaultTenantName, notAppId);
                a2.ShouldThrow<ApplicationException>().WithMessage("Could not load package: " + notAppId.ToString("B"));
            }
            finally
            {
                DeleteEmptyApp();
            }
        }
        
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var resource = Assembly.GetExecutingAssembly().GetName().Name + "." + EmptyApp;
            var output = GetEmptyAppLocation();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream == null)
                    throw new Exception("failed to extract empty test app");

                using (var fileStream = new FileStream(output, FileMode.Create))
                {
                    stream.CopyTo(fileStream);

                    fileStream.Close();
                }
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            var output = GetEmptyAppLocation();
            if (File.Exists(output))
            {
                try
                {
                    File.Delete(output);
                }
                catch (Exception)
                {
                    // in use. can't do much
                    Console.WriteLine(@"failed to clean up " + output);
                }
            }
        }

        #region Test Helpers

        private void ImportEmptyApp()
        {
            AppManager.ImportAppPackage(GetEmptyAppLocation());
        }

        private void DeleteEmptyApp()
        {
            try
            {
                UninstallEmptyApp();
            }
            catch (Exception)
            {
                // may or may not have been installed
            }

            AppManager.DeleteApp(EmptyAppVersionId);
        }

        private void InstallEmptyApp()
        {
            try
            {
                AppManager.DeployApp(RunAsDefaultTenant.DefaultTenantName, _emptyAppId.ToString("B"), "1.0.0.0");
            }
            finally
            {
                TenantHelper.Invalidate();
            }
        }

        private void UninstallEmptyApp()
        {
            try
            {
                AppManager.RemoveApp(RunAsDefaultTenant.DefaultTenantName, _emptyAppId.ToString("B"));
            }
            finally
            {
                TenantHelper.Invalidate();
            }
        }

        private static string GetEmptyAppLocation()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(dir))
                throw new Exception("failed to derive output path for empty test app");

            return Path.Combine(dir, EmptyApp);
        }

        private static Guid _emptyAppId = new Guid("{c7ef834b-e625-407a-8e83-fadd58f54702}");

        private static readonly Guid EmptyAppVersionId = new Guid("{162f6491-6ee4-40dd-9209-972fffbf74ea}");
        
        private const string EmptyApp = "EmptyApp.db";

        #endregion
    }
}
