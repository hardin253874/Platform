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
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    [Explicit("These tests are hitting external servers so should not be run as part of automated tests")]
    public class TwilioApiTest
    {
        

        [Test]
        public void Send()
        {
            var tw = new TwilioApi(new TwilioSmsReceiver());

            var taskResult = tw.SendMessage(1111, TwilioTestHelper.TrialAccountSid, TwilioTestHelper.TrialAuthToken, TwilioTestHelper.TestFromNumber_Valid, "Test", TwilioTestHelper.TestToNumber_Valid.ToEnumerable());
           
            taskResult.Wait();

            var result = taskResult.Result.First();
            Console.WriteLine(result.Message ?? "Success");

            Assert.That(result.Success, Is.True);

        }


        //[Test]
        //public void SendAUrl()
        //{
        //    var tw = new TwilioApi();

        //    var url = "https://dev.readinow.net/spapi/approval/approve?token=12345";
        //    var taskResult = tw.SendMessage(TwilioTestHelper.TrialAccountSid, TwilioTestHelper.TrialAuthToken, TwilioTestHelper.TrialSendingNumber, url, "+61421510505".ToEnumerable());

        //    taskResult.Wait();

        //    var result = taskResult.Result.First();
        //    Console.WriteLine(result.Message ?? "Success");

        //    Assert.That(result.Success, Is.True);

        //}

        [Test]
        public void RegisterUrlForIncomingSms()
        {
            var tw = new TwilioApi(new TwilioSmsReceiver());

            //tw.RegisterUrlForIncomingSms(TwilioTestHelper.TestAccountSid, TwilioTestHelper.TestAuthToken, TwilioTestHelper.TestFromNumber_Valid, "https://Dummy.com/Dummy");
            tw.RegisterUrlForIncomingSms(TwilioTestHelper.TrialAccountSid, TwilioTestHelper.TrialAuthToken, TwilioTestHelper.TrialSendingNumber, 111);
        }


        [Test]
        public void DeregisterUrlForIncomingSms()
        {
            var tw = new TwilioApi(new TwilioSmsReceiver());

            tw.DeregisterUrlForIncomingSms(TwilioTestHelper.TrialAccountSid, TwilioTestHelper.TrialAuthToken, TwilioTestHelper.TrialSendingNumber);
        }

    }
}
