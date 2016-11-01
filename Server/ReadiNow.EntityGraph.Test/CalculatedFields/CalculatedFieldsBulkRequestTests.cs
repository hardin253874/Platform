// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database.Types;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.EntityGraph.Test.CalculatedFields
{
    /// <summary>
    /// Test integration of calculated fields into the bulk 
    /// </summary>
    [RunWithTransaction]
    [RunAsDefaultTenant]
    [TestFixture]
    public class CalculatedFieldsBulkRequestTests
    {
        [TestCase("core:stringField", "'Test'+Name", "TestName1", typeof(StringType), typeof(string))]
        [TestCase("core:intField", "len(Name)", "5", typeof(Int32Type), typeof(int))]
        [TestCase("core:decimalField", "convert(decimal,len(Name)+0.5)", "5.5", typeof(DecimalType), typeof(decimal))]
        [TestCase("core:currencyField", "convert(currency,len(Name)+0.5)", "5.5", typeof(CurrencyType), typeof(decimal))]
        [TestCase("core:boolField", "Name='Name1'", "True", typeof(BoolType), typeof(bool))]
        [TestCase("core:dateField", "iif(Name='Name1',getdate(),getdate())", "*", typeof(DateType), typeof(DateTime))]
        [TestCase("core:timeField", "iif(Name='Name1',gettime(),gettime())", "*", typeof(TimeType), typeof(DateTime))]
        [TestCase("core:dateTimeField", "iif(Name='Name1',getdatetime(),getdatetime())", "*", typeof(DateTimeType), typeof(DateTime))]
        public void Test_CalculatedFieldsInBulkRequestRunner_FieldTypes(string fieldTypeAlias, string calc, string exected, Type databaseType, Type dataType)
        {
            var scenario = CreateScenario(fieldTypeAlias, calc);
            var field = scenario.Item1;
            var inst = scenario.Item2;

            string request = "#" + field.Id.ToString();
            EntityData result = BulkRequestRunner.GetEntityData(new EntityRef(inst.Id), request);

            // Check results
            CheckResult(result, exected, databaseType, dataType);
        }

        [Test]
        public void Test_Data_Changed()
        {
            var scenario = CreateScenario("core:stringField", "'Test'+Name");
            var field = scenario.Item1;
            var inst = scenario.Item2;

            string request = "#" + field.Id.ToString();
            EntityData result = BulkRequestRunner.GetEntityData(new EntityRef(inst.Id), request);

            CheckResult(result, "TestName1", typeof(StringType), typeof(string));

            // Rename
            inst.Name = "NewName";
            inst.Save();

            result = BulkRequestRunner.GetEntityData(new EntityRef(inst.Id), request);

            CheckResult(result, "TestNewName", typeof(StringType), typeof(string));
        }

        private Tuple<Field, Resource> CreateScenario(string fieldTypeAlias, string calculation)
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

            return new Tuple<Field, Resource>(field, inst);
        }

        private void CheckResult(EntityData actual, string expected, Type databaseType, Type dataType)
        {
            TypedValue value = actual.Fields[0].Value;

            Assert.That(value.Type, Is.TypeOf(databaseType));
            object data = value.Value;
            Assert.That(data, Is.InstanceOf(dataType));

            if (dataType == typeof(DateTime) && data != null)
            {
                DateTime dateOrTime = (DateTime)data;
                Assert.That(dateOrTime.Kind, Is.EqualTo(DateTimeKind.Utc));
            }

            if (expected == "*")
                Assert.That(data.ToString(), Is.Not.Null.And.Not.Empty);
            else
                Assert.That(data.ToString(), Is.EqualTo(expected));
        }
    }
}
