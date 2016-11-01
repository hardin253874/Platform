// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Model.CacheInvalidation;
using ReadiNow.QueryEngine.ReportConverter;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.ReportConverter
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class CachingReportToQueryConverterTests
    {
        [Test]
        public void Test_Ctor()
        {
            CachingReportToQueryConverter cachingReportToQueryConverter;
            Mock<IReportToQueryConverter> mockReportToQueryConverter;
            IReportToQueryConverter reportToQueryConverter;

            mockReportToQueryConverter = new Mock<IReportToQueryConverter>(MockBehavior.Strict);
            reportToQueryConverter = mockReportToQueryConverter.Object;

            cachingReportToQueryConverter = new CachingReportToQueryConverter(reportToQueryConverter);
            Assert.That(cachingReportToQueryConverter,
                Has.Property("Converter").SameAs(reportToQueryConverter));
            Assert.That(cachingReportToQueryConverter,
                Has.Property("Cache").Not.Null
                   .And.Property("Cache").Count.EqualTo(0));
            Assert.That(cachingReportToQueryConverter,
                Has.Property("CacheInvalidator").Not.Null);

            mockReportToQueryConverter.VerifyAll();
        }

        [Test]
        public void Test_Ctor_Null()
        {
            Assert.That(() => new CachingReportToQueryConverter(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("converter"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_Cached()
        {
            CachingReportToQueryConverter cachingReportToQueryConverter;
            Mock<IReportToQueryConverter> mockReportToQueryConverter;
            IReportToQueryConverter reportToQueryConverter;
            Report report;
            StructuredQuery structuredQuery;

            report = new Report();
            structuredQuery = new StructuredQuery();

            mockReportToQueryConverter = new Mock<IReportToQueryConverter>(MockBehavior.Strict);
            mockReportToQueryConverter.Setup(x => x.Convert(report, It.IsAny<ReportToQueryConverterSettings>()))
                                      .Returns(() => structuredQuery)
                                      .Verifiable();
            reportToQueryConverter = mockReportToQueryConverter.Object;
            cachingReportToQueryConverter = new CachingReportToQueryConverter(reportToQueryConverter);

            Assert.That(cachingReportToQueryConverter.Convert(report, null),
                Is.SameAs(structuredQuery), "Incorrect conversion");
            Assert.That(cachingReportToQueryConverter.Cache,
                Has.Count.EqualTo(1));
            Assert.That(cachingReportToQueryConverter.Cache,
                Has.Exactly(1).Property("Key").Property( "ReportId" ).EqualTo( report.Id ));
            Assert.That(cachingReportToQueryConverter.Cache,
                Has.Exactly(1).Property("Value").Property("StructuredQuery").EqualTo(structuredQuery));

            mockReportToQueryConverter.Verify(x => x.Convert(report, It.IsAny<ReportToQueryConverterSettings>()), Times.Exactly(1));
            mockReportToQueryConverter.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_ReturnCached()
        {
            CachingReportToQueryConverter cachingReportToQueryConverter;
            Mock<IReportToQueryConverter> mockReportToQueryConverter;
            IReportToQueryConverter reportToQueryConverter;
            Report report;
            StructuredQuery structuredQuery;
            CachingReportToQueryConverterKey key;

            report = new Report();
            structuredQuery = new StructuredQuery();

            mockReportToQueryConverter = new Mock<IReportToQueryConverter>(MockBehavior.Strict);
            reportToQueryConverter = mockReportToQueryConverter.Object;
            cachingReportToQueryConverter = new CachingReportToQueryConverter(reportToQueryConverter);

            key = new CachingReportToQueryConverterKey( report, ReportToQueryConverterSettings.Default );
            cachingReportToQueryConverter.Cache.Add( key, new CachingReportToQueryConverterValue( structuredQuery ) );

            Assert.That(cachingReportToQueryConverter.Convert(report),
                Is.SameAs(structuredQuery), "Incorrect conversion");

            mockReportToQueryConverter.Verify(x => x.Convert(report), Times.Never());
            mockReportToQueryConverter.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_EnsureCached()
        {
            CachingReportToQueryConverter cachingReportToQueryConverter;
            Mock<IReportToQueryConverter> mockReportToQueryConverter;
            IReportToQueryConverter reportToQueryConverter;
            Report report;
            StructuredQuery structuredQuery;

            report = new Report();
            structuredQuery = new StructuredQuery();

            mockReportToQueryConverter = new Mock<IReportToQueryConverter>(MockBehavior.Strict);
            mockReportToQueryConverter.Setup(x => x.Convert(report, It.IsAny<ReportToQueryConverterSettings>()))
                                      .Returns(() => structuredQuery)
                                      .Verifiable();
            reportToQueryConverter = mockReportToQueryConverter.Object;
            cachingReportToQueryConverter = new CachingReportToQueryConverter(reportToQueryConverter);

            cachingReportToQueryConverter.Convert(report, null);
            cachingReportToQueryConverter.Convert(report, null);

            mockReportToQueryConverter.Verify( x => x.Convert( report, It.IsAny<ReportToQueryConverterSettings>( ) ), Times.Exactly( 1 ) );
            mockReportToQueryConverter.VerifyAll();
        }

    }
}
