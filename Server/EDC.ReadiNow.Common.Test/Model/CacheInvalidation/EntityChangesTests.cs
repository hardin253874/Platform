// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model.CacheInvalidation;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    [TestFixture]
	[RunWithTransaction]
    public class EntityChangesTests
    {
        [Test]
        public void Test_DefaultCtor()
        {
            EntityChanges fieldsAndRelationshipTypes;

            fieldsAndRelationshipTypes = new EntityChanges();

            Assert.That(fieldsAndRelationshipTypes, Has.Property("RelationshipTypesToEntities").Empty);
            Assert.That(fieldsAndRelationshipTypes, Has.Property("FieldTypes").Empty);
        }

        [Test]
        public void Test_Ctor_NullRelationshipTypesToEntities()
        {
            Assert.That(() => new EntityChanges(null, new HashSet<long>()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("relationshipTypesToEntities"));
        }

        [Test]
        public void Test_Ctor_NullFieldTypes()
        {
            Assert.That(() => new EntityChanges(new HashSet<RelationshipTypeToEntity>(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("fieldTypes"));
        }

        [Test]
        public void Test_Ctor()
        {
            EntityChanges fieldsAndRelationshipTypes;
            HashSet<RelationshipTypeToEntity> relationshipTypesToEntities;
            HashSet<long> fieldTypes;

            relationshipTypesToEntities = new HashSet<RelationshipTypeToEntity>();
            fieldTypes = new HashSet<long>();

            fieldsAndRelationshipTypes = new EntityChanges(relationshipTypesToEntities, fieldTypes);

            Assert.That(fieldsAndRelationshipTypes, Has.Property("RelationshipTypesToEntities").EquivalentTo(relationshipTypesToEntities));
            Assert.That(fieldsAndRelationshipTypes, Has.Property("FieldTypes").EquivalentTo(fieldTypes));
        }
    }
}
