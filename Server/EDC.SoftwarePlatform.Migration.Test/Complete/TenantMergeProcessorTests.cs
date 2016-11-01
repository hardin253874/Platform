// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test.Complete
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
	public class TenantMergeProcessorTests
	{
        private static readonly Guid IsOfTypeUid = new Guid( "e1afc9e2-a526-4dc6-b90f-e2271e130f24" );
        
        /// <summary>
        ///     Tests the tear down.
        /// </summary>
        [TearDown]
		public void TestTearDown( )
		{
			TenantHelper.Flush( );
		}

		/// <summary>
		///     Ensure that updating a relationship A->X->B to become C->X->B (where relationship X is set to cascade delete)
		///     doesn't result in B being deleted before the new link can be established.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_CascadeDeleteWithUpdate( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			application.CreateAppDataRow( TableNames.AppDataBit, application.AppVerUid, application.RelationshipUid, AppDetails.CascadeDeleteUid, true );
			application.SetCardinality( CardinalityEnum_Enumeration.ManyToMany );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.FromInstanceUid ), "From Instance was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.ToInstanceUid ), "To Instance was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, application.RelationshipUid, application.FromInstanceUid, application.ToInstanceUid ), "Relationship between typeAInstance and typeBInstance was not deployed." );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			application.SetVersion( "1.1" );

			application.DeleteEntity( application.FromInstanceUid );

			Guid newInstanceUid = application.CreateInstance( application.FromTypeUid );

			application.CreateAppRelationshipRow( application.AppVerUid, application.RelationshipUid, newInstanceUid, application.ToInstanceUid );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.FromInstanceUid ), "Type A Instance was not deleted as part of the application deploy." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, newInstanceUid ), "Type A Instance 2 was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.ToInstanceUid ), "Type B Instance was not deployed as part of the application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, application.RelationshipUid, newInstanceUid, application.ToInstanceUid ), "Relationship between typeAInstance and typeBInstance was not deployed." );
		}

		/// <summary>
		///     Exclude the in solution relationship for external solutions.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		[Ignore( "No longer relevant due to 'inSolution' relationship now manyToOne cardinality" )]
		public void NewApplication_ExcludeInSolutionRelationshipForExternalSolutions( )
		{
			Solution solutionA = TestHelper.CreateSolution( "Solution A", null );
			Solution solutionB = TestHelper.CreateSolution( "Solution B", null );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solutionA );
			typeA.InSolution = solutionB;

			EntityType typeB = TestHelper.CreateEntityType( "Type A", null, null, solutionA );
			typeB.InSolution = solutionB;

			Relationship relationship = TestHelper.CreateRelationship( "Test Relationship", null, typeA, typeB, solutionA );
			relationship.InSolution = solutionB;

			solutionA.Save( );
			solutionB.Save( );
			typeA.Save( );
			typeB.Save( );
			relationship.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionA.Name );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionB.Name );

			Guid solutionAUpgradeId = solutionA.UpgradeId;
			Guid solutionBUpgradeId = solutionB.UpgradeId;
			Guid typeAUpgradeId = typeA.UpgradeId;
			Guid typeBUpgradeId = typeB.UpgradeId;
			Guid relationshipUpgradeId = relationship.UpgradeId;

			DatabaseContext context = DatabaseContext.GetContext( );

			if ( solutionA.PackageId != null )
			{
				Guid packageId = solutionA.PackageId.Value;

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeAUpgradeId ), "Type A was not published as part of solution A." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeBUpgradeId ), "Type B was not published as part of solution A." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship fromType was not published as part of solution A." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship toType was not published as part of solution A." );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeAUpgradeId, solutionAUpgradeId ), "typeA inSolution relationship was not published as part of solution A." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeBUpgradeId, solutionAUpgradeId ), "typeB inSolution relationship was not published as part of solution A." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, relationshipUpgradeId, solutionAUpgradeId ), "relationships inSolution relationship was not published as part of solution A." );

				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeAUpgradeId, solutionBUpgradeId ), "typeA inSolution relationship was published as part of solution A." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeBUpgradeId, solutionBUpgradeId ), "typeB inSolution relationship was published as part of solution A." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, relationshipUpgradeId, solutionBUpgradeId ), "relationships inSolution relationship was published as part of solution A." );
			}

			if ( solutionB.PackageId != null )
			{
				Guid packageId = solutionB.PackageId.Value;

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeAUpgradeId ), "Type A was not published as part of solution B." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeBUpgradeId ), "Type B was not published as part of solution B." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship fromType was not published as part of solution B." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship toType was not published as part of solution B." );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeAUpgradeId, solutionBUpgradeId ), "typeA inSolution relationship was not published as part of solution B." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeBUpgradeId, solutionBUpgradeId ), "typeB inSolution relationship was not published as part of solution B." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, relationshipUpgradeId, solutionBUpgradeId ), "relationships inSolution relationship was not published as part of solution B." );

				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeAUpgradeId, solutionAUpgradeId ), "typeA inSolution relationship was published as part of solution B." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeBUpgradeId, solutionAUpgradeId ), "typeB inSolution relationship was published as part of solution B." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, relationshipUpgradeId, solutionAUpgradeId ), "relationships inSolution relationship was published as part of solution B." );
			}
		}

		/// <summary>
		///     Implicit in solution links should be converted to explicit indirect in solution links upon deploy.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_ImplicitInSolutionShouldBeConvertedToExplicitIndirectInSolution( )
		{
			Solution solution = TestHelper.CreateSolution( "Solution A", null );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solution );

			EntityType typeB = TestHelper.CreateEntityType( "Type B", null );

			Relationship relationship = TestHelper.CreateRelationship( "Test Relationship", null, typeA, typeB, solution );
			relationship.ImplicitInSolution = true;

			solution.Save( );
			typeA.Save( );
			typeB.Save( );
			relationship.Save( );

			var typeAInstance = ( Entity ) TestHelper.CreateInstance( typeA, "Type A Instance", null, solution );
			var typeBInstance = ( Entity ) TestHelper.CreateInstance( typeB, "Type B Instance", null );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeBInstance
			} );
			typeAInstance.Save( );
			typeBInstance.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solution.Name );

			Guid typeAInstanceUpgradeId = typeAInstance.UpgradeId;
			Guid typeBInstanceUpgradeId = typeBInstance.UpgradeId;
			Guid relationshipUpgradeId = relationship.UpgradeId;
			Guid solutionUpgradeId = solution.UpgradeId;

			DatabaseContext context = DatabaseContext.GetContext( );

			if ( solution.PackageId != null )
			{
				Guid packageId = solution.PackageId.Value;

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeAInstanceUpgradeId ), "Type A Instance was not published as part of the application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeBInstanceUpgradeId ), "Type B Instance was not published as part of the application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeBInstanceUpgradeId ), "Relationship between typeAInstance and typeBInstance was not published." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InSolutionUid, typeAInstanceUpgradeId, solutionUpgradeId ), "inSolution relationship for typeAInstance was not published." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.InDirectInSolutionUid, typeBInstanceUpgradeId, solutionUpgradeId ), "indirectInSolution relationship for typeBInstance was not published." );
			}
		}

		/// <summary>
		///     Tests the missing dependency scenario.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_MissingDependency( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			application.DeleteEntity( application.ToInstanceUid );
			application.CreateAppRelationshipRow( application.AppVerUid, application.RelationshipUid, application.FromInstanceUid, application.ToInstanceUid );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			DatabaseContext context = DatabaseContext.GetContext( );
			long tenantId = RequestContext.TenantId;

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, application.AppVerUid, application.FromInstanceUid ), "Type A was not published as part of the new application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, application.AppVerUid, application.ToInstanceUid ), "Type A was not published as part of the new application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, application.AppVerUid, Direction.Forward, AppDetails.FromTypeTypeUid, application.RelationshipUid, application.FromTypeUid ), "Relationship fromType was not published." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, application.AppVerUid, Direction.Forward, AppDetails.ToTypeTypeUid, application.RelationshipUid, application.ToTypeUid ), "Relationship toType was not published." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.FromInstanceUid ), "Type A was not deployed as part of an existing application." );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.ToInstanceUid ), "Type B was not deployed as part of an existing application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.FromTypeTypeUid, application.RelationshipUid, application.FromTypeUid ), "Relationship was not deployed." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, application.RelationshipUid, application.ToTypeUid ), "Relationship was not deployed." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmDroppedRelationship( context, application.AppVerUid, tenantId, Direction.Forward, application.RelationshipUid, application.FromInstanceUid, application.ToInstanceUid ), "Relationship was not dropped." );
		}

		/// <summary>
		///     Ensure user modifications to a field are kept.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_PreserveUserFieldModifications( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			application.SetVersion( "1.1" );

			application.SetFieldValue( TableNames.AppDataNVarChar, application.FromInstanceUid, AppDetails.NameUid, "Bob" );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			long instanceId = Entity.GetIdFromUpgradeId( application.FromInstanceUid );

			var instance = ( Entity ) Entity.Get( instanceId, true );

			instance.SetField( Resource.Name_Field, "My Custom Instance" );

			instance.Save( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, application.FromInstanceUid, AppDetails.NameUid, "My Custom Instance" ) );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, application.FromInstanceUid, AppDetails.NameUid, "My Custom Instance" ) );
		}

		/// <summary>
		///     Application preserve user field modifications to null.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_PreserveUserFieldModificationsToNull( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			application.SetVersion( "1.1" );

			application.SetFieldValue( TableNames.AppDataNVarChar, application.FromInstanceUid, AppDetails.NameUid, "Bob" );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			long instanceId = Entity.GetIdFromUpgradeId( application.FromInstanceUid );

			var instance = ( Entity ) Entity.Get( instanceId, true );

			instance.SetField( Resource.Name_Field, null );

			instance.Save( );

			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, application.FromInstanceUid, AppDetails.NameUid, "My Custom Instance" ) );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, application.FromInstanceUid, AppDetails.NameUid, "Bob" ) );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, application.FromInstanceUid, AppDetails.NameUid, "My Custom Instance" ) );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, application.FromInstanceUid, AppDetails.NameUid, "Bob" ) );
		}

		// ---------------------------------------
		// THE FOLLOWING TESTS NEED TO BE SPED UP
		// ---------------------------------------


		/// <summary>
		///     Preserve user lookup modifications.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_PreserveUserLookupModifications( )
		{
			Solution solution = TestHelper.CreateSolution( "Test Solution A" );
			solution.Save( );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solution );
			typeA.Save( );

			EntityType typeB = TestHelper.CreateEntityType( "Type B", null, null, solution );
			typeB.Save( );

			Relationship relationship = TestHelper.CreateRelationship( "Test Relationship", null, typeA, typeB, solution );
			relationship.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
			relationship.Save( );

			var typeAInstance = ( Entity ) TestHelper.CreateInstance( typeA, "TypeA Instance", null, solution );
			typeAInstance.Save( );

			var typeBInstance = ( Entity ) TestHelper.CreateInstance( typeB, "TypeB Instance", null, solution );
			typeBInstance.Save( );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeBInstance
			} );
			typeAInstance.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solution.Name );

			var typeCInstance = ( Entity ) TestHelper.CreateInstance( typeB, "TypeC Instance", null, solution );
			typeCInstance.Save( );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeCInstance
			} );
			typeAInstance.Save( );

			solution.SolutionVersionString = "1.1";
			solution.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solution.Name );

			Guid typeBUpgradeId = typeB.UpgradeId;
			Guid typeAInstanceUpgradeId = typeAInstance.UpgradeId;
			Guid typeBInstanceUpgradeId = typeBInstance.UpgradeId;
			Guid typeCInstanceUpgradeId = typeCInstance.UpgradeId;
			Guid relationshipUpgradeId = relationship.UpgradeId;
			Guid tenantTypeXUpgradeId;

			const string tenantName = "ZZZ";
			DatabaseContext context = DatabaseContext.GetContext( );

			long tenantId = TestHelper.CreateTenant( tenantName, null );

			AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

			AppManager.DeployApp( tenantName, solution.Name, "1.0" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeBInstanceUpgradeId ) );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long tenantTypeBId = Entity.GetIdFromUpgradeId( typeBUpgradeId );
				long tenantTypeAInstanceId = Entity.GetIdFromUpgradeId( typeAInstanceUpgradeId );
				long tenantRelationshipId = Entity.GetIdFromUpgradeId( relationshipUpgradeId );

				var tenantTypeB = Entity.Get<EntityType>( tenantTypeBId, true );

				var tenantTypeXInstance = ( Entity ) TestHelper.CreateInstance( tenantTypeB, "TypeX Instance", null );
				tenantTypeXInstance.Save( );

				tenantTypeXUpgradeId = tenantTypeXInstance.UpgradeId;

				var tenantTypeAInstance = Entity.Get( tenantTypeAInstanceId, true );
				var tenantRelationship = Entity.Get( tenantRelationshipId );

				tenantTypeAInstance.SetRelationships( tenantRelationship, new EntityRelationshipCollection<IEntity>
				{
					tenantTypeXInstance
				} );

				tenantTypeAInstance.Save( );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, tenantTypeXUpgradeId ) );
			}

			AppManager.UpgradeApp( tenantName, solution.Name, "1.1" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, tenantTypeXUpgradeId ) );
		}

		/// <summary>
		///     Application preserve user lookup modifications to null.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_PreserveUserLookupModificationsToNull( )
		{
			Solution solution = TestHelper.CreateSolution( "Test Solution A" );
			solution.Save( );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solution );
			typeA.Save( );

			EntityType typeB = TestHelper.CreateEntityType( "Type B", null, null, solution );
			typeB.Save( );

			Relationship relationship = TestHelper.CreateRelationship( "Test Relationship", null, typeA, typeB, solution );
			relationship.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
			relationship.Save( );

			var typeAInstance = ( Entity ) TestHelper.CreateInstance( typeA, "TypeA Instance", null, solution );
			typeAInstance.Save( );

			var typeBInstance = ( Entity ) TestHelper.CreateInstance( typeB, "TypeB Instance", null, solution );
			typeBInstance.Save( );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeBInstance
			} );
			typeAInstance.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solution.Name );

			var typeCInstance = ( Entity ) TestHelper.CreateInstance( typeB, "TypeC Instance", null, solution );
			typeCInstance.Save( );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeCInstance
			} );
			typeAInstance.Save( );

			solution.SolutionVersionString = "1.1";
			solution.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solution.Name );

			Guid typeAInstanceUpgradeId = typeAInstance.UpgradeId;
			Guid typeBInstanceUpgradeId = typeBInstance.UpgradeId;
			Guid typeCInstanceUpgradeId = typeCInstance.UpgradeId;
			Guid relationshipUpgradeId = relationship.UpgradeId;

			const string tenantName = "ZZZ";
			DatabaseContext context = DatabaseContext.GetContext( );

			long tenantId = TestHelper.CreateTenant( tenantName, null );

			AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

			AppManager.DeployApp( tenantName, solution.Name, "1.0" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeBInstanceUpgradeId ) );

			using ( new TenantAdministratorContext( tenantId ) )
			{
				long tenantTypeAInstanceId = Entity.GetIdFromUpgradeId( typeAInstanceUpgradeId );
				long tenantRelationshipId = Entity.GetIdFromUpgradeId( relationshipUpgradeId );

				var tenantTypeAInstance = Entity.Get( tenantTypeAInstanceId, true );
				var tenantRelationship = Entity.Get( tenantRelationshipId );

				tenantTypeAInstance.SetRelationships( tenantRelationship, null );

				tenantTypeAInstance.Save( );

				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeBInstanceUpgradeId ) );
			}

			AppManager.UpgradeApp( tenantName, solution.Name, "1.1" );

			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeBInstanceUpgradeId ) );
			Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeCInstanceUpgradeId ) );
		}

		/// <summary>
		///     Test to resolve missing dependencies.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_ResolveMissingDependency( )
		{
			Solution solutionA = TestHelper.CreateSolution( "Solution A", null );
			Solution solutionB = TestHelper.CreateSolution( "Solution B", null );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solutionA );

			EntityType typeB = TestHelper.CreateEntityType( "Type A", null, null, solutionB );

			solutionB.Save( );
			typeB.Save( );

			/////
			// Publish solution B before the relationship is established.
			/////
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionB.Name );

			Relationship relationship = TestHelper.CreateRelationship( "Test Relationship", null, typeA, typeB, solutionA );

			solutionA.Save( );
			typeA.Save( );
			typeB.Save( );
			relationship.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionA.Name );

			Guid typeAUpgradeId = typeA.UpgradeId;
			Guid typeBUpgradeId = typeB.UpgradeId;
			Guid relationshipUpgradeId = relationship.UpgradeId;

			if ( solutionA.PackageId != null )
			{
				Guid packageId = solutionA.PackageId.Value;

				const string tenantName = "ZZZ";
				DatabaseContext context = DatabaseContext.GetContext( );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeAUpgradeId ), "Type A was not published as part of the new application." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeBUpgradeId ), "Type A was not published as part of the new application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship fromType was not published." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship toType was not published." );

				long tenantId = TestHelper.CreateTenant( tenantName, null );

				AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

				AppManager.DeployApp( tenantName, solutionA.Name );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeAUpgradeId ), "Type A was not deployed as part of an existing application." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeBUpgradeId ), "Type B was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship was not deployed." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not deployed." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmDroppedRelationship( context, packageId, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not dropped." );

				solutionA.SolutionVersionString = "1.1";
				solutionA.Save( );

				AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionA.Name );

				AppManager.DeployApp( tenantName, solutionB.Name );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeAUpgradeId ), "Type A was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeBUpgradeId ), "Type B was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship was not deployed." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not deployed." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmDroppedRelationship( context, packageId, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not dropped." );

				AppManager.DeployApp( tenantName, solutionA.Name );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeAUpgradeId ), "Type A was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeBUpgradeId ), "Type B was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship was not deployed." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not deployed." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmDroppedRelationship( context, packageId, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not dropped." );
			}
		}

		/// <summary>
		///     Resolve missing dependency via different application.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_ResolveMissingDependencyViaDifferentApplication( )
		{
			Solution solutionA = TestHelper.CreateSolution( "Solution A", null );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solutionA );

			EntityType typeB = TestHelper.CreateEntityType( "Type A", null );

			Relationship relationship = TestHelper.CreateRelationship( "Test Relationship", null, typeA, typeB, solutionA );

			solutionA.Save( );
			typeA.Save( );
			typeB.Save( );
			relationship.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionA.Name );

			Guid typeAUpgradeId = typeA.UpgradeId;
			Guid typeBUpgradeId = typeB.UpgradeId;
			Guid relationshipUpgradeId = relationship.UpgradeId;

			if ( solutionA.PackageId != null )
			{
				Guid packageId = solutionA.PackageId.Value;

				const string tenantName = "ZZZ";
				DatabaseContext context = DatabaseContext.GetContext( );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeAUpgradeId ), "Type A was not published as part of the new application." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmAppLibraryEntity( context, packageId, typeBUpgradeId ), "Type A was not published as part of the new application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship fromType was not published." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmAppLibraryRelationship( context, packageId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship toType was not published." );


				long tenantId = TestHelper.CreateTenant( tenantName, null );

				AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

				AppManager.DeployApp( tenantName, solutionA.Name );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeAUpgradeId ), "Type A was not deployed as part of an existing application." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeBUpgradeId ), "Type B was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship was not deployed." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not deployed." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmDroppedRelationship( context, packageId, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not dropped." );

				Solution solutionB = TestHelper.CreateSolution( "Solution B", null );

				typeB.InSolution = solutionB;

				typeB.Save( );
				solutionB.Save( );

				AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionB.Name );

				solutionA.SolutionVersionString = "1.1";
				solutionA.Save( );

				AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionA.Name );

				AppManager.DeployApp( tenantName, solutionB.Name );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeAUpgradeId ), "Type A was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeBUpgradeId ), "Type B was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship was not deployed." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not deployed." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmDroppedRelationship( context, packageId, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not dropped." );

				AppManager.DeployApp( tenantName, solutionA.Name );

				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeAUpgradeId ), "Type A was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, typeBUpgradeId ), "Type B was not deployed as part of an existing application." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.FromTypeTypeUid, relationshipUpgradeId, typeAUpgradeId ), "Relationship was not deployed." );
				Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not deployed." );
				Assert.IsFalse( TenantMergeProcessorTestHelper.ConfirmDroppedRelationship( context, packageId, tenantId, Direction.Forward, AppDetails.ToTypeTypeUid, relationshipUpgradeId, typeBUpgradeId ), "Relationship was not dropped." );
			}
		}

		/// <summary>
		///     Test update of a field value.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_UpdateFieldValue( )
		{
			Solution solution = TestHelper.CreateSolution( );
			solution.Save( );

			EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
			entityType.Save( );

			var instance = ( Entity ) TestHelper.CreateInstance( entityType, "Base Instance", null, solution );
			instance.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

			instance.SetField( Resource.Name_Field, "Renamed Instance" );
			instance.Save( );

			solution.SolutionVersionString = "1.1";
			solution.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

			Guid instanceUpgradeId = instance.UpgradeId;

			const string tenantName = "ZZZ";
			DatabaseContext context = DatabaseContext.GetContext( );

			long tenantId = TestHelper.CreateTenant( tenantName, null );

			AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

			AppManager.DeployApp( tenantName, TestHelper.DefaultSolutionName, "1.0" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, instanceUpgradeId, AppDetails.NameUid, "Base Instance" ) );

			AppManager.UpgradeApp( tenantName, TestHelper.DefaultSolutionName, "1.1" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, instanceUpgradeId, AppDetails.NameUid, "Renamed Instance" ) );
		}

		/// <summary>
		///     Update lookup values.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_UpdateLookup( )
		{
			Solution solution = TestHelper.CreateSolution( "Test Solution A" );
			solution.Save( );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solution );
			typeA.Save( );

			EntityType typeB = TestHelper.CreateEntityType( "Type B", null, null, solution );
			typeB.Save( );

			Relationship relationship = TestHelper.CreateRelationship( "Test Relationship", null, typeA, typeB, solution );
			relationship.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
			relationship.Save( );

			var typeAInstance = ( Entity ) TestHelper.CreateInstance( typeA, "TypeA Instance", null, solution );
			typeAInstance.Save( );

			var typeBInstance = ( Entity ) TestHelper.CreateInstance( typeB, "TypeB Instance", null, solution );
			typeBInstance.Save( );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeBInstance
			} );
			typeAInstance.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solution.Name );

			var typeCInstance = ( Entity ) TestHelper.CreateInstance( typeB, "TypeC Instance", null, solution );
			typeCInstance.Save( );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeCInstance
			} );
			typeAInstance.Save( );

			solution.SolutionVersionString = "1.1";
			solution.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solution.Name );

			Guid typeAInstanceUpgradeId = typeAInstance.UpgradeId;
			Guid typeBInstanceUpgradeId = typeBInstance.UpgradeId;
			Guid typeCInstanceUpgradeId = typeCInstance.UpgradeId;
			Guid relationshipUpgradeId = relationship.UpgradeId;

			const string tenantName = "ZZZ";
			DatabaseContext context = DatabaseContext.GetContext( );

			long tenantId = TestHelper.CreateTenant( tenantName, null );

			AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

			AppManager.DeployApp( tenantName, solution.Name, "1.0" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeBInstanceUpgradeId ) );

			AppManager.UpgradeApp( tenantName, solution.Name, "1.1" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeCInstanceUpgradeId ) );
        }

        /// <summary>
        ///     Test update of a field value.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void NewApplication_DeleteEntity( )
        {
            Solution solution = TestHelper.CreateSolution( );
            solution.Save( );

            EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
            entityType.Save( );

            var instance = ( Entity ) TestHelper.CreateInstance( entityType, "Base Instance", null, solution );
            instance.Save( );

            Guid instanceUpgradeId = instance.UpgradeId;

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            instance.Delete( );

            solution.SolutionVersionString = "1.1";
            solution.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            const string tenantName = "ZZZ";
            DatabaseContext context = DatabaseContext.GetContext( );

            long tenantId = TestHelper.CreateTenant( tenantName, null );

            AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

            AppManager.DeployApp( tenantName, TestHelper.DefaultSolutionName, "1.0" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.True );

            Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, instanceUpgradeId, AppDetails.NameUid, "Base Instance" ) );

            AppManager.UpgradeApp( tenantName, TestHelper.DefaultSolutionName, "1.1" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.False );
        }

        /// <summary>
        ///     Test removing an entity for an application without actually deleting the entity.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void NewApplication_RemoveEntity( )
        {
            Guid isOfTypeUid = new Guid( "e1afc9e2-a526-4dc6-b90f-e2271e130f24" );
            
            Solution solution = TestHelper.CreateSolution( );
            solution.Save( );

            EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
            entityType.Save( );

            var instance = ( Entity ) TestHelper.CreateInstance( entityType, "Instance", null, solution );
            instance.Save( );

            Guid instanceUpgradeId = instance.UpgradeId;

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            Resource resource = instance.Cast<Resource>( );
            resource.InSolution = null;
            resource.Save( );

            solution.SolutionVersionString = "1.1";
            solution.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            const string tenantName = "ZZZ";
            DatabaseContext context = DatabaseContext.GetContext( );

            long tenantId = TestHelper.CreateTenant( tenantName, null );

            AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

            AppManager.DeployApp( tenantName, TestHelper.DefaultSolutionName, "1.0" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, instanceUpgradeId, AppDetails.NameUid, "Instance" ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, isOfTypeUid, instanceUpgradeId, entityType.UpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.InSolutionUid, instanceUpgradeId, solution.UpgradeId ), Is.True );

            AppManager.UpgradeApp( tenantName, TestHelper.DefaultSolutionName, "1.1" );

            // Entity has not been deleted, nor has field or relationship data
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, instanceUpgradeId, AppDetails.NameUid, "Instance" ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, isOfTypeUid, instanceUpgradeId, entityType.UpgradeId ), Is.True );

            // Entity has been removed from app
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship(context, tenantId, Direction.Forward, AppDetails.InSolutionUid, instanceUpgradeId, solution.UpgradeId ), Is.False );
        }

        /// <summary>
        ///     Test that 'doNotRemove' data is carried forward into publishing subsequent versions of the app.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void NewApplication_RemoveDataCarriedForwards( )
        {
            Solution solution = TestHelper.CreateSolution( );
            solution.Save( );

            EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
            entityType.Save( );

            var instance = ( Entity ) TestHelper.CreateInstance( entityType, "Instance", null, solution );
            instance.Save( );

            Guid instanceUpgradeId = instance.UpgradeId;

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            Resource resource = instance.Cast<Resource>( );
            resource.InSolution = null;
            resource.Save( );

            solution.SolutionVersionString = "1.1";
            solution.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            solution.SolutionVersionString = "1.2";
            solution.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            const string tenantName = "ZZZ";
            DatabaseContext context = DatabaseContext.GetContext( );

            long tenantId = TestHelper.CreateTenant( tenantName, null );

            AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

            AppManager.DeployApp( tenantName, TestHelper.DefaultSolutionName, "1.0" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, instanceUpgradeId, AppDetails.NameUid, "Instance" ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, IsOfTypeUid, instanceUpgradeId, entityType.UpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.InSolutionUid, instanceUpgradeId, solution.UpgradeId ), Is.True );

            AppManager.UpgradeApp( tenantName, TestHelper.DefaultSolutionName, "1.2" );

            // Entity has not been deleted, nor has field or relationship data
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, instanceUpgradeId, AppDetails.NameUid, "Instance" ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, IsOfTypeUid, instanceUpgradeId, entityType.UpgradeId ), Is.True );

            // Entity has been removed from app
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.InSolutionUid, instanceUpgradeId, solution.UpgradeId ), Is.False );
        }

        /// <summary>
        ///     Test update of a field value.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void NewApplication_DeleteRelatedEntities( )
        {
            Solution solution = TestHelper.CreateSolution( );
            solution.Save( );

            EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
            entityType.Save( );

            var instance = ( Entity ) TestHelper.CreateInstance( entityType, "Base Instance", null, solution );
            instance.Save( );

            Guid instanceUpgradeId = instance.UpgradeId;
            Guid entityTypeUpgradeId = entityType.UpgradeId;

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            entityType.Delete( );
            instance.Delete( );

            solution.SolutionVersionString = "1.1";
            solution.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            const string tenantName = "ZZZ";
            DatabaseContext context = DatabaseContext.GetContext( );

            long tenantId = TestHelper.CreateTenant( tenantName, null );

            AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

            AppManager.DeployApp( tenantName, TestHelper.DefaultSolutionName, "1.0" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, IsOfTypeUid, instanceUpgradeId, entityTypeUpgradeId ), Is.True );

            AppManager.UpgradeApp( tenantName, TestHelper.DefaultSolutionName, "1.1" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.False );
        }

        /// <summary>
        ///     Test removing related entities (and the relationship between them) without actually deleting the entities.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void NewApplication_RemoveRelatedEntity( )
        {
            Guid isOfTypeUid = new Guid( "e1afc9e2-a526-4dc6-b90f-e2271e130f24" );

            Solution solution = TestHelper.CreateSolution( );
            solution.Save( );

            EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
            entityType.Save( );

            var instance = ( Entity ) TestHelper.CreateInstance( entityType, "Instance", null, solution );
            instance.Save( );

            Guid instanceUpgradeId = instance.UpgradeId;

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            Resource resource = instance.Cast<Resource>( );
            resource.InSolution = null;
            resource.Save( );
            entityType.InSolution = null;
            entityType.Save( );

            solution.SolutionVersionString = "1.1";
            solution.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            const string tenantName = "ZZZ";
            DatabaseContext context = DatabaseContext.GetContext( );

            long tenantId = TestHelper.CreateTenant( tenantName, null );

            AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

            AppManager.DeployApp( tenantName, TestHelper.DefaultSolutionName, "1.0" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, entityType.UpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, isOfTypeUid, instanceUpgradeId, entityType.UpgradeId ), Is.True );

            AppManager.UpgradeApp( tenantName, TestHelper.DefaultSolutionName, "1.1" );

            // Entities have not been deleted, nor has relationship data
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, instanceUpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, entityType.UpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, isOfTypeUid, instanceUpgradeId, entityType.UpgradeId ), Is.True );
        }

        /// <summary>
        ///     Test removing related entities (and the relationship between them) without actually deleting the entities.
        /// </summary>
        [Test]
        [TestCase( "Detatch" )]
        [TestCase( "Reattach" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void DoNotRemove_Vs_CascadeDelete( string scenario )
        {
            Guid isOfTypeUid = new Guid( "e1afc9e2-a526-4dc6-b90f-e2271e130f24" );

            Solution solution = TestHelper.CreateSolution( );
            solution.Save( );

            EntityType entityType = TestHelper.CreateEntityType( inSolution: solution );
            StringField sf = new StringField( );
            sf.Name = "SomeField";
            entityType.Fields.Add( sf.As<Field>( ) );
            entityType.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            if (scenario == "Detatch")
            {
                sf.FieldIsOnType = null;
                sf.Save( );
            }
            else if ( scenario == "Reattach" )
            {
                EntityType entityType2 = TestHelper.CreateEntityType( name: "Type2", inSolution: solution );
                entityType2.Save( );
                sf.FieldIsOnType = entityType2;
                sf.Save( );
            }

            Guid entityTypeUid = entityType.UpgradeId;
            entityType.Delete( );

            solution.SolutionVersionString = "1.1";
            solution.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

            const string tenantName = "ZZZ";
            DatabaseContext context = DatabaseContext.GetContext( );

            long tenantId = TestHelper.CreateTenant( tenantName, null );

            AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );
            AppManager.DeployApp( tenantName, TestHelper.DefaultSolutionName, "1.0" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, entityTypeUid ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, sf.UpgradeId ), Is.True );

            AppManager.UpgradeApp( tenantName, TestHelper.DefaultSolutionName, "1.1" );

            // Entities have not been deleted, nor has relationship data
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, entityTypeUid ), Is.False );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, sf.UpgradeId ), Is.True );
        }

        /// <summary>
        ///     Test moving an entity from one application to another.
        ///     Test upgrading the recipient before and after the original app.
        /// </summary>
        [Test]
        [TestCase( "SourceThenTarget" )]
        [TestCase( "TargetThenSource" )]
        [TestCase( "TargetOnly" )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_MoveEntityToNewApp( string upgradeOrder )
        {
            // Create both apps
            Solution sourceApp = TestHelper.CreateSolution( "SourceApp" );
            sourceApp.Save( );

            Solution targetApp = TestHelper.CreateSolution( "TargetApp" );
            targetApp.Save( );

            EntityType entityType = TestHelper.CreateEntityType( inSolution: sourceApp );
            entityType.Save( );

            // Publish both apps
            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "SourceApp" );
            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "TargetApp" );

            // Create a tenant and deploy both apps
            string tenantName = "TestMoveEntityToNewApp" + upgradeOrder;
            DatabaseContext context = DatabaseContext.GetContext( );

            long tenantId = TestHelper.CreateTenant( tenantName, null );
            AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );
            AppManager.DeployApp( tenantName, "SourceApp", "1.0" );
            AppManager.DeployApp( tenantName, "TargetApp", "1.0" );

            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, entityType.UpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.InSolutionUid, entityType.UpgradeId, sourceApp.UpgradeId ), Is.True );

            // Remove the entity from the old app and add it to the new app
            entityType.InSolution = targetApp;
            entityType.Save( );
            sourceApp.SolutionVersionString = "2.0";
            sourceApp.Save( );
            targetApp.SolutionVersionString = "2.0";
            targetApp.Save( );

            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "SourceApp" );
            AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "TargetApp" );

            // Upgrade the tenant
            if ( upgradeOrder == "SourceThenTarget" )
            {
                AppManager.UpgradeApp( tenantName, "SourceApp", "2.0" );
                AppManager.UpgradeApp( tenantName, "TargetApp", "2.0" );
            }
            else if ( upgradeOrder == "TargetThenSource" )
            {
                AppManager.UpgradeApp( tenantName, "SourceApp", "2.0" );
                AppManager.UpgradeApp( tenantName, "TargetApp", "2.0" );
            }
            else if ( upgradeOrder == "TargetOnly" )
            {
                AppManager.UpgradeApp( tenantName, "SourceApp", "2.0" );
                AppManager.UpgradeApp( tenantName, "TargetApp", "2.0" );
            }
            else
            {
                throw new Exception( "Test error" );
            }

            // Confirm entity is present and assigned to target
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, entityType.UpgradeId ), Is.True );
            Assert.That( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.InSolutionUid, entityType.UpgradeId, targetApp.UpgradeId ), Is.True );
        }

        /// <summary>
        ///     Add a new entity.
        /// </summary>
        [Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void _NewApplication_AddNewEntity( )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			Guid appVerUid = Guid.NewGuid( );
			Guid appPackageUid = Guid.NewGuid( );

			application.ReplaceUpgradeId( application.AppVerUid, appVerUid );
			application.ReplaceUpgradeId( application.AppPackageUid, appPackageUid );

			var newEntityId = Guid.NewGuid( );

			DataTable appEntity = application.Data.Tables[ TableNames.AppEntity ];

			AppDetails.CreateAppEntityRow( appEntity, appVerUid, newEntityId );

			DataTable appRelationship = application.Data.Tables[ TableNames.AppRelationship ];

			AppDetails.CreateAppRelationshipRow( appRelationship, appVerUid, AppDetails.IsOfTypeUid, newEntityId, application.FromTypeUid );

			application.SetVersion( "1.1" );

			application.AppVerUid = appVerUid;
			application.AppPackageUid = appPackageUid;

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, newEntityId ), "Relationship was not deployed as part of a new application." );
		}

		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		[Ignore("No longer relevant due to 'inSolution' relationship now manyToOne cardinality")]
		public void NewApplication_Field_LastInWins( )
		{
			Solution solutionA = TestHelper.CreateSolution( "Test Solution A" );
			solutionA.Save( );

			Solution solutionB = TestHelper.CreateSolution( "Test Solution B" );
			solutionB.Save( );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solutionA );
			typeA.InSolution = solutionB;
			typeA.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionA.Name );

			typeA.Name = "Type A Two";
			typeA.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionB.Name );

			Guid typeAUpgradeId = typeA.UpgradeId;

			const string tenantName = "ZZZ";
			DatabaseContext context = DatabaseContext.GetContext( );

			long tenantId = TestHelper.CreateTenant( tenantName, null );

			AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

			AppManager.DeployApp( tenantName, solutionA.Name, "1.0" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, typeAUpgradeId, EntityType.Name_Field.UpgradeId, "Type A" ) );

			AppManager.DeployApp( tenantName, solutionB.Name, "1.0" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.NVarChar, typeAUpgradeId, EntityType.Name_Field.UpgradeId, "Type A Two" ) );
		}
	}
}