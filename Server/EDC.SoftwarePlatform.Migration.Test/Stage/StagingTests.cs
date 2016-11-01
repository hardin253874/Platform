// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test.Stage
{
	[TestFixture]
	[Category( "ExtendedTests" )]
	[Category( "AppLibraryTests" )]
	public class StagingTests
	{
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void Bug24784( )
		{
			Guid newInstanceId = Guid.NewGuid( );

			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			application.CreateAppEntityRow( application.AppVerUid, newInstanceId );
			application.CreateAppDataRow( TableNames.AppDataNVarChar, application.AppVerUid, newInstanceId, AppDetails.NameUid, "Violation Test" );

			application.SetCardinality( CardinalityEnum_Enumeration.OneToOne );

			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.IsOfTypeUid, newInstanceId, application.FromTypeUid );
			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.InSolutionUid, newInstanceId, application.SolutionUid );
			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.SecurityOwnerUid, newInstanceId, AppDetails.AdministratorUserAccountUid );
			application.CreateAppRelationshipRow( application.AppVerUid, application.RelationshipUid, newInstanceId, application.ToInstanceUid );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			StatisticsReport statisticsReport = AppManager.StageApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsNull( statisticsReport.CardinalityViolations, "Cardinality violations not null in report" );
		}
	}
}