// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test
{
    [TestFixture]
    [RunAsGlobalTenant]
    [RunWithTransaction]
    public class SystemHelperTests
    {
        [Test]
        public void SetDocumentationSettingsClearSettingsTest()
        {
            CacheManager.ClearCaches();

            var settings = new Dictionary<string, string>
            {
                {"core:navHeaderDocumentationUrl", ""},
                {"core:documentationUrl", ""},
                {"core:releaseNotesUrl", ""},
                {"core:contactSupportUrl", ""},
                {"core:documentationUserName", ""},
                {"core:documentationUserPassword", ""}
            };

            SystemHelper.SetDocumentationSettings(settings);

            // Verify the settings
            var docoSettingsEntity = Entity.Get<SystemDocumentationSettings>("core:systemDocumentationSettingsInstance");

            Assert.AreEqual(settings["core:navHeaderDocumentationUrl"], docoSettingsEntity.NavHeaderDocumentationUrl);
            Assert.AreEqual(settings["core:documentationUrl"], docoSettingsEntity.DocumentationUrl);
            Assert.AreEqual(settings["core:releaseNotesUrl"], docoSettingsEntity.ReleaseNotesUrl);
            Assert.AreEqual(settings["core:contactSupportUrl"], docoSettingsEntity.ContactSupportUrl);
            Assert.AreEqual(settings["core:documentationUserName"], docoSettingsEntity.DocumentationUserName);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual(settings["core:documentationUserPassword"],
                Factory.SecuredData.Read(docoSettingsEntity.DocumentationUserPasswordSecureId.Value));
        }

        [Test]
        public void SetDocumentationSettingsInvalidSettingsTest()
        {
            var settings = new Dictionary<string, string>
            {
                {"core:fieldThatDoesNotApply", "http://navHeaderDocumentationUrl/"}
            };

            Assert.That(() => SystemHelper.SetDocumentationSettings(settings),
                Throws.TypeOf<ArgumentException>()
                    .And.Property("ParamName")
                    .EqualTo("docoSettings")
                    .And.Message.StartsWith(
                        "The field with alias core:fieldThatDoesNotApply does not apply to the system documentation settings type."));
        }

        [Test]
        public void SetDocumentationSettingsNullSettingsTest()
        {
            Assert.That(() => SystemHelper.SetDocumentationSettings(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("docoSettings"));
        }

        [Test]
        public void SetDocumentationSettingsValidSettingsTest()
        {
            CacheManager.ClearCaches();

            var settings = new Dictionary<string, string>
            {
                {"core:navHeaderDocumentationUrl", "http://navHeaderDocumentationUrl.com/"},
                {"core:documentationUrl", "http://documentationUrl.com/"},
                {"core:releaseNotesUrl", "http://releaseNotesUrl.com/"},
                {"core:contactSupportUrl", "http://contactSupportUrl.com/"},
                {"core:documentationUserName", "documentationUserName" + Guid.NewGuid()},
                {"core:documentationUserPassword", "documentationUserPassword" + Guid.NewGuid()}
            };

            SystemHelper.SetDocumentationSettings(settings);

            // Verify the settings
            var docoSettingsEntity = Entity.Get<SystemDocumentationSettings>("core:systemDocumentationSettingsInstance");

            Assert.AreEqual(settings["core:navHeaderDocumentationUrl"], docoSettingsEntity.NavHeaderDocumentationUrl);
            Assert.AreEqual(settings["core:documentationUrl"], docoSettingsEntity.DocumentationUrl);
            Assert.AreEqual(settings["core:releaseNotesUrl"], docoSettingsEntity.ReleaseNotesUrl);
            Assert.AreEqual(settings["core:contactSupportUrl"], docoSettingsEntity.ContactSupportUrl);
            Assert.AreEqual(settings["core:documentationUserName"], docoSettingsEntity.DocumentationUserName);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual(settings["core:documentationUserPassword"],
                Factory.SecuredData.Read(docoSettingsEntity.DocumentationUserPasswordSecureId.Value));
        }
    }
}