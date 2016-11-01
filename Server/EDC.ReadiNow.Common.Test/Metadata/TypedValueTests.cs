// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Metadata;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Metadata.Media
{
    /// <summary>
    ///     This class tests the ColorInfoHelper class.
    ///     Note: Choice/Inline relationships are currently treated as strings (for compatibility), but these should really be changed to longs.
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class TypedValueTests
    {
        #region API edge cases
        [Test]
        public void Check_Untyped_Null()
        {
            TypedValue tv = new TypedValue();
            Assert.IsNull(tv.Value, "Before set");
            tv.Value = null;
            Assert.IsNull(tv.Value, "After set");
        }

        [Test]
        public void Check_String()
        {
            CheckType(DatabaseType.StringType, "abcd");
        }

        [Test]
        public void Check_String_Empty()
        {
            CheckType(DatabaseType.StringType, "");
        }

        [Test]
        public void Check_Int()
        {
            CheckType(DatabaseType.Int32Type, 123);
        }

        [Test]
        public void Check_Decimal()
        {
            CheckType(DatabaseType.DecimalType, 123.456M);
        }

        [Test]
        public void Check_Currency()
        {
            CheckType(DatabaseType.CurrencyType, 123.45M);
        }

        [Test]
        public void Check_Date()
        {
            CheckType(DatabaseType.DateType, new DateTime(2001, 12, 31));
        }

        [Test]
        public void Check_DateTime()
        {
            CheckType(DatabaseType.DateTimeType, new DateTime(2001, 12, 31, 15, 30, 45));
        }

        [Test]
        public void Check_Time()
        {
            CheckType(DatabaseType.TimeType, TimeType.NewTime(20, 45, 50));
        }

        [Test]
        public void Check_Identifier()
        {
            CheckType(DatabaseType.IdentifierType, 1234L);
        }

        [Test]
        public void Check_ChoiceRelationship()
        {
            CheckType(DatabaseType.ChoiceRelationshipType, "1234"); // this should be 1234L
        }

        [Test]
        public void Check_InlineRelationship()
        {
            CheckType(DatabaseType.InlineRelationshipType, "1234"); // this should be 1234L
        }

        [Test]
        public void Check_Xml()
        {
            CheckType(DatabaseType.XmlType, "<data></data>");
        }

        [Test]
        public void Check_Guid()
        {
            CheckType(DatabaseType.GuidType, new Guid("{da0d062e-011d-4359-a723-a7ca4b7f7dc8}"));
        }

        [Test]
        public void Check_Bool_False()
        {
            CheckType(DatabaseType.BoolType, false);
        }

        [Test]
        public void Check_Bool_True()
        {
            CheckType(DatabaseType.BoolType, true);
        }
        #endregion

        public static void CheckType<T>(DatabaseType type, T testData) where T : IComparable
        {
            // Before set
            TypedValue tv0 = new TypedValue();
            Assert.IsNull(tv0.Value, "Value before set");
            Assert.IsNull(tv0.ValueString, "ValueString before set");

            // Data check
            TypedValue tv1 = new TypedValue();
            tv1.Type = type;
            tv1.Value = testData;
            Assert.IsInstanceOf<T>(tv1.Value, "Check type1");
            Assert.IsTrue(testData.CompareTo(tv1.Value) == 0, "Check value1");

            // Null check
            TypedValue tv2 = new TypedValue();
            tv2.Type = type;
            tv2.Value = null;
            Assert.IsInstanceOf<T>(tv1.Value, "Check type2");
            Assert.IsNull(tv2.Value, "Check value2");
            Assert.IsNull(tv2.ValueString, "Check ValueString");

            // Zero/default check
            if ((object)default(T) != null && !( type is TimeType))
            {
                TypedValue tv3 = new TypedValue();
                tv3.Type = type;
                tv3.Value = default(T);
                Assert.IsInstanceOf<T>(tv1.Value, "Check value3");
                Assert.IsTrue(default(T).CompareTo(tv3.Value) == 0, "Check default value");
            }

            // Ensure that test data round-trips via DataContract serialization
            CheckDataContract<T>(type, testData, false, "data");

            // Ensure that null data round-trips via DataContract serialization
            CheckDataContract<T>(type, null, true, "null");

            // Ensure that default values round-trips via DataContract serialization
            if ((object)default(T) != null && !(type is TimeType))
            {
                CheckDataContract<T>(type, default(T), false, "default");
            }
        }

        public static void CheckDataContract<T>(DatabaseType type, object testData, bool testNull, string message) where T : IComparable
        {
            TypedValue initial = new TypedValue();
            initial.Type = type;
            initial.Value = (object) testData;

            TypedValue actual;
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(TypedValue));
            using (var stream = new MemoryStream())
            {
                dataContractSerializer.WriteObject(stream, initial);
                stream.Position = 0;
                actual = (TypedValue) dataContractSerializer.ReadObject(stream);
            }

            if (type is StringType)
            {
                // These tests are here for string, because string is currently broken, but fixing string will break stuff in the console.
                // I.e. currently nulls are getting turned into empty strings, when they shouldn't be.

                string expected = (string)testData ?? "";
                Assert.IsTrue(expected.CompareTo(actual.Value) == 0, "Data contract " + message);

            }
            else
            {
                // These two tests should be the correct behavior for everything, including string.

                if (testData == null)
                {
                    Assert.IsNull(actual.Value, "Data contract " + message);
                }
                else
                {
                    Assert.IsTrue(((T)testData).CompareTo(actual.Value) == 0, "Data contract " + message);
                }
            }
        }


    }
}