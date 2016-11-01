// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test.Complete
{
	/// <summary>
	///     Tenant Merge Processor Complete Tests.
	/// </summary>
	[TestFixture]
	[Category( "ExtendedTests" )]
	[Category( "AppLibraryTests" )]
	public class TenantMergeProcessorCompleteTests
	{
		/// <summary>
		///     Adds a new relationship instance in the forward direction.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="expectedInstanceCount">The expected instance count.</param>
		/// <exception cref="MissingRelationshipException"></exception>
		[TestCase( CardinalityEnum_Enumeration.OneToOne, 1 )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany, 1 )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne, 2 )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany, 2 )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_AddRelationshipInstance_From( CardinalityEnum_Enumeration cardinality, int expectedInstanceCount )
		{
			Guid newInstanceId = Guid.NewGuid( );

			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			application.CreateAppEntityRow( application.AppVerUid, newInstanceId );
			application.CreateAppDataRow( TableNames.AppDataNVarChar, application.AppVerUid, newInstanceId, AppDetails.NameUid, "Violation Test" );

			application.SetCardinality( cardinality );

			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.IsOfTypeUid, newInstanceId, application.FromTypeUid );
			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.InSolutionUid, newInstanceId, application.SolutionUid );
			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.SecurityOwnerUid, newInstanceId, AppDetails.AdministratorUserAccountUid );
			application.CreateAppRelationshipRow( application.AppVerUid, application.RelationshipUid, newInstanceId, application.ToInstanceUid );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.RelationshipUid ), "New entity was not deployed as part of the new application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.FromInstanceUid ), "Existing entity was not deployed as part of an existing application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.ToInstanceUid ), "Existing entity was not deployed as part of an existing application." );

			int instanceCount = 0;

			/////
			// One of the relationships must be deployed while the other is not. Order is non-deterministic.
			/////
			if ( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, application.RelationshipUid, application.FromInstanceUid, application.ToInstanceUid ) )
			{
				instanceCount++;
			}

			if ( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, application.RelationshipUid, newInstanceId, application.ToInstanceUid ) )
			{
				instanceCount++;
			}

			Assert.AreEqual( expectedInstanceCount, instanceCount, "Relationship instance count incorrect." );
		}

		/// <summary>
		///     Add a relationship instance in the reverse direction.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="expectedInstanceCount">The expected instance count.</param>
		/// <exception cref="MissingRelationshipException"></exception>
		[TestCase( CardinalityEnum_Enumeration.OneToOne, 1 )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany, 2 )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne, 1 )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany, 2 )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_AddRelationshipInstance_To( CardinalityEnum_Enumeration cardinality, int expectedInstanceCount )
		{
			Guid newInstanceId = Guid.NewGuid( );

			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			application.CreateAppEntityRow( application.AppVerUid, newInstanceId );
			application.CreateAppDataRow( TableNames.AppDataNVarChar, application.AppVerUid, newInstanceId, AppDetails.NameUid, "Violation Test" );

			application.SetCardinality( cardinality );

			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.IsOfTypeUid, newInstanceId, application.ToTypeUid );
			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.InSolutionUid, newInstanceId, application.SolutionUid );
			application.CreateAppRelationshipRow( application.AppVerUid, AppDetails.SecurityOwnerUid, newInstanceId, AppDetails.AdministratorUserAccountUid );
			application.CreateAppRelationshipRow( application.AppVerUid, application.RelationshipUid, application.FromInstanceUid, newInstanceId );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.RelationshipUid ), "New entity was not deployed as part of the new application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.FromInstanceUid ), "Existing entity was not deployed as part of an existing application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.ToInstanceUid ), "Existing entity was not deployed as part of an existing application." );

			int instanceCount = 0;

			/////
			// One of the relationships must be deployed while the other is not. Order is non-deterministic.
			/////
			if ( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, application.RelationshipUid, application.FromInstanceUid, application.ToInstanceUid ) )
			{
				instanceCount++;
			}

			if ( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, application.RelationshipUid, application.FromInstanceUid, newInstanceId ) )
			{
				instanceCount++;
			}

			Assert.AreEqual( expectedInstanceCount, instanceCount, "Relationship instance count incorrect." );
		}

		/// <summary>
		///     A new application attempts to modify an existing relationship.
		/// </summary>
		/// <param name="testCardinality">The test cardinality.</param>
		/// <param name="relationshipCardinality">The relationship cardinality.</param>
		private void NewApplication_ModifyExistingRelationship( CardinalityEnum_Enumeration testCardinality, CardinalityEnum_Enumeration relationshipCardinality )
		{
			var applicationA = TestMigrationHelper.CreateAppLibraryApplication( );

			applicationA.SetCardinality( relationshipCardinality );

			TestMigrationHelper.SaveAppLibraryApplication( applicationA );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, applicationA.SolutionUid.ToString( "B" ) );

			var applicationB = TestMigrationHelper.CreateAppLibraryApplication( );

			applicationB.ReplaceUpgradeId( applicationB.RelationshipUid, applicationA.RelationshipUid );
			applicationB.RelationshipUid = applicationA.RelationshipUid;

			applicationB.SetCardinality( testCardinality );

			TestMigrationHelper.SaveAppLibraryApplication( applicationB );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, applicationB.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, applicationA.RelationshipUid ), "Relationship was not deployed as part of a new application." );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.CardinalityUid, applicationA.RelationshipUid, AppDetails.ConvertCardinalityEnumToUpgradeId( testCardinality ) ), "Relationship cardinality was not deployed." );
		}

		/// <summary>
		///     Existing the application_ modify existing relationship.
		/// </summary>
		/// <param name="testCardinality">The test cardinality.</param>
		/// <param name="relationshipCardinality">The relationship cardinality.</param>
		protected void ExistingApplication_ModifyExistingRelationship( CardinalityEnum_Enumeration testCardinality, CardinalityEnum_Enumeration relationshipCardinality )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			application.SetCardinality( relationshipCardinality );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.RelationshipUid ), "Relationship was not deployed as part of a new application." );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.CardinalityUid, application.RelationshipUid, AppDetails.ConvertCardinalityEnumToUpgradeId( testCardinality ) ), "Relationship cardinality was not deployed." );
		}

		/// <summary>
		///     Modify an existing one to one relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		[TestCase( CardinalityEnum_Enumeration.OneToOne )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_ModifyExistingOneToOneRelationship( CardinalityEnum_Enumeration cardinality )
		{
			NewApplication_ModifyExistingRelationship( cardinality, CardinalityEnum_Enumeration.OneToOne );
		}

		/// <summary>
		///     Modify an existing one to many relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		[TestCase( CardinalityEnum_Enumeration.OneToOne )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_ModifyExistingOneToManyRelationship( CardinalityEnum_Enumeration cardinality )
		{
			NewApplication_ModifyExistingRelationship( cardinality, CardinalityEnum_Enumeration.OneToMany );
		}

		/// <summary>
		///     Modify an existing many to one relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		[TestCase( CardinalityEnum_Enumeration.OneToOne )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_ModifyExistingManyToOneRelationship( CardinalityEnum_Enumeration cardinality )
		{
			NewApplication_ModifyExistingRelationship( cardinality, CardinalityEnum_Enumeration.ManyToOne );
		}

		/// <summary>
		///     Modify an existing many to many relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		[TestCase( CardinalityEnum_Enumeration.OneToOne )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_ModifyExistingManyToManyRelationship( CardinalityEnum_Enumeration cardinality )
		{
			NewApplication_ModifyExistingRelationship( cardinality, CardinalityEnum_Enumeration.ManyToMany );
		}

		/// <summary>
		///     Modify an existing one to one relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		[TestCase( CardinalityEnum_Enumeration.OneToOne )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void ExistingApplication_ModifyExistingOneToOneRelationship( CardinalityEnum_Enumeration cardinality )
		{
			ExistingApplication_ModifyExistingRelationship( cardinality, CardinalityEnum_Enumeration.OneToOne );
		}

		/// <summary>
		///     Modify an existing one to many relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		[TestCase( CardinalityEnum_Enumeration.OneToOne, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void ExistingApplication_ModifyExistingOneToManyRelationship( CardinalityEnum_Enumeration cardinality )
		{
			ExistingApplication_ModifyExistingRelationship( cardinality, CardinalityEnum_Enumeration.OneToMany );
		}

		/// <summary>
		///     Modify an existing many to one relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		[TestCase( CardinalityEnum_Enumeration.OneToOne, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void ExistingApplication_ModifyExistingManyToOneRelationship( CardinalityEnum_Enumeration cardinality )
		{
			ExistingApplication_ModifyExistingRelationship( cardinality, CardinalityEnum_Enumeration.ManyToOne );
		}

		/// <summary>
		///     Modify an existing many to many relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		[TestCase( CardinalityEnum_Enumeration.OneToOne, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.OneToMany, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.ManyToOne, Ignore = true, IgnoreReason = "This should pass. Writing the new cardinality relationship before removing the existing one would in itself cause a cardinality violation and as such the new relationship is dropped." )]
		[TestCase( CardinalityEnum_Enumeration.ManyToMany )]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void ExistingApplication_ModifyExistingManyToManyRelationship( CardinalityEnum_Enumeration cardinality )
		{
			ExistingApplication_ModifyExistingRelationship( cardinality, CardinalityEnum_Enumeration.ManyToMany );
		}

		/// <summary>
		///     Ensures the cascade delete does not remove entities that are still in use.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void EnsureCascadeDeleteDoesNotRemoveEntitiesThatAreStillInUse( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			Guid typeA = application.CreateType( "TypeA" );

			Guid relationshipId = application.CreateRelationship( "A -> A", typeA, typeA, AppDetails.ManyToOneUid, true, true );

			Guid a1 = application.CreateInstance( typeA, "a1" );
			Guid a2 = application.CreateInstance( typeA, "a2" );
			Guid a3 = application.CreateInstance( typeA, "a3" );

			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a1, a2 );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a2, a3 );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a3 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, a2 ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a2, a3 ), "Relationship was not deployed as part of the application." );

			application.DeleteRelationship( relationshipId, a2, a3 );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a1, a3 );
			application.DeleteEntity( a2 );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a3 ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a2, a3 ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, a3 ), "Relationship was not deployed as part of the application." );
		}

		/// <summary>
		///     Ensures the one to one relationship gets updated in the forward direction.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void EnsureOneToOneRelationshipGetsUpdatedInTheForwardDirection( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			Guid typeA = application.CreateType( "TypeA" );

			Guid relationshipId = application.CreateRelationship( "A -> A", typeA, typeA, AppDetails.OneToOneUid, true, true );

			Guid a1 = application.CreateInstance( typeA, "a1" );
			Guid a2 = application.CreateInstance( typeA, "a2" );
			Guid a3 = application.CreateInstance( typeA, "a3" );

			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a1, a2 );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a3 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, a2 ), "Relationship was not deployed as part of the application." );

			application.DeleteRelationship( relationshipId, a1, a2 );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a1, a3 );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a3 ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, a2 ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, a3 ), "Relationship was not deployed as part of the application." );
		}

		/// <summary>
		///     Ensures the one to one relationship gets updated in the reverse direction.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void EnsureOneToOneRelationshipGetsUpdatedInTheReverseDirection( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			Guid typeA = application.CreateType( "TypeA" );

			Guid relationshipId = application.CreateRelationship( "A -> A", typeA, typeA, AppDetails.OneToOneUid, true, true );

			Guid a1 = application.CreateInstance( typeA, "a1" );
			Guid a2 = application.CreateInstance( typeA, "a2" );
			Guid a3 = application.CreateInstance( typeA, "a3" );

			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a1, a2 );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a3 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, a2 ), "Relationship was not deployed as part of the application." );

			application.DeleteRelationship( relationshipId, a1, a2 );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a3, a2 );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a3 ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, a2 ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a3, a2 ), "Relationship was not deployed as part of the application." );
		}

		/// <summary>
		///     Ensures the relationship change made by application is preserved.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void EnsureRelationshipChangeMadeByApplicationIsPreserved( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			Guid typeA = application.CreateType( "TypeA" );
			Guid typeB = application.CreateType( "TypeB" );

			Guid relationshipId = application.CreateRelationship( "A -> B", typeA, typeB, AppDetails.ManyToOneUid, true, false );

			Guid a1 = application.CreateInstance( typeA, "a1" );
			Guid a2 = application.CreateInstance( typeA, "a2" );
			Guid b1 = application.CreateInstance( typeB, "b1" );

			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a1, b1 );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeB ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, b1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, b1 ), "Relationship was not deployed as part of the application." );

			application.DeleteRelationship( relationshipId, a1, b1 );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a2, b1 );
			application.DeleteEntity( a1 );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeB ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, b1 ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, b1 ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a2, b1 ), "Relationship was not deployed as part of the application." );
		}

		/// <summary>
		///     Ensures the relationship change made by both application and tenant are preserved.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void EnsureRelationshipChangeMadeByBothApplicationAndTenantArePreserved( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			Guid typeA = application.CreateType( "TypeA" );
			Guid typeB = application.CreateType( "TypeB" );

			Guid relationshipId = application.CreateRelationship( "A -> B", typeA, typeB, AppDetails.ManyToOneUid, true, false );

			Guid a1 = application.CreateInstance( typeA, "a1" );
			Guid a2 = application.CreateInstance( typeA, "a2" );
			Guid b1 = application.CreateInstance( typeB, "b1" );

			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a1, b1 );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeB ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, b1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, b1 ), "Relationship was not deployed as part of the application." );

			const string commandText = @"
