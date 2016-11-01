// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ReadiNow.Expressions.Test.CalculatedFields
{
    /// <summary>
    /// 
    /// </summary>
    [RunAsDefaultTenant]
    [TestFixture]
    public class DateTimeTests
    {
        [TestCase("core:dateField", "[Field]", "2016-07-01T00:00:00", "2016-07-01T00:00:00")]
        [TestCase("core:dateTimeField", "[Field]", "2016-07-01T12:34:56", "2016-07-01T12:34:56")]
        [TestCase("core:timeField", "[Field]", "1753-01-01T12:34:56", "1753-01-01T12:34:56")]
        public void Test_DateTypes(string fieldTypeAlias, string calculation, string fieldValue, string expectValue)
        {
            var curContext = RequestContext.GetContext();
            var newContext = new RequestContextData(curContext.Identity, curContext.Tenant, curContext.Culture, "Australia/Sydney");
            RequestContext.SetContext(newContext);

            try
            {
                DateTime value = DateTime.ParseExact(fieldValue + ".0000000Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                DateTime expected = DateTime.ParseExact(expectValue + ".0000000Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                Assert.That(value.Kind, Is.EqualTo(DateTimeKind.Utc), "Test sanity check");

                var scenario = CreateScenario(fieldTypeAlias, "[Field]", value);
                var field = scenario.Item1;
                var inst = scenario.Item2;

                // Get calculated field
                DateTime result = (DateTime)inst.GetField(field.Id);

                Assert.That(result, Is.EqualTo(expected));
                Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
            }
            finally
            {
                RequestContext.SetContext(curContext);
            }
        }

        private Tuple<Field, Resource> CreateScenario(string fieldTypeAlias, string calculation, object instData)
        {
            // Define type
            EntityType type = new EntityType();

            // Define calculated field
            Field field = Entity.Create(new EntityRef(fieldTypeAlias)).As<Field>();
            field.IsCalculatedField = true;
            field.FieldCalculation = calculation;
            type.Fields.Add(field);

            Field dataField = Entity.Create(new EntityRef(fieldTypeAlias)).As<Field>();
            dataField.Name = "Field";
            type.Fields.Add(dataField);

            type.Save();

            // Create instance
            Resource inst = Entity.Create(type.Id).As<Resource>();
            inst.Name = "Name1";
            inst.SetField(dataField.Id, instData);
            inst.Save();

            return new Tuple<Field, Resource>(field, inst);
        }
    }
}
