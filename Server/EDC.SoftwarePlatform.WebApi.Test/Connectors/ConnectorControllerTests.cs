// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using ReadiNow.Connector;

namespace EDC.SoftwarePlatform.WebApi.Test.Connectors
{
    [TestFixture]
    public class ConnectorControllerTests
    {
        const string ApiKey = "6cb36a1cd60e460bbbfce5af03eb9507"; // or whatever
        string TenantName = RunAsDefaultTenant.DefaultTenantName;
        string ApiAddress = "testapi";
        string EndpointAddress = "testtype";
        
        UserAccount userAccount;
        ApiKey key;
        Api api;
        EntityType type;
        EntityType type2;
        ApiResourceMapping mapping;
        ApiResourceEndpoint endpoint;
        ApiFieldMapping fieldMapping;
        Relationship lookup;
        StringField stringField;
        AccessRule accessRule;
        Resource updateInstance;
        string updateInstanceName;
        string updateInstanceDesc;
        Guid updateInstanceGuid;
        ApiRelationshipMapping lookupMapping;

        List<IEntity> cleanup;

        [TestFixtureSetUp]
        public void Setup( )
        {
            // Getting Forbidden? Or ConnectorConfigException?
            // Maybe there's duplicate copies of these objects in the DB.

            // Define key and user
            using ( new TenantAdministratorContext( TenantName ) )
            {
                // Define schema
                type = new EntityType( );
                type.Inherits.Add(UserResource.UserResource_Type);
                type.Name = "Test type " + Guid.NewGuid( );
                type.Save( );

                type2 = new EntityType();
                type2.Inherits.Add(UserResource.UserResource_Type);
                type2.Name = "Test type2 " + Guid.NewGuid();
                type2.Save();

                stringField = new StringField( );
                stringField.Name = "Field 1";
                stringField.FieldIsOnType = type;
                stringField.Save( );

                lookup = new Relationship();
                lookup.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
                lookup.FromType = type;
                lookup.ToType = type2;

                // Define API
                mapping = new ApiResourceMapping( );
                mapping.Name = "Test mapping " + Guid.NewGuid( ); ;
                mapping.MappedType = type;
                mapping.Save( );

                lookupMapping = new ApiRelationshipMapping();
                lookupMapping.Name = "lookup1";
                lookupMapping.MappedRelationship = lookup;
                lookupMapping.MemberForResourceMapping = mapping;
                lookupMapping.Save();

                fieldMapping = new ApiFieldMapping( );
                fieldMapping.Name = "field1";
                fieldMapping.MappedField = stringField.As<Field>( );
                fieldMapping.MemberForResourceMapping = mapping;
                fieldMapping.Save( );

                endpoint = new ApiResourceEndpoint( );
                endpoint.Name = "Test endpoint " + Guid.NewGuid( );
                endpoint.ApiEndpointAddress = EndpointAddress;
                endpoint.EndpointResourceMapping = mapping;
                endpoint.ApiEndpointEnabled = true;
                endpoint.EndpointCanCreate = true;
                endpoint.EndpointCanDelete = true;
                endpoint.EndpointCanUpdate = true;
                endpoint.Save( );

                api = new Api( );
                api.Name = "Test API " + Guid.NewGuid( ); ;
                api.ApiAddress = ApiAddress;
                api.ApiEnabled = true;
                api.ApiEndpoints.Add( endpoint.As<ApiEndpoint>( ) );
                api.Save( );

                // Define access
                userAccount = new UserAccount( );
                userAccount.Name = "Test user " + Guid.NewGuid( );
                userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
                userAccount.Password = "HelloWorld123!@#";
                userAccount.Save( );

                key = new ApiKey( );
                key.Name = ApiKey;
                key.ApiKeyUserAccount = userAccount;
                key.ApiKeyEnabled = true;
                key.KeyForApis.Add( api );
                key.Save( );

                updateInstance = Entity.Create( type ).AsWritable<Resource>( );
                updateInstance.Name = updateInstanceName = "ResourceToUpdate" + Guid.NewGuid( );
                updateInstance.Description = updateInstanceDesc = "ResourceToUpdate" + Guid.NewGuid( );
                updateInstance.Save( );
                updateInstanceGuid = updateInstance.UpgradeId;
                
                IAccessRuleFactory accessControlHelper = new AccessRuleFactory( );
                accessRule = accessControlHelper.AddAllowCreate( userAccount.As<Subject>( ), type.As<SecurableEntity>( ) );
                accessRule = accessControlHelper.AddAllowByQuery( userAccount.As<Subject>( ), type.As<SecurableEntity>( ), new[] { Permissions.Read, Permissions.Modify, Permissions.Delete }, TestQueries.Entities( type ).ToReport( ) );
            }

            cleanup = new List<IEntity> { userAccount, key, api, type, mapping, endpoint, fieldMapping, stringField, accessRule, updateInstance };
        }

