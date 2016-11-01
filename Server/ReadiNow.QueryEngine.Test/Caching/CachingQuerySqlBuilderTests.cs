// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using Moq;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using CachingQuerySqlBuilder = ReadiNow.QueryEngine.CachingBuilder.CachingQuerySqlBuilder;
using EDC.ReadiNow.Test;
using ReadiNow.QueryEngine.Builder;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Security;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Security.AccessControl;
using EntityRef = EDC.ReadiNow.Model.EntityRef;
using System.Collections.Generic;

namespace ReadiNow.QueryEngine.Test.Caching
{
    [TestFixture]
    public class CachingQuerySqlBuilderTests
    {
        [Test]
        public void Test_Ctor()
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>(MockBehavior.Strict);
            querySqlBuilder = mockQuerySqlBuilder.Object;

            cachingQuerySqlBuilder = new CachingQuerySqlBuilder( querySqlBuilder, userRuleSetProvider );
            Assert.That(cachingQuerySqlBuilder,
                Has.Property("QuerySqlBuilder").SameAs(querySqlBuilder));
            Assert.That(cachingQuerySqlBuilder,
                Has.Property("Cache").Not.Null
                   .And.Property("Cache").Count.EqualTo(0));
            Assert.That(cachingQuerySqlBuilder.CacheInvalidator, Is.Not.Null);

            mockQuerySqlBuilder.VerifyAll();
        }

        [Test]
        public void Test_Ctor_Null_QuerySqlBuilder( )
        {
            IUserRuleSetProvider userRuleSetProvider;

            userRuleSetProvider = MockUserRuleSetProvider( );
            Assert.That( ( ) => new CachingQuerySqlBuilder( null, userRuleSetProvider ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "querySqlBuilder" ) );
        }

        [Test]
        public void Test_Ctor_Null_UserRuleSetProvider( )
        {
            IQuerySqlBuilder querySqlBuilder = new Mock<IQuerySqlBuilder>( ).Object;

            Assert.That( ( ) => new CachingQuerySqlBuilder( querySqlBuilder, null ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("userRuleSetProvider"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_Cached()
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            StructuredQuery structuredQuery;
            QuerySqlBuilderSettings settings;
            QueryBuild queryBuild;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            structuredQuery = new StructuredQuery( );
            settings = new QuerySqlBuilderSettings( );
            queryBuild = new QueryBuild( );

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>(MockBehavior.Strict);
            mockQuerySqlBuilder.Setup( x => x.BuildSql( structuredQuery, settings ) )
                                      .Returns( ( ) => queryBuild )
                                      .Verifiable();
            querySqlBuilder = mockQuerySqlBuilder.Object;
            cachingQuerySqlBuilder = new CachingQuerySqlBuilder(querySqlBuilder, userRuleSetProvider);

            Assert.That( cachingQuerySqlBuilder.BuildSql( structuredQuery, settings ),
                Is.SameAs( queryBuild ), "Incorrect conversion" );
            Assert.That(cachingQuerySqlBuilder.Cache,
                Has.Count.EqualTo(1));

            mockQuerySqlBuilder.Verify( x => x.BuildSql( structuredQuery, settings ), Times.Exactly( 1 ) );
            mockQuerySqlBuilder.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_EnsureCached()
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            StructuredQuery structuredQuery;
            QuerySqlBuilderSettings settings;
            QueryBuild queryBuild;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            structuredQuery = new StructuredQuery( );
            settings = new QuerySqlBuilderSettings( );
            queryBuild = new QueryBuild( );

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>(MockBehavior.Strict);
            mockQuerySqlBuilder.Setup( x => x.BuildSql( structuredQuery, settings ) )
                                      .Returns( ( ) => queryBuild )
                                      .Verifiable();
            querySqlBuilder = mockQuerySqlBuilder.Object;
            cachingQuerySqlBuilder = new CachingQuerySqlBuilder(querySqlBuilder, userRuleSetProvider);

            cachingQuerySqlBuilder.BuildSql( structuredQuery, settings );
            cachingQuerySqlBuilder.BuildSql( structuredQuery, settings );

            mockQuerySqlBuilder.Verify( x => x.BuildSql( structuredQuery, settings ), Times.Exactly( 1 ) );
            mockQuerySqlBuilder.VerifyAll();
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_EnsureCached_TwoThreads( )
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            StructuredQuery structuredQuery;
            QuerySqlBuilderSettings settings;
            QueryBuild queryBuild;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            structuredQuery = new StructuredQuery( );
            settings = new QuerySqlBuilderSettings( );
            queryBuild = new QueryBuild( );

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Strict );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( structuredQuery, settings ) )
                                      .Returns( ( ) => {
                                          Thread.Sleep( 100 );
                                          return queryBuild;
                                      } )
                                      .Verifiable( );
            querySqlBuilder = mockQuerySqlBuilder.Object;
            cachingQuerySqlBuilder = new CachingQuerySqlBuilder( querySqlBuilder, userRuleSetProvider );

            Task task1 = Task.Factory.StartNew( ( ) => cachingQuerySqlBuilder.BuildSql( structuredQuery, settings ) );
            Thread.Sleep( 1 ); // BlockIfPending can handle overlapping, but no coincident requests
            Task task2 = Task.Factory.StartNew( ( ) => cachingQuerySqlBuilder.BuildSql( structuredQuery, settings ) );
            Task.WaitAll(task1, task2);

            mockQuerySqlBuilder.Verify( x => x.BuildSql( structuredQuery, settings ), Times.Exactly( 1 ) );
            mockQuerySqlBuilder.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SqlIsUncacheable( )
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            StructuredQuery structuredQuery;
            QuerySqlBuilderSettings settings;
            QueryBuild queryBuild;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            structuredQuery = new StructuredQuery( );
            settings = new QuerySqlBuilderSettings( );
            queryBuild = new QueryBuild( );
            queryBuild.SqlIsUncacheable = true;

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Strict );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( structuredQuery, settings ) )
                                      .Returns( ( ) => queryBuild )
                                      .Verifiable( );
            querySqlBuilder = mockQuerySqlBuilder.Object;
            cachingQuerySqlBuilder = new CachingQuerySqlBuilder( querySqlBuilder, userRuleSetProvider );

