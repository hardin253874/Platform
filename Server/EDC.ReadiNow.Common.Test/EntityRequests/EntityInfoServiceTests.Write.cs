// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using FluentAssertions;
using NUnit.Framework;
using EDC.ReadiNow.IO;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.ReadiNow.Test.EntityRequests
{
	[TestFixture]
    [RunWithTransaction]
	public class EntityInfoServiceTests_Write
	{
        [Test]
		[RunAsDefaultTenant]
		public void CreateInstance( )
		{
			// Define a new entity
			var data = new EntityData
				{
					Fields = new List<FieldData>( ),
					TypeIds = new EntityRef( "test", "person" ).ToEnumerable( ).ToList( )
				};
			data.Fields.Add( new FieldData
				{
					FieldId = new EntityRef( "name" ),
					Value = new TypedValue( "Isaac Newton" )
				} );

			// Create it
			var svc = new EntityInfoService( );
			EntityRef id = svc.CreateEntity( data );

			// Verify it was created
			Assert.IsTrue( id.Id > 0, "Positive Id" );

			// Select the data
			EntityMemberRequest request = EntityRequestHelper.BuildRequest( "name" );
			EntityData result = svc.GetEntityData( id, request );

			// Verify results
			Assert.AreEqual( "Isaac Newton", result.Fields[ 0 ].Value.Value );

			svc.DeleteEntity( id );
		}

		[Test]
		[RunAsDefaultTenant]
		public void DeleteInstance( )
		{
			// Define a new entity
			var data = new EntityData
				{
					Fields = new List<FieldData>( ),
                    TypeIds = new EntityRef( "test", "person" ).ToEnumerable( ).ToList( )
				};
			data.Fields.Add( new FieldData
				{
					FieldId = new EntityRef( "name" ),
					Value = new TypedValue( "Isaac Newton" )
				} );

			// Create it
			var svc = new EntityInfoService( );
			EntityRef id = svc.CreateEntity( data );

			// Verify it was created
			Assert.IsTrue( id.Id > 0, "Positive Id" );

			// Select the data
			EntityMemberRequest request = EntityRequestHelper.BuildRequest( "name" );
			EntityData result = svc.GetEntityData( id, request );

			// Verify results
			Assert.AreEqual( "Isaac Newton", result.Fields[ 0 ].Value.Value );

			long entityId = id.Id;

			// Delete the entity
			Entity.Delete( id );

			var newRef = new EntityRef( entityId );

			result = svc.GetEntityData( newRef, request );
			Assert.IsNull( result );
		}

		[Test]
		[RunAsDefaultTenant]
		public void GetFieldInstanceDbType( )
		{
			/////
			// Define a new entity
			/////
			var data = new EntityData
				{
					Fields = new List<FieldData>( ),
                    TypeIds = new EntityRef( "test", "person" ).ToEnumerable( ).ToList( )
				};

			/////
			// Create it
			/////
			var svc = new EntityInfoService( );
			EntityRef id = svc.CreateEntity( data );

			/////
			// Verify it was created
			/////
			Assert.IsTrue( id.Id > 0, "Positive Id" );

			EntityMemberRequest fieldRequest = EntityRequestHelper.BuildRequest( @"
				fields.isOfType.name,
				fields.isOfType.alias,
				fields.isOfType.dbType,
				fields.isOfType.dbTypeFull,
				fields.isOfType.readiNowType,
				fields.isOfType.xsdType" );

			EntityData result = svc.GetEntityData( data.TypeIds.First( ), fieldRequest );

			/////
			// Verify results
			/////
			Assert.AreEqual( 1, result.Relationships.Count );

			svc.DeleteEntity( id );
		}

		/// <summary>
		///     Test the reverse direction on a request works.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestChangeType( )
		{
			// Note: refer to additional tests directly on Entity.ChangeType.

			var svc = new EntityInfoService( );

			IEntity e = null;
			try
			{
                e = Entity.Create( new EntityRef( "test:person" ) );

				var ed = new EntityData
					{
						Id = e.Id
					};
                ed.TypeIds.Add( new EntityRef( "test:manager" ) );

				svc.UpdateEntityType( ed );
			}
			catch
			{
				if ( e != null )
				{
					e.Delete( );
				}
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestLoopsDontBreakCreate( )
		{
			// Define a new entity
			var data1 = new EntityData
				{
					Fields = new List<FieldData>( ),
                    TypeIds = new EntityRef( "test", "person" ).ToEnumerable( ).ToList( )
				};
			data1.Fields.Add( new FieldData
				{
					FieldId = new EntityRef( "name" ),
					Value = new TypedValue( "Isaac Newton" )
				} );

			// Define a new related entity
			var data2 = new EntityData
				{
					Fields = new List<FieldData>( ),
                    TypeIds = new EntityRef( "test", "employee" ).ToEnumerable( ).ToList( )
				};
			data2.Fields.Add( new FieldData
				{
					FieldId = new EntityRef( "name" ),
					Value = new TypedValue( "Isaac Newtons Emmployer" )
				} );
			data2.DataState = DataState.Create;

			data1.Relationships = new List<RelationshipData>
				{
					new RelationshipData
						{
							RelationshipTypeId = new EntityRef( "test", "reportsTo" ),
							IsReverse = false,
							Instances = new List<RelationshipInstanceData>
								{
									new RelationshipInstanceData
										{
											Entity = data2,
											DataState = DataState.Create
										}
								},
						}
				};

			data2.Relationships = new List<RelationshipData>
				{
					new RelationshipData
						{
							RelationshipTypeId = new EntityRef( "test", "reportsTo" ),
							IsReverse = false,
							Instances = new List<RelationshipInstanceData>
								{
									new RelationshipInstanceData
										{
											Entity = data1,
											DataState = DataState.Create
										}
								},
						}
				};

			// Create it
			var svc = new EntityInfoService( );
			EntityRef id = svc.CreateEntity( data1 );

			// Verify it was created
			Assert.IsTrue( id.Id > 0, "Positive Id" );

            svc.DeleteEntity( id );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestTempDeletedChildItemIgnored( )
		{
			var svc = new EntityInfoService( );

			EntityRef orgRef = null;

			try
			{
				var o = new Report
					{
						Name = "org rpt",
						Alias = "shared:a" + Guid.NewGuid( )
					};

				var b = new ReportColumn
					{
						Name = "bob",
						Alias = "shared:a" + Guid.NewGuid( ),
                        ColumnForReport = o
					};

				EntityData result = svc.GetEntityData( b, EntityRequestHelper.BuildRequest( "name, columnForReport.name" ) );

				result.Relationships[ 0 ].Instances[ 0 ].DataState = DataState.Delete;

				orgRef = svc.CreateEntity( result );
			}
			finally
			{
				if ( orgRef != null )
				{
					Entity.Delete( orgRef );
				}
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void UpdateField( )
		{
			var rq = new EntityMemberRequest( );
			rq.Fields.Add( new EntityRef( "description" ) );

			var svc = new EntityInfoService( );
            EntityData entity = svc.GetEntityData(new EntityRef("test", "person"), rq);

			entity.Fields[ 0 ].Value.Value = "Hello world";
			svc.UpdateEntity( entity );
		}

        [Test]
		[RunAsDefaultTenant]
	    public void UpdateEntityDeleteItemFromSecuresToRelationship()
	    {
	        // this case requires that the entity to be delete ONLY receives its access for read/delete through
            // the relationship with the entity being updated.
            // (I.e. activityPrompts removed from promptForArguments relationship with promptUserActivity)
            
            /////////////////////////////////////////////////////////////////////////////////////////
            // Arrange 
            /////////////////////////////////////////////////////////////////////////////////////////
            var parentType = Create<EntityType>();
            parentType.Inherits.Add(UserResource.UserResource_Type);
            parentType.Save();

            var childType = Create<EntityType>();
            childType.Inherits.Add(UserResource.UserResource_Type);
            childType.Save();

            var rel = Create<Relationship>();
            rel.FromType = parentType;
            rel.ToType = childType;
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.OneToMany;
            rel.SecuresTo = true;
            rel.Save();

            var parentInstance = Entity.Create(parentType.Id);
            var childInstance = Entity.Create(childType.Id);
            SetRel(parentInstance, childInstance, rel);

            parentInstance.IsTemporaryId.Should().BeFalse();
            childInstance.IsTemporaryId.Should().BeFalse();
            Entity.Exists(parentInstance.Id).Should().BeTrue();
            Entity.Exists(childInstance.Id).Should().BeTrue();

            var user = Create<UserAccount>(true);
            user.Name = "UpdateEntityDeleteItemFromSecuresToRelationship";
            user.Save();
            using (new SetUser(user))
            {
                Action getParentNoPermission = () => Entity.Get(parentInstance.Id);
                getParentNoPermission.ShouldThrow<PlatformSecurityException>().WithMessage("*does not have view access to*");

                Action getChildNoPermission = () => Entity.Get(childInstance.Id);
                getChildNoPermission.ShouldThrow<PlatformSecurityException>().WithMessage("*does not have view access to*");
            }
            
            // grant access via parent
            var query = TestQueries.Entities(parentType);
            var queryResult = Factory.QueryRunner.ExecuteQuery(query, new QuerySettings { SecureQuery = true });
            queryResult.Should().NotBeNull();
            queryResult.DataTable.Rows.Count.Should().Be(1);
            queryResult.DataTable.Rows[0].ItemArray.Should().Contain(parentInstance.Id);

            new AccessRuleFactory().AddAllowByQuery(user.As<Subject>(),
                parentType.As<SecurableEntity>(),
                new[] { Permissions.Read, Permissions.Modify, Permissions.Delete },
                query.ToReport());

            Factory.EntityAccessControlService.ClearCaches();

            using (new SetUser(user))
            {
                IEntity p = null;
                Action getParentPermission = () => p = Entity.Get(parentInstance.Id);
                getParentPermission.ShouldNotThrow();
                p.Should().NotBeNull();
                p.Id.ShouldBeEquivalentTo(parentInstance.Id);

                IEntity c = null;
                Action getChildPermission = () => c = Entity.Get(childInstance.Id);
                getChildPermission.ShouldNotThrow();
                c.Should().NotBeNull();
                c.Id.ShouldBeEquivalentTo(childInstance.Id);
            }

            var data = new EntityData
            {
                Id = new EntityRef(parentInstance.Id),
                Relationships = new List<RelationshipData>
                {
                    new RelationshipData
                    {
                        RelationshipTypeId = new EntityRef(rel.Id),
                        Instances = new List<RelationshipInstanceData>
                        {
                            new RelationshipInstanceData {
                                Entity = new EntityData { Id = new EntityRef(childInstance.Id), DataState = DataState.Delete }
                            }
                        }
                    }
                }
            };

            /////////////////////////////////////////////////////////////////////////////////////////
            // Act
            /////////////////////////////////////////////////////////////////////////////////////////
            using (new SetUser(user))
            {
                var svc = new EntityInfoService();
                Action callUpdate = () => svc.UpdateEntity(data);
                callUpdate.ShouldNotThrow();
            }

            /////////////////////////////////////////////////////////////////////////////////////////
            // Assert 
            /////////////////////////////////////////////////////////////////////////////////////////
            Entity.Exists(parentInstance.Id).Should().BeTrue();
            Entity.Exists(childInstance.Id).Should().BeFalse();
	    }

        [Test]
        [RunAsDefaultTenant]
	    public void UpdateEntityRemoveExistingFromCascadeDeleted()
	    {
            /////////////////////////////////////////////////////////////////////////////////////////
            // Arrange 
            /////////////////////////////////////////////////////////////////////////////////////////
            var typeA = Create<EntityType>(true);
            var typeB = Create<EntityType>(true);
            var typeC = Create<EntityType>(true);
            var rel1 = Create<Relationship>();
            rel1.FromType = typeA;
            rel1.ToType = typeB;
            rel1.Cardinality_Enum = CardinalityEnum_Enumeration.OneToMany;
            rel1.Save();
            var rel2 = Create<Relationship>();
            rel2.FromType = typeB;
            rel2.ToType = typeC;
            rel2.Cardinality_Enum = CardinalityEnum_Enumeration.OneToMany;
            rel2.CascadeDeleteTo = true;
            rel2.Save();

            var aInstance = Entity.Create(typeA.Id);
            var bInstance = Entity.Create(typeB.Id);
            var cInstance = Entity.Create(typeC.Id);
            SetRel(aInstance, bInstance, rel1);
            SetRel(bInstance, cInstance, rel2);

            aInstance.IsTemporaryId.Should().BeFalse();
            bInstance.IsTemporaryId.Should().BeFalse();
            cInstance.IsTemporaryId.Should().BeFalse();
            Entity.Exists(aInstance.Id).Should().BeTrue();
            Entity.Exists(bInstance.Id).Should().BeTrue();
            Entity.Exists(cInstance.Id).Should().BeTrue();

            var children = bInstance.GetRelationships(rel2);
            children.Should().NotBeNull().And.NotBeEmpty();
            children.Count.Should().Be(1);
            children.Select(c => c.Id).Should().Contain(cInstance.Id);

            var data = new EntityData
            {
                // top level node is the "update"
                Id = new EntityRef(aInstance.Id),
                DataState = DataState.Update,
                Relationships = new List<RelationshipData>
                {
                    new RelationshipData
                    {
                        RelationshipTypeId = new EntityRef(rel1.Id),
                        Instances = new List<RelationshipInstanceData>
                        {
                            new RelationshipInstanceData
                            {
                                Entity = new EntityData
                                {
                                    // this is the node we are deleting and removing from
                                    Id = new EntityRef(bInstance.Id),
                                    DataState = DataState.Delete,
                                    Relationships = new List<RelationshipData>
                                    {
                                        new RelationshipData
                                        {
                                            RemoveExisting = true,
                                            RelationshipTypeId = new EntityRef(rel2.Id),
                                            Instances = new List<RelationshipInstanceData>()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            /////////////////////////////////////////////////////////////////////////////////////////
            // Act
            /////////////////////////////////////////////////////////////////////////////////////////
            var svc = new EntityInfoService();
            Action callUpdate = () => svc.UpdateEntity(data);
            callUpdate.ShouldNotThrow();

            /////////////////////////////////////////////////////////////////////////////////////////
            // Assert 
            /////////////////////////////////////////////////////////////////////////////////////////
            Entity.Exists(aInstance.Id).Should().BeTrue();
            Entity.Exists(bInstance.Id).Should().BeFalse();

            // should not have been cascade deleted if it was removed from the relationship
            Entity.Exists(cInstance.Id).Should().BeTrue();
	    }

		[Test]
		[RunAsDefaultTenant]
		public void UpdateFieldOnRelatedInstance( )
		{
			// Define a new entity
			var data = new EntityData
				{
					Fields = new List<FieldData>( ),
                    TypeIds = new EntityRef("test", "person").ToEnumerable().ToList()
				};
			data.Fields.Add( new FieldData
				{
					FieldId = new EntityRef( "name" ),
					Value = new TypedValue( "Isaac Newton" )
				} );

			// Define a new related entity
			var data2 = new EntityData
				{
					Fields = new List<FieldData>( ),
                    TypeIds = new EntityRef("test", "employee").ToEnumerable().ToList()
				};
			data2.Fields.Add( new FieldData
				{
					FieldId = new EntityRef( "name" ),
					Value = new TypedValue( "Isaac Newtons Emmployer" )
				} );
			data2.DataState = DataState.Create;

			data.Relationships = new List<RelationshipData>
				{
					new RelationshipData
						{
							RelationshipTypeId = new EntityRef( "test", "reportsTo" ),
							IsReverse = false,
							Instances = new List<RelationshipInstanceData>
								{
									new RelationshipInstanceData
										{
											Entity = data2,
											DataState = DataState.Create
										}
								},
						}
				};

			// Create it
			var svc = new EntityInfoService( );
			EntityRef id = svc.CreateEntity( data );

			// Verify it was created
			Assert.IsTrue( id.Id > 0, "Positive Id" );

			// Select the data
			EntityMemberRequest request = EntityRequestHelper.BuildRequest( "name, test:reportsTo.name" );
			EntityData result = svc.GetEntityData( id, request );

			// Verify results
			Assert.AreEqual( "Isaac Newton", result.Fields[ 0 ].Value.Value );

			EntityData employee = result.Relationships[ 0 ].Instances[ 0 ].Entity;
			FieldData employersNameField = employee.Fields[ 0 ];
			Assert.AreEqual( "Isaac Newtons Emmployer", employersNameField.Value.Value );

			// Update employees name
			employersNameField.Value.Value = "bob";
			employee.DataState = DataState.Update;

			svc.UpdateEntity( result );

			// comfirm it changed
			EntityData resultAfterUpdate = svc.GetEntityData( id, request );

			employee = resultAfterUpdate.Relationships[ 0 ].Instances[ 0 ].Entity;
			employersNameField = employee.Fields[ 0 ];
			Assert.AreEqual( "bob", employersNameField.Value.Value );

			// delete the referenced Entity leaving the data state of the top entity unchanged
			employee.DataState = DataState.Delete;

			svc.UpdateEntity( resultAfterUpdate );

			// comfirm it deleted
			EntityData resultAfterDelete = svc.GetEntityData( id, request );

			Assert.AreEqual( 0, resultAfterDelete.Relationships[ 0 ].Instances.Count, "There should be no manager" );

			// clean up
			svc.DeleteEntity( id );
		}

        [TestCase( "manyToMany", "removeExisting rel3", "ent1 ent2a ent2b rel2b rel3" )]
        [TestCase( "oneToMany", "removeExisting", "ent1 ent2a ent2b rel2b" )]
        [TestCase( "manyToOne", "removeExisting rel3", "ent1 ent2a ent2b rel2b rel3" )]
        [TestCase( "oneToOne", "removeExisting", "ent1 ent2a ent2b rel2b" )]
        [TestCase( "manyToMany", "deleteExisting rel3", "ent1 ent2b rel2b rel3" )]
        [TestCase( "oneToMany", "deleteExisting", "ent1 ent2b rel2b" )]
        [TestCase( "manyToOne", "deleteExisting rel3", "ent1 ent2b rel2b rel3" )]
        [TestCase( "oneToOne", "deleteExisting", "ent1 ent2b rel2b" )]
        [TestCase( "manyToMany", "autoCardinality rel3", "ent1 ent2a ent2b rel2a rel2b rel3" )]
        [TestCase( "oneToMany", "autoCardinality rel3", "ent1 ent2a ent2b rel2a rel2b" )]
        [TestCase( "manyToOne", "autoCardinality rel3", "ent1 ent2a ent2b rel2b rel3" )]
        [TestCase( "oneToOne", "autoCardinality rel3", "ent1 ent2a ent2b rel2b" )]
        [RunAsDefaultTenant]
        public void UpdateRelationships( string cardinality, string options, string expect )
        {
            var type1 = Create<EntityType>( true );
            var type2 = Create<EntityType>( true );
            var rel = Create<Relationship>( );
            rel.FromType = type1;
            rel.ToType = type2;
            rel.Cardinality = Entity.Get<CardinalityEnum>( new EntityRef( "core:" + cardinality ) );
            rel.Save( );

            var inst1 = Entity.Create( type1.Id );
            var inst2a = Entity.Create( type2.Id );
            var inst2b = Entity.Create( type2.Id );
            var inst3 = Entity.Create( type2.Id );
            SetRel(inst1, inst2a, rel);
            
            if ( options.Contains( "rel3" ) )
                SetRel( inst3, inst2b, rel );

            EntityData data = new EntityData
            {
                Id = new EntityRef( inst1.Id ),
                Relationships = new List<RelationshipData>
                {
                    new RelationshipData
                    {
                        RemoveExisting = options.Contains("removeExisting"),
                        DeleteExisting = options.Contains("deleteExisting"),
                        AutoCardinality = options.Contains("autoCardinality"),
                        RelationshipTypeId = new EntityRef(rel.Id),
                        Instances = new List<RelationshipInstanceData>
                        {
                            new RelationshipInstanceData {
                                DataState = DataState.Create,
                                Entity = new EntityData { Id = new EntityRef(inst2b.Id) }
                            }
                        }
                    }
                }

            };

            // Call service
            var svc = new EntityInfoService( );
            svc.UpdateEntity( data );

            Assert.That( Entity.Exists( inst1.Id ), Is.EqualTo( expect.Contains( "ent1" ) ), "ent1" );
            Assert.That( Entity.Exists( inst2a.Id ), Is.EqualTo( expect.Contains( "ent2a" ) ), "ent2a" );
            Assert.That( Entity.Exists( inst2b.Id ), Is.EqualTo( expect.Contains( "ent2b" ) ), "ent2b" );

            var values = inst1.GetRelationships( rel ).Select( r => r.Entity.Id ).ToList( );
            Assert.That( values.Contains( inst2a.Id ), Is.EqualTo( expect.Contains( "rel2a" ) ), "rel2a" );
            Assert.That( values.Contains( inst2b.Id ), Is.EqualTo( expect.Contains( "rel2b" ) ), "rel2b" );

            var values2 = inst3.GetRelationships( rel ).Select( r => r.Entity.Id ).ToList( );
            Assert.That( values2.Contains( inst2b.Id ), Is.EqualTo( expect.Contains( "rel3" ) ), "rel3" );
        }

	    /// <summary>
	    ///     Test attempting to clone a temporary entity
	    /// </summary>
	    [Test]
	    [RunAsDefaultTenant]
	    public void TestCloneAndUpdateTemporaryEntity()
	    {
	        var svc = new EntityInfoService();

	        IEntity e = Entity.Create(new EntityRef("test:person"));

	        var ed = new EntityData
	        {
	            Id = e.Id
	        };

	        Assert.Throws<InvalidOperationException>(() => svc.CloneAndUpdateEntity(ed));
	    }

        /// <summary>
	    ///     Test clone and update fields
	    /// </summary>
	    [Test]
        [RunAsDefaultTenant]
        public void TestCloneAndUpdateUpdateFields()
        {
            var svc = new EntityInfoService();

            string initialName = "Initial name" + Guid.NewGuid();
            string initialDescription = "Initial description" + Guid.NewGuid();

            string newName = "New name" + Guid.NewGuid();
            string newDescription = "New description" + Guid.NewGuid();

            IEntity e = Entity.Create(new EntityRef("test:person"));
            e.SetField("core:name", initialName);
            e.SetField("core:description", initialDescription);
            e.Save();

            Assert.AreEqual(initialName, e.GetField("core:name"));
            Assert.AreEqual(initialDescription, e.GetField("core:description"));

            var data = new EntityData
            {
                Id = e.Id,
                Fields = new List<FieldData>()
                {
                    new FieldData
                    {
                        FieldId = new EntityRef("name"),
                        Value = new TypedValue(newName)
                    },
                    new FieldData
                    {
                        FieldId = new EntityRef("description"),
                        Value = new TypedValue(newDescription)
                    }
                },
                DataState = DataState.Update
            };

            var cloneIdsMap = svc.CloneAndUpdateEntity(data);

            long cloneId;            

            Assert.IsTrue(cloneIdsMap.TryGetValue(e.Id, out cloneId), "The initial entity is not found.");

            // Check the fields were cloned
            IEntity clonedEntity = Entity.Get(cloneId);
            Assert.AreEqual(newName, clonedEntity.GetField("core:name"));
            Assert.AreEqual(newDescription, clonedEntity.GetField("core:description"));

            // Check initial entity is not touched
            IEntity initialEntity = Entity.Get(e.Id);
            Assert.AreEqual(initialName, initialEntity.GetField("core:name"));
            Assert.AreEqual(initialDescription, initialEntity.GetField("core:description"));
        }

        /// <summary>
	    ///     Test clone and update relationships when cloning references
	    /// </summary>
	    [Test]
        [RunAsDefaultTenant]
        public void TestCloneAndUpdateCloneByRefUpdateRelationships()
        {
            var svc = new EntityInfoService();

            EntityType type1 = new EntityType();
            type1.Save();

            EntityType type2 = new EntityType();
            type2.Save();

            Relationship rel = new Relationship
            {
                FromType = type1,
                ToType = type2,
                CloneAction_Enum = CloneActionEnum_Enumeration.CloneReferences,
                Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
            };
            rel.Save();            

            IEntity type2A = Entity.Create(type2.Id);            
            type2A.Save();

            IEntity type2B = Entity.Create(type2.Id);
            type2B.Save();

            IEntity type2C = Entity.Create(type2.Id);
            type2C.Save();

            IEntity entity1 = Entity.Create(type1.Id);
            entity1.SetRelationships(rel, new EntityRelationshipCollection<IEntity> { type2A, type2B }, Direction.Forward);
            entity1.Save();

            string newEntityName = Guid.NewGuid().ToString();

            var data = new EntityData
            {
                Id = entity1.Id,
                Relationships = new List<RelationshipData>
                {
                    new RelationshipData
                    {
                        RelationshipTypeId = new EntityRef(rel),
                        Instances = new List<RelationshipInstanceData>
                        {
                            new RelationshipInstanceData
                            {
                                DataState = DataState.Create,
                                Entity = new EntityData {Id = new EntityRef(type2C.Id)}
                            },
                            new RelationshipInstanceData
                            {
                                DataState = DataState.Delete,
                                Entity = new EntityData {Id = new EntityRef(type2B.Id)}
                            },
                            new RelationshipInstanceData
                            {
                                DataState = DataState.Create,
                                Entity = new EntityData
                                {
                                    DataState = DataState.Create,
                                    Fields = new List<FieldData>()
                                    {
                                        new FieldData
                                        {
                                            FieldId = new EntityRef("name"),
                                            Value = new TypedValue(newEntityName)
                                        }
                                    },
                                    TypeIds = new EntityRef(type2.Id).ToEnumerable().ToList()
                                }
                            }
                        }
                    }
                },
                DataState = DataState.Update
            };

            var cloneIdsMap = svc.CloneAndUpdateEntity(data);

            long cloneId;

            Assert.IsTrue(cloneIdsMap.TryGetValue(entity1.Id, out cloneId), "The initial entity is not found.");

            // Check the relationships were cloned
            IEntity clonedEntity = Entity.Get(cloneId);
            var clonedRelationships = clonedEntity.GetRelationships(rel, Direction.Forward);
            Assert.AreEqual(3, clonedRelationships.Count);
            Assert.IsFalse(clonedRelationships.Any(r => r.Id == type2B.Id));
            Assert.IsTrue(clonedRelationships.Any(r => r.Id == type2A.Id));
            Assert.IsTrue(clonedRelationships.Any(r => r.Id == type2C.Id));
            Assert.IsTrue(clonedRelationships.Any(r => (string)r.GetField("core:name") == newEntityName));

            // Check initial entity is not touched
            IEntity initialEntity = Entity.Get(entity1.Id);
            var initialRelationships = initialEntity.GetRelationships(rel, Direction.Forward);
            Assert.AreEqual(2, initialRelationships.Count);
            Assert.IsTrue(initialRelationships.Any(r => r.Id == type2A.Id));
            Assert.IsTrue(initialRelationships.Any(r => r.Id == type2B.Id));
        }

        /// <summary>
	    ///     Test clone and update relationships when cloning entities
	    /// </summary>
	    [Test]
        [RunAsDefaultTenant]
        public void TestCloneAndUpdateCloneByEntityRelationships()
        {
            var svc = new EntityInfoService();

            EntityType type1 = new EntityType();
            type1.Save();

            EntityType type2 = new EntityType();
            type2.Save();

            Relationship rel = new Relationship
            {
                FromType = type1,
                ToType = type2,
                CloneAction_Enum = CloneActionEnum_Enumeration.CloneEntities,
                Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
            };
            rel.Save();

            string initialName = Guid.NewGuid().ToString();
            IEntity entityRelated = Entity.Create(type2.Id);
            entityRelated.SetField("core:name", initialName);
            entityRelated.Save();           

            IEntity entity1 = Entity.Create(type1.Id);
            entity1.SetRelationships(rel, new EntityRelationshipCollection<IEntity> { entityRelated }, Direction.Forward);
            entity1.Save();

            string newEntityName = Guid.NewGuid().ToString();

            var data = new EntityData
            {
                Id = entity1.Id,
                Relationships = new List<RelationshipData>
                {
                    new RelationshipData
                    {
                        RelationshipTypeId = new EntityRef(rel),
                        Instances = new List<RelationshipInstanceData>
                        {
                            new RelationshipInstanceData
                            {
                                DataState = DataState.Unchanged,
                                Entity = new EntityData
                                {
                                    DataState = DataState.Update,
                                    Id = new EntityRef(entityRelated.Id),
                                    Fields = new List<FieldData>()
                                    {
                                        new FieldData
                                        {
                                            FieldId = new EntityRef("name"),
                                            Value = new TypedValue(newEntityName)
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                },
                DataState = DataState.Update
            };

            var cloneIdsMap = svc.CloneAndUpdateEntity(data);

            long cloneId1, cloneIdRelated;

            Assert.IsTrue(cloneIdsMap.TryGetValue(entity1.Id, out cloneId1), "The initial entity is not found.");
            Assert.IsTrue(cloneIdsMap.TryGetValue(entityRelated.Id, out cloneIdRelated), "The related entity is not found.");

            // Check the relationships were cloned
            IEntity clonedEntity = Entity.Get(cloneId1);
            var clonedRelationships = clonedEntity.GetRelationships(rel, Direction.Forward);
            Assert.AreEqual(1, clonedRelationships.Count);            
            Assert.IsTrue(clonedRelationships.Any(r => r.Id == cloneIdRelated));
            Assert.IsTrue(clonedRelationships.Any(r => (string)r.GetField("core:name") == newEntityName));

            // Check initial entity is not touched
            IEntity initialEntity = Entity.Get(entity1.Id);
            var initialRelationships = initialEntity.GetRelationships(rel, Direction.Forward);
            Assert.AreEqual(1, initialRelationships.Count);
            Assert.IsTrue(initialRelationships.Any(r => r.Id == entityRelated.Id));
            Assert.IsTrue(initialRelationships.Any(r => (string)r.GetField("core:name") == initialName));
        }

        private T Create<T>( bool save = false ) where T : IEntity, new()
        {
            T res = new T( );
            if ( save )
                res.Save( );
            return res;
        }

        private IEntity Create<T>( EntityType type ) where T : IEntity, new( )
        {
            IEntity res = Entity.Create( type.Id );
            return res;
        }

        private void SetRel( IEntity source, IEntity target, IEntity rel, Direction direction = Direction.Forward )
        {
            IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					target
				};
            source.SetRelationships( rel, relationshipsCol, direction );
            source.Save( );
        }
	}
}