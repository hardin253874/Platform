// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.QueryEngine.CachingBuilder;
using ReadiNow.QueryEngine.CachingRunner;
using ReadiNow.QueryEngine.ReportConverter;
using EDC.ReadiNow.Core.Cache.Providers;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    [TestFixture]
	[RunWithTransaction]
    public class CacheInvalidatorFactoryTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_CacheInvalidators()
        {
            CacheInvalidatorFactory factory;

            factory = new CacheInvalidatorFactory();

            Assert.That(factory.CacheInvalidators,
                        Has.Exactly(2).TypeOf<SecurityQueryCacheInvalidator>());
            Assert.That(factory.CacheInvalidators,
                        Has.Exactly(2).TypeOf<SecurityCacheInvalidatorBase<UserEntityPermissionTuple, bool>>());
            Assert.That(factory.CacheInvalidators,
                        Has.Exactly(1).TypeOf<ReportToQueryCacheInvalidator>());
            Assert.That(factory.CacheInvalidators,
                        Has.Exactly(1).TypeOf<EntityMemberRequestCacheInvalidator>());
            Assert.That(factory.CacheInvalidators,
                        Has.Exactly(1).TypeOf<UserRoleRepositoryCacheInvalidator>());
            Assert.That(factory.CacheInvalidators,
                        Has.Exactly(1).Property("Name").Matches("PerTenantEntityTypeCacheInvalidator"));
            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 1 ).TypeOf<CachingQuerySqlBuilderInvalidator>( ) );
            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 1 ).TypeOf<CachingQueryRunnerInvalidator>( ) );
            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 1 ).TypeOf<CachingUserRuleSetProviderInvalidator>( ) );
            Assert.That(factory.CacheInvalidators,
                Has.Exactly(1).TypeOf<MetadataCacheInvalidator>());

        }
    }
}
