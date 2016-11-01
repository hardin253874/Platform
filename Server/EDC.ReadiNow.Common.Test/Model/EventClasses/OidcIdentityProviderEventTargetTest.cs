// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.EventClasses;
using EDC.Security;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class OidcIdentityProviderEventTargetTest
    {
        [Test]
        public void Password()
        {
            SecuredDataTestHelper.TestField(
                OidcIdentityProvider.OidcIdentityProvider_Type, 
                OidcIdentityProvider.OidcClientSecret_Field.As<Field>(),
                OidcIdentityProvider.OidcClientSecretSecureId_Field.As<Field>());
        }

        [Test]
        public void Upgrade()
        {
            var crypto = new EncodingCryptoProvider();

            SecuredDataTestHelper.TestUpgrade(
                OidcIdentityProvider.OidcIdentityProvider_Type,
                OidcIdentityProvider.OidcClientSecret_Field.As<Field>(),
                OidcIdentityProvider.OidcClientSecretSecureId_Field.As<Field>(),
                (s) => crypto.EncryptAndEncode(s)
                );
        }
    }
}
