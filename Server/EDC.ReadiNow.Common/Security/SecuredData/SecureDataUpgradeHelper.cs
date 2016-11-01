// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.EventClasses;
using EDC.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Security.SecuredData
{
    /// <summary>
    /// Used to move over fields into SecureId
    /// </summary>
    public static class SecureDataUpgradeHelper
    {
        /// <summary>
        /// Called after upgrade.
        /// </summary>
        public static void Upgrade(string tenantName)
        {
            using (new SecurityBypassContext())
            using (new TenantAdministratorContext(tenantName))
            {
                var crypto = new EncodingCryptoProvider();

                // ImapEmailProvider
                Factory.SecuredDataSaveHelper.UpgradeField(
                    ImapEmailProviderEventTarget.SecureIdContext,
                    ImapEmailProvider.ImapEmailProvider_Type,
                    ImapEmailProvider.OaPassword_Field,
                    ImapEmailProvider.OaPasswordSecureId_Field,
                    (s) => crypto.DecodeAndDecrypt(s));

                // identityProvider OidcClientSecret
                Factory.SecuredDataSaveHelper.UpgradeField(
                    OidcIdentityProviderEventTarget.SecureIdContext,
                    OidcIdentityProvider.OidcIdentityProvider_Type,
                    OidcIdentityProvider.OidcClientSecret_Field,
                    OidcIdentityProvider.OidcClientSecretSecureId_Field,
                    (s) => crypto.DecodeAndDecrypt(s)
                    );
            }
        }
       
    }
}
