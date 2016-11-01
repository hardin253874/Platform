// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Data.SqlClient;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Utc;
using NUnit.Framework;


namespace EDC.ReadiNow.Test.Utc
{
    /// <summary>
    /// Summary description for TimeZoneHelperTests
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class TimeZoneHelperTests
    {
        #region Olson to Ms mapping tests 
        
        /// <summary>
        /// Tests the name of the get valid fiji tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidFijiTzName()
        {
            const string olsonTzName = "Pacific/Fiji";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Fiji Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid auckland tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidAucklandTzName()
        {
            const string olsonTzName = "Pacific/Auckland";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid hobart tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidHobartTzName()
        {
            const string olsonTzName = "Australia/Hobart";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Tasmania Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the get valid Sydney microsoft tz name by olson tz name.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidSydneyTzName()
        {
            const string olsonTzName = "Australia/Sydney";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneHelper.SydneyTimeZoneName);
        }

        /// <summary>
        /// Tests the name of the get valid adelaide tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidAdelaideTzName()
        {
            const string olsonTzName = "Australia/Adelaide";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Cen. Australia Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid perth tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidPerthTzName()
        {
            const string olsonTzName = "Australia/Perth";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid singapore tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidSingaporeTzName()
        {
            const string olsonTzName = "Asia/Singapore";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid hong kong tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidHongKongTzName()
        {
            const string olsonTzName = "Asia/Hong_Kong";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time").StandardName);
        }
        
        /// <summary>
        /// Tests the name of the get valid moscow tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidMoscowTzName()
        {
            const string olsonTzName = "Europe/Moscow";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid dubai tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidDubaiTzName()
        {
            const string olsonTzName = "Asia/Dubai";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid nairobi tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidNairobiTzName()
        {
            const string olsonTzName = "Africa/Nairobi";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid baghdad tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidBaghdadTzName()
        {
            const string olsonTzName = "Asia/Baghdad";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Arabic Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid istanbul tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidIstanbulTzName()
        {
            const string olsonTzName = "Europe/Istanbul";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid harare tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidHarareTzName()
        {
            const string olsonTzName = "Africa/Harare";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid johannesburg tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidJohannesburgTzName()
        {
            const string olsonTzName = "Africa/Johannesburg";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid malabo tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidMalaboTzName()
        {
            const string olsonTzName = "Africa/Malabo";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid paris tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidParisTzName()
        {
            const string olsonTzName = "Europe/Paris";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid brussels tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidBrusselsTzName()
        {
            const string olsonTzName = "Europe/Brussels";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid prague tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidPragueTzName()
        {
            const string olsonTzName = "Europe/Prague";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid stockholm tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidStockholmTzName()
        {
            const string olsonTzName = "Europe/Stockholm";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid berlin tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidBerlinTzName()
        {
            const string olsonTzName = "Europe/Berlin";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid reykjavik tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidReykjavikTzName()
        {
            const string olsonTzName = "Atlantic/Reykjavik";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Greenwich Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid london tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidLondonTzName()
        {
            const string olsonTzName = "Europe/London";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid cayenne tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidCayenneTzName()
        {
            const string olsonTzName = "America/Cayenne";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("SA Eastern Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid santiago tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidSantiagoTzName()
        {
            const string olsonTzName = "America/Santiago";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid puerto rico tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidPuertoRicoTzName()
        {
            const string olsonTzName = "America/Puerto_Rico";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid barbados tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidBarbadosTzName()
        {
            const string olsonTzName = "America/Barbados";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid bermuda tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidBermudaTzName()
        {
            const string olsonTzName = "Atlantic/Bermuda";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid new york tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidNewYorkTzName()
        {
            const string olsonTzName = "America/New_York";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid jamaica tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidJamaicaTzName()
        {
            const string olsonTzName = "America/Jamaica";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid chicago tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidChicagoTzName()
        {
            const string olsonTzName = "America/Chicago";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid denver tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidDenverTzName()
        {
            const string olsonTzName = "America/Denver";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid phoenix tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidPhoenixTzName()
        {
            const string olsonTzName = "America/Phoenix";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid los angeles tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidLosAngelesTzName()
        {
            const string olsonTzName = "America/Los_Angeles";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid anchorage tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidAnchorageTzName()
        {
            const string olsonTzName = "America/Anchorage";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time").StandardName);
        }

        /// <summary>
        /// Tests the name of the get valid honolulu tz.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetValidHonoluluTzName()
        {
            const string olsonTzName = "Pacific/Honolulu";

            TimeZoneInfo msTz = TimeZoneHelper.GetTimeZoneInfo(olsonTzName);

            Assert.AreEqual(msTz.StandardName, TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time").StandardName);
        }
        #endregion

        #region Conversion tests for SQL funtions 

        /// <summary>
        /// Test converts to local time.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ConvertToLocalTime()
        {
            const string sql = @"select dbo.fnConvertToLocalTime(cast('2012-12-04 23:08:00' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    object res = cmd.ExecuteScalar();
                    var time = (DateTime)res;
                    Assert.AreEqual(new DateTime(2012, 12, 05, 10, 8, 0), time);
                }
            }
        }

        /// <summary>
        /// Test convert to UTC.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ConvertToUtc()
        {
            const string sql = @"select dbo.fnConvertToUtc(cast('2012-12-05 10:08:00' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    object res = cmd.ExecuteScalar();
                    var time = (DateTime)res;
                    Assert.AreEqual(new DateTime(2012, 12, 04, 23, 8, 0), time);
                }
            }
        }

        /// <summary>
        /// Converts to UTC lower bound invalid.
        /// this should throw : The datediff function resulted in an overflow. The number of dateparts separating two date/time instances is too large. Try to use datediff with a less precise datepart.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [ExpectedException(typeof(SqlException))]
        public void ConvertToUtcLowerBoundInvalid()
        {
            const string sql = @"select dbo.fnConvertToUtc(cast('1900-01-01 00:00:00' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Converts to UTC lower bound valid.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ConvertToUtcLowerBoundValid()
        {
            const string sql = @"select dbo.fnConvertToUtc(cast('1901-12-15 00:00:00' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Converts to UTC upper bound invalid.
        /// this should throw : The datediff function resulted in an overflow. The number of dateparts separating two date/time instances is too large. Try to use datediff with a less precise datepart.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [ExpectedException(typeof(SqlException))]
        public void ConvertToUtcUpperBoundInvalid()
        {
            const string sql = @"select dbo.fnConvertToUtc(cast('2040-01-01 00:00:00' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Converts to UTC upper bound valid.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ConvertToUtcUpperBoundValid()
        {
            const string sql = @"select dbo.fnConvertToUtc(cast('2038-01-19 03:14:07' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Converts to local lower bound invalid.
        /// this should throw : The datediff function resulted in an overflow. The number of dateparts separating two date/time instances is too large. Try to use datediff with a less precise datepart.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [ExpectedException(typeof(SqlException))]
        public void ConvertToLocalLowerBoundInvalid()
        {
            const string sql = @"select dbo.fnConvertToLocalTime(cast('1900-01-01 00:00:00' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Converts to UTC lower bound valid.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ConvertToLocalLowerBoundValid()
        {
            const string sql = @"select dbo.fnConvertToLocalTime(cast('1901-12-13 20:45:53' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Converts to local upper bound invalid.
        /// this should throw : The datediff function resulted in an overflow. The number of dateparts separating two date/time instances is too large. Try to use datediff with a less precise datepart.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [ExpectedException(typeof(SqlException))]
        public void ConvertToLocalUpperBoundInvalid()
        {
            const string sql = @"select dbo.fnConvertToLocalTime(cast('2040-01-01 00:00:00' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Converts to local upper bound valid.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ConvertToLocalUpperBoundValid()
        {
            const string sql = @"select dbo.fnConvertToLocalTime(cast('2038-01-18 16:14:07' as datetime), 'AUS Eastern Standard Time' )";

            using (DatabaseContext ctx = DatabaseContext.GetContext())
            {
                using (IDbCommand cmd = ctx.CreateCommand(sql))
                {
                    cmd.ExecuteScalar();
                }
            }
        }

        #endregion
    }
}