DECLARE @a1 BIGINT
DECLARE @a2 BIGINT
DECLARE @b1 BIGINT
DECLARE @r BIGINT

SELECT @a1 = Id FROM Entity WHERE UpgradeId = '{0}' AND TenantId = {4}
SELECT @a2 = Id FROM Entity WHERE UpgradeId = '{1}' AND TenantId = {4}
SELECT @b1 = Id FROM Entity WHERE UpgradeId = '{2}' AND TenantId = {4}
SELECT @r = Id FROM Entity WHERE UpgradeId = '{3}' AND TenantId = {4}

DELETE FROM Relationship WHERE TenantId = {4} AND TypeId = @r AND FromId = @a1 AND ToId = @b1
INSERT INTO Relationship (TenantId, TypeId, FromId, ToId ) VALUES ( {4}, @r, @a2, @b1 )";

			using ( IDbCommand command = context.CreateCommand( string.Format( commandText, a1, a2, b1, relationshipId, tenantId ) ) )
			{
				command.ExecuteNonQuery( );
			}

			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, b1 ), "Relationship was not updated." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a2, b1 ), "Relationship was not updated." );

			application.DeleteRelationship( relationshipId, a1, b1 );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a2, b1 );
			application.DeleteEntity( a1 );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeB ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a1 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a2 ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, b1 ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a1, b1 ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a2, b1 ), "Relationship was not deployed as part of the application." );
		}

		/// <summary>
		///     Add an existing entity.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_AddExistingEntity( )
		{
			var applicationA = TestMigrationHelper.CreateAppLibraryApplication( );

			TestMigrationHelper.SaveAppLibraryApplication( applicationA );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, applicationA.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, applicationA.ToInstanceUid ), "Existing entity was not deployed as part of an existing application." );

			var applicationB = TestMigrationHelper.CreateAppLibraryApplication( );

			TestMigrationHelper.SaveAppLibraryApplication( applicationB );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, applicationB.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, applicationB.ToInstanceUid ), "Existing entity is no longer present after deploying it a second time." );
		}

		/// <summary>
		///     Add an existing relationship to a new application.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_AddExistingRelationship( )
		{
			var applicationA = TestMigrationHelper.CreateAppLibraryApplication( );

			TestMigrationHelper.SaveAppLibraryApplication( applicationA );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, applicationA.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, applicationA.RelationshipUid ), "Existing entity was not deployed as part of an existing application." );

			var applicationB = TestMigrationHelper.CreateAppLibraryApplication( );

			TestMigrationHelper.SaveAppLibraryApplication( applicationB );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, applicationB.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, applicationB.RelationshipUid ), "Existing entity is no longer present after deploying it a second time." );
		}

		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		[Ignore( "Currently not implemented" )]
		public void EnsureRelationshipChangeEnforcesCardinalityRules( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			Guid typeA = application.CreateType( "TypeA" );

			Guid relationshipId = application.CreateRelationship( "A -> A", typeA, typeA, AppDetails.OneToOneUid, true, true );

			Guid a = application.CreateInstance( typeA, "a" );
			Guid b = application.CreateInstance( typeA, "b" );
			Guid c = application.CreateInstance( typeA, "c" );
			Guid d = application.CreateInstance( typeA, "d" );

			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a, b );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, c, d );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, b ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, c ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, d ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a, b ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, c, d ), "Relationship was not deployed as part of the application." );

			application.DeleteRelationship( relationshipId, a, b );
			application.DeleteRelationship( relationshipId, c, d );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a, d );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, b ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, c ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, d ), "Entity was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a, b ), "Relationship was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, c, d ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a, d ), "Relationship was not deployed as part of the application." );
		}

		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		[Ignore( "Currently not implemented" )]
		public void EnsureRelationshipChangeEnforcesCardinalityRules2( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			Guid typeA = application.CreateType( "TypeA" );

			Guid relationshipId = application.CreateRelationship( "A -> A", typeA, typeA, AppDetails.OneToOneUid, true, true );

			Guid a = application.CreateInstance( typeA, "a" );
			Guid b = application.CreateInstance( typeA, "b" );
			Guid c = application.CreateInstance( typeA, "c" );
			Guid d = application.CreateInstance( typeA, "d" );

			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a, b );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, c, d );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, b ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, c ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, d ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a, b ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, c, d ), "Relationship was not deployed as part of the application." );

			application.ChangeRelationshipCardinality( relationshipId, AppDetails.OneToOneUid, AppDetails.OneToManyUid );
			application.DeleteRelationship( relationshipId, c, d );
			application.CreateAppRelationshipRow( application.AppVerUid, relationshipId, a, d );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeA ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, relationshipId ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, a ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, b ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, c ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, d ), "Entity was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a, b ), "Relationship was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, a, d ), "Relationship was not deployed as part of the application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipId, c, d ), "Relationship was not deployed as part of the application." );
		}
	}
}