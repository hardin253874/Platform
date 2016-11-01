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
    public static class ResolveEndpointHelper
    {
        public static EndpointAddressResult ResolveEndpoint(this IEndpointResolver resolver, string apiPath, bool apiOnly)
        {
            string[] parts = apiPath.Split(new[] { '/' });
            return resolver.ResolveEndpoint( parts, false );
        }            
    }

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    public class EndpointResolverTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_Null( )
        {
            IEndpointResolver resolver = GetResolver( );
            EndpointAddressResult result = resolver.ResolveEndpoint( null, false );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.ApiId, Is.EqualTo( 0 ) );
            Assert.That( result.EndpointId, Is.EqualTo( 0 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Empty( )
        {
            IEndpointResolver resolver = GetResolver( );
            EndpointAddressResult result = resolver.ResolveEndpoint( new string[] {}, false );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.ApiId, Is.EqualTo(0) );
            Assert.That( result.EndpointId, Is.EqualTo( 0 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ApiOnly_Exists( )
        {
            Api api = MakeApi( "address1" );

            IEndpointResolver resolver = GetResolver( );
            EndpointAddressResult result = resolver.ResolveEndpoint( "address1", false );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.ApiId, Is.EqualTo( api.Id ) );
            Assert.That( result.EndpointId, Is.EqualTo( 0 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ApiOnly_CaseMismatch( )
        {
            Api api = MakeApi( "aDDress1" );

            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<EndpointNotFoundException>( ( ) => resolver.ResolveEndpoint( "address1", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ApiOnly_WithOthers( )
        {
            Api api = MakeApi( "address1" );
            Api api2 = MakeApi( "whatever" );

            IEndpointResolver resolver = GetResolver( );
            EndpointAddressResult result = resolver.ResolveEndpoint( "address1", false );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.ApiId, Is.EqualTo( api.Id ) );
            Assert.That( result.EndpointId, Is.EqualTo( 0 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ApiOnly_DuplicateDisabled( )
        {
            Api api = MakeApi( "address1", true );
            Api api2 = MakeApi( "address1", false );           

            IEndpointResolver resolver = GetResolver( );
            EndpointAddressResult result = resolver.ResolveEndpoint( "address1", false );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.ApiId, Is.EqualTo( api.Id ) );
            Assert.That( result.EndpointId, Is.EqualTo( 0 ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ApiOnly_DuplicateName( )
        {
            Api api = MakeApi( "address1", true );
            Api api2 = MakeApi( "address1", true ); 

            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<ConnectorConfigException>( ( ) => resolver.ResolveEndpoint( "address1", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ApiOnly_NameNotFound( )
        {
            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<EndpointNotFoundException>( ( ) => resolver.ResolveEndpoint( "address1", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ApiOnly_Disabled( )
        {
            Api api = MakeApi( "address1", false );
            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<EndpointNotFoundException>( ( ) => resolver.ResolveEndpoint( "address1", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Endpoint_Exists( )
        {
            Api api = MakeApi( "my-api" );
            ApiResourceEndpoint endpoint = MakeEndpoint( api, "my-end-point" );

            IEndpointResolver resolver = GetResolver( );
            EndpointAddressResult result = resolver.ResolveEndpoint( "my-api/my-end-point", false );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.ApiId, Is.EqualTo( api.Id ) );
            Assert.That( result.EndpointId, Is.EqualTo( endpoint.Id ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Endpoint_WithSuffix( )
        {
            Api api = MakeApi( "my-api" );
            ApiResourceEndpoint endpoint = MakeEndpoint( api, "my-end-point" );

            IEndpointResolver resolver = GetResolver( );
            EndpointAddressResult result = resolver.ResolveEndpoint( "my-api/my-end-point/0", false );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.ApiId, Is.EqualTo( api.Id ) );
            Assert.That( result.EndpointId, Is.EqualTo( endpoint.Id ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Endpoint_NotFound( )
        {
            Api api = MakeApi( "my-api" );

            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<EndpointNotFoundException>( ( ) => resolver.ResolveEndpoint( "my-api/my-end-point", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Endpoint_CaseMismatch( )
        {
            Api api = MakeApi( "my-api" );
            ApiResourceEndpoint endpoint = MakeEndpoint( api, "my-eNd-point" );

            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<EndpointNotFoundException>( ( ) => resolver.ResolveEndpoint( "my-api/my-end-point", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Endpoint_Disabled( )
        {
            Api api = MakeApi( "my-api" );
            ApiResourceEndpoint endpoint = MakeEndpoint( api, "my-end-point", false );

            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<EndpointNotFoundException>( ( ) => resolver.ResolveEndpoint( "my-api/my-end-point", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Endpoint_DuplicateName( )
        {
            Api api = MakeApi( "my-api" );
            ApiResourceEndpoint endpoint = MakeEndpoint( api, "my-end-point" );
            ApiResourceEndpoint endpoint2 = MakeEndpoint( api, "my-end-point" );

            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<ConnectorConfigException>( ( ) => resolver.ResolveEndpoint( "my-api/my-end-point", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Endpoint_WrongApi( )
        {
            Api api = MakeApi( "my-api" );
            Api api2 = MakeApi( "my-api2" );
            ApiResourceEndpoint endpoint = MakeEndpoint( api2, "my-end-point" );

            IEndpointResolver resolver = GetResolver( );

            Assert.Throws<EndpointNotFoundException>( ( ) => resolver.ResolveEndpoint( "my-api/my-end-point", false ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Endpoint_DuplicateDisabled( )
        {
            Api api = MakeApi( "my-api" );
            ApiResourceEndpoint endpoint = MakeEndpoint( api, "my-end-point" );
            ApiResourceEndpoint endpoint2 = MakeEndpoint( api, "my-end-point", false );

            IEndpointResolver resolver = GetResolver( );
            EndpointAddressResult result = resolver.ResolveEndpoint( "my-api/my-end-point", false );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.ApiId, Is.EqualTo( api.Id ) );
            Assert.That( result.EndpointId, Is.EqualTo( endpoint.Id ) );
        }

        private Api MakeApi( string address, bool enabled = true )
        {
            Api api = new Api( );
            api.ApiAddress = address;
            api.ApiEnabled = enabled;
            api.Save( );
            return api;
        }

        private ApiResourceEndpoint MakeEndpoint( Api api, string address, bool enabled = true )
        {
            ApiResourceEndpoint endpoint = new ApiResourceEndpoint( );
            endpoint.ApiEndpointAddress = address;
            endpoint.ApiEndpointEnabled = enabled;
            if ( api != null )
            {
                endpoint.EndpointForApi = api;
            }
            endpoint.Save( );
            return endpoint;
        }

        private IEndpointResolver GetResolver( )
        {
            return new EndpointResolver( );
        }

    }
}
