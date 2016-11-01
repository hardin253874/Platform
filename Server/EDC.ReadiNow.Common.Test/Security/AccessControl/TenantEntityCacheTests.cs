// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class TenantEntityCacheTests
    {
        [Test]
        public void Test_Creation()
        {
            TenantEntityCache tenantEntityCache;
            const string testAlias = "bar";
            const string testNamespace = "foo";

            tenantEntityCache = new TenantEntityCache(testNamespace, testAlias);
            Assert.That(tenantEntityCache, Has.Property("EntityNamespace").EqualTo(testNamespace));
            Assert.That(tenantEntityCache, Has.Property("EntityAlias").EqualTo(testAlias));
            Assert.That(tenantEntityCache.TenantEntityMap.Keys, Has.Count.EqualTo(0));
        }

        [Test]
        public void Test_Creation_NullNamespace()
        {
            Assert.That(() => new TenantEntityCache(null, "alias"),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityNamespace"));
        }

        [Test]
        public void Test_Creation_NullAlias()
        {
            Assert.That(() => new TenantEntityCache("namespace", null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entityAlias"));
        }

        [Test]
        public void Test_Entity_NoRequestContext()
        {
            Assert.That(() => new TenantEntityCache("core", "resource").EntityRef,
                Throws.InvalidOperationException);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Entity()
        {
            TenantEntityCache tenantEntityCache;
            const string testAlias = "resource";
            const string testNamespace = "core";

            tenantEntityCache = new TenantEntityCache(testNamespace, testAlias);

            Assert.That(tenantEntityCache.EntityRef, Is.EqualTo(new EntityRef(testNamespace, testAlias)).Using(EntityRefComparer.Instance));
            Assert.That(tenantEntityCache.TenantEntityMap.Keys, Has.Count.EqualTo(1));
            Assert.That(tenantEntityCache.TenantEntityMap.Keys, Has.Exactly(1).EqualTo(RequestContext.TenantId));
            Assert.That(tenantEntityCache.TenantEntityMap[RequestContext.TenantId],
                Is.EqualTo(new EntityRef(testNamespace, testAlias)).Using(EntityRefComparer.Instance));
        }
    }
}
