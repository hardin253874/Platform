// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Database;
using NUnit.Framework;

namespace EDC.Test.Database
{
    /// <summary>
    ///     This is a test class for the  DatabaseTypeHelper type.
    ///     Tests the converters on DatabaseTypeHelper itself, rather than on DatabaseType.
    /// </summary>
    [TestFixture]
    public class DatabaseTypeHelperTests2
    {


        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Binary value.
        /// </summary>
        [Test]
        public void ConvertFromString_Binary()
        {
            var originalData = new byte[]
				{
					1, 2, 3, 4, 0x8
				};
            string originalStringData = Convert.ToBase64String(originalData);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.BinaryType, originalStringData);
            var convertedData = (byte[])objectData;

            Assert.AreEqual(originalData.Length, convertedData.Length, "The converted data length is invalid.");
            for (int i = 0; i < originalData.Length; i++)
            {
                Assert.AreEqual(originalData[i], convertedData[i], "The converted data is invalid.");
            }
        }


        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Bool value.
        /// </summary>
        [Test]
        public void ConvertFromString_Bool()
        {
            const bool originalData = true;
            string originalStringData = originalData.ToString();

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.BoolType, originalStringData);
            var convertedData = (bool)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        ///// <summary>
        ///// Verifies that the ConvertFromString method correctly converts a Byte value.
        ///// </summary>
        //[Test]
        //public void ConvertFromString_Byte()
        //{
        //    byte originalData = 123;
        //    string originalStringData = originalData.ToString();

        //    object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.ByteType, originalStringData);
        //    byte convertedData = (byte)objectData;

