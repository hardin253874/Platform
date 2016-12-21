// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.Security;
using ReadiNow.Integration.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using EDC.Common;
using EDC.ReadiNow.Diagnostics;

namespace EDC.SoftwarePlatform.Activities.Notify
{
    /// <summary>
    /// Route a notification to Twilio
    /// </summary>
    public class TwilioRouter: INotificationRouter
    {
        const int BatchSize = 20;

        const string MissingPhoneNumberMessage = "The person does not have a mobile phone entry."; 
        public static TwilioRouter Instance { get; } = new TwilioRouter();      // TODO: switch to DI


        public void Send(IEntity notifier, Notification notification, IEnumerable<IEntity> people, bool expectReply)
        {
            Delegates.Batch(people, BatchSize, (batch) =>
            {
                SendBatch(notifier, notification, batch, expectReply);
            });
        }

        /// <summary>
        /// Notify using Twilio
        /// </summary>
        public void SendBatch(IEntity notifier, Notification notification, IEnumerable<IEntity> people, bool expectReply)
        {
            using (Profiler.Measure("TwilioRouter.SendBatch"))
            {
                // Get the right provider
                var tNotifier = notifier.Cast<TwilioNotifier>();
                ISmsProvider smsProvder;
                Action loopbackFlush = null;

                if (tNotifier.TpEnableTestMode ?? false)
                {
                    var lookbackApi = new LoopbackApi(tNotifier, Factory.Current.Resolve<ITwilioSmsReceiver>());
                    loopbackFlush = lookbackApi.GetFlushAction();

                    smsProvder = lookbackApi;
                }
                else
                {
                    smsProvder = Factory.Current.Resolve<ISmsProvider>();
                }


                var accountSid = tNotifier.TpAccountSid;
                var authToken = tNotifier.TpAuthToken;
                var sendingNumber = tNotifier.TpSendingNumber;

                if (String.IsNullOrEmpty(sendingNumber))
                    throw new ArgumentException("Router has no sending number");

                var mobilePhoneField = Factory.ScriptNameResolver.GetInstance("Mobile phone", StringField.StringField_Type.Id);

                IEnumerable<SmsSendRecord> sendRecords = CreateSendRecords(notification, sendingNumber, mobilePhoneField, people);


                // refetch (The is some double handling here, hopefully caching will keep it from being expensive)
                var ids = sendRecords.Select(e => e.Id);

                var reloadedSendRecords = Entity.Get<SmsSendRecord>(ids, writable: true).ToList();

                var phoneable = reloadedSendRecords.Where(r => r.SsrTo != null).ToList();
                var notPhoneable = reloadedSendRecords.Except(phoneable);

                Entity.Save(notPhoneable);

                var numbers = phoneable.Select(r => r.SsrTo); 

                IEnumerable<SendResponse> sendResults;

                using (Profiler.Measure("TwilioRouter.SendBatch SendMessage"))
                {
                    var sendTask = smsProvder.SendMessage(tNotifier.Id, accountSid, authToken, sendingNumber, notification.NMessage, numbers);
                    sendTask.Wait();


                    sendResults = sendTask.Result;
                }

                // Zip the results into the receipts (we can't use the function Zip as it doesn't handle side effects well.)
                // Save as we go. 
                var i = 0;
                foreach (var result in sendResults)
                //for (int i = 0; i < phoneable.Count(); i++)
                {
                    var sendRecord = phoneable[i++];
                    //var result = sendResults[i];

                    sendRecord.SrErrorMessage = result.Success ? null : result.Message;
                    sendRecord.SrSendDate = DateTime.UtcNow;
                    sendRecord.SsrMessageSid = result.MessageSid;
                    sendRecord.Save();
                }

                Entity.Save(reloadedSendRecords);


                // This is a little ugly. We need to ensure the loopback does not send replies until the sendRecords have been created
                if (loopbackFlush != null)
                {
                    using (Profiler.Measure("TwilioRouter.SendBatch Flush"))
                    {
                        loopbackFlush();
                    }
                    
                }
            }
        }


        private static IEnumerable<SmsSendRecord> CreateSendRecords(Notification notification, string sendingNumber, IEntity mobilePhoneField, IEnumerable<IEntity> people)
        {
            using (Profiler.Measure("TwilioRouter.CreateSendRecords"))
            {
                var sendRecords = people.Select(p =>
                {
                    var sendRecord = Entity.Create<SmsSendRecord>();

                    sendRecord.SendToNotification = notification;
                    sendRecord.SsrFrom = PhoneNumberHelper.CleanNumber(sendingNumber);
                    sendRecord.SrToPerson = p?.Cast<Person>();

                    var toNumber = p.GetField<string>(mobilePhoneField);

                    if (toNumber != null)
                        sendRecord.SsrTo = PhoneNumberHelper.CleanNumber(toNumber);
                    else
                    {
                        sendRecord.SrErrorMessage = MissingPhoneNumberMessage;
                    }

                    return sendRecord;
                });

                return sendRecords;
            }
        }
    }
}
