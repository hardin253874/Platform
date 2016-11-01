// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EDC.ReadiNow.Test.Model.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    public class GetTests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase("", false)]
        [TestCase("core:read", true)]
        [TestCase("core:modify", false)]
        [TestCase("core:delete", false)]
        [TestCase("core:read,core:modify", true)]
        [TestCase("core:read,core:delete", true)]
        [TestCase("core:modify,core:delete", false)]
        [TestCase("core:read,core:modify,core:delete", true)]
        public void Test_ReadyOnly(string permissionAliases, bool canAccessEntity1)
        {
            UserAccount userAccount = null;
            EntityType entityType = null;
            IEntity entity1 = null;
            IEntity entity2 = null;

            userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Test user " + Guid.NewGuid().ToString();
            userAccount.Save();

            entityType = new EntityType();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Save();

            entity1 = Entity.Create(new EntityRef(entityType));
            entity1.SetField("core:name", "A");
            entity1.Save();

            entity2 = Entity.Create(new EntityRef(entityType));
            entity2.SetField("core:name", "B");
            entity2.Save();

            if (permissionAliases.Length > 0)
            {
                new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(), entityType.As<SecurableEntity>(),
                                                          permissionAliases.Split(',').Select(x => new EntityRef(x)),
                                                          TestQueries.EntitiesWithNameA().ToReport());
            }

            using (new SetUser(userAccount))
            {
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity1)),
                    canAccessEntity1 
                        ? (Constraint) Is.EqualTo(entity1).Using(EntityRefComparer.Instance)
                        : Throws.TypeOf<PlatformSecurityException>(), "Entity 1 access incorrect");
                Assert.That(() => Entity.Get<IEntity>(new EntityRef(entity2)),
                    Throws.TypeOf<PlatformSecurityException>(), "Entity 2 access somehow worked");
            }
        }

        
        [Test]
        [RunAsDefaultTenant]
        [TestCase("", false)]
        [TestCase("core:read", false)]
        [TestCase("core:read,core:modify", true)]
        public void Test_Writeable_Related_Entity(string toPermissionAliases, bool toEntityWriteable)
        {                        
            var userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.Save();

            var fromType = new EntityType();
            fromType.Inherits.Add(UserResource.UserResource_Type);
            fromType.Save();

            var toType = new EntityType();
            toType.Inherits.Add(UserResource.UserResource_Type);
            toType.Save();

            var relationship = new Relationship {FromType = fromType, ToType = toType};
            relationship.Save();

            IEntity fromEntity = Entity.Create(new EntityRef(fromType));
            fromEntity.SetField("core:name", "A");
            fromEntity.Save();

            IEntity toEntity = Entity.Create(new EntityRef(toType));
            toEntity.SetField("core:name", "B");
            toEntity.SetRelationships(relationship, new EntityRelationshipCollection<IEntity>() { fromEntity }, Direction.Reverse);
            toEntity.Save();

            // Read / modify from type
            new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(), fromType.As<SecurableEntity>(),
                                                    new List<EntityRef>{ new EntityRef("core:read"), new EntityRef("core:modify")},
                                                    TestQueries.EntitiesWithNameA().ToReport());

            if (toPermissionAliases.Length > 0)
            {
                // Access to to type
                new AccessRuleFactory().AddAllowByQuery(userAccount.As<Subject>(), toType.As<SecurableEntity>(),
                                                        toPermissionAliases.Split(',').Select(x => new EntityRef(x)),
                                                        TestQueries.EntitiesWithNameB().ToReport());                
            }            

            using (new SetUser(userAccount))
            {
                // The from entity should be able to be retrieved
                var fromEntityWriteable = Entity.Get<IEntity>(new EntityRef(fromEntity), true);
                Assert.AreEqual(fromEntity.Id, fromEntityWriteable.Id);

                // Check access to to entity
	            IEntity entity = fromEntityWriteable.GetRelationships( relationship, Direction.Forward ).FirstOrDefault( );

	            IEntity toEntityFromRel = null;

	            if ( entity != null )
	            {
		            toEntityFromRel = entity.Entity;
	            }

                if (toPermissionAliases.Length > 0)
                {
                    Assert.AreEqual(toEntity.Id, toEntityFromRel.Id);
                    var toEntityWrite = toEntityFromRel.AsWritable();
                    toEntityWrite.SetField("core:description", "Test");

                    if (toEntityWriteable)
                    {                        
                        // Should be able to save
                        Assert.DoesNotThrow(() => toEntityWrite.Save());                        
                    }
                    else
                    {
                        // Should not be able to save
                        Assert.That(toEntityWrite.Save,                                                                        
                            Throws.TypeOf<PlatformSecurityException>(), "Entity access is incorrect. Should notbe able to save.");                        
                    }                    
                }
                else
                {
                    // We do not have read access to the to entity so it should be null
                    Assert.IsNull(toEntityFromRel);    
                }                                
            }
        }
    }
}
