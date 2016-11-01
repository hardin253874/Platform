// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.SQL.Test
{
	[TestFixture]
	public class ClrTests
	{
		private void ReturnsOne( string sql )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand cmd = ctx.CreateCommand( sql ) )
				{
					object ores = cmd.ExecuteScalar( );
					var res = ( int ) ores;
					Assert.AreEqual( 1, res );
				}
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void AssemblyOk( )
		{
			const string sql = @"select 1 from sys.assemblies where name='SPSQL'";
			ReturnsOne( sql );
		}

		[Test]
		[RunAsDefaultTenant]
		public void ClrEnabledOk( )
		{
			const string sql = @"SELECT value FROM sys.configurations WHERE name = 'clr enabled'";
			ReturnsOne( sql );
			const string sql2 = @"SELECT value_in_use FROM sys.configurations WHERE name = 'clr enabled'";
			ReturnsOne( sql2 );
		}

		[Test]
		[RunAsDefaultTenant]
		public void DatabaseOk( )
		{
			const string sql = @"select 1";
			ReturnsOne( sql );
		}

		[Test]
		[RunAsDefaultTenant]
		public void SpsqlclrKeyOk( )
		{
			const string sql = @"select 1 from master.sys.asymmetric_keys where name = 'SPSQLCLRKey'";
			ReturnsOne( sql );
		}

		[Test]
		[RunAsDefaultTenant]
		public void SpsqlclrLoginOk( )
		{
			const string sql = @"select 1 from master..syslogins where name = 'SPSQLCLRLogin'";
			ReturnsOne( sql );
		}
	}
}