// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.EntityRequests
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class WriteOnlyFieldReadValueGeneratorTests
    {
        [Test]
        public void GenerateValue_NullFieldId()
        {
            Assert.That(
                () => new WriteOnlyFieldReadValueGenerator().GenerateValue(null, new StringType()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("fieldId"));
        }

        [Test]
        public void GenerateValue_NullType()
        {
            Assert.That(
                () => new WriteOnlyFieldReadValueGenerator().GenerateValue(new EntityRef(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("type"));
        }

        [Test]
        [TestCase("core:name", "string", "*******")]
        [TestCase("core:password", "string", "*******")]
        [TestCase("core:badLogonCount", "int", null)]
        public void GenerateValue(string fieldAlias, string type, object expectedValue)
        {
            EntityRef fieldEntityRef;
            Dictionary<string, DatabaseType> nameToDatabaseType;
            DatabaseType databaseType;

            nameToDatabaseType = new Dictionary<string, DatabaseType>
            {
                { "string", DatabaseType.StringType },
                { "int", DatabaseType.Int32Type }
            };

            fieldEntityRef = new EntityRef(fieldAlias);
            databaseType = nameToDatabaseType[type];

            Assert.That(
                new WriteOnlyFieldReadValueGenerator().GenerateValue(fieldEntityRef, databaseType),
                Has.Property("Value").EqualTo(expectedValue));
        }
    }
}
