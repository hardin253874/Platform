// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ActivationTests
    {
        [Test]
        public void RuleRepository( )
        {
            IRuleRepository ruleRepo = Factory.RuleRepository;
            Assert.That( ruleRepo, Is.TypeOf<RuleRepository>() );
        }

        [Test]
        public void CachingUserRuleSetProvider( )
        {
            IUserRuleSetProvider provider = Factory.UserRuleSetProvider;
            Assert.That( provider, Is.TypeOf<CachingUserRuleSetProvider>() );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CacheInvalidators( )
        {
            CacheInvalidatorFactory factory;

            factory = new CacheInvalidatorFactory( );

            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 1 ).TypeOf<CachingUserRuleSetProviderInvalidator>( ) );
            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 2 ).TypeOf<SecurityQueryCacheInvalidator>( ) );
            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 2 ).TypeOf<SecurityCacheInvalidatorBase<UserEntityPermissionTuple, bool>>( ) );
            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 1 ).TypeOf<EntityMemberRequestCacheInvalidator>( ) );
            Assert.That( factory.CacheInvalidators,
                        Has.Exactly( 1 ).TypeOf<UserRoleRepositoryCacheInvalidator>( ) );
        }

    }
}
