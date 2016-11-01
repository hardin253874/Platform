// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using NUnit.Framework;
using EDC.SoftwarePlatform.Services.ApplicationManager;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Test;

namespace EDC.SoftwarePlatform.Services.Test.Messaging
{
    [TestFixture]
    public class ApplicationManagerTests
    {
        /// <summary>
        ///     Verifies the logic for when a tenant is permitted to deploy an app.
        /// </summary>
        [TestCase( "AppLib=1.0,  Tenant=none, Security=Full", true )]
        [TestCase( "AppLib=1.0,  Tenant=none, Security=Full", true )]
        [TestCase( "AppLib=1.0,  Tenant=none, Security=PerTenant, InstallPerm=True, PublishPerm=True", true )]
        [TestCase( "AppLib=none, Tenant=none, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=none, Security=PerTenant, InstallPerm=False, PublishPerm=False", false )]
        [TestCase( "AppLib=1.0,  Tenant=none, Security=Restricted", false )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=2.0,  Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=2.0, Security=Full", false )]
        public void TestCanDeploy( string scenario, bool expected )
        {
            var service = new AppManagerService( );
            bool actual = CheckIfPossible( scenario, service.CanDeploy );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///     Verifies the logic for when a tenant is permitted to upgrade an app.
        /// </summary>
        [TestCase( "AppLib=2.0,  Tenant=1.0, Security=Full", true )]
        [TestCase( "AppLib=2.0,  Tenant=1.0, Security=Full", true )]
        [TestCase( "AppLib=1.10,  Tenant=1.9, Security=Full", true )]
        [TestCase( "AppLib=1.9,  Tenant=1.10, Security=Full", false )]
        [TestCase( "AppLib=2.0,  Tenant=1.0, Security=PerTenant, InstallPerm=True, PublishPerm=True", true )]
        [TestCase( "AppLib=none, Tenant=none, Security=Full", false )]
        [TestCase( "AppLib=2.0,  Tenant=1.0, Security=PerTenant, InstallPerm=False, PublishPerm=False", false )]
        [TestCase( "AppLib=2.0,  Tenant=1.0, Security=Restricted", true )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=2.0, Security=Full", false )]
        [TestCase( "AppLib=none, Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=none, Security=Full", false )]
        public void TestCanUpgrade( string scenario, bool expected )
        {
            var service = new AppManagerService( );
            bool actual = CheckIfPossible( scenario, service.CanUpgrade );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///     Verifies the logic for when a tenant is permitted to repair an app.
        /// </summary>
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=Full", true )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=PerTenant, InstallPerm=True, PublishPerm=True", true )]
        [TestCase( "AppLib=none, Tenant=none, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=PerTenant, InstallPerm=False, PublishPerm=False", false )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=Restricted", true )]
        [TestCase( "AppLib=2.0,  Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=2.0, Security=Full", false )]
        [TestCase( "AppLib=none, Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=none, Security=Full", false )]
        public void TestCanRepair( string scenario, bool expected )
        {
            var service = new AppManagerService( );
            bool actual = CheckIfPossible( scenario, service.CanRepair );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///     Verifies the logic for when a tenant is permitted to uninstall an app.
        /// </summary>
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=Full", true )]
        [TestCase( "AppLib=none, Tenant=1.0, Security=Full", true )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=PerTenant, InstallPerm=True, PublishPerm=True", true )]
        [TestCase( "AppLib=1.0,  Tenant=none, Security=Full", false )]
        [TestCase( "AppLib=none, Tenant=none, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=PerTenant, InstallPerm=False, PublishPerm=False", false )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=Restricted", false )]
        public void TestCanUninstall( string scenario, bool expected )
        {
            var service = new AppManagerService( );
            bool actual = CheckIfPossible( scenario, service.CanUninstall );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///     Verifies the logic for when a tenant is permitted to export an app.
        /// </summary>
        [TestCase( "AppLib=1.0,  Tenant=none, Security=Full", true )]
        [TestCase( "AppLib=none,  Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=Restricted", false )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=PerTenant, InstallPerm=True, PublishPerm=True", true )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=PerTenant, InstallPerm=True, PublishPerm=False", false )]
        public void TestCanExport( string scenario, bool expected )
        {
            var service = new AppManagerService( );
            bool actual = CheckIfPossible( scenario, service.CanExport );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///     Verifies the logic for when a tenant is permitted to export an app.
        /// </summary>
        [TestCase( "AppLib=none,  Tenant=1.0, Security=Full", true )]
        [TestCase( "AppLib=none,  Tenant=1.0, Security=Restricted", true )]
        [TestCase( "AppLib=none,  Tenant=1.0, Security=PerTenant, InstallPerm=False, PublishPerm=False", true )]
        [TestCase( "AppLib=1.0,  Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=2.0,  Tenant=1.0, Security=Full", false )]
        [TestCase( "AppLib=1.0,  Tenant=2.0, Security=Full", true )]
        [TestCase( "AppLib=1.0,  Tenant=2.0, Security=Restricted", false )]
        [TestCase( "AppLib=1.0,  Tenant=2.0, Security=PerTenant, InstallPerm=True, PublishPerm=False", false )]
        [TestCase( "AppLib=1.0,  Tenant=2.0, Security=PerTenant, InstallPerm=True, PublishPerm=True", true )]
        [TestCase( "AppLib=1.0,  Tenant=none, Security=Full", false )]
        [TestCase( "AppLib=none,  Tenant=none, Security=Full", false )]
        public void TestCanPublish( string scenario, bool expected )
        {
            var service = new AppManagerService( );
            bool actual = CheckIfPossible( scenario, service.CanPublish );
            Assert.AreEqual( expected, actual );
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestGetAvailableApps( )
        {
            var service = new AppManagerService( );
            var result = service.GetAvailableApplications( );
            Assert.That( result, Has.Count.GreaterThan( 0 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestGetInstalledApps( )
        {
            var service = new AppManagerService( );
            var result = service.GetInstalledApplications( );
            Assert.That( result, Has.Count.GreaterThan( 0 ) );
        }


        private bool CheckIfPossible( string scenario, Func<InstalledApplication, AvailableApplication, AppSecurityModel, bool> validationFunction )
        {
            InstalledApplication installedApp = null;
            AvailableApplication availableApp = null;
            AppSecurityModel security = AppSecurityModel.Full;

            // Process scenario settings
            ReadScenarioSetttings( scenario, ref installedApp, ref availableApp, ref security );

            // Run test
            bool res = validationFunction( installedApp, availableApp, security );
            return res;
        }

        private static void ReadScenarioSetttings( string scenario, ref InstalledApplication installedApp, ref AvailableApplication availableApp, ref AppSecurityModel security )
        {
            Guid appId = Guid.NewGuid( );
            string [ ] parts = scenario.Split( ',' );
            foreach ( string part in parts )
            {
                string [ ] pair = part.Trim( ).Split( '=' );
                string key = pair [ 0 ];
                string value = pair [ 1 ];

                switch ( key )
                {
                    case "AppLib":
                        // Pass a version string, or 'none' to indicate that no version is available.
                        if ( value != "none" )
                        {
                            availableApp = new AvailableApplication
                            {
                                ApplicationId = appId,
                                Name = "TestApp",
                                PackageVersion = value
                            };
                        }
                        break;

                    case "Tenant":
                        // Pass a version string, or 'none' to indicate that no version is available.
                        if ( value != "none" )
                        {
                            installedApp = new InstalledApplication
                            {
                                ApplicationId = appId,
                                Name = "TestApp",
                                PackageVersion = value,
                                SolutionVersion = value,
                            };
                        }
                        break;

                    case "Security":
                        security = ( AppSecurityModel ) Enum.Parse( typeof( AppSecurityModel ), value );
                        break;

                    case "InstallPerm":
                        if ( availableApp != null )
                            availableApp.HasInstallPermission = value == "True";
                        break;

                    case "PublishPerm":
                        if ( availableApp != null )
                            availableApp.HasPublishPermission = value == "True";
                        break;
                }
            }
        }
    }
}