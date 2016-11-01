// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.Security;
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
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class TwilioSmsReceiverTest
    {
        Random _rand = new Random();

        TwilioSmsReceiver _receiver;

        [TestFixtureSetUp]
        public void Setup()
        {
            _receiver = new TwilioSmsReceiver();
        }

        [Test]
        public void GenerateUrl()
        {
            var notifierId = 111;

            var url = _receiver.GetIncommingUrl(notifierId);

            Assert.That(url, Is.StringStarting("https://"));
            Assert.That(url, Is.StringEnding($"/integration/twilio/sms/EDC/{notifierId}"));
        }

        [Test]
        public void HandleRequest()
        {
            var notifier = CreateNotifier();
            var note = CreateNotification();

            var sms = CreateMatchingReply(notifier, note.SendRecords.First().As<SmsSendRecord>());

            _receiver.HandleRequest(notifier.Id, sms);

            var replies = note.SendRecords.First().SrToReply;

            Assert.That(replies.Count, Is.EqualTo(1));

            var reply = replies.First().As<ReplyRecord>();

            var closed = Entity.Get<SmsSendRecord>(note.SendRecords.First().Id).SsrClosed;

            Assert.That(closed ?? false, Is.False);     // We don't close a response, there may be more
        }


        [Test]
        public void HandleRequest_CantSendToClosed()
        {
            var notifier = CreateNotifier();
            var note = CreateNotification();

            var send = note.SendRecords.First().AsWritable<SmsSendRecord>();
            send.SsrClosed = true;
            send.Save();

            var sms = CreateMatchingReply(notifier, send);

            _receiver.HandleRequest(notifier.Id, sms);

            var replies = note.SendRecords.First().SrToReply;

            Assert.That(replies.Count, Is.EqualTo(0));
        }

        [Test]
        public void HandleRequest_DoubleResponse()
        {
            var notifier = CreateNotifier();
            var note = CreateNotification();
            var send = note.SendRecords.First().As<SmsSendRecord>();
            var sms1 = CreateMatchingReply(notifier, send, "First");

            _receiver.HandleRequest(notifier.Id, sms1);

            var sms2 = CreateMatchingReply(notifier, send, "Second");

            _receiver.HandleRequest(notifier.Id, sms2);

            var replies = note.SendRecords.First().SrToReply;

            Assert.That(replies.Count, Is.EqualTo(2));
        }

        [Test]
        public void HandleRequest_DoubleSend()
        {
            var notifier = CreateNotifier();

            var note1 = CreateNotification();

            var send1 = note1.SendRecords.First().As<SmsSendRecord>();


            // Second notification with the same to and from
            var note2 = CreateNotification();

            var send2 = note2.SendRecords.First().AsWritable<SmsSendRecord>();
            send2.SsrFrom = send1.SsrFrom;
            send2.SsrTo = send1.SsrTo;
            send2.Save();
            

            var sms1 = CreateMatchingReply(notifier, send1);

            _receiver.HandleRequest(notifier.Id, sms1);


            var replies1 = note1.SendRecords.First().SrToReply;
            var replies2 = note2.SendRecords.First().SrToReply;

            // The second note is the one replied to
            Assert.That(replies1.Count, Is.EqualTo(0));
            Assert.That(replies2.Count, Is.EqualTo(1));

            //The first note has been closed
            Assert.That(note1.SendRecords.First().As<SmsSendRecord>().SsrClosed, Is.True);
            Assert.That(note2.SendRecords.First().As<SmsSendRecord>().SsrClosed ?? false, Is.False);
        }


        [Test]
        public void HandleRequest_MatchesCorrectly()
        {
            var notifier = CreateNotifier();

            new List<Notification>();

            var notifications = Enumerable.Range(0, 20).Select(n =>
            {
                var note = CreateNotification();
                return note;
            });

            Entity.Save(notifications);

            foreach (var note in notifications)
            {
                var send = note.SendRecords.First().As<SmsSendRecord>();
                var sms = CreateMatchingReply(notifier, send);
                _receiver.HandleRequest(notifier.Id, sms);

                Assert.That(send.SrToReply.Count, Is.EqualTo(1));
            }
        }

        [TestCase(1, 1)]
        [TestCase(-1, 0)]
        public void HandleRequest_AcceptRepliesUntil(int daysLeft, int expectedMatched)
        {
            var notifier = CreateNotifier();
            var note = CreateNotification(daysLeft);

            var send = note.SendRecords.First().AsWritable<SmsSendRecord>();
            send.Save();

            var sms = CreateMatchingReply(notifier, send);

            _receiver.HandleRequest(notifier.Id, sms);

            var replies = note.SendRecords.First().SrToReply;

            Assert.That(replies.Count, Is.EqualTo(expectedMatched));
        }


        [Test]
        [ExpectedException(typeof(UnmatchedNotifierException))]
        public void CantFindNotifier()
        {
            _receiver.HandleRequest(-9, CreateSms());
        }



        [ExpectedException(typeof(UnauthorisedApiException))]
        public void UnauthorisedApi()
        {
            Assert.Fail();
        }


        [TestCase("sent", null, "sent", null)]
        [TestCase("failed", "10001", "failed", "Account is not active (Twilio Error Code: 10001)")]
        public void HandleStatusUpdate(string status, string errorCode, string expectedStatus, string expectedErrorMessage)
        {
            var notifier = new TwilioNotifier();
            notifier.Save();

            var messageSid = _rand.Next().ToString();
            var receiver = new TwilioSmsReceiver();

            var send = new SmsSendRecord { SsrMessageSid = messageSid };
            send.Save();

            bool handled = receiver.HandleStatusUpdate(notifier.Id, messageSid, status, errorCode);

            Assert.That(handled, Is.True);

            var send2 = Entity.Get<SmsSendRecord>(send.Id);
            Assert.That(send2.SsrDeliveryStatus, Is.EqualTo(expectedStatus));
            Assert.That(send2.SrErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void HandleStatusUpdate_MessageDoesNotExist()
        {
            var notifier = new TwilioNotifier();
            notifier.Save();

            var receiver = new TwilioSmsReceiver();

            var handled = receiver.HandleStatusUpdate(notifier.Id, _rand.Next().ToString(), "Sent", null);

            Assert.That(handled, Is.False);
            // Ignored
        }

        

        TwilioNotifier CreateNotifier()
        {
            var notifier = Entity.Create<TwilioNotifier>();
            notifier.TpAccountSid = _rand.Next().ToString();
            notifier.Save();
            return notifier;
        }


        Notification CreateNotification(int daysLeft = 1)
        {
            var note = Entity.Create<Notification>();
            note.NMessage = "Sent Message";
            note.SendRecords.Add(CreateSend().As<SendRecord>());
            note.NAcceptRepliesUntil = DateTime.UtcNow.AddDays(daysLeft);
            note.Save();
            return note;
        }

        SmsSendRecord CreateSend()
        {
            var send = Entity.Create<SmsSendRecord>();
            send.SsrFrom = "+1" + _rand.Next();
            send.SsrTo = "+2" + _rand.Next();
            send.SrSendDate = DateTime.UtcNow;
            send.Save();
            return send;
        }

        TwilioSms CreateMatchingReply(TwilioNotifier notifier, SmsSendRecord send, string body="Unimportant")
        {
            return CreateSms(accountSid: notifier.TpAccountSid, from: send.SsrTo, to: send.SsrFrom, body: body);
        }

        TwilioSms CreateSms(string messageSid = "messageSid", string accountSid = "accountSid", string messageServiceSid = "messageServiceSid", string from = "+1111111", string to = "+2222222", string body="body")
        {
            return new TwilioSms { MessageSid = messageSid, AccountSid = accountSid, From = from, To = to, Body = body };
        }
    }
}
