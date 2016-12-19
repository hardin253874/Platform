// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.SecuredData;
using EDC.Security;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Security.SecuredData
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class SecureDataUpgradeHelperTests
    {
        EncodingCryptoProvider _crypto = new EncodingCryptoProvider();

        [Test]
        public void TenantInboxesSettingsUpgrade()
        {
            //throw new Exception("TODO");
            /*var tenantInboxesSettings = new TenantInboxesSettings();
            var oldSecret =  "Test" + DateTime.UtcNow.Ticks;
            tenantInboxesSettings.ImapPassword = _crypto.EncryptAndEncode(oldSecret);

            using (var scope = Factory.Current.BeginLifetimeScope(builder => builder.RegisterType<SecuredDataSaveHelper.DisabledSaveHelper>().As<ISecuredDataSaveHelper>()))
            using (Factory.SetCurrentScope(scope))
            {
                tenantInboxesSettings.Save();
            }

            SecureDataUpgradeHelper.Upgrade("EDC");

            var secureId = tenantInboxesSettings.ImapPasswordSecureId;
            Assert.That(secureId, Is.Not.Null);
            var secret = Factory.SecuredData.Read((Guid) secureId);

            Assert.That(secret, Is.EqualTo(oldSecret));
            */
        }

        [Test]
        public void TenantEmailServerSettingUpgrade()
        {
            var emailSettings = new TenantEmailSetting();
            var oldSecret = "Test" + DateTime.UtcNow.Ticks;
            emailSettings.SmtpPassword = _crypto.EncryptAndEncode(oldSecret);

            using (var scope = Factory.Current.BeginLifetimeScope(builder => builder.RegisterType<SecuredDataSaveHelper.DisabledSaveHelper>().As<ISecuredDataSaveHelper>()))
            using (Factory.SetCurrentScope(scope))
            {
                emailSettings.Save();
            }

            SecureDataUpgradeHelper.Upgrade("EDC");

            var secureId = emailSettings.SmtpPasswordSecureId;
            Assert.That(secureId, Is.Not.Null);
            var secret = Factory.SecuredData.Read((Guid)secureId);

            Assert.That(secret, Is.EqualTo(oldSecret));
        }

        [Test]
        public void OAuthUpgrade()
        {
            var oip = new OidcIdentityProvider();
            var oldSecret = "Test" + DateTime.UtcNow.Ticks;
            oip.OidcClientSecret = _crypto.EncryptAndEncode(oldSecret);

            using (var scope = Factory.Current.BeginLifetimeScope(builder => builder.RegisterType<SecuredDataSaveHelper.DisabledSaveHelper>().As<ISecuredDataSaveHelper>()))
            using (Factory.SetCurrentScope(scope))
            {
                oip.Save();
            }

            SecureDataUpgradeHelper.Upgrade("EDC");

            var secureId = oip.OidcClientSecretSecureId;
            Assert.That(secureId, Is.Not.Null);
            var secret = Factory.SecuredData.Read((Guid)secureId);

            Assert.That(secret, Is.EqualTo(oldSecret));
        }
    }
}
