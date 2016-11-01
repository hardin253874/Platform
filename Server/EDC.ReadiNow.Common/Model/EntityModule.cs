// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using Autofac.Extras.AttributeMetadata;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model.CacheInvalidation;
using ReadiNow.Common;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Autofac dependency injection module for data connector.
    /// </summary>
    public class EntityModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder">The autofac container builder.</param>
        protected override void Load( ContainerBuilder builder )
        {
            // Register EntityRepository
            builder.RegisterType<EntityRepository>( )
                .As<IEntityRepository>( );

            // Register EntitySaver
            builder.RegisterType<EntitySaver>( )
                .As<IEntitySaver>( );

            // Register EntityResolver
            builder.RegisterType<EntityResolverProvider>( )
                .As<IEntityResolverProvider>( );

            // Register multi cache invalidator
            builder.Register( c =>
                new MultiCacheInvalidator( new CacheInvalidatorFactory( ).CacheInvalidators ) )
                .Named<ICacheInvalidator>( MultiCacheInvalidator.Autofac_Key )
                .SingleInstance( ); // for efficiency only

            // Register EntityDefaultsDecoratorProvider
            builder.Register( ctx => new EntityDefaultsDecoratorProvider( ctx.ResolveNamed<IEntityRepository>( "Graph" ), ctx.Resolve<IDateTime>( ) ) )
                .As<EntityDefaultsDecoratorProvider>( )
                .Keyed<IEntityDefaultsDecoratorProvider>( Factory.NonCachedKey );

            // CachingEntityDefaultsDecoratorProvider
            builder.RegisterType<CachingEntityDefaultsDecoratorProvider>( )
                .WithAttributeFilter( )
                .As<IEntityDefaultsDecoratorProvider>( )
                .As<ICacheService>( )
                .SingleInstance( );

            // Register UpgradeIdProvider
            builder.RegisterType<UpgradeIdProvider>()
                .As<IUpgradeIdProvider>();
        }
    }
}
