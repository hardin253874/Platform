// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class IdentityProviderContextCacheTests
    {
        [Test]
        [RunWithTransaction]
        public void Constructor()
        {
            var cache = new IdentityProviderContextCache();
            Assert.IsNotNull(cache, "Cache should not be null.");
        }


        [Test]
        [RunWithTransaction]
        [TestCase(0)]
        [TestCase(500000)]
        public void GetValidRequestContextFromCache_InvalidProvider(long providerId)
        {
            var userAccount = new UserAccount {Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active};
            userAccount.Save();

            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(RequestContext.TenantId, providerId, userAccount.Name, true);

            Assert.IsNull(requestContextData, "Context data should be null");
        }

        [Test]
        [RunWithTransaction]
        [TestCase(0)]
        public void GetValidRequestContextFromCache_ZeroTenant(long tenantId)
        {
            var userAccount = new UserAccount {Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active };
            userAccount.Save();

            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(tenantId, WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, userAccount.Name, true);

            Assert.IsNull(requestContextData, "Context data should be null");
        }

        [Test]
        [RunWithTransaction]
        [TestCase( 500000 )]
        [ExpectedException]
        public void GetValidRequestContextFromCache_InvalidTenant( long tenantId )
        {
            var userAccount = new UserAccount { Name = "Test User " + Guid.NewGuid( ), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active };
            userAccount.Save( );

            var cache = new IdentityProviderContextCache( );
            cache.GetRequestContextData( tenantId, WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, userAccount.Name, true );
        }


        [Test]
        [RunWithTransaction]
        [TestCase("")]
        [TestCase(null)]
        public void GetValidRequestContextFromCache_InvalidUserName(string userName)
        {
            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(RequestContext.TenantId, WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, userName, true);

            Assert.IsNull(requestContextData, "Context data should be null");
        }

        [Test]
        [RunWithTransaction]
        public void GetValidRequestContextFromCache_OidcProvider()
        {
            var entitiesToSave = new List<IEntity>();

            // Setup provider, provider user and user account
            var provider = new OidcIdentityProvider {Name = "ID Provider " + Guid.NewGuid()};
            entitiesToSave.Add(provider);

            var userAccount = new UserAccount {Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active };
            entitiesToSave.Add(userAccount);

            var providerUser = new OidcIdentityProviderUser
            {
                AssociatedUserAccount = userAccount,
                Name = "Provider User" + Guid.NewGuid(),
                IdentityProviderForUser = provider.As<IdentityProvider>()
            };
            entitiesToSave.Add(providerUser);

            Entity.Save(entitiesToSave);

            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(RequestContext.TenantId, provider.Id, providerUser.Name, true);

            Assert.IsNotNull(requestContextData, "Context data should not be null.");
            Assert.AreEqual(userAccount.Id, requestContextData.Identity.Id, "The context identity user id is invalid.");
            Assert.AreEqual(userAccount.Name, requestContextData.Identity.Name, "The context identity user name is invalid.");
            Assert.AreEqual(provider.Id, requestContextData.Identity.IdentityProviderId, "The context identity provider is invalid.");
            Assert.AreEqual(providerUser.Id, requestContextData.Identity.IdentityProviderUserId, "The context identity provider user id is invalid.");
            Assert.AreEqual(provider.IsOfType[0].Alias, requestContextData.Identity.IdentityProviderTypeAlias, "The context identity provider alias is invalid.");
        }


        [Test]
        [RunWithTransaction]
        public void GetValidRequestContextFromCache_OidcProviderInvalidateOnUserChange()
        {
            var entitiesToSave = new List<IEntity>();

            // Setup provider, provider user and user account
            var provider = new OidcIdentityProvider {Name = "ID Provider " + Guid.NewGuid()};
            entitiesToSave.Add(provider);

            var userAccount = new UserAccount {Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active };
            entitiesToSave.Add(userAccount);

            var providerUser = new OidcIdentityProviderUser
            {
                AssociatedUserAccount = userAccount,
                Name = "Provider User" + Guid.NewGuid(),
                IdentityProviderForUser = provider.As<IdentityProvider>()
            };
            entitiesToSave.Add(providerUser);

            Entity.Save(entitiesToSave);

            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(RequestContext.TenantId, provider.Id, providerUser.Name, true);

            Assert.IsNotNull(requestContextData, "Context data should not be null.");
            Assert.AreEqual(userAccount.Id, requestContextData.Identity.Id, "The context identity user id is invalid.");
            Assert.AreEqual(userAccount.Name, requestContextData.Identity.Name, "The context identity user name is invalid.");
            Assert.AreEqual(provider.Id, requestContextData.Identity.IdentityProviderId, "The context identity provider is invalid.");
            Assert.AreEqual(providerUser.Id, requestContextData.Identity.IdentityProviderUserId, "The context identity provider user id is invalid.");
            Assert.AreEqual(provider.IsOfType[0].Alias, requestContextData.Identity.IdentityProviderTypeAlias, "The context identity provider alias is invalid.");

            // This should invalidate the cache
            providerUser.AssociatedUserAccount = null;
            providerUser.Save();

            cache.CacheInvalidator.OnEntityChange(new IEntity[] {providerUser}, InvalidationCause.Save, null);

            requestContextData = cache.GetRequestContextData(RequestContext.TenantId, provider.Id, providerUser.Name, true);
            Assert.IsNull(requestContextData, "Context data should be null.");
        }


        [Test]
        [RunWithTransaction]
        public void GetValidRequestContextFromCache_ReadiNowProvider()
        {
            var userAccount = new UserAccount {Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active };
            userAccount.Save();

            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(RequestContext.TenantId, WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, userAccount.Name, true);

            var identityProvider = Entity.Get<ReadiNowIdentityProvider>(WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance);

            Assert.IsNotNull(requestContextData, "Context data should not be null.");
            Assert.AreEqual(userAccount.Id, requestContextData.Identity.Id, "The context identity user id is invalid.");
            Assert.AreEqual(userAccount.Name, requestContextData.Identity.Name, "The context identity user name is invalid.");
            Assert.AreEqual(WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, requestContextData.Identity.IdentityProviderId,
                "The context identity provider is invalid.");
            Assert.AreEqual(userAccount.Id, requestContextData.Identity.IdentityProviderUserId, "The context identity provider user id is invalid.");
            Assert.AreEqual(identityProvider.IsOfType[0].Alias, requestContextData.Identity.IdentityProviderTypeAlias, "The context identity provider alias is invalid.");
        }

        [Test]
        [RunWithTransaction]
        public void GetValidRequestContextFromCache_UnmappedOidcUser()
        {
            var entitiesToSave = new List<IEntity>();

            // Setup provider, provider user and user account
            var provider = new OidcIdentityProvider {Name = "ID Provider " + Guid.NewGuid()};
            entitiesToSave.Add(provider);

            var userAccount = new UserAccount {Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active };
            entitiesToSave.Add(userAccount);

            // Provider user has no associated user account
            var providerUser = new OidcIdentityProviderUser
            {
                Name = "Provider User" + Guid.NewGuid(),
                IdentityProviderForUser = provider.As<IdentityProvider>()
            };
            entitiesToSave.Add(providerUser);

            Entity.Save(entitiesToSave);

            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(RequestContext.TenantId, provider.Id, providerUser.Name, true);

            Assert.IsNull(requestContextData, "Context data should be null");
        }

        [Test]
        [RunWithTransaction]
        [TestCase(true)]
        [TestCase(false)]
        public void GetValidRequestContextFromCache_DisabledUserAccount(bool ensureActive)
        {
            var entitiesToSave = new List<IEntity>();

            // Setup provider, provider user and user account
            var provider = new OidcIdentityProvider { Name = "ID Provider " + Guid.NewGuid() };
            entitiesToSave.Add(provider);

            var userAccount = new UserAccount { Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Disabled };
            entitiesToSave.Add(userAccount);

            // Provider user has no associated user account
            var providerUser = new OidcIdentityProviderUser
            {
                AssociatedUserAccount = userAccount,
                Name = "Provider User" + Guid.NewGuid(),
                IdentityProviderForUser = provider.As<IdentityProvider>()
            };
            entitiesToSave.Add(providerUser);

            Entity.Save(entitiesToSave);

            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(RequestContext.TenantId, provider.Id, providerUser.Name, ensureActive);

            if (ensureActive)
            {
                Assert.IsNull(requestContextData, "Context data should be null");
            }
            else
            {
                Assert.IsNotNull(requestContextData, "Context data should not be null");
            }
        }

        [Test]
        [RunWithTransaction]        
        public void GetValidRequestContextFromCache_DisabledToActiveUserAccount()
        {
            var entitiesToSave = new List<IEntity>();

            // Setup provider, provider user and user account
            var provider = new OidcIdentityProvider { Name = "ID Provider " + Guid.NewGuid() };
            entitiesToSave.Add(provider);

            var userAccount = new UserAccount { Name = "Test User " + Guid.NewGuid(), AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Disabled };
            entitiesToSave.Add(userAccount);

            // Provider user has no associated user account
            var providerUser = new OidcIdentityProviderUser
            {
                AssociatedUserAccount = userAccount,
                Name = "Provider User" + Guid.NewGuid(),
                IdentityProviderForUser = provider.As<IdentityProvider>()
            };
            entitiesToSave.Add(providerUser);

            Entity.Save(entitiesToSave);

            var cache = new IdentityProviderContextCache();
            var requestContextData = cache.GetRequestContextData(RequestContext.TenantId, provider.Id, providerUser.Name, false);
            
            Assert.IsNotNull(requestContextData, "Context data should not be null");

            requestContextData = cache.GetRequestContextData(RequestContext.TenantId, provider.Id, providerUser.Name, true);

            Assert.IsNull(requestContextData, "Context data should be null");

            // Change the account to active
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Save();

            cache.CacheInvalidator.OnEntityChange(new IEntity[] { userAccount }, InvalidationCause.Save, null);

            requestContextData = cache.GetRequestContextData(RequestContext.TenantId, provider.Id, providerUser.Name, true);

            Assert.IsNotNull(requestContextData, "Context data should not be null");
        }
    }
}