// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.EntityRequests
{
	[TestFixture]
	[RunWithTransaction]
	public class PreloaderTests
	{
		private static long _reportId;

		[Test]
		[RunAsDefaultTenant]
		public void ReportPreloadTest( )
		{
			if ( _reportId == 0 )
				_reportId = CodeNameResolver.GetInstance( "Student report", "Report" ).Id;

			var rq = new EntityRequest( _reportId, ReportHelpers.ReportPreloaderQuery, "ReportPreloadTest" );
			rq.IgnoreResultCache = true;
			BulkPreloader.Preload( rq );
		}
	}
}