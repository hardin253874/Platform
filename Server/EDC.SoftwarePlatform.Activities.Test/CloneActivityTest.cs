// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using FluentAssertions;
using NUnit.Framework;
using EDC.ReadiNow.Core.Cache;
using System;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    [RunWithTransaction]
	public class CloneActivityTest : TestBase
	{
		[Test]
        //[Ignore("Karen broke this test with changes to shared:(")]
		[RunAsDefaultTenant]
		public void CloneWithDefinition( )
		{
            var store1Alias = Entity.Get("test:aaWoolworths");
            var storeRefAlias = Entity.Get("test:aaStoresSnacks");
            
			var cloneActivity = new CloneActivity( );
			cloneActivity.Save( );
			ToDelete.Add( cloneActivity.Id );

            var storeDefList = Entity.GetByName<EntityType>("AA_Stores");

            Assert.IsTrue(storeDefList.Count() == 1);
            var storeDef = storeDefList.First();



			var nextActivity = ( CloneImplementation ) cloneActivity.As<WfActivity>( ).CreateWindowsActivity( );

			var inputs = new Dictionary<string, object>
				{
					{
						"Record",  store1Alias
					},
					{
						"Definition of new Resource", storeDef
					}
				};

			IDictionary<string, object> result = RunActivity( nextActivity, inputs );

			var clone = ( IEntity ) result[ "Record" ];



            Assert.IsTrue(clone.TypeIds.Contains(storeDef.Id), "Clone has the correct type Id");

		    var singleLine = new EntityRef("core:name");
            var snack = new EntityRef("test:aaStoresSnacks");
            Assert.AreEqual("Woolworths", clone.GetField<string>(singleLine), "The clone has copied the single line field correctly");

            var store1 = Entity.Get(store1Alias.Id, Resource.Name_Field );
            var allFieldsRel = store1.GetRelationships(snack, Direction.Reverse);
            var cloneRel = clone.GetRelationships(snack, Direction.Reverse);
 		}

        /// <summary>
        /// Clone an entity that has a one-to-one relationship and extended target type
        /// Bug 26792: Workflow: A clone fails if the source object has a one to one lookup and both clone actions are set to 'Clone Entities"
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
	    public void CloneWithOneToOneRelationship()
        {
            var x = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss");

            #region Arrange

            var colour = new Definition();
            colour.Name = "Colour_" + x;
            colour.Save();
            var objectA = new Definition();
            objectA.Name = "ObjectA_" + x;
            objectA.Save();
            var objectB = new Definition();
            objectB.Name = "ObjectB_" + x;
            objectB.Inherits.Add(objectA.As<EntityType>());
            objectB.Save();

            var relationship = new Relationship();
            relationship.Name = "Object To Colour " + x;
            relationship.FromType = objectA.As<EntityType>();
            relationship.ToType = colour.As<EntityType>();
            relationship.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
            relationship.CloneAction_Enum = CloneActionEnum_Enumeration.CloneEntities;
            relationship.ReverseCloneAction_Enum = CloneActionEnum_Enumeration.CloneEntities;
            relationship.Save();

            var red = Entity.Create(colour.Id);
            red.Save();
            red.SetField(new EntityRef("core", "name"), "Red." + red.Id);
            red.Save();
            var blue = Entity.Create(colour.Id);
            blue.Save();
            blue.SetField(new EntityRef("core", "name"), "Blue." + blue.Id);
            blue.Save();
            var green = Entity.Create(colour.Id);
            green.Save();
            green.SetField(new EntityRef("core", "name"), "Green." + green.Id);
            green.Save();

            var colours = new EntityRelationshipCollection<IEntity>();
            colours.Add(blue);

            var instanceA = Entity.Create(objectA.Id);
            instanceA.Save();
            instanceA.SetField(new EntityRef("core", "name"), "A." + instanceA.Id);
            instanceA.SetRelationships(relationship, colours);
            instanceA.Save();

            // Workflow Setup
            var cloneActivity = new CloneActivity();
            cloneActivity.Save();

            var nextActivity = (CloneImplementation)cloneActivity.As<WfActivity>().CreateWindowsActivity();
            var inputs = new Dictionary<string, object>
            {
                { "Record", instanceA },
                { "Object", objectB } // core:newDefinitionCloneArgument
            };

            ToDelete.Add(cloneActivity.Id);
            ToDelete.Add(instanceA.Id);
            ToDelete.Add(red.Id);
            ToDelete.Add(blue.Id);
            ToDelete.Add(green.Id);
            ToDelete.Add(relationship.Id);
            ToDelete.Add(objectB.Id);
            ToDelete.Add(objectA.Id);
            ToDelete.Add(colour.Id);

            #endregion

            var result = RunActivity(nextActivity, inputs);
            var clone = ((IEntity)result["Record"]);
            var original = Entity.Get(instanceA.Id);
            var originalColour = Entity.Get(blue.Id);

            clone.Id.Should().NotBe(original.Id);
            
            ToDelete.Add(clone.Id);

            clone.GetField(new EntityRef("core", "name")).Should().Be("A." + instanceA.Id);

            var related = clone.GetRelationships(relationship);
            related.Should().NotBeNull().And.NotBeEmpty();
            related.Count.Should().Be(1);

            var cloneColour = related.First();

            cloneColour.Id.Should().NotBe(originalColour.Id);

            ToDelete.Add(cloneColour.Id);

            cloneColour.GetField(new EntityRef("core", "name")).Should().Be("Blue." + blue.Id);
        }

        /// <summary>
        /// Clone the relationships from an inherited base type, between to definitions that inherit from it.
        /// Bug 26717: Workflow - Cloning records with children is duplicating link back to original record.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
	    public void CloneWithBaseDefinition()
        {
            var x = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss");

            #region Arrange

	        // Types
            var userResource = Entity.Get<EntityType>(new EntityRef("core", "userResource"));
            var myBaseDefinition = new Definition { Name = "clonebasedefinition" + x, Inherits = new EntityCollection<EntityType>(userResource.ToEnumerable()) };
            myBaseDefinition.Save();
            
            var myRelatedDefinition = new Definition { Name = "clonerelateddefinition" + x, Inherits = new EntityCollection<EntityType>(userResource.ToEnumerable()) };
            myRelatedDefinition.Save();

            var myBaseType = Entity.Get<EntityType>(myBaseDefinition);
            var myRelatedType = Entity.Get<EntityType>(myRelatedDefinition);
            var myBaseToRelatedRelationship = new Relationship
            {
                FromType = myBaseType,
                ToType = myRelatedType,
                Name = "clonebaserelationship" + x,
                Description = "Clone With Base Definition",
                Cardinality = Entity.Get<CardinalityEnum>(new EntityRef("core", "oneToMany")),
                CascadeDelete = false,
                CascadeDeleteTo = true,
                CloneAction_Enum = CloneActionEnum_Enumeration.CloneEntities,
                ReverseCloneAction_Enum = CloneActionEnum_Enumeration.CloneReferences
            };
            myBaseToRelatedRelationship.Save();

            var myDefinitionA = new Definition { Name = "clonedefinitionA" + x, Inherits = new EntityCollection<EntityType>(myBaseType.ToEnumerable()) };
            myDefinitionA.Save();

            var myDefinitionB = new Definition { Name = "clonedefinitionB" + x, Inherits = new EntityCollection<EntityType>(myBaseType.ToEnumerable()) };
            myDefinitionB.Save();

            // Instances
            var rOne = new Entity(myRelatedDefinition);
            rOne.SetField(new EntityRef("core", "name"), "Related 1." + x);
            rOne.Save();

            var rTwo = new Entity(myRelatedDefinition);
            rTwo.SetField(new EntityRef("core", "name"), "Related 2." + x);
            rTwo.Save();

            var rCollection = new EntityRelationshipCollection<IEntity>();
            rCollection.Add(rOne);
            rCollection.Add(rTwo);

            var cA = new Entity(myDefinitionA);
            cA.SetField(new EntityRef("core", "name"), "Clone Me A." + x);
            cA.SetRelationships(myBaseToRelatedRelationship, rCollection);
            cA.Save();
            
            // Workflow Setup
            var cloneActivity = new CloneActivity();
            cloneActivity.Save();

            var nextActivity = (CloneImplementation)cloneActivity.As<WfActivity>().CreateWindowsActivity();
            var inputs = new Dictionary<string, object>
            {
                { "Record", cA },
                { "Object", myDefinitionB } // core:newDefinitionCloneArgument
            };

            ToDelete.Add(cloneActivity.Id);
            ToDelete.Add(cA.Id);
            ToDelete.Add(myDefinitionA.Id);
            ToDelete.Add(myDefinitionB.Id);
            ToDelete.Add(myRelatedDefinition.Id);
            ToDelete.Add(myBaseDefinition.Id);

            #endregion

            var result = RunActivity(nextActivity, inputs);
            var clone = ((IEntity)result["Record"]);
            var original = Entity.Get(cA.Id);

            clone.Id.Should().NotBe(original.Id);

            ToDelete.Add(clone.Id);

            var rid = myBaseToRelatedRelationship.Id;

            var cloneRels = clone.GetRelationships(rid, Direction.Forward).ToList();
            cloneRels.Count.Should().Be(2);
            foreach (var cloneRel in cloneRels)
            {
                var revs = cloneRel.GetRelationships(rid, Direction.Reverse);
                revs.Select(r => r.Id).Should().NotContain(original.Id);
            }

            var originalRels = original.GetRelationships(rid, Direction.Forward).ToList();

            originalRels.Count.Should().Be(2);
            originalRels.Select(o => o.Id).Should().NotIntersectWith(cloneRels.Select(c => c.Id));
        }

        /// <summary>
        /// Clone the relationships from an inherited base type, between to definitions that inherit from it.
        /// Bug 26718: Workflow - Cloning records with children by reference is not duplicating the links.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
	    public void CloneRefsWithBaseDefinition()
	    {
            var x = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss");

            #region Arrange

            // Types
            var userResource = Entity.Get<EntityType>(new EntityRef("core", "userResource"));
            var myBaseDefinition = new Definition { Name = "clonebasedefinition" + x, Inherits = new EntityCollection<EntityType>(userResource.ToEnumerable()) };
            myBaseDefinition.Save();

            var myRelatedDefinition = new Definition { Name = "clonerelateddefinition" + x, Inherits = new EntityCollection<EntityType>(userResource.ToEnumerable()) };
            myRelatedDefinition.Save();

            var myBaseType = Entity.Get<EntityType>(myBaseDefinition);
            var myRelatedType = Entity.Get<EntityType>(myRelatedDefinition);
            var myBaseToRelatedRelationship = new Relationship
            {
                FromType = myBaseType,
                ToType = myRelatedType,
                Name = "clonebaserelationship" + x,
                Description = "Clone With Base Definition",
                Cardinality = Entity.Get<CardinalityEnum>(new EntityRef("core", "manyToMany")),
                CascadeDelete = false,
                CascadeDeleteTo = false,
                CloneAction_Enum = CloneActionEnum_Enumeration.CloneReferences,
                ReverseCloneAction_Enum = CloneActionEnum_Enumeration.CloneReferences
            };
            myBaseToRelatedRelationship.Save();

            var myDefinitionA = new Definition { Name = "clonedefinitionA" + x, Inherits = new EntityCollection<EntityType>(myBaseType.ToEnumerable()) };
            myDefinitionA.Save();

            var myDefinitionB = new Definition { Name = "clonedefinitionB" + x, Inherits = new EntityCollection<EntityType>(myBaseType.ToEnumerable()) };
            myDefinitionB.Save();

            // Instances
            var rOne = new Entity(myRelatedDefinition);
            rOne.SetField(new EntityRef("core", "name"), "Related 1." + x);
            rOne.Save();

            var rTwo = new Entity(myRelatedDefinition);
            rTwo.SetField(new EntityRef("core", "name"), "Related 2." + x);
            rTwo.Save();

            var rCollection = new EntityRelationshipCollection<IEntity>();
            rCollection.Add(rOne);
            rCollection.Add(rTwo);

            var cA = new Entity(myDefinitionA);
            cA.SetField(new EntityRef("core", "name"), "Clone Me A." + x);
            cA.SetRelationships(myBaseToRelatedRelationship, rCollection);
            cA.Save();

            // Workflow Setup
            var cloneActivity = new CloneActivity();
            cloneActivity.Save();

            var nextActivity = (CloneImplementation)cloneActivity.As<WfActivity>().CreateWindowsActivity();
            var inputs = new Dictionary<string, object>
            {
                { "Record", cA },
                { "Object", myDefinitionB } // core:newDefinitionCloneArgument
            };

            ToDelete.Add(cloneActivity.Id);
            ToDelete.Add(cA.Id);
            ToDelete.Add(myDefinitionA.Id);
            ToDelete.Add(myDefinitionB.Id);
            ToDelete.Add(myRelatedDefinition.Id);
            ToDelete.Add(myBaseDefinition.Id);

            #endregion

            var result = RunActivity(nextActivity, inputs);
            var clone = ((IEntity)result["Record"]);
            var original = Entity.Get(cA.Id);

            clone.Id.Should().NotBe(original.Id);

            ToDelete.Add(clone.Id);

            var rid = myBaseToRelatedRelationship.Id;

            var cloneRels = clone.GetRelationships(rid, Direction.Forward).ToList();
            cloneRels.Count.Should().Be(2);
            foreach (var cloneRel in cloneRels)
            {
                var revs = cloneRel.GetRelationships(rid, Direction.Reverse);
                revs.Select(r => r.Id).Should().Contain(new [] { original.Id, clone.Id });
            }

            var originalRels = original.GetRelationships(rid, Direction.Forward).ToList();

            originalRels.Count.Should().Be(2);
            originalRels.Select(o => o.Id).Should().BeEquivalentTo(cloneRels.Select(c => c.Id));
        }

		[Test]
		[RunAsDefaultTenant]        
		public void CloneWithoutDefinition( )
		{
			var person = CodeNameResolver.GetInstance("Jude Jacobs", "AA_Employee");

			var cloneActivity = new CloneActivity( );
			cloneActivity.Save( );
			ToDelete.Add( cloneActivity.Id );

			var nextActivity = ( CloneImplementation ) cloneActivity.As<WfActivity>( ).CreateWindowsActivity( );

			var inputs = new Dictionary<string, object>
				{
					{
						"Record", person
					}
				};

			IDictionary<string, object> result = RunActivity( nextActivity, inputs );

			var clone = ( IEntity ) result[ "Record" ];


			var ignoredFields = new List<string>
				{
					"name",
					"alias",
					"createdDate",
					"modifiedDate"
				};
			foreach ( IEntity field in Person.AllFields.Where( f => f.Is<Field>( ) && ignoredFields.All( a => a != f.Alias ) ) )
			{
				Assert.AreEqual( person.GetField( field ), clone.GetField( field ), "Fields are not equal:" + field.Alias );
			}
		}

        [Test]
        [RunAsDefaultTenant]
        [RunWithoutTransaction]
        public void CloneWithResourceKey_Bug_27477()
        {

            var sibling1Type = new EntityType { Name = "sibling1Type type" };
            sibling1Type.Save();
            ToDelete.Add(sibling1Type.Id);

            var sibling2Type = new EntityType { Name = "sibling2Type type" };
            sibling2Type.Save();
            ToDelete.Add(sibling2Type.Id);

            var resKey = new ResourceKey { Name = "CloneWithResourceKey", KeyAppliesToType = sibling1Type, KeyFields = { ResourceKey.Name_Field.As<Field>() }, MergeDuplicates = false, ResourceKeyMessage = "Blah" };
            resKey.Save();
            ToDelete.Add(resKey.Id);

            var sibling1 = Entity.Create(sibling1Type).As<Resource>();
            sibling1.Name = "sibling1";
            sibling1.Save();
            ToDelete.Add(sibling1.Id);


            var clone = CloneImplementation.CreateClone(sibling1, sibling2Type, (e) => e.SetField(Resource.Name_Field, sibling1.Name + "_2"));
            clone.Save();

            ToDelete.Add(clone.Id);

            var hashes = clone.As<Resource>().ResourceHasResourceKeyDataHashes;

            Assert.That(hashes, Is.Empty);
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithoutTransaction]
        public void CloneWithResourceKey_Bug_27477_2()
        {
            var parentType = new EntityType { Name = "parent type" };
            parentType.Inherits.Add(Resource.Resource_Type);
            parentType.Save();
            ToDelete.Add(parentType.Id);

            var sibling1Type = new EntityType { Name = "sibling1Type type" };
            sibling1Type.Inherits.Add(parentType);
            sibling1Type.Save();
            ToDelete.Add(sibling1Type.Id);

            var sibling2Type = new EntityType { Name = "sibling2Type type" };
            sibling2Type.Inherits.Add(parentType);
            sibling2Type.Save();
            ToDelete.Add(sibling2Type.Id);

            var resKey = new ResourceKey { Name = "CloneWithResourceKey", KeyAppliesToType = sibling1Type, KeyFields = { ResourceKey.Name_Field.As<Field>() }, MergeDuplicates = false, ResourceKeyMessage = "Blah" };
            resKey.Save();
            ToDelete.Add(resKey.Id);

            var sibling1 = Entity.Create(sibling1Type).As<Resource>();
            sibling1.Name = "sibling1";
            sibling1.Save();
            ToDelete.Add(sibling1.Id);

            var cloneActivity = new CloneActivity();
            cloneActivity.InputArguments.Add(new ResourceArgument { Name = "1" }.Cast<ActivityArgument>());
            cloneActivity.InputArguments.Add(new IntegerArgument { Name = "1_value" }.Cast<ActivityArgument>());
            cloneActivity.Save();
            ToDelete.Add(cloneActivity.Id);


            var nextActivity = (CloneImplementation)cloneActivity.As<WfActivity>().CreateWindowsActivity();

            var inputs = new Dictionary<string, object>
                {
                    {
                        "Record",  sibling1
                    },
                    {
                        "Definition of new Resource", sibling2Type
                    },
                                        {
                        "1_value", "sibling2"
                    },
                    {
                        "1", (new EntityRef(Resource.Name_Field.Id)).Entity
                    }
                };

            IDictionary<string, object> result = RunActivity(nextActivity, inputs);

            var clone = (IEntity)result["Record"];

            // This should trigger the failure
            var clone2 = clone.AsWritable<Resource>();
            clone2.Name = "Test";
            clone2.Save();

        }

        [Test]
        [RunAsDefaultTenant]
        //[Ignore("This test currently fails - waiting for task Id")]
        public void CloneWorkflow()
        {
            var wf = CreateDoubleLogWorkflow();

            var cloneActivity = new CloneActivity();
            cloneActivity.InputArguments.Add(new ResourceArgument { Name = "1" }.Cast<ActivityArgument>());
            cloneActivity.InputArguments.Add(new IntegerArgument { Name = "1_value" }.Cast<ActivityArgument>());

            cloneActivity.Save();
            ToDelete.Add(cloneActivity.Id);

            var nextActivity = (CloneImplementation)cloneActivity.As<WfActivity>().CreateWindowsActivity();

            var inputs = new Dictionary<string, object>
				{
					{
						"Record", wf
					},
                    {
                        "1_value", "CLONE-" + wf.Name
                    },
                    {
                        "1", (new EntityRef(Resource.Name_Field.Id)).Entity
                    }
				};

            IDictionary<string, object> result = RunActivity(nextActivity, inputs);

            var clone = (IEntity)result["Record"];

            var wfId = wf.Id;
            var cloneId = clone.Id;

            CacheManager.ClearCaches();

            wf = Entity.Get<Workflow>(wfId);
            var cloneWf = Entity.Get<Workflow>(cloneId);
            Assert.AreEqual(wf.Transitions.Count, cloneWf.Transitions.Count);
            Assert.AreEqual(wf.Terminations.Count, cloneWf.Terminations.Count);
            VerifyWorkflow(wf);
            VerifyWorkflow(cloneWf);

            var wfAllIds = wf.ContainedActivities.Select(a => a.Id);
            var cloneAllIds = cloneWf.ContainedActivities.Select(a => a.Id);

            Assert.IsEmpty(wfAllIds.Intersect(cloneAllIds));

            Assert.AreNotEqual(wf.FirstActivity.Id, cloneWf.FirstActivity.Id);

        }

        protected Workflow CreateDoubleLogWorkflow()
        {
            var workflow = new Workflow
            {
                Name = string.Format("{0}:{1}", "DoubleLog", DateTime.Now)
            };

            workflow
                .AddDefaultExitPoint()
                .AddLog("Log 1", "Log 1")
                .AddLog("Log 2", "Log 2")
                .Save();

            ToDelete.Add(workflow.Id);
            return workflow;
        }

        void VerifyWorkflow(Workflow wf)
        {
            var allActId = wf.ContainedActivities.Select(a => a.Id);

            foreach (var tran in wf.Transitions)
            {
                Assert.IsNotNull(tran.FromActivity);
                Assert.IsNotNull(tran.FromExitPoint);
                Assert.IsNotNull(tran.ToActivity);
                Assert.IsTrue(allActId.Contains(tran.FromActivity.Id), "from activity within workflow");
                Assert.IsTrue(allActId.Contains(tran.ToActivity.Id), "to activity within workflow");
            }


            foreach (var term in wf.Terminations)
            {
                Assert.IsNotNull(term.FromActivity);
                Assert.IsNotNull(term.FromExitPoint);
                Assert.IsTrue(allActId.Contains(term.FromActivity.Id), "from activity within workflow");
            }
        }
	}
}