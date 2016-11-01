// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.QueryEngine.CachingBuilder;
using EDC.ReadiNow.Security.AccessControl;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReadiNow.QueryEngine.Test.Caching
{
    [TestFixture]
    public class CachingQuerySqlBuilderKeyTests
    {
        [Test]
        public void CacheKeyMatches( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;

            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            
            Assert.That( key1, Is.EqualTo( key2 ) );
            Assert.That( key1, Is.Not.SameAs( key2 ) );
        }

        [Test]
        public void CacheKeyMatches_MatchingRuleSets( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings1 = new QuerySqlBuilderSettings( );
            QuerySqlBuilderSettings settings2 = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet1 = new UserRuleSet( new List<long> { 1, 2, 3 } );
            UserRuleSet userRuleSet2 = new UserRuleSet( new List<long> { 1, 2, 3 } );
            settings1.RunAsUser = 111;
            settings2.RunAsUser = 222;

            var key1 = new CachingQuerySqlBuilderKey( sq, settings1, userRuleSet1 );
            var key2 = new CachingQuerySqlBuilderKey( sq, settings2, userRuleSet2 );

            Assert.That( key1, Is.EqualTo( key2 ) );
            Assert.That( key1, Is.Not.SameAs( key2 ) );
        }

        [Test]
        public void CacheKeyMatches_ClientAggregates( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;
            settings.SupportClientAggregate = true;
            settings.ClientAggregate = new ClientAggregate( );
            settings.ClientAggregate.AggregatedColumns.Add( new EDC.ReadiNow.Metadata.Reporting.ReportAggregateField( ) );
            settings.ClientAggregate.GroupedColumns.Add( new EDC.ReadiNow.Metadata.Reporting.ReportGroupField( ) );

            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.EqualTo( key2 ) );
            Assert.That( key1, Is.Not.SameAs( key2 ) );
        }

        [TestCase( false, false )]
        [TestCase( true, false )]
        [TestCase( false, true )]
        [TestCase( true, true )]
        public void CacheKeyMatches_Match_With_Different_Instances( bool differentUsers, bool concurrent )
        {
            Func<int, CachingQuerySqlBuilderKey> makeKey = ( int userId ) =>
            {
                StructuredQuery sq = new StructuredQuery( );
                sq.SelectColumns.Add( new SelectColumn( ) );
                sq.SelectColumns.Add( new SelectColumn( ) );
                QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
                UserRuleSet userRuleSet = new UserRuleSet( new List<long> { 1, 2, 3 } );
                settings.RunAsUser = userId;
                settings.SupportClientAggregate = true;
                settings.ClientAggregate = new ClientAggregate( );
                settings.ClientAggregate.AggregatedColumns.Add( new EDC.ReadiNow.Metadata.Reporting.ReportAggregateField { ReportColumnId = sq.SelectColumns [ 0 ].ColumnId } );
                settings.ClientAggregate.GroupedColumns.Add( new EDC.ReadiNow.Metadata.Reporting.ReportGroupField { ReportColumnId = sq.SelectColumns [ 1 ].ColumnId } );
                var key = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
                return key;            
            };

            CachingQuerySqlBuilderKey key1 = null;
            CachingQuerySqlBuilderKey key2 = null;
            if ( concurrent )
            {
                Task.WaitAll(
                    Task.Factory.StartNew( ( ) => { key1 = makeKey( 111 ); } ),
                    Task.Factory.StartNew( ( ) => { key2 = makeKey( differentUsers ? 222 : 111 ); } )
                    );
            }
            else
            {
                key1 = makeKey( 111 );
                key2 = makeKey( differentUsers ? 222 : 111 );
            }

            Assert.That( key1, Is.EqualTo( key2 ) );
            Assert.That( key1, Is.Not.SameAs( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_SecureQuery( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;

            settings.SecureQuery = false;
            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            settings.SecureQuery = true;
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_SupportPaging( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;

            settings.SupportPaging = false;
            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            settings.SupportPaging = true;
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_SupportQuickSearch( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;

            settings.SupportQuickSearch = false;
            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            settings.SupportQuickSearch = true;
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_UseSharedSql( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;

            settings.UseSharedSql = false;
            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            settings.UseSharedSql = true;
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_FullAggregateClustering( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;

            settings.FullAggregateClustering = false;
            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            settings.FullAggregateClustering = true;
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_RunAsUserRuleSet( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet1 = new UserRuleSet( new List<long> { 22, 33 } );
            UserRuleSet userRuleSet2 = new UserRuleSet( new List<long> { 44, 55 } );
            
            settings.RunAsUser = 1;
            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet1 );
            settings.RunAsUser = 2;
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet2 );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_StructuredQuery( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;

            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            sq.RootEntity = new ResourceEntity( );
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_TimeZoneName( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;

            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            sq.TimeZoneName = "asdf";
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void CacheKeyDifferent_ClientAggregates( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );
            settings.RunAsUser = 0;
            settings.SupportClientAggregate = true;
            settings.ClientAggregate = new ClientAggregate( );

            var key1 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );
            settings.ClientAggregate.GroupedColumns.Add( new EDC.ReadiNow.Metadata.Reporting.ReportGroupField( ) );
            var key2 = new CachingQuerySqlBuilderKey( sq, settings, userRuleSet );

            Assert.That( key1, Is.Not.EqualTo( key2 ) );
        }

        [Test]
        public void DoesRequestAllowForCaching_Default( )
        {
            StructuredQuery sq = new StructuredQuery();
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings();
            settings.SecureQuery = false;

            Assert.That( CachingQuerySqlBuilderKey.DoesRequestAllowForCaching( sq, settings ), Is.True );
        }

        [Test]
        public void DoesRequestAllowForCaching_AllCacheableTrimmings( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            settings.SecureQuery = true;
            settings.SupportPaging = true;
            settings.SupportQuickSearch = true;
            settings.UseSharedSql = true;
            settings.FullAggregateClustering = true;

            Assert.That( CachingQuerySqlBuilderKey.DoesRequestAllowForCaching( sq, settings ), Is.True );
        }

        [Test]
        public void DoesRequestAllowForCaching_AdditionalOrderColumns( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            settings.AdditionalOrderColumns = new System.Collections.Generic.Dictionary<Guid, SelectColumn>( );
            settings.AdditionalOrderColumns [ Guid.Empty ] = null;

            // See notes in CachingQuerySqlBuilderKey.DoesRequestAllowForCaching
            Assert.That( CachingQuerySqlBuilderKey.DoesRequestAllowForCaching( sq, settings ), Is.True );
        }

        [Test]
        public void DoesRequestAllowForCaching_CaptureExpressionMetadata( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            settings.CaptureExpressionMetadata = true;

            Assert.That( CachingQuerySqlBuilderKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }

        [Test]
        public void DoesRequestAllowForCaching_DebugMode( )
        {
            StructuredQuery sq = new StructuredQuery( );
            QuerySqlBuilderSettings settings = new QuerySqlBuilderSettings( );
            settings.DebugMode = true;

            Assert.That( CachingQuerySqlBuilderKey.DoesRequestAllowForCaching( sq, settings ), Is.False );
        }
    }
}
