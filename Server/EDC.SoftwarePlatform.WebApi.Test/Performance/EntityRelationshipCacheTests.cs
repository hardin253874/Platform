// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.ReadiNow.Test.Model
// ReSharper restore CheckNamespace
{
	/// <summary>
	///     Ensuring that the entity relationship write and read caches work as expected
	/// </summary>
	[TestFixture]
	public class EntityRelationshipCacheTests
	{
		/// <summary>
		///     Automated test reproducing this issue that poped up when optimization on the cache was done:
		///     1.	Create a new definition
		///     2.	Name it ‘Test’
		///     3.	Press Next
		///     4.	Press ‘new field group’
		///     5.	Name it ‘FG1’, but don’t close the window yet
		///     6.	Press ‘new field’  (from within the new field group window)
		///     7.	Name it ‘Field 1’, and make it a string field.
		///     8.	Press Apply to close field
		///     9.	Press Apply to close field group
		///     10.	Press Next/Finish to close definition
		///     11.	Try to re-open the definition
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreateNewTypeWithFieldAndReopenTest( )
		{
			var saveList = new List<IEntity>( );

			try
			{
				var newType = new EntityType
				{
					Name = "TestCreateNewTypeWithFieldAndReopenTest"
				};
				newType.Inherits.Add( Entity.Get<EntityType>( new EntityRef( "core", "userResource" ) ) );
				saveList.Add( newType );

				var newGroup = new FieldGroup
				{
					Name = "newGroup",
					FieldGroupBelongsToType = newType
				};
				saveList.Add( newGroup );

				var newField = new Field
				{
					Name = "newField"
				};
				newField.IsOfType.Add( Entity.Get<EntityType>( new EntityRef( "core", "stringField" ) ) );
				newField.FieldIsOnType = newType;
				newField.FieldInGroup = newGroup;
				saveList.Add( newField );

				Entity.Save( saveList );

				var readNewType = Entity.Get<EntityType>( newType.Id );

				Assert.IsNotNull( readNewType.Fields[ 0 ].FieldIsOnType, "the reverse relationship should be populated" );
			}
			finally
			{
				Entity.Delete( saveList.Select( e => e.Id ) );
			}
		}

		/// <summary>
		///     Ensure that if someone has a reference to an interator over a relationship, a write to that relationship will not
		///     break the iterator.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestRelationshipEnumeratorForReadableRead( )
		{
			var deleteList = new List<EntityRef>( );
			try
			{
				var edc = new Report
				{
					Name = "EDC"
				};
				edc.Save( );
				deleteList.Add( edc );

				var jude = new ReportColumn
				{
					Name = "Jude",
					Alias = "edc:jude",
					ColumnForReport = edc
				};
				jude.Save( );
				deleteList.Add( jude );

				var pete = new ReportColumn
				{
					Name = "Pete",
					Alias = "edc:pete",
					ColumnForReport = edc
				};
				pete.Save( );
				deleteList.Add( pete );

				Assert.AreEqual( 2, edc.ReportColumns.Count );

				IEnumerator<ReportColumn> inumerator = edc.ReportColumns.GetEnumerator( );

				var writableEdc = Entity.Get<Report>( edc.Id, true );

				writableEdc.ReportColumns.Clear( );
				writableEdc.Save( );

				int count = 0;
				while ( inumerator.MoveNext( ) )
				{
					count++;
					ReportColumn referenced = inumerator.Current;
					Assert.IsNotNull( referenced, "The referenced object is still there." );
				}

				Assert.AreEqual( 2, count, "We still have two things in the iterator." );
			}
			finally
			{
				Entity.Delete( deleteList );
			}
		}


		/// <summary>
		///     Ensure that if someone has a reference to an interator over a relationship, a write to that relationship will not
		///     break the iterator.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestRelationshipEnumeratorForRepeatableRead( )
		{
			var deleteList = new List<EntityRef>( );
			try
			{
				var edc = new Report
				{
					Name = "EDC"
				};
				edc.Save( );
				deleteList.Add( edc );

				var jude = new ReportColumn
				{
					Name = "Jude",
					Alias = "edc:jude",
					ColumnForReport = edc
				};
				jude.Save( );
				deleteList.Add( jude );

				var pete = new ReportColumn
				{
					Name = "Pete",
					Alias = "edc:pete",
					ColumnForReport = edc
				};
				pete.Save( );
				deleteList.Add( pete );

				Assert.AreEqual( 2, edc.ReportColumns.Count );

				IEnumerator<ReportColumn> inumerator = edc.ReportColumns.GetEnumerator( );

				var writableEdc = Entity.Get<Report>( edc.Id, true );

				writableEdc.ReportColumns.Clear( );
				writableEdc.Save( );

				int count = 0;
				while ( inumerator.MoveNext( ) )
				{
					count++;
					ReportColumn referenced = inumerator.Current;
					Assert.IsNotNull( referenced, "The referenced object is still there." );
				}

				Assert.AreEqual( 2, count, "We still have two things in the iterator." );
			}
			finally
			{
				Entity.Delete( deleteList );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestWritableRelationshipsClearTheReadCacheOnSave( )
		{
			var deleteList = new List<EntityRef>( );
			try
			{
				var edc = new Report
				{
					Name = "EDC"
				};
				edc.Save( );
				deleteList.Add( edc );

				var jude = new ReportColumn
				{
					Name = "Jude",
					Alias = "edc:jude"
				};
				jude.Save( );
				deleteList.Add( jude );

				var writeableJude = Entity.Get<ReportColumn>( jude.Id, true );
				edc = Entity.Get<Report>( edc.Id );

				writeableJude.ColumnForReport = edc;

				var shadowJude = Entity.Get<ReportColumn>( jude.Id );
				Assert.AreNotEqual( shadowJude.ColumnForReport, writeableJude.ColumnForReport, "writable copies are not shared" );

				writeableJude.Save( );

				Assert.IsNotNull( shadowJude.ColumnForReport, "Readable relationships should be updated when a write on the related obejects occur." );

				var readableJude = Entity.Get<ReportColumn>( jude.Id );

				Assert.IsNotNull( readableJude.ColumnForReport, "Should have been updated" );

				Assert.AreEqual( shadowJude.ColumnForReport.Id, readableJude.ColumnForReport.Id, "Readable copies are shared and are flushed and updated when saves occur that overlap with them" );
			}
			finally
			{
				Entity.Delete( deleteList );
			}
		}
	}
}