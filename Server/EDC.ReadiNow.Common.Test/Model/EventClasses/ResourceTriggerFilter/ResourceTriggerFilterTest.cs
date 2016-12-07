// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using Autofac;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Test.Model.EventClasses.ResourceTriggerFilter
{
    [TestFixture]
    public class ResourceTriggerFilterTest
    {

        class TestHandler : IFilteredSaveEventHandler
        {
            public int ExpectedBeforeSaveCount { get; set; }
            public int ExpectedAfterSaveCount { get; set; }
            public int ExpectedBeforeReverseAddCount { get; set; }
            public int ExpectedAfterReverseAddCount { get; set; }
            public int ExpectedBeforeReverseRemoveCount { get; set; }
            public int ExpectedAfterReverseRemoveCount { get; set; }
            public int ExpectedBeforeDeleteCount { get; set; }
            public int ExpectedAfterDeleteCount { get; set; }

            int BeforeSaveCount { get; set; }
            int AfterSaveCount { get; set; }
            int BeforeReverseAddCount { get; set; }
            int AfterReverseAddCount { get; set; }
            int BeforeReverseRemoveCount { get; set; }
            int AfterReverseRemoveCount { get; set; }
            int BeforeDeleteCount { get; set; }
            int AfterDeleteCount { get; set; }

            public void OnAfterSave(ResourceTriggerFilterDef policy, IEntity entity, bool isNew, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
            {
                AfterSaveCount++;
            }

            public bool OnBeforeSave(ResourceTriggerFilterDef policy, IEntity entity, bool isNew, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels)
            {
                BeforeSaveCount++;
                return false;
            }

            public bool OnBeforeReverseAdd(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity otherEntity, IEntity entity, bool isNew)
            {
                BeforeReverseAddCount++;

                return false;
            }

            public void OnAfterReverseAdd(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity otherEntity, IEntity entity, bool isNew)
            {
                AfterReverseAddCount++;
            }
            public bool OnBeforeReverseRemove(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity otherEntity, IEntity entity)
            {
                BeforeReverseRemoveCount++;

                return false;
            }

            public void OnAfterReverseRemove(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity otherEntity, IEntity entity)
            {
                AfterReverseRemoveCount++;
            }


            bool IFilteredSaveEventHandler.OnBeforeDelete(ResourceTriggerFilterDef policy, IEntity entity)
            {
                BeforeDeleteCount++;

                return false;
            }

            void IFilteredSaveEventHandler.OnAfterDelete(ResourceTriggerFilterDef policy, IEntity entity)
            {
                AfterDeleteCount++;
            }

            public void Clear()
            {
                BeforeSaveCount = 0;
                AfterSaveCount = 0;
                BeforeReverseAddCount = 0;
                AfterReverseAddCount = 0;
                BeforeReverseRemoveCount = 0;
                AfterReverseRemoveCount = 0;
                BeforeDeleteCount = 0;
                AfterDeleteCount = 0;

            }
            public void Check()
            {
                Assert.That(BeforeSaveCount, Is.EqualTo(ExpectedBeforeSaveCount));
                Assert.That(AfterSaveCount, Is.EqualTo(ExpectedAfterSaveCount));
                Assert.That(BeforeReverseAddCount, Is.EqualTo(ExpectedBeforeReverseAddCount));
                Assert.That(AfterReverseAddCount, Is.EqualTo(ExpectedAfterReverseAddCount));
                Assert.That(BeforeReverseRemoveCount, Is.EqualTo(ExpectedBeforeReverseRemoveCount));
                Assert.That(AfterReverseRemoveCount, Is.EqualTo(ExpectedAfterReverseRemoveCount));
                Assert.That(BeforeDeleteCount, Is.EqualTo(ExpectedBeforeDeleteCount));
                Assert.That(AfterDeleteCount, Is.EqualTo(ExpectedAfterDeleteCount));

            }
        }

        class TestFactory : IFilteredTargetHandlerFactory
        {
            public TestHandler TestHandler {get;}
            public TestFactory(EntityType handledType, TestHandler testHandler)
            {
                HandledType = handledType;
                TestHandler = testHandler;
            }

            public static EntityType HandledType { get; set; }

            public IFilteredSaveEventHandler CreateSaveEventHandler()
            {
                return TestHandler;
            }

            public EntityType GetHandledType()
            {
                return HandledType;
            }
        }


        protected readonly List<long> _toDelete = new List<long>();


        [TestFixtureTearDown]
        public void TestFinalize()
        {
            // Note that due to dependency on transactions for the test we can not us runWithTransaction
            using (new TenantAdministratorContext("EDC"))
            {
                DateTime start = DateTime.Now;

                var deleteList = _toDelete.Distinct().ToList();

                if (deleteList.Count > 0)
                {
                    try
                    {
                        Entity.Delete(deleteList);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        // This can sometimes fail due to test errors or dirty data
                    }
                }

                Console.WriteLine("Deleted took: " + (DateTime.Now - start) + "  Items deleted: " + deleteList.Count);
            }
        }



        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void PolicyHandlerMapSetUp()
        {
            var dummyType = Entity.Create<EntityType>();
            dummyType.Name = "ResourceTriggerFilterTest type";
            dummyType.Save();

            var factory = new TestFactory(dummyType, new TestHandler());

            var policyCache = new ResourceTriggerFilterPolicyCache(factory.ToEnumerable());

            Assert.That(policyCache.PolicyTypeHandlerMap.Count(), Is.EqualTo(1));
        }



        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFire_OnCreate()
        {
            var testHandler = new TestHandler
            {
                ExpectedBeforeSaveCount = 1,
                ExpectedAfterSaveCount = 1,
            };

            TestTrigger(testHandler,

                (policy) => 
                {
                },
                (policy) =>
                {
                    var dummyObj = Entity.Create(policy.TriggeredOnType);
                    dummyObj.Save();
                    _toDelete.Add(dummyObj.Id);
                }
                );
        }


        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFire_OnDelete()
        {
            IEntity dummyObj = null;

            var testHandler = new TestHandler
            {
                ExpectedBeforeDeleteCount = 1,
                ExpectedAfterDeleteCount = 1,
            };

            TestTrigger(testHandler,
                (policy) =>
                {
                    dummyObj = Entity.Create(policy.TriggeredOnType);
                    dummyObj.Save();

                },
                (policy) =>
                {
                    dummyObj.Delete();
                }
                );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFire_OnUpdate()
        {
            IEntity dummyObj = null;

            var testHandler = new TestHandler
            {
                ExpectedBeforeSaveCount = 1,
                ExpectedAfterSaveCount = 1,
            };

            TestTrigger(testHandler,
                (policy) =>
                {
                    var writable = policy.AsWritable< ResourceTriggerFilterDef>();
                    writable.UpdatedFieldsToTriggerOn.Add(Resource.Name_Field.As<Field>());
                    writable.Save();

                    dummyObj = Entity.Create(policy.TriggeredOnType);
                    dummyObj.Save();
                    _toDelete.Add(dummyObj.Id);
                },
                (policy) =>
                {
                    var writable = dummyObj.AsWritable<Resource>();
                    writable.Name = "bob";
                    writable.Save();
                }
                );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFires_OnRelUpdate()
        {
            Relationship rel = null;
            IEntity dummyObj = null;
            IEntity dummyOtherObj = null;

            var testHandler = new TestHandler
            {
                ExpectedBeforeSaveCount = 1,
                ExpectedAfterSaveCount = 1,
            };

            TestTrigger(testHandler,
                (policy) =>
                {
                    var otherDummyType = Entity.Create<EntityType>();
                    otherDummyType.Save();
                    _toDelete.Add(otherDummyType.Id);

                    rel = Entity.Create<Relationship>();
                    rel.FromType = policy.TriggeredOnType;
                    rel.ToType = otherDummyType;
                    rel.RelType_Enum = RelTypeEnum_Enumeration.RelLookup;
                    rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
                    rel.Save();
                    _toDelete.Add(rel.Id);

                    var writablePolicy = policy.AsWritable<ResourceTriggerFilterDef>();
                    writablePolicy.UpdatedRelationshipsToTriggerOn.Add(rel);
                    writablePolicy.Save();


                    dummyObj = Entity.Create(policy.TriggeredOnType);
                    dummyObj.Save();
                    _toDelete.Add(dummyObj.Id);

                },
                (policy) =>
                {
                    var writable = dummyObj.AsWritable<IEntity>();
                    writable.GetRelationships(rel, Direction.Forward).Add(dummyOtherObj);
                    writable.Save();
                }
                );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFires_OnReverseRelUpdate()
        {
            Relationship rel = null;
            IEntity dummyObj = null;
            IEntity dummyOtherObj1 = null;
            IEntity dummyOtherObj2 = null;

            var testHandler = new TestHandler
            {
                ExpectedBeforeReverseAddCount = 1,
                ExpectedAfterReverseAddCount = 1,
            };

            TestTrigger(testHandler,
                (policy) =>
                {
                    var otherType = Entity.Create<EntityType>();
                    otherType.Save();
                    _toDelete.Add(otherType.Id);

                    rel = Entity.Create<Relationship>();
                    rel.FromType = policy.TriggeredOnType;
                    rel.ToType = otherType;
                    rel.RelType_Enum = RelTypeEnum_Enumeration.RelLookup;
                    rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
                    rel.Save();
                    _toDelete.Add(rel.Id);

                    var writablePolicy = policy.AsWritable<ResourceTriggerFilterDef>();
                    writablePolicy.UpdatedRelationshipsToTriggerOn.Add(rel);
                    writablePolicy.Save();

                    dummyOtherObj1 = Entity.Create(otherType);
                    dummyOtherObj1.Save();
                    _toDelete.Add(dummyOtherObj1.Id);

                    dummyOtherObj2 = Entity.Create(otherType);
                    dummyOtherObj2.Save();
                    _toDelete.Add(dummyOtherObj2.Id);

                    dummyObj = Entity.Create(policy.TriggeredOnType);
                    dummyObj.GetRelationships(rel, Direction.Reverse).Add(dummyOtherObj1);  // add an exsiting item to ensure we a are only triggering on the new one.
                    dummyObj.Save();
                    _toDelete.Add(dummyObj.Id);


                },
                (policy) =>
                {
                    var writable = dummyOtherObj2.AsWritable<IEntity>();
                    writable.GetRelationships(rel, Direction.Reverse).Add(dummyObj);
                    writable.Save();
                }
                );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFires_OnReverseRelUpdate2_bug27761()
        {
            Relationship rel = null;
            IEntity dummyObj1 = null;
            IEntity dummyObj2 = null;
            IEntity dummyOtherObj1 = null;

            var testHandler = new TestHandler
            {
                ExpectedBeforeReverseAddCount = 1,
                ExpectedAfterReverseAddCount = 1,
            };

            TestTrigger(testHandler,
                (policy) =>
                {
                    var otherType = Entity.Create<EntityType>();
                    otherType.Save();
                    _toDelete.Add(otherType.Id);

                    rel = Entity.Create<Relationship>();
                    rel.FromType = policy.TriggeredOnType;
                    rel.ToType = otherType;
                    rel.RelType_Enum = RelTypeEnum_Enumeration.RelLookup;
                    rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
                    rel.Save();
                    _toDelete.Add(rel.Id);

                    var writablePolicy = policy.AsWritable<ResourceTriggerFilterDef>();
                    writablePolicy.UpdatedRelationshipsToTriggerOn.Add(rel);
                    writablePolicy.Save();



                    dummyObj1 = Entity.Create(policy.TriggeredOnType);
                    dummyObj1.Save();
                    _toDelete.Add(dummyObj1.Id);

                    dummyObj2 = Entity.Create(policy.TriggeredOnType);
                    dummyObj2.Save();
                    _toDelete.Add(dummyObj2.Id);

                    dummyOtherObj1 = Entity.Create(otherType);
                    var rels = dummyOtherObj1.GetRelationships(rel);  // add an exsiting item to ensure we a are only triggering on the new one.
                    rels.Add(dummyObj1);
                    dummyOtherObj1.Save();
                    _toDelete.Add(dummyOtherObj1.Id);

                },
                (policy) =>
                {
                    var writable = dummyOtherObj1.AsWritable<IEntity>();
                    var rels = writable.GetRelationships(rel);
                    rels.Clear();                                                   // Only real changes should trigger.
                    rels.Add(dummyObj1);
                    rels.Add(dummyObj2);
                    writable.Save();
                }
                );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFires_OnReverseRelDelete()
        {
            Relationship rel = null;
            IEntity dummyObj = null;
            IEntity dummyOtherObj = null;

            var testHandler = new TestHandler
            { 
                ExpectedBeforeReverseRemoveCount = 1,
                ExpectedAfterReverseRemoveCount = 1,
            };

            TestTrigger(testHandler,
                (policy) =>
                {
                    var otherDummyType = Entity.Create<EntityType>();
                    otherDummyType.Save();
                    _toDelete.Add(otherDummyType.Id);

                    rel = Entity.Create<Relationship>();
                    rel.FromType = policy.TriggeredOnType;
                    rel.ToType = otherDummyType;
                    rel.RelType_Enum = RelTypeEnum_Enumeration.RelLookup;
                    rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
                    rel.Save();
                    _toDelete.Add(rel.Id);

                    var writablePolicy = policy.AsWritable<ResourceTriggerFilterDef>();
                    writablePolicy.UpdatedRelationshipsToTriggerOn.Add(rel);
                    writablePolicy.Save();

                    dummyObj = Entity.Create(policy.TriggeredOnType);
                    dummyObj.Save();
                    _toDelete.Add(dummyObj.Id);

                    dummyOtherObj = Entity.Create(otherDummyType);
                    dummyOtherObj.GetRelationships(rel, Direction.Reverse).Add(dummyObj);
                    dummyOtherObj.Save();
                    _toDelete.Add(dummyOtherObj.Id);
                },
                (policy) =>
                {
                    var writable = dummyOtherObj.AsWritable<IEntity>();
                    writable.GetRelationships(rel, Direction.Reverse).Clear();
                    writable.Save();
                }
                );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFires_OnDeleteOfOther()
        {
            Relationship rel = null;
            IEntity dummyObj = null;
            IEntity dummyOtherObj1 = null, dummyOtherObj2 = null;

            var testHandler = new TestHandler
            {
                ExpectedBeforeReverseRemoveCount = 2,
                ExpectedAfterReverseRemoveCount = 2,
            };

            TestTrigger(testHandler,
                (policy) =>
                {
                    var otherDummyType = Entity.Create<EntityType>();
                    otherDummyType.Save();
                    _toDelete.Add(otherDummyType.Id);

                    rel = Entity.Create<Relationship>();
                    rel.FromType = policy.TriggeredOnType;
                    rel.ToType = otherDummyType;
                    rel.RelType_Enum = RelTypeEnum_Enumeration.RelLookup;
                    rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                    rel.Save();
                    _toDelete.Add(rel.Id);

                    var writablePolicy = policy.AsWritable<ResourceTriggerFilterDef>();
                    writablePolicy.UpdatedRelationshipsToTriggerOn.Add(rel);
                    writablePolicy.Save();

                    dummyObj = Entity.Create(policy.TriggeredOnType);
                    dummyObj.Save();
                    _toDelete.Add(dummyObj.Id);

                    dummyOtherObj1 = Entity.Create(otherDummyType);
                    dummyOtherObj1.GetRelationships(rel, Direction.Reverse).Add(dummyObj);
                    dummyOtherObj1.Save();
                    _toDelete.Add(dummyOtherObj1.Id);

                    dummyOtherObj2 = Entity.Create(otherDummyType);
                    dummyOtherObj2.GetRelationships(rel, Direction.Reverse).Add(dummyObj);
                    dummyOtherObj2.Save();
                    _toDelete.Add(dummyOtherObj2.Id);
                },
                (policy) =>
                {
                    Entity.Delete(dummyOtherObj1.Id);
                    Entity.Delete(dummyOtherObj2.Id);
                }
                );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RegisteredTriggerHandlersFires_NothingForNothing()
        {
            IEntity dummyOtherObj1 = null;

            var testHandler = new TestHandler
            {
            };

            TestTrigger(testHandler,
                (policy) =>
                {
                    var otherDummyType = Entity.Create<EntityType>();
                    otherDummyType.Save();
                    _toDelete.Add(otherDummyType.Id);
                    
                    dummyOtherObj1 = Entity.Create(otherDummyType);
                    dummyOtherObj1.Save();
                    _toDelete.Add(dummyOtherObj1.Id);
                },
                (policy) =>
                {
                    Entity.Delete(dummyOtherObj1.Id);
                }
                );
        }

        void TestTrigger(TestHandler testHandler,
            Action<ResourceTriggerFilterDef> configurePolicyAction,
            Action<ResourceTriggerFilterDef> doStuff)
        {
            var dummyPolicyType = Entity.Create<EntityType>();
            dummyPolicyType.Name = "ResourceTriggerFilterTest type";
            dummyPolicyType.Inherits.Add(Entity.Get<EntityType>("core:resourceTriggerFilterDef"));
            dummyPolicyType.Save();
            _toDelete.Add(dummyPolicyType.Id);

            var factory = new TestFactory(dummyPolicyType, testHandler);

            var policyCache = new ResourceTriggerFilterPolicyCache(factory.ToEnumerable());

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => factory).As<IFilteredTargetHandlerFactory>();
                builder.Register(ctx => policyCache).As<IResourceTriggerFilterPolicyCache>();

            }))
            using (Factory.SetCurrentScope(scope))
            {
                var dummyTriggerType = Entity.Create<EntityType>();
                dummyTriggerType.Save();
                _toDelete.Add(dummyTriggerType.Id);

                var policy = Entity.Create(dummyPolicyType.Id).As<ResourceTriggerFilterDef>();
                policy.TriggerEnabled = true;
                policy.TriggeredOnType = dummyTriggerType;
                policy.Save();
                _toDelete.Add(policy.Id);

                configurePolicyAction(policy);

                testHandler.Clear();

                doStuff(policy);

                testHandler.Check();
            }

        }

        [Explicit]
        [Test]
        public void TriggerFiresOnDerivedType()
        {
            Assert.Fail();
        }

        [Explicit]
        [Test]
        public void TriggerFiresOnDerivedTypeCreatedAfterPolicy()
        {
            //TO BE IMPLEMENTED
            Assert.Fail();
        }
    }
}
