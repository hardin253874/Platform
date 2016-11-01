// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.QueryEngine.CachingBuilder;
using ReadiNow.QueryEngine.CachingRunner;
using System.Collections.Generic;
using EDC.ReadiNow.Security.AccessControl;
using System.Collections.Concurrent;

namespace ReadiNow.QueryEngine.Test.Caching
{
    [TestFixture]
    public class CachingQueryRunnerKeyTests
    {
        [Test]
        public void CacheKeyMatches( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            long userId = 0;

            var key1 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId );
            var key2 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId );

            Assert.That( key1, Is.EqualTo( key2 ) );
            Assert.That( key1, Is.Not.SameAs( key2 ) );
        }

        [Test]
        public void CacheKeyMatches_DifferentInstancesInKey( )
        {
            StructuredQuery sq1 = new StructuredQuery( );
            StructuredQuery sq2 = new StructuredQuery( );
            QuerySettings settings1 = new QuerySettings( );
            QuerySettings settings2 = new QuerySettings( );
            UserRuleSet userRuleSet1 = new UserRuleSet( new List<long> { 1, 2 } );
            UserRuleSet userRuleSet2 = new UserRuleSet( new List<long> { 1, 2 } );
            long userId = 0;

            var key1 = new CachingQueryRunnerKey( sq1, settings1, userRuleSet1, userId );
            var key2 = new CachingQueryRunnerKey( sq2, settings2, userRuleSet2, userId );

            Assert.That( key1, Is.EqualTo( key2 ) );
            Assert.That( key1, Is.Not.SameAs( key2 ) );

            ConcurrentDictionary<CachingQueryRunnerKey, long> dict = new ConcurrentDictionary<CachingQueryRunnerKey, long>( );
            dict.AddOrUpdate( key1, 1, ( k, v ) => 1 );
            long value;
            bool exists = dict.TryGetValue( key2, out value );
            Assert.That( exists, Is.True );
        }

        [Test]
        public void CacheKeyMatches_TargetResource( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            long userId = 0;
            settings.TargetResource = 1;

            var key1 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId );
            var key2 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId );

            Assert.That( key1, Is.EqualTo( key2 ) );
            Assert.That( key1, Is.Not.SameAs( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_TargetResource( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            long userId = 0;
            settings.RunAsUser = 0;

            settings.TargetResource = 1;
            var key1 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId );

            settings.TargetResource = 2;
            var key2 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_ResultSchemaOnly( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            long userId = 0;
            settings.RunAsUser = 0;

            settings.ResultSchemaOnly = false;
            var key1 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId );

            settings.ResultSchemaOnly = true;
            var key2 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_User( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            long userId1 = 1;
            long userId2 = 2;

            var key1 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId1 );

            var key2 = new CachingQueryRunnerKey( sq, settings, userRuleSet, userId2 );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void DoesRequestAllowForCaching_Default( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );

            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.True );            
        }

        [Test]
        public void DoesRequestAllowForCaching_AllCacheableTrimmings( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            settings.SecureQuery = true;
            settings.SupportPaging = true;
            settings.UseSharedSql = true;
            settings.FullAggregateClustering = true;

            Assert.That( CachingQuerySqlBuilderKey.DoesRequestAllowForCaching( sq, settings ), Is.True );
        }

        [Test]
        public void DoesRequestAllowForCaching_NonFirstPage( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            settings.SecureQuery = false;
            settings.SupportPaging = true;
            settings.FirstRow = 10;

            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }

        [Test]
        public void DoesRequestAllowForCaching_RootIdFilter( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            settings.SecureQuery = false;
            settings.SupportRootIdFilter = true;

            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }

        [Test]
        public void DoesRequestAllowForCaching_QuickSearch( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            settings.SecureQuery = false;
            settings.SupportQuickSearch = true;

            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }

        [Test]
        public void DoesRequestAllowForCaching_IncludeResources( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            settings.SecureQuery = false;
            settings.IncludeResources = new List<long> { 1 };

            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }

        [Test]
        public void DoesRequestAllowForCaching_ExcludeResources( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            settings.SecureQuery = false;
            settings.IncludeResources = new List<long> { 1 };

            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }

        [Test]
        public void DoesRequestAllowForCaching_AdditionalOrderColumns( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            settings.AdditionalOrderColumns = new System.Collections.Generic.Dictionary<Guid, SelectColumn>( );
            settings.AdditionalOrderColumns [ Guid.Empty ] = null;
            
            // See notes in CachingQuerySqlBuilderKey.DoesRequestAllowForCaching
            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.True );
        }

        [Test]
        public void DoesRequestAllowForCaching_CaptureExpressionMetadata( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            settings.CaptureExpressionMetadata = true;

            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }

        [Test]
        public void DoesRequestAllowForCaching_DebugMode( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySettings settings = new QuerySettings( );
            settings.DebugMode = true;

            Assert.That( CachingQueryRunnerKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }
    }
}
