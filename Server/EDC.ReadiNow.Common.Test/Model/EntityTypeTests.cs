// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Entity tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class EntityTypeTests
	{
		private void ChangeTypeTest( string before, string after )
		{
            try
            {
                List<long> initialTypes = before.Split(',').Select(t => Entity.GetByName<EntityType>(t).First().Id).ToList();
                List<long> finalTypes = after.Split(',').Select(t => (Entity.GetByName<EntityType>(t)).First().Id).ToList();

                IEntity e = null;
                try
                {
                    e = Entity.Create(initialTypes);
                    e.Save();
                    Entity.ChangeType(e, finalTypes);
                    e.Save();

                    // Ensure results are equal
                    IEntity e2 = Entity.Get(e.Id);
                    Assert.IsFalse(finalTypes.Except(e2.TypeIds).Any());
                    Assert.IsFalse(e2.TypeIds.Except(finalTypes).Any());
                }
                finally
                {
                    if (e != null)
                    {
                        e.Delete();
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Message != null && ex.Message.Contains("deadlock"))
                {
                    Assert.Ignore("Ignore deadlock for now: " + ex.Message); // TODO: #22623 - FIX THE DEADLOCKS!!
                }
                else
                {
                    throw;
                }
            }
		}

		[Test]
		[RunAsDefaultTenant]
		public void DemoteResourceType( )
		{
			ChangeTypeTest(
                before: "Sedan",
				after: "Car" );
		}

		[Test]
		[RunAsDefaultTenant]
		public void RemoveAdditionalResourceType( )
		{
			ChangeTypeTest(
				before: "Car,Motorbike",
				after: "Car" );
		}

		[Test]
		[RunAsDefaultTenant]
		public void AddAdditionalResourceType( )
		{
			ChangeTypeTest(
				before: "Car",
                after: "Car,Motorbike");
		}

		[Test]
		[RunAsDefaultTenant]
		public void PromoteResourceType( )
		{
			ChangeTypeTest(
				before: "Car",
				after: "Sedan" );
		}

		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ValidationException ) )]
		public void ChangeToIncompatibleType( )
		{
            ChangeTypeTest(
				before: "Car",
				after: "Car,Report" );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestCreateEntityType( )
		{
			EntityType newType = null;
			FieldGroup group = null;
			StringField field1 = null;

			try
			{
				newType = Entity.Create<EntityType>( );
				newType.Name = "TestEntityType";
				newType.Description = "Test new EntityType";
				newType.Save( );
				long typeId = newType.Id;

				group = new FieldGroup
					{
						Name = "TestGroup1"
					};
				newType.FieldGroups.Add( group );
				group.Save( );

				field1 = new StringField
					{
						Name = "TestField1",
						AllowMultiLines = true,
						FieldInGroup = @group,
						FieldIsOnType = newType
					};

				field1.Save( );

				var savedEntityType = Entity.Get<EntityType>( new EntityRef( typeId ), true );

				Assert.IsTrue( savedEntityType.Name == "TestEntityType" );
				savedEntityType.Delete( );
			}
			finally
			{
				if ( field1 != null )
				{
					field1.Delete( );
				}

				if ( group != null )
				{
					group.Delete( );
				}

				if ( newType != null )
				{
					newType.Delete( );
				}
			}
		}

		/// <summary>
		///     Tests the entity_ constructor_ type_ null.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestTypesDerivingFromType( )
		{
			// Test to make sure no-one accidentally adds types that inherit from 'type'.
			// Important: there are very few valid reasons for a type to inherit from 'type'.
			// If you think you need to do this, then check with Peter first.

			const string allowedToInheritFromType = "fieldType,relationship,enumType,activityType,argumentType,definition,k:renderControlType,scheduleAction,emailScanner,activityArgument,managedType";

			string[] allowedAliases = allowedToInheritFromType
				.Split( ',' )
				.Select( EntityRefHelper.ConvertAliasPrefixToFull )
				.ToArray( );

			foreach ( EntityType derivedType in EntityType.EntityType_Type.DerivedTypes )
			{
				if ( string.IsNullOrEmpty( derivedType.Alias ) )
				{
					throw new Exception( "A type deriving from 'type' does not have an alias." );
				}

                if (allowedAliases.Contains(derivedType.Alias))
                    continue;

			    if (derivedType.InstancesInheritByDefault != null &&
			        derivedType.InstancesInheritByDefault.Alias != "core:resource")
			        continue; // if someone's gone to the effort of specifying this, then it probably wasn't by accident.

			    throw new Exception( "The type '" + derivedType.Alias + "' unexpectedly derived from 'type'" );
			}
		}
	}
}