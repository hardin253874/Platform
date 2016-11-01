// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ReadiNow.Expressions.NameResolver;

namespace ReadiNow.Expressions.Test.NameResolver
{
    [TestFixture]
    public class CachingScriptNameResolverTests
    {
        /// <summary>
        /// Script name resolver to use in tests.
        /// </summary>
        private IScriptNameResolver Resolver
        {
            get { return Factory.ScriptNameResolver; }
        }

        [TestCase(false, 1)]
        [TestCase(true, 2)]
        [RunAsDefaultTenant] // required because cache uses metadatacache, which uses PerUserRuleSet
        public void Test_GetTypeByName_SingleCall(bool clear, int expect)
        {
            Mock<IScriptNameResolver> mock = new Mock<IScriptNameResolver>(MockBehavior.Strict);
            CachingScriptNameResolver caching = new CachingScriptNameResolver(mock.Object);

            mock.Setup(r => r.GetTypeByName("Name1")).Returns(123);


            long result;
            result = caching.GetTypeByName("Name1");
            Assert.That(result, Is.EqualTo(123));
            if (clear)
                caching.Clear();

            result = caching.GetTypeByName("Name1");
            Assert.That(result, Is.EqualTo(123));

            mock.Verify(r => r.GetTypeByName("Name1"), Times.Exactly(expect));
        }

        [TestCase(false, 1)]
        [TestCase(true, 2)]
        [RunAsDefaultTenant] // required because cache uses metadatacache, which uses PerUserRuleSet
        public void Test_GetMemberOfType_SingleCall(bool clear, int expect)
        {
            Mock<IScriptNameResolver> mock = new Mock<IScriptNameResolver>(MockBehavior.Strict);
            CachingScriptNameResolver caching = new CachingScriptNameResolver(mock.Object);
            long typeId = 123;
            MemberType filter = MemberType.Field;
            string name = "abc";
            MemberInfo mockResult = new MemberInfo { MemberId = 456, MemberType = MemberType.Field };

            mock.Setup(r => r.GetMemberOfType(name, typeId, filter)).Returns(mockResult);

            MemberInfo result;
            result = caching.GetMemberOfType(name, typeId, filter);
            Assert.That(result, Is.EqualTo(mockResult));
            if (clear)
                caching.Clear();

            result = caching.GetMemberOfType(name, typeId, filter);
            Assert.That(result, Is.EqualTo(mockResult));

            mock.Verify(r => r.GetMemberOfType(name, typeId, filter), Times.Exactly(expect));
        }

        [TestCase("N1", "S1", "N2", "S2", "S1", true, false)]
        [TestCase("N1", "S1", "N2", "S2", "S2", false, true)]
        [TestCase("N1", null, "N2", "N1", "N1", true, true)]
        [TestCase("N1", null, "N2", null, "N1", true, false)]
        [TestCase("N1", null, "N2", null, "N2", false, true)]
        [TestCase("N1", "S1", "Delete", "S1", "S1", true, false)]
        [RunAsDefaultTenant]
        public void Test_Invalidated_TypeName(string name1, string sname1, string name2, string sname2, string search, bool expect1, bool expect2)
        {
            EntityType type = null;
            try
            {
                type = new EntityType();
                type.Name = name1;
                type.TypeScriptName = sname1;
                type.Save();

                long typeId = Resolver.GetTypeByName(search);
                Assert.That(typeId != 0, Is.EqualTo(expect1));

                if (name2 != name1)
                    type.Name = name2;
                if (sname2 != sname1)
                    type.TypeScriptName = sname2;

                if (name2 == "Delete")
                    type.Delete();
                else
                    type.Save();

                typeId = Resolver.GetTypeByName(search);
                Assert.That(typeId != 0, Is.EqualTo(expect2));
            }
            finally
            {
                if (type != null)
                    type.Delete();
            }
        }

