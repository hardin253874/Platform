// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Diagnostics;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Console;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using System.Net;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Processing;

namespace EDC.SoftwarePlatform.WebApi.Test.Console
{
    /// <summary>
    /// Test class for the Console Controller.
    /// </summary>
    [TestFixture]
    public class ConsoleControllerTests
    {
        #region Tests
        /// <summary>
        /// Tests the GetSessionInfo method.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void GetSessionInfo()
        {            
            using (var request = new PlatformHttpRequest("data/v1/console/sessioninfo"))
            {
                var response = request.GetResponse();
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                var sessionInfo = request.DeserialiseResponseBody<SessionInfoResult>();
                Assert.IsNotEmpty(sessionInfo.BookmarkScheme);
                Assert.IsNotEmpty(sessionInfo.PlatformVersion);
            }
        }

        /// <summary>
        /// Tests the Noop method.
        /// </summary>
        [Test]
        public void Noop()
        {
            using (PlatformHttpRequest request = new PlatformHttpRequest("data/v1/console/noop", PlatformHttpMethod.Get, null, true))
            {
                var response = request.GetResponse();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
        }


        /// <summary>
        /// Tests the GetSessionInfo method.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void ReportError()
        {
            using (var request = new PlatformHttpRequest("data/v1/console/reporterror?error=thisisanerror"))
            {
                var response = request.GetResponse();
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // We can't monitor the log as the write is happening on a different thread.
            }
        }

        /// <summary>
        /// Tests the getDocoSettings method.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void GetDocoSettings()
        {
            var originalSettings = new Dictionary<string, string>
            {
                {"core:navHeaderDocumentationUrl", ""},
                {"core:documentationUrl", ""},
                {"core:releaseNotesUrl", ""},
                {"core:contactSupportUrl", ""},
                {"core:documentationUserName", ""},
                {"core:documentationUserPassword", ""}
            };

            try
            {
                // Save previous settings
                using (new GlobalAdministratorContext())
                {
                    var docoSettingsEntity = Entity.Get<SystemDocumentationSettings>("core:systemDocumentationSettingsInstance");

                    var passwordSecureId = docoSettingsEntity.DocumentationUserPasswordSecureId;

                    originalSettings["core:navHeaderDocumentationUrl"] = docoSettingsEntity.NavHeaderDocumentationUrl;
                    originalSettings["core:documentationUrl"] = docoSettingsEntity.DocumentationUrl;
                    originalSettings["core:releaseNotesUrl"] = docoSettingsEntity.ReleaseNotesUrl;
                    originalSettings["core:contactSupportUrl"] = docoSettingsEntity.ContactSupportUrl;
                    originalSettings["core:documentationUserName"] = docoSettingsEntity.DocumentationUserName;                    
                    originalSettings["core:documentationUserPassword"] = passwordSecureId != null ? Factory.SecuredData.Read(passwordSecureId.Value) : null;
                }

                var newSettings = new Dictionary<string, string>
                {
                    {"core:navHeaderDocumentationUrl", "http://navHeaderDocumentationUrl.com/"},
                    {"core:documentationUrl", "http://documentationUrl.com/"},
                    {"core:releaseNotesUrl", "http://releaseNotesUrl.com/"},
                    {"core:contactSupportUrl", "http://contactSupportUrl.com/"},
                    {"core:documentationUserName", "documentationUserName" + Guid.NewGuid()},
                    {"core:documentationUserPassword", "documentationUserPassword" + Guid.NewGuid()}
                };

                // Set new values
                SystemHelper.SetDocumentationSettings(newSettings);

                using (var request = new PlatformHttpRequest("data/v1/console/getDocoSettings"))
                {
                    var response = request.GetResponse();
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                    var docoSettings = request.DeserialiseResponseBody<DocoSettingsResult>();

                    Assert.AreEqual(newSettings["core:contactSupportUrl"], docoSettings.ContactSupportUrl);
                    Assert.AreEqual(newSettings["core:documentationUrl"], docoSettings.DocumentationUrl);
                    Assert.AreEqual(newSettings["core:documentationUserName"], docoSettings.DocumentationUserName);
                    Assert.AreEqual(newSettings["core:documentationUserPassword"], docoSettings.DocumentationUserPassword);
                    Assert.AreEqual(newSettings["core:navHeaderDocumentationUrl"], docoSettings.NavHeaderDocumentationUrl);
                    Assert.AreEqual(newSettings["core:releaseNotesUrl"], docoSettings.ReleaseNotesUrl);
                }
            }
            finally
            {                     
                // Reset the settings
                SystemHelper.SetDocumentationSettings(originalSettings);
            }            
        }

        #endregion


    }
}
