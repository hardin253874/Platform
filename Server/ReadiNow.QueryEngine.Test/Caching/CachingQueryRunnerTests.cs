// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using ReadiNow.QueryEngine.CachingRunner;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model.CacheInvalidation;
using ReadiNow.QueryEngine.Runner;
using EDC.ReadiNow.Security;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Model;
using ResourceExpression = EDC.ReadiNow.Metadata.Query.Structured.ResourceExpression;

namespace ReadiNow.QueryEngine.Test.Caching
{
    [TestFixture]
    public class CachingQueryRunnerTests
    {
		[SetUp]
	    public void TestSetup( )
		{
			CachingQueryRunner cachingQueryRunner;
			Mock<IQueryRunner> mockQueryRunner;
			IQueryRunner queryRunner;
			IQuerySqlBuilder queryBuilder = MockQuerySqlBuilder( );
			IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

			mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
			queryRunner = mockQueryRunner.Object;

			cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );

			cachingQueryRunner.Clear( );

			Thread.Sleep( 1000 );
		}

	    [Test]
        public void Test_Ctor( )
        {
            CachingQueryRunner cachingQueryRunner;
            Mock<IQueryRunner> mockQueryRunner;
            IQueryRunner queryRunner;
            IQuerySqlBuilder queryBuilder = MockQuerySqlBuilder( );
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
            queryRunner = mockQueryRunner.Object;

            cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );
            Assert.That( cachingQueryRunner,
                Has.Property( "QueryRunner" ).SameAs( queryRunner ) );
            Assert.That( cachingQueryRunner,
                Has.Property( "UserRuleSetProvider" ).SameAs( userRuleSetProvider ) );
            Assert.That( cachingQueryRunner,
                Has.Property( "Cache" ).Not.Null
                   .And.Property( "Cache" ).Count.EqualTo( 0 ) );
            Assert.That( cachingQueryRunner.CacheInvalidator, Is.Not.Null );

