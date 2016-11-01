// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Threading;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Internal;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     EntitySnapshotContextTests tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class EntitySnapshotContextTests
	{
		/// <summary>
		///     Tests the entity snapshot context is caching a read only entity.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCachingReadOnlyEntity( )
		{
			using ( new EntitySnapshotContext( ) )
			{
				var nameField = Entity.Get<Field>( new EntityRef( "core", "name" ) );

				EntitySnapshotContextData snaphotData = EntitySnapshotContext.GetContextData( );

				IEntity cachedEntity;
				Assert.IsTrue( snaphotData.TryGetEntity( nameField.Id, out cachedEntity ), "The entity should be cached" );
			}
		}


		/// <summary>
		///     Tests the entity snapshot context is caching a read only entity.
		///     and ignoring any writes to that entity
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCachingReadOnlyEntityIgnoreWrites( )
		{
            // Note: in order to remove dependencies on 'Shared' objects, the following changes were made in this test:
            // 'Organisation' got changed to 'Report'
            // 'Employee' got changed to 'ReportColumn'
            // 'WorksFor' got changed to 'ColumnForReport' 

			var organisationA = new Report( );
			organisationA.Save( );

            var organisationB = new Report();
			organisationB.Save( );

            var organisationC = new Report();
			organisationC.Save( );

			var employee = new ReportColumn
				{
					Name = "Initial Name",
					ColumnForReport = organisationA
				};
			employee.Save( );

			using ( new EntitySnapshotContext( ) )
			{
				// This will cache the employee
				var readOnlyEmployee = Entity.Get<ReportColumn>( employee.Id );

				EntitySnapshotContextData snaphotData = EntitySnapshotContext.GetContextData( );

				IEntity cachedEntity;
				Assert.IsTrue( snaphotData.TryGetEntity( readOnlyEmployee.Id, out cachedEntity ), "The entity should be cached" );
				Assert.AreEqual( "Initial Name", readOnlyEmployee.Name, "The name of the read only employee is invalid" );
                Assert.AreEqual(organisationA.Id, readOnlyEmployee.ColumnForReport.Id, "The organisation of the read only employee is invalid");

				// Get a writable copy and change the name & organisation
                var writeableEmployee = readOnlyEmployee.AsWritable<ReportColumn>();
				writeableEmployee.Name = "New Name";
                writeableEmployee.ColumnForReport = organisationB;
				writeableEmployee.Save( );

				// Ensure that requesting a read only employee returns the cached one
                readOnlyEmployee = Entity.Get<ReportColumn>(employee.Id);
				Assert.AreEqual( "Initial Name", readOnlyEmployee.Name );
                Assert.AreEqual(organisationA.Id, readOnlyEmployee.ColumnForReport.Id, "The organisation of the read only employee is invalid");

				// Ensure that requesting a writeable employee returns the new one
                writeableEmployee = Entity.Get<ReportColumn>(employee.Id, true);
				Assert.AreEqual( "New Name", writeableEmployee.Name );
				Assert.AreEqual( organisationB.Id, writeableEmployee.ColumnForReport.Id, "The organisation of the writeable employee is invalid" );
			}

			// This will cache the employee
            var readOnlyEmployee2 = Entity.Get<ReportColumn>(employee.Id);
			Assert.AreEqual( "New Name", readOnlyEmployee2.Name );
            Assert.AreEqual(organisationB.Id, readOnlyEmployee2.ColumnForReport.Id, "The organisation of the read only employee is invalid");

			// Get a writable copy and change the name
            var writeableEmployee2 = readOnlyEmployee2.AsWritable<ReportColumn>();
			writeableEmployee2.Name = "New Name2";
            writeableEmployee2.ColumnForReport = organisationC;
			writeableEmployee2.Save( );

			// Ensure that requesting a read only employee returns the new one
            readOnlyEmployee2 = Entity.Get<ReportColumn>(employee.Id);
			Assert.AreEqual( "New Name2", readOnlyEmployee2.Name );
            Assert.AreEqual(organisationC.Id, readOnlyEmployee2.ColumnForReport.Id, "The organisation of the read only employee is invalid");

			// Ensure that requesting a writeable employee returns the new one
            writeableEmployee2 = Entity.Get<ReportColumn>(employee.Id, true);
			Assert.AreEqual( "New Name2", writeableEmployee2.Name );
            Assert.AreEqual(organisationC.Id, writeableEmployee2.ColumnForReport.Id, "The organisation of the writeable employee is invalid");
		}


		/// <summary>
		///     Test that multiple threads using independent caches.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
        [Ignore("Failing. No idea why.")]
		public void TestMultipleThreads( )
		{
			bool thread1Failed = true;
			bool thread2Failed = true;

			var startEvent = new ManualResetEvent( false );
			var thread1GotNameFieldEvent = new ManualResetEvent( false );
			var thread2GotDescriptionFieldEvent = new ManualResetEvent( false );

		    string thread1Result = string.Empty;
            string thread2Result = string.Empty;

			var thread1 = new Thread( ( ) =>
				{
					startEvent.WaitOne( );

					var descriptionField = Entity.Get<Field>( new EntityRef( "core", "description" ) );

					using ( new EntitySnapshotContext( ) )
					{
						// The name field should be cached for this thread
						var nameField = Entity.Get<Field>( new EntityRef( "core", "name" ) );

						EntitySnapshotContextData snaphotData = EntitySnapshotContext.GetContextData( );

						// Wait for thread2 to have the description field cached
						thread1GotNameFieldEvent.Set( );
						thread2GotDescriptionFieldEvent.WaitOne( );

						IEntity cachedEntity;
						if(!snaphotData.TryGetEntity( nameField.Id, out cachedEntity ) )
						{
						    thread1Result = "The name entity should be cached";
						}
						else if(snaphotData.TryGetEntity( descriptionField.Id, out cachedEntity ))
						{
						    thread1Result = "The description entity should not be cached";
						}
						else
						{
                            thread1Failed = false;
						}
					}
				} );

			var thread2 = new Thread( ( ) =>
				{
					startEvent.WaitOne( );

					var nameField = Entity.Get<Field>( new EntityRef( "core", "name" ) );

					using ( new EntitySnapshotContext( ) )
					{
						// The description field should be cached for this thread
						var descriptionField = Entity.Get<Field>( new EntityRef( "core", "description" ) );

						EntitySnapshotContextData snaphotData = EntitySnapshotContext.GetContextData( );

						// Wait for thread1 to have the name field cached
						thread2GotDescriptionFieldEvent.Set( );
						thread1GotNameFieldEvent.WaitOne( );

						IEntity cachedEntity;
                        if (!snaphotData.TryGetEntity(descriptionField.Id, out cachedEntity))
                        {
                            thread2Result = "The description entity should be cached";
                        }
                        else if (snaphotData.TryGetEntity(nameField.Id, out cachedEntity))
                        {
                            thread2Result = "The name entity should not be cached";
                        }
                        else
                        {
                            thread2Failed = false;
                        }
					}
				} );

			thread1.Start( );
			thread2.Start( );

			startEvent.Set( );

			thread1.Join( );
			thread2.Join( );

			Assert.IsFalse( thread1Failed, "Thread 1 failed: " + thread1Result );
            Assert.IsFalse( thread2Failed, "Thread 2 failed: " + thread2Result );
		}

		/// <summary>
		///     Test a nested context
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestNestedSnapshotCachingReadonlyEntity( )
		{
			using ( new EntitySnapshotContext( ) )
			{
				long nameFieldId;
				using ( new EntitySnapshotContext( ) )
				{
					var nameField = Entity.Get<Field>( new EntityRef( "core", "name" ) );
					nameFieldId = nameField.Id;

					EntitySnapshotContextData snaphotData2 = EntitySnapshotContext.GetContextData( );

					IEntity cachedEntity2;
					Assert.IsTrue( snaphotData2.TryGetEntity( nameFieldId, out cachedEntity2 ), "The entity should be cached" );
				}

				EntitySnapshotContextData snaphotData1 = EntitySnapshotContext.GetContextData( );

				IEntity cachedEntity1;
				Assert.IsTrue( snaphotData1.TryGetEntity( nameFieldId, out cachedEntity1 ), "The entity should be cached" );
			}
		}
	}
}