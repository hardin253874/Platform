// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using Moq;
using NUnit.Framework;
using ReadiNow.Integration.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadiNow.Integration.Test.EventTarget
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class TwilioNotifierEventTargetTests
    {
        [Test]
        public void RegistrationDeregistration()
        {
            var notifier = new TwilioNotifier();
            notifier.Save(); // get the id

            notifier = notifier.AsWritable<TwilioNotifier>();
            notifier.TpAccountSid = "1111";
            notifier.TpAuthToken = "2222";
            notifier.TpSendingNumber = "+1234578";
            notifier.TpEnableTestMode = false;

            var tapiMock = new Mock<ISmsProvider>();
            tapiMock.Setup(c => c.RegisterUrlForIncomingSms(notifier.TpAccountSid, notifier.TpAuthToken, notifier.TpSendingNumber, It.Is<long>(v => v == notifier.Id)));
            tapiMock.Setup(c => c.DeregisterUrlForIncomingSms(notifier.TpAccountSid, notifier.TpAuthToken, notifier.TpSendingNumber));

            using (var scope = Factory.Current.BeginLifetimeScope(b =>
            {
                b.Register(c => tapiMock.Object).As<ISmsProvider>().SingleInstance();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                // Register
                notifier.Save();                           

                // Deregister
                notifier.AsWritable<TwilioNotifier>();
                notifier.TpEnableTestMode = true;
                notifier.Save();

            }

            tapiMock.VerifyAll();
        }

        [Test]
        public void ReregisterOnAccountChange()
        {
            var notifier = new TwilioNotifier();
            notifier = notifier.AsWritable<TwilioNotifier>();
            notifier.TpAccountSid = "1111";
            notifier.TpAuthToken = "2222";
            notifier.TpSendingNumber = "3333";
            notifier.TpEnableTestMode = false;
            notifier.Save(); // get the id

            var tapiMock = new Mock<ISmsProvider>();
            tapiMock.Setup(c => c.RegisterUrlForIncomingSms("91111", "2222", "3333", It.Is<long>(v => v == notifier.Id)));
            tapiMock.Setup(c => c.RegisterUrlForIncomingSms("91111", "92222", "3333", It.Is<long>(v => v == notifier.Id)));
            tapiMock.Setup(c => c.RegisterUrlForIncomingSms("91111", "92222", "93333", It.Is<long>(v => v == notifier.Id)));

            using (var scope = Factory.Current.BeginLifetimeScope(b =>
            {
                b.Register(c => tapiMock.Object).As<ISmsProvider>().SingleInstance();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                notifier = notifier.AsWritable<TwilioNotifier>();
                notifier.TpAccountSid = "91111";
                notifier.Save();

                notifier = notifier.AsWritable<TwilioNotifier>();
                notifier.TpAuthToken = "92222";
                notifier.Save();

                notifier = notifier.AsWritable<TwilioNotifier>();
                notifier.TpSendingNumber = "93333";
                notifier.Save();
            }

            tapiMock.VerifyAll();
        }

        [Test]
        public void NotRegisterOnNewWithTestMode()
        {
            var tapiMock = new Mock<ISmsProvider>();
           
            using (var scope = Factory.Current.BeginLifetimeScope(b =>
            {
                b.Register(c => tapiMock.Object).As<ISmsProvider>().SingleInstance();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                var notifier = new TwilioNotifier();
                notifier = notifier.AsWritable<TwilioNotifier>();
                notifier.TpAccountSid = "1111";
                notifier.TpAuthToken = "2222";
                notifier.TpSendingNumber = "3333";
                notifier.TpEnableTestMode = true;
                notifier.Save(); 
            }

            tapiMock.VerifyAll();
        }
    }
}
