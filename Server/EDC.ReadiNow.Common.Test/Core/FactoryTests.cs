// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.IO;
using NUnit.Framework;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Security.AccessControl;
using QuerySqlBuilder = ReadiNow.QueryEngine.Builder.QuerySqlBuilder;
using CachingQuerySqlBuilder = ReadiNow.QueryEngine.CachingBuilder.CachingQuerySqlBuilder;
using ReadiNow.QueryEngine.ReportConverter;
using ReadiNow.Database;

namespace EDC.ReadiNow.Test.Core
{
    [TestFixture]
    public class FactoryTests
    {
        [Test]
        public void ReportToQueryConverter_ReturnsCache( )
        {
            Assert.That( Factory.ReportToQueryConverter, Is.TypeOf<CachingReportToQueryConverter>( ) );

            CachingReportToQueryConverter caching = ( CachingReportToQueryConverter ) Factory.ReportToQueryConverter;
            Assert.That( caching.Converter, Is.TypeOf<ReportToQueryConverter>( ) );
        }

        [Test]
        public void ReportToQueryConverter_SameInstanceForCache( )
        {
            Assert.That( Factory.ReportToQueryConverter, Is.SameAs(Factory.ReportToQueryConverter) );
        }

        [Test]
        public void NonCachedReportToQueryConverter_ReturnsNonCached( )
        {
            Assert.That( Factory.NonCachedReportToQueryConverter, Is.TypeOf<ReportToQueryConverter>( ) );
        }

        [Test]
        public void ReportToQueryConverter_SameInstanceForCacheViaResolve( )
        {
            IReportToQueryConverter converter = Factory.Current.Resolve<IReportToQueryConverter>();

            Assert.That( converter, Is.SameAs( Factory.ReportToQueryConverter ) );
        }

        [Test]
        public void QuerySqlBuilder_ReturnsCache()
        {
            Assert.That(Factory.QuerySqlBuilder, Is.TypeOf<CachingQuerySqlBuilder>());

            CachingQuerySqlBuilder caching = (CachingQuerySqlBuilder) Factory.QuerySqlBuilder;
            Assert.That(caching.QuerySqlBuilder, Is.TypeOf<QuerySqlBuilder>());
        }

        [Test]
        public void Test_QueryRepository()
        {
            Assert.That(Factory.QueryRepository, Is.TypeOf<RoleCheckingQueryRepository>());
        }

        [Test]
        public void QuerySqlBuilder_SameInstanceForCache( )
        {
            Assert.That( Factory.QuerySqlBuilder, Is.SameAs( Factory.QuerySqlBuilder ) );
        }

        [Test]
        public void NonCachedQuerySqlBuilder_ReturnsNonCached( )
        {
            Assert.That( Factory.NonCachedQuerySqlBuilder, Is.TypeOf<QuerySqlBuilder>( ) );
        }

        [Test]
        public void Test_EntityAccessControlService()
        {
            Assert.That(Factory.EntityAccessControlService, Is.TypeOf<EntityAccessControlService>());
        }

        [Test]
        public void Test_EntityAccessControlService_SameInstance()
        {
            Assert.That(Factory.EntityAccessControlService, Is.SameAs(Factory.EntityAccessControlService));
        }

        [Test]
        public void Test_DatabaseProvider()
        {
            Assert.That(Factory.DatabaseProvider, Is.TypeOf<DatabaseProvider>());
        }

        [Test]
        public void Test_BinaryFileRepository()
        {
            Assert.That(Factory.BinaryFileRepository, Is.TypeOf<FileRepository>());
        }

        [Test]
        public void Test_DocumentFileRepository()
        {
            Assert.That(Factory.DocumentFileRepository, Is.TypeOf<FileRepository>());
        }

        [Test]
        public void Test_TemporaryFileRepository()
        {
            Assert.That(Factory.TemporaryFileRepository, Is.TypeOf<FileRepository>());
        }

        [Test]
        public void Test_AppLibraryFileRepository()
        {
            Assert.That(Factory.AppLibraryFileRepository, Is.TypeOf<FileRepository>());
        }
    }
}