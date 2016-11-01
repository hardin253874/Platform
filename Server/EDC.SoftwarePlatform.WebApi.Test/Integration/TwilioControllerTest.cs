// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using ReadiNow.Integration.Sms;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace EDC.SoftwarePlatform.WebApi.Test.Integration
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class TwilioControllerTest
    {
        Random rand = new Random();
        TwilioNotifier _notifier = null;

        [TestFixtureSetUp]
        public void SetUp()
        {
            using (new TenantAdministratorContext("EDC"))
            {
                _notifier = CreateNotifier();
            }
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            using (new TenantAdministratorContext("EDC"))
            {
                _notifier?.Delete();
            }
        }

        [Test]
        public void IncommingUrlCanBeConsumed()
        {
            string uri;

            using (new TenantAdministratorContext("EDC"))
            {
                //var baseAddress = ConfigurationSettings.GetSiteConfigurationSection().SiteSettings.Address;
                var baseAddress = GetFqdn();

                var tenantName = RequestContext.GetContext().Tenant.Name;
                var parameters = $"MessageSid=1111&AccountSid={_notifier.TpAccountSid}&MessagingServiceSid=3333&From=+11111&To=+22222&Body=This is a reply";

               // uri = $"https://{baseAddress}/spapi/integration/twilio/sms/{tenantName}/{_notifier.Id}?{parameters}";
                uri = $"https://{baseAddress}/spapi/integration/twilio/sms/{tenantName}/{_notifier.Id}?{parameters}";
            }

            using (var request = new PlatformHttpRequest(uri, PlatformHttpMethod.Get, null, true, isFullUri: true))
            {
                var response = request.GetResponse();

                // check that it worked (201)
                Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
            }
        }

      

        TwilioNotifier CreateNotifier()
        {
            var notifier = Entity.Create<TwilioNotifier>();
            notifier.TpAccountSid = rand.Next().ToString();
            notifier.TpAuthToken = rand.Next().ToString();
            notifier.TpSendingNumber = rand.Next().ToString();
            notifier.TpEnableTestMode = true;
            notifier.Save();
            return notifier;
        }

        public string GetFqdn()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            domainName = "." + domainName;

            if (!hostName.EndsWith(domainName)) // if hostname does not already include domain name
            {
                hostName += domainName; // add the domain name part
            }

            return hostName; // return the fully qualified name
        }
    }
}
