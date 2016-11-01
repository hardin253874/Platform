// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.Security;
using System;
using System.Linq;
using static EDC.Security.CryptoHelper;

namespace EDC.ReadiNow.Security.SecuredData
{
    /// <summary>
    /// A helper class to remove some of the boilerplate code around storing sensitive information in the entity model
    /// </summary>
    public class SecuredDataSaveHelper : ISecuredDataSaveHelper
    {
        private static readonly string EmptyStringReplacement = "This is an empty string {FCDEEDD9-0ACD-4415-9350-9F2B3E826008}";
        private static readonly string EmptyStringHash = CryptoHelper.CreateEncodedSaltedHash(EmptyStringReplacement);

        private ISecuredData _securedData;

        /// <summary>
        ///     Constructor
        /// </summary>
        public SecuredDataSaveHelper(ISecuredData securedData)
        {
            _securedData = securedData;
        }

        /// <summary>
        ///     OnBeforeSave handler. This taskes the value in the data field and pushed into secureData. It then replace the field value with a hash.
        /// </summary>
        /// <param name="securedDataContext">The context to store the secure data under</param>
        /// <param name="dataField">The data field on the entity</param>
        /// <param name="secureIdField">The secureId field on the entity</param>
        /// <param name="entity">The entity being updated</param>
        public void OnBeforeSave(string securedDataContext, EntityRef dataField, EntityRef secureIdField, IEntity entity)
        {
            if (securedDataContext == null)
                throw new ArgumentNullException(nameof(securedDataContext));

            if (dataField == null)
                throw new ArgumentNullException(nameof(dataField));

            if (secureIdField == null)
                throw new ArgumentNullException(nameof(secureIdField));


            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var isCreate = entity.IsTemporaryId;

            var secureId = entity.GetField<Guid?>(secureIdField);

            var oldPassword = Entity.Get(entity.Id).GetField<string>(dataField);
            var newPassword = entity.GetField<String>(dataField);

            if (!isCreate && (oldPassword == newPassword))
                return; // nothing has changed.

            if (newPassword != null)
            {
                var hash = CreateHash(newPassword);

                // Only update if the hash has changed
                if (isCreate || (hash != oldPassword))
                {
                    // Transfer the password to secure storage
                    if (secureId == null && newPassword != null)
                    {
                        // There is a window for the save to fail leaving us with an orphed secure record, but it's a low risk for a low volume field.
                        secureId = _securedData.Create(RequestContext.TenantId, securedDataContext, newPassword);
                        entity.SetField(secureIdField, secureId);
                    }
                    else
                    {
                        _securedData.Update((Guid)secureId, newPassword);
                    }
                }

                entity.SetField(dataField, hash);       // Always set the field so that no save will occur if the password hasn't changed.
            }
            else
            {
                // Clear the secureId field
                if (secureId != null)
                {
                    _securedData.Delete((Guid)secureId);
                    entity.SetField(secureIdField, null);
                }
            }
        }

        /// <summary>
        ///     Move all the passwords into secure storage, replacing the data field with the default value.
        /// </summary>
        /// <param name="securedDataContext">The secure context to store the value against.</param>
        /// <param name="entityType">The type of the entity to upgrade.</param>
        /// <param name="dataField">The field containing the data.</param>
        /// <param name="secureIdField">Teh field containing the secureId</param>
        /// <param name="decryptor">Function to decrypt the stored value</param>
        /// <param name="defaultValue">The value to put in the data field after upgrading. Only necessary if the field is mandatory.</param>
        public void UpgradeField(string securedDataContext, EntityType entityType, EntityRef dataField, EntityRef secureIdField, Func<string, string> decryptor)
        {
            var instances = Entity.GetInstancesOfType(entityType);  

            var toSave = instances
                .Where(i => i.GetField(secureIdField) == null && i.GetField(dataField) != null);

            // Stop the normal save helper code running as we have already dealt with the update
            using (var scope = Factory.Current.BeginLifetimeScope(builder => builder.RegisterType<DisabledSaveHelper>().As<ISecuredDataSaveHelper>()))
            using (Factory.SetCurrentScope(scope))
            {
                EventLog.Application.WriteInformation($"Upgrading {toSave.Count()} instances of {entityType.Name} to SecureId.");

                foreach (var entity in toSave)
                {
                    var we = entity.AsWritable();

                    var encryptedPw = entity.GetField<string>(dataField);
                    var password = decryptor(encryptedPw);

                    if (password != null)
                    {
                        var secureId = _securedData.Create(RequestContext.TenantId, securedDataContext, password);
                        we.SetField(secureIdField, secureId);
                    }

                    we.SetField(dataField, CreateHash(password));
                    we.Save();
                }
            }

        }

        /// <summary>
        /// Create a hash that includes hashing an empty string. (The normal hasher does not.)
        /// </summary>
        /// <param name="s">The string to hash</param>
        /// <returns>The hashed string.</returns>
        string CreateHash(string s)
        {
            return s != String.Empty ? CryptoHelper.CreateEncodedSaltedHash(s) : EmptyStringHash;
        }

        /// <summary>
        /// This is an alternative save help that is used when doing upgrades to ensure the normal save behaviour is bypassed.
        /// </summary>
        public class DisabledSaveHelper : ISecuredDataSaveHelper
        {
            public void UpgradeField(string securedDataContext, EntityType entityType, EntityRef dataField, EntityRef secureIdField, Func<string, string> decrytor)
            {
            }

            public void OnBeforeSave(string securedDataContext, EntityRef dataField, EntityRef secureIdField, IEntity entity)
            {
            }
        }
    }
}
