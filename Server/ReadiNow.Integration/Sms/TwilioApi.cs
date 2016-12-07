// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using System.Net;
using System.IO;
using EDC.ReadiNow.Diagnostics;
using Twilio;

namespace ReadiNow.Integration.Sms
{
    /// <summary>
    ///     Twilio integration piece.
    /// </summary>
    public class TwilioApi : ISmsProvider
    {
        const string TestSendNumberPrefix = "+1500555";

        ITwilioSmsReceiver _receiver;

        public TwilioApi(ITwilioSmsReceiver receiver)
        {
            _receiver = receiver;
        }

        //
        // TODO: Once the updated version of the Twilio library with async support comes out of Alpha, use that.
        public async Task<IEnumerable<SendResponse>> SendMessage(long notifierId, string accountSid, string authToken, string sendingNumber, string message, IEnumerable<string> numbers)
        {
            Task<IEnumerable<SendResponse>> result;

            //var resetEvent = new ManualResetEvent(false);

            Func<IEnumerable<SendResponse>> fn = () =>
            {
                return SendMessageSync(notifierId, accountSid, authToken, sendingNumber, message, numbers);
            };

            result = new Task<IEnumerable<SendResponse>>(fn, TaskCreationOptions.AttachedToParent);
            result.Start();

            return await result;
        }

        public IEnumerable<SendResponse> SendMessageSync(long notifierId, string accountSid, string authToken, string sendingNumber, string message, IEnumerable<string> numbers)
        {
            var twilioClient = new TwilioRestClient(accountSid, authToken);

            var result = new List<SendResponse>();

            EventLog.Application.WriteTrace($"Twilio sending to {numbers.Count()} numbers");     

            foreach (var number in numbers)
            {
                var sendResponse = new SendResponse { Number = number };

                var sentMessage = twilioClient.SendMessage(sendingNumber, number, message, _receiver.GetStatusUpdateUrl(notifierId));

                if (sentMessage == null)
                {
                    throw new ApplicationException("Twilio send returned a null. This has only happened when there has been a network outage.");
                }

                sendResponse.MessageSid = sentMessage.Sid;
                sendResponse.Success = sentMessage.RestException == null;

                if (sendResponse.Success)
                {
                    EventLog.Application.WriteTrace($"Twilio send to {number} suceeded");     
                }
                else
                {
                    sendResponse.Success = false;
                    sendResponse.Message =  sentMessage.RestException.Message;
                    EventLog.Application.WriteError($"Twilio send to {number} failed with message: {sendResponse.Message}");
                }

                result.Add(sendResponse);
            }

            return result;
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
            var uri = _receiver.GetIncommingUrl(notifierId);

            var phoneOptions = new PhoneNumberOptions
            {
                SmsUrl = uri,
                SmsMethod = "GET"
            };

            HandleUrlForIncomingSms(accountSid, authToken, receivingNumber, phoneOptions);
        }

        public virtual void DeregisterUrlForIncomingSms(string accountSid, string authToken, string receivingNumber)
        {
            if (!receivingNumber.StartsWith(TestSendNumberPrefix))
            {
                var phoneOptions = new PhoneNumberOptions
                {
                    SmsUrl = string.Empty,
                    SmsMethod = "POST"
                };

                HandleUrlForIncomingSms(accountSid, authToken, receivingNumber, phoneOptions);
            }
        }

        void HandleUrlForIncomingSms(string accountSid, string authToken, string receivingNumber, PhoneNumberOptions phoneOptions)
        {
            if (!receivingNumber.StartsWith(TestSendNumberPrefix))
            {
                var cleanNumber = PhoneNumberHelper.CleanNumber(receivingNumber);

                var twilioClient = new TwilioRestClient(accountSid, authToken);
                var numbersRequest = twilioClient.ListIncomingPhoneNumbers();

                if (numbersRequest.RestException != null)
                {
                    throw new TwilioException($"Failed to fetch list of numbers from Twilio: {numbersRequest.RestException.Message}");
                }

                var number = numbersRequest.IncomingPhoneNumbers.FirstOrDefault(n => PhoneNumberHelper.CleanNumber(n.PhoneNumber) == cleanNumber);

                if (number == null)
                    throw new ArgumentException("Unable to find matching number", nameof(receivingNumber));

                twilioClient.UpdateIncomingPhoneNumber(number.Sid, phoneOptions);
            }
        }

    }
}