            cachingQuerySqlBuilder.BuildSql( structuredQuery, settings );
            cachingQuerySqlBuilder.BuildSql( structuredQuery, settings );

            mockQuerySqlBuilder.Verify( x => x.BuildSql( structuredQuery, settings ), Times.Exactly( 2 ) );
            mockQuerySqlBuilder.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_SqlIsUncacheable_TwoThreads( )
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            StructuredQuery structuredQuery;
            QuerySqlBuilderSettings settings;
            QueryBuild queryBuild;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            structuredQuery = new StructuredQuery( );
            settings = new QuerySqlBuilderSettings( );
            queryBuild = new QueryBuild( );
            queryBuild.SqlIsUncacheable = true;

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Strict );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( structuredQuery, settings ) )
                                      .Returns( ( ) =>
                                      {
                                          Thread.Sleep( 100 );
                                          return queryBuild;
                                      } )
                                      .Verifiable( );
            querySqlBuilder = mockQuerySqlBuilder.Object;
            cachingQuerySqlBuilder = new CachingQuerySqlBuilder( querySqlBuilder, userRuleSetProvider );

            Task task1 = Task.Factory.StartNew( ( ) => cachingQuerySqlBuilder.BuildSql( structuredQuery, settings ) );
            Thread.Sleep( 1 ); // BlockIfPending can handle overlapping, but no coincident requests
            Task task2 = Task.Factory.StartNew( ( ) => cachingQuerySqlBuilder.BuildSql( structuredQuery, settings ) );
            Task.WaitAll( task1, task2 );

            mockQuerySqlBuilder.Verify( x => x.BuildSql( structuredQuery, settings ), Times.Exactly( 2 ) );
            mockQuerySqlBuilder.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_EnsureDifferentInstancesCanCacheMatch( )
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            QuerySqlBuilderSettings settings;
            QueryBuild queryBuild;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            StructuredQuery sq1 = ReportHelpers.BuildFilterQuery( "Name='test1'", new EntityRef( "test:person" ), true );
            sq1.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq1.RootEntity, "core:name" ) } );

            StructuredQuery sq2 = ReportHelpers.BuildFilterQuery( "Name='test1'", new EntityRef( "test:person" ), true );
            sq2.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq2.RootEntity, "core:name" ) } );

            settings = new QuerySqlBuilderSettings( );
            queryBuild = new QueryBuild( );

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Strict );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( sq1, settings ) )
                                      .Returns( ( ) => queryBuild )
                                      .Verifiable( );
            querySqlBuilder = mockQuerySqlBuilder.Object;
            cachingQuerySqlBuilder = new CachingQuerySqlBuilder( querySqlBuilder, userRuleSetProvider );

            cachingQuerySqlBuilder.BuildSql( sq1, settings );
            cachingQuerySqlBuilder.BuildSql( sq2, settings );

            mockQuerySqlBuilder.Verify( x => x.BuildSql( sq1, settings ), Times.Exactly( 1 ) );
            mockQuerySqlBuilder.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_EnsureResultsFromDifferentInstancesReturnCorrectRequestColumnData( )
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            QuerySqlBuilderSettings settings;
            QueryBuild queryBuild;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            StructuredQuery sq1 = new StructuredQuery { RootEntity = new ResourceEntity( "test:person" ) };
            sq1.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq1.RootEntity, "core:name" ) } );

            StructuredQuery sq2 = new StructuredQuery { RootEntity = new ResourceEntity( "test:person" ) };
            sq2.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq2.RootEntity, "core:name" ) } );

            Assert.That( sq1.SelectColumns [ 0 ].ColumnId, Is.Not.EqualTo( sq2.SelectColumns [ 0 ].ColumnId ) );

            settings = new QuerySqlBuilderSettings( );
            queryBuild = new QueryBuild();
            queryBuild.Columns.Add( new ResultColumn { RequestColumn = sq1.SelectColumns [ 0 ] } );

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Strict );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( sq1, settings ) )
                                      .Returns( ( ) => queryBuild )
                                      .Verifiable( );
            querySqlBuilder = mockQuerySqlBuilder.Object;
            cachingQuerySqlBuilder = new CachingQuerySqlBuilder( querySqlBuilder, userRuleSetProvider );

            QueryBuild result1 = cachingQuerySqlBuilder.BuildSql( sq1, settings );
            Assert.That( result1.Columns [ 0 ].RequestColumn, Is.EqualTo( sq1.SelectColumns [ 0 ] ) );

            QueryBuild result2 = cachingQuerySqlBuilder.BuildSql( sq2, settings );
            Assert.That( result2.Columns [ 0 ].RequestColumn, Is.EqualTo( sq2.SelectColumns [ 0 ] ) );

            mockQuerySqlBuilder.Verify( x => x.BuildSql( sq1, settings ), Times.Exactly( 1 ) );
            mockQuerySqlBuilder.VerifyAll( );
        }

        internal static void Test_Scenario( StructuredQuery structuredQuery, Action invalidationCallback, bool expectInvalidation )
        {
            CachingQuerySqlBuilder cachingQuerySqlBuilder;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            IQuerySqlBuilder querySqlBuilder;
            QuerySettings settings;
            QueryBuild queryBuild;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            settings = new QuerySettings( );
            queryBuild = new QueryBuild( );

            var cacheInvalidators = new CacheInvalidatorFactory( ).CacheInvalidatorsList_TestOnly;

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Strict );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( It.IsAny<StructuredQuery>( ), settings ) )
                                      .Returns<StructuredQuery, QuerySettings>( ( sq, qs ) =>
                                      {
                                          QuerySqlBuilder.IdentifyCacheDependencies( sq, settings );
                                          return queryBuild;
                                      } )
                                      .Verifiable( );
            querySqlBuilder = mockQuerySqlBuilder.Object;
            cachingQuerySqlBuilder = new CachingQuerySqlBuilder( querySqlBuilder, userRuleSetProvider );

            try
            {
                // Add current cache invalidator to global factory
                cacheInvalidators.Add( cachingQuerySqlBuilder.CacheInvalidator );

                using ( var scope = Factory.Current.BeginLifetimeScope( cb =>
                {
                    cb.Register( c => cachingQuerySqlBuilder ).As<ICacheService>( );
                } ) )
                using ( Factory.SetCurrentScope( scope ) )
                {
                    // Run first time
                    cachingQuerySqlBuilder.BuildSql( structuredQuery.DeepCopy( ), settings );

                    // Perform potential invalidation task
                    using ( new SecurityBypassContext( ) )
                    {
                        invalidationCallback( );
                    }

                    // Run second time
                    cachingQuerySqlBuilder.BuildSql( structuredQuery.DeepCopy( ), settings );
                }

                int times = expectInvalidation ? 2 : 1;
                mockQuerySqlBuilder.Verify( x => x.BuildSql( It.IsAny<StructuredQuery>( ), settings ), Times.Exactly( times ) );
                mockQuerySqlBuilder.VerifyAll( );
            }
            finally
            {
                // Restore cache invalidators
                cacheInvalidators.Remove( cachingQuerySqlBuilder.CacheInvalidator );
            }
        }

        private static IUserRuleSetProvider MockUserRuleSetProvider( )
        {
            var mock = new Mock<IUserRuleSetProvider>( MockBehavior.Loose );
            mock.Setup( x => x.GetUserRuleSet( It.IsAny<long>( ), It.IsAny<EntityRef>( ) ) ).Returns( new UserRuleSet( new List<long>() ) );
            return mock.Object;
        }

    }
}
