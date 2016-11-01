// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Twilio.TwiML;

namespace ReadiNow.Integration.Sms
{
    /// <summary>
    /// Handle an incomming message from Twilio. This may be a status message or a reply to a send SMS.
    /// </summary>
    public class TwilioSmsReceiver : ITwilioSmsReceiver
    {
        const string InboundUrlFormat = "/twilio/sms/{tenant}/{apiKey}";

        RequestValidator _validator;


        public TwilioSmsReceiver()
        {
            _validator = new RequestValidator();
        }

        



        public void HandleRequest(long notifierId, TwilioSms sms)
        {
            if (sms == null)
                throw new ArgumentException(nameof(sms));

            var notifier = Entity.Get<TwilioNotifier>(notifierId);


            if (notifier == null)
                throw new UnmatchedNotifierException();

            ValidateRequest(notifier);


            // check the account Sid matches. This ensures that someone else with a Twilio account can't create a phony incoming message. 
            if (notifier.TpAccountSid != sms.AccountSid)
                throw new UnauthorisedApiException();

            string matchExpression = $"[From]='{sms.To}' and [To]='{sms.From}' and not [Closed] and GetDateTime() < [Notification].[Accept replies until] ";       // Looking for a matching reply

            // do Something
            // Match sent to received.

            var matches = Entity.GetCalculationMatchesAsIds(matchExpression, SmsSendRecord.SmsSendRecord_Type, false);
            var countMatches = matches.Count();

            if (countMatches == 1)
            {
                var send = Entity.Get<SmsSendRecord>(matches.First());
                AddReply(send, sms.Body);
            }
            else if (countMatches > 1)
            {
                // Find the newest record and close the rest
                var matchedSends = Entity.Get<SmsSendRecord>(matches);
                var dateOrdered = matchedSends.OrderByDescending(s => s.SrSendDate ?? DateTime.MinValue);
                var newest = dateOrdered.First();

                AddReply(newest, sms.Body);

                var rest = dateOrdered.Skip(1);

                Close(rest);
            }
            else
            {
                EventLog.Application.WriteInformation($"Unmatched SMS received and ingored.\nNotifier: {notifier.Name}({notifier.Id})\n AccountSid: {sms.AccountSid}\nFrom: {sms.From}\nTo: {sms.To}\nBody: '{sms.Body}'");
            }
        }

             private void ValidateRequest(TwilioNotifier notifier)
        {
            var httpContext = System.Web.HttpContext.Current;

            if (httpContext == null)    // We are not coming via the http stack so don't validate.
                return;                 

            // To validate a dev request we need to ignore any ADC transformations that may have occurred.
            var externalAddress = ConfigurationSettings.GetSiteConfigurationSection().SiteSettings.Address;
            var alternativeUrl = $"https://{externalAddress}{httpContext.Request.Url.PathAndQuery}";
            bool isValid = _validator.IsValidRequest(httpContext, notifier.TpAuthToken, alternativeUrl);

            if (!isValid)
                throw new TwilioValidationException();
        }

        void AddReply(SmsSendRecord send, string body)
        {
            var reply = Entity.Create<ReplyRecord>();
            reply.RrReply = body;
            reply.RrReplyDate = DateTime.UtcNow;
            reply.RrToSend = send.As<SendRecord>();
            reply.Save();

            EventLog.Application.WriteInformation($"SMS matched and reply created");

        }

        void Close(IEnumerable<SmsSendRecord> oldSends)
        {
            var updated = oldSends.Select(s =>
            {
                var writable = s.AsWritable<SmsSendRecord>();
                writable.SsrClosed = true;
                return writable;
            });

            Entity.Save(updated);
        }


        /// <summary>
        /// Create the URL used for receiving the request
        /// </summary>
        /// <param name="notifier"></param>
        /// <returns></returns>
        public string GetIncommingUrl(long notifierId)
        {
            var tenantName = RequestContext.GetContext().Tenant.Name;

            return $"https://{GetBaseAddress()}/spapi/integration/twilio/sms/{tenantName}/{notifierId}";
        }

        /// <summary>
        /// Process a status update for the send record
        /// </summary>
        /// <returns>True if the status is handled,</returns>
        /// <param name="sendRecordId"></param>
        public bool HandleStatusUpdate(long notifierId, string messageSid, string messageStatus, string errorCode)
        {
            var notifier = Entity.Get<TwilioNotifier>(notifierId);

            if (notifier == null)
                throw new UnmatchedNotifierException();

            ValidateRequest(notifier);

            string matchExpression = $"[MessageSid]='{messageSid}'";       // Looking for a matching reply

            // do Something
            // Match sent to received.

            var matches = Entity.GetCalculationMatchesAsIds(matchExpression, SmsSendRecord.SmsSendRecord_Type, false);

            var match = matches.FirstOrDefault();

            if (match != 0)
            {
                var sendRecord = Entity.Get<SmsSendRecord>(match, true);

                sendRecord.SsrDeliveryStatus = messageStatus;

                if (!String.IsNullOrWhiteSpace(errorCode))
                    sendRecord.SrErrorMessage = ErrorCodeHelper.MessageFromCode(errorCode);

                sendRecord.Save();

                return true;
            }
            else
            {
                EventLog.Application.WriteInformation($"Status update, unmatched SMS received and ingored.\nMessageSid: {messageSid}\nMessageStatus: {messageStatus}\nErrorCode: {errorCode}");
                return false;
            }
        }

        /// <summary>
        /// Get the URL for twilio to call to update message statuses
        /// </summary>
        /// <param name="sendRecord"></param>
        /// <returns></returns>
        public string GetStatusUpdateUrl(long notifierId)
        {
            var tenantName = RequestContext.GetContext().Tenant.Name;

            return $"https://{GetBaseAddress()}/spapi/integration/twilio/sms/sendstatus/{tenantName}/{notifierId}";
        }

        string GetBaseAddress()
        {
            return ConfigurationSettings.GetSiteConfigurationSection().SiteSettings.Address; 
        }
    }
}
