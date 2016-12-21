// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Model;
using System.Collections.Generic;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.EventTarget
{
    public class SendRecordEventTarget : IEntityEventSave
    {
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            return false;
        }

        /// <summary>
        /// Check if there is a pending workflow related to the notification and let it know that a reply was received
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="state"></param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var notifications =
                entities
                    .Select(s => s.As<SendRecord>().SendToNotification)
                    .Where(n => n != null)
                    .Distinct<Notification>(new EntityEqualityComparer());

            notifications.CheckAndResume();
        }


    }

}
