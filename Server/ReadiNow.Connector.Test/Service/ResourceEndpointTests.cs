// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Connector.EndpointTypes;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;

namespace ReadiNow.Connector.Test.Service
{
    /// <summary>
    /// Test ResourceEndpointTests
    /// </summary>
    [TestFixture]
    public class ResourceEndpointTests
    {
        [TestCase(false)]
        [TestCase(true)]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void Test_SuppressWorkflow(bool endpointSuppressWorkflows)
        {
            Mock<IReaderToEntityAdapterProvider> m1 = new Mock<IReaderToEntityAdapterProvider>(MockBehavior.Strict);
            Mock<IResourceResolverProvider> m2 = new Mock<IResourceResolverProvider>(MockBehavior.Loose);
            Mock<IResourceUriGenerator> m3 = new Mock<IResourceUriGenerator>(MockBehavior.Loose);

            ApiResourceMapping mapping = new ApiResourceMapping();

			long typeId = Factory.ScriptNameResolver.GetTypeByName( "AA_Drink" );

			if ( typeId == 0 )
			{
				mapping.MappedType = null;
			}
			else
			{
				mapping.MappedType = Entity.Get( typeId ).As<EntityType>( );
			}

            mapping.MappingSuppressWorkflows = endpointSuppressWorkflows;
            mapping.Save();

            ApiResourceEndpoint apiResourceEndpoint = new ApiResourceEndpoint();
            apiResourceEndpoint.EndpointResourceMapping = mapping;
            apiResourceEndpoint.EndpointCanCreate = true;
            apiResourceEndpoint.ApiEndpointEnabled = true;
            apiResourceEndpoint.Save();

            ResourceEndpoint re = new ResourceEndpoint(m1.Object, m2.Object, m3.Object);

            ConnectorRequest request = new ConnectorRequest();
            request.Verb = ConnectorVerb.Post;

            m1.Setup<IReaderToEntityAdapter>(m => m.GetAdapter(It.IsAny<long>(), It.IsAny<ReaderToEntityAdapterSettings>())).Returns(
                () =>
                {
                    Assert.That(WorkflowRunContext.Current.DisableTriggers, Is.EqualTo(endpointSuppressWorkflows));
                    return null;
                }
                );

            // We don't care what happens .. as long as the callback was called with the correct context setting
            try
            {
                re.HandleRequest(request, apiResourceEndpoint);
            }
            catch { }

            m1.VerifyAll();
        }
    }
}
