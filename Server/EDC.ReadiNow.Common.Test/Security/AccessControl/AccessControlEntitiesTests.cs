// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using NUnit.Framework;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class AccessControlEntitiesTests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase("type", "securableEntity")]
        [TestCase("userAccount", "subject")]
        [TestCase("role", "subject")]
        public void Test_TypeInheritsFromType(string testedType, string expectedInheritedType)
        {
            EntityType entityType = Entity.Get<EntityType>(testedType);
            Assert.That(entityType, Is.Not.Null, "Tested type not found");
            Assert.That(entityType.Inherits, Has.Some.Property("Alias").EndsWith(":" + expectedInheritedType),
                "Type does not inherit from expected type");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("subject", "allowAccess", "accessRule")]
        [TestCase("accessRule", "controlAccess", "securableEntity")]
        [TestCase("accessRule", "permissionAccess", "permission")]
        [TestCase("accessRule", "accessRuleReport", "report")]
        [TestCase("report", "securityReportInFolder", "folder")]
        public void Test_SecurityRelationships_Exist(string testedType, string relationshipName, string expectedtargetType)
        {
            EntityType entityType = Entity.Get<EntityType>(testedType);
            Assert.That(entityType, Is.Not.Null, "Tested type not found");
            Relationship relationship = entityType.Relationships.Single(r => r.Alias.EndsWith(":" + relationshipName));
            Assert.That(relationship, Has.Property("ToType").Not.Null
                                         .And.Property("ToType").Property("Alias").EndsWith(":" + expectedtargetType),
                "Relationship not found or incorrect target type");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("securableEntity", "controlAccess")]
        [TestCase("accessRule", "allowAccess")]
        [TestCase("report", "accessRuleReport")]
        [TestCase("folder", "securityReportInFolder")]
        public void Test_SecurityReverseRelationships_Exist(string testedType, string expectedRelationshipName)
        {
            EntityType entityType = Entity.Get<EntityType>(testedType);
            Assert.That(entityType, Is.Not.Null, "Tested type not found");
            Assert.That(entityType.ReverseRelationships, Has.Some.Property("Alias").EndsWith(":" + expectedRelationshipName),
                "Relationship not found");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("accessRule", "core:accessRuleEnabled", "false")]
        [TestCase("relationship", "core:securesTo", "false")]
        [TestCase("relationship", "core:securesFrom", "false")]
        public void Test_SecurityFields_Exist(string testedType, string fieldAlias, string expectedDefaultValue)
        {
            EntityType entityType = Entity.Get<EntityType>(testedType);
            Assert.That(entityType.Fields, Has.Exactly(1).Property("Alias").EqualTo(fieldAlias), "Field does not exist");
            Assert.That(entityType.Fields.First(x => x.Alias == fieldAlias), Has.Property("DefaultValue").EqualTo(expectedDefaultValue).IgnoreCase, "Incorrect default value");
        }
    }
}
