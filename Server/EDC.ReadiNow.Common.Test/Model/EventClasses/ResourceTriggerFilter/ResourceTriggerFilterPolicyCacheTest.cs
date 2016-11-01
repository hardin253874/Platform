// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Model.EventClasses.ResourceTriggerFilter
{
    [RunAsDefaultTenant]
    public class ResourceTriggerFilterPolicyCacheTest
    {

        [Test]
        public void IsRegistered()
        {
            Assert.That(Factory.ResourceTriggerFilterPolicyCache, Is.Not.Null);
        }

        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void AddPolicy(bool isEnabled, bool isTypeSet, bool expectedAdded)
        {
            var policyCache = new ResourceTriggerFilterPolicyCache(new List<IFilteredTargetHandlerFactory>());

            ResourceTriggerFilterDef policy = CreateDummyPolicy();
            policy.TriggerEnabled = isEnabled;

            if (!isTypeSet)
                policy.TriggeredOnType = null;

            policyCache.UpdateOrAddPolicy(policy);

            if (expectedAdded)
            {
                Assert.That(policyCache.PolicyToFieldsMap.ContainsKey(policy.Id), Is.True);
                Assert.That(policyCache.TypePolicyMap.ContainsKey(policy.TriggeredOnType.Id), Is.True);
            }
            else
            {
                Assert.That(policyCache.PolicyToFieldsMap, Is.Empty);
                Assert.That(policyCache.TypePolicyMap, Is.Empty);
            }
        }



        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void CacheCleanedWhenPolicyRemoved()
        {
            ResourceTriggerFilterDef policy = CreateDummyPolicy();

            var policyCache = new ResourceTriggerFilterPolicyCache(new List<IFilteredTargetHandlerFactory>());

            policyCache.UpdateOrAddPolicy(policy);
            policyCache.RemovePolicy(policy.Id);

            Assert.That(policyCache.PolicyToFieldsMap.ContainsKey(policy.Id), Is.False);

            var policies = policyCache.TypePolicyMap[policy.TriggeredOnType.Id];

            Assert.That(policies.FirstOrDefault( p => p.Id == policy.Id), Is.Null);
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void CacheCleanedWhenPolicyDisabled()
        {
            ResourceTriggerFilterDef policy = CreateDummyPolicy();

            var policyCache = new ResourceTriggerFilterPolicyCache(new List<IFilteredTargetHandlerFactory>());

            policyCache.UpdateOrAddPolicy(policy);

            policy = policy.AsWritable<ResourceTriggerFilterDef>();
            policy.TriggerEnabled = false;

            policyCache.UpdateOrAddPolicy(policy);

            Assert.That(policyCache.PolicyToFieldsMap.ContainsKey(policy.Id), Is.False);

            var policies = policyCache.TypePolicyMap[policy.TriggeredOnType.Id];

            Assert.That(policies.FirstOrDefault(p => p.Id == policy.Id), Is.Null);
        }

        private static ResourceTriggerFilterDef CreateDummyPolicy()
        {
            var dummyPolicyType = Entity.Create<EntityType>();
            dummyPolicyType.Inherits.Add(Entity.Get<EntityType>("core:resourceTriggerFilterDef"));
            dummyPolicyType.Save();

            var dummyType = Entity.Create<EntityType>();
            dummyType.Save();

            var policy = Entity.Create(dummyPolicyType.Id).As<ResourceTriggerFilterDef>();
            policy.TriggerEnabled = true;
            policy.TriggeredOnType = dummyType;
            return policy;
        }
    }
}
