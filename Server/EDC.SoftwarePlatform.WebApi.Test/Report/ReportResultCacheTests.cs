// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using ReadiNow.Reporting;
using ReadiNow.Reporting.Request;

namespace EDC.SoftwarePlatform.WebApi.Test.Report
{
	[TestFixture]
	public class ReportResultCacheTests
	{
		[Test]
		[RunAsDefaultTenant]
		public void TestSerializationOfReportResultCacheKey( )
		{
			/////
			// Get the AA_All Fields Report since it has rollups.
			/////
			long reportId = Entity.GetIdFromUpgradeId( new Guid( "da986865-1c90-4ae4-9a48-36cd4514208c" ) );

			var reportingInterface = new ReportingInterface( );

			ReportCompletionData reportCompletion = reportingInterface.PrepareReport( reportId, new ReportSettings( ) );

			ReportResultCacheKey reportResultCacheKey = reportCompletion.ResultCacheKey;

			using ( var stream = new MemoryStream( ) )
			{
				ProtoBuf.Serializer.Serialize( stream, reportResultCacheKey );

				byte [ ] bytes = stream.ToArray( );

				using ( var stream2 = new MemoryStream( bytes ) )
				{
					var key2 = ProtoBuf.Serializer.Deserialize<ReportResultCacheKey>( stream2 );

					Assert.AreEqual( key2, reportResultCacheKey );
				}
			}
		}
	}
}
