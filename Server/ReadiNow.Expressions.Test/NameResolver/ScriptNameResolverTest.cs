// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using ReadiNow.Expressions.NameResolver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Test.NameResolver
{
    [TestFixture("ScriptNameResolver")]
    [TestFixture("CachingScriptNameResolver")]
    [RunAsDefaultTenant]
    public class ScriptNameResolverTest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">Provider to test</param>
        public ScriptNameResolverTest(string resolver)
        {
            if (resolver == "CachingScriptNameResolver")
            {
                ScriptNameResolver = Factory.ScriptNameResolver;
                if (!(ScriptNameResolver is CachingScriptNameResolver))
                    throw new Exception("Expected CachingScriptNameResolver");
            }
            else if (resolver == "ScriptNameResolver")
            {
                ScriptNameResolver = Factory.Current.Resolve<ScriptNameResolver>();
            }
        }

        const string WfInstanceName = "Person Name Update";

        /// <summary>
        /// Script name resolver to use in tests.
        /// </summary>
        private IScriptNameResolver ScriptNameResolver { get; set; }

        [Test]
        public void Test_GetInstance()
        {
            IEntity entity = ScriptNameResolver.GetInstance(WfInstanceName, new EntityRef("core:workflow").Id);
            Assert.That(entity, Is.Not.Null);
        }

        [Test]
        public void Test_GetInstance_UnknownName()
        {
            IEntity entity = ScriptNameResolver.GetInstance("I don't exist anywhere", new EntityRef("core:workflow").Id);
            Assert.That(entity, Is.Null);
        }


        [TestCase("AA_Herb", "test:herb")]    // normal object
        [TestCase("Workflow", "core:workflow")]   // internal type
        [TestCase("Cardinality", "core:cardinalityEnum")] // choice field
        [TestCase( "Log Entry", "core:tenantLogEntry" )] // managed type
        public void Test_GetTypeByName( string name, string expected )
        {
            long typeId = ScriptNameResolver.GetTypeByName(name );
            Assert.That(typeId, Is.EqualTo( new EntityRef(expected).Id ));
        }

        [Test]
        public void Test_GetTypeByName_Unknown()
        {
            long typeId = ScriptNameResolver.GetTypeByName("I don't exist");
            Assert.That(typeId, Is.EqualTo(0));
        }

        [TestCase("object name", "scriptName", "scriptName", true)]
        [TestCase("object name", "scriptName", "object name", false)]
        [TestCase("object name", null, "object name", true)]
        [TestCase("object name", null, "OBJECT name", true)]
        public void Test_GetTypeByName_Scenarios( string name, string scriptName, string search, bool expect)
        {
            EntityType type = null;
            try
            {
                type = new EntityType();
                type.Name = name;
                type.TypeScriptName = scriptName;
                type.Save();

                long typeId = ScriptNameResolver.GetTypeByName(search);

                Assert.That(typeId != 0, Is.EqualTo(expect));
            }
            finally
            {
                type.Delete();
            }
        }

        [Test]
        public void Test_GetMemberOfType_NameField()
        {
            MemberInfo memberInfo = ScriptNameResolver.GetMemberOfType( "Name", new EntityRef("core:resource").Id, MemberType.Any );
            Assert.That(memberInfo.MemberType, Is.EqualTo(MemberType.Field));
            Assert.That(memberInfo.MemberId, Is.EqualTo( new EntityRef("core:name").Id ));
        }

        [TestCase("A", null, "A", 1)]
        [TestCase("A", null, "Z", null)]
        [TestCase("A", "B", "A", null)]   // name is not supported if script-name is provided
        [TestCase("A", "B", "B", 1)]
        [TestCase("A", "B", "Z", null)]
        [RunWithTransaction]
        public void Test_GetMemberOfType_OneField(string name1, string scriptName1, string memberName, int? expect)
        {
            // Scenario
            EntityType type = new EntityType();
            Field field1 = new StringField().As<Field>();
            field1.Name = name1;
            field1.FieldScriptName = scriptName1;
            type.Fields.Add(field1);
            type.Save();

            MemberInfo memberInfo = ScriptNameResolver.GetMemberOfType(memberName, type.Id, MemberType.Any);

            if (expect == 1)
                Assert.That(memberInfo.MemberId, Is.EqualTo(field1.Id));
            else
                Assert.That(memberInfo, Is.Null);
        }

        [TestCase("A", "B", "C", "D", "D", 2)]
        [TestCase("P", "Q", "Q", "P", "P", 2)]       // collision OK on scriptName1 and name2, because scriptName2 was provided
        [TestCase("P", "Q", "Q", null, "P", null)]   // collision on scriptName1 and name2, with no scriptName2
        [TestCase("A", "B", "C", "B", "B", null)]    // collision on scriptName1 and scriptName2
        [RunWithTransaction]
        public void Test_GetMemberOfType_TwoFields(string name1, string scriptName1, string name2, string scriptName2, string memberName, int? expect)
        {
            // Scenario
            EntityType type = new EntityType();
            Field field1 = new StringField().As<Field>();
            field1.Name = name1;
            field1.FieldScriptName = scriptName1;
            type.Fields.Add(field1);
            Field field2 = new StringField().As<Field>();
            field2.Name = name2;
            field2.FieldScriptName = scriptName2;
            type.Fields.Add(field2);
            type.Save();

            MemberInfo memberInfo = ScriptNameResolver.GetMemberOfType(memberName, type.Id, MemberType.Any);

            if (expect == 1)
                Assert.That(memberInfo.MemberId, Is.EqualTo(field1.Id));
            else if (expect == 2)
                Assert.That(memberInfo.MemberId, Is.EqualTo(field2.Id));
            else
                Assert.That(memberInfo, Is.Null);
        }

        [TestCase("From", "F", "Fs", "T", "Ts", "F", null)]
        [TestCase("From", "F", "Fs", "T", "Ts", "Fs", null)] // wrong end of relationship
        [TestCase("From", "F", "Fs", "T", "Ts", "T", null)]
        [TestCase("From", "F", "Fs", "T", "Ts", "Ts", Direction.Forward)]
        [TestCase("From", "F", "Fs", "T", null, "T", Direction.Forward)]
        [TestCase("To", "F", "Fs", "T", "Ts", "T", null)]
        [TestCase("To", "F", "Fs", "T", "Ts", "Ts", null)] // wrong end of relationship
        [TestCase("To", "F", "Fs", "T", "Ts", "F", null)]
        [TestCase("To", "F", "Fs", "T", "Ts", "Fs", Direction.Reverse)]
        [TestCase("To", "F", null, "T", "Ts", "F", Direction.Reverse)]
        [TestCase("Both", "F", "Fs", "T", "Ts", "Fs", Direction.Reverse)]
        [TestCase("Both", "F", null, "T", "Ts", "F", Direction.Reverse)]
        [TestCase("From", "F", "Same", "T", "Same", "Same", Direction.Forward)]
        [TestCase("To", "F", "Same", "T", "Same", "Same", Direction.Reverse)]
        [TestCase("Both", "F", "Same", "T", "Same", "Same", null)]
        [RunWithTransaction]
        public void Test_GetMemberOfType_Relationship(string attach, string fromName, string fromScriptName, string toName, string toScriptName, string memberName, Direction? expect)
        {
            // Scenario
            EntityType type = new EntityType();
            EntityType type2 = new EntityType();
            Relationship rel = new Relationship();
            rel.Name = "RelName";
            rel.FromName = fromName;
            rel.ToName = toName;
            rel.FromScriptName = fromScriptName;
            rel.ToScriptName = toScriptName;
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
            if (attach == "From")
            {
                type.Relationships.Add(rel);
                rel.ToType = type2;
            }
            else if (attach == "To")
            {
                type.ReverseRelationships.Add(rel);
                rel.FromType = type2;
            }
            else if (attach == "Both")
            {
                type.Relationships.Add(rel);
                rel.ToType = type;
            }
            type.Save();

            MemberInfo memberInfo = ScriptNameResolver.GetMemberOfType(memberName, type.Id, MemberType.Any);

            if (expect == null)
                Assert.That(memberInfo, Is.Null);
            else
            {
                Assert.That(memberInfo, Is.Not.Null);
                Assert.That(memberInfo.Direction, Is.EqualTo(expect.Value));
            }

        }

        [Test]
        public void Test_GetMemberOfType_Derived_NameField()
        {
            MemberInfo memberInfo = ScriptNameResolver.GetMemberOfType("Name", new EntityRef("core:workflow").Id, MemberType.Any);
            Assert.That(memberInfo.MemberType, Is.EqualTo(MemberType.Field));
            Assert.That(memberInfo.MemberId, Is.EqualTo(new EntityRef("core:name").Id));
        }

        [Test]
        public void Test_GetMemberOfType_UnknownName()
        {
            MemberInfo memberInfo = ScriptNameResolver.GetMemberOfType("I don't exist", new EntityRef("core:resource").Id, MemberType.Any);
            Assert.That(memberInfo, Is.Null);
        }
    }
}