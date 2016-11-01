// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Sources;
using NUnit.Framework;
using EDC.ReadiNow.Database;

namespace EDC.SoftwarePlatform.Migration.Test.Publish
{
	[TestFixture]
	[Category( "ExtendedTests" )]
	[Category( "AppLibraryTests" )]
	public class PublishTests : TestBase
	{
		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <param name="report">The report.</param>
		/// <param name="countId">The count identifier.</param>
		/// <returns></returns>
		private int GetCount( StatisticsReport report, string countId )
		{
			string lookup = null;

			switch ( countId )
			{
				case "SourceEntityCount":
					lookup = "Current Application Entities";
					break;
				case "TargetEntityCount":
					lookup = "Copied Entities";
					break;
				case "SourceRelationshipCount":
					lookup = "Current Application Relationships";
					break;
				case "TargetRelationshipCount":
					lookup = "Copied Relationships";
					break;
				case "SourceEntityDataCount_NVarChar":
					lookup = "Copied NVarChar Data";
					break;
			}

			StatisticsCount count = report.Counts.FirstOrDefault( pair => pair.Name == lookup );

			if ( count != null )
			{
				return count.Count;
			}

			return -1;
		}

		private int GetSourceDataCount( StatisticsReport report )
		{
			return report.Counts.Count( pair => pair.Name.StartsWith( "Current Application" ) && pair.Name.EndsWith( "Data" ) );
		}

		private int GetTargetDataCount( StatisticsReport report )
		{
			return report.Counts.Count( pair => pair.Name.StartsWith( "Copied" ) && pair.Name.EndsWith( "Data" ) );
		}


		/// <summary>
		///     Basics the publish.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void BasicPublish( )
		{
			Solution solution = TestHelper.CreateSolution( );

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                solution.Save();

                ctx.CommitTransaction();
            }            

			List<IEntity> entities = TestHelper.PopulateBasicSolution( solution );

			var context = new ProcessingContext( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetSourceDataCount( context.Report ), GetTargetDataCount( context.Report ) );

			Assert.Greater( GetCount( context.Report, "SourceEntityCount" ), 4 ); // Solution + DummyType + 2 Instances
            Assert.Greater(GetCount(context.Report, "SourceRelationshipCount"), 23); // ( isOfType, inSolution, createdBy, modifiedBy, createdDate, modifiedDate ) * 4 - resourceInFolder
			Assert.Greater( GetCount( context.Report, "SourceEntityDataCount_NVarChar"), 8 ); // ( name, description ) * 4

			List<Guid> addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

            foreach ( IEntityInternal entity in entities.Select( e => e as IEntityInternal ) )
			{
				Assert.Contains( entity.UpgradeId, addedEntities );
			}
		}

		[Test]
		[RunAsDefaultTenant]
        public void RePublish()
		{
			Solution solution = TestHelper.CreateSolution( );

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                solution.Save();

                ctx.CommitTransaction();
            }            

			List<IEntity> entities = TestHelper.PopulateBasicSolution( solution );

			var context = new ProcessingContext( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceEntityDataCount" ), GetCount( context.Report, "TargetEntityDataCount" ) );

			Assert.Greater( GetCount( context.Report, "SourceEntityCount" ), 4 ); // Solution + DummyType + 2 Instances
            Assert.Greater(GetCount(context.Report, "SourceRelationshipCount"), 23); // ( isOfType, inSolution, createdBy, modifiedBy, createdDate, modifiedDate ) * 4
			Assert.Greater( GetCount( context.Report, "SourceEntityDataCount_NVarChar"), 8 ); // ( name, description ) * 4

			List<Guid> addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

            foreach ( IEntityInternal entity in entities.Select( e => e as IEntityInternal ) )
			{
				Assert.Contains( entity.UpgradeId, addedEntities );
			}

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceEntityDataCount" ), GetCount( context.Report, "TargetEntityDataCount" ) );

			Assert.Greater( GetCount( context.Report, "SourceEntityCount" ), 4 ); // Solution + DummyType + 2 Instances
            Assert.Greater(GetCount(context.Report, "SourceRelationshipCount"), 23); // ( isOfType, inSolution, createdBy, modifiedBy, createdDate, modifiedDate ) * 4
			Assert.Greater( GetCount( context.Report, "SourceEntityDataCount_NVarChar"), 8 ); // ( name, description ) * 4

			addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

            foreach ( IEntityInternal entity in entities.Select( e => e as IEntityInternal ) )
			{
				Assert.Contains( entity.UpgradeId, addedEntities );
			}
		}

