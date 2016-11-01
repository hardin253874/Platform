// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
	[TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class SecurityQueryCacheInvalidatorTests
    {
        [Test]
        public void Test_Ctor()
        {
            ICache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>> cache;
            SecurityQueryCacheInvalidator securityQueryCacheInvalidator;

            cache = new DictionaryCache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>>( );
            securityQueryCacheInvalidator = new SecurityQueryCacheInvalidator(cache);

            Assert.That(securityQueryCacheInvalidator, Has.Property("Cache").EqualTo(cache));
        }

        [Test]
        public void Test_Ctor_NullCache()
        {
            Assert.That(() => new SecurityQueryCacheInvalidator(null), 
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("cache"));
        }

        [Test]
        public void Test_InvalidateEntries_NullKeysToRemove()
        {
            Assert.That(
                () => new SecurityQueryCacheInvalidator(new DictionaryCache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>>()).InvalidateCacheEntries(null, ()=>"test"),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("keysToRemove"));
        }

        [Test]
        public void Test_InvalidateEntries_NullCause()
        {
            Assert.That(
                () => new SecurityQueryCacheInvalidator(new DictionaryCache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>>()).InvalidateCacheEntries(new SubjectPermissionTypesTuple[0] , null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("cause"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_InvalidateEntries_AccessRule()
        {
            AccessRule accessRule;
            Subject subject;
            ICache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>> dictionaryCache;
            SecurityQueryCacheInvalidator securityQueryCacheInvalidator;

            subject = new Subject();
            subject.Save();

            accessRule = new AccessRule();
            accessRule.AllowAccessBy = subject;
            accessRule.Save();

            dictionaryCache = new DictionaryCache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>>
                {
                    {new SubjectPermissionTypesTuple(subject.Id, 2, new long[0]), new AccessRuleQuery[0]},
                    {new SubjectPermissionTypesTuple(subject.Id, 3, new long[0]), new AccessRuleQuery[0]},
                    {new SubjectPermissionTypesTuple(subject.Id + 1, 3, new long[0]), new AccessRuleQuery[0]},
                    {new SubjectPermissionTypesTuple(subject.Id + 2, 3, new long[0]), new AccessRuleQuery[0]}
                };

            securityQueryCacheInvalidator = new SecurityQueryCacheInvalidator(dictionaryCache);
            securityQueryCacheInvalidator.InvalidateCacheEntries(new []
            {
                new SubjectPermissionTypesTuple(subject.Id, 2, new long[0]), 
                new SubjectPermissionTypesTuple(subject.Id, 3, new long[0]), 
            }, ()=>"test");

            Assert.That(dictionaryCache, Has.None.Property("Key").Property("SubjectId").EqualTo(subject.Id));
        }
    }
}
