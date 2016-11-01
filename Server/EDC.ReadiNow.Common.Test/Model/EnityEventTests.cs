// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Model
{
	[TestFixture]
	[RunWithTransaction]
	public class EntityEventTests
	{
		/// <summary>
		///     Initializes this instance.
		/// </summary>
		[SetUp]
		public void Initialize( )
		{
			using ( new AdministratorContext( ) )
			{
				_testTarget = new Target
					{
						Name = "TestEntityEventsTarget",
						TypeName = typeof ( EventTestingTarget ).FullName,
						AssemblyName = typeof ( EventTestingTarget ).Assembly.FullName
					};

				_testMessageType = new EntityType
					{
						Name = "TestAfterDeleteMessage_type"
					};

				_testMessageType.OnBeforeDelete.Add( _testTarget );
				_testMessageType.OnAfterDelete.Add( _testTarget );
				_testMessageType.OnBeforeSave.Add( _testTarget );
				_testMessageType.OnAfterSave.Add( _testTarget );

				_testMessageType.Save( );
			}
		}

		/// <summary>
		///     Cleanups this instance.
		/// </summary>
		[TearDown]
		public void Cleanup( )
		{
			using ( new AdministratorContext( ) )
			{
				if ( _testMessageType != null )
				{
					_testMessageType.Delete( );
				}
			}
		}

		private EntityType _testMessageType;
		private Target _testTarget;

		private class EventTestingTarget : IEntityEventSave, IEntityEventDelete
		{
			private static int _saveState;
			private static int _deleteState;

			public static bool IsFinishedDelete
			{
				get
				{
					return _deleteState == 2;
				}
			}

			public static bool IsFinishedSave
			{
				get
				{
					return _saveState == 2;
				}
			}


			public bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
			{
				Assert.IsTrue( _saveState == 0 );
				_saveState++;
				return false;
			}

			public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
			{
				Assert.IsTrue( _saveState == 1 );
				_saveState++;
			}

			public bool OnBeforeDelete( IEnumerable<IEntity> entities, IDictionary<string, object> state )
			{
				Assert.IsTrue( _deleteState == 0 );
				_deleteState++;
				return false;
			}

			public void OnAfterDelete( IEnumerable<long> entities, IDictionary<string, object> state )
			{
				Assert.IsTrue( _deleteState == 1 );
				_deleteState++;
			}

			public static void Reset( )
			{
				_saveState = 0;
				_deleteState = 0;
			}
		}


		[Test]
		[RunAsGlobalTenant]
		public void TestDeleteMessage( )
		{
			// Straight normal save and delete
			EventTestingTarget.Reset( );

			var instance = new Entity( _testMessageType );

			instance.Save( );
			instance.Delete( );

			Assert.IsTrue( EventTestingTarget.IsFinishedDelete, "Delete raised messages correctly" );
		}

		[Test]
		[RunAsGlobalTenant]
		public void TestDeleteUsingIdList( )
		{
			// Straight normal save and delete
			EventTestingTarget.Reset( );

			var instance = new Entity( _testMessageType );

			instance.Save( );

			Entity.Delete( new[]
				{
					instance.Id
				} );

			Assert.IsTrue( EventTestingTarget.IsFinishedDelete, "Delete raised messages correctly" );
		}

		[Test]
		[RunAsGlobalTenant]
		public void TestSaveMessage( )
		{
			// Straight normal save and delete
			EventTestingTarget.Reset( );

			var instance = new Entity( _testMessageType );

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                instance.Save();

                ctx.CommitTransaction();
            }                

			Assert.IsTrue( EventTestingTarget.IsFinishedSave, "Save raised messages correctly" );
		}
	}
}