// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database.Types;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using ReadiNow.Expressions.CalculatedFields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Test.CalculatedFields
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class CalculatedFieldProviderIntegrationTests
    {
        [TestCase("core:stringField", "'Test'+Name", "TestName1", typeof(string))]
        [TestCase("core:intField", "len(Name)", "5", typeof(int))]
        [TestCase("core:decimalField", "convert(decimal,len(Name)+0.5)", "5.5", typeof(decimal))]
        [TestCase("core:currencyField", "convert(currency,len(Name)+0.5)", "5.5", typeof(decimal))]
        [TestCase("core:boolField", "Name='Name1'", "True", typeof(bool))]
        [TestCase("core:dateField", "iif(Name='Name1',getdate(),getdate())", "*", typeof(DateTime))]
        [TestCase("core:timeField", "iif(Name='Name1',gettime(),gettime())", "*", typeof(DateTime))]
        [TestCase("core:dateTimeField", "iif(Name='Name1',getdatetime(),getdatetime())", "*", typeof(DateTime))]
        [RunWithTransaction]
        [RunAsDefaultTenant]

        public void Test_FieldTypes(string fieldTypeAlias, string calc, string exected, Type expectedType)
        {
            var scenario = CreateScenario(fieldTypeAlias, calc);
            var field = scenario.Item1;
            var inst = scenario.Item2;

            // Get provider
            ICalculatedFieldProvider provider = Factory.CalculatedFieldProvider;

            // Run test
            object result = provider.GetCalculatedFieldValue(field.Id, inst.Id, CalculatedFieldSettings.Default);

            // Check results
            CheckResult(result, exected);
        }

        [Test]
        [RunAsDefaultTenant]

        public void Test_Invalidate_WhenCalcChanged()
        {
            EntityType type = null;

            try
            {
                var scenario = CreateScenario("core:stringField", "'TestA '+Name");
                var field = scenario.Item1;
                var inst = scenario.Item2;
                type = scenario.Item3;

                // Get provider
                ICalculatedFieldProvider provider = Factory.CalculatedFieldProvider;

                // Run test
                object result = provider.GetCalculatedFieldValue(field.Id, inst.Id, CalculatedFieldSettings.Default);
                CheckResult(result, "TestA Name1");

                // Change calculation
                field.FieldCalculation = "'TestB ' + Name";
                field.Save();

                result = provider.GetCalculatedFieldValue(field.Id, inst.Id, CalculatedFieldSettings.Default);
                CheckResult(result, "TestB Name1");
            }
            finally
            {
                if (type != null)
                    type.Delete();
            }
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]

        public void Test_Invalidate_WhenDataChanged()
        {
            EntityType type = null;

            try
            {
                var scenario = CreateScenario("core:stringField", "'TestA '+Name");
                var field = scenario.Item1;
                var inst = scenario.Item2;
                type = scenario.Item3;

                // Get provider
                ICalculatedFieldProvider provider = Factory.CalculatedFieldProvider;

                // Run test
                object result = provider.GetCalculatedFieldValue(field.Id, inst.Id, CalculatedFieldSettings.Default);
                CheckResult(result, "TestA Name1");

                // Change data
                inst.Name = "Name9";
                inst.Save();

                result = provider.GetCalculatedFieldValue(field.Id, inst.Id, CalculatedFieldSettings.Default);
                CheckResult(result, "TestA Name9");
            }
            finally
            {
                if (type != null)
                    type.Delete();
            }
        }



        private Tuple<Field, Resource, EntityType> CreateScenario(string fieldTypeAlias, string calculation)
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

            return new Tuple<Field, Resource, EntityType>(field, inst, type);
        }

        private void CheckResult(object actual, string expected)
        {
            if (expected == "*")
                Assert.That(actual.ToString(), Is.Not.Null.And.Not.Empty);
            else
                Assert.That(actual.ToString(), Is.EqualTo(expected));
        }
    }
}
