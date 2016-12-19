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
            {
                var crypto = new EncodingCryptoProvider();

                using (new GlobalAdministratorContext())
                {
                    /* 
                     ImapServerSettings has been moved to the global tenant only. Not sure if we need to move this elsewhere as here it will be run for each tenant
                     */
                    Factory.SecuredDataSaveHelper.UpgradeField(
                        ImapServerSettingsEventTarget.SecureIdContext,
                        ImapServerSettings.ImapServerSettings_Type,
                        ImapServerSettings.ImapPassword_Field,
                        ImapServerSettings.ImapPasswordSecureId_Field,
                        (s) => crypto.DecodeAndDecrypt(s));
                }
                using (new TenantAdministratorContext(tenantName))
                {

                    Factory.SecuredDataSaveHelper.UpgradeField(
                        EmailServerSettingsEventTarget.SecureIdContext,
                        TenantEmailSetting.TenantEmailSetting_Type,
                        TenantEmailSetting.SmtpPassword_Field,
                        TenantEmailSetting.SmtpPasswordSecureId_Field,
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
}
