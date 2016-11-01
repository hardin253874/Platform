// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Model;
using NUnit.Framework;

#pragma warning disable 168

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Entity tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class RelationshipTests
	{
		[Test]
		[RunAsDefaultTenant]
		public void DeleteType( )
		{
			/////
			// Create a type.
			/////
			var coffeeCup = new EntityType
				{
					Name = "CoffeeCup",
					Alias = "coffeeCup"
				};
			coffeeCup.Save( );

			Entity.Delete( coffeeCup.Id );
		}

		[Test]
		[RunAsDefaultTenant]
		public void GetRelationshipInstancesByRelId( )
		{
			IEntity entity = Entity.GetByName( "Test 01" ).First( );
			long relId = Entity.GetId( "test", "herbs" );

			IEntityRelationshipCollection<IEntity> instances =
				entity.GetRelationships( relId, Direction.Forward );

			foreach ( var i in instances )
			{
				IEntity relatedEntity = i.Entity; // this is typically what you want
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void GetRelationshipsForType( )
		{
			var type = Entity.Get<EntityType>( "test:person" );

			// Forward relationship definitions, where type is the 'from' type.
			foreach ( Relationship rel in type.Relationships )
			{
				string linkName = rel.ToName ?? rel.Name;
				EntityType otherEnd = rel.ToType;

				string cardinality = rel.Cardinality.Alias; // core:oneToMany, etc
				bool otherEndIsMulti = cardinality.EndsWith( "ToMany" );
			}

			// Reverse relationship definitions, where type is the 'to' type.
			foreach ( Relationship rel in type.ReverseRelationships )
			{
				string linkName = rel.FromName ?? rel.Name;
				EntityType otherEnd = rel.FromType;

				string cardinality = rel.Cardinality.Alias; // core:oneToMany, etc
				bool otherEndIsMulti = cardinality.StartsWith( "core:manyTo" );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestClearRelationship( )
		{
			Folder f1 = null;
			Resource r = null;

			try
			{
				// Create a folders
				f1 = new Folder( );
				f1.Save( );

				// Create resource in folder
			    r = new Resource();
                r.ResourceInFolder.Add(f1.As<NavContainer>());
				r.Save( );
				long id = r.Id;

				// Get entity again.
				r = Entity.Get<Resource>( id );
				Assert.IsNotNull( r.ResourceInFolder );

				// Clear the relationship
				r = r.AsWritable<Resource>( );
				r.ResourceInFolder.Clear();
				r.Save( );

				// Ensure folder was removed
				r = Entity.Get<Resource>( id );
				
                Assert.AreEqual(0, r.ResourceInFolder.Count);
				f1 = Entity.Get<Folder>( f1.Id );
				Assert.AreEqual( 0, f1.FolderContents.Count );
			}
			finally
			{
				if ( f1 != null )
				{
					if ( r != null )
					{
						r.AsWritable( ).Delete( );
					}
				}

				if ( r != null )
				{
					r.AsWritable( ).Delete( );
				}
			}
		}

		/// <summary>
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestEntity_Constructor_Type_Null( )
		{
			// Create parent and child folders
			var parent = new Folder
				{
					Name = "Parent"
				};
			parent.Save( );

			var child = new Folder
				{
					Name = "Child"
				};
			child.Save( );

			// Link them together via child
			child.ResourceInFolder.Add(parent.As<NavContainer>( ));
			child.Save( );

			// Assert that the parent has the relationship
			parent = Entity.Get<Folder>( parent.Id );
			Assert.AreEqual( 1, parent.FolderContents.Count );

			// Get writeable copy, modify and save
			parent = parent.AsWritable<Folder>( );
			parent.Name = "Name2";
			parent.Save( );

			// Assert that the parent still has the relationship
			parent = Entity.Get<Folder>( parent.Id );
			Assert.AreEqual( 1, parent.FolderContents.Count );

			child.Delete( );
			parent.AsWritable( ).Delete( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestEnum( )
		{
			var r = Entity.Get<Relationship>( "core:fieldIsOnType" );
            CardinalityEnum_Enumeration? result = r.Cardinality_Enum;
            Assert.IsTrue( result == CardinalityEnum_Enumeration.ManyToOne );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestMoveToNewFolder( )
		{
			Folder f1 = null;
			Folder f2 = null;
			Resource r = null;

			try
			{
				// Create two folders
				f1 = new Folder( );
				f1.Save( );
				f2 = new Folder( );
				f2.Save( );

				// Create resource in first folder
			    r = new Resource();
                r.ResourceInFolder.Add(f1.As<NavContainer>());
				r.Save( );

				// Move resource into second folder
				r = Entity.Get<Resource>( r.Id ).AsWritable<Resource>( );
			    r.ResourceInFolder.Remove(f1.As<NavContainer>());
                r.ResourceInFolder.Add(f2.As<NavContainer>());
				r.Save( );

				// Verify that resource is in second folder            
				f2 = Entity.Get<Folder>( f2.Id );
				Assert.AreEqual( 1, f2.FolderContents.Count );

				// Verify that resource is not in first folder
				f1 = Entity.Get<Folder>( f1.Id );
				Assert.AreEqual( 0, f1.FolderContents.Count );
			}
			finally
			{
				if ( r != null )
				{
					r.Delete( );
				}

				if ( f1 != null )
				{
					f1.AsWritable( ).Delete( );
				}

				if ( f2 != null )
				{
					f2.AsWritable( ).Delete( );
				}
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestRelationships01( )
		{
			var child = new NavContainer( );
			var parent = new NavContainer( );
            child.ResourceInFolder.Add(parent);
			child.Name = "Test " + Guid.NewGuid( ).ToString( );
			child.Save( );

			Assert.IsTrue( parent.FolderContents.Count > 0 );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestRelationships01Rev( )
		{
			var child = new Resource( );
			var parent = new NavContainer( );
			parent.FolderContents.Add( child );
			parent.Save( );

			Assert.IsNotNull( child.ResourceInFolder );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestRelationships02( )
		{
			var child = new NavContainer( );
			var parent = new NavContainer( );
            child.ResourceInFolder.Add(parent);
			child.Name = "Test " + Guid.NewGuid( ).ToString( );
			child.Save( );

			parent = Entity.Get<NavContainer>( parent.Id );

			Assert.IsTrue( parent.FolderContents.Count > 0 );
		}

        [Test]
        [RunAsDefaultTenant]
        public void TestRelationships01FetchNoSave()
        {
            var person = new Person();
            var account = new UserAccount();
            account.AccountHolder = person;
            account.Save();

            account = Entity.Get<UserAccount>(account.Id, true);
            account.AccountHolder = null;

            var rels = account.GetRelationships<Person>(UserAccount.AccountHolder_Field, Direction.Reverse);

            Assert.That(rels.Count(), Is.EqualTo(0));
        }


        [Test]
		[RunAsDefaultTenant]
		public void TestRelationships02Rev( )
		{
			var child = new Resource( );
			var parent = new NavContainer( );
			parent.FolderContents.Add( child );
			parent.Save( );

			child = Entity.Get<Resource>( child.Id );

			Assert.IsNotNull( child.ResourceInFolder );
		}

        [Test]
        [RunAsDefaultTenant]
        public void TestRelationshipsBug19508()
        {
            // The delete of a relationship is not being removed from teh write cache where the referenced resource is deleted.
            var child = new Resource();
            var parent = new NavContainer();
            parent.FolderContents.Add(child);
            parent.Save();
            parent.FolderContents.Remove(child);
            child.Delete();
            parent.Save();
        }

        [Test]
        [RunAsDefaultTenant]
        public void VerifyListOfVisibleResourceRelationships()
        {
            // Only a limited set of relationships should be visible from/to the root resource type.
            // All others should be marked as hideOnFromType (if Resource is the fromType) or hideOnToType (if Resource is the toType).

            EntityType resource = Resource.Resource_Type;
            
            Assert.That(
                resource
                    .Relationships
                    .Where(r => r.HideOnFromType == false)
                    .Select(r => r.Alias ?? r.Name)
                    .ToList(),
                Is.EquivalentTo(new []
                {
                    "core:createdBy",
                    "core:isOfType",
                    "core:lastModifiedBy",
                    "core:securityOwner"
                }), "Forward relationships incorrect");
            Assert.That(
                resource
                    .ReverseRelationships
                    .Where(r => r.HideOnToType == false)
                    .Select(r => r.Alias ?? r.Name)
                    .ToList(),
                Is.Empty, "Reverse relationships incorrect");
        }
	}
#pragma warning restore 168
}