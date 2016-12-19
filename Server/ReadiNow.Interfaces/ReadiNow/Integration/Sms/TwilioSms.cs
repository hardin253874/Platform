// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Integration.Sms
{
    /// <summary>
    /// An incoming  Twilio sms message as rwec
    /// </summary>
    public class TwilioSms
    {
        public string MessageSid { get; set; }
        public string AccountSid { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }

    }
}
