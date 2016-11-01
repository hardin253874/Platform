// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;
using EDC.ReadiNow.Core;
using System.IO;

namespace EDC.SoftwarePlatform.Migration.Test.Export
{
	/// <summary>
	///     Tenant Merge Processor Tests
	/// </summary>
	/// <remarks>
	///     Tests to come
	///     -------------
	///     User lookup changes remain
	/// </remarks>
	[TestFixture]
	[Category( "ExtendedTests" )]
	[Category( "AppLibraryTests" )]
	public class TenantExportImportTests
	{
		/// <summary>
		///     Tests the tear down.
		/// </summary>
		[TearDown]
		public void TestTearDown( )
		{
			TenantHelper.Flush( );
		}

		

		/// <summary>
		///     Preserve user lookup modifications.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void SecureDataRoundTrip( ) 
		{
            var ticks = DateTime.UtcNow.Ticks;
            string oldName = $"TenantExportImportTests Old {ticks}";
            string newName = $"TenantExportImportTests New {ticks}";

            using (new GlobalAdministratorContext())
            {
                long oldTenantId = 0;
                long newTenantId = 0;
                var filePath = Path.GetTempFileName() + ".db";

                try
                {
                    oldTenantId = TestHelper.CreateTenant(oldName, null);
                    AppManager.DeployApp(oldName, Applications.CoreApplicationId.ToString("B"));

                    // Set some custom info

                    var secureDataId = Factory.SecuredData.Create(oldTenantId, "test", "testValue");

                    // Export and Import
                    TenantManager.ExportTenant(oldName, filePath);

                    newTenantId = TenantManager.ImportTenant(filePath, newName);

                    using (new TenantAdministratorContext(newTenantId))
                    {
                        var securedData = Factory.SecuredData.Read(secureDataId);
                        Assert.That(securedData, Is.EqualTo("testValue"));
                    }
                }
                finally
                {
                    if (oldTenantId > 0)
                    {
                        TenantHelper.DeleteTenant(oldTenantId);
                    }

                    if (newTenantId > 0)
                    {
                        TenantHelper.DeleteTenant(newTenantId);
                        File.Delete(filePath);
                    }
                }
            }
		}
	}
}