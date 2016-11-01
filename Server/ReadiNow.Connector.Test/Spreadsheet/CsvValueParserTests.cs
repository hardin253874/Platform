// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using NUnit.Framework;
using System.Globalization;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    /// Test the integration of the JilDynamicObjectReader with the ReaderToEntityAdapterProvider and ReaderToEntityAdapter.
    /// </summary>
    [TestFixture]
    class CsvValueParserTests
    {
        [TestCase("y", true)]
        [TestCase("yes", true)]
        [TestCase("true", true)]
        [TestCase("Y", true)]
        [TestCase("YES", true)]
        [TestCase("TRUE", true)]
        [TestCase("Yes", true)]
        [TestCase("True", true)]
        [TestCase("t", true)]
        [TestCase("T", true)]
        [TestCase("1", true)]
        [TestCase("n", false)]
        [TestCase("no", false)]
        [TestCase("false", false)]
        [TestCase("N", false)]
        [TestCase("NO", false)]
        [TestCase("FALSE", false)]
        [TestCase("No", false)]
        [TestCase("False", false)]
        [TestCase("f", false)]
        [TestCase("F", false)]
        [TestCase("0", false)]
        public void Test_TryParseBool_Valid(string jsonData, bool expectedData)
        {
            bool value;
            bool res = CsvValueParser.TryParseBool(jsonData, out value);
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expectedData));
        }

        [TestCase("abc")]
        [TestCase("123")]
        [TestCase("123")]
        [TestCase("")]
        [TestCase(" true ")]
        [TestCase(" false")]
        [TestCase("false ")]
        public void Test_TryParseBool_InvalidFormat(string jsonData)
        {
            bool value;
            bool res = CsvValueParser.TryParseBool(jsonData, out value);
            Assert.That(res, Is.False);
        }

        [TestCase("1", 1)]
        [TestCase("-1", -1)]
        [TestCase("1000", 1000)]
        [TestCase("-1000", -1000)]
        [TestCase("1,000", 1000)]
        [TestCase("1000.0", 1000)]
        [TestCase(" 1000", 1000)]
        [TestCase("1000 ", 1000)]
        [TestCase("01000", 1000)]
        [TestCase(" -1,000 ", -1000)]
        [TestCase( "0.4", 0 )]
        [TestCase( "0.5", 1 )]
        [TestCase( "0.6", 1 )]
        [TestCase( "1.4", 1 )]
        [TestCase( "1.5", 2 )]
        [TestCase( "-0.4", 0 )]
        [TestCase( "-0.5", -1 )]
        [TestCase( "-0.6", -1 )]
        [TestCase( "-1.4", -1 )]
        [TestCase( "-1.5", -2 )]
        public void Test_TryParseInt_Valid(string jsonData, int expectedData)
        {
            int value;
            bool res = CsvValueParser.TryParseInt(jsonData, out value);
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expectedData));
        }

        [TestCase("abc")]
        [TestCase("")]
        [TestCase("1e2")]
        [TestCase("0x1123")]
        public void Test_TryParseInt_InvalidFormat(string jsonData)
        {
            int value;
            bool res = CsvValueParser.TryParseInt(jsonData, out value);
            Assert.That(res, Is.False);
        }

        [TestCase("1", 1.0)]
        [TestCase("-1", -1.0)]
        [TestCase("$1000", 1000.0)]
        [TestCase("1000", 1000.0)]
        [TestCase("-1000", -1000.0)]
        [TestCase("1,000", 1000.0)]
        [TestCase("01000", 1000.0)]
        [TestCase(" 1000", 1000.0)]
        [TestCase("1000 ", 1000.0)]
        [TestCase("1000.1234", 1000.1234)]
        [TestCase(" -1,000.1234 ", -1000.1234)]
        public void Test_TryParseDecimal_Valid(string jsonData, double expectedData)
        {
            decimal expected = (decimal) expectedData;
            decimal value;
            bool res = CsvValueParser.TryParseDecimal(jsonData, out value);
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expected));
        }

        [TestCase("abc")]
        [TestCase("")]
        [TestCase("1e2")]
        [TestCase("0x1123")]
        public void Test_TryParseDecimal_InvalidFormat(string jsonData)
        {
            decimal value;
            bool res = CsvValueParser.TryParseDecimal(jsonData, out value);
            Assert.That(res, Is.False);
        }

        [TestCase("2012-12-31", "2012-12-31T00:00:00Z")]
        [TestCase("1900-01-01", "1900-01-01T00:00:00Z")]
        [TestCase("2100-12-31", "2100-12-31T00:00:00Z")]
        [TestCase("2100-12-31", "2100-12-31T00:00:00Z")]
        [TestCase("3/10/2013", "2013-10-03T00:00:00Z")]
        [TestCase("03/10/2013", "2013-10-03T00:00:00Z")]
        [TestCase(" 2012-12-31 ", "2012-12-31T00:00:00Z")]
        [TestCase("2012-12-31Z", "2012-12-31T00:00:00Z")]
        [TestCase("2012-12-31T23:59:59", "2012-12-31T00:00:00Z")]
        [TestCase("2012-12-31T23:59:59.0000000Z", "2012-12-31T00:00:00Z")]
        public void Test_TryParseDate_Valid(string jsonData, string expectedData)
        {
            DateTime value;
            DateTime expected = DateTime.Parse(expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            bool res = CsvValueParser.TryParseDate(jsonData, out value);
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(value.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [TestCase("abc")]
        [TestCase("123")]
        [TestCase("123")]
        [TestCase("true")]
        [TestCase("false")]
        public void Test_TryParseDate_InvalidFormat(string jsonData)
        {
            DateTime value;
            bool res = CsvValueParser.TryParseDate(jsonData, out value);
            Assert.That(res, Is.False);
        }

        [TestCase( false, "2012-12-31", "2012-12-31T00:00:00Z" )]
        [TestCase( false, "1900-01-01", "1900-01-01T00:00:00Z" )]
        [TestCase( false, "2100-12-31", "2100-12-31T00:00:00Z" )]
        [TestCase( false, "2012-12-31T23:59", "2012-12-31T23:59:00Z" )]
        [TestCase( false, "2012-12-31T23:59:59", "2012-12-31T23:59:59Z" )]
        [TestCase( false, "2012-12-31T23:59Z", "2012-12-31T23:59:00Z" )]
        [TestCase( false, "2012-12-31T23:59:59Z", "2012-12-31T23:59:59Z" )]
        [TestCase( false, "2012-12-31T12:15+01:30", "2012-12-31T10:45:00Z" )]
        [TestCase( false, "2012-12-31T12:15:00+01:30", "2012-12-31T10:45:00Z" )]
        [TestCase( false, "2012-12-31T12:15:00-01:30", "2012-12-31T13:45:00Z" )]
        [TestCase( false, "3/10/2013 15:00", "2013-10-03T15:00:00Z" )]
        [TestCase( false, "03/10/2013 15:00", "2013-10-03T15:00:00Z" )]
        [TestCase( false, "2012-12-31 23:59:59", "2012-12-31T23:59:59Z" )]
        [TestCase( false, "2012-12-31 07:00 am", "2012-12-31T07:00:00Z" )]
        [TestCase( false, " 2012-12-31T23:59 ", "2012-12-31T23:59:00Z" )]
        [TestCase( true, "2012-12-31", "2012-12-30T23:00:00Z")]
        [TestCase( true, "1900-01-01", "1899-12-31T23:00:00Z")]
        [TestCase( true, "2100-12-31", "2100-12-30T23:00:00Z")]
        [TestCase( true, "2012-12-31T23:59", "2012-12-31T22:59:00Z")]
        [TestCase( true, "2012-12-31T23:59:59", "2012-12-31T22:59:59Z")]
        [TestCase( true, "2012-12-31T23:59Z", "2012-12-31T23:59:00Z")]
        [TestCase( true, "2012-12-31T23:59:59Z", "2012-12-31T23:59:59Z")]
        [TestCase( true, "2012-12-31T12:15+01:30", "2012-12-31T10:45:00Z")]
        [TestCase( true, "2012-12-31T12:15:00+01:30", "2012-12-31T10:45:00Z")]
        [TestCase( true, "2012-12-31T12:15:00-01:30", "2012-12-31T13:45:00Z")]
        [TestCase( true, "3/10/2013 15:00", "2013-10-03T14:00:00Z")]
        [TestCase( true, "03/10/2013 15:00", "2013-10-03T14:00:00Z")]
        [TestCase( true, "2012-12-31 23:59:59", "2012-12-31T22:59:59Z")]
        [TestCase( true, "2012-12-31 07:00 am", "2012-12-31T06:00:00Z")]
        [TestCase( true, " 2012-12-31T23:59 ", "2012-12-31T22:59:00Z")]
        public void Test_TryParseDateTime_Valid( bool provideTimeZone, string csvData, string expectedData)
        {
            // If csvData contains a timezone, then that timezone is used
            // Otherwise, if provideTimeZone=true, then the test timezone of +1 is used
            // Otherwise, if provideTimeZone=false, then the value is assumed to be UTC already

            TimeZoneInfo tz = null;
            if ( provideTimeZone )
            {
                string displayName = "(GMT+01:00) ReadiNow/PlusOne";
                string standardName = "Hammer Time";
                TimeSpan offset = new TimeSpan( 1, 00, 00 );
                tz = TimeZoneInfo.CreateCustomTimeZone( standardName, offset, displayName, standardName );
            }

            DateTime value;
            DateTime expected = DateTime.Parse(expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            bool res = CsvValueParser.TryParseDateTime( csvData, tz, out value );
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(value.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [TestCase("abc")]
        [TestCase("123")]
        [TestCase("123")]
        [TestCase("true")]
        [TestCase("false")]
        [TestCase("06/60/2012")]
        public void Test_TryParseDateTime_InvalidFormat(string jsonData)
        {
            DateTime value;
            TimeZoneInfo info = TimeZoneInfo.Local;
            bool res = CsvValueParser.TryParseDateTime(jsonData, info, out value);
            Assert.That(res, Is.False);
        }

        [TestCase("00:00", "1753-01-01T00:00:00Z")]
        [TestCase("00:00:00", "1753-01-01T00:00:00Z")]
        [TestCase("7:00", "1753-01-01T07:00:00Z")]
        [TestCase("23:59", "1753-01-01T23:59:00Z")]
        [TestCase("23:59:59", "1753-01-01T23:59:59Z")]
        [TestCase("3:00am", "1753-01-01T03:00:00Z")]
        [TestCase("3:00PM", "1753-01-01T15:00:00Z")]
        [TestCase("3:00 PM", "1753-01-01T15:00:00Z")]
        [TestCase("3:00 pm", "1753-01-01T15:00:00Z")]
        [TestCase("3:00:00 PM", "1753-01-01T15:00:00Z")]
        [TestCase(" 23:59:59 ", "1753-01-01T23:59:59Z")]
        [TestCase("2012-12-31 23:59:59", "1753-01-01T23:59:59Z")]
        public void Test_TryParseTime_Valid(string jsonData, string expectedData)
        {
            DateTime value;
            DateTime expected = DateTime.Parse(expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            bool res = CsvValueParser.TryParseTime(jsonData, out value);
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(value.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [TestCase("abc")]
        [TestCase("123")]
        [TestCase("123")]
        [TestCase("true")]
        [TestCase("false")]
        public void Test_TryParseTime_InvalidFormat(string jsonData)
        {
            DateTime value;
            bool res = CsvValueParser.TryParseTime(jsonData, out value);
            Assert.That(res, Is.False);
        }

    }
}
