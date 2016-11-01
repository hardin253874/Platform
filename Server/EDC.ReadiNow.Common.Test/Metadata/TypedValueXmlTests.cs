// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Xml;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Metadata;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Metadata.Media
{
    /// <summary>
    ///     This class tests the ColorInfoHelper class
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class TypedValueXmlTests
    {
        public static string RemoveNamespaces(string xml)
        {
            string res = xml;
            res = res.Replace(@" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "");
            res = res.Replace(@" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "");
            res = res.Replace(@" xmlns=""http://enterprisedata.com.au/readinow/v2/query/2.0""", "");
            return res;
        }

        #region Current XML Format

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_String()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.StringType, Value = "Hello" };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""String"">Hello</TypedValue>";

            CheckRoundTrip<string>(tv, xml);
        }
        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_String_Null()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.StringType, Value = null };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""String"" />";

            CheckRoundTrip<string>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Int()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.Int32Type, Value = -1234 };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Int32"">-1234</TypedValue>";

            CheckRoundTrip<int>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Int_Null()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.Int32Type, Value = null };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Int32"" />";

            CheckRoundTrip<int>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Currency()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.CurrencyType, Value = -1234.55 };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Currency"">-1234.55</TypedValue>";

            CheckRoundTrip<decimal>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Decimal()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.DecimalType, Value = 1234.5 };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Decimal"">1234.5</TypedValue>";

            CheckRoundTrip<decimal>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Date()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.DateType, Value = new DateTime(2012, 12, 31) };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Date"">2012-12-31</TypedValue>";

            CheckRoundTrip<DateTime>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Date_Today()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.DateType, ValueString = "TODAY" };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Date"">TODAY</TypedValue>";

            CheckRoundTrip<DateTime>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_DateTime()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.DateTimeType, Value = new DateTime(2012, 12, 31, 15, 30, 0) };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""DateTime"">2012-12-31 15:30:00</TypedValue>";

            CheckRoundTrip<DateTime>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_DateTime_Today()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.DateTimeType, ValueString = "TODAY" };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""DateTime"">TODAY</TypedValue>";

            CheckRoundTrip<DateTime>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Time()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.TimeType, Value = TimeType.NewTime(15, 30, 0) };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Time"">15:30:00</TypedValue>";

            CheckRoundTrip<DateTime>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Bool()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.BoolType, Value = true };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Bool"">True</TypedValue>";

            CheckRoundTrip<bool>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Bool_Null()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.BoolType, Value = null };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Bool"" />";

            CheckRoundTrip<bool>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Guid()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.GuidType, Value = new Guid("da0d062e-011d-4359-a723-a7ca4b7f7dc8") };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Guid"">{da0d062e-011d-4359-a723-a7ca4b7f7dc8}</TypedValue>";

            CheckRoundTrip<Guid>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Xml()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.XmlType, Value = "<data></data>" };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Xml"">&lt;data&gt;&lt;/data&gt;</TypedValue>";

            CheckRoundTrip<string>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_InlineRelationship()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.InlineRelationshipType, Value = "1234" };  // should be 1234L

            // XML Serialization of InlineRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""InlineRelationship"" entityRef=""true"">1234</TypedValue>";

            CheckRoundTrip<string>(tv, xml);    //long
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_InlineRelationship_WithType()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.InlineRelationshipType, Value = "1234", SourceEntityTypeId = 5678 };  // should be 1234L

            // XML Serialization of InlineRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""InlineRelationship"" entityRef=""true"">1234<SourceEntityTypeId entityRef=""true"">5678</SourceEntityTypeId></TypedValue>";

            CheckRoundTrip<string>(tv, xml);    //long
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_ChoiceRelationship()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.ChoiceRelationshipType, Value = "1234" };  // should be 1234L

            // XML Serialization of InlineRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""ChoiceRelationship"" entityRef=""true"">1234</TypedValue>";

            CheckRoundTrip<string>(tv, xml);    //long
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_ChoiceRelationship_WithType()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.ChoiceRelationshipType, Value = "1234", SourceEntityTypeId = 5678 };  // should be 1234L

            // XML Serialization of ChoiceRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""ChoiceRelationship"" entityRef=""true"">1234<SourceEntityTypeId entityRef=""true"">5678</SourceEntityTypeId></TypedValue>";

            CheckRoundTrip<string>(tv, xml);    //long
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXml_Identifier()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.IdentifierType, Value = 1234 };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue type=""Identifier"" entityRef=""true"">1234</TypedValue>";

            CheckRoundTrip<long>(tv, xml);
        }
        #endregion

        #region Legacy XML format
        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_String()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.StringType, Value = "Hello" };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue>
  <Type xsi:type=""StringType"">
  </Type>
  <Value xsi:type=""xsd:string"">Hello</Value>
