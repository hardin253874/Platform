// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Diagnostics;
using System.Threading;
using EDC.ReadiNow.Core;
using Autofac;
using System.Linq;
using EDC.ReadiNow.IO;

namespace ReadiNow.Integration.Sms
{
    /// <summary>
    ///     A provider that just echos a reply to any message directly to the incomming URL
    /// </summary>
    public class LoopbackApi : ISmsProvider
    {
        TwilioNotifier _notifier;
        ITwilioSmsReceiver _receiver;
        List<TwilioSms> _replies;

        /// <summary>
        /// If a message is sent to this number, it will always fail.
        /// </summary>
        public const string FailNumber = "1800666";

        /// <summary>
        /// If a message is sent to this number, it will always succeed.
        /// </summary>
        public const string SucceedNumber = "1800111";

        public string FailMessage = "send failed";
        public LoopbackApi(TwilioNotifier notifier, ITwilioSmsReceiver receiver)
        {
            _notifier = notifier;
            _receiver = receiver;
            _replies = new List<TwilioSms>();
        }

      
        /// <summary>
        /// Simulate Sending a message
        /// </summary>
        /// <returns>The responses</returns>
        public async Task<IEnumerable<SendResponse>> SendMessage(long notifierId, string accountSid, string authToken, string sendingNumber, string message, IEnumerable<string> numbers)
        {
            Task<IEnumerable<SendResponse>> result;

            Func<IEnumerable<SendResponse>> fn = () =>
            {
                return SendMessageSync(notifierId, sendingNumber, message, numbers);
            };

            result = new Task<IEnumerable<SendResponse>>(fn, TaskCreationOptions.AttachedToParent);
            result.Start();

            return await result;
        }

        /// <summary>
        /// Send a message syncronously
        /// </summary>
        /// <returns>The messages</returns>
        public IEnumerable<SendResponse> SendMessageSync(long notifierId, string sendingNumber, string message, IEnumerable<string> numbers)
        {
            var cleanFailNumber = PhoneNumberHelper.CleanNumber(FailNumber);
            var statusUrl = _receiver.GetStatusUpdateUrl(notifierId);
            var incommingUrl = _receiver.GetIncommingUrl(_notifier.Id);

            var reply = $"Reply:{message}";

            var result = new List<SendResponse>();

            foreach (var number in numbers)
            {
                //TODO: Emulate status updating

                bool isFailNumber = number == cleanFailNumber;

                var messageSid = Guid.NewGuid().ToString();

                if (!isFailNumber)
                {
                    var twilioSms = new TwilioSms
                    {
                        MessageSid = messageSid,
                        AccountSid = _notifier.TpAccountSid,
                        From = PhoneNumberHelper.CleanNumber(number),
                        To = PhoneNumberHelper.CleanNumber(sendingNumber),
                        Body = $"TestMode:Reply:{ResponseFromNumber(number)}"
                    };

                    _replies.Add(twilioSms);

                }

                var sendResponse = new SendResponse
                {
                    Number = number,
                    MessageSid = messageSid,
                    Success = !isFailNumber,
                    Message = isFailNumber ? FailMessage : null
                };

                result.Add(sendResponse);
            }

            return result;
        }

        /// <summary>
        /// Get an action that will flush any pending replies.
        /// </summary>
        public Action GetFlushAction()
        {
            return () => Flush();
        }

        
        /// <summary>
        /// Flush any pending replies
        /// </summary>
        void Flush()
        {
            // All the requests run in the same context as the TwilioController.
            using( new TenantAdministratorContext(RequestContext.TenantId))
            {
                lock (_replies)
                {

                    if (_replies.Any())
                    {
                        foreach (var sms in _replies)
                        {
                            _receiver.HandleStatusUpdate(_notifier.Id, sms.MessageSid, "delivered", null);
                        }

                        foreach (var sms in _replies)
                        {
                            _receiver.HandleStatusUpdate(_notifier.Id, sms.MessageSid, "sent", null);
                        }

                        foreach (var sms in _replies)
                        {
                            _receiver.HandleRequest(_notifier.Id, sms);
                        }

                        _replies.Clear();
                    }
                }
            }
        }


        string ResponseFromNumber(string number)
        {
            switch(number?[number.Length - 1])
            {
                case '1': return "1";
                case '2': return "2";
                case '3': return "3";
                case '4': return "4";
                case '5': return "5";
                case '6': return "yes";
                case '7': return "no";
                case '8': return "approve";
                case '9': return "reject";

                default: return "cancel";
            }
        }

        /// <summary>
        /// Register a notifier to 
        /// </summary>
        /// <param name="accountSid"></param>
        /// <param name="authToken"></param>
        /// <param name="receivingNumber"></param>
        /// <param name="notifierId"></param>
        public virtual void RegisterUrlForIncomingSms(string accountSid, string authToken, string receivingNumber, long notifierId)
        {
            // Do nothing
        }

        public virtual void DeregisterUrlForIncomingSms(string accountSid, string authToken, string receivingNumber)
        {
            // Do nothing
        }

    }
}
