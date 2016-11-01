// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    /// <summary>
    /// Test whether relationship instances can be read and/or modified in various scenarios.
    /// </summary>
    [TestFixture]
    class RelationshipSecurityTests
    {
        // The following tests represent the status quot. It may not be what we actually want.
        // Summary:
        // 1. Reading an instance requires read at both ends
        // 2. Modify only requires modify from the end you're modifying (which means that sometimes a given instance can be created, but only if done from the right end)
        // 3. Modify (add/remove) typically requires view to the other end, so you can say what you want to add/remove
        // 4. Except for 'clear', which doesn't even require read at the other end. This would apply, for example, when changing a lookup.
        [TestCase("Read", "read", "read", Direction.Forward, false, false, true)]
        [TestCase("Read", "read", "read", Direction.Reverse, false, false, true)]
        [TestCase("Read", "read", "", Direction.Forward, false, false, false)]
        [TestCase("Read", "", "read", Direction.Reverse, false, false, false)]
        [TestCase("Create", "modify", "modify", Direction.Forward, false, false, true)]
        [TestCase("Create", "modify", "modify", Direction.Reverse, false, false, true)]
        [TestCase("Create", "modify", "read", Direction.Forward, false, false, true)]
        [TestCase("Create", "read", "modify", Direction.Reverse, false, false, true)]
        [TestCase("Create", "modify", "", Direction.Forward, false, false, false)]
        [TestCase("Create", "", "modify", Direction.Reverse, false, false, false)]
        [TestCase("Create", "modify", "read", Direction.Reverse, true, false, true)]
        [TestCase("Create", "modify", "read", Direction.Reverse, false, false, false)]
        [TestCase("Create", "modify", "read", Direction.Reverse, true, true, false)]
        [TestCase("Create", "read", "modify", Direction.Forward, true, false, true)]        
        [TestCase("Create", "read", "modify", Direction.Forward, false, false, false)]
        [TestCase("Create", "read", "modify", Direction.Forward, true, true, false)]
        [TestCase("Remove", "modify", "modify", Direction.Forward, false, false, true)]
        [TestCase("Remove", "modify", "modify", Direction.Reverse, false, false, true)]
        [TestCase("Remove", "modify", "read", Direction.Forward, false, false, true)]
        [TestCase("Remove", "read", "modify", Direction.Reverse, false, false, true)]
        [TestCase("Remove", "modify", "", Direction.Forward, false, false, false)]
        [TestCase("Remove", "", "modify", Direction.Reverse, false, false, false)]
        [TestCase("Remove", "modify", "read", Direction.Reverse, false, false, false)]
        [TestCase("Remove", "modify", "read", Direction.Reverse, true, false, true)]
        [TestCase("Remove", "modify", "read", Direction.Reverse, true, true, false)]
        [TestCase("Remove", "read", "modify", Direction.Forward, false, false, false)]        
        [TestCase("Remove", "read", "modify", Direction.Forward, true, false, true)]
        [TestCase("Remove", "read", "modify", Direction.Forward, true, true, false)]
        [TestCase("Clear", "modify", "modify", Direction.Forward, false, false, true)]
        [TestCase("Clear", "modify", "modify", Direction.Reverse, false, false, true)]
        [TestCase("Clear", "modify", "read", Direction.Forward, false, false, true)]
        [TestCase("Clear", "read", "modify", Direction.Reverse, false, false, true)]
        [TestCase("Clear", "modify", "", Direction.Forward, false, false, true)]
        [TestCase("Clear", "", "modify", Direction.Reverse, false, false, true)]
        [TestCase("Clear", "modify", "read", Direction.Reverse, true, false, true)]
        [TestCase("Clear", "modify", "read", Direction.Reverse, false, false, false)]
        [TestCase("Clear", "modify", "read", Direction.Reverse, true, true, false)]
        [TestCase("Clear", "read", "modify", Direction.Forward, true, false, true)]
        [TestCase("Clear", "read", "modify", Direction.Forward, false, false, false)]
        [TestCase("Clear", "read", "modify", Direction.Forward, true, true, false)]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_RelationshipInstance(string action, string fromPerms, string toPerms, Direction direction, bool saveBothEnds, bool haveFieldChanges, bool expectAllow)
        {
            if (fromPerms == "modify")
                fromPerms = "read,modify";
            if (toPerms == "modify")
                toPerms = "read,modify";

            // Create schema
            EntityType fromType = new EntityType();
            fromType.Inherits.Add(UserResource.UserResource_Type);
            fromType.Name = Guid.NewGuid().ToString();
            fromType.Save();

            EntityType toType = new EntityType();
            toType.Inherits.Add(UserResource.UserResource_Type);
            toType.Name = Guid.NewGuid().ToString();
            toType.Save();

            Relationship rel = new Relationship();
            rel.Name = Guid.NewGuid().ToString();
            rel.FromType = fromType;
            rel.ToType = toType;
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
            rel.Save();

            // Create data
            IEntity toInst = new Entity(toType);
            toInst.Save();
            IEntity fromInst = new Entity(fromType);
            if (action != "Create")
            {
                fromInst.SetRelationships(rel, new EntityRelationshipCollection<IEntity> { toInst });
            }
            fromInst.Save();

            // Create test user
            UserAccount userAccount = Entity.Create<UserAccount>();
            userAccount.Name = Guid.NewGuid().ToString();
            userAccount.Save();

            // Grant access
            if (!string.IsNullOrEmpty(fromPerms))
            { 
                new AccessRuleFactory().AddAllowByQuery(
                        userAccount.As<Subject>(),
                        fromType.As<SecurableEntity>(),
                        fromPerms.Split(',').Select(pa => new EntityRef(pa)),
                        TestQueries.Entities().ToReport());
            }
            if (!string.IsNullOrEmpty(toPerms))
            {
                new AccessRuleFactory().AddAllowByQuery(
                        userAccount.As<Subject>(),
                        toType.As<SecurableEntity>(),
                        toPerms.Split(',').Select(pa => new EntityRef(pa)),
                        TestQueries.Entities().ToReport());
            }

            // Test

            bool allowed = false;

            try
            {
                using (new SetUser(userAccount))
                {
                    IEntity source = Entity.Get(direction == Direction.Forward ? fromInst.Id : toInst.Id);
                    if (action != "Read")
                    {
                        source = source.AsWritable();
                    }
                    Func<IEntity> target = () => Entity.Get(direction == Direction.Forward ? toInst.Id : fromInst.Id);

                    IEntityRelationshipCollection<IEntity> relCol = null;

                    switch (action)
                    {
                        case "Read":
                            relCol = source.GetRelationships(rel.Id, direction);
                            IEntity entity = relCol.FirstOrDefault();
                            allowed = entity != null;
                            break;

                        case "Create":
                            relCol = new EntityRelationshipCollection<IEntity> { target() };
                            source.SetRelationships(rel, relCol);
                            if (haveFieldChanges)
                            {
                                source.SetField("core:name", Guid.NewGuid().ToString());
                            }
                            if (saveBothEnds)
                            {
                                Entity.Save(new[] {source, target()});
                            }
                            else
                            {
                                source.Save();
                            }
                            allowed = true;
                            break;

                        case "Remove":
                            relCol = source.GetRelationships(rel.Id, direction);
                            relCol.Remove(target());
                            source.SetRelationships(rel, relCol);
                            if (haveFieldChanges)
                            {
                                source.SetField("core:name", Guid.NewGuid().ToString());
                            }
                            if (saveBothEnds)
                            {
                                Entity.Save(new[] { source, target() });
                            }
                            else
                            {
                                source.Save();
                            }
                            allowed = true;
                            break;

                        case "Clear":
                            relCol = source.GetRelationships(rel.Id, direction);
                            relCol.Clear();
                            source.SetRelationships(rel, relCol);
                            if (haveFieldChanges)
                            {
                                source.SetField("core:name", Guid.NewGuid().ToString());
                            }
                            if (saveBothEnds)
                            {
                                Entity.Save(new[] { source, target() });
                            }
                            else
                            {
                                source.Save();
                            }
                            allowed = true;
                            break;

                        default:
                            throw new InvalidOperationException("Unknown " + action);
                    }
                }
                Assert.That(allowed, Is.EqualTo(expectAllow));
            }
            catch (PlatformSecurityException)
            {
                Assert.That(false, Is.EqualTo(expectAllow));
            }

        }
    }
}