		[Test]
		[RunAsDefaultTenant]
        public void InvalidVersionNumber()
		{
			Solution solution = TestHelper.CreateSolution( );

			solution.SolutionVersionString = "Test";

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                solution.Save();

                ctx.CommitTransaction();
            }

            List<IEntity> entities = TestHelper.PopulateBasicSolution( solution );

			var context = new ProcessingContext( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceEntityDataCount" ), GetCount( context.Report, "TargetEntityDataCount" ) );

			Assert.Greater( GetCount( context.Report, "SourceEntityCount" ), 4 ); // Solution + DummyType + 2 Instances
            Assert.Greater(GetCount(context.Report, "SourceRelationshipCount"), 23); // ( isOfType, inSolution, createdBy, modifiedBy, createdDate, modifiedDate ) * 4
			Assert.Greater( GetCount( context.Report, "SourceEntityDataCount_NVarChar"), 8 ); // ( name, description ) * 4

			List<Guid> addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

            foreach ( IEntityInternal entity in entities.Select( e => e as IEntityInternal ) )
			{
				Assert.Contains( entity.UpgradeId, addedEntities );
			}

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );
		}

		[Test]
		[RunAsDefaultTenant]
        public void CircularReference()
		{
			Solution solution = TestHelper.CreateSolution( );

			solution.Save( );

			EntityType entityType1 = TestHelper.CreateEntityType( "Type1", "Type1 Desc", null, solution );
			EntityType entityType2 = TestHelper.CreateEntityType( "Type2", "Type2 Desc", null, solution );

			Relationship fowardRelationship = TestHelper.CreateRelationship( "Dummy Relationship", "My Dummy Relationship", entityType1, entityType2, solution, true, true );
			fowardRelationship.Save( );

			Relationship reverseRelationship = TestHelper.CreateRelationship( "Dummy Relationship", "My Dummy Relationship", entityType2, entityType1, solution, true, true );
			reverseRelationship.Save( );

			IEntity entityType1Instance = TestHelper.CreateInstance( entityType1, "Instance1", "Instance1 Desc" );
			entityType1Instance.Save( );

			IEntity entityType2Instance = TestHelper.CreateInstance( entityType2, "Instance2", "Instance2 Desc", solution );
			entityType2Instance.Save( );

			entityType1Instance.SetRelationships( fowardRelationship, new EntityRelationshipCollection<IEntity>
				{
					entityType2Instance
				} );

			entityType1Instance.Save( );

			entityType2Instance.SetRelationships( reverseRelationship, new EntityRelationshipCollection<IEntity>
				{
					entityType1Instance
				} );

			entityType2Instance.Save( );

			var context = new ProcessingContext( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceEntityDataCount" ), GetCount( context.Report, "TargetEntityDataCount" ) );

			List<Guid> addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

			Assert.Contains( ( ( Entity ) entityType1Instance ).UpgradeId, addedEntities );
			Assert.Contains( ( ( Entity ) entityType2Instance ).UpgradeId, addedEntities );
		}

		[Test]
		[RunAsDefaultTenant]
        public void ImplicitInSolution()
		{
			Solution solution = TestHelper.CreateSolution( );

			solution.Save( );

			EntityType entityType1 = TestHelper.CreateEntityType( "Type1", "Type1 Desc", null, solution );
			EntityType entityType2 = TestHelper.CreateEntityType( "Type2", "Type2 Desc", null, solution );

			Relationship relationship = TestHelper.CreateRelationship( "Dummy Relationship", "My Dummy Relationship", entityType1, entityType2, solution, true );
			relationship.Save( );

			IEntity entityType1Instance = TestHelper.CreateInstance( entityType1, "Instance1", "Instance1 Desc", solution );
			entityType1Instance.Save( );

			IEntity entityType2Instance = TestHelper.CreateInstance( entityType2, "Instance2", "Instance2 Desc" );

			entityType1Instance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
				{
					entityType2Instance
				} );

			entityType1Instance.Save( );

			var context = new ProcessingContext( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceEntityDataCount" ), GetCount( context.Report, "TargetEntityDataCount" ) );

			List<Guid> addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

			Assert.Contains( ( ( Entity ) entityType1Instance ).UpgradeId, addedEntities );
			Assert.Contains( ( ( Entity ) entityType2Instance ).UpgradeId, addedEntities );
		}

		[Test]
		[RunAsDefaultTenant]
        public void ImplicitInSolutionRecursive()
		{
			Solution solution = TestHelper.CreateSolution( );

			solution.Save( );

			EntityType entityType1 = TestHelper.CreateEntityType( "Type1", "Type1 Desc", null, solution );
			EntityType entityType2 = TestHelper.CreateEntityType( "Type2", "Type2 Desc", null, solution );
			EntityType entityType3 = TestHelper.CreateEntityType( "Type3", "Type3 Desc", null, solution );

			Relationship relationship1 = TestHelper.CreateRelationship( "Dummy Relationship 1", "My Dummy Relationship 1", entityType1, entityType2, solution, true );
			relationship1.Save( );

			Relationship relationship2 = TestHelper.CreateRelationship( "Dummy Relationship 2", "My Dummy Relationship 2", entityType2, entityType3, solution, true );
			relationship2.Save( );

			IEntity entityType1Instance = TestHelper.CreateInstance( entityType1, "Instance1", "Instance1 Desc", solution );
			entityType1Instance.Save( );

			IEntity entityType2Instance = TestHelper.CreateInstance( entityType2, "Instance2", "Instance2 Desc" );

			entityType1Instance.SetRelationships( relationship1, new EntityRelationshipCollection<IEntity>
				{
					entityType2Instance
				} );

			entityType1Instance.Save( );

			IEntity entityType3Instance = TestHelper.CreateInstance( entityType2, "Instance3", "Instance3 Desc" );

			entityType2Instance.SetRelationships( relationship2, new EntityRelationshipCollection<IEntity>
				{
					entityType3Instance
				} );

			entityType2Instance.Save( );

			var context = new ProcessingContext( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceEntityDataCount" ), GetCount( context.Report, "TargetEntityDataCount" ) );

			List<Guid> addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

			Assert.Contains( ( ( Entity ) entityType1Instance ).UpgradeId, addedEntities );
			Assert.Contains( ( ( Entity ) entityType2Instance ).UpgradeId, addedEntities );
			Assert.Contains( ( ( Entity ) entityType3Instance ).UpgradeId, addedEntities );
		}

		[Test]
		[RunAsDefaultTenant]
		public void RelationshipEntityData( )
		{
		}

		[Test]
		[RunAsDefaultTenant]
		public void RelationshipInstanceRelationships( )
		{
		}

		[Test]
		[RunAsDefaultTenant]
		public void RelationshipInstances( )
		{
		}

		[Test]
		[RunAsDefaultTenant]
        public void ReverseImplicitInSolution()
		{
			Solution solution = TestHelper.CreateSolution( );

			solution.Save( );

			EntityType entityType1 = TestHelper.CreateEntityType( "Type1", "Type1 Desc", null, solution );
			EntityType entityType2 = TestHelper.CreateEntityType( "Type2", "Type2 Desc", null, solution );

			Relationship relationship = TestHelper.CreateRelationship( "Dummy Relationship", "My Dummy Relationship", entityType1, entityType2, solution, false, true );
			relationship.Save( );

			IEntity entityType1Instance = TestHelper.CreateInstance( entityType1, "Instance1", "Instance1 Desc" );
			entityType1Instance.Save( );

			IEntity entityType2Instance = TestHelper.CreateInstance( entityType2, "Instance2", "Instance2 Desc", solution );

			entityType1Instance.SetRelationships( relationship, new EntityRelationshipCollection<IEntity>
				{
					entityType2Instance
				} );

			entityType1Instance.Save( );

			var context = new ProcessingContext( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceEntityDataCount" ), GetCount( context.Report, "TargetEntityDataCount" ) );

			List<Guid> addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

			Assert.Contains( ( ( Entity ) entityType1Instance ).UpgradeId, addedEntities );
			Assert.Contains( ( ( Entity ) entityType2Instance ).UpgradeId, addedEntities );
		}

		[Test]
		[RunAsDefaultTenant]
        public void ReverseImplicitInSolutionRecursive()
		{
			Solution solution = TestHelper.CreateSolution( );

			solution.Save( );

			EntityType entityType1 = TestHelper.CreateEntityType( "Type1", "Type1 Desc", null, solution );
			EntityType entityType2 = TestHelper.CreateEntityType( "Type2", "Type2 Desc", null, solution );
			EntityType entityType3 = TestHelper.CreateEntityType( "Type3", "Type3 Desc", null, solution );

			Relationship relationship1 = TestHelper.CreateRelationship( "Dummy Relationship 1", "My Dummy Relationship 1", entityType1, entityType2, solution, false, true );
			relationship1.Save( );

			Relationship relationship2 = TestHelper.CreateRelationship( "Dummy Relationship 2", "My Dummy Relationship 2", entityType2, entityType3, solution, false, true );
			relationship2.Save( );

			IEntity entityType1Instance = TestHelper.CreateInstance( entityType1, "Instance1", "Instance1 Desc" );
			entityType1Instance.Save( );

			IEntity entityType2Instance = TestHelper.CreateInstance( entityType2, "Instance2", "Instance2 Desc" );

			entityType1Instance.SetRelationships( relationship1, new EntityRelationshipCollection<IEntity>
				{
					entityType2Instance
				} );

			entityType1Instance.Save( );

			IEntity entityType3Instance = TestHelper.CreateInstance( entityType2, "Instance3", "Instance3 Desc", solution );

			entityType2Instance.SetRelationships( relationship2, new EntityRelationshipCollection<IEntity>
				{
					entityType3Instance
				} );

			entityType2Instance.Save( );

			var context = new ProcessingContext( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName, context );

			Assert.IsNotNull( context );
			Assert.AreEqual( context.Report.FailedEntity.Count, 0 );
			Assert.AreEqual( context.Report.FailedRelationship.Count, 0 );
			Assert.AreEqual( context.Report.FailedEntityData.Count, 0 );

			Assert.AreEqual( GetCount( context.Report, "SourceEntityCount" ), GetCount( context.Report, "TargetEntityCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceRelationshipCount" ), GetCount( context.Report, "TargetRelationshipCount" ) );
			Assert.AreEqual( GetCount( context.Report, "SourceEntityDataCount" ), GetCount( context.Report, "TargetEntityDataCount" ) );

			List<Guid> addedEntities = context.Report.AddedEntities.Select( e => e.EntityId ).ToList( );

			Assert.Contains( ( ( Entity ) entityType1Instance ).UpgradeId, addedEntities );
			Assert.Contains( ( ( Entity ) entityType2Instance ).UpgradeId, addedEntities );
			Assert.Contains( ( ( Entity ) entityType3Instance ).UpgradeId, addedEntities );
		}

		[Test]
		[RunAsDefaultTenant]
        public void SaveEmptySolution()
		{
			Solution solution = TestHelper.CreateSolution( );
			solution.Save( );
			long solutionId = solution.Id;

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, TestHelper.DefaultSolutionName );

			solution = Entity.Get<Solution>( solutionId );

			Assert.IsNotNull( solution );
			Assert.IsNotNull( solution.PackageId );

			if ( solution.PackageId != null )
			{
				Guid packageId = solution.PackageId.Value;

				using ( new GlobalAdministratorContext( ) )
				{
					AppPackage package = SystemHelper.GetPackageByVerId( packageId );

					Assert.IsNotNull( package );
					Assert.IsNotNull( package.AppVerId );
					Assert.AreEqual( "1.0.0.0", package.AppVersionString );
					Assert.AreEqual( string.Format( "{0} Application Package {1}", TestHelper.DefaultSolutionName, "1.0.0.0" ), package.Name );
					Assert.AreEqual( string.Format( "Application Package for version {1} of {0}.", TestHelper.DefaultSolutionName, "1.0.0.0" ), package.Description );

					if ( package.AppVerId != null )
					{
						var source = new LibraryAppSource
						{
							AppId = package.AppVerId.Value
						};

						IEnumerable<EntityEntry> entities = source.GetEntities( null );

						Assert.IsFalse( entities.Any( ) );

						IEnumerable<RelationshipEntry> relationships = source.GetRelationships( null );

						Assert.IsFalse( relationships.Any( ) );

						foreach ( string fieldDataTable in Helpers.FieldDataTables )
						{
							IEnumerable<DataEntry> fieldData = source.GetFieldData( fieldDataTable, null );

							Assert.IsFalse( fieldData.Any( ) );
						}
					}
				}
			}
		}
	}
}