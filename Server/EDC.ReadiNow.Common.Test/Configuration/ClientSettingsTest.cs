// Copyright 2011-2016 Global Software Innovation Pty Ltd

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Configuration
{
    [TestFixture]
    public class ClientSettingsTest
    {
        /// <summary>
        /// Check the the required client is checked
        /// </summary>
        [TestCase(null, "2.0.222", "2.0")]
        [TestCase("", "2.0.222", "2.0")]
        [TestCase("1.0", "2.0.222", "2.0")]
        [TestCase("2.0.333", "2.0.222", "2.0.333")]
        [RunAsDefaultTenant]
        public void GetRequiredClientVersion_Test(string requiredVersion, string platformVersion, string expected)
        {
            var clientSettings = new ReadiNow.Configuration.ClientSettings { MinClientVersion = requiredVersion };
            Assert.That(clientSettings.GetMinClientVersion(platformVersion), Is.EqualTo(expected));
        }
    }
}
