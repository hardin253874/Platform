// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Diagnostics;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Console;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using System.Net;

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

        #endregion


    }
}