        [TestCase("N1", "S1", "N2", "S2", "S1", true, false)]
        [TestCase("N1", "S1", "N2", "S2", "S2", false, true)]
        [TestCase("N1", null, "N2", "N1", "N1", true, true)]
        [TestCase("N1", null, "N2", null, "N1", true, false)]
        [TestCase("N1", null, "N2", null, "N2", false, true)]
        [TestCase("N1", "S1", "Delete", "S1", "S1", true, false)]
        [RunAsDefaultTenant]
        public void Test_Invalidated_FieldName(string name1, string sname1, string name2, string sname2, string search, bool expect1, bool expect2)
        {
            EntityType type = null;
            try
            {
                type = new EntityType();
                type.Name = Guid.NewGuid().ToString();
                Field field = new StringField().As<Field>();
                field.Name = name1;
                field.FieldScriptName = sname1;
                type.Fields.Add(field);
                type.Save();

                MemberInfo member = Resolver.GetMemberOfType(search, type.Id, MemberType.Any);
                Assert.That(member != null, Is.EqualTo(expect1), "First check");

                if (name2 != name1)
                    field.Name = name2;
                if (sname2 != sname1)
                    field.FieldScriptName = sname2;

                if (name2 == "Delete")
                    field.Delete();
                else
                    field.Save();

                member = Resolver.GetMemberOfType(search, type.Id, MemberType.Any);
                Assert.That(member != null, Is.EqualTo(expect2), "Second check");
            }
            finally
            {
                if (type != null)
                    type.Delete();
            }
        }

        [TestCase("N1", "S1", "N2", "S2", "S1", true, false)]
        [TestCase("N1", "S1", "N2", "S2", "S2", false, true)]
        [TestCase("N1", null, "N2", "N1", "N1", true, true)]
        [TestCase("N1", null, "N2", null, "N1", true, false)]
        [TestCase("N1", null, "N2", null, "N2", false, true)]
        [TestCase("N1", "S1", "Delete", "S1", "S1", true, false)]
        [RunAsDefaultTenant]
        public void Test_Invalidated_RelationshipName(string name1, string sname1, string name2, string sname2, string search, bool expect1, bool expect2)
        {
            EntityType type = null;
            try
            {
                type = new EntityType();
                type.Name = Guid.NewGuid().ToString();
                Relationship rel = new Relationship();
                rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                rel.Name = "Test";
                rel.ToName = name1;
                rel.ToScriptName = sname1;
                type.Relationships.Add(rel);
                type.Save();

                MemberInfo member = Resolver.GetMemberOfType(search, type.Id, MemberType.Relationship);
                Assert.That(member != null, Is.EqualTo(expect1));

                if (name2 != name1)
                    rel.ToName = name2;
                if (sname2 != sname1)
                    rel.ToScriptName = sname2;

                if (name2 == "Delete")
                    rel.Delete();
                else
                    rel.Save();

                member = Resolver.GetMemberOfType(search, type.Id, MemberType.Relationship);
                Assert.That(member != null, Is.EqualTo(expect2));
            }
            finally
            {
                if (type != null)
                    type.Delete();
            }
        }

        [TestCase("N1", "S1", "N2", "S2", "S1", true, false)]
        [TestCase("N1", "S1", "N2", "S2", "S2", false, true)]
        [TestCase("N1", null, "N2", "N1", "N1", true, true)]
        [TestCase("N1", null, "N2", null, "N1", true, false)]
        [TestCase("N1", null, "N2", null, "N2", false, true)]
        [TestCase("N1", "S1", "Delete", "S1", "S1", true, false)]
        [RunAsDefaultTenant]
        public void Test_Invalidated_ReverseRelationshipName(string name1, string sname1, string name2, string sname2, string search, bool expect1, bool expect2)
        {
            EntityType type = null;
            try
            {
                type = new EntityType();
                type.Name = Guid.NewGuid().ToString();
                Relationship rel = new Relationship();
                rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                rel.Name = "Test";
                rel.FromName = name1;
                rel.FromScriptName = sname1;
                type.ReverseRelationships.Add(rel);
                type.Save();

                MemberInfo member = Resolver.GetMemberOfType(search, type.Id, MemberType.Relationship);
                Assert.That(member != null, Is.EqualTo(expect1));

                if (name2 != name1)
                    rel.FromName = name2;
                if (sname2 != sname1)
                    rel.FromScriptName = sname2;

                if (name2 == "Delete")
                    rel.Delete();
                else
                    rel.Save();

                member = Resolver.GetMemberOfType(search, type.Id, MemberType.Relationship);
                Assert.That(member != null, Is.EqualTo(expect2));
            }
            finally
            {
                if (type != null)
                    type.Delete();
            }
        }



    }
}
