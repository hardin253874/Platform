// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using ReadiNow.Connector.Service;
using System.Net;

namespace ReadiNow.Connector.Test.Service
{
    /// <summary>
    /// Test ApiKeySecurity
    /// </summary>
    [TestFixture]
    public class ExceptionServiceLayerTests
    {
        [TestCase("ConnectorConfigException", HttpStatusCode.InternalServerError)]
        [TestCase("ConnectorRequestException", HttpStatusCode.BadRequest)]
        public void Test_Catch_ConnectorConfigException(string exception, HttpStatusCode expectCode)
        {
            Mock<IConnectorService> mockService;
            IConnectorService exceptionLayer;
            ConnectorRequest request;
            ConnectorResponse response;

            // Define service and mock
            mockService = new Mock<IConnectorService>(MockBehavior.Strict);
            exceptionLayer = new ExceptionServiceLayer(mockService.Object);

            // Define request
            request = new ConnectorRequest();

            // Setup 
            mockService.Setup(connector => connector.HandleRequest(request)).Returns(() =>
            {
                if (exception == "ConnectorConfigException")
                    throw new ConnectorConfigException("E1234 Test message");
                if (exception == "ConnectorRequestException")
                    throw new ConnectorRequestException("E1234 Test message");
                throw new Exception();
            }).Verifiable();

            // Place request
            response = exceptionLayer.HandleRequest(request);

            Assert.That(response.StatusCode, Is.EqualTo(expectCode));
            Assert.That(response.MessageResponse, Is.Not.Null);
            Assert.That(response.MessageResponse.PlatformMessageCode, Is.EqualTo("E1234"));
            Assert.That(response.MessageResponse.Message, Is.EqualTo("Test message") );

            mockService.VerifyAll();
        }
        
    }
}