        //    Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        //}


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Binary value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Binary()
        {
            var originalData = new byte[]
				{
					1, 2, 3, 4, 0x8
				};
            string originalStringData = Convert.ToBase64String(originalData);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.BinaryType, originalStringData);
            string convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.BinaryType, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Bool value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Bool()
        {
            const bool originalData = true;
            string originalStringData = originalData.ToString();

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.BoolType, originalStringData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.BoolType, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Currency value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Currency()
        {
            const decimal originalData = 74845739.345M;
            string originalStringData = originalData.ToString(CultureInfo.InvariantCulture);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.CurrencyType, originalStringData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.CurrencyType, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Date value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Date()
        {
            var originalData = new DateTime(2011, 4, 4);
            string originalStringData = originalData.ToString(DateType.DateFormatString);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.DateType, originalStringData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.DateType, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a DateTime value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_DateTime()
        {
            var originalData = new DateTime(2011, 4, 4, 4, 6, 7, DateTimeKind.Utc);
            string originalStringData = originalData.ToString(DateTimeType.DateTimeFormatString);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.DateTimeType, originalStringData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.DateTimeType, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Decimal value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Decimal()
        {
            const decimal originalData = 83673838.3335M;
            string originalStringData = originalData.ToString(CultureInfo.InvariantCulture);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.DecimalType, originalStringData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.DecimalType, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Guid value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Guid()
        {
            Guid originalData = Guid.NewGuid();
            string originalStringData = originalData.ToString("B");

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.GuidType, originalStringData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.GuidType, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Int32 value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Int32()
        {
            const int originalData = 12387352;
            string originalStringData = originalData.ToString(CultureInfo.InvariantCulture);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.Int32Type, originalStringData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.Int32Type, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_String_Empty()
        {
            const string originalData = "";

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.StringType, originalData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.StringType, objectData);

            Assert.AreEqual(originalData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_String_Null()
        {
            const string originalData = null;

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.StringType, originalData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.StringType, objectData);

            Assert.AreEqual(originalData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_String()
        {
            const string originalData = "string data";

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.StringType, originalData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.StringType, objectData);

            Assert.AreEqual(originalData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Time value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Time()
        {
            var originalData = new TimeSpan(13, 4, 13);
            string originalStringData = originalData.ToString("c");

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.TimeType, originalStringData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.TimeType, objectData);

            Assert.AreEqual(originalStringData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Xml value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_Xml()
        {
            const string originalData = "<xml></xml>";

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.XmlType, originalData);
            object convertedStringData = DatabaseTypeHelper.ConvertToString(DatabaseType.XmlType, objectData);

            Assert.AreEqual(originalData, convertedStringData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Currency value.
        /// </summary>
        [Test]
        public void ConvertFromString_Currency()
        {
            const decimal originalData = 74845739.345M;
            string originalStringData = originalData.ToString(CultureInfo.InvariantCulture);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.CurrencyType, originalStringData);
            var convertedData = (Decimal)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Date value.
        /// </summary>
        [Test]
        public void ConvertFromString_Date()
        {
            var originalData = new DateTime(2011, 4, 4);
            string originalStringData = originalData.ToString(DateType.DateFormatString);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.DateType, originalStringData);
            var convertedData = (DateTime)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a DateTime value.
        /// </summary>
        [Test]
        public void ConvertFromString_DateTime()
        {
            var originalData = new DateTime(2011, 4, 4, 4, 6, 7);
            string originalStringData = originalData.ToString(DateTimeType.DateTimeFormatString);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.DateTimeType, originalStringData);
            var convertedData = (DateTime)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a DateTime value.
        /// </summary>
        [Test]
        public void ConvertFromString_DateTime_NotCustomFormat()
        {
            var originalData = new DateTime(2011, 4, 4, 4, 6, 7);
            string originalStringData = originalData.ToUniversalTime().ToString("u");

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.DateTimeType, originalStringData);
            var convertedData = (DateTime)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Date value.
        /// </summary>
        [Test]
        public void ConvertFromString_Date_NotCustomFormat()
        {
            var originalData = new DateTime(2011, 4, 4);
            string originalStringData = originalData.ToString(CultureInfo.InvariantCulture);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.DateType, originalStringData);
            var convertedData = (DateTime)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Decimal value.
        /// </summary>
        [Test]
        public void ConvertFromString_Decimal()
        {
            const decimal originalData = 83673838.3335M;
            string originalStringData = originalData.ToString(CultureInfo.InvariantCulture);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.DecimalType, originalStringData);
            var convertedData = (Decimal)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Guid value.
        /// </summary>
        [Test]
        public void ConvertFromString_Guid()
        {
            Guid originalData = Guid.NewGuid();
            string originalStringData = originalData.ToString();

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.GuidType, originalStringData);
            var convertedData = (Guid)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Int32 value.
        /// </summary>
        [Test]
        public void ConvertFromString_Int32()
        {
            const int originalData = 12387352;
            string originalStringData = originalData.ToString(CultureInfo.InvariantCulture);

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.Int32Type, originalStringData);
            var convertedData = (Int32)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a null value.
        /// </summary>
        [Test]
        public void ConvertFromString_Null()
        {
            object obj = DatabaseTypeHelper.ConvertFromString(DatabaseType.XmlType, null);
            Assert.IsNull(obj, "Failed to convert null value");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_String()
        {
            const string originalData = "string data";

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.StringType, originalData);
            var convertedData = (string)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_String_Empty()
        {
            const string originalData = "";

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.StringType, originalData);
            var convertedData = (string)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_String_Null()
        {
            const string originalData = null;

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.StringType, originalData);
            var convertedData = (string)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Time value.
        /// </summary>
        [Test]
        public void ConvertFromString_Time()
        {
            var originalData = new TimeSpan(13, 4, 13);
            string originalStringData = originalData.ToString("c");

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.TimeType, originalStringData);
            var convertedData = (DateTime)objectData;       // TODO: DateTime here, but TimeSpan elsewhere!!

            Assert.AreEqual(originalData, convertedData.TimeOfDay, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Time value.
        /// </summary>
        [Test]
        public void ConvertFromString_TimeLegacy()
        {
            var originalData = new TimeSpan(13, 4, 13);
            string originalStringData = "05/02/2013 13:04:13";

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.TimeType, originalStringData);
            var convertedData = (DateTime)objectData;       // TODO: DateTime here, but TimeSpan elsewhere!!

            Assert.AreEqual(originalData, convertedData.TimeOfDay, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a Xml value.
        /// </summary>
        [Test]
        public void ConvertFromString_Xml()
        {
            const string originalData = "<xml></xml>";

            object objectData = DatabaseTypeHelper.ConvertFromString(DatabaseType.XmlType, originalData);
            var convertedData = (string)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Binary value.
        /// </summary>
        [Test]
        public void ConvertToString_Binary()
        {
            var originalData = new byte[]
				{
					1, 2, 3, 4, 5, 0xF
				};
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.BinaryType, originalData);
            byte[] convertedData = Convert.FromBase64String(stringData);

            Assert.AreEqual(originalData.Length, convertedData.Length, "The length of the converted data is invalid.");
            for (int i = 0; i < originalData.Length; i++)
            {
                Assert.AreEqual(originalData[i], convertedData[i], "The converted data is invalid.");
            }
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Bool value.
        /// </summary>
        [Test]
        public void ConvertToString_Bool()
        {
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.BoolType, true);
            bool convertedData = bool.Parse(stringData);

            Assert.AreEqual(true, convertedData, "The converted data is invalid.");

            stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.BoolType, false);
            convertedData = bool.Parse(stringData);

            Assert.AreEqual(false, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Currency value.
        /// </summary>
        [Test]
        public void ConvertToString_Currency()
        {
            const decimal originalData = 8726387463.4562M;
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.CurrencyType, originalData);
            Decimal convertedData = Decimal.Parse(stringData);

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Date value.
        /// </summary>
        [Test]
        public void ConvertToString_Date()
        {
            var originalData = new DateTime(2011, 5, 23);
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.DateType, originalData);
            DateTime convertedData = DateTime.Parse(stringData);

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a DateTime value.
        /// </summary>
        [Test]
        public void ConvertToString_DateTime()
        {
            var originalData = new DateTime(2011, 5, 23, 5, 4, 3);
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.DateTimeType, originalData);
            DateTime convertedData = DateTime.Parse(stringData);

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a DbNull value.
        /// </summary>
        [Test]
        public void ConvertToString_DbNull()
        {
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.BinaryType, DBNull.Value);
            Assert.AreEqual(null, stringData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Decimal value.
        /// </summary>
        [Test]
        public void ConvertToString_Decimal()
        {
            const decimal originalData = 8722387463.4562M;
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.DecimalType, originalData);
            Decimal convertedData = Decimal.Parse(stringData);

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Guid value.
        /// </summary>
        [Test]
        public void ConvertToString_Guid()
        {
            Guid originalData = Guid.NewGuid();
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.GuidType, originalData);
            var convertedData = new Guid(stringData);

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Int32 value.
        /// </summary>
        [Test]
        public void ConvertToString_Int32()
        {
            const int originalData = 476685;
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.Int32Type, originalData);
            Int32 convertedData = Int32.Parse(stringData);

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

        /// <summary>
        ///     Verifies that the ConvertToString method throws the expected expection
        ///     when invalid databasetypes are specified.
        /// </summary>
        [Test]
        public void ConvertToString_InvalidDatabaseType()
        {
            var supportedTypes = new List<Type>
				{
					typeof( AutoIncrementType ),
					typeof ( BinaryType ),
					typeof ( BoolType ),
					typeof ( CurrencyType ),
					typeof ( DateType ),
					typeof ( DateTimeType ),
					typeof ( DecimalType ),
					typeof ( GuidType ),
					typeof ( Int32Type ),
					typeof ( StructureLevelsType ),
					typeof ( ChoiceRelationshipType ),
					typeof ( InlineRelationshipType ),
					typeof ( StringType ),
					typeof ( TimeType ),
					typeof ( XmlType ),
					typeof ( IdentifierType )
				};

            IEnumerable<Type> list = typeof(DatabaseType).Assembly.GetTypes().Where(a => typeof(DatabaseType).IsAssignableFrom(a) && !a.IsAbstract);

            //Array values = list.ToArray();
            foreach (Type databaseType in list)
            {
                if (!supportedTypes.Contains(databaseType))
                {
                    bool exceptionThrown = false;

                    try
                    {
                        var dbType = Activator.CreateInstance(databaseType) as DatabaseType;
                        if (dbType != null)
                        {
                            dbType.ConvertToString(" ");
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionThrown = true;
                    }

                    Assert.IsTrue(exceptionThrown, "The expected exception for DatabaseType {0} was not thrown", databaseType);
                }
            }
        }

        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a null value.
        /// </summary>
        [Test]
        public void ConvertToString_Null()
        {
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.BinaryType, null);
            Assert.AreEqual(null, stringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertToString_String()
        {
            const string originalData = "original data";
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.StringType, originalData);
            string convertedData = stringData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertToString_String_Empty()
        {
            const string originalData = "";
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.StringType, originalData);
            string convertedData = stringData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertToString_String_Null()
        {
            const string originalData = null;
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.StringType, originalData);
            string convertedData = stringData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Time value.
        /// </summary>
        [Test]
        public void ConvertToString_Time()
        {
            var originalData = new TimeSpan(10, 34, 45);
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.TimeType, originalData);
            TimeSpan convertedData = TimeSpan.Parse(stringData);

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a Xml value.
        /// </summary>
        [Test]
        public void ConvertToString_Xml()
        {
            const string originalData = "<originalXml></originalXml>";
            string stringData = DatabaseTypeHelper.ConvertToString(DatabaseType.XmlType, originalData);
            string convertedData = stringData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

    }
}