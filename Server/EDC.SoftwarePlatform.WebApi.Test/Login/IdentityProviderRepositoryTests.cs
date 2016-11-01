// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Login
{
    /// <summary>
    ///     Class IdentityProviderRepositoryTests.
    /// </summary>
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class IdentityProviderRepositoryTests
    {
        [Test]        
        public void GetIdentityProvidersInvalidTenantTest()
        {
            var idpRepository = new IdentityProviderRepository();
            var providers = idpRepository.GetIdentityProviders(Guid.NewGuid().ToString());

            Assert.IsNotNull(providers, "Providers response should not be null.");
            Assert.AreEqual(0, providers.IdentityProviders.Count, "The providers count is invalid.");
        }

        [Test]        
        public void GetIdentityProvidersNullAndEmptyTenantTest()
        {
            var idpRepository = new IdentityProviderRepository();

            Assert.That(() => idpRepository.GetIdentityProviders(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("tenant"));

            Assert.That(() => idpRepository.GetIdentityProviders(string.Empty),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("tenant"));
        }

        [Test]        
        [TestCase(true)]
        [TestCase(false)]
        [ClearCaches(ClearCachesAttribute.Caches.BulkResultCache)]
        public void GetIdentityProvidersValidTenantTest(bool enabled)
        {
            var oidcProvider1 = new OidcIdentityProvider
            {
                Name = "TestProvider1 " + Guid.NewGuid(),
                IsProviderEnabled = enabled,
                ProviderOrdinal = 100
            };
            oidcProvider1.Save();

            var oidcProvider2 = new OidcIdentityProvider
            {
                Name = "TestProvider2 " + Guid.NewGuid(),
                IsProviderEnabled = enabled,
                ProviderOrdinal = 200
            };
            oidcProvider2.Save();

            var idpRepository = new IdentityProviderRepository();
            var context = RequestContext.GetContext();
            var providers = idpRepository.GetIdentityProviders(context.Tenant.Name);

            Assert.IsNotNull(providers, "Providers response should not be null.");
            Assert.AreEqual(enabled ? 3 : 1, providers.IdentityProviders.Count, "The providers count is invalid.");

            var provider1 = providers.IdentityProviders.FirstOrDefault(i => i.Id == oidcProvider1.Id);
            if (enabled)
            {
                Assert.IsNotNull(provider1, "Provider 1 was not found.");
                Assert.AreEqual(oidcProvider1.Name, provider1.Name, "Provider 1 name is invalid.");
                Assert.AreEqual(oidcProvider1.ProviderOrdinal, provider1.Ordinal, "Provider 1 ordinal is invalid.");
                Assert.AreEqual(oidcProvider1.IsOfType[0].Alias, provider1.TypeAlias, "Provider 1 type alias is invalid.");
            }
            else
            {
                Assert.IsNull(provider1, "Provider 1 was found.");
            }

            var provider2 = providers.IdentityProviders.FirstOrDefault(i => i.Id == oidcProvider2.Id);
            if (enabled)
            {
                Assert.IsNotNull(provider2, "Provider 2 was not found.");
                Assert.AreEqual(oidcProvider2.Name, provider2.Name, "Provider 2 name is invalid.");
                Assert.AreEqual(oidcProvider2.ProviderOrdinal, provider2.Ordinal, "Provider 2 ordinal is invalid.");
                Assert.AreEqual(oidcProvider2.IsOfType[0].Alias, provider2.TypeAlias, "Provider 2 type alias is invalid.");
            }
            else
            {
                Assert.IsNull(provider2, "Provider 2 was found.");
            }

            var readiNowProviderEntity = Entity.Get<ReadiNowIdentityProvider>(WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance);

            var readiNowProvider = providers.IdentityProviders.FirstOrDefault(i => i.Id == readiNowProviderEntity.Id);
            Assert.IsNotNull(readiNowProvider, "ReadiNow provider was not found.");
            Assert.AreEqual(readiNowProviderEntity.Name, readiNowProvider.Name, "ReadiNow provider name is invalid.");
            Assert.AreEqual(readiNowProviderEntity.ProviderOrdinal, readiNowProvider.Ordinal, "ReadiNow provider ordinal is invalid.");
            Assert.AreEqual(readiNowProviderEntity.IsOfType[0].Alias, readiNowProvider.TypeAlias, "ReadiNow provider type alias is invalid.");
        }
    }
}