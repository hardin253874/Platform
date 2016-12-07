// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadiNow.Integration.Sms
{
    /// <summary>
    /// Interface for a reciever for Twilio SMS messages
    /// </summary>
    public interface ITwilioSmsReceiver
    {
        /// <summary>
        /// Get the URL that twilio will call with incomming messages
        /// </summary>
        /// <param name="notifierId"></param>
        /// <returns>The url</returns>
        string GetIncommingUrl(long notifierId);

        /// <summary>
        /// Handle and incomminng SMS message
        /// </summary>
        /// <param name="tenant">Tenant name</param>
        /// <param name="notifierId">The id of the notifier</param>
        /// <param name="sms">Twilio sms message</param>
        void HandleRequest(long notifierId, TwilioSms sms);

        /// <summary>
        /// Handle the receipt of a status update for a sent message
        /// </summary>
        /// <returns>True if the status was handled</returns>
        /// <param name="sendRecordId"></param>
        bool HandleStatusUpdate(long notifierId, string messageSid, string messageStatus, string errorCode);

        /// <summary>
        /// Get the URL for Twilio to send status updates
        /// </summary>
        /// <returns></returns>
        string GetStatusUpdateUrl(long notifierId);
    }

    public class InvalidTenantException: Exception
    {
        public InvalidTenantException(): base("The provided tenant is invalid")
        {}
    }


    public class UnauthorisedApiException : Exception
    {
        public UnauthorisedApiException() : base("The provided api key is not authorised")
        { }
    }

    public class UnmatchedNotifierException : Exception
    {
        public UnmatchedNotifierException() : base("The provided notifier Id is invalid")
        { }
    }

    public class UnmatchedSendRecordException : Exception
    {
        public UnmatchedSendRecordException() : base("The provided send record Id is invalid")
        { }
    }

    public class TwilioValidationException : Exception
    {
        public TwilioValidationException() : base("The request did not appear to originate from Twilio")
        { }
    }
}
