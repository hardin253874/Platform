// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using ReadiNow.Connector.Test.Payload;
using EDC.ReadiNow.Core;
using System.Net;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;

namespace ReadiNow.Connector.Test.Service
{
    /// <summary>
    /// Test integation of entire service pipeline, except web.
    /// </summary>
    [TestFixture]
    class ServiceIntegrationTests
    {
        string ApiKey = "6cb36a1cd60e460bbbfce5af03eb9507"; // or whatever
        string TenantName = RunAsDefaultTenant.DefaultTenantName;
        string ApiAddress = "testapi";
        string EndpointAddress = "testtype";

        UserAccount userAccount;
        ApiKey key;
        Api api;
        EntityType type;
        ApiResourceMapping mapping;
        ApiResourceEndpoint endpoint;
        ApiFieldMapping fieldMapping;
        StringField stringField;
        IEntity scenarioInstance;
        EntityType type2;
        Relationship lookup;
        Relationship relationship;
        ApiRelationshipMapping lookupMapping;
        ApiRelationshipMapping relationshipMapping;
        IEntity foreignInstance;
        string foreignName;

            
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void CreateScenario( )
        {
            CreateScenarioImpl( null, ( ) => new EntityRef[] { } );
        }

        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void Cleanup( )
        {
            Entity.Delete( Entity.GetInstancesOfType<ApiKey>( ).Select( e => e.Id ) );
            Entity.Delete( Entity.GetInstancesOfType<Api>( ).Select( e => e.Id ) );
            Entity.Delete( Entity.GetInstancesOfType<ApiResourceEndpoint>( ).Select( e => e.Id ) );
            Entity.Delete( Entity.GetInstancesOfType<ApiResourceMapping>( ).Select( e => e.Id ) );
            Entity.Delete( Entity.GetInstancesOfType<ApiFieldMapping>( ).Select( e => e.Id ) );
        }

