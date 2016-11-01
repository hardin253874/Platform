// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.Security;
using EDC.ReadiNow.Security.SecuredData;
using System;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    ///     This class is responsible for handling Imap Email Provider events.
    /// </summary>
    public class ImapEmailProviderEventTarget : IEntityEventSave
    {
        /// <summary>
        /// The SecureId context used for Imap provider
        /// </summary>
        public const string SecureIdContext = "imapEmailProvider OaPassword";

        #region IEntityEventSave Members


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
            var saveHelper = Factory.SecuredDataSaveHelper;

            IList<IEntity> enumerable = entities as IList<IEntity> ?? entities.ToList();

            foreach (IEntity entity in enumerable)
            {
                saveHelper.OnBeforeSave(SecureIdContext, ImapEmailProvider.OaPassword_Field, ImapEmailProvider.OaPasswordSecureId_Field, entity);        
            }

            return false;
        }

        public bool OnBeforeUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            return false;
        }


        #endregion
    }
}