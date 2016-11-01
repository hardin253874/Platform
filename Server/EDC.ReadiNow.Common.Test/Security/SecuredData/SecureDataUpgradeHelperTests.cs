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
        public void ImapUpgrade()
        {
            var imap = new ImapEmailProvider();
            var oldSecret =  "Test" + DateTime.UtcNow.Ticks;
            imap.OaPassword = _crypto.EncryptAndEncode(oldSecret);

            using (var scope = Factory.Current.BeginLifetimeScope(builder => builder.RegisterType<SecuredDataSaveHelper.DisabledSaveHelper>().As<ISecuredDataSaveHelper>()))
            using (Factory.SetCurrentScope(scope))
            {
                imap.Save();
            }

            SecureDataUpgradeHelper.Upgrade("EDC");

            var secureId = imap.OaPasswordSecureId;
            Assert.That(secureId, Is.Not.Null);
            var secret = Factory.SecuredData.Read((Guid) secureId);

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
