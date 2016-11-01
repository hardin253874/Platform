// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using Autofac.Extras.AttributeMetadata;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Caching layer for generating entity decorators that set default values, e.g. for relationships.
    /// </summary>
    class CachingEntityDefaultsDecoratorProvider : GenericCacheService<long, IEntityDefaultsDecorator>, IEntityDefaultsDecoratorProvider
    {
        /// <summary>
        /// Create a new <see cref="CachingEntityDefaultsDecoratorProvider"/>.
        /// </summary>
        /// <param name="innerProvider">
        /// The <see cref="IEntityDefaultsDecoratorProvider"/> that will actually provide default-value decorators.
        /// </param>
        public CachingEntityDefaultsDecoratorProvider( [WithKey( Factory.NonCachedKey )] IEntityDefaultsDecoratorProvider innerProvider )
            : base( "EntityDefaultsDecoratorProviderKey", new CacheFactory { MetadataCache = true } )
        {
            if ( innerProvider == null )
                throw new ArgumentNullException( "innerProvider" );

            InnerProvider = innerProvider;
        }


        /// <summary>
        /// The IEntityDefaultsDecoratorProvider that performs the actual work.
        /// </summary>
        internal IEntityDefaultsDecoratorProvider InnerProvider { get; private set; }


        /// <summary>
        /// Creates a IEntityDefaultsDecorator for a particular type.
        /// </summary>
        /// <param name="typeId">The type.</param>
        /// <returns>A default-value decorator.</returns>
        public IEntityDefaultsDecorator GetDefaultsDecorator( long typeId )
        {
            // Check cache
            return GetOrAdd( typeId, k => InnerProvider.GetDefaultsDecorator( typeId ) );
        }
    }
}
