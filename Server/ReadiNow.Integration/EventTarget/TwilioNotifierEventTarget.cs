// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using ReadiNow.Integration.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadiNow.Integration.EventTarget
{
    public class TwilioNotifierEventTarget : IEntityEventSave
    {
        const string RegisterListKey = "TwilioNotifierEventTarget.RegisterList";
        const string DeregisterListKey = "TwilioNotifierEventTarget.DeregisterList";


        /// <summary>
        /// Find out which notifiers need to be registered and deregistered.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var watched = new List<string> { "core:tpAccountSid", "core:tpAuthToken", "core:tpSendingNumber"};
            var watchedRefs = watched.Select(r => new EntityRef(r));
            
            var registerList = new List<IEntity>();
            var deregisterList = new List<IEntity>();

            foreach (var entity in entities)
            {
                var testMode = entity.GetField<bool?>("core:tpEnableTestMode") ?? false;
                var testModeChanged = entity.HasChanges(new EntityRef("core:tpEnableTestMode").ToEnumerable());

                // Check if we were already in test mode. If so don't need to do anything
                if (testMode && (entity.IsTemporaryId || !testModeChanged))
                    continue;

                if (entity.HasChanges(watchedRefs) || testModeChanged)
                {
                    if (testMode)
                    {
                        if (!entity.IsTemporaryId)
                            deregisterList.Add(entity);
                    }
                    else
                    {
                        registerList.Add(entity);
                    }
                }
            }

            state.Add(RegisterListKey, registerList);
            state.Add(DeregisterListKey, deregisterList);

            return false;
        }

        /// <summary>
        /// Register and deregister the update notifiers. This occurs outside the transaction as we are relying on an outside service to complete the
        /// request. We should consider pushing this into a background service.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="state"></param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var smsReceiver = Factory.Current.Resolve<ITwilioSmsReceiver>();

            var registerList = (List<IEntity>) state[RegisterListKey];
            var deregisterList = (List<IEntity>) state[DeregisterListKey];

            foreach (var entity in registerList)
            {
                RegisterHandler(entity.Id);
            }

            foreach (var entity in deregisterList)
            {
               DeregisterHandler(entity.Id);
            }
        }


        /// <summary>
        /// Register a notifier with Twilio
        /// </summary>
        /// <param name="notifierId"></param>
        public void RegisterHandler(long notifierId)
        {
            var notifier = Entity.Get<TwilioNotifier>(notifierId);

            if (notifier == null)
                throw new ArgumentException("Provided id is not a TwilioNotifier.", nameof(notifierId));

            var smsProvider = Factory.Current.Resolve<ISmsProvider>();

            smsProvider.RegisterUrlForIncomingSms(notifier.TpAccountSid, notifier.TpAuthToken, notifier.TpSendingNumber, notifierId);
        }

        /// <summary>
        /// Degregister a notifier
        /// </summary>
        /// <param name="notifierId"></param>
        public void DeregisterHandler(long notifierId)
        {
            var notifier = Entity.Get<TwilioNotifier>(notifierId);

            if (notifier == null)
                throw new ArgumentException("Provided id is not a TwilioNotifier.", nameof(notifierId));

            var smsProvider = Factory.Current.Resolve<ISmsProvider>();

            smsProvider.DeregisterUrlForIncomingSms(notifier.TpAccountSid, notifier.TpAuthToken, notifier.TpSendingNumber);
        }
    }
}
