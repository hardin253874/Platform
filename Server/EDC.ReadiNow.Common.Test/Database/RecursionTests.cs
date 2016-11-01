// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using EDC.ReadiNow.Database;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Test.Database
{
    /// <summary>
    ///     Cardinality Violation tests.
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    public class RecursionTests
    {
        [Test]
        [TestCase(0, ExpectedResult = 6)]
        [TestCase(1, ExpectedResult = 9)]
        [RunAsDefaultTenant]
        public int FnGetRelationshipRecAndSelf(int includeSelf)
        {
            EntityType type = new EntityType();
            type.Name = "RecTest";
            type.Save();

            Relationship rel = new Relationship();
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
            rel.FromType = type;
            rel.ToType = type;
            rel.Save();

            Entity inst1 = new Entity(type.Id);
            Entity inst2 = new Entity(type.Id);
            Entity inst3 = new Entity(type.Id);
            inst1.GetRelationships(rel.Id).Add(inst2);
            inst1.GetRelationships(rel.Id).Add(inst3);
            inst2.GetRelationships(rel.Id).Add(inst1);
            inst2.GetRelationships(rel.Id).Add(inst3);
            inst3.GetRelationships(rel.Id).Add(inst1);
            inst3.GetRelationships(rel.Id).Add(inst2);
            inst1.Save();
            inst2.Save();
            inst3.Save();

            long tenant = RequestContext.TenantId;

            using (var context = DatabaseContext.GetContext())
            {
                string sql = $"select count(*) from (select distinct FromId, ToId from dbo.fnGetRelationshipRecAndSelf({rel.Id},{tenant},{includeSelf},{type.Id},{type.Id}))t";
                var command = context.CreateCommand(sql);

                return (int)command.ExecuteScalar();
            }
        }

        [Test]
        [TestCase("fnDerivedTypes")]
        [TestCase("fnDescendantsAndSelf")]
        [TestCase("fnAncestorsAndSelf")]
        [RunAsDefaultTenant]
        public void Test_CircularInheritance(string fn)
        {
            long inherits = WellKnownAliases.CurrentTenant.Inherits;

            EntityType inst1 = new EntityType();
            EntityType inst2 = new EntityType();
            EntityType inst3 = new EntityType();
            inst1.Inherits.Add(inst2);
            inst1.Inherits.Add(inst3);
            inst2.Inherits.Add(inst1);
            inst2.Inherits.Add(inst3);
            inst3.Inherits.Add(inst1);
            inst3.Inherits.Add(inst2);
            inst1.Save();
            inst2.Save();
            inst3.Save();

            long tenant = RequestContext.TenantId;
            string relType = fn == "fnDerivedTypes" ? "" : $"{WellKnownAliases.CurrentTenant.Inherits}, ";

            using (var context = DatabaseContext.GetContext())
            {
                string sql = $"select count(distinct Id) from dbo.{fn}({relType}{inst1.Id},{tenant}) t";
                var command = context.CreateCommand(sql);

                Assert.That((int)command.ExecuteScalar(), Is.EqualTo(3));
            }
        }
    }
}
