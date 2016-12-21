// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class SetRelationshipImplementationTest : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void SetRelationship( )
		{
			var employeeType = Entity.Get<EntityType>( "test:employee" );
			var managerType = Entity.Get<EntityType>( "test:manager" );
			var nameField = Entity.Get<StringField>( "core:name" );

			IEntity emp = new Entity( employeeType );
			emp.SetField( nameField, "Test Employee" );

			IEntity emp2 = new Entity( employeeType );
			emp.SetField( nameField, "Test Employee 2" );

			IEntity mgr = ( new Entity( employeeType ) );
			emp.SetField( nameField, "Test Manager" );

			mgr.As<Resource>( ).IsOfType.Add( managerType );

			emp.Save( );
			emp2.Save( );
			mgr.Save( );

			ToDelete.Add( emp.Id );
			ToDelete.Add( emp2.Id );
			ToDelete.Add( mgr.Id );


			var setRel = new SetRelationshipActivity( );
			setRel.Save( );
			ToDelete.Add( setRel.Id );

			var setRelAs = setRel.As<WfActivity>( );

			ActivityImplementationBase nextActivity = setRelAs.CreateWindowsActivity( );

			var args = new Dictionary<string, object>
			{
				{
					"Origin", emp
				},
				{
					"Relationship", new EntityRef( "test:reportsTo" ).Entity
				},
				{
					"Destination", mgr
				},
			};

			RunActivity( nextActivity, args );

			emp = Entity.Get( emp.Id );

			IEntityRelationshipCollection<IEntity> rels = emp.GetRelationships( "test:reportsTo" );

			Assert.AreEqual( 1, rels.Count( ), "Ensure the manager has been set" );
			Assert.AreEqual( rels.First( ).Entity.Id, mgr.Id, "Ensure the manager has been set to the correct value" );


			// clear relationships
			args = new Dictionary<string, object>
			{
				{
					"Origin", emp
				},
				{
					"Relationship", new EntityRef( "test:reportsTo" ).Entity
				},
				{
					"Destination", null
				},
				{
					"Replace Existing Destination", true
				}
			};

			RunActivity( nextActivity, args );

			emp = Entity.Get( emp.Id );

			rels = emp.GetRelationships( "test:reportsTo" );

			Assert.AreEqual( 0, rels.Count( ), "Ensure the manager has been cleared" );

			// set the reverse relationship
			args = new Dictionary<string, object>
			{
				{
					"Origin", mgr
				},
				{
					"Relationship", new EntityRef( "test:directReports" ).Entity
				},
				{
					"Destination", emp
				},
				{
					"(Internal) Is this a reverse relationship", true
				}
			};

			RunActivity( nextActivity, args );

			mgr = Entity.Get( mgr.Id );

			rels = mgr.GetRelationships( "test:directReports" );

			Assert.AreEqual( 1, rels.Count( ), "Ensure the employee has been set" );
			Assert.AreEqual( rels.First( ).Entity.Id, emp.Id, "Ensure the employee has been set to the correct value" );

			// add a second relationship, clearing the first
			args = new Dictionary<string, object>
			{
				{
					"Origin", mgr
				},
				{
					"Relationship", new EntityRef( "test:directReports" ).Entity
				},
				{
					"Destination", emp2
				},
				{
					"(Internal) Is this a reverse relationship", true
				},
				{
					"Replace Existing Destination", true
				}
			};

			RunActivity( nextActivity, args );

			mgr = Entity.Get( mgr.Id );

			rels = mgr.GetRelationships( "test:directReports" );

			Assert.AreEqual( 1, rels.Count( ), "Ensure the new employee has been set and the old cleared" );
			Assert.AreEqual( rels.First( ).Entity.Id, emp2.Id, "Ensure the manager has been set to the correct value" );

			// add the first back in
			args = new Dictionary<string, object>
			{
				{
					"Origin", mgr
				},
				{
					"Relationship", new EntityRef( "test:directReports" ).Entity
				},
				{
					"Destination", emp
				},
				{
					"(Internal) Is this a reverse relationship", true
				},
				{
					"Replace Existing Destination", false
				}
			};

			RunActivity( nextActivity, args );

			mgr = Entity.Get( mgr.Id );

			rels = mgr.GetRelationships( "test:directReports" );

			Assert.AreEqual( 2, rels.Count( ), "Add a second relationship" );
		}
	}
}