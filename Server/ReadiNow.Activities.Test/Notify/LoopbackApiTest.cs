// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.Security;
using EDC.SoftwarePlatform.Activities.Notify;
using Moq;
using NUnit.Framework;
using ReadiNow.Integration.Sms;
using ReadiNow.Integration.Test.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Notify
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class LoopbackApiTest
    {
        
        [TestCase("1", "TestMode:Reply:1" )]
        [TestCase("2", "TestMode:Reply:2" )]
        [TestCase("3", "TestMode:Reply:3" )]
        [TestCase("4", "TestMode:Reply:4" )]
        [TestCase("5", "TestMode:Reply:5" )]
        [TestCase("6", "TestMode:Reply:yes" )]
        [TestCase("7", "TestMode:Reply:no" )]
        [TestCase("8", "TestMode:Reply:approve" )]
        [TestCase("9", "TestMode:Reply:reject")]
        [TestCase("0", "TestMode:Reply:cancel")]
        [RunWithoutTransaction]     // As we are calling back into IIS, this can not run in a transaction
        public void Send(string terminalDigit, string expectedReply)
        {
            Random rand = new Random();

            TwilioNotifier notifier = null;
            Person person = null;
            SendRecord result = null;
            Notification notification = null;

            try
            {
                notifier = new TwilioNotifier()
                {
                    Name = "LoopbackApiTest.Notify " + DateTime.UtcNow,
                    TpAccountSid = rand.Next().ToString() ,
                    TpAuthToken = rand.Next().ToString(),
                    TpSendingNumber = rand.Next().ToString(),
                    TpEnableTestMode = true
                };
                notifier.Save();

                person = new Person { Name = "LoopbackApiTest.Notify " + DateTime.UtcNow };

                var mobilePhoneField = Factory.ScriptNameResolver.GetInstance("Mobile phone", StringField.StringField_Type.Id);
                var mobileNumber = rand.Next().ToString() + terminalDigit;
                person.SetField(mobilePhoneField, mobileNumber);
                person.Save();

                notification = new Notification { NMessage = "TestMessage", NAcceptRepliesUntil = DateTime.UtcNow.AddDays(1) };
                notification.Save();

                TwilioRouter.Instance.Send(notifier, notification, person.ToEnumerable(), true);

                var results = Entity.Get<Notification>(notification.Id).SendRecords;
                Assert.That(results, Is.Not.Null);
                Assert.That(results.Count, Is.EqualTo(1));

                result = results.First();
                Assert.That(result.SrErrorMessage, Is.Null);

                // It can take a while for the second thread's update to work its way through.
                SendRecord result2 = null;
                for (int i = 0; i < 50; i++)
                {
                    result2 = Entity.Get<SendRecord>(result.Id);
                    if (result2.SrToReply.Count() == 1)
                        break;
                }

                Assert.That(result2.SrToReply.Count(), Is.EqualTo(1));
                Assert.That(result2.SrToReply.First().RrReply, Is.EqualTo(expectedReply));
            }
            finally
            {
                notifier?.Delete();
                person?.Delete();
                notification?.Delete();
            }
        }

        [TestCase(50)]
        [TestCase(100)]
        [TestCase(500)]
        [TestCase(1000)]
        [Explicit]
        [RunWithoutTransaction]         // Can't be in a transaction as it takes too long
        public void SendMany(int numberToSend)
        {
            Random rand = new Random();

            TwilioNotifier notifier = null;
            List<Person> people = null;
            Notification notification = null;

            try
            {
                notifier = new TwilioNotifier()
                {
                    Name = "LoopbackApiTest.Notify " + DateTime.UtcNow,
                    TpAccountSid = rand.Next().ToString(),
                    TpAuthToken = rand.Next().ToString(),
                    TpSendingNumber = rand.Next().ToString(),
                    TpEnableTestMode = true
                };
                notifier.Save();

                people = new List<Person>();

                var mobilePhoneField = Factory.ScriptNameResolver.GetInstance("Mobile phone", StringField.StringField_Type.Id);

                for (int i = 0; i < numberToSend; i++)
                {
                    var person = new Person { Name = "LoopbackApiTest.Notify " + DateTime.UtcNow };

                    var mobileNumber = rand.Next().ToString();
                    person.SetField(mobilePhoneField, mobileNumber);
                    people.Add(person);
                }

                Entity.Save(people);

                notification = new Notification { NMessage = "TestMessage", NAcceptRepliesUntil = DateTime.UtcNow.AddDays(1) };
                notification.Save();

                TwilioRouter.Instance.Send(notifier, notification, people, true);

                var results = Entity.Get<Notification>(notification.Id).SendRecords;
                Assert.That(results, Is.Not.Null);
                Assert.That(results.Count, Is.EqualTo(numberToSend));

                foreach (var result in results)
                {
                    Assert.That(result.SrErrorMessage, Is.Null);
                }
            }
            finally
            {
                notifier?.Delete();
                if (people.Any())
                    Entity.Delete(people.Select(p => p.Id));
                notification?.Delete();
            }
        }

    }
}
