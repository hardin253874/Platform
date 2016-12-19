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
    public class InboxEventTarget : IEntityEventSave
    {

        #region IEntityEventSave Members


        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
        }

        string CreateUniqueEmailRecipient(string name)
        {
            var cleanName = "";
            name.ToList<char>().ForEach(c =>
            {
                if (char.IsLetterOrDigit(c))
                    cleanName += c;
            });

            var rnd = new Random().Next(int.MaxValue);
            return $"{cleanName}{rnd}";
        }


        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            string emailDomain = null;
            using (new GlobalAdministratorContext())
            {
                 var imapSettings = Entity.Get<ImapServerSettings>("core:imapServerSettingsInstance");
                emailDomain = imapSettings.InboxEmailAddressDomain;
            }

                var saveHelper = Factory.SecuredDataSaveHelper;
            var list = entities as IList<IEntity> ?? entities.ToList();
            foreach (var entity in list)
            {
                var inbox = entity.As<Inbox>();

                //Create a new unique email address for this inbox
                if (string.IsNullOrEmpty(inbox.InboxEmailAddress))
                {
                    
                    if (!string.IsNullOrEmpty(emailDomain))
                    {
                        inbox.InboxEmailAddress = CreateUniqueEmailRecipient(inbox.Name) + "@" + emailDomain;
                    }
                }

                // By default add the match sent to received email action
                if (!inbox.InboxEmailActions.Any(x => x.Alias == "core:matchSentToReceivedEmailsAction"))
                {
                    var action = Entity.Get<InboxEmailAction>("core:matchSentToReceivedEmailsAction");
                    inbox.InboxEmailActions.Add(action);
                }
                
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