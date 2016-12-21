// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.Notify;
using Moq;
using NUnit.Framework;
using ReadiNow.Integration.Sms;
using ReadiNow.Integration.Test.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Notify
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class TwilioRouterTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Notify(bool expectReply)
        {
            var provider = new DummyProvider();

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => provider).As<ISmsProvider>();

            }))
            using (Factory.SetCurrentScope(scope))
            {

                var notifier = new TwilioNotifier()
                {
                    Name = "TwilioTest",
                    TpAccountSid = TwilioTestHelper.TestAccountSid,
                    TpAuthToken = TwilioTestHelper.TestAuthToken,
                    TpSendingNumber = TwilioTestHelper.TestFromNumber_Valid
                };

                var person = new Person();
                var mobilePhoneField = Factory.ScriptNameResolver.GetInstance("Mobile phone", StringField.StringField_Type.Id);
                person.SetField(mobilePhoneField, TwilioTestHelper.TestToNumber_Valid);

                var notification = new Notification { NMessage = "TestMessage" };

                TwilioRouter.Instance.Send(notifier,  notification, person.ToEnumerable(), expectReply);

                var result = Entity.Get<Notification>(notification.Id).SendRecords;
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result.First().SrErrorMessage, Is.Null);
            }
        }



        class DummyProvider : ISmsProvider
        {
            public Task<IEnumerable<SendResponse>> SendMessage(long notifierId, string accountSid, string authToken, string sendingNumber, string message, IEnumerable<string> numbers)
            {
                var result =  new Task<IEnumerable<SendResponse>>(() =>
                {
                    return numbers.Select(n =>
                        new SendResponse
                        {
                            Message = message,
                            Number = n,
                            Success = true
                        }
                    );
                });

                result.Start();

                return result;
            }

            public void RegisterUrlForIncomingSms(string accountSid, string authToken, string receivingNumber, long notifierId)
            { }

            public void DeregisterUrlForIncomingSms(string accountSid, string authToken, string receivingNumber)
            { }


            public DummyProvider() { }
        }

    }
}
