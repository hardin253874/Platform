// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
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
    [RunAsGlobalTenant]
    [RunWithTransaction]
    public class ImapServerSettingsEventTargetTest
    {
        [Test]
        public void Password()
        {
            SecuredDataTestHelper.TestField(
                ImapServerSettings.ImapServerSettings_Type,
                ImapServerSettings.ImapPassword_Field.As<Field>(),
                ImapServerSettings.ImapPasswordSecureId_Field.As<Field>());
        }

        [Test]
        public void Upgrade()
        {
            var crypto = new EncodingCryptoProvider();

            SecuredDataTestHelper.TestUpgrade(
                ImapServerSettings.ImapServerSettings_Type,
                ImapServerSettings.ImapPassword_Field.As<Field>(),
                ImapServerSettings.ImapPasswordSecureId_Field.As<Field>(),
                (s) => crypto.EncryptAndEncode(s)
            );
        }

    }
}
