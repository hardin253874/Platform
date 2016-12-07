// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Services.Console;
using EDC.ReadiNow.Test.Security.AccessControl;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Services.Console
{
    [TestFixture]
	[RunWithTransaction]
    public class SecurityActionMenuItemFilterTests
    {
        [Test]
        public void Test_Ctor_Arg()
        {
            SecurityActionMenuItemFilter securityActionMenuItemFilter;
            Mock<IEntityAccessControlService> mockEntityAccessControlService;
            IEntityAccessControlService entityAccessControlService;

            mockEntityAccessControlService = new Mock<IEntityAccessControlService>(MockBehavior.Strict);
            entityAccessControlService = mockEntityAccessControlService.Object;

            securityActionMenuItemFilter = new SecurityActionMenuItemFilter(entityAccessControlService);

            mockEntityAccessControlService.VerifyAll();
            Assert.That(securityActionMenuItemFilter, Has.Property("Service").SameAs(entityAccessControlService));
        }

        [Test]
        public void Test_Ctor_NoArg()
        {
            Assert.That(new SecurityActionMenuItemFilter(), Has.Property("Service").Not.Null);
        }

        [Test]
        public void Test_Filter_NullActions()
        {
            Assert.That(() => new SecurityActionMenuItemFilter().Filter(-1, new long[0], null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("actions"));
        }

        [Test]
        public void Test_Filter_ActionsContainsNull()
        {
            Assert.That(() => new SecurityActionMenuItemFilter().Filter(-1, new long[0], new ActionMenuItemInfo[] { null }),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("actions"));
        }

        [Test]
        public void Test_Filter_NullSelectedResources()
        {
            Assert.That(() => new SecurityActionMenuItemFilter().Filter(-1, null, new ActionMenuItemInfo[0]),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("selectedResourceIds"));
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("console:viewResourceAction", new[] { "read" })]
        [TestCase("console:editResourceAction", new[] { "read", "modify" })]
        [TestCase("console:deleteResourceAction", new[] { "read", "delete" })]
        public void Test_Filter(string actionAlias, string[] expectedCheckedPermissionAliases)
        {
            SecurityActionMenuItemFilter securityActionMenuItemFilter;
            List<ActionMenuItemInfo> actions;
            Mock<IEntityAccessControlService> mockEntityAccessControlService;
            long id;

            id = Entity.GetId(actionAlias);
            actions = new List<ActionMenuItemInfo>
            {
                new ActionMenuItemInfo
                {
                    Id = id,
                    EntityId = id,
                    Children = null
                }
            };

            mockEntityAccessControlService = new Mock<IEntityAccessControlService>(MockBehavior.Strict);
            mockEntityAccessControlService.Setup(
                                            eacs => eacs.Check(
                                                It.Is<IList<EntityRef>>(list => list.SequenceEqual(new [] {new EntityRef(id)}, EntityRefComparer.Instance)),
                                                It.Is<IList<EntityRef>>(p => p.Select(er => er.Id).OrderBy(l => l).SequenceEqual(expectedCheckedPermissionAliases.Select(Entity.GetId).OrderBy(l => l)))))
                                          .Returns(new Dictionary<long, bool>{ {id, true} });
            mockEntityAccessControlService.Setup(eacs => eacs.Check(It.IsAny<EntityRef>(), It.IsAny<IList<EntityRef>>())).Returns(true);

            securityActionMenuItemFilter = new SecurityActionMenuItemFilter(mockEntityAccessControlService.Object);
            securityActionMenuItemFilter.Filter(-1, new[] { id }, actions);

            mockEntityAccessControlService.VerifyAll();
            Assert.That(actions, Has.Count.EqualTo(1), "Action menu item removed incorrectly");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Filter_Remove()
        {
            SecurityActionMenuItemFilter securityActionMenuItemFilter;
            List<ActionMenuItemInfo> actions;
            Mock<IEntityAccessControlService> mockEntityAccessControlService;
            long id;

            id = Entity.GetId("console:viewResourceAction");
            actions = new List<ActionMenuItemInfo>
            {
                new ActionMenuItemInfo
                {
                    Id = id,
                    EntityId = id,
                    Children = null
                }
            };

            mockEntityAccessControlService = new Mock<IEntityAccessControlService>(MockBehavior.Strict);
            mockEntityAccessControlService.Setup(
                                            eacs => eacs.Check(
                                                It.Is<IList<EntityRef>>(list => list.SequenceEqual(new [] {new EntityRef(id)}, EntityRefComparer.Instance)),
                                                It.IsAny<IList<EntityRef>>()))
                                          .Returns(new Dictionary<long, bool>{ {id, false} });
            mockEntityAccessControlService.Setup(eacs => eacs.Check(It.IsAny<EntityRef>(), It.IsAny<IList<EntityRef>>())).Returns(true);

            securityActionMenuItemFilter = new SecurityActionMenuItemFilter(mockEntityAccessControlService.Object);
            securityActionMenuItemFilter.Filter(-1, new[] { id }, actions);

            mockEntityAccessControlService.VerifyAll();
            Assert.That(actions, Is.Empty, "Action menu item not removed");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Filter_InvalidEntity()
        {
            SecurityActionMenuItemFilter securityActionMenuItemFilter;
            List<ActionMenuItemInfo> actions;
            Mock<IEntityAccessControlService> mockEntityAccessControlService;
            long id;

            id = Entity.GetId("console:viewResourceAction");
            actions = new List<ActionMenuItemInfo>
            {
                new ActionMenuItemInfo
                {
                    Id = id,
                    EntityId = 0,
                    Children = null
                }
            };

            mockEntityAccessControlService = new Mock<IEntityAccessControlService>(MockBehavior.Strict);
            mockEntityAccessControlService.Setup(
                                            eacs => eacs.Check(
                                                It.Is<IList<EntityRef>>(list => list.SequenceEqual(new[] { new EntityRef(id) }, EntityRefComparer.Instance)),
                                                It.IsAny<IList<EntityRef>>()))
                                          .Returns(new Dictionary<long, bool> { { id, true } });

            securityActionMenuItemFilter = new SecurityActionMenuItemFilter(mockEntityAccessControlService.Object);
            securityActionMenuItemFilter.Filter(-1, new[] { id }, actions);

            mockEntityAccessControlService.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Filter_Children()
        {
            SecurityActionMenuItemFilter securityActionMenuItemFilter;
            List<ActionMenuItemInfo> actions;
            Mock<IEntityAccessControlService> mockEntityAccessControlService;
            long parentId;
            long childId;

            parentId = Entity.GetId("console:viewResourceAction");
            childId = Entity.GetId("console:editResourceAction");
            actions = new List<ActionMenuItemInfo>
            {
                new ActionMenuItemInfo
                {
                    Id = parentId,
                    EntityId = parentId,
                    Children = new List<ActionMenuItemInfo>
                    {
                        new ActionMenuItemInfo()
                        {
                            Id = childId,
                            EntityId = childId,
                            Children = null
                        }
                    }
                }
            };

            mockEntityAccessControlService = new Mock<IEntityAccessControlService>(MockBehavior.Strict);
            mockEntityAccessControlService.Setup(
                                            eacs => eacs.Check(
                                                It.Is<IList<EntityRef>>(list => list.SequenceEqual(new [] {new EntityRef(parentId)}, EntityRefComparer.Instance)),
                                                It.IsAny<IList<EntityRef>>()))
                                          .Returns(new Dictionary<long, bool> { { parentId, true } });
            mockEntityAccessControlService.Setup(eacs => eacs.Check(It.IsAny<EntityRef>(), It.IsAny<IList<EntityRef>>())).Returns(true);

            securityActionMenuItemFilter = new SecurityActionMenuItemFilter(mockEntityAccessControlService.Object);
            securityActionMenuItemFilter.Filter(-1, new[] { parentId }, actions);

            mockEntityAccessControlService.VerifyAll();
            Assert.That(actions, Has.Count.EqualTo(1), "Action menu removed incorrectly");
            Assert.That(actions[0], Has.Property("Children").Count.EqualTo(1), "Child action menu removed incorrectly");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Filter_CreateNew()
        {
            SecurityActionMenuItemFilter securityActionMenuItemFilter;
            List<ActionMenuItemInfo> actions;
            Mock<IEntityAccessControlService> mockEntityAccessControlService;
            IEntity entity;

            entity = Entity.Get("console:actionMenuItem");
            actions = new List<ActionMenuItemInfo>
            {
                new ActionMenuItemInfo
                {
                    EntityId = entity.Id,
                    Children = null,
                    HtmlActionState = ActionService.CreateMenuItemActionState,
                    IsNew = true
                }
            };

            mockEntityAccessControlService = new Mock<IEntityAccessControlService>(MockBehavior.Strict);
            mockEntityAccessControlService.Setup(eacs => eacs.CanCreate(It.Is<EntityType>(et => new EntityEqualityComparer().Equals(et, entity))))
                                          .Returns(true);

            securityActionMenuItemFilter = new SecurityActionMenuItemFilter(mockEntityAccessControlService.Object);
            securityActionMenuItemFilter.Filter(-1, new[] { entity.Id }, actions);

            mockEntityAccessControlService.VerifyAll();
            Assert.That(actions, Has.Count.EqualTo(1), "Action menu removed incorrectly");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("core:read,core:modify", "core:create,core:read,core:modify,core:delete")]
        [TestCase("core:read,core:modify", "core:create,core:read")]
        [TestCase("core:read,core:modify", "core:create,core:read,core:modify")]
        [TestCase("core:read,core:modify", "core:create,core:read,core:delete")]
        [TestCase("core:read,core:modify", "core:read,core:modify,core:delete")]
        [TestCase("core:read,core:modify", "core:read")]
        [TestCase("core:read,core:modify", "core:read,core:modify")]
        [TestCase("core:read,core:modify", "core:read,core:delete")]
        [TestCase("core:read", "core:create,core:read,core:modify,core:delete")]
        [TestCase("core:read", "core:create,core:read")]
        [TestCase("core:read", "core:create,core:read,core:modify")]
        [TestCase("core:read", "core:create,core:read,core:delete")]
        [TestCase("core:read", "core:read,core:modify,core:delete")]
        [TestCase("core:read", "core:read")]
        [TestCase("core:read", "core:read,core:modify")]
        [TestCase("core:read", "core:read,core:delete")]
        [RunWithTransaction]
        public void Test_Filter_ActionRequiresParentModifyAccess(string parentEntityPermissions, string childEntityPermissions)
        {
            SecurityActionMenuItemFilter securityActionMenuItemFilter;
            UserAccount userAccount;
            EntityType parentEntityType;
            EntityType childEntityType;
            IEntity parentEntity;
            IEntity childEntity;
            
            const string viewResourceActionAlias = "console:viewResourceAction";
            const string editResourceActionAlias = "console:editResourceAction";
            const string deleteResourceActionAlias = "console:deleteResourceAction";
            const string addRelationshipActionAlias = "console:addRelationshipAction";
            const string removeRelationshipActionAlias = "console:removeRelationshipAction";

            var splitParentEntityPermissions = parentEntityPermissions.Split(new[] { ',' },
                StringSplitOptions.RemoveEmptyEntries);

            var splitChildEntityPermissions = childEntityPermissions.Split(new[] { ',' },
                StringSplitOptions.RemoveEmptyEntries);

            
            userAccount = new UserAccount();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.Save();

            // parent
            parentEntityType = new EntityType();
            parentEntityType.Inherits.Add(UserResource.UserResource_Type);
            parentEntityType.Save();

            parentEntity = Entity.Create(new EntityRef(parentEntityType));
            parentEntity.SetField("core:name", "A"); // "A" so it will match the access rule
            parentEntity.Save();

            // related child entity
            childEntityType = new EntityType();
            childEntityType.Inherits.Add(UserResource.UserResource_Type);
            childEntityType.Save();

            childEntity = Entity.Create(new EntityRef(childEntityType));
            childEntity.SetField("core:name", "B"); // "B" so it will match the access rule
            childEntity.Save();

            // grant accesses 
            // parent entity
            new AccessRuleFactory().AddAllowByQuery(
                userAccount.As<Subject>(),
                parentEntityType.As<SecurableEntity>(),
                splitParentEntityPermissions.Select(s => new EntityRef(s)),
                TestQueries.EntitiesWithNameA().ToReport());
            
            // child entity
            new AccessRuleFactory().AddAllowByQuery(
                userAccount.As<Subject>(),
                childEntityType.As<SecurableEntity>(),
                splitChildEntityPermissions.Select(s => new EntityRef(s)),
                TestQueries.EntitiesWithNameB().ToReport());

            // actions
            var dummyRequest = new ActionRequestExtended();
            Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> dummyHandler = (a, i) => new ActionTargetInfo();
            var actions = new List<ActionMenuItemInfo>();
            foreach (string menuItemAlias in new[]
            {
                viewResourceActionAlias,
                editResourceActionAlias,
                addRelationshipActionAlias,
                removeRelationshipActionAlias,
                deleteResourceActionAlias,
            })
            {
                actions.Add(Entity.Get<ActionMenuItem>(menuItemAlias).ToInfo(dummyRequest, null, dummyHandler));
            }

            actions.Add(new ActionMenuItemInfo
            {
                EntityId = childEntityType.Id,
                HtmlActionState = "createForm",
                IsNew = true
            });

            // filter actions
            using (new SetUser(userAccount))
            {
                securityActionMenuItemFilter = new SecurityActionMenuItemFilter();
                securityActionMenuItemFilter.Filter(parentEntity.Id, new[] { childEntity.Id }, actions);
            }

            // checks
            if (splitParentEntityPermissions.Contains("core:read") && splitParentEntityPermissions.Contains("core:modify"))
            {
                Assert.That(actions, Has.Exactly(1).Property("Alias").EqualTo(addRelationshipActionAlias), "Missing add relationship resource action");
                Assert.That(actions, Has.Exactly(1).Property("Alias").EqualTo(removeRelationshipActionAlias), "Missing remove relationship resource action");

                // child create
                if (splitChildEntityPermissions.Contains("core:create"))
                {
                    Assert.That(actions, Has.Exactly(1).Property("HtmlActionState").EqualTo("createForm"), "Missing create resource action");
                }
                else
                {
                    Assert.That(actions, Has.None.Property("HtmlActionState").EqualTo("createForm"), "Create resource action should not be available");
                }

                // child read
                if (splitChildEntityPermissions.Contains("core:read"))
                {
                    Assert.That(actions, Has.Exactly(1).Property("Alias").EqualTo(viewResourceActionAlias), "Missing view resource action");
                }
                else
                {
                    Assert.That(actions, Has.None.Property("Alias").EqualTo(viewResourceActionAlias), "View resource action should not be available");
                }

                // child modify
                if (splitChildEntityPermissions.Contains("core:modify"))
                {
                    Assert.That(actions, Has.Exactly(1).Property("Alias").EqualTo(editResourceActionAlias), "Missing edit resource action");
                }
                else
                {
                    Assert.That(actions, Has.None.Property("Alias").EqualTo(editResourceActionAlias), "Edit resource action should not be available");
                }

                // child delete
                if (splitChildEntityPermissions.Contains("core:delete"))
                {
                    Assert.That(actions, Has.Exactly(1).Property("Alias").EqualTo(deleteResourceActionAlias), "Missing delete resource action");
                }
                else
                {
                    Assert.That(actions, Has.None.Property("Alias").EqualTo(deleteResourceActionAlias), "Delete resource action should not be available");
                }
            }
            else if (splitParentEntityPermissions.Contains("core:read") && !splitParentEntityPermissions.Contains("core:modify"))
            {
                Assert.That(actions, Has.None.Property("Alias").EqualTo(addRelationshipActionAlias), "Add relationship action should not be available");
                Assert.That(actions, Has.None.Property("Alias").EqualTo(removeRelationshipActionAlias), "Remove relationship action should not be available");

                // child create
                Assert.That(actions, Has.None.Property("HtmlActionState").EqualTo("createForm"), "Create resource action should not be available");

                // child read
                if (splitChildEntityPermissions.Contains("core:read"))
                {
                    Assert.That(actions, Has.Exactly(1).Property("Alias").EqualTo(viewResourceActionAlias), "Missing view resource action");
                }
                else
                {
                    Assert.That(actions, Has.None.Property("Alias").EqualTo(viewResourceActionAlias), "View resource action should not be available");
                }

                // child modify
                if (splitChildEntityPermissions.Contains("core:modify"))
                {
                    Assert.That(actions, Has.Exactly(1).Property("Alias").EqualTo(editResourceActionAlias), "Missing edit resource action");
                }
                else
                {
                    Assert.That(actions, Has.None.Property("Alias").EqualTo(editResourceActionAlias), "Edit resource action should not be available");
                }

                // child delete
                Assert.That(actions, Has.None.Property("Alias").EqualTo(deleteResourceActionAlias), "Delete resource action should not be available");
            }
        }
    }
}