</TypedValue>";

            CheckLegacyLoad<string>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_String_NullMetadata()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.StringType, Value = "Hello" };

            // Changes to TypedValue require dev-manager approval
            const string xml = @"<TypedValue>
  <Type xsi:type=""StringType"" />
  <Value xsi:type=""xsd:string"">Hello</Value>
</TypedValue>";

            CheckLegacyLoad<string>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Numeric()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.Int32Type, Value = -1234 };

            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""Int32Type"">
  </Type>
  <Value xsi:type=""xsd:int"">-1234</Value>
</TypedValue>";

            CheckLegacyLoad<int>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Currency()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.CurrencyType, Value = -1234.55 };

            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""CurrencyType"">
  </Type>
  <Value>-1234.55</Value>
</TypedValue>";

            CheckLegacyLoad<decimal>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Decimal()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.DecimalType, Value = 1234.5 };

            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""DecimalType"">
  </Type>
  <Value>1234.5</Value>
</TypedValue>";

            CheckLegacyLoad<decimal>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Date()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.DateType, Value = new DateTime(2012, 12, 31) };

            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""DateType"">
  </Type>
  <Value>2012-12-31</Value>
</TypedValue>";

            CheckLegacyLoad<DateTime>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_DateTime()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.DateTimeType, Value = new DateTime(2012, 12, 31, 15, 30, 0) };

            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""DateTimeType"">
  </Type>
  <Value>2012-12-31 15:30:00.0000000</Value>
</TypedValue>";

            CheckLegacyLoad<DateTime>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Time()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.TimeType, Value = TimeType.NewTime(15, 30, 0) };

            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""TimeType"">
  </Type>
  <Value>01/01/1753 15:30:00</Value>
</TypedValue>";

            CheckLegacyLoad<DateTime>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Bool()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.BoolType, Value = true };

            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""BoolType"">
  </Type>
  <Value>True</Value>
</TypedValue>";

            CheckLegacyLoad<bool>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Guid()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.GuidType, Value = new Guid("da0d062e-011d-4359-a723-a7ca4b7f7dc8") };

            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""GuidType"">
  </Type>
  <Value>{da0d062e-011d-4359-a723-a7ca4b7f7dc8}</Value>
</TypedValue>";

            CheckLegacyLoad<Guid>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_ChoiceRelationship_WithType()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.ChoiceRelationshipType, Value = "1234", SourceEntityTypeId = 5678 }; // should be 1234L

            // XML Serialization of InlineRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""ChoiceRelationshipType"">
  </Type>
  <SourceEntityTypeId>5678</SourceEntityTypeId>
  <Value>1234:Not Available;11293:unknown;</Value>
</TypedValue>";

            CheckLegacyLoad<string>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_InlineRelationship_WithType()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.InlineRelationshipType, Value = "1234", SourceEntityTypeId = 5678 }; // should be 1234L

            // XML Serialization of InlineRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml =
@"<TypedValue>
  <Type xsi:type=""InlineRelationshipType"">
  </Type>
  <SourceEntityTypeId>5678</SourceEntityTypeId>
  <Value>1234:Not Available;11293:unknown;</Value>
</TypedValue>";

            CheckLegacyLoad<string>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_String_LegacyConditionalFormat()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.StringType, Value = "Hello" };

            // XML Serialization of InlineRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml =
@"
<root>
  <whatever>
    <type>String</type>
    <value>Hello</value>
  </whatever>
</root>
";
            CheckLegacyConditionalFormat<string>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Int_LegacyConditionalFormat()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.Int32Type, Value = 100 };

            // XML Serialization of InlineRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml =
@"
<root>
  <whatever>
    <type>Int32</type>
    <value>100</value>
  </whatever>
