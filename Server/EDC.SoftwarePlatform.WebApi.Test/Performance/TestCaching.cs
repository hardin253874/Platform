// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Test.Common;

namespace EDC.SoftwarePlatform.WebApi.Test.Performance
{
	//[TestFixture]
	public class TestCaching
	{
		//[Test]
		[RunAsDefaultTenant]
		public void TestMinusOneInRelationshipInstanceFetchBug( )
		{
			Entity.Get<UiContext>( new EntityRef( "console", "uiContextHtml" ) );
			var intField = Entity.Get<FieldType>( new EntityRef( "core", "intField" ) );

			// cache
			Action action = ( ) =>
			{
#pragma warning disable 168
				UiContext f = intField.RenderingControl.First( ).Context;
#pragma warning restore 168
			};

			// cache
			action( );

			// time
			CommonServiceTestsHelper.AssertFasterThan( action, 10, "The second fetch should not hit the DB" );
		}


		//[Test]
		[RunAsDefaultTenant]
		public void TestSingleRelationship( )
		{
			var personRef = new EntityRef( "core", "person" );
			var employeeRef = new EntityRef( "shared", "employee" );

			var employee = Entity.Get<EntityType>( employeeRef );
			var person = Entity.Get<EntityType>( personRef );

			// this should cache the relationship in either direction
#pragma warning disable 168
			IEntityCollection<EntityType> list = employee.Inherits;
			IEntityCollection<EntityType> list2 = person.DerivedTypes;
#pragma warning restore 168

			CommonServiceTestsHelper.AssertFasterThan(
				( ) =>
				{
					// we run the test 100 times to amplify the effect of the cache failure. 
					for ( int i = 0; i < 100; i++ )
					{
#pragma warning disable 168
						IEntityCollection<EntityType> a = employee.Inherits;
						IEntityCollection<EntityType> b = person.DerivedTypes;
#pragma warning restore 168
					}
				}
				, 100, "Pulling precached relationships should be very quick. Failure indicates that reverse and forward relationships are interfering with each other." );
		}


		//[Test]
		[RunAsDefaultTenant]
		public void TestSingleRelationship2( )
		{
			var personRef = new EntityRef( "core", "person" );
			var orgRef = new EntityRef( "shared", "organisation" );

			var person = Entity.Get<EntityType>( personRef );
			var org = Entity.Get<EntityType>( orgRef );

			// this should cache the relationship in either direction
#pragma warning disable 168
			IEntityCollection<EntityType> list = person.Inherits;
			IEntityCollection<EntityType> list2 = org.Inherits;
#pragma warning restore 168

			CommonServiceTestsHelper.AssertFasterThan(
				( ) =>
				{
					// we run the test 100 times to amplify the effect of the cache failure. 
					for ( int i = 0; i < 100; i++ )
					{
#pragma warning disable 168
						IEntityCollection<EntityType> a = person.Inherits;
						IEntityCollection<EntityType> b = org.Inherits;
#pragma warning restore 168
					}
				}
				, 50, "Pulling precached relationships should be very quick. Failure indicates that two rels looking at the same object are interfering with each other." );
		}
	}
}