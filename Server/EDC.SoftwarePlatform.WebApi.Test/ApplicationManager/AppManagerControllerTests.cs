// Copyright 2011-2014 Global Software Innovation Pty Ltd
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.SoftwarePlatform.WebApi.Controllers.ApplicationManager;

namespace EDC.SoftwarePlatform.WebApi.Test.ApplicationManager
{
    /// <summary>
    /// Test class for the Application Manager Controller and <see cref="AppManagerService"/>.
    /// </summary>
    [TestFixture]
    public class AppManagerControllerTests
    {
        /// <summary>
        /// Tests the <see cref="GetApplicationData" /> method on <see cref="AppManagerController"/>.
        /// </summary>
        [Test]
        public void TestGetApplicationData()
        {
            var appManagerController = new AppManagerController();

            

            var result = appManagerController.GetApplicationData();
            result.Should().NotBeNull();
            result.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
