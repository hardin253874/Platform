// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Model
{
    [TestFixture]
	[RunWithTransaction]
    public class PerTenantEntityTypeCacheTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void ClearTest()
        {
            // Get the base resource type
            var resourceType = Entity.Get<EntityType>("core:resource");

            ISet<long> descendants1 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> descendants2 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);

            ISet<long> ancestors1 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(resourceType.Id);
            ISet<long> ancestors2 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(resourceType.Id);

            Assert.AreSame(descendants1, descendants2);
            Assert.AreSame(ancestors1, ancestors2);

            // After the clear a new cache entry should be created
            PerTenantEntityTypeCache.Instance.Clear();

            descendants2 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ancestors2 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);            
                        
            // After the clear a new cache entry should be created
            PerTenantEntityTypeCache.Instance.Clear();

            Assert.AreNotSame(descendants1, descendants2);
            Assert.AreNotSame(ancestors1, ancestors2);
        }


        [Test]
        [RunAsDefaultTenant]
        public void GetAncestorsAndSelfIsCorrectTest()
        {
            // Get the report resource type
            var reportType = Entity.Get<EntityType>("core:report");

            ISet<long> ancestorsViaCache = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            IEnumerable<EntityType> ancestorsViaModel = reportType.GetAncestorsAndSelf();

            Assert.AreEqual(ancestorsViaModel.Count(), ancestorsViaCache.Count);

            ancestorsViaCache.SetEquals(ancestorsViaModel.Select(e => e.Id));
        }


        [Test]
        [RunAsDefaultTenant]
        public void GetDescendantsAndSelfIsCorrectTest()
        {
            // This should only clear the cache for tenant 0
            PerTenantEntityTypeCache.Instance.Clear();

            // clear entity model cache
            CacheManager.ClearCaches();

            // Get the base resource type
            var resourceType = Entity.Get<EntityType>("core:resource");

            ISet<long> descendantsViaCache = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);

            IEnumerable<EntityType> descendentsViaModel = resourceType.GetDescendantsAndSelf();

            // TODO : THESE SHOULD BE EQUAL .. BUT THERE ARE RELATIONSHIPS IN VARIOUS APPLICATIONS THAT STILL INVALIDLY INHERIT FROM TYPE. See #23917
            //Assert.AreEqual(descendentsViaModel.Count(), descendantsViaCache.Count);   
            Assert.GreaterOrEqual(descendantsViaCache.Count, descendentsViaModel.Count());

            descendantsViaCache.SetEquals(descendentsViaModel.Select(e => e.Id));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetAncestorsAndSelfInvalidTypeTest()
        {
            ISet<long> ancestors = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(-5);
            Assert.IsTrue(ancestors.SetEquals(new List<long> {-5}));
        }


        [Test]
        [RunAsDefaultTenant]
        public void GetDescendantsAndSelfInvalidTypeTest()
        {
            ISet<long> descendants = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(-5);
            Assert.IsTrue(descendants.SetEquals(new List<long> { -5 }));            
        }


        [Test]
        [RunAsDefaultTenant]
        public void TenantIsolationTest()
        {
            long defaultTenantId = RequestContext.TenantId;

            // Get the base resource type
            var resourceTypeDefTen = Entity.Get<EntityType>("core:resource");

            // This belongs to the default tenant
            ISet<long> descendantsDefTenA = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceTypeDefTen.Id);
            ISet<long> ancestorsDefTenA = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(resourceTypeDefTen.Id);

            // Change the tenant context to the global tenant
            SetTenant(0);

            var resourceTypeGlobTen = Entity.Get<EntityType>("core:resource");

            // This belongs to the global tenant
            ISet<long> descendantsGlobTenA = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceTypeGlobTen.Id);
            ISet<long> ancestorsGlobTenA = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(resourceTypeGlobTen.Id);

            // This should only clear the cache for tenant 0
            PerTenantEntityTypeCache.Instance.Clear();

            ISet<long> descendantsGlobTenB = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceTypeGlobTen.Id);
            ISet<long> ancestorsGlobTenB = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(resourceTypeGlobTen.Id);

            // These should be different as the cache should be cleared for tenant 0
            Assert.AreNotSame(descendantsGlobTenA, descendantsGlobTenB);
            Assert.AreNotSame(ancestorsGlobTenA, ancestorsGlobTenB);

            // Change the tenant back to the default one            
            SetTenant(defaultTenantId);

            ISet<long> descendantsDefTenB = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceTypeDefTen.Id);
            ISet<long> ancestorsDefTenB = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(resourceTypeDefTen.Id);

            // These should be the same as the default tenant should remain unaffected
            Assert.AreSame(descendantsDefTenA, descendantsDefTenB);
            Assert.AreSame(ancestorsDefTenA, ancestorsDefTenB);
        }


        /// <summary>
        /// Verifies that creating or deleting and entity type
        /// invalidates the cache.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void ShouldInvalidateTest()
        {
            EntityType newType;

            // Get the base resource type
            var resourceType = Entity.Get<EntityType>("core:resource");
            // Get the report type
            var reportType = Entity.Get<EntityType>("core:report");

            ISet<long> descendantsResource1 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport1 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                // Create a new entity type and save it. 
                newType = new EntityType();
                newType.Inherits.Add(resourceType);
                newType.Name = "PerTenantEntityTypeCache Test";
                // This should invalidate the caches
                newType.Save();

                ctx.CommitTransaction();
            }

            ISet<long> descendantsResource2 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport2 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            Assert.AreNotSame(descendantsResource1, descendantsResource2);
            Assert.AreNotSame(ancestorsReport1, ancestorsReport2);
            Assert.IsFalse(descendantsResource1.SetEquals(descendantsResource2), "The set of descendants should have changed. The cache was not invalidated.");
            Assert.IsTrue(ancestorsReport1.SetEquals(ancestorsReport2), "The set of ancestors should not have changed.");
            Assert.IsTrue(descendantsResource2.Contains(newType.Id), "The set of descendants does not contain the new created type. The cache was not invalidated.");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                // This should also invalidate the cache
                newType.Delete();
                ctx.CommitTransaction();
            }

            ISet<long> descendantsResource3 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport3 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            Assert.AreNotSame(descendantsResource2, descendantsResource3);
            Assert.AreNotSame(ancestorsReport2, ancestorsReport3);
            Assert.IsFalse(descendantsResource2.SetEquals(descendantsResource3), "The set of descendants should have changed. The cache was not invalidated.");
            Assert.IsTrue(ancestorsReport2.SetEquals(ancestorsReport3), "The set of ancestors should not have changed.");
            Assert.IsTrue(!descendantsResource3.Contains(newType.Id), "The set of descendants contains the deleted type. The cache was not invalidated.");
        }


        /// <summary>
        /// Verifies that creating or deleting an entity that is not
        /// and entity type does not invalidate the cache.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void ShouldNotInvalidateTest1()
        {
            Report report;

            // Get the base resource type
            var resourceType = Entity.Get<EntityType>("core:resource");
            // Get the report type
            var reportType = Entity.Get<EntityType>("core:report");

            ISet<long> descendantsResource1 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport1 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                // Create a new entity type and save it. 
                report = new Report();
                report.Name = "PerTenantEntityTypeCache Report Test";
                // This should not invalidate the caches
                report.Save();
                ctx.CommitTransaction();
            }

            ISet<long> descendantsResource2 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport2 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            Assert.AreSame(descendantsResource1, descendantsResource2);
            Assert.AreSame(ancestorsReport1, ancestorsReport2);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                // This should also not invalidate the cache
                report.Delete();
                ctx.CommitTransaction();
            }

            ISet<long> descendantsResource3 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport3 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            Assert.AreSame(descendantsResource2, descendantsResource3);
            Assert.AreSame(ancestorsReport2, ancestorsReport3);
        }


        /// <summary>
        /// Verifies that updating and entity type does not invalidate the cache.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void ShouldNotInvalidateTest2()
        {
            EntityType newType;

            // Get the base resource type
            var resourceType = Entity.Get<EntityType>("core:resource");
            // Get the report type
            var reportType = Entity.Get<EntityType>("core:report");

            ISet<long> descendantsResource1 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport1 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                // Create a new entity type and save it. 
                newType = new EntityType();
                newType.Inherits.Add(resourceType);
                newType.Name = "PerTenantEntityTypeCache Test";
                // This should invalidate the caches
                newType.Save();
                ctx.CommitTransaction();
            }

            ISet<long> descendantsResource2 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport2 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            Assert.AreNotSame(descendantsResource1, descendantsResource2);
            Assert.AreNotSame(ancestorsReport1, ancestorsReport2);
            Assert.IsFalse(descendantsResource1.SetEquals(descendantsResource2), "The set of descendants should have changed. The cache was not invalidated.");
            Assert.IsTrue(ancestorsReport1.SetEquals(ancestorsReport2), "The set of ancestors should not have changed.");
            Assert.IsTrue(descendantsResource2.Contains(newType.Id), "The set of descendants does not contain the new created type. The cache was not invalidated.");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                // This should not invalidate the cache
                newType.Name = "PerTenantEntityTypeCache Test2";
                newType.Save();
                ctx.CommitTransaction();
            }

            ISet<long> descendantsResource3 = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(resourceType.Id);
            ISet<long> ancestorsReport3 = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(reportType.Id);

            Assert.AreSame(descendantsResource2, descendantsResource3);
            Assert.AreSame(ancestorsReport2, ancestorsReport3);            
        }


        /// <summary>
        ///     Sets the tenant.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        private void SetTenant(long tenantId)
        {
            var identity = new IdentityInfo(0, "TestUser");
            var tenant = new TenantInfo(tenantId);

            RequestContext.SetContext(identity, tenant, "en-US");
        }


        /// <summary>
        /// Verify that PerTenantEntityTypeCache.IsInstanceOf works.
        /// </summary>
        [Test]
        [TestCase( null, "core:stringField", false )]
        [TestCase( "core:name", "core:stringField", true )]
        [TestCase( "core:name", "core:field", true )]
        [TestCase( "core:name", "core:report", false )]
        [TestCase( "core:resource", "core:fieldType", false )]
        [RunAsDefaultTenant]
        public void IsInstanceOf( string entity, string type, bool expected )
        {
            IEntity ientity = entity == null ? null : Entity.Get( new EntityRef( entity ) );
            long typeId = (new EntityRef(type)).Id;

            // This should only clear the cache for tenant 0
            bool actual = PerTenantEntityTypeCache.Instance.IsInstanceOf( ientity, typeId );

            Assert.AreEqual( expected, actual );
        }
    }
}