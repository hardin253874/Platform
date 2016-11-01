// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReadiNow.Connector.Service;
using EDC.ReadiNow.Test;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Model;

namespace ReadiNow.Connector.Test.Service
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    public class ResourceUriGeneratorTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_Null_Instance( )
        {
            IResourceUriGenerator generator = GetGenerator( );
            Assert.Throws<ArgumentNullException>( ( ) => generator.CreateResourceUri( null, new ConnectorRequest( ), new ApiResourceMapping( ) ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Null_Request( )
        {
            IResourceUriGenerator generator = GetGenerator( );
            Assert.Throws<ArgumentNullException>( ( ) => generator.CreateResourceUri( new Resource(), null, new ApiResourceMapping( ) ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Null_Mapping( )
        {
            IResourceUriGenerator generator = GetGenerator( );
            Assert.Throws<ArgumentNullException>( ( ) => generator.CreateResourceUri( new Resource( ), new ConnectorRequest( ), null ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Mapping_EntityGuid( )
        {
            ApiResourceMapping mapping = new ApiResourceMapping( );
            mapping.Save();

            ConnectorRequest request = new ConnectorRequest
            {
                ControllerRootPath = "https://whatever/spapi/api/",
                TenantName = "Tenant1",
                ApiPath = new string [ ] { "testapi", "testendpoint" }
            };

            Resource resource = new Resource( );
            resource.Save( );
            Assert.That( resource.UpgradeId, Is.Not.EqualTo( Guid.Empty ) );

            string expected = "https://whatever/spapi/api/Tenant1/testapi/testendpoint/" + resource.UpgradeId.ToString( );

            IResourceUriGenerator generator = GetGenerator( );
            string actual = generator.CreateResourceUri( resource, request, mapping );
            Assert.That( actual, Is.EqualTo( expected ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Mapping_StringField( )
        {
            ApiResourceMapping mapping = new ApiResourceMapping( );
            mapping.ResourceMappingIdentityField = Entity.Get<Field>( "core:description" );
            mapping.Save( );

            ConnectorRequest request = new ConnectorRequest
            {
                ControllerRootPath = "https://whatever/spapi/api/",
                TenantName = "Tenant1",
                ApiPath = new string [ ] { "testapi", "testendpoint" }
            };

            Resource resource = new Resource( );
            resource.Description = "Test1";
            resource.Save( );

            string expected = "https://whatever/spapi/api/Tenant1/testapi/testendpoint/Test1";

            IResourceUriGenerator generator = GetGenerator( );
            string actual = generator.CreateResourceUri( resource, request, mapping );
            Assert.That( actual, Is.EqualTo( expected ) );
        }

        private IResourceUriGenerator GetGenerator( )
        {
            return new ResourceUriGenerator( );
        }

    }
}
