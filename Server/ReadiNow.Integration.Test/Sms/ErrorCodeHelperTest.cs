// Copyright 2011-2016 Global Software Innovation Pty Ltd

using NUnit.Framework;
using ReadiNow.Integration.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadiNow.Integration.Test.Sms
{
    [TestFixture]
    public class ErrorCodeHelperTest
    {
        [TestCase("10001", "Account is not active (Twilio Error Code: 10001)")]
        [TestCase(" 10001 ", "Account is not active (Twilio Error Code: 10001)")] 
        [TestCase("", "Unknown error code (Twilio Error Code: BLANK)")]
        [TestCase(" ", "Unknown error code (Twilio Error Code: BLANK)")]
        [TestCase(null, "Unknown error code (Twilio Error Code: BLANK)")]
        [TestCase("99999", "Unknown error code (Twilio Error Code: 99999)")]
        public void ErrorCodeToMessage(string errorCode, string expected)
        {
            Assert.That(ErrorCodeHelper.MessageFromCode(errorCode), Is.EqualTo(expected));
        }
    }
}
