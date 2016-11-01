// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using System.Globalization;
using ReadiNow.Connector.Payload;

namespace ReadiNow.Connector.Test.Payload
{
    /// <summary>
    /// Test the integration of the JilDynamicObjectReader with the ReaderToEntityAdapterProvider and ReaderToEntityAdapter.
    /// </summary>
    [TestFixture]
    class JsonValueParserTests
    {
        [TestCase("2012-12-31", "2012-12-31T00:00:00Z")]
        [TestCase("1900-01-01", "1900-01-01T00:00:00Z")]
        [TestCase("2100-12-31", "2100-12-31T00:00:00Z")]
        [RunAsDefaultTenant]
        public void Test_TryParseDate_Valid(string jsonData, string expectedData)
        {
            DateTime value;
            DateTime expected = DateTime.Parse(expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            bool res = JsonValueParser.TryParseDate(jsonData, out value);
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expected));
        }

        [TestCase("abc")]
        [TestCase("123")]
        [TestCase("123")]
        [TestCase("true")]
        [TestCase("false")]
        [TestCase("06/06/2012")]
        [TestCase("2012-12-31Z")]
        [TestCase("2012-12-31T23:59:59")]
        [TestCase("2012-12-31T23:59:59.0000000Z")]
        public void Test_TryParseDate_InvalidFormat(string jsonData)
        {
            DateTime value;
            bool res = JsonValueParser.TryParseDate(jsonData, out value);
            Assert.That(res, Is.False);
        }

        [TestCase("2012-12-31", "2012-12-31T00:00:00Z")]            // TODO: Consider timezones further
        [TestCase("1900-01-01", "1900-01-01T00:00:00Z")]            // TODO: Consider timezones further
        [TestCase("2100-12-31", "2100-12-31T00:00:00Z")]            // TODO: Consider timezones further
        [TestCase("2012-12-31T23:59", "2012-12-31T23:59:00Z")]      // TODO: Consider timezones further
        [TestCase("2012-12-31T23:59:59", "2012-12-31T23:59:59Z")]   // TODO: Consider timezones further
        [TestCase("2012-12-31T23:59Z", "2012-12-31T23:59:00Z")]
        [TestCase("2012-12-31T23:59:59Z", "2012-12-31T23:59:59Z")]
        public void Test_TryParseDateTime_Valid(string jsonData, string expectedData)
        {
            DateTime value;
            DateTime expected = DateTime.Parse(expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            bool res = JsonValueParser.TryParseDateTime(jsonData, out value);
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expected));
        }

        [TestCase("abc")]
        [TestCase("123")]
        [TestCase("123")]
        [TestCase("true")]
        [TestCase("false")]
        [TestCase("06/60/2012")]
        [TestCase("2012-12-31 23:59:59")]
        [TestCase("2012-12-31 07:00 am")]
        public void Test_TryParseDateTime_InvalidFormat(string jsonData)
        {
            DateTime value;
            bool res = JsonValueParser.TryParseDateTime(jsonData, out value);
            Assert.That(res, Is.False);
        }

        [TestCase("00:00", "1753-01-01T00:00:00Z")]
        [TestCase("00:00:00", "1753-01-01T00:00:00Z")]
        [TestCase("23:59", "1753-01-01T23:59:00Z")]
        [TestCase("23:59:59", "1753-01-01T23:59:59Z")]
        public void Test_TryParseTime_Valid(string jsonData, string expectedData)
        {
            DateTime value;
            DateTime expected = DateTime.Parse(expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            bool res = JsonValueParser.TryParseTime(jsonData, out value);
            Assert.That(res, Is.True);
            Assert.That(value, Is.EqualTo(expected));
        }

        [TestCase("abc")]
        [TestCase("123")]
        [TestCase("123")]
        [TestCase("true")]
        [TestCase("false")]
        [TestCase("7:00")]
        [TestCase("07:00pm")]
        [TestCase("07:00:00 pm")]
        [TestCase("2012-12-31Z")]
        [TestCase("2012-12-31T23:59:59")]
        [TestCase("2012-12-31T23:59:59.0000000Z")]
        public void Test_TryParseTime_InvalidFormat(string jsonData)
        {
            DateTime value;
            bool res = JsonValueParser.TryParseTime(jsonData, out value);
            Assert.That(res, Is.False);
        }

    }
}
