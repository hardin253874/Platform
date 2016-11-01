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
    public class PhoneNumberHelperTests
    {
        const string countryCode = "+64";

        [TestCase("+644212345678", "+644212345678")]
        [TestCase("(042) 1234 5678", "+644212345678")]
        [TestCase("042 1234 5678", "+644212345678")]
        [TestCase(" + 1 42 1234 5678", "+14212345678")]
        [TestCase("+ 1 (042) 1234 5678", "+14212345678")]
        [TestCase("+ 1 (0) 42 1234 5678", "+14212345678")]
        public void CleanNumber(string dirty, string expected)
        {
            Assert.That(PhoneNumberHelper.CleanNumber(countryCode, dirty), Is.EqualTo(expected));
        }
    }
}
