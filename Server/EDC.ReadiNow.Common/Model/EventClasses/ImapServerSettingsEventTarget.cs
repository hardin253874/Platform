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
    public class ImapServerSettingsEventTarget : IEntityEventSave
    {
        public const string SecureIdContext = "ImapServerSettings ImapPassword";

        #region IEntityEventSave Members


        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
        }


        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var saveHelper = Factory.SecuredDataSaveHelper;
            var list = entities as IList<IEntity> ?? entities.ToList();
            foreach (var entity in list)
            {
                saveHelper.OnBeforeSave(SecureIdContext, ImapServerSettings.ImapPassword_Field, ImapServerSettings.ImapPasswordSecureId_Field, entity);        
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