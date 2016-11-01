// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.SQL.Test
{
	[TestFixture]
	public class TimeZoneHelperTests
	{
		[Test]
		[RunAsDefaultTenant]
		public void ConvertToLocalTime( )
		{
			// Check that all relationship instances are deployed to the tenant

            const string sql = @"select dbo.fnConvertToLocalTime(cast('2012-12-04 23:08:00' as datetime), 'AUS Eastern Standard Time' )";

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand cmd = ctx.CreateCommand( sql ) )
				{
					object res = cmd.ExecuteScalar( );
					var time = ( DateTime ) res;
					Assert.AreEqual( new DateTime( 2012, 12, 05, 10, 8, 0 ), time );
				}
			}
		}

        [Test]
        [RunAsDefaultTenant]
        public void ConvertToUtc()
        {
            // Check that all relationship instances are deployed to the tenant

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
	}
}