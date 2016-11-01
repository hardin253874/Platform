// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    [TestFixture]
	[RunWithTransaction]
    public class TestRelationshipTypeToEntity
    {
        [Test]
        public void Test_Ctor()
        {
            RelationshipTypeToEntity relationshipTypeToEntity;
            EntityRef entity;
            EntityRef relationshipType;

            entity = new EntityRef(1);
            relationshipType = new EntityRef(2);

            relationshipTypeToEntity = new RelationshipTypeToEntity(entity, relationshipType);

            Assert.That(relationshipTypeToEntity, Has.Property("Entity").EqualTo(entity));
            Assert.That(relationshipTypeToEntity, Has.Property("RelationshipType").EqualTo(relationshipType));
        }

        [Test]
        public void Test_Ctor_NullEntity()
        {
            Assert.That(() => new RelationshipTypeToEntity(null, new EntityRef()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entity"));    
        }

        [Test]
        public void Test_Ctor_NullRelationshipType()
        {
            Assert.That(() => new RelationshipTypeToEntity(new EntityRef(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("relationshipType"));
        }
    }
}
