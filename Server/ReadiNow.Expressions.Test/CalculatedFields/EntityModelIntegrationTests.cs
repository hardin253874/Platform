// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using System;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.Database;
using EDC.ReadiNow.Core;

namespace ReadiNow.Expressions.Test.CalculatedFields
{
    /// <summary>
    /// 
    /// </summary>
    [RunAsDefaultTenant]
    [TestFixture]
    public class EntityModelIntegrationTests
    {
        [TestCase("core:stringField", "'Test'+Name", "TestName1", typeof(string))]
        [TestCase("core:intField", "len(Name)", "5", typeof(int))]
        [TestCase("core:decimalField", "convert(decimal,len(Name)+0.5)", "5.5", typeof(decimal))] // hmm
        [TestCase("core:currencyField", "convert(currency,len(Name)+0.5)", "5.5", typeof(decimal))]
        [TestCase("core:boolField", "Name='Name1'", "True", typeof(bool))]
        [TestCase("core:dateField", "iif(Name='Name1',getdate(),getdate())", "*", typeof(DateTime))]
        [TestCase("core:timeField", "iif(Name='Name1',gettime(),gettime())", "*", typeof(DateTime))]
        [TestCase("core:dateTimeField", "iif(Name='Name1',getdatetime(),getdatetime())", "*", typeof(DateTime))]
        [RunWithTransaction]
        public void TestSingle_FieldTypes(string fieldTypeAlias, string calc, string exected, Type expectedType)
        {
            var scenario = CreateScenario(fieldTypeAlias, calc);
            var field = scenario.Item1;
            var inst = scenario.Item2[0];

            // Get calculated field
            object result = inst.GetField(field.Id);

            // Check results
            CheckResult(result, exected, expectedType);
        }

        [TestCase("core:stringField", "[Name", null, typeof(string))]        // parse exception
        [TestCase("core:stringField", "1/0", null, typeof(string))]          // eval exception
        [RunWithTransaction]
        public void TestSingle_ReturnNullIfError(string fieldTypeAlias, string calc, string exected, Type expectedType)
        {
            var scenario = CreateScenario(fieldTypeAlias, calc);
            var field = scenario.Item1;
            var inst = scenario.Item2[0];

            // Get calculated field
            object result = inst.GetField(field.Id);

            // Check results
            CheckResult(result, exected, expectedType);
        }

        [TestCase("core:stringField", "'Test'+Name", "TestName1", "TestName2", typeof(string))]
        [RunWithTransaction]
        public void TestMultiple_ById(string fieldTypeAlias, string calc, string expected1, string expected2, Type expectedType)
        {
            var scenario = CreateScenario(fieldTypeAlias, calc);
            var field = scenario.Item1;
            var insts = scenario.Item2;

            // Get calculated field
            var dict = Entity.GetField(insts.Select(i => new EntityRef(i.Id)), new EntityRef(field.Id));

            // Check results
            CheckResult(dict[new EntityRef(insts[0].Id)], expected1, expectedType);
            CheckResult(dict[new EntityRef(insts[1].Id)], expected2, expectedType);
        }

        [TestCase("core:stringField", "'Test'+Name", "TestName1", "TestName2", typeof(string))]
        [RunWithTransaction]
        public void TestMultiple_ByEntity(string fieldTypeAlias, string calc, string expected1, string expected2, Type expectedType)
        {
            var scenario = CreateScenario(fieldTypeAlias, calc);
            var field = scenario.Item1;
            var insts = scenario.Item2;

            // Get calculated field
            var dict = Entity.GetField(insts, new EntityRef(field.Id));

            // Check results
            CheckResult(dict[insts[0]], expected1, expectedType);
            CheckResult(dict[insts[1]], expected2, expectedType);
        }

        [Test]
        public void Test_Invalidate_WhenCalcChanged()
        {
            EntityType type = null;
            try
            {
                var scenario = CreateScenario("core:stringField", "'TestA '+Name");
                var field = scenario.Item1;
                var inst = scenario.Item2[0];
                type = field.FieldIsOnType;

                // Get calculated field
                object result = inst.GetField(field.Id);
                CheckResult(result, "TestA Name1", typeof(string));

                // Update
                field.FieldCalculation = "'TestB '+Name";
                field.Save();

                // Get calculated field
                result = inst.GetField(field.Id);
                CheckResult(result, "TestB Name1", typeof(string));
            }
            finally
            {
                if (type != null)
                    type.AsWritable().Delete();
            }            
        }

        [Test]
        public void Test_Invalidate_WhenDataChanged()
        {
            EntityType type = null;
            try
            {
                var scenario = CreateScenario("core:stringField", "'TestA '+Name");
                var field = scenario.Item1;
                var inst = scenario.Item2[0];
                type = field.FieldIsOnType;

                // Get calculated field
                object result = inst.GetField(field.Id);
                CheckResult(result, "TestA Name1", typeof(string));

                // Update
                inst.Name = "Name9";
                inst.Save();

                // Get calculated field
                result = inst.GetField(field.Id);
                CheckResult(result, "TestA Name9", typeof(string));
            }
            finally
            {
                if (type != null)
                    type.AsWritable().Delete();
            }
        }

        [Test]
        [RunWithTransaction]
        public void RunCalculatedFieldFromEvalHost()
        {
            // Ensure a workflow can access a calculated field.

            // Create scenario
            EntityType type = new EntityType();
            type.Name = "Test" + Guid.NewGuid().ToString();
            Field field = new StringField().As<Field>();
            field.Name = "MyCalcField";
            field.FieldCalculation = "Name + 'hello'";
            field.IsCalculatedField = true;
            type.Fields.Add(field);
            type.Save();

            Resource inst = Entity.Create(type.Id).As<Resource>();
            inst.Name = "TestName";
            inst.Save();

            var script = "MyCalcField";

            BuilderSettings settings = new BuilderSettings
            {
                RootContextType = new ExprType { Type = DataType.Entity, EntityType = new EntityRef(type.Id) },
                ScriptHost = ScriptHostType.Evaluate
            };

            IExpression expr = Factory.ExpressionCompiler.Compile(script, settings);
            EvaluationSettings evalSettings = new EvaluationSettings();
            evalSettings.ContextEntity = inst;
            evalSettings.TimeZoneName = "Australia/Sydney";

            var runResult = Factory.ExpressionRunner.Run(expr, evalSettings);
            Assert.That(runResult.Value, Is.EqualTo("TestNamehello"));
        }


        private Tuple<Field, Resource[]> CreateScenario(string fieldTypeAlias, string calculation)
        {
            // Define type
            EntityType type = new EntityType();

            // Define calculated field
            Field field = Entity.Create(new EntityRef(fieldTypeAlias)).As<Field>();
            field.IsCalculatedField = true;
            field.FieldCalculation = calculation;
            type.Fields.Add(field);

            type.Save();

            // Create instance
            Resource inst = Entity.Create(type.Id).As<Resource>();
            inst.Name = "Name1";
            inst.Save();

            Resource inst2 = Entity.Create(type.Id).As<Resource>();
            inst2.Name = "Name2";
            inst2.Save();

            return new Tuple<Field, Resource[]>(field, new[] { inst, inst2 });
        }

        private void CheckResult(object actual, string expected, Type expectedType)
        {
            if (expected == null)
            {
                Assert.That(actual, Is.Null);
                return;
            }
            Assert.That(actual, Is.InstanceOf(expectedType));
            if (expected == "*")
                Assert.That(actual.ToString(), Is.Not.Null.And.Not.Empty);
            else
                Assert.That(actual.ToString(), Is.EqualTo(expected));
        }
    }
}
