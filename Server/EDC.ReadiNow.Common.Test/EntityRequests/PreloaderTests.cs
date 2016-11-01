// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;

namespace EDC.ReadiNow.Test.EntityRequests
{
    [TestFixture]
    [RunWithTransaction]
    public class PreloaderTests
    {
        static long _reportId;
        
        [Test]
        [RunAsDefaultTenant]
        public void ReportPreloadTest( )
        {
            BulkPreloader.TenantWarmupIfNotWarm( );

            if (_reportId == 0)
                _reportId = EDC.ReadiNow.Expressions.CodeNameResolver.GetInstance("Student report", "Report").Id;

            var rq = new EntityRequest( _reportId, ReportHelpers.ReportPreloaderQuery, "ReportPreloadTest" );
            rq.IgnoreResultCache = true;
            BulkPreloader.Preload( rq );
        }
    }
}
