// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test.Deploy
{
	/// <summary>
	///     Deployment tests.
	/// </summary>
	[TestFixture]
	[Category( "ExtendedTests" )]
	[Category( "AppLibraryTests" )]
	public class DeployTests : TestBase
	{
		/// <summary>
		///     Deletes the multi referenced entity.
		/// </summary>
		/// <remarks>
		///     1. Create a solution
		///     2. Create a type
		///     3. Create an instance of the type
		///     4. Add the instance to the solution
		///     5. Publish the application
		///     6. Create a second solution
		///     7. Add the instance to the second solution
		///     8. Publish the second solution
		///     9. Deploy core to the new tenant
		///     10. Deploy the first solution to the tenant
		///     11. Deploy the second solution to the tenant
		///     12. Remove the instance from the second solution
		///     13. Republish the second solution
		///     14. Upgrade the tenant to the latest version of the second solution
		///     15. Ensure the instance did not get deleted
		/// </remarks>
		[Test]
		[RunAsDefaultTenant]
		[Ignore( "No longer relevant due to 'inSolution' relationship now manyToOne cardinality" )]
		public void DeleteMultiReferencedEntity( )
		{
			/////
			// Create a solution (solution1)
			/////
			Solution solution1 = TestHelper.CreateSolution( );
			solution1.Save( );

			/////
			// Create a type (adding it to solution1)
			/////
			EntityType entityType = TestHelper.CreateEntityType( "Type", "Type Desc", null, solution1 );
			entityType.Save( );

			/////
			// Create an instance (adding it to solution1)
			/////
			IEntity instance = TestHelper.CreateInstance( entityType, "Test Instance", "Test Instance Desc.", solution1 );
			instance.Save( );

			/////
			// Publish solution1 (version 1.0).
			/////
			var context = new ProcessingContext( );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			/////
			// Create a second solution (solution2).
			/////
			Solution solution2 = TestHelper.CreateSolution( "Migration Testing Solution 2" );
			solution2.SolutionVersionString = "1.0";
			solution2.Save( );

			/////
			// Ensure the instance is in both solutions.
			/////
			instance.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
			{
				solution1,
				solution2
			} );
			instance.Save( );

			Guid instanceUpgradeId = Entity.GetUpgradeId( instance.Id );

			/////
			// Publish solution2 (version 1.0).
			/////
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "Migration Testing Solution 2", context );

			long tenantId;

			/////
			// Create a new tenant.
			/////
			using ( new GlobalAdministratorContext( ) )
			{
				var tenant = Entity.Create<Tenant>( );
				tenant.Name = "Test Tenant";
				tenant.Save( );

				tenantId = tenant.Id;
			}

			/////
			// Deploy ReadiNow Core to the new tenant.
			/////
			AppManager.DeployApp( "Test Tenant", "ReadiNow Core", null, context );

			/////
			// Deploy solution1 (1.0) to the new tenant.
			/////
			AppManager.DeployApp( "Test Tenant", TestHelper.DefaultSolutionName, null, context );

			/////
			// Deploy solution2 (1.0) to the new tenant.
			/////
			AppManager.DeployApp( "Test Tenant", "Migration Testing Solution 2", null, context );

			/////
			// Remove the instance from solution2.
			/////
			instance.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
			{
				solution1
			} );
			instance.Save( );

			solution2.SolutionVersionString = "1.1";
			solution2.Save( );

			/////
			// Publish solution2 (version 1.1).
			// This should have removed the instance entity and all relationships pointing to/from it.
			/////
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "Migration Testing Solution 2", context );

			/////
			// Upgrade the new tenant from solution2 (1.0) to solution2 (1.1).
			// All relationships pointing to the instance from solution2 should have been removed but the instance itself should not have.
			// BUG: Should relationships be ref-counted so that the instance still resides in solution1 after upgrading solution2?
			/////
			AppManager.UpgradeApp( "Test Tenant", "Migration Testing Solution 2", null, context );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long id = Entity.GetIdFromUpgradeId( instanceUpgradeId );

				IEntity entity = Entity.Get( id );

				Assert.IsNotNull( entity );
			}
		}

		/// <summary>
		///     Deletes the multi referenced entity from all solution.
		/// </summary>
		/// <remarks>
		///     1. Create a solution
		///     2. Create a type
		///     3. Create an instance of the type
		///     4. Publish the application
		///     5. Create a second solution
		///     6. Place the instance in the second solution
		///     7. Publish the second solution
		///     8. Deploy core to the new tenant
		///     9. Deploy the first solution to the tenant
		///     10. Deploy the second solution to the tenant
		///     11. Remove the instance from the second solution
		///     12. Republish the second solution
		///     13. Upgrade the tenant to the latest version of the second solution
		///     14. Ensure the instance got deleted
		/// </remarks>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteMultiReferencedEntityFromAllSolution( )
		{
			/////
			// Create a solution (solution1).
			/////
			Solution solution1 = TestHelper.CreateSolution( );
			solution1.Save( );

			/////
			// Create a type (adding it to solution1)
			/////
			EntityType entityType = TestHelper.CreateEntityType( inSolution: solution1 );
			entityType.Save( );

			/////
			// Create an instance.
			/////
			IEntity instance = TestHelper.CreateInstance( entityType );
			instance.Save( );

			/////
			// Publish solution1 (1.0).
			/////
			var context = new ProcessingContext( );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			/////
			// Create a new solution (solution2).
			/////
			Solution solution2 = TestHelper.CreateSolution( "Migration Testing Solution 2" );
			solution2.SolutionVersionString = "1.0";
			solution2.Save( );

			/////
			// Set the instance to only be in solution2.
			/////
			instance.SetRelationships( "core:inSolution", new EntityRelationshipCollection<IEntity>
			{
				solution2
			} );

			instance.Save( );

			Guid instanceUpgradeId = Entity.GetUpgradeId( instance.Id );

			/////
			// Publish solution2 (1.0).
			/////
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "Migration Testing Solution 2", context );

			/////
			// Create a new tenant
			/////
			long tenantId = TestHelper.CreateTenant( );

			/////
			// Deploy ReadiNow Core to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, "ReadiNow Core", null, context );

			/////
			// Deploy solution1 (1.0) to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, null, context );

			/////
			// Deploy solution2 (1.0) to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, "Migration Testing Solution 2", null, context );

			/////
			// Remove the instance from all solutions.
			/////
			instance.Delete( );

			solution2.SolutionVersionString = "1.1";
			solution2.Save( );

			/////
			// Publish solution 2 (1.1).
			// At this point, the instance should no longer be part of solution2.
			/////
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "Migration Testing Solution 2", context );

			/////
			// Upgrade the new tenant from solution2 (1.0) to solution2 (1.1).
			// The instance should be removed from the system since there is no external reference.
			/////
			AppManager.UpgradeApp( TestHelper.DefaultTenantName, "Migration Testing Solution 2", null, context );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long id = Entity.GetIdFromUpgradeId( instanceUpgradeId );

				IEntity entity = Entity.Get( id );

				Assert.IsNull( entity );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestWorkItem22022( )
		{
			/////
			// Create a solution (solution1).
			/////
			Solution solution = TestHelper.CreateSolution( );
			solution.Save( );

			/////
			// Create a base type (adding it to solution1)
			/////
			EntityType baseType1 = TestHelper.CreateEntityType( "testBase1", "test base 1", null, solution);
			baseType1.Save( );

			/////
			// Create a derived type.
			/////
			EntityType derivedType = TestHelper.CreateEntityType( "testDerived1", "test derived 1", baseType1, solution );
			derivedType.Save( );

			/////
			// Publish solution1 (1.0).
			/////
			var context = new ProcessingContext( );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			/////
			// Create a new solution (solution2).
			/////
			EntityType baseType2 = TestHelper.CreateEntityType( "testBase2", "test base 2", null, solution );
			baseType1.Save( );

			/////
			// Set the instance to only be in solution2.
			/////
			derivedType.SetRelationships( "core:inherits", new EntityRelationshipCollection<IEntity>
			{
				baseType2
			} );

			derivedType.Save( );

			Guid derivedUpgradeId = Entity.GetUpgradeId( derivedType.Id );

			solution.SolutionVersionString = "1.1";
			solution.Save( );

			/////
			// Publish solution2 (1.0).
			/////
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			/////
			// Create a new tenant
			/////
			long tenantId = TestHelper.CreateTenant( );

			/////
			// Deploy ReadiNow Core to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, "ReadiNow Core", null, context );

			/////
			// Deploy solution1 (1.0) to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, "1.0", context );

			/////
			// Upgrade the new tenant from solution2 (1.0) to solution2 (1.1).
			// The instance should be removed from the system since there is no external reference.
			/////
			AppManager.UpgradeApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, null, context );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long id = Entity.GetIdFromUpgradeId( derivedUpgradeId );

				IEntity entity = Entity.Get( id );

				Assert.IsNotNull( entity );
			}
		}

		/// <summary>
		///     Upgrades the existing application.
		/// </summary>
		/// <remarks>
		///     1. Create a solution
		///     2. Create a type
		///     3. Create an instance of the type
		///     4. Publish the application
		///     5. Rename the instance
		///     6. Publish the application again
		///     7. Create a new tenant
		///     8. Deploy Core to the new tenant
		///     9. Deploy the first version of the test solution to the new tenant
		///     10. Upgrade the application to the latest version in the new tenant
		///     11. Ensure that the tenants instance got renamed
		/// </remarks>
		[Test]
		[RunAsDefaultTenant]
		public void UpgradeExistingApp( )
		{
			/////
			// Create a solution (solution)
			/////
			Solution solution = TestHelper.CreateSolution( );
			solution.Save( );

			/////
			// Create a type (adding it to solution)
			/////
			EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
			entityType.Save( );

			/////
			// Create an instance (adding it to solution)
			/////
			IEntity instance = TestHelper.CreateInstance( entityType, inSolution: solution );
			instance.Save( );

			/////
			// Publish solution (version 1.0).
			/////
			var context = new ProcessingContext( );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			/////
			// Modify the name field of instance.
			/////
			instance.SetField( Resource.Name_Field, "Renamed Instance" );
			instance.Save( );

			solution.SolutionVersionString = "1.1";
			solution.Save( );

			/////
			// Publish solution (version 1.1).
			/////
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Guid instanceUpgradeId = Entity.GetUpgradeId( instance.Id );

			/////
			// Create a new tenant.
			/////
			long tenantId = TestHelper.CreateTenant( );

			/////
			// Deploy ReadiNow Core to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, "ReadiNow Core", null, context );

			/////
			// Deploy solution (1.0) to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, "1.0", context );

			/////
			// Upgrade solution (1.1) on the new tenant.
			/////
			AppManager.UpgradeApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, "1.1", context );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long id = Entity.GetIdFromUpgradeId( instanceUpgradeId );

				IEntity entity = Entity.Get( id );

				Assert.IsNotNull( entity );

				var name = entity.GetField<string>( Resource.Name_Field );

				Assert.AreEqual( name, "Renamed Instance" );
			}
		}

		/// <summary>
		///     Upgrades the existing application preserving user changes.
		/// </summary>
		/// <remarks>
		///     1. Create a solution
		///     2. Create a type
		///     3. Create an instance of the type
		///     4. Publish the application
		///     5. Rename the instance
		///     6. Publish the application again
		///     7. Create a new tenant
		///     8. Deploy Core to the new tenant
		///     9. Deploy the first version of the test solution to the new tenant
		///     10. Log into the new tenant and change the deployed instances name
		///     11. Upgrade the application to the latest version in the new tenant
		///     12. Ensure that the tenants instance did not get renamed
		/// </remarks>
		[Test]
		[RunAsDefaultTenant]
		public void UpgradeExistingAppPreserveUserChanges( )
		{
			/////
			// Create a solution (solution)
			/////
			Solution solution = TestHelper.CreateSolution( );
			solution.Save( );

			/////
			// Create a type (adding it to solution)
			/////
			EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
			entityType.Save( );

			/////
			// Create an instance (adding it to solution)
			/////
			IEntity instance = TestHelper.CreateInstance( entityType, inSolution: solution );
			instance.Save( );

			/////
			// Publish solution (version 1.0).
			/////
			var context = new ProcessingContext( );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			/////
			// Modify the name field of instance.
			/////
			instance.SetField( Resource.Name_Field, "Renamed Instance" );
			instance.Save( );

			solution.SolutionVersionString = "1.1";
			solution.Save( );

			/////
			// Publish solution (version 1.1).
			/////
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Guid instanceUpgradeId = Entity.GetUpgradeId( instance.Id );

			/////
			// Create a new tenant.
			/////
			long tenantId = TestHelper.CreateTenant( );

			/////
			// Deploy ReadiNow Core to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, "ReadiNow Core", null, context );

			/////
			// Deploy solution (1.0) to the new tenant.
			/////
			AppManager.DeployApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, "1.0", context );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long instanceId = Entity.GetIdFromUpgradeId( instanceUpgradeId );

				instance = Entity.Get( instanceId, true );

				/////
				// User modification to the name field.
				/////
				instance.SetField( Resource.Name_Field, "My Custom Instance" );

				instance.Save( );
			}

			/////
			// Upgrade solution (1.1) on the new tenant.
			/////
			AppManager.UpgradeApp( TestHelper.DefaultTenantName, TestHelper.DefaultSolutionName, "1.1", context );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long id = Entity.GetIdFromUpgradeId( instanceUpgradeId );

				IEntity entity = Entity.Get( id );

				Assert.IsNotNull( entity );

				var name = entity.GetField<string>( Resource.Name_Field );

				/////
				// Ensure the user modification remains intact.
				/////
				Assert.AreEqual( name, "My Custom Instance" );
			}
		}
	}
}