        [TestFixtureTearDown]
        public void Teardown( )
        {
            using ( new TenantAdministratorContext( TenantName ) )
            {
                if ( cleanup != null )
                {
                    Entity.Delete( cleanup.Select( e => e.Id ) );
                }
            }
        }

        [Test]
        public void Test_PostCreate_OK( )
        {
            string uri = string.Format( "api/{0}/{1}/{2}?key={3}", TenantName, ApiAddress, EndpointAddress, ApiKey );
            Guid guid;

            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Post, null, true ) )
            {
                string json = "{ 'field1' : 'Hello world' }";
                request.PopulateBodyString( json.Replace("'",@"""" ));

                var response = request.GetResponse( );

                // check that it worked (201)
                Assert.AreEqual( HttpStatusCode.Created, response.StatusCode );

                // Check location header
                Assert.That( response.Headers.AllKeys, Contains.Item( "Location" ) );
                string location =  response.Headers["Location"];
                Assert.That( location, Is.Not.Null.And.Not.Empty );

                string expectedPrefix = string.Format( "{0}/SpApi/api/{1}/{2}/{3}/", PlatformHttpRequest.GetHost( ), TenantName, ApiAddress, EndpointAddress );
                Assert.That( location, Is.StringStarting( expectedPrefix ) );
                string suffix = location.Substring( expectedPrefix.Length );
                Assert.That( Guid.TryParse(suffix, out guid), Is.True );
            }

            // Check instance got created
            using ( new TenantAdministratorContext( TenantName ) )
            {
                var instances = Entity.GetInstancesOfType( type.Id ).ToList( );
                var filtered = instances.Where( instance => instance.GetField<string>( stringField.Id ) == "Hello world" ).ToList();
                
                int found = filtered.Count;
                Assert.That( found, Is.EqualTo( 1 ) );

                Assert.That( filtered [ 0 ].UpgradeId, Is.EqualTo(guid) );
            }
        }

        [TestCase( "{ 'field1' : 123 }", "E1001", "Value for 'field1' was formatted incorrectly.")]     // was expecting a string field, not a numeric
        [TestCase( "{ 'field1' : '123'", null, null )]     // missing brace
        [TestCase( "{ 'field1' : '123' 'field2':'123' }", null, null)]     // missing comma
        public void Test_PostCreate_InvalidFormat( string json, string expectCode, string expectMessage )
        {
            string uri = string.Format( "api/{0}/{1}/{2}?key={3}", TenantName, ApiAddress, EndpointAddress, ApiKey );

            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Post, null, true ) )
            {
                request.PopulateBodyString( json.Replace( "'", @"""" ) );

                var response = request.GetResponse( );

                // check that it worked (201)
                Assert.AreEqual( HttpStatusCode.BadRequest, response.StatusCode );

                MessageResponse message = request.DeserialiseResponseBody<MessageResponse>( );
                Assert.That( message.PlatformMessageCode, Is.EqualTo( expectCode ) );
                Assert.That( message.Message, Is.EqualTo( expectMessage ) );
            }
        }

        [TestCase("{ 'field1' : 123, 'lookup1' : 'IDontExist' }", "E1003", "No resources were found that matched 'IDontExist'.")]  // Bug 26761
        public void Test_PostCreate_WebArgumentException(string json, string expectCode, string expectMessage)
        {
            string uri = string.Format("api/{0}/{1}/{2}?key={3}", TenantName, ApiAddress, EndpointAddress, ApiKey);

            using (var request = new PlatformHttpRequest(uri, PlatformHttpMethod.Post, null, true))
            {
                request.PopulateBodyString(json.Replace("'", @""""));

                var response = request.GetResponse();

                // check that it worked (201)
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

                MessageResponse message = request.DeserialiseResponseBody<MessageResponse>();
                Assert.That(message.PlatformMessageCode, Is.EqualTo(expectCode));
                Assert.That(message.Message, Is.EqualTo(expectMessage));
            }
        }

        [TestCase( "By Name" )]
        [TestCase( "By Guid" )]
        public void Test_PutUpdate_OK( string byMethod )
        {
            string newFieldValue = "Field Updated " + byMethod;
            string id = byMethod == "By Name" ? updateInstanceName : updateInstanceGuid.ToString( );

            string uri = string.Format( "api/{0}/{1}/{2}/{3}?key={4}", TenantName, ApiAddress, EndpointAddress, id, ApiKey );

            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Put, null, true ) )
            {
                string json = "{ 'field1' : '" + newFieldValue + "' }";
                request.PopulateBodyString( json.Replace( "'", @"""" ) );

                var response = request.GetResponse( );

                // check that it worked (200)
                Assert.AreEqual( HttpStatusCode.OK, response.StatusCode );
            }

            // Check instance got created
            using ( new TenantAdministratorContext( TenantName ) )
            {
                Resource instance = Entity.Get<Resource>( updateInstance.Id ).AsWritable<Resource>();
                instance.Description = Guid.NewGuid( ).ToString( ); // Hack around cache issue                
                instance.Save( );
                string fieldValue = instance.GetField<string>( stringField.Id );

                Assert.That( fieldValue, Is.EqualTo( newFieldValue ) );
            }
        }

        [TestCase( "By Name" )]
        [TestCase( "By Guid" )]
        public void Test_Delete_OK( string byMethod )
        {
            string name = "ResourceToDelete" + Guid.NewGuid( ).ToString( );
                
            // Create instance
            long id;
            string idString;
            using ( new TenantAdministratorContext( TenantName ) )
            {
                IEntity instance = Entity.Create( type );
                instance.SetField( "core:name", name );
                instance.Save( );
                id = instance.Id;
                idString = byMethod == "By Name" ? name : instance.UpgradeId.ToString( );
            }            

            // Test
            string uri = string.Format( "api/{0}/{1}/{2}/{3}?key={4}", TenantName, ApiAddress, EndpointAddress, idString, ApiKey );

            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Delete, null, true ) )
            {
                var response = request.GetResponse( );

                // check that it worked (200)
                Assert.AreEqual( HttpStatusCode.OK, response.StatusCode );
            }

            using ( new TenantAdministratorContext( TenantName ) )
            {
                var find = Entity.GetByName( name );
                Assert.That( find, Is.Empty );
            }
        }

        [TestCase( "api/wrongtenant/testapi/testtype?key=" + ApiKey, HttpStatusCode.Unauthorized )]  // to prevent discovery
        [TestCase( "api/edc/wrongapi/testtype?key=" + ApiKey, HttpStatusCode.Forbidden )]
        [TestCase( "api/edc/testapi/wrongendpoint?key=" + ApiKey, HttpStatusCode.NotFound )]
        [TestCase( "api?key=" + ApiKey, HttpStatusCode.NotFound )]
        public void Test_PostCreate_InvalidRequest( string uri, HttpStatusCode expectedCode )
        {
            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Post, null, true ) )
            {
                string json = "{ 'field1' : 'Hello world' }";
                request.PopulateBodyString( json.Replace( "'", @"""" ) );

                // Run
                request.GetResponse( );
                //Assert.Throws<WebException>( ( ) => request.GetResponse( ) );
                Assert.That( request.HttpWebResponse.StatusCode, Is.EqualTo( expectedCode ) );
            }
        }

        [TestCase( "api/wrongtenant/testapi/testtype/testinst?key=" + ApiKey, HttpStatusCode.Unauthorized )]  // to prevent discovery
        [TestCase( "api/edc/wrongapi/testtype/testinst?key=" + ApiKey, HttpStatusCode.Forbidden )]
        [TestCase( "api/edc/testapi/wrongendpoint/testinst?key=" + ApiKey, HttpStatusCode.NotFound )]
        [TestCase( "api/edc/testapi/testtype?key=" + ApiKey, HttpStatusCode.MethodNotAllowed )]
        [TestCase( "api/edc/testapi/testtype/wronginst?key=" + ApiKey, HttpStatusCode.NotFound )]
        [TestCase( "api?key=" + ApiKey, HttpStatusCode.NotFound )]
        public void Test_PutUpdate_InvalidRequest( string uri, HttpStatusCode expectedCode )
        {
            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Put, null, true ) )
            {
                string json = "{ 'field1' : 'Hello world' }";
                request.PopulateBodyString( json.Replace( "'", @"""" ) );

                // Run
                request.GetResponse( );
                //Assert.Throws<WebException>( ( ) => request.GetResponse( ) );
                Assert.That( request.HttpWebResponse.StatusCode, Is.EqualTo( expectedCode ) );
            }
        }

        [TestCase( "api/wrongtenant/testapi/testtype/testinst?key=" + ApiKey, HttpStatusCode.Unauthorized )]  // to prevent discovery
        [TestCase( "api/edc/wrongapi/testtype/testinst?key=" + ApiKey, HttpStatusCode.Forbidden )]
        [TestCase( "api/edc/testapi/wrongendpoint/testinst?key=" + ApiKey, HttpStatusCode.NotFound )]
        [TestCase( "api/edc/testapi/testtype?key=" + ApiKey, HttpStatusCode.MethodNotAllowed )]
        [TestCase( "api/edc/testapi/testtype/wronginst?key=" + ApiKey, HttpStatusCode.NotFound )]
        [TestCase( "api?key=" + ApiKey, HttpStatusCode.NotFound )]
        public void Test_Delete_InvalidRequest( string uri, HttpStatusCode expectedCode )
        {
            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Delete, null, true ) )
            {
                string json = "{ 'field1' : 'Hello world' }";
                request.PopulateBodyString( json.Replace( "'", @"""" ) );

                // Run
                request.GetResponse( );
                //Assert.Throws<WebException>( ( ) => request.GetResponse( ) );
                Assert.That( request.HttpWebResponse.StatusCode, Is.EqualTo( expectedCode ) );
            }
        }

        [TestCase( "api/edc/testapi/wrongendpoint?key=wrong", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc/testapi/wrongendpoint?key", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc/testapi/wrongendpoint", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc/testapi/testtype?key=wrongkey", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc/testapi/testtype?key", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc/testapi/testtype", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc/testapi?key=wrongkey", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc/testapi?key", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc/testapi", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc?key=wrongkey", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc?key", HttpStatusCode.Unauthorized )]
        [TestCase( "api/edc", HttpStatusCode.Unauthorized )]
        [TestCase( "api?key=wrongkey", HttpStatusCode.NotFound )]
        [TestCase( "api?key", HttpStatusCode.NotFound )]
        [TestCase( "api", HttpStatusCode.NotFound )]
        public void Test_PostCreate_InvalidKey( string uri, HttpStatusCode expectedCode )
        {
            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Post, null, true ) )
            {
                string json = "{ 'field1' : 'Hello world' }";
                request.PopulateBodyString( json.Replace( "'", @"""" ) );

                // Run
                request.GetResponse( );
                //Assert.Throws<WebException>( ( ) => request.GetResponse( ) );
                Assert.That( request.HttpWebResponse.StatusCode, Is.EqualTo( expectedCode ) );
            }
        }

        [TestCase( "api/right/testapi/testtype?key=wrong" )]
        [TestCase( "api/wrong/testapi/testtype?key=wrong" )]
        [TestCase( "api/right/testapi/testtype?key" )]
        [TestCase( "api/wrong/testapi/testtype?key" )]
        [TestCase( "api/right/testapi/testtype" )]
        [TestCase( "api/wrong/testapi/testtype" )]
        [TestCase( "api/right/testapi?key=wrong" )]
        [TestCase( "api/wrong/testapi?key=wrong" )]
        [TestCase( "api/right/testapi?key" )]
        [TestCase( "api/wrong/testapi?key" )]
        [TestCase( "api/right/testapi" )]
        [TestCase( "api/wrong/testapi" )]
        [TestCase( "api/right?key=wrong" )]
        [TestCase( "api/wrong?key=wrong" )]
        [TestCase( "api/right?key" )]
        [TestCase( "api/wrong?key" )]
        [TestCase( "api/right" )]
        [TestCase( "api/wrong" )]
        public void Test_Ensure_Tenant_Not_Discoverable( string uri )
        {
            // If the key does not belong to the tenant, the behavior between right tenant and wrong tenant should be indistinguisable.
            uri = uri.Replace( "right", "edc" );

            // Test correct tenant
            using ( var request = new PlatformHttpRequest( uri, PlatformHttpMethod.Post, null, true ) )
            {
                string json = "{ 'field1' : 'Hello world' }";
                request.PopulateBodyString( json.Replace( "'", @"""" ) );

                // Run
                request.GetResponse( );
                //Assert.Throws<WebException>( ( ) => request.GetResponse( ) );
                Assert.That( request.HttpWebResponse.StatusCode, Is.EqualTo( HttpStatusCode.Unauthorized ) );
            }
        }
    }
}
