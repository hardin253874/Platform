// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadiNow.Connector.Test.Scheduled
{
    [TestFixture]
    public class ScheduledImportConfigSaveHandlerTest
    {
        [TestCase("myPassword")]
        [TestCase("")]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void Create(string password)
        {
                var config = new ScheduledImportConfig { SicUrl = "ftps://any.com/file.csv", SicPassword = password };
                config.Save();

                Assert.That(config.SicPassword, Is.Not.EqualTo(password));
                Assert.That(config.SicPasswordSecureId, Is.Not.Null);
                Assert.That(((Guid)config.SicPasswordSecureId), Is.Not.EqualTo(Guid.Empty));

                var storedPassword = Factory.SecuredData.Read((Guid)config.SicPasswordSecureId);

                Assert.That(storedPassword, Is.EqualTo(password));
        }

        [TestCase("myPassword2")]
        [TestCase("")]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void Update(string password2)
        {
            var password1 = "myPassword1";

            var config = new ScheduledImportConfig { SicUrl = "ftps://any.com/file.csv", SicPassword = password1 };
                config.Save();

            var secureId = (Guid) config.SicPasswordSecureId;

            config = config.AsWritable<ScheduledImportConfig>();
            config.SicPassword = password2;
            config.Save();

            Assert.That(config.SicPassword, Is.Not.EqualTo(password2));
            Assert.That(config.SicPasswordSecureId, Is.Not.Null);
            Assert.That(((Guid)config.SicPasswordSecureId), Is.EqualTo(secureId));

            var storedPassword = Factory.SecuredData.Read((Guid)config.SicPasswordSecureId);

            Assert.That(storedPassword, Is.EqualTo(password2));
        }

        [TestCase]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void UpdateFollowedBySomeOtherChange()
        {
            var password1 = "myPassword1";

            var config = new ScheduledImportConfig { SicUrl = "ftps://any.com/file.csv", SicPassword = password1 };
            config.Save();

            var secureId = (Guid)config.SicPasswordSecureId;

            var config2 = Entity.Get<ScheduledImportConfig>(config.Id);
            var pw = config.SicPassword;

            config = config.AsWritable<ScheduledImportConfig>();
            config.TriggerEnabled = true;
            config.Save();

            Assert.That(config.SicPassword, Is.Not.EqualTo(password1));
            Assert.That(config.SicPasswordSecureId, Is.Not.Null);
            Assert.That(((Guid)config.SicPasswordSecureId), Is.EqualTo(secureId));

            var storedPassword = Factory.SecuredData.Read((Guid)config.SicPasswordSecureId);

            Assert.That(storedPassword, Is.EqualTo(password1));
        }
    }
}
