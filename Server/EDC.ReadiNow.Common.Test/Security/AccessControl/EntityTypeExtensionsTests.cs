// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    public class EntityTypeExtensionsTests
    {
        [Test]
        public void Test_GetSecuringRelationships_NullEntityType()
        {
            Assert.That(() => EntityTypeExtensions.GetSecuringRelationships(null, true, false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityType"));
            
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_GetSecuringRelationships_Relationships()
        {
            EntityType entityType1;
            EntityType entityType2;
            Relationship from1To2SecuresTo;
            Relationship from1To2SecuresFrom;
            Relationship from1To2NoSecuresFlags;
            Relationship from1To2BothSecuresFlags;
            Relationship from2To1SecuresTo;
            Relationship from2To1SecuresFrom;
            Relationship from2To1NoSecuresFlags;
            Relationship from2To1BothSecuresFlags;

            entityType1 = new EntityType();
            entityType1.Save();
            entityType2 = new EntityType();
            entityType2.Save();

            from1To2SecuresTo = new Relationship
            {
                Name = "from1To2SecuresTo",
                FromType = entityType1,
                ToType = entityType2,
                SecuresTo = true
            };
            from1To2SecuresTo.Save();

            from1To2SecuresFrom = new Relationship
            {
                Name = "from1To2SecuresFrom",
                FromType = entityType1,
                ToType = entityType2,
                SecuresFrom = true
            };
            from1To2SecuresFrom.Save();

            from1To2NoSecuresFlags = new Relationship
            {
                Name = "from1To2NoSecuresFlags",
                FromType = entityType1,
                ToType = entityType2
            };
            from1To2NoSecuresFlags.Save();

            from1To2BothSecuresFlags = new Relationship
            {
                Name = "from1To2BothSecuresFlags",
                FromType = entityType1,
                ToType = entityType2,
                SecuresTo = true,
                SecuresFrom = true
            };
            from1To2BothSecuresFlags.Save();

            from2To1SecuresTo = new Relationship
            {
                Name = "from2To1SecuresTo",
                FromType = entityType2,
                ToType = entityType1,
                SecuresTo = true
            };
            from2To1SecuresTo.Save();

            from2To1SecuresFrom = new Relationship
            {
                Name = "from2To1SecuresFrom",
                FromType = entityType2,
                ToType = entityType1,
                SecuresFrom = true
            };
            from2To1SecuresFrom.Save();

            from2To1NoSecuresFlags = new Relationship
            {
                Name = "from2To1NoSecuresFlags",
                FromType = entityType2,
                ToType = entityType1
            };
            from2To1NoSecuresFlags.Save();

            from2To1BothSecuresFlags = new Relationship
            {
                Name = "from2To1BothSecuresFlags",
                FromType = entityType2,
                ToType = entityType1,
                SecuresTo = true,
                SecuresFrom = true
            };
            from2To1BothSecuresFlags.Save();
            
            Assert.That(
                string.Join(", ", 
                    entityType1.GetSecuringRelationships(false, false)
                               .Select(r => r.Item1.Name)
                               .OrderBy(s => s)),
                Is.EqualTo("from1To2BothSecuresFlags, from1To2SecuresFrom, from2To1BothSecuresFlags, from2To1SecuresTo"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase("A", true, "A")]
        [TestCase("A", false, "A, B, C, D, E")]
        [TestCase("B", true, "B")]
        [TestCase("B", false, "A, B, C, D, E, F")]
        [TestCase("C", true, "A, C")]
        [TestCase("C", false, "A, B, C, D, E")]
        [TestCase("D", true, "B, D")]
        [TestCase("D", false, "A, B, C, D, E, F")]
        [TestCase("E", true, "A, B, C, D, E")]
        [TestCase("E", false, "A, B, C, D, E")]
        [TestCase("F", true, "B, D, F")]
        [TestCase("F", false, "B, D, F")]
        public void Test_GetSecuringRelationships_TypeInheritance(string typeName, bool ancestorsOnly, string expectedTypes)
        {
            // Entity type structure (inheritance):
            //
            //   +--------+         +--------+
            //   | Type A |         | Type B |
            //   +--------+         +--------+
            //       ^                   ^
            //       |                   |
            //   +--------+         +--------+
            //   | Type C |         | Type D |
            //   +--------+         +--------+
            //       ^                   ^
            //       |                   |
            //       |    +--------+     |    +--------+
            //       +----| Type E |-----+----| Type F |
            //            +--------+          +--------+


            string[] names = {"A", "B", "C", "D", "E", "F"};
            IDictionary<string, EntityType> nameToEntityType;
            IDictionary<Relationship, string> relationshipToSourceEntityName;
            IList<Tuple<Relationship, Direction, EntityType, object>> relationships;
            EntityType toType;

            nameToEntityType = new Dictionary<string, EntityType>();
            foreach (string name in names)
            {
                EntityType entityType;

                entityType = new EntityType();
                entityType.Name = name;
                entityType.Save();

                nameToEntityType[name] = entityType;
            }

            // Add inheritance
            nameToEntityType["C"].Inherits.Add(nameToEntityType["A"]);
            nameToEntityType["C"].Save();
            nameToEntityType["D"].Inherits.Add(nameToEntityType["B"]);
            nameToEntityType["D"].Save();
            nameToEntityType["E"].Inherits.Add(nameToEntityType["C"]);
            nameToEntityType["E"].Inherits.Add(nameToEntityType["D"]);
            nameToEntityType["E"].Save();
            nameToEntityType["F"].Inherits.Add(nameToEntityType["D"]);
            nameToEntityType["F"].Save();

            // Add test relationships
            toType = new EntityType();
            toType.Save();
            relationshipToSourceEntityName = new Dictionary<Relationship, string>(new EntityIdEqualityComparer<Relationship>());
            foreach (string name in names)
            {
                Relationship relationship;

                relationship = new Relationship()
                {
                    FromType = nameToEntityType[name],
                    ToType = toType,
                    SecuresFrom = true
                };
                relationship.Save();

                relationshipToSourceEntityName[relationship] = name;
            }

            relationships = nameToEntityType[typeName].GetSecuringRelationships(ancestorsOnly, false);

            Assert.That(
                string.Join(
                    ", ", 
                    relationships.Select(r => relationshipToSourceEntityName[r.Item1])
                                 .OrderBy(s => s)), 
                Is.EquivalentTo(expectedTypes));
        }
    }
}
