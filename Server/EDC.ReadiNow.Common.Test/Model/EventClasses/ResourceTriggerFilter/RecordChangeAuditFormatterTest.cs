// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter.EventHandlers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Model.EventClasses.ResourceTriggerFilter
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class RecordChangeAuditFormatterTest_R_2_4
    {
        const string Fourty7Characters = "12345678901234567890123456789012345678901234567";
        const string FiftyCharacters =   "12345678901234567890123456789012345678901234567890";
        const string FiftyOneCharacters = FiftyCharacters + "1";

        [TestCase("boolField",  null,  true,    "[MyField] changed to 'True'") ]
        [TestCase("boolField",  false,  true,   "[MyField] changed to 'True'") ]
        [TestCase("boolField",  true,  null,    "[MyField] changed to 'False'") ]
        [TestCase("boolField",  true,  false,   "[MyField] changed to 'False'") ]

        [TestCase("xmlField",  null,  "<a/>",   "[MyField] set") ]
        [TestCase("xmlField",  "<a/>",  "<b/>", "[MyField] changed") ]
        [TestCase("xmlField", "<a/>",  null,    "[MyField] cleared")]

        [TestCase("aliasField", null, "set",    "[MyField] set to 'set'")]
        [TestCase("aliasField", "set", "extra", "[MyField] changed from 'set' -> 'extra'")]
        [TestCase("aliasField", "extra", null,  "[MyField] cleared from 'extra'")]

        [TestCase("autoNumberField", null, 1,  "[MyField] set")]
        [TestCase("autoNumberField", 1, 2,     "[MyField] changed")]
        [TestCase("autoNumberField", 2, null,  "[MyField] cleared")]

        [TestCase("stringField", null, "set",   "[MyField] set to 'set'")]
        [TestCase("stringField", "set", "extra", "[MyField] changed from 'set' -> 'extra'")]
        [TestCase("stringField", "extra", null,  "[MyField] cleared from 'extra'")]
        [TestCase("stringField", null, FiftyCharacters, "[MyField] set to '" +  FiftyCharacters + "'")]
        [TestCase("stringField", null, FiftyOneCharacters, "[MyField] set to '" + Fourty7Characters + "...'")]
        public void FromToTest(string fieldType, object oldValue, object newValue, string expected) 
        {
            var field = Entity.Create(new EntityRef("core", fieldType)).Cast<Field>();
            field.Name = "MyField";

            FromToTest(field, oldValue, newValue, expected);
        }

        public void FromToTest(Field field, object oldValue, object newValue, string expected)
        {
            
            var formatter = new RecordChangeAuditFormatter("dummyName", "dummyId", null);

            formatter.AddChangedField(field, oldValue, newValue);
            var result = formatter.ToString();

            Assert.That(result.Trim(), Is.EqualTo(expected));
        }

        [TestCase("currencyField")]
        [TestCase("decimalField")]
        [TestCase("intField")]
        public void NumberFromToTest(string fieldType)
        {
            FromToTest(fieldType, null, 1.0, "[MyField] set to '1'");
            FromToTest(fieldType, 1.0, null, "[MyField] cleared from '1'");
            FromToTest(fieldType, 1.0, 2.0, "[MyField] changed from '1' -> '2'");
        }

        [Test]
        public void DateFromToTest()
        {
            FromToTest("dateField",  null,  new DateTime(2016, 11, 23), "[MyField] set to '2016-11-23'");
            FromToTest("dateField",  new DateTime(2016, 11, 23),  new DateTime(2017, 11, 23), "[MyField] changed from '2016-11-23' -> '2017-11-23'");
            FromToTest("dateField",  new DateTime(2016, 11, 23),  null, "[MyField] cleared from '2016-11-23'");
        }

        [Test]
        public void DateTimeFromToTest()
        {
            FromToTest("dateTimeField", null, new DateTime(2016, 11, 23, 1, 2, 3, DateTimeKind.Utc), "[MyField] set to '2016-11-23 01:02:03.000Z'");
            FromToTest("dateTimeField", new DateTime(2016, 11, 23, 1, 2, 3, DateTimeKind.Utc), new DateTime(2017, 11, 23, 1, 2, 3, DateTimeKind.Utc), "[MyField] changed from '2016-11-23 01:02:03.000Z' -> '2017-11-23 01:02:03.000Z'");
            FromToTest("dateTimeField", new DateTime(2016, 11, 23, 1, 2, 3, DateTimeKind.Utc), null, "[MyField] cleared from '2016-11-23 01:02:03.000Z'");

        }

        [Test]
        public void TimeFromToTest()
        {
            FromToTest("timeField", null, new DateTime(2016, 11, 23, 1, 2, 3), "[MyField] set to '01:02:03'");
            FromToTest("timeField", new DateTime(2016, 11, 23, 1, 2, 3), new DateTime(2017, 11, 23, 1, 2, 3), "[MyField] changed from '01:02:03' -> '01:02:03'");
            FromToTest("timeField", new DateTime(2016, 11, 23, 1, 2, 3), null, "[MyField] cleared from '01:02:03'");
        }

        [TestCase(null, "new password", "[Password] set")]
        [TestCase("old password", "new password", "[Password] changed")]
        [TestCase("old password", null, "[Password] cleared")]
        public void TestPasswordField(object oldValue, object newValue, string expected)
        {
            FromToTest(Entity.Get<Field>(new EntityRef("core:password")), oldValue, newValue, expected);
        }


        [TestCase("MyRel", "oldResource", "newResource", "[MyRel] changed from 'oldResource' -> 'newResource'") ]
        [TestCase("MyRel", "oldResource", "newResource", "[MyRel] changed from 'oldResource' -> 'newResource'") ]
        [TestCase("MyRel", "oldResource", null, "[MyRel] cleared from 'oldResource'") ]
        [TestCase("MyRel", null, "newResource", "[MyRel] set to 'newResource'") ]
        public void TestLookup(string relName, string oldResName, string newResName, string expected)
        {
            var formatter = new RecordChangeAuditFormatter("dummyEntity", "dummyUser", null);

            var oldRes = oldResName != null ? new Resource { Name = oldResName } : null;
            var newRes = newResName != null ? new Resource { Name = newResName } : null;

            formatter.AddChangedLookup(relName, oldRes, newRes);
            var result = formatter.ToString();

            Assert.That(result, Is.StringStarting(expected));
        }

        [TestCase("MyRel", 0, 1, 0,  "[MyRel] added 'a1'")]
        [TestCase("MyRel", 0, 2, 0, "[MyRel] added 'a1' | 'a2'")]
        [TestCase("MyRel", 0, 3, 0, "[MyRel] added 'a1' | 'a2' plus 1 more")]

        [TestCase("MyRel", 1, 0, 0, "[MyRel] removed 'r1'")]
        [TestCase("MyRel", 2, 0, 0, "[MyRel] removed 'r1' | 'r2'")]
        [TestCase("MyRel", 3, 0, 0, "[MyRel] removed 'r1' | 'r2' plus 1 more")]

        [TestCase("MyRel", 1, 1, 0, "[MyRel] removed 'r1' added 'a1'")]
        [TestCase("MyRel", 3, 3, 0, "[MyRel] removed 'r1' | 'r2' plus 1 more added 'a1' | 'a2' plus 1 more")]
        public void TestRelationship(string relName, int removed, int added, int count, string expected)
        {
            var formatter = new RecordChangeAuditFormatter("dummyEntity", "dummyUser", null) { MaxRelationshipsNamesReported = 2 };
           
            
            formatter.AddChangedRelationship(relName, CreateDummies("r", removed), CreateDummies("a", added), count);
            var result = formatter.ToString();

            Assert.That(result, Is.EqualTo(expected));
        }


        [Test]
        public void RemovingThenAddingSameRecordIgnored()
        {
            var formatter = new RecordChangeAuditFormatter("dummyEntity", "dummyUser", null);

            var overlapping = CreateDummies("o", 1).ToList();
            var removed = CreateDummies("r", 1).Union(overlapping);
            var added = CreateDummies("a", 1).Union(overlapping);
            formatter.AddChangedRelationship("MyRel", removed, added, 10);
            var result = formatter.ToString();

            Assert.That(result, Is.EqualTo("[MyRel] removed 'r1' added 'a1'"));
        }

        [Test]
        public void Delete()
        {
            var formatter = new RecordChangeAuditFormatter("dummyEntity", "dummyUser", null);

            formatter.IsDelete = true;
            var result = formatter.ToString();

            Assert.That(result, Is.EqualTo("deleted"));
        }

        [Test]
        public void DescriptionTooLong()
        {
            var formatter = new RecordChangeAuditFormatter("dummyEntity", "dummyUser", null) { MaxDescriptionLength = 20 };

            formatter.AddChangedRelationship("Dummy", CreateDummies("r", 10), CreateDummies("a", 0), 10);
            var result = formatter.ToString();

            Assert.That(result.Length, Is.EqualTo(20));
        }



        [Test]
        public void EntryOrder()
        {
            var formatter = new RecordChangeAuditFormatter("dummyEntity", "dummyUser", null);

            formatter.AddChangedLookup("B", null, new Resource {Name="r" } );


            var field1 = new IntField().As<Field>();
            field1.Name = "C";

            formatter.AddChangedField(field1, 1, 2);

            var field2 = new IntField().As<Field>();
            field2.Name = "A";

            formatter.AddChangedField(field2, 1, 2);

            var result = formatter.ToString();


            var indexA = result.IndexOf("[A]");
            var indexB = result.IndexOf("[B]");
            var indexC = result.IndexOf("[C]");

            Assert.That(indexA, Is.LessThan(indexB));
            Assert.That(indexB, Is.LessThan(indexC));
        }

        [TestCase("user", null, "entity", false, "user updated 'entity'")]
        [TestCase("user", "user2", "entity", false, "user(user2) updated 'entity'")]
        [TestCase(null, "user2", "entity", false, "user2 updated 'entity'")]
        [TestCase(null, null, "entity", false, "[Unnamed] updated 'entity'")]
        [TestCase("user", null, null, false, "user updated '[Unnamed]'")]
        [TestCase("user", null, "entity", true, "user created 'entity'")]
        public void GetNameString(string userName, string secondaryUser, string entityName, bool isCreate, string expected)
        {
            var formatter = new RecordChangeAuditFormatter("dummyEntity", "dummyUser", null);
            formatter.AddUserName(userName, secondaryUser);
            formatter.AddEntityName(entityName);
            formatter.IsCreate = isCreate;

            Assert.That(formatter.GetNameString(), Is.EqualTo(expected));
        }



        IEnumerable<long> CreateDummies(string prefix, int count)
        {
            for (int i=0; i< count; i++)
            {
                var e = Entity.Create<Resource>();
                e.Name = prefix + (i+1).ToString();
                yield return e.Id;
            }
        }

    }
}
