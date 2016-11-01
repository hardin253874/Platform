// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System.Collections.Generic;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.Test.Runner
{
    [TestFixture]
    class ScenarioTests
    {
        [TestCase("script: BoolField ;context:TestBool27122:TestInstance  ;expect: bool:False")]
        [TestCase("script: iif(BoolField,1,2) ;context:TestBool27122:TestInstance  ;expect: int:2")]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_NullBoolEvalsToFalse_27122(string test)
        {
            string typeName = Guid.NewGuid().ToString();
            test = test.Replace("TestBool27122", typeName);

            EntityType type = new EntityType();
            type.Name = typeName;
            type.Save();

            Resource instance = Entity.Create(type.Id).As<Resource>();
            instance.Name = "TestInstance";
            instance.Save();

            // Retrospectively add a bool field
            BoolField field = new BoolField();
            field.Name = "BoolField";
            type.Fields.Add(field.As<Field>());
            type.Save();

            TestHelper.Test(test);
        }

        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase("iif(count([Input].[Risk])>0, -1, -2)", -2)]
        [TestCase("count([Input].[Risk])", 0)]
        public void Test_CountFromParam_26597(string script, int expect)
        {
            // Schema
            string typeName = Guid.NewGuid().ToString();
            EntityType type = new EntityType();
            type.Name = typeName;
            type.Save();
            EntityType type2 = new EntityType();
            type2.Name = typeName + "B";
            type2.Save();
            Relationship rel = new Relationship();
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
            rel.FromType = type;
            rel.ToType = type2;
            rel.ToName = "Risk";
            rel.Save();

            // Instance
            Resource instance = Entity.Create(type.Id).As<Resource>();
            instance.Name = "TestInstance";
            instance.Save();

            // Compile
            BuilderSettings settings = new BuilderSettings();
            settings.ParameterNames = new List<string> { "Input" };
            settings.StaticParameterResolver = param => ExprTypeHelper.EntityListOfType(new EntityRef(type.Id));
            IExpression expr = Factory.ExpressionCompiler.Compile(script, settings);

            // Run
            EvaluationSettings eval = new EvaluationSettings();
            eval.ParameterResolver = param => instance;
            var result = Factory.ExpressionRunner.Run(expr, eval);

            // Check
            Assert.That(result.Value, Is.EqualTo(expect));
        }

        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase("script: [RTC2] < Max([RTC])                  ;context:Test27148:TestInstance  ;expect: bool:false")]
        [TestCase("script: iif(context().[RTC] is null,'z',iif(context().[RTC] < Max([Process Requirement].[RTO]),'Gap','OK') )   ;context:Test27148:TestInstance  ;expect: string:z")]
        public void Test_AggregateBug_27148(string script)
        {
            // Schema
            string typeName = Guid.NewGuid().ToString();
            script = script.Replace("Test27148", typeName);
            EntityType type = new EntityType();
            type.Name = typeName;
            type.Save();
            EntityType type2 = new EntityType();
            type2.Name = typeName + "B";
            type2.Save();
            // 'RTC' Choice field on type
            Relationship rel = new Relationship();
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
            rel.FromType = type;
            rel.ToType = Entity.Get<EntityType>("test:weekdayEnum");
            rel.ToName = "RTC";
            rel.Save();
            // 'RTC2' Choice field on type
            Relationship rel4 = new Relationship();
            rel4.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
            rel4.FromType = type;
            rel4.ToType = Entity.Get<EntityType>("test:weekdayEnum");
            rel4.ToName = "RTC2";
            rel4.Save();
            // 'Process Requirement' rel from type to type2
            Relationship rel2 = new Relationship();
            rel2.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
            rel2.FromType = type;
            rel2.ToType = type2;
            rel2.ToName = "Process Requirement";
            rel2.Save();
            // 'RTO' Choice field on type2
            Relationship rel3 = new Relationship();
            rel3.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne;
            rel3.FromType = type2;
            rel3.ToType = Entity.Get<EntityType>("test:weekdayEnum");
            rel3.ToName = "RTO";
            rel3.Save();

            // Instance
            Resource instance = Entity.Create(type.Id).As<Resource>();
            instance.Name = "TestInstance";
            instance.Save();

            // Test
            TestHelper.Test(script);
        }

    }
}
