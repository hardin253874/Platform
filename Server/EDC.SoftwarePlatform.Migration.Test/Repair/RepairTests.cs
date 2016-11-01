// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test.Repair
{
	[TestFixture]
	[Category( "ExtendedTests" )]
	[Category( "AppLibraryTests" )]
	public class RepairTests : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void RepairAppRevertUserChanges( )
		{
			Solution solution = TestHelper.CreateSolution( );
			solution.Save( );

			EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
			entityType.Save( );

			IEntity instance1 = TestHelper.CreateInstance( entityType, inSolution: solution );
			instance1.Save( );

			IEntity instance2 = TestHelper.CreateInstance( entityType, inSolution: solution );
			instance2.Save( );

			var context = new ProcessingContext( );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Guid instance1UpgradeId = Entity.GetUpgradeId( instance1.Id );
			Guid instance2UpgradeId = Entity.GetUpgradeId( instance2.Id );

			long tenantId = TestHelper.CreateTenant( );

			AppManager.DeployApp( TestHelper.DefaultTenantName, "ReadiNow Core", null, context );

			AppManager.DeployApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, "1.0", context );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long instance1Id = Entity.GetIdFromUpgradeId( instance1UpgradeId );
				long instance2Id = Entity.GetIdFromUpgradeId( instance2UpgradeId );

				instance1 = Entity.Get( instance1Id, true );

				instance1.SetField( Resource.Name_Field, "My Custom Instance" );

				instance1.Save( );

				Entity.Delete( instance2Id );

				Assert.IsFalse( Entity.Exists( instance2Id ) );
			}

			AppManager.RepairApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			EntityCache.Instance.Clear( );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long id1 = Entity.GetIdFromUpgradeId( instance1UpgradeId );
				long id2 = Entity.GetIdFromUpgradeId( instance2UpgradeId );

				IEntity entity = Entity.Get( id1 );

				Assert.IsNotNull( entity );

				var name = entity.GetField<string>( Resource.Name_Field );

				Assert.AreEqual( name, TestHelper.DefaultInstanceName );

				Assert.IsTrue( Entity.Exists( id2 ) );
			}
		}
	}
}
