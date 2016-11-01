// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Multiple Inheritance tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class MultipleInheritanceTests
	{
		/// <summary>
		///     Creates the entity with multiple base classes.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void CreateEntityWithMultipleBaseClasses( )
		{
			const string name = "Folder and Report";

			var folderReport = new Entity( new[]
				{
					new EntityRef( "core", "folder" ), new EntityRef( "core", "report" )
				} );

			folderReport.Save( );

			folderReport.SetField( Report.Name_Field, name );
            folderReport.SetField(Report.RollupGrandTotals_Field, true);

			var chart = folderReport.As<Folder>( );

			Assert.IsTrue( folderReport.GetField<string>(Folder.Name_Field ) == chart.Name );
			Assert.IsTrue( chart.Name == name );

			var report = folderReport.As<Report>( );

            Assert.IsTrue(folderReport.GetField<bool>(Report.RollupGrandTotals_Field) == report.RollupGrandTotals);
            Assert.IsTrue(report.RollupGrandTotals == true);

			report.Save( );

            IEntity blankEntity = Entity.Get(report.Id, Folder.Name_Field, Report.RollupGrandTotals_Field);

            Assert.IsTrue(blankEntity.GetField<string>(Folder.Name_Field) == name);
            Assert.IsTrue(blankEntity.GetField<bool>(Report.RollupGrandTotals_Field));

			IEnumerable<EntityType> types = blankEntity.EntityTypes.Cast<EntityType>( );

			IList<EntityType> entityTypes = types as IList<EntityType> ?? types.ToList( );
			Assert.IsTrue( entityTypes.Count( ) == 2 );
			Assert.IsTrue( entityTypes.Any( et => et.Name == "Folder" ) );
			Assert.IsTrue( entityTypes.Any( et => et.Name == "Report" ) );

			Assert.IsTrue( blankEntity.TypeIds.Count( ) == 2 );
		}


		[Test]
		[RunAsDefaultTenant]
		public void TestCreateSaveVariant1( )
		{
			IEntity e = Entity.Create( typeof ( EntityType ) );

			e.Save( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestCreateSaveVariant2( )
		{
			var e2 = Entity.Create( typeof ( EntityType ) ).As<Entity>( );

			e2.Save( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestCreateSaveVariant3( )
		{
			var e3 = Entity.Create( typeof ( EntityType ) ).As<EntityType>( );

			e3.Save( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestCreateSaveVariant4( )
		{
			IEntity e3 = Entity.Create( typeof ( EntityType ) );

			var et = e3.As<EntityType>( );

			et.Save( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void Test_TempEntityHasTypeIdsAndIsOfType( )
		{
			var p = new Person( );

			IEntityCollection<EntityType> isOfType = p.IsOfType;
			Assert.IsTrue( isOfType.Count > 0, "Temp item should have at least on isOfType entry" );

			IEnumerable<long> typeIds = p.TypeIds;
			Assert.IsTrue( typeIds.Any( ), "Temp item should have at least one typeId" );
		}

		[Test]
		[RunAsDefaultTenant]
		public void Test_TempEntityHasTypeIdsAndIsOfType2( )
		{
			var personTypeRef = new EntityRef( "core", "person" );
			IEntity p = new Entity( personTypeRef );

			IEnumerable<long> typeIds = p.TypeIds;
			Assert.IsTrue( typeIds.Any( ), "Temp item should have at least one typeId" );

			p.Save( );
		}
	}
}