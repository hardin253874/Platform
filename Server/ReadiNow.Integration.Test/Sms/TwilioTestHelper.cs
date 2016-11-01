using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ReadiNow.Integration.Sms;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Model;

namespace ReadiNow.Integration.Test.Sms
{
    public static class TwilioTestHelper
    {
        /*
        **  Test numbers from https://www.twilio.com/docs/api/rest/test-credentials
            VALUE	DESCRIPTION	ERROR CODE
            +15005550001	This phone number is invalid.	21212
            +15005550007	This phone number is not owned by your account or is not SMS-capable.	21606
            +15005550008	This number has an SMS message queue that is full.	21611
            +15005550006	This number passes all validation.	No error
            All Others	This phone number is not owned by your account or is not SMS-capable.	21606
        */

        public const string TestSendingNumber_Valid = "+15005550006";
        public const string TestFromNumber_Valid = "+15005550006";
        public const string TestToNumber_Valid = "+14108675309";

        public const string TestSendingNumber_Invalid = "+77777777777";
        public const string TestToNumber_CantReceive = "+15005550007";
        public const string TestToNumber_QueueFull = "+15005550008";

        public const string TestAccountSid = "AC568b00b892cccbc9ebb9c0c227d634f4";     // test account
        public const string TestAuthToken = "f2dffea73301bedb69361c4352f2f384";        // 

        public const string TrialAccountSid = "AC19c585f5922b490e044784d0abadaab6";     // scott.hopwood trial account
        public const string TrialAuthToken = "76eb1ade0d346d96659b9290f6f31cf4";        // scott.hopwood trial account
        public const string TrialSendingNumber = "+61481072165";

    }
}
