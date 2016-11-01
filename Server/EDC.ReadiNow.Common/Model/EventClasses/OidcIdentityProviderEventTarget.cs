// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.Security;
using EDC.ReadiNow.Security.SecuredData;
using System;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    ///     This class is responsible for handling OpenId Connect Identity Provider events.
    /// </summary>
    public class OidcIdentityProviderEventTarget : IEntityEventSave
    {
        /// <summary>
        /// Context to use for Secure storage
        /// </summary>
        public const string SecureIdContext = "identityProvider OidcClientSecret"; 

        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
        }

        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var enumerable = entities as IList<IEntity> ?? entities.ToList();

            var saveHelper = Factory.SecuredDataSaveHelper;

            foreach (var entity in enumerable)
            {
                // Encode and encrypt the client secret                              
                saveHelper.OnBeforeSave(SecureIdContext, OidcIdentityProvider.OidcClientSecret_Field, OidcIdentityProvider.OidcClientSecretSecureId_Field, entity);
            }

            return false;
        }

    }
}