// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ReadiNow.QueryEngine.CachingBuilder;
using ReadiNow.QueryEngine.CachingRunner;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.Caching
{
    [TestFixture]
    public class IQueryRunnerCacheKeyProviderTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void CacheKeyMatches( )
        {
            StructuredQuery sq = new StructuredQuery( );
            sq.RootEntity = new ResourceEntity( new EDC.ReadiNow.Model.EntityRef( "test:person" ) );

            QuerySettings settings = new QuerySettings( );
            UserRuleSet userRuleSet = new UserRuleSet( new List<long>( ) );

            IQueryRunnerCacheKeyProvider provider = Factory.Current.Resolve<IQueryRunnerCacheKeyProvider>( );
            var key1 = provider.CreateCacheKey( sq, settings );
            var key2 = provider.CreateCacheKey( sq, settings );

            Assert.That( key1, Is.EqualTo( key2 ) );
            Assert.That( key1, Is.Not.SameAs( key2 ) );
        }
    }
}
