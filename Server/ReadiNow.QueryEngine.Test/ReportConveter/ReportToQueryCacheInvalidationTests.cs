// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using EDC.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security.AccessControl;
using ReadiNow.QueryEngine.ReportConverter;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;

namespace ReadiNow.QueryEngine.Test.ReportConverter
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class ReportToQueryCacheInvalidationTests
    {
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(InvalidationCause.Delete)]
        [TestCase(InvalidationCause.Save)]
        public void BasicTest(InvalidationCause invalidationCause)
        {
            CachingReportToQueryConverter cachingReportToQueryConverter;
            Report report;
            StructuredQuery structuredQuery;
            ItemsRemovedEventHandler<CachingReportToQueryConverterKey> itemsRemovedEventHandler;
            List<long> itemsRemoved;

            using (var ctx = EDC.ReadiNow.Database.DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                report = TestQueries.Entities().ToReport();
                report.Save();
                ctx.CommitTransaction();
            }

            cachingReportToQueryConverter = Factory.Current.Resolve<CachingReportToQueryConverter>();
            Assert.That(cachingReportToQueryConverter, Is.Not.Null,
                "Not report to query converter cache found");
            Assert.That( cachingReportToQueryConverter, Is.Not.Null,
                "Not report to query converter cache found" );

            itemsRemoved = new List<long>();
            itemsRemovedEventHandler = (sender, args) => itemsRemoved.AddRange(args.Items.Select(key => key.ReportId));

            structuredQuery = cachingReportToQueryConverter.Convert(report, null);
            try
            {

                cachingReportToQueryConverter.Cache.ItemsRemoved += itemsRemovedEventHandler;
                Assert.That( cachingReportToQueryConverter.Cache,
                    Has.Exactly( 1 ).Property( "Key" ).Property( "ReportId" ).EqualTo( report.Id ) );
                Assert.That( cachingReportToQueryConverter.Cache,
                    Has.Exactly( 1 ).Property( "Value" ).Property( "StructuredQuery" ).SameAs( structuredQuery ) );

                using (var ctx = EDC.ReadiNow.Database.DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    if (invalidationCause == InvalidationCause.Save)
                    {
                        report.Save();
                    }
                    else if (invalidationCause == InvalidationCause.Delete)
                    {
                        report.Delete();
                    }
                    else
                    {
                        Assert.Fail("Unknown invalidation cause");
                    }
                    ctx.CommitTransaction();
                }

                Assert.That(cachingReportToQueryConverter.Cache,
                    Has.Exactly(0).Property("Key").EqualTo(report.Id));
                Assert.That(itemsRemoved,
                    Has.Exactly(1).EqualTo(report.Id));
            }
            finally
            {
                cachingReportToQueryConverter.Cache.ItemsRemoved -= itemsRemovedEventHandler;
            }
        }
    }
}
