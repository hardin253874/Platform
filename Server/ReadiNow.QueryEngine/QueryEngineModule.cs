// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using Autofac.Extras.AttributeMetadata;

using ReadiNow.QueryEngine.Builder;
using ReadiNow.QueryEngine.CachingBuilder;
using ReadiNow.QueryEngine.Runner;
using ReadiNow.QueryEngine.CachingRunner;
using EDC.ReadiNow.Core;
using ReadiNow.QueryEngine.ReportConverter;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.Database;

namespace ReadiNow.QueryEngine
{
    /// <summary>
    /// Autofac dependency injection module for query engine.
    /// </summary>
    public class QueryEngineModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            ProtobufRegistrations( );

            // QuerySqlBuilder 
            builder.RegisterType<QuerySqlBuilder>( )
                .Keyed<IQuerySqlBuilder>( Factory.NonCachedKey );

            // CachingQuerySqlBuilder 
            builder.RegisterType<CachingQuerySqlBuilder>( )
                .WithAttributeFilter( )
                .As<IQuerySqlBuilder>( )
                .As<ICacheService>( )
                .SingleInstance( );


            // QueryRunner 
            builder.RegisterType<QueryRunner>( )
                .Keyed<IQueryRunner>( Factory.NonCachedKey );
            
            // QueryRunner for use in tests - uncached runner using uncached builder
            builder.Register(
                ctx => new QueryRunner(
                    new QuerySqlBuilder(),
                    ctx.Resolve<IDatabaseProvider >() )
                )
                .Keyed<IQueryRunner>( "Test" );

            // CachingQueryRunner 
            builder.RegisterType<CachingQueryRunner>( )
                .WithAttributeFilter( )
                .As<IQueryRunner>( )
                .As<ICacheService>( )
                .As<IQueryRunnerCacheKeyProvider>( ) 
                .SingleInstance( );


            // ReportToQueryConverter 
            builder.RegisterType<ReportToQueryConverter>( )
                .Keyed<IReportToQueryConverter>( Factory.NonCachedKey );

            // CachingReportToQueryConverter 
            builder.RegisterType<CachingReportToQueryConverter>( )
                .WithAttributeFilter( )
                .As<IReportToQueryConverter>( )
                .As<ICacheService>( )
                .As<CachingReportToQueryConverter>( )
                .SingleInstance( );

            // ReportToQueryPartsConverter
            builder.RegisterType<ReportToQueryPartsConverter>( )
                .As<IReportToQueryPartsConverter>( );            
        }

        /// <summary>
        /// Register ProtobufSerialization types.
        /// </summary>
        private static void ProtobufRegistrations( )
        {
            // IQueryRunnerCacheKey is defined in common, but CachingQueryRunnerKey in this assembly.
            // So need to dynamically register it as a subtype.

            ProtoBuf.Meta.RuntimeTypeModel.Default.Add( typeof( IQueryRunnerCacheKey ), true )
                .AddSubType( 50, typeof( CachingQueryRunnerKey ) );
        }
    }
}
