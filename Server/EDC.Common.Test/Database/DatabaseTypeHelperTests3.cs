// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Database;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.Test.Database
{
    /// <summary>
    ///     This is a test class for the  DatabaseTypeHelper type.
    ///     Tests the converters on DatabaseTypeHelper itself, rather than on DatabaseType.
    /// </summary>
    [TestFixture]
    public class DatabaseTypeHelperTests3
    {
        //DatabaseType.BinaryType
        //DatabaseType.StructureLevelsType

        private static void TestType<T>(DatabaseType type, T value, string sValue,
            bool testDatabaseTypeHelper = true, bool testTypeOverrides = true, bool testFromString = true, bool testToString = true) where T : IComparable
        {
            if (testDatabaseTypeHelper)
            {
                if (testFromString)
                {
                    // Test DatabaseTypeHelper.ConvertFromString
                    object actualValue = DatabaseTypeHelper.ConvertFromString(type, sValue);
                    Assert.IsTrue(value.CompareTo(actualValue) == 0, "DatabaseTypeHelper.ConvertFromString");
                }

                if (testToString)
                {
                    // Test DatabaseTypeHelper.ConvertToString
                    string actualString = DatabaseTypeHelper.ConvertToString(type, value);
                    Assert.AreEqual(sValue, actualString, "DatabaseTypeHelper.ConvertToString");
                }
            }

            if (testTypeOverrides)
            {
                if (testFromString)
                {
                    // Test type.ConvertFromString
                    object actualValue2 = type.ConvertFromString(sValue);
                    Assert.IsTrue(value.CompareTo(actualValue2) == 0, "DatabaseTypeHelper.ConvertFromString");
                }

                if (testToString)
                {
                    // Test type.ConvertToString
                    string actualString2 = type.ConvertToString(value);
                    Assert.AreEqual(sValue, actualString2, "DatabaseTypeHelper.ConvertToString");
                }
            }
        }

        [Test]
        public void Test_Unknown_ToString_Null()
        {
            string result = DatabaseTypeHelper.ConvertToString(DatabaseType.UnknownType, null);
            Assert.IsNull(result);
        }

        [Test]
        public void Test_Unknown_ToString_Empty()
        {
            string result = DatabaseTypeHelper.ConvertToString(DatabaseType.UnknownType, "");
            Assert.IsNull(result);
        }

        [Test]
        public void Test_Unknown_FromString_Null()
        {
            object result = DatabaseTypeHelper.ConvertFromString(DatabaseType.UnknownType, null);
            Assert.IsNull(result);
        }

        [Test]
        public void Test_Unknown_FromString_Empty()
        {
            object result = DatabaseTypeHelper.ConvertFromString(DatabaseType.UnknownType, "");
            Assert.IsNull(result);
        }

        [Test]
        public void Test_Bool_True()
        {
            TestType<bool>(DatabaseType.BoolType, true, "True");
        }

        [Test]
        public void Test_Bool_False()
        {
            TestType<bool>(DatabaseType.BoolType, false, "False");
        }

        [Test]
        public void Test_Guid()
        {
            TestType<Guid>(DatabaseType.GuidType, new Guid("a572fd86-9df2-44dd-b6ad-d8314dec88d6"), "{a572fd86-9df2-44dd-b6ad-d8314dec88d6}");
        }

        [Test]
        public void Test_Empty()
        {
            TestType<Guid>(DatabaseType.GuidType, Guid.Empty, "{00000000-0000-0000-0000-000000000000}");
        }

        [Test]
        public void Test_String()
        {
            TestType<string>(DatabaseType.StringType, "Hello", "Hello");
        }

        [Test]
        public void Test_String_Empty()
        {
            TestType<string>(DatabaseType.StringType, "", "");
        }

        [Test]
        public void Test_Xml()
        {
            TestType<string>(DatabaseType.XmlType, "<data></data>", "<data></data>");
        }

        [Test]
        public void Test_Xml_Empty()
        {
            TestType<string>(DatabaseType.XmlType, "", "");
        }

        [Test]
        public void Test_Date()
        {
            TestType<DateTime>(DatabaseType.DateType, new DateTime(2013,12,31), "2013-12-31");
        }

        [Test]
        public void Test_DateTime()
        {
            TestType<DateTime>(DatabaseType.DateTimeType, new DateTime(2013,12,31, 13, 45, 59), "2013-12-31 13:45:59");
        }

        [Test]
        public void Test_Time()
        {
            // TODO : Fix Time!!!!
            TestType<DateTime>(DatabaseType.TimeType, new DateTime(1753, 1, 1, 13, 45, 59), "13:45:59", testTypeOverrides:false);

            // Trying to rid TimeType of the DateTime/Timespan duality. Always deal in DateTime now.
            DatabaseType.TimeType.ConvertFromString("13:45:59").ShouldBeEquivalentTo(new DateTime(1753, 1, 1, 13, 45, 59, DateTimeKind.Utc));
            DatabaseType.TimeType.ConvertToString(new TimeSpan(13, 45, 59)).Should().Be("13:45:59");
        }

        [Test]
        public void Test_Int32()
        {
            TestType<int>(DatabaseType.Int32Type, 123456, "123456");
        }

        [Test]
        public void Test_Int32_Zero()
        {
            TestType<int>(DatabaseType.Int32Type, 0, "0");
        }

        [Test]
        public void Test_Int32_Negative()
        {
            TestType<int>(DatabaseType.Int32Type, -123456, "-123456");
        }

        [Test]
        public void Test_Identifier()
        {
            TestType<long>(DatabaseType.IdentifierType, 123456, "123456");
        }

        [Test]
        public void Test_Identifier_Zero()
        {
            TestType<long>(DatabaseType.IdentifierType, 0, "0");
        }

        [Test]
        public void Test_Identifier_Negative()
        {
            TestType<long>(DatabaseType.IdentifierType, -123456, "-123456");
        }

        [Test]
        public void Test_ChoiceRelationship()
        {
            TestType<string>(DatabaseType.ChoiceRelationshipType, "123456", "123456");   // this should be a long
        }

        //[Test]
        //public void Test_ChoiceRelationship_Empty()
        //{
        //    // Need to decide what the expected behavior for empty string
        //    TestType<string>(DatabaseType.ChoiceRelationshipType, "", null);   // this should be a long
        //}

        [Test]
        public void Test_InlineRelationship()
        {
            TestType<string>(DatabaseType.InlineRelationshipType, "123456", "123456");   // this should be a long
        }

        //[Test]
        //public void Test_InlineRelationship_Empty()
        //{
        //    TestType<string>(DatabaseType.InlineRelationshipType, "", null);   // this should be a long
        //}

        [Test]
        public void Test_Decimal()
        {
            TestType<decimal>(DatabaseType.DecimalType, -1234.56M, "-1234.56");
        }
        [Test]
        public void Test_Decimal_Zero()
        {
            TestType<decimal>(DatabaseType.DecimalType, 0M, "0");
        }

        [Test]
        public void Test_Currency()
        {
            TestType<decimal>(DatabaseType.CurrencyType, -1234.56M, "-1234.56");
        }

        [Test]
        public void Test_Currency_Zero()
        {
            TestType<decimal>(DatabaseType.CurrencyType, 0M, "0");
        }

    }
}