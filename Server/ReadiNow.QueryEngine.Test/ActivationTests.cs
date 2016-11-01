// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Core;
using ReadiNow.QueryEngine.Runner;
using ReadiNow.QueryEngine.CachingBuilder;
using ReadiNow.QueryEngine.CachingRunner;
using ReadiNow.QueryEngine.ReportConverter;
using EDC.ReadiNow.Model.CacheInvalidation;
using ReadiNow.Database;

namespace ReadiNow.QueryEngine.Test
{
    [TestFixture]
    public class ActivationTests
    {
        [Test]
        public void QuerySqlBuilder_Instance( )
        {
            IQuerySqlBuilder instance = Factory.NonCachedQuerySqlBuilder;
            Assert.That( instance, Is.Not.Null );
        }

        [Test]
        public void CachingQuerySqlBuilder_Instance( )
        {
            IQuerySqlBuilder instance = Factory.QuerySqlBuilder;
            Assert.That( instance, Is.Not.Null );
        }

        [Test]
        public void QueryRunner( )
        {
            IQueryRunner instance = Factory.QueryRunner;
            Assert.That( instance, Is.TypeOf<CachingQueryRunner>( ) );

            CachingQueryRunner cachingRunner = ( CachingQueryRunner ) instance;
            Assert.That( cachingRunner.QueryRunner, Is.TypeOf<QueryRunner>( ) );

            QueryRunner runner = ( QueryRunner ) cachingRunner.QueryRunner;
            Assert.That( runner.QuerySqlBuilder, Is.TypeOf<CachingQuerySqlBuilder>( ) );
            Assert.That( runner.DatabaseProvider, Is.TypeOf<DatabaseProvider>( ) );
        }

        [Test]
        public void ReportToQueryConverter( )
        {
            IReportToQueryConverter instance = Factory.ReportToQueryConverter;
            Assert.That( instance, Is.TypeOf<CachingReportToQueryConverter>( ) );

            CachingReportToQueryConverter cachingConverter = ( CachingReportToQueryConverter ) instance;
            Assert.That( cachingConverter.Converter, Is.TypeOf<ReportToQueryConverter>( ) );
        }

        [Test]
        public void ReportToQueryPartsConverter( )
        {
            IReportToQueryPartsConverter instance = Factory.Current.Resolve<IReportToQueryPartsConverter>( );
            Assert.That( instance, Is.TypeOf<ReportToQueryPartsConverter>( ) );
        }

        [Test]
        public void IQueryRunnerCacheKeyProvider( )
        {
            IQueryRunnerCacheKeyProvider instance = Factory.Current.Resolve<IQueryRunnerCacheKeyProvider>( );
            Assert.That( instance, Is.Not.Null );
        }


        [Test]
        public void Test_CacheInvalidators( )
        {
            CacheInvalidatorFactory factory = new CacheInvalidatorFactory( );

            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 1 ).TypeOf<CachingQuerySqlBuilderInvalidator>( ) );
            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 1 ).TypeOf<CachingQueryRunnerInvalidator>( ) );
        }
        
        
    }
}