</root>
";
            CheckLegacyConditionalFormat<int>(tv, xml);
        }

        /// <summary>
        ///     Tests that the ReadColorInfoXml method throws the correct exception
        ///     when a null element name is used.
        /// </summary>
        [Test]
        public void TestXmlLegacy_Int_LegacyConditionalFormat2()
        {
            TypedValue tv = new TypedValue { Type = DatabaseType.Int32Type, Value = 100 };

            // XML Serialization of InlineRelationshipType is deprecated.
            // Changes to TypedValue require dev-manager approval
            const string xml =
@"
<root>
  <whatever>
    <type name=""Int32""/>
    <value>100</value>
  </whatever>
</root>
";
            CheckLegacyConditionalFormat<int>(tv, xml);
        }

        #endregion

        public static void CheckRoundTrip<T>(TypedValue expectedValue, string expectedXml) where T : IComparable
        {
            // Verify serialize
            string actualXml = TypedValueHelper.ToXml(expectedValue);
            string actualXml2 = RemoveNamespaces(actualXml);
            Assert.AreEqual(expectedXml, actualXml2, "Check XML");

            // Attempt deserialize
            TypedValue actualValue = TypedValueHelper.FromXml(actualXml);

            // Verify type
            Assert.AreEqual(expectedValue.Type.GetType(), actualValue.Type.GetType(), "Check type");
            
            // Verify data
            if (expectedValue.Value == null)
            {
                Assert.IsNull(actualValue.Value, "Check null data");
            }
            else
            {
                T expectedData = (T) expectedValue.Value;
                T actualData = (T) actualValue.Value;

                Assert.IsTrue(expectedData.CompareTo(actualData) == 0, "Check Data");
            }
            Assert.AreEqual(expectedValue.SourceEntityTypeId, actualValue.SourceEntityTypeId,
                            "Check SourceEntityTypeId");

            // Attempt alternate deserialize
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(actualXml);
            TypedValue actualValue2 = TypedValueHelper.FromXml(doc.DocumentElement);

            // Verify type2
            Assert.AreEqual(expectedValue.Type.GetType(), actualValue2.Type.GetType(), "Check type2");

            // Verify data2
            if (expectedValue.Value == null)
            {
                Assert.IsNull(actualValue2.Value, "Check null data2");
            }
            else
            {
                T expectedData = (T)expectedValue.Value;
                T actualData = (T)actualValue2.Value;

                Assert.IsTrue(expectedData.CompareTo(actualData) == 0, "Check Data2");
            }
            Assert.AreEqual(expectedValue.SourceEntityTypeId, actualValue2.SourceEntityTypeId,
                            "Check SourceEntityTypeId2");

        }

        public static void CheckLegacyLoad<T>(TypedValue expectedValue, string legacyXml) where T : IComparable
        {
            const string namespaces = @"xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://enterprisedata.com.au/readinow/v2/query/2.0""";
            string legacyXml2 = legacyXml.Replace("<TypedValue", "<TypedValue " + namespaces);
            TypedValue actualValue = TypedValueHelper.FromXml(legacyXml2);
            T expectedData = (T)expectedValue.Value;
            T actualData = (T)actualValue.Value;

            Assert.AreEqual(expectedValue.Type.GetType(), actualValue.Type.GetType(), "Check type");
            Assert.IsTrue(expectedData.CompareTo(actualData) == 0, "Check Data");
            Assert.AreEqual(expectedValue.SourceEntityTypeId, actualValue.SourceEntityTypeId, "Check SourceEntityTypeId");
        }

        public static void CheckLegacyConditionalFormat<T>(TypedValue expectedValue, string legacyXml) where T : IComparable
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(legacyXml);

            TypedValue actualValue = TypedValueHelper.ReadTypedValueXml(doc.DocumentElement, "whatever");

            T expectedData = (T)expectedValue.Value;
            T actualData = (T)actualValue.Value;

            Assert.AreEqual(expectedValue.Type.GetType(), actualValue.Type.GetType(), "Check type");
            Assert.IsTrue(expectedData.CompareTo(actualData) == 0, "Check Data");
            Assert.AreEqual(expectedValue.SourceEntityTypeId, actualValue.SourceEntityTypeId, "Check SourceEntityTypeId");
        }
    }
}