            mockQueryRunner.VerifyAll( );
        }

        [Test]
        public void Test_Ctor_Null_QueryRunner( )
        {
            IQuerySqlBuilder queryBuilder = MockQuerySqlBuilder( );
            IUserRuleSetProvider userRuleSetProvider = new Mock<IUserRuleSetProvider>( ).Object;

            Assert.That( ( ) => new CachingQueryRunner( null, userRuleSetProvider, queryBuilder ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "queryRunner" ) );
        }

        [Test]
        public void Test_Ctor_Null_UserRuleSetProvider( )
        {
            IQuerySqlBuilder queryBuilder = MockQuerySqlBuilder( );
            IQueryRunner queryRunner = new Mock<IQueryRunner>( ).Object;

            Assert.That( ( ) => new CachingQueryRunner( queryRunner, null, queryBuilder ),
                Throws.TypeOf<ArgumentNullException>( ).And.Property( "ParamName" ).EqualTo( "userRuleSetProvider" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_Cached( )
        {
            CachingQueryRunner cachingQueryRunner;
            Mock<IQueryRunner> mockQueryRunner;
            IQueryRunner queryRunner;
            StructuredQuery structuredQuery;
            QuerySettings settings;
            QueryBuild queryBuild;
            QueryResult result;
            IQuerySqlBuilder queryBuilder = MockQuerySqlBuilder( );
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            structuredQuery = new StructuredQuery( );
            settings = new QuerySettings( );
            queryBuild = new QueryBuild( );
            result = new QueryResult( queryBuild );

            mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
            mockQueryRunner.Setup( x => x.ExecutePrebuiltQuery( structuredQuery, settings, It.IsNotNull<QueryBuild>() ) )
                                      .Returns( ( ) => 
                                          {
                                              return result;
                                          })
                                      .Verifiable( );
            queryRunner = mockQueryRunner.Object;
            cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );

            Assert.That( cachingQueryRunner.ExecuteQuery( structuredQuery, settings ),
                Is.SameAs( result ) );
            Assert.That( cachingQueryRunner.Cache,
                Has.Count.EqualTo( 1 ) );

            mockQueryRunner.Verify( x => x.ExecutePrebuiltQuery( structuredQuery, settings, It.IsNotNull<QueryBuild>( ) ), Times.Exactly( 1 ) );
            mockQueryRunner.VerifyAll( );
        }

        [TestCase(false, false, true)]
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_EnsureCached( bool resultIsUserSpecific, bool runAsDifferentUser, bool expectedToCache )
        {
            CachingQueryRunner cachingQueryRunner;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder; 
            Mock<IQueryRunner> mockQueryRunner;
            IQueryRunner queryRunner;
            StructuredQuery structuredQuery;
            QuerySettings settings;
            QueryBuild queryBuild;
            QueryResult result;
            IQuerySqlBuilder queryBuilder;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            structuredQuery = new StructuredQuery( );
            settings = new QuerySettings( );
            settings.RunAsUser = 1;
            queryBuild = new QueryBuild( );
            result = new QueryResult( queryBuild );
            queryBuild.DataReliesOnCurrentUser = resultIsUserSpecific;

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Strict );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( It.IsAny<StructuredQuery>( ), It.IsNotNull<QuerySqlBuilderSettings>( ) ) ).Returns( queryBuild );
            queryBuilder = mockQuerySqlBuilder.Object;
            
            mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
            mockQueryRunner.Setup( x => x.ExecutePrebuiltQuery( structuredQuery, settings, It.IsNotNull<QueryBuild>( ) ) )
                                      .Returns( ( ) => result )
                                      .Verifiable( );
            queryRunner = mockQueryRunner.Object;
            cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );

            cachingQueryRunner.ExecuteQuery( structuredQuery, settings );
            if (runAsDifferentUser)
            {
                settings.RunAsUser = 2;
            }
            cachingQueryRunner.ExecuteQuery( structuredQuery, settings );

            int calls = expectedToCache ? 1 : 2;
            mockQueryRunner.Verify( x => x.ExecutePrebuiltQuery( structuredQuery, settings, It.IsNotNull<QueryBuild>( ) ), Times.Exactly( calls ) );
            mockQueryRunner.VerifyAll( );
        }

        [TestCase( false, false, true )]
        [TestCase( false, true, true )]
        [TestCase( true, false, true )]
        [TestCase( true, true, false )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [Ignore]        // Blocking layer has been disabled for this cache.
        public void Test_EnsureCached_TwoThreads( bool resultIsUserSpecific, bool runAsDifferentUser, bool expectedToCache )
        {
            CachingQueryRunner cachingQueryRunner;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            Mock<IQueryRunner> mockQueryRunner;
            IQueryRunner queryRunner;
            StructuredQuery structuredQuery;
            QuerySettings settings1;
            QuerySettings settings2;
            QueryBuild queryBuild;
            QueryResult result;
            IQuerySqlBuilder queryBuilder;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            structuredQuery = new StructuredQuery( );
            settings1 = new QuerySettings( );
            settings1.RunAsUser = 1;
            settings2 = new QuerySettings( );
            settings2.RunAsUser = runAsDifferentUser ? 2 : 1;
            queryBuild = new QueryBuild( );
            result = new QueryResult( queryBuild );
            queryBuild.DataReliesOnCurrentUser = resultIsUserSpecific;

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Strict );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( It.IsAny<StructuredQuery>( ), It.IsNotNull<QuerySqlBuilderSettings>( ) ) ).Returns( queryBuild );
            queryBuilder = mockQuerySqlBuilder.Object;

            mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
            mockQueryRunner.Setup( x => x.ExecutePrebuiltQuery( structuredQuery, It.IsNotNull<QuerySettings>(), It.IsNotNull<QueryBuild>( ) ) )
                                      .Returns( ( ) =>
                                      {
                                          Thread.Sleep( 100 );
                                          return result;
                                      } )
                                      .Verifiable( );
            queryRunner = mockQueryRunner.Object;
            cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );

            Task task1 = Task.Factory.StartNew( ( ) => cachingQueryRunner.ExecuteQuery( structuredQuery, settings1 ) );
            Thread.Sleep( 1 ); // BlockIfPending can handle overlapping, but no coincident requests
            Task task2 = Task.Factory.StartNew( ( ) => cachingQueryRunner.ExecuteQuery( structuredQuery, settings2 ) );
            Task.WaitAll( task1, task2 );

            int calls = expectedToCache ? 1 : 2;
            mockQueryRunner.Verify( x => x.ExecutePrebuiltQuery( structuredQuery, It.IsNotNull<QuerySettings>(), It.IsNotNull<QueryBuild>( ) ), Times.Exactly( calls ) );
            mockQueryRunner.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_EnsureDifferentInstancesCanCacheMatch( )
        {
            CachingQueryRunner cachingQueryRunner;
            Mock<IQueryRunner> mockQueryRunner;
            IQueryRunner queryRunner;
            QuerySettings settings;
            QueryBuild queryBuild;
            QueryResult result;
            IQuerySqlBuilder queryBuilder = MockQuerySqlBuilder( );
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            StructuredQuery sq1 = ReportHelpers.BuildFilterQuery( "Name='test1'", new EntityRef( "test:person" ), true );
            sq1.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq1.RootEntity, "core:name" ) } );

            StructuredQuery sq2 = ReportHelpers.BuildFilterQuery( "Name='test1'", new EntityRef( "test:person" ), true );
            sq2.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq2.RootEntity, "core:name" ) } );

            settings = new QuerySettings( );
            queryBuild = new QueryBuild( );
            result = new QueryResult( queryBuild );

            mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
            mockQueryRunner.Setup( x => x.ExecutePrebuiltQuery( sq1, settings, It.IsNotNull<QueryBuild>( ) ) )
                                      .Returns( ( ) => result )
                                      .Verifiable( );
            queryRunner = mockQueryRunner.Object;
            cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );

            cachingQueryRunner.ExecuteQuery( sq1, settings );
            cachingQueryRunner.ExecuteQuery( sq2, settings );

            mockQueryRunner.Verify( x => x.ExecutePrebuiltQuery( sq1, settings, It.IsNotNull<QueryBuild>( ) ), Times.Exactly( 1 ) );
            mockQueryRunner.VerifyAll( );
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_EnsureResultsFromDifferentInstancesReturnCorrectRequestColumnData( )
        {
            CachingQueryRunner cachingQueryRunner;
            Mock<IQueryRunner> mockQueryRunner;
            IQueryRunner queryRunner;
            QuerySettings settings;
            QueryBuild queryBuild;
            QueryResult result;
            IQuerySqlBuilder queryBuilder = MockQuerySqlBuilder( );
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            StructuredQuery sq1 = new StructuredQuery { RootEntity = new ResourceEntity( "test:person" ) };
            sq1.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq1.RootEntity, "core:name" ) } );

            StructuredQuery sq2 = new StructuredQuery { RootEntity = new ResourceEntity( "test:person" ) };
            sq2.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq2.RootEntity, "core:name" ) } );

            Assert.That( sq1.SelectColumns [ 0 ].ColumnId, Is.Not.EqualTo( sq2.SelectColumns [ 0 ].ColumnId ) );

            settings = new QuerySettings( );
            queryBuild = new QueryBuild( );
            result = new QueryResult( queryBuild );
            result.Columns.Add( new ResultColumn { RequestColumn = sq1.SelectColumns [ 0 ] } );

            mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
            mockQueryRunner.Setup( x => x.ExecutePrebuiltQuery( sq1, settings, It.IsNotNull<QueryBuild>( ) ) )
                                      .Returns( ( ) => result )
                                      .Verifiable( );
            queryRunner = mockQueryRunner.Object;
            cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );

            QueryResult result1 = cachingQueryRunner.ExecuteQuery( sq1, settings );
            Assert.That( result1.Columns [ 0 ].RequestColumn, Is.EqualTo( sq1.SelectColumns [ 0 ] ) );

            QueryResult result2 = cachingQueryRunner.ExecuteQuery( sq2, settings );
            Assert.That( result2.Columns [ 0 ].RequestColumn, Is.EqualTo( sq2.SelectColumns [ 0 ] ) );

            mockQueryRunner.Verify( x => x.ExecutePrebuiltQuery( sq1, settings, It.IsNotNull<QueryBuild>( ) ), Times.Exactly( 1 ) );
            mockQueryRunner.VerifyAll( );
        }

        internal static void Test_Scenario( StructuredQuery structuredQuery, Action invalidationCallback, bool expectInvalidation )
        {
            CachingQueryRunner cachingQueryRunner;
            Mock<IQuerySqlBuilder> mockQuerySqlBuilder;
            Mock<IQueryRunner> mockQueryRunner;
            IQueryRunner queryRunner;
            QuerySettings settings;
            QueryResult result;
            QueryBuild prebuiltQuery;
            IQuerySqlBuilder queryBuilder;
            IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

            settings = new QuerySettings( );
            prebuiltQuery = new QueryBuild( );
            result = new QueryResult( prebuiltQuery );

            var cacheInvalidators = new CacheInvalidatorFactory( ).CacheInvalidatorsList_TestOnly;

            mockQuerySqlBuilder = new Mock<IQuerySqlBuilder>( MockBehavior.Loose );
            mockQuerySqlBuilder.Setup( x => x.BuildSql( It.IsAny<StructuredQuery>( ), It.IsNotNull<QuerySqlBuilderSettings>( ) ) ).Returns( prebuiltQuery );
            queryBuilder = mockQuerySqlBuilder.Object;

            mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
            mockQueryRunner.Setup( x => x.ExecutePrebuiltQuery( It.IsAny<StructuredQuery>( ), settings, prebuiltQuery ) )
                                      .Returns<StructuredQuery, QuerySettings, QueryBuild >( (sq, qs, pbq ) =>
                                      {
                                          QueryRunner.IdentifyCacheDependencies(sq, settings);
                                          return result;
                                      } )
                                      .Verifiable( );
            queryRunner = mockQueryRunner.Object;

            cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );

            try
            {
                // Add current cache invalidator to global factory
                cacheInvalidators.Add( cachingQueryRunner.CacheInvalidator );

                using ( var scope = Factory.Current.BeginLifetimeScope( cb =>
                {
                    cb.Register( c => cachingQueryRunner ).As<ICacheService>( );
                } ) )
                using ( Factory.SetCurrentScope( scope ) )
                {
                    // Run first time
                    cachingQueryRunner.ExecuteQuery( structuredQuery.DeepCopy( ), settings );

                    // Perform potential invalidation task
                    using ( new SecurityBypassContext( ) )
                    {
                        invalidationCallback( );
                    }

                    // Run second time
                    cachingQueryRunner.ExecuteQuery( structuredQuery.DeepCopy( ), settings );
                }

                int times = expectInvalidation ? 2 : 1;
                mockQueryRunner.Verify( x => x.ExecutePrebuiltQuery( It.IsAny<StructuredQuery>( ), settings, It.IsAny<QueryBuild>( ) ), Times.Exactly( times ) );
                mockQueryRunner.VerifyAll( );
            }
            finally
            {
                // Restore cache invalidators
                cacheInvalidators.Remove( cachingQueryRunner.CacheInvalidator );
            }
        }

        private static IQuerySqlBuilder MockQuerySqlBuilder( )
        {
            var mock = new Mock<IQuerySqlBuilder>( MockBehavior.Loose );
            mock.Setup( x => x.BuildSql( It.IsAny<StructuredQuery>(), It.IsNotNull<QuerySqlBuilderSettings>()) ).Returns( new QueryBuild( ) );
            return mock.Object;
        }

        private static IUserRuleSetProvider MockUserRuleSetProvider( )
        {
            var mock = new Mock<IUserRuleSetProvider>( MockBehavior.Loose );
            mock.Setup( x => x.GetUserRuleSet( It.IsAny<long>( ), It.IsAny<EntityRef>( ) ) ).Returns( new UserRuleSet( new List<long>( ) ) );
            return mock.Object;
        }

    }
}
