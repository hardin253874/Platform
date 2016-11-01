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
	///     Tenant Merge Processor Relationship Tests.
	/// </summary>
	[TestFixture]
	[Category( "ExtendedTests" )]
	[Category( "AppLibraryTests" )]
	public class TenantMergeProcessorRelationshipTests
	{
		/// <summary>
		///     News the application_ add new relationship implementation.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="cascadeDelete">if set to <c>true</c> [cascade delete].</param>
		/// <param name="cascadeDeleteTo">if set to <c>true</c> [cascade delete to].</param>
		/// <param name="implicitInSolution">if set to <c>true</c> [implicit in solution].</param>
		/// <param name="reverseImplicitInSolution">if set to <c>true</c> [reverse implicit in solution].</param>
		public void NewApplication_AddNewRelationshipImpl( CardinalityEnum_Enumeration cardinality, bool cascadeDelete, bool cascadeDeleteTo, bool implicitInSolution, bool reverseImplicitInSolution )
		{
			var application = TestMigrationHelper.CreateAppLibraryApplication( );

			application.SetCardinality( cardinality );

			DataTable appDataBit = application.Data.Tables[ TableNames.AppDataBit ];

			AppDetails.CreateAppDataRow( appDataBit, application.AppVerUid, application.RelationshipUid, AppDetails.CascadeDeleteUid, cascadeDelete );
			AppDetails.CreateAppDataRow( appDataBit, application.AppVerUid, application.RelationshipUid, AppDetails.CascadeDeleteToUid, cascadeDeleteTo );
			AppDetails.CreateAppDataRow( appDataBit, application.AppVerUid, application.RelationshipUid, AppDetails.ImplicitInSolutionUid, implicitInSolution );
			AppDetails.CreateAppDataRow( appDataBit, application.AppVerUid, application.RelationshipUid, AppDetails.ReverseImplicitInSolutionUid, reverseImplicitInSolution );

			TestMigrationHelper.SaveAppLibraryApplication( application );

			AppManager.DeployApp( RunAsDefaultTenant.DefaultTenantName, application.SolutionUid.ToString( "B" ) );

			long tenantId = RequestContext.TenantId;
			DatabaseContext context = DatabaseContext.GetContext( );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.FromTypeUid ), "Type A was not deployed as part of an existing application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.ToTypeUid ), "Type B was not deployed as part of an existing application." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantEntity( context, tenantId, application.RelationshipUid ), "Relationship was not deployed as part of an existing application." );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, AppDetails.CardinalityUid, application.RelationshipUid, AppDetails.ConvertCardinalityEnumToUpgradeId( cardinality ) ), "Relationship cardinality was not deployed." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.Bit, application.RelationshipUid, AppDetails.CascadeDeleteUid, cascadeDelete ), "Relationship cascadeDelete value was not deployed." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.Bit, application.RelationshipUid, AppDetails.CascadeDeleteToUid, cascadeDeleteTo ), "Relationship cascadeDelete to value was not deployed." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.Bit, application.RelationshipUid, AppDetails.ImplicitInSolutionUid, implicitInSolution ), "Relationship implicitInSolution value was not deployed." );
			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantFieldValue( context, tenantId, DataTableType.Bit, application.RelationshipUid, AppDetails.ReverseImplicitInSolutionUid, reverseImplicitInSolution ), "Relationship reverseImplicitInSolution value was not deployed." );
		}

		/// <summary>
		///     News the application_ add new relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="cascadeDelete">if set to <c>true</c> [cascade delete].</param>
		/// <param name="cascadeDeleteTo">if set to <c>true</c> [cascade delete to].</param>
		/// <param name="implicitInSolution">if set to <c>true</c> [implicit in solution].</param>
		/// <param name="reverseImplicitInSolution">if set to <c>true</c> [reverse implicit in solution].</param>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_AddNewRelationship_ManyToMany(
			[Values( CardinalityEnum_Enumeration.ManyToMany )] CardinalityEnum_Enumeration cardinality,
			[Values( true, false )] bool cascadeDelete,
			[Values( true, false )] bool cascadeDeleteTo,
			[Values( true, false )] bool implicitInSolution,
			[Values( true, false )] bool reverseImplicitInSolution
			)
		{
			NewApplication_AddNewRelationshipImpl( cardinality, cascadeDelete, cascadeDeleteTo, implicitInSolution, reverseImplicitInSolution );
		}

		/// <summary>
		///     News the application_ add new relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="cascadeDelete">if set to <c>true</c> [cascade delete].</param>
		/// <param name="cascadeDeleteTo">if set to <c>true</c> [cascade delete to].</param>
		/// <param name="implicitInSolution">if set to <c>true</c> [implicit in solution].</param>
		/// <param name="reverseImplicitInSolution">if set to <c>true</c> [reverse implicit in solution].</param>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_AddNewRelationship_ManyToOne(
			[Values( CardinalityEnum_Enumeration.ManyToOne )] CardinalityEnum_Enumeration cardinality,
			[Values( true, false )] bool cascadeDelete,
			[Values( true, false )] bool cascadeDeleteTo,
			[Values( true, false )] bool implicitInSolution,
			[Values( true, false )] bool reverseImplicitInSolution
			)
		{
			NewApplication_AddNewRelationshipImpl( cardinality, cascadeDelete, cascadeDeleteTo, implicitInSolution, reverseImplicitInSolution );
		}

		/// <summary>
		///     News the application_ add new relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="cascadeDelete">if set to <c>true</c> [cascade delete].</param>
		/// <param name="cascadeDeleteTo">if set to <c>true</c> [cascade delete to].</param>
		/// <param name="implicitInSolution">if set to <c>true</c> [implicit in solution].</param>
		/// <param name="reverseImplicitInSolution">if set to <c>true</c> [reverse implicit in solution].</param>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_AddNewRelationship_OneToMany(
			[Values( CardinalityEnum_Enumeration.OneToMany )] CardinalityEnum_Enumeration cardinality,
			[Values( true, false )] bool cascadeDelete,
			[Values( true, false )] bool cascadeDeleteTo,
			[Values( true, false )] bool implicitInSolution,
			[Values( true, false )] bool reverseImplicitInSolution
			)
		{
			NewApplication_AddNewRelationshipImpl( cardinality, cascadeDelete, cascadeDeleteTo, implicitInSolution, reverseImplicitInSolution );
		}

		/// <summary>
		///     News the application_ add new relationship.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="cascadeDelete">if set to <c>true</c> [cascade delete].</param>
		/// <param name="cascadeDeleteTo">if set to <c>true</c> [cascade delete to].</param>
		/// <param name="implicitInSolution">if set to <c>true</c> [implicit in solution].</param>
		/// <param name="reverseImplicitInSolution">if set to <c>true</c> [reverse implicit in solution].</param>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_AddNewRelationship_OneToOne(
			[Values( CardinalityEnum_Enumeration.OneToOne )] CardinalityEnum_Enumeration cardinality,
			[Values( true, false )] bool cascadeDelete,
			[Values( true, false )] bool cascadeDeleteTo,
			[Values( true, false )] bool implicitInSolution,
			[Values( true, false )] bool reverseImplicitInSolution
			)
		{
			NewApplication_AddNewRelationshipImpl( cardinality, cascadeDelete, cascadeDeleteTo, implicitInSolution, reverseImplicitInSolution );
		}

		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void NewApplication_Relationship_LastInWins( )
		{
			Solution solutionA = TestHelper.CreateSolution( "Test Solution A" );
			solutionA.Save( );

			Solution solutionB = TestHelper.CreateSolution( "Test Solution B" );
			solutionB.Save( );

			EntityType typeA = TestHelper.CreateEntityType( "Type A", null, null, solutionA );
			typeA.Save( );

			EntityType typeB = TestHelper.CreateEntityType( "Type B", null, null, solutionA );
			typeB.Save( );

			Relationship relationship = TestHelper.CreateRelationship( "Test Relationship", null, typeA, typeB, solutionA );
			relationship.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
			relationship.Save( );

			var typeAInstance = ( Entity ) TestHelper.CreateInstance( typeA, "TypeA Instance", null, solutionA );
			typeAInstance.Save( );

			var typeBInstance = ( Entity ) TestHelper.CreateInstance( typeB, "TypeB Instance", null, solutionA );
			typeBInstance.Save( );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeBInstance
			} );
			typeAInstance.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionA.Name );

			var typeBInstance2 = ( Entity ) TestHelper.CreateInstance( typeB, "TypeB Instance", null, solutionB );
			typeBInstance.Save( );

			typeAInstance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
			{
				typeBInstance2
			} );
			typeAInstance.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, solutionB.Name );

			Guid typeAInstanceUpgradeId = typeAInstance.UpgradeId;
			Guid typeBInstanceUpgradeId = typeBInstance.UpgradeId;
			Guid typeB2InstanceUpgradeId = typeBInstance2.UpgradeId;
			Guid relationshipUpgradeId = relationship.UpgradeId;

			const string tenantName = "ZZZ";
			DatabaseContext context = DatabaseContext.GetContext( );

			long tenantId = TestHelper.CreateTenant( tenantName, null );

			AppManager.DeployApp( tenantName, Applications.CoreApplicationId.ToString( "B" ) );

			AppManager.DeployApp( tenantName, solutionA.Name, "1.0" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeBInstanceUpgradeId ) );

			AppManager.DeployApp( tenantName, solutionB.Name, "1.0" );

			Assert.IsTrue( TenantMergeProcessorTestHelper.ConfirmTenantRelationship( context, tenantId, Direction.Forward, relationshipUpgradeId, typeAInstanceUpgradeId, typeB2InstanceUpgradeId ) );
		}
	}
}