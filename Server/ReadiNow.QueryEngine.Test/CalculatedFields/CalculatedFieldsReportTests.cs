// Copyright 2011-2016 Global Software Innovation Pty Ltd
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Test;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.ReadiNow.Core;
using EDC.Database.Types;

namespace ReadiNow.QueryEngine.Test.CalculatedFields
{
    [TestFixture]
    public class CacheScenarioTests
    {
        [TestCase("core:stringField", "'Test'+Name", "TestName1", typeof(StringType), typeof(string))]
        [TestCase("core:intField", "len(Name)", "5", typeof(Int32Type), typeof(int))]
        [TestCase("core:decimalField", "convert(decimal,len(Name)+0.5)", "5.5000000000", typeof(DecimalType), typeof(decimal))] // hmm
        [TestCase("core:currencyField", "convert(currency,len(Name)+0.5)", "5.5000", typeof(CurrencyType), typeof(decimal))]
        [TestCase("core:boolField", "Name='Name1'", "True", typeof(BoolType), typeof(bool))]
        [TestCase("core:dateField", "iif(Name='Name1',getdate(),getdate())", "*", typeof(DateType), typeof(DateTime))]
        [TestCase("core:timeField", "iif(Name='Name1',gettime(),gettime())", "*", typeof(TimeType), typeof(DateTime))]
        [TestCase("core:dateTimeField", "iif(Name='Name1',getdatetime(),getdatetime())", "*", typeof(DateTimeType), typeof(DateTime))]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void Test_FieldTypes(string fieldTypeAlias, string calc, string equalTo, Type databaseType, Type cellDataType)
        {
            // Define type
            EntityType type = new EntityType();

            // Define calculated field
            Field field = Entity.Create(new EntityRef(fieldTypeAlias)).As<Field>();
            field.IsCalculatedField = true;
            field.FieldCalculation = calc;
            type.Fields.Add(field);

            type.Save();

            // Create instance
            Resource inst = Entity.Create(type.Id).As<Resource>();
            inst.Name = "Name1";
            inst.Save();

            // Define structured query
            var sq = new SQ.StructuredQuery();
            sq.RootEntity = new SQ.ResourceEntity(new EntityRef(type.Id));
            var column = new SQ.SelectColumn();
            column.Expression = new SQ.ResourceDataColumn(sq.RootEntity, new EntityRef(field.Id));
            sq.SelectColumns.Add(column);

            // Run report
            var settings = new SQ.QuerySettings();
            var queryResult = Factory.QueryRunner.ExecuteQuery(sq, settings);

            // Check results
            Assert.That(queryResult.Columns[0].ColumnType, Is.TypeOf(databaseType));
            var row = queryResult.DataTable.Rows[0];
            object data = row[0];
            Assert.That(data, Is.InstanceOf(cellDataType));

            if (equalTo == "*")
                Assert.That(data.ToString(), Is.Not.Null.And.Not.Empty);
            else
                Assert.That(data.ToString(), Is.EqualTo(equalTo));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_InvalidateWhen_ReferencedDataChanged()
        {
            EntityType type = null;
            Field field;
            Resource inst;

            try
            {
                // Define type
                type = new EntityType();

                // Define calculated field
                field = Entity.Create(new EntityRef("core:stringField")).As<Field>();
                field.IsCalculatedField = true;
                field.FieldCalculation = "'Value '+Description";
                type.Fields.Add(field);
                type.Save();

                // Create instance
                inst = Entity.Create(type.Id).As<Resource>();
                inst.Name = "Name1";
                inst.Description = "One";
                inst.Save();

                // Define structured query
                var sq = new SQ.StructuredQuery();
                sq.RootEntity = new SQ.ResourceEntity(new EntityRef(type.Id));
                var column = new SQ.SelectColumn();
                column.Expression = new SQ.ResourceDataColumn(sq.RootEntity, new EntityRef(field.Id));
                sq.SelectColumns.Add(column);

                // Run report
                var settings = new SQ.QuerySettings();
                var queryResult = Factory.QueryRunner.ExecuteQuery(sq, settings);

                // Check results
                var row = queryResult.DataTable.Rows[0];
                object data = row[0];
                Assert.That(data, Is.EqualTo("Value One"));

                // Modify data
                inst.Description = "Two";
                inst.Save();

                // Rerun report
                queryResult = Factory.QueryRunner.ExecuteQuery(sq, settings);

                // Recheck results
                row = queryResult.DataTable.Rows[0];
                data = row[0];
                Assert.That(data, Is.EqualTo("Value Two"));
            }
            finally
            {
                if (type != null)
                    type.Delete();
                // Using RunWithTransaction causes this test to fail
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_InvalidateWhen_CalculationChanged()
        {
            EntityType type = null;
            Field field;
            Resource inst;

            try
            {
                // Define type
                type = new EntityType();

                // Define calculated field
                field = Entity.Create(new EntityRef("core:stringField")).As<Field>();
                field.IsCalculatedField = true;
                field.FieldCalculation = "Name + ' One'";
                type.Fields.Add(field);

                type.Save();

                // Create instance
                inst = Entity.Create(type.Id).As<Resource>();
                inst.Name = "Script";
                inst.Save();

                // Define structured query
                var sq = new SQ.StructuredQuery();
                sq.RootEntity = new SQ.ResourceEntity(new EntityRef(type.Id));
                var column = new SQ.SelectColumn();
                column.Expression = new SQ.ResourceDataColumn(sq.RootEntity, new EntityRef(field.Id));
                sq.SelectColumns.Add(column);

                // Run report
                var settings = new SQ.QuerySettings();
                var queryResult = Factory.QueryRunner.ExecuteQuery(sq, settings);

                // Check results
                var row = queryResult.DataTable.Rows[0];
                object data = row[0];
                Assert.That(data, Is.EqualTo("Script One"));

                // Modify calculation
                field = Entity.Get<Field>(field.Id, true);
                field.FieldCalculation = "Name + ' Two'";
                field.Save();

                // Rerun report
                queryResult = Factory.QueryRunner.ExecuteQuery(sq, settings);

                // Recheck results
                row = queryResult.DataTable.Rows[0];
                data = row[0];
                Assert.That(data, Is.EqualTo("Script Two"));
            }
            finally
            {
                if (type != null)
                    type.Delete();

                // Using RunWithTransaction causes this test to fail
            }
        }

    }
}
