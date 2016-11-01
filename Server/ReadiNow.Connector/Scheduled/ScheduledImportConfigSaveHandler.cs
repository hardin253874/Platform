// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using System.Collections.Generic;

namespace ReadiNow.Connector.Scheduled
{
    /// <summary>
    /// Handle the transfer of the ftp password to secure storage.
    /// </summary>
    public class ScheduledImportConfigSaveHandler : IEntityEventSave
    {
        const string FtpSecuredContext = "ScheduledImportConfigSaveHandler.SicPassword";

        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var saveHelper = Factory.SecuredDataSaveHelper;

            foreach (var entity in entities)
            {
                saveHelper.OnBeforeSave(FtpSecuredContext, ScheduledImportConfig.SicPassword_Field, ScheduledImportConfig.SicPasswordSecureId_Field, entity);
            }

            return false;
        }

        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // Do nothing
        }


    }
}
