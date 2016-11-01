// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System.Collections.Generic;

namespace ReadiNow.Expressions.Test.Runner
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [RunAsDefaultTenant]
    public class ChoiceOrderingTests
    {
        // Expected order is B < C < A (see notes below)
        // #26844 follows
        [TestCase("C", "B", "script: Choice1>Choice2 ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("A", "C", "script: Choice1>Choice2 ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("A", "B", "script: Choice1>Choice2 ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("B", "A", "script: Choice1>Choice2 ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("C", "A", "script: Choice1>Choice2 ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("B", "C", "script: Choice1>Choice2 ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("C", "B", "script: Choice1<Choice2 ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("A", "C", "script: Choice1<Choice2 ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("A", "B", "script: Choice1<Choice2 ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("B", "A", "script: Choice1<Choice2 ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("C", "A", "script: Choice1<Choice2 ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("B", "C", "script: Choice1<Choice2 ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("A", "A", "script: Choice1>Choice2 ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("A", "A", "script: Choice1<Choice2 ;context:TestType26844:TestInstance  ;expect: bool:False")]
        // #23754 follows
        [TestCase("C", "B", "script: Choice1>'B' ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("A", "C", "script: Choice1>'C' ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("A", "B", "script: Choice1>'B' ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("B", "A", "script: Choice1>'A' ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("C", "A", "script: Choice1>'A' ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("B", "C", "script: Choice1>'C' ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("C", "B", "script: Choice1<'B' ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("A", "C", "script: Choice1<'C' ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("A", "B", "script: Choice1<'B' ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("B", "A", "script: Choice1<'A' ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("C", "A", "script: Choice1<'A' ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("B", "C", "script: Choice1<'C' ;context:TestType26844:TestInstance  ;expect: bool:True")]
        [TestCase("A", "A", "script: Choice1>'A' ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [TestCase("A", "A", "script: Choice1<'A' ;context:TestType26844:TestInstance  ;expect: bool:False")]
        [RunWithTransaction]
        public void ChoiceField_Comparison_26844_23754(string value1, string value2, string test)
        {
            EnumType choiceType;
            EntityType testType;
            string testTypeName = Guid.NewGuid().ToString();
            test = test.Replace("TestType26844", testTypeName);

            // Define choice-field
            choiceType = new EnumType();
            choiceType.Name = "EnumType26844";
            choiceType.Inherits.Add(EnumValue.EnumValue_Type);
            choiceType.Save();

            // Save and add each individually - order of creation is important, as it affects the ID numbers, and the bug was related to the ordering of the ID numbers.
            // This ordering is selected so that ID ordering, name ordering, and 'enumOrder' ordering are all different.
            // The expected result is enum ordering, which should be in the order of B, C, A
            // DONT use a dictionary, or we can't control creation ordering.
            string[] nameAndOrderPairs = new[] { "B|1", "A|3", "C|2" };
            var valueLookup = new Dictionary<string, EnumValue>();

            foreach (string pair in nameAndOrderPairs)
            {
                string[] parts = pair.Split('|');
                EnumValue enumValue = Entity.Create(choiceType.Id).As<EnumValue>();
                enumValue.Name = parts[0];
                enumValue.EnumOrder = int.Parse(parts[1]);
                enumValue.EnumOwner = choiceType;
                enumValue.Save();
                valueLookup[enumValue.Name] = enumValue;
            }

            // Define a test type
            testType = new EntityType();
            testType.Name = testTypeName;

            // Add two choice fields to it
            Relationship rel1 = new Relationship();
            rel1.Name = "Choice1";
            rel1.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
            rel1.ToType = choiceType.As<EntityType>();
            rel1.ToName = "Choice1";
            testType.Relationships.Add(rel1);

            Relationship rel2 = new Relationship();
            rel2.Name = "Choice2";
            rel2.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
            rel2.ToType = choiceType.As<EntityType>();
            rel2.ToName = "Choice2";
            testType.Relationships.Add(rel2);
            testType.Save();

            // Create a test instance
            Resource instance = Entity.Create(testType.Id).As<Resource>();
            instance.Name = "TestInstance";
            instance.SetRelationships(rel1.Id, new EntityRelationship<IEntity>(valueLookup[value1]).ToEntityRelationshipCollection());
            if (value2 != "")
                instance.SetRelationships(rel2.Id, new EntityRelationship<IEntity>(valueLookup[value2]).ToEntityRelationshipCollection());
            instance.Save();

            TestHelper.Test(test);
        }

        [TestCase("script: all ([TestType23754]) where [Priority]>'Low' ;host:Evaluate ;expect: TestType23754 list:T2,T3,T4", true)]
        [TestCase("script: all ([TestType23754]) where [Priority]>'Low' ;host:Evaluate ;expect: TestType23754 list:T2,T4,T6", false)]
        [TestCase("script: all ([TestType23754]) where [Priority]='Low' ;host:Evaluate ;expect: TestType23754 list:T1", true)]
        [RunWithTransaction]
        public void ChoiceField_Comparison_23754(string test, bool isEnum)
        {
            EnumType choiceType;
            EntityType testType;
            string testTypeName = Guid.NewGuid().ToString();
            test = test.Replace("TestType23754", testTypeName);

            // Define choice-field
            choiceType = new EnumType();
            choiceType.Name = "EnumType23754";
            if (isEnum)
            { 
                choiceType.Inherits.Add(EnumValue.EnumValue_Type);
            }
            choiceType.Save();

            // Save and add each individually - order of creation is important, as it affects the ID numbers, and the bug was related to the ordering of the ID numbers.
            // This ordering is selected so that ID ordering, name ordering, and 'enumOrder' ordering are all different.
            // The expected result is enum ordering, which should be in the order of B, C, A
            // DONT use a dictionary, or we can't control creation ordering.
            string[] nameAndOrderPairs = new[] { "Low|1", "Medium|2", "High|3", "NullOrder|null" };
            var valueLookup = new Dictionary<string, IEntity>();
            valueLookup.Add("null", null);

            foreach (string pair in nameAndOrderPairs)
            {
                string[] parts = pair.Split('|');
                Resource resource = Entity.Create(choiceType.Id).As<Resource>();
                resource.Name = parts[0];
                if (isEnum)
                {
                    EnumValue enumValue = resource.As<EnumValue>();
                    if (parts[1] != "null")
                        enumValue.EnumOrder = int.Parse(parts[1]);
                    enumValue.EnumOwner = choiceType;
                }
                resource.Save();
                valueLookup[resource.Name] = resource;
            }

            // Define a test type
            testType = new EntityType();
            testType.Name = testTypeName;

            // Add a choice field to it
            Relationship rel1 = new Relationship();
            rel1.Name = "Priority";
            rel1.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
            rel1.ToType = choiceType.As<EntityType>();
            rel1.ToName = "Priority";
            testType.Relationships.Add(rel1);
            testType.Save();

            // Create test instances
            string[] nameAndPriorityPairs = new[] { "T1|Low", "T2|Medium", "T3|High", "T4|Medium", "T5|null", "T6|NullOrder" };
            foreach (string pair in nameAndPriorityPairs)
            {
                string[] parts = pair.Split('|');
                string name = parts[0];
                IEntity priority = valueLookup[parts[1]];

                Resource instance = Entity.Create(testType.Id).As<Resource>();
                instance.Name = name;
                if (priority != null)
                    instance.SetRelationships(rel1.Id, new EntityRelationship<IEntity>(priority).ToEntityRelationshipCollection());
                instance.Save();
            }

            TestHelper.Test(test);
        }
    }
}