        [Test]
        [RunWithTransaction]
        public void Test_Resource_PostCreate( )
        {
            // Getting Forbidden? Or ConnectorConfigException?
            // Maybe there's duplicate copies of these objects in the DB.

            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;
            string tenantName = "EDC";

            CreateScenarioImpl( null, ( ) => new [ ] { Permissions.Create } );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new[] { ApiAddress, EndpointAddress },
                Verb = ConnectorVerb.Post,
                Payload = JilDynamicObjectReaderTests.GetReader( @"{ ""field1"":""hello"", ""lookup1"":""" + foreignName + @""", ""rel1"":[""" + foreignName + @"""] }"),
                ControllerRootPath = "https://whatever/SpApi/api/"
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest( request );

            // Check result
            Assert.That( response, Is.Not.Null );
            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.Created ) );
            Assert.That( response.Headers["Location"], Is.Not.Null );
            string location = response.Headers [ "Location" ];
            string expectedPrefix = "https://whatever/SpApi/api/" + tenantName + "/" + ApiAddress + "/" + EndpointAddress + "/";
            Assert.That( location, Is.StringStarting(expectedPrefix));
            string suffix = location.Substring( expectedPrefix.Length );
            Guid guid;
            Assert.That( Guid.TryParse( suffix, out guid ), Is.True );

            // Check instance got created
            using ( new TenantAdministratorContext( TenantName ) )
            {
                var instances = Entity.GetInstancesOfType( type.Id ).ToList( );
                Assert.That( instances, Has.Count.EqualTo( 1 ) );
                IEntity instance = instances [ 0 ];

                string fieldValue = instance.GetField<string>( stringField );
                Assert.That( fieldValue, Is.EqualTo( "hello" ) );

                var relInst = instance.GetRelationships( relationship ).First();
                Assert.That( relInst.Id, Is.EqualTo(foreignInstance.Id) );
            }
        }

        [Test]
        [RunWithTransaction]
        public void Test_Resource_PostCreate_WithoutOptionalRels_Bug26778()
        {
            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;

            CreateScenarioImpl(null, () => new[] { Permissions.Create });

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new[] { ApiAddress, EndpointAddress },
                Verb = ConnectorVerb.Post,
                Payload = JilDynamicObjectReaderTests.GetReader(@"{ ""field1"":""hello"" }"),
                ControllerRootPath = "https://whatever/SpApi/api/"
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest(request);

            // Check result
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            
            // Check instance got created
            using (new TenantAdministratorContext(TenantName))
            {
                var instances = Entity.GetInstancesOfType(type.Id).ToList();
                Assert.That(instances, Has.Count.EqualTo(1));
                IEntity instance = instances[0];
            }
        }

        [TestCase("{}")]
        [RunWithTransaction]
        public void Test_Resource_PostCreate_WithEmptyBody_Bug26808( string body )
        {
            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;

            CreateScenarioImpl(null, () => new[] { Permissions.Create });

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new[] { ApiAddress, EndpointAddress },
                Verb = ConnectorVerb.Post,
                Payload = JilDynamicObjectReaderTests.GetReader(body),
                ControllerRootPath = "https://whatever/SpApi/api/"
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest(request);

            // Check result
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.MessageResponse.PlatformMessageCode, Is.EqualTo("E1009"));
        }

        [Test]
        [RunWithTransaction]
        public void Test_Resource_PostCreate_WithInvalidLookup_Bug26761()
        {
            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;

            CreateScenarioImpl(null, () => new[] { Permissions.Create });

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new[] { ApiAddress, EndpointAddress },
                Verb = ConnectorVerb.Post,
                Payload = JilDynamicObjectReaderTests.GetReader(@"{ ""field1"":""hello"", ""lookup1"":""IDontExist"" }"),
                ControllerRootPath = "https://whatever/SpApi/api/"
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest(request);

            // Check result
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.MessageResponse.PlatformMessageCode, Is.EqualTo("E1003"));
            Assert.That(response.MessageResponse.Message, Is.EqualTo("No resources were found that matched 'IDontExist'."));
        }

        [Test]
        [RunWithTransaction]
        public void Test_Resource_PutUpdate( )
        {
            // Getting Forbidden? Or ConnectorConfigException?
            // Maybe there's duplicate copies of these objects in the DB.

            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;
            string resourceName = "ResourceToUpdate";

            CreateScenarioImpl( resourceName, ( ) => new [ ] { Permissions.Read, Permissions.Modify } );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new [ ] { ApiAddress, EndpointAddress, resourceName },
                Verb = ConnectorVerb.Put,
                Payload = JilDynamicObjectReaderTests.GetReader( @"{ ""field1"":""hello"", ""lookup1"":""" + foreignName + @""", ""rel1"":[""" + foreignName + @"""] }" )
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest( request );

            // Check result
            Assert.That( response, Is.Not.Null );
            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );

            // Check instance got created
            using ( new TenantAdministratorContext( TenantName ) )
            {
                var instances = Entity.GetInstancesOfType( type.Id ).ToList( );
                Assert.That( instances, Has.Count.EqualTo( 1 ) );
                IEntity instance = instances [ 0 ];

                string fieldValue = instance.GetField<string>( stringField );
                Assert.That( fieldValue, Is.EqualTo( "hello" ) );

                var relInst = instance.GetRelationships( relationship ).First( );
                Assert.That( relInst.Id, Is.EqualTo( foreignInstance.Id ) );
            }
        }

        [TestCase("{}")]
        [RunWithTransaction]
        public void Test_Resource_PutUpdate_WithEmptyBody_Bug26808(string body)
        {
            // Getting Forbidden? Or ConnectorConfigException?
            // Maybe there's duplicate copies of these objects in the DB.

            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;
            string resourceName = "ResourceToUpdate";

            CreateScenarioImpl(resourceName, () => new[] { Permissions.Read, Permissions.Modify });

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new[] { ApiAddress, EndpointAddress, resourceName },
                Verb = ConnectorVerb.Put,
                Payload = JilDynamicObjectReaderTests.GetReader(body)
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest(request);

            // Check result
            Assert.That(response, Is.Not.Null);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.MessageResponse.PlatformMessageCode, Is.EqualTo("E1009"));
        }

        [Test]
        [RunWithTransaction]
        public void Test_Resource_Delete( )
        {
            // Getting Forbidden? Or ConnectorConfigException?
            // Maybe there's duplicate copies of these objects in the DB.

            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;
            string resourceName = "ResourceToDelete";

            CreateScenarioImpl( resourceName, () => new [ ] { Permissions.Read, Permissions.Delete } );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new [ ] { ApiAddress, EndpointAddress, resourceName },
                Verb = ConnectorVerb.Delete
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest( request );

            // Check result
            Assert.That( response, Is.Not.Null );
            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );

            // Check instance got created
            using ( new TenantAdministratorContext( TenantName ) )
            {
                var instances = Entity.GetInstancesOfType( type.Id ).ToList( );
                Assert.That( instances, Has.Count.EqualTo( 0 ) );
            }
        }

        [Test]
        [RunWithTransaction]
        public void Test_Cardinality_Error( )
        {
            // Getting Forbidden? Or ConnectorConfigException?
            // Maybe there's duplicate copies of these objects in the DB.

            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;

            CreateScenarioImpl( null, ( ) => new [ ] { Permissions.Create } );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new [ ] { ApiAddress, EndpointAddress },
                Verb = ConnectorVerb.Post,
                Payload = JilDynamicObjectReaderTests.GetReader( @"{ ""lookup1"":""" + foreignName + @""" }" ),
                ControllerRootPath = "https://whatever/SpApi/api/"
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest( request );
            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.Created ) );

            response = service.HandleRequest( request );
            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.BadRequest ) );
            Assert.That( response.MessageResponse.PlatformMessageCode, Is.EqualTo( "E1007" ) );
        }

        [Test]
        [RunWithTransaction]
        public void Test_FieldValidation_Error( )
        {
            // Getting Forbidden? Or ConnectorConfigException?
            // Maybe there's duplicate copies of these objects in the DB.

            ConnectorRequest request;
            ConnectorResponse response;
            IConnectorService service;
            string tooLong = new string( 'z', 1000 );

            CreateScenarioImpl( null, ( ) => new [ ] { Permissions.Create } );

            // Define request
            request = new ConnectorRequest
            {
                TenantName = TenantName,
                QueryString = new Dictionary<string, string> { { "key", ApiKey } },
                ApiPath = new [ ] { ApiAddress, EndpointAddress },
                Verb = ConnectorVerb.Post,
                Payload = JilDynamicObjectReaderTests.GetReader( @"{ ""field1"":""" + tooLong + @""" }" ),
                ControllerRootPath = "https://whatever/SpApi/api/"
            };

            // Get service
            service = Factory.ConnectorService;

            // Place request
            response = service.HandleRequest( request );
            Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.BadRequest ) );
            Assert.That( response.MessageResponse.PlatformMessageCode, Is.EqualTo( "E1008" ) );
        }

        private void CreateScenarioImpl( string testInstanceName, Func<EntityRef[]> permissionsCallback )
        {
            // Define key and user
            using ( new TenantAdministratorContext( TenantName ) )
            {
                // Define schema
                type = new EntityType( );
                type.Inherits.Add(UserResource.UserResource_Type);
                type.Name = "Test type " + Guid.NewGuid( );
                type.Save( );

                type2 = new EntityType( );
                type2.Inherits.Add(UserResource.UserResource_Type);
                type2.Name = "Test type2 " + Guid.NewGuid( );
                type2.Save( );

                stringField = new StringField( );
                stringField.Name = "Field 1";
                stringField.FieldIsOnType = type;
                stringField.MaxLength = 50;
                stringField.Save( );

                lookup = new Relationship( );
                lookup.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
                lookup.FromType = type;
                lookup.ToType = type2;

                relationship = new Relationship( );
                relationship.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                relationship.FromType = type;
                relationship.ToType = type2;

                // Define API
                mapping = new ApiResourceMapping( );
                mapping.Name = "Test mapping " + Guid.NewGuid( ); ;
                mapping.MappedType = type;
                mapping.Save( );

                fieldMapping = new ApiFieldMapping( );
                fieldMapping.Name = "field1";
                fieldMapping.MappedField = stringField.As<Field>( );
                fieldMapping.MemberForResourceMapping = mapping;
                fieldMapping.Save( );

                lookupMapping = new ApiRelationshipMapping( );
                lookupMapping.Name = "lookup1";
                lookupMapping.MappedRelationship = lookup;
                lookupMapping.MemberForResourceMapping = mapping;
                lookupMapping.Save( );

                relationshipMapping = new ApiRelationshipMapping( );
                relationshipMapping.Name = "rel1";
                relationshipMapping.MappedRelationship = relationship;
                relationshipMapping.MemberForResourceMapping = mapping;
                relationshipMapping.Save( );

                endpoint = new ApiResourceEndpoint( );
                endpoint.Name = "Test endpoint " + Guid.NewGuid( ); ;
                endpoint.ApiEndpointAddress = EndpointAddress;
                endpoint.EndpointResourceMapping = mapping;
                endpoint.ApiEndpointEnabled = true;
                endpoint.EndpointCanCreate = true;
                endpoint.EndpointCanUpdate = true;
                endpoint.EndpointCanDelete = true;
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

                if ( testInstanceName != null )
                {
                    scenarioInstance = Entity.Create( type );
                    scenarioInstance.SetField( "core:name", testInstanceName );
                    scenarioInstance.Save( );
                }

                foreignName = "Foreign" + Guid.NewGuid( ).ToString( );
                foreignInstance = Entity.Create( type2 );
                foreignInstance.SetField( "core:name", foreignName );
                foreignInstance.Save( );

                // Grant create
                var permissions = permissionsCallback( );
                IAccessRuleFactory accessControlHelper = new AccessRuleFactory( );
                if ( permissions [ 0 ] == Permissions.Create )
                {
                    accessControlHelper.AddAllowCreate( userAccount.As<Subject>( ), type.As<SecurableEntity>( ) );
                }
                else if ( permissions.Length > 0 )
                {
                    accessControlHelper.AddAllowByQuery( userAccount.As<Subject>( ), type.As<SecurableEntity>( ), permissions, TestQueries.Entities( type ).ToReport( ) );
                }

                accessControlHelper.AddAllowByQuery( userAccount.As<Subject>( ), type2.As<SecurableEntity>( ), new [ ] { Permissions.Read, Permissions.Modify }, TestQueries.Entities( type2 ).ToReport( ) );
            }
        }


    }
}
