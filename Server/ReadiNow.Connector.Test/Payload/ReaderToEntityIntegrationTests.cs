// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Test;
using System.Dynamic;
using EDC.ReadiNow.Model;
using ReadiNow.Connector.Interfaces;
using NUnit.Framework.Constraints;
using System.Globalization;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.ReadiNow.Security;
using ReadiNow.Connector.Processing;

namespace ReadiNow.Connector.Test.Payload
{
    /// <summary>
    /// Test the integration of the JilDynamicObjectReader with the ReaderToEntityAdapterProvider and ReaderToEntityAdapter.
    /// </summary>
    [TestFixture]
    class ReaderToEntityIntegrationTests
    {
        [TestCase( "'Hello'", "Hello" )]
        [TestCase( "'Multi\nLine'", "Multi Line" )]
        [TestCase( "'  \n Multi \n \n Line \n '", "Multi Line" )]
        [TestCase( "'Multi\r\nLine'", "Multi Line" )]
        [TestCase( "'Multi\n\rLine'", "Multi Line" )]
        [TestCase( "'Multi\rLine'", "Multi Line" )]
        [TestCase( "'\nLine'", "Line" )]
        [TestCase( "'\r\nLine'", "Line" )]
        [TestCase( "'\n\rLine'", "Line" )]
        [TestCase( "'\rLine'", "Line" )]
        [TestCase( "''", null )]
        [TestCase( "null", null )]
        [RunAsDefaultTenant]
        public void Test_StringField_Valid( string jsonData, string expectedData )
        {
            RunSingleTest<string>( jsonData, "test:afString", Is.EqualTo( expectedData ) );
        }

        [TestCase( "true" )]
        [TestCase( "false" )]
        [TestCase( "1" )]
        [RunAsDefaultTenant]
        public void Test_StringField_InvalidFormat( string jsonData )
        {
            Assert.Throws<ConnectorRequestException>( ( ) => RunSingleTest<string>( jsonData, "test:afString" ), "field1 was formatted incorrectly." );
        }

        [TestCase( "'Hello'", "Hello" )]
        [TestCase( "'Multi\nLine'", "Multi\nLine" )]
        [TestCase( "'Multi\r\nLine'", "Multi\nLine" )]
        [TestCase( "'Multi\n\rLine'", "Multi\nLine" )]
        [TestCase( "'Multi\rLine'", "Multi\nLine" )]
        [TestCase( "''", null )]   // note: test:afMultiline has a defaultValue of empty, otherwise this would be null
        [TestCase( "null", null )] // note: test:afMultiline has a defaultValue of empty, otherwise this would be null
        [RunAsDefaultTenant]
        public void Test_MultiLineStringField_Valid( string jsonData, string expectedData )
        {
            RunSingleTest<string>( jsonData, "test:afMultiline", Is.EqualTo( expectedData ) );
        }

        [TestCase( "0", 0 )]
        [TestCase( "123", 123 )]
        [TestCase( "1000000000", 1000000000 )]
        [TestCase( "-1000000000", -1000000000 )]
        [TestCase( "null", null )]
        [RunAsDefaultTenant]
        public void Test_IntField_Valid( string jsonData, int? expectedData )
        {
            RunSingleTest<int?>( jsonData, "test:afNumber", Is.EqualTo( expectedData ) );
        }

        [TestCase( "'abc'" )]
        [TestCase( "'123'" )]
        [TestCase( "1000000000000000000" )]
        [TestCase( "100.1" )]
        [RunAsDefaultTenant]
        public void Test_IntField_InvalidFormat( string jsonData )
        {
            Assert.Throws<ConnectorRequestException>( () => RunSingleTest<int?>( jsonData, "test:afNumber" ), "field1 was formatted incorrectly." );
        }

        // Not sure if we like this or not .. but this is what it currently does
        [TestCase( "0", null )]
        [TestCase( "123", null )]
        [TestCase( "null", null )]
        [RunAsDefaultTenant]
        public void Test_AutoNumberField_Valid( string jsonData, int? expectedData )
        {
            RunSingleTest<int?>( jsonData, "test:afAutonumber", Is.EqualTo( expectedData ) );
        }

        [TestCase( "true", true )]
        [TestCase( "false", false )]
        [TestCase( "null", false )]
        [RunAsDefaultTenant]
        public void Test_BoolField_Valid( string jsonData, bool? expectedData )
        {
            RunSingleTest<bool?>( jsonData, "test:afBoolean", Is.EqualTo( expectedData ) );
        }

        [TestCase( "'abc'" )]
        [TestCase( "1" )]
        [TestCase( "0" )]
        [TestCase( "'True'" )]
        [TestCase( "'true'" )]
        [TestCase( "'False'" )]
        [TestCase( "'false'" )]
        [RunAsDefaultTenant]
        public void Test_BoolField_InvalidFormat( string jsonData )
        {
            Assert.Throws<ConnectorRequestException>( ( ) => RunSingleTest<bool?>( jsonData, "test:afBoolean" ), "field1 was formatted incorrectly." );
        }

        [TestCase( "0", "parseJson" )]
        [TestCase( "123", "parseJson" )]
        [TestCase( "123.456", "parseJson" )]
        [TestCase( "1000000000.001", "parseJson" )]
        [TestCase( "-1000000000.001", "parseJson" )]
        [TestCase( "null", null )]
        [RunAsDefaultTenant]
        public void Test_DecimalField_Valid( string jsonData, string expectedData )
        {
            // Note: cannot represent decimal in TestCase attribute.
            decimal? expectedDecimal = null;
            if ( expectedData != null )
                expectedDecimal = decimal.Parse( expectedData == "parseJson" ? jsonData : expectedData );

            RunSingleTest<decimal?>( jsonData, "test:afDecimal", Is.EqualTo( expectedDecimal ) );
        }

        [TestCase( "'abc'" )]
        [TestCase( "'123'" )]
        [TestCase( "true" )]
        [TestCase( "false" )]
        [RunAsDefaultTenant]
        public void Test_DecimalField_InvalidFormat( string jsonData )
        {
            Assert.Throws<ConnectorRequestException>( ( ) => RunSingleTest<decimal?>( jsonData, "test:afNumber" ), "field1 was formatted incorrectly." );
        }

        [TestCase( "0", "parseJson" )]
        [TestCase( "123", "parseJson" )]
        [TestCase( "123.45", "parseJson" )]
        [TestCase( "1000000000.01", "parseJson" )]
        [TestCase( "-1000000000.01", "parseJson" )]
        [TestCase( "null", null )]
        [RunAsDefaultTenant]
        public void Test_CurrencyField_Valid( string jsonData, string expectedData )
        {
            // Note: cannot represent decimal in TestCase attribute.
            decimal? expectedDecimal = null;
            if ( expectedData != null )
                expectedDecimal = decimal.Parse( expectedData == "parseJson" ? jsonData : expectedData );

            RunSingleTest<decimal?>( jsonData, "test:afCurrency", Is.EqualTo( expectedDecimal ) );
        }

        [TestCase( "'abc'" )]
        [TestCase( "'123'" )]
        [TestCase( "true" )]
        [TestCase( "false" )]
        [RunAsDefaultTenant]
        public void Test_CurrencyField_InvalidFormat( string jsonData )
        {
            Assert.Throws<ConnectorRequestException>( ( ) => RunSingleTest<decimal?>( jsonData, "test:afCurrency" ), "field1 was formatted incorrectly." );
        }

        [TestCase( "'2012-12-31'", "2012-12-31T00:00:00Z" )]
        [TestCase( "'1900-01-01'", "1900-01-01T00:00:00Z" )]
        [TestCase( "'2100-12-31'", "2100-12-31T00:00:00Z" )]
        [TestCase( "null", null )]
        [RunAsDefaultTenant]
        public void Test_DateField_Valid( string jsonData, string expectedData )
        {
            // Note: cannot represent decimal in TestCase attribute.
            DateTime? expectedDate = null;
            if ( expectedData != null )
                expectedDate = DateTime.Parse( expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );

            RunSingleTest<DateTime?>( jsonData, "test:afDate", Is.EqualTo( expectedDate ) );
        }

        [TestCase( "'abc'" )]
        [TestCase( "'123'" )]
        [TestCase( "123" )]
        [TestCase( "true" )]
        [TestCase( "false" )]
        [TestCase( "'06/06/2012'" )]
        [TestCase( "'2012-12-31Z'" )]
        [TestCase( "'2012-12-31T23:59:59'" )]
        [TestCase( "'2012-12-31T23:59:59.0000000Z'" )]
        [RunAsDefaultTenant]
        public void Test_DateField_InvalidFormat( string jsonData )
        {
            Assert.Throws<ConnectorRequestException>( ( ) => RunSingleTest<decimal?>( jsonData, "test:afDate" ), "field1 was formatted incorrectly." );
        }

        [TestCase( "'2012-12-31'", "2012-12-31T00:00:00Z" )]            // TODO: Consider timezones further
        [TestCase( "'1900-01-01'", "1900-01-01T00:00:00Z" )]            // TODO: Consider timezones further
        [TestCase( "'2100-12-31'", "2100-12-31T00:00:00Z" )]            // TODO: Consider timezones further
        [TestCase( "'2012-12-31T23:59'", "2012-12-31T23:59:00Z" )]      // TODO: Consider timezones further
        [TestCase( "'2012-12-31T23:59:59'", "2012-12-31T23:59:59Z" )]   // TODO: Consider timezones further
        [TestCase( "'2012-12-31T23:59Z'", "2012-12-31T23:59:00Z" )]
        [TestCase( "'2012-12-31T23:59:59Z'", "2012-12-31T23:59:59Z" )]
        [TestCase( "null", null )]
        [RunAsDefaultTenant]
        public void Test_DateTimeField_Valid( string jsonData, string expectedData )
        {
            // Note: cannot represent decimal in TestCase attribute.
            DateTime? expectedDate = null;
            if ( expectedData != null )
                expectedDate = DateTime.Parse( expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );

            RunSingleTest<DateTime?>( jsonData, "test:afDateTime", Is.EqualTo( expectedDate ) );
        }

        [TestCase( "'abc'" )]
        [TestCase( "'123'" )]
        [TestCase( "123" )]
        [TestCase( "true" )]
        [TestCase( "false" )]
        [TestCase( "'06/60/2012'" )]
        [TestCase( "'2012-12-31 23:59:59'" )]
        [TestCase( "'2012-12-31 07:00 am'" )]
        [RunAsDefaultTenant]
        public void Test_DateTimeField_InvalidFormat( string jsonData )
        {
            Assert.Throws<ConnectorRequestException>( ( ) => RunSingleTest<decimal?>( jsonData, "test:afDateTime" ), "field1 was formatted incorrectly." );
        }

        [TestCase( "'00:00'", "1753-01-01T00:00:00Z" )]
        [TestCase( "'00:00:00'", "1753-01-01T00:00:00Z" )]
        [TestCase( "'23:59'", "1753-01-01T23:59:00Z" )]
        [TestCase( "'23:59:59'", "1753-01-01T23:59:59Z" )]
        [TestCase( "null", null )]
        [RunAsDefaultTenant]
        public void Test_TimeField_Valid( string jsonData, string expectedData )
        {            
            // Note: cannot represent decimal in TestCase attribute.
            DateTime? expectedDate = null;
            if ( expectedData != null )
                expectedDate = DateTime.Parse( expectedData, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );

            RunSingleTest<DateTime?>( jsonData, "test:afTime", Is.EqualTo( expectedDate ) );
        }

        [TestCase( "'abc'" )]
        [TestCase( "'123'" )]
        [TestCase( "123" )]
        [TestCase( "true" )]
        [TestCase( "false" )]
        [TestCase( "'7:00'" )]
        [TestCase( "'07:00pm'" )]
        [TestCase( "'07:00:00 pm'" )]
        [TestCase( "'2012-12-31Z'" )]
        [TestCase( "'2012-12-31T23:59:59'" )]
        [TestCase( "'2012-12-31T23:59:59.0000000Z'" )]
        [RunAsDefaultTenant]
        public void Test_TimeField_InvalidFormat( string jsonData )
        {
            Assert.Throws<ConnectorRequestException>( ( ) => RunSingleTest<decimal?>( jsonData, "test:afTime" ), "field1 was formatted incorrectly." );
        }

        [TestCase( "null", null )]
        [TestCase( "'0ec0016c-2151-4b16-9c2b-c8583f5ef924'", "0ec0016c-2151-4b16-9c2b-c8583f5ef924" )]
        [TestCase( "'0EC0016C-2151-4B16-9C2B-C8583F5EF924'", "0ec0016c-2151-4b16-9c2b-c8583f5ef924" )]
        [TestCase( "'00000000-0000-0000-0000-000000000000'", "00000000-0000-0000-0000-000000000000" )]
        [RunAsDefaultTenant]
        public void Test_GuidField_Valid( string jsonData, string expectedData )
        {
            // Create schema
            EntityType type = Entity.Create<EntityType>( );
            type.Name = "Test Type";
            GuidField field = Entity.Create<GuidField>( );
            field.Name = "Test Field";
            field.FieldIsOnType = type;
            type.Save( );

            // Expected Guid
            Guid? expected = expectedData == null ? null : (Guid?)new Guid( expectedData );

            RunSingleTest<Guid?>( jsonData, new EntityRef( type.Id ), new EntityRef( field.Id ), Is.EqualTo( expected ) );
        }

        [TestCase( "'abc'" )]
        [TestCase( "123" )]
        [TestCase( "true" )]
        [TestCase( "'{0ec0016c-2151-4b16-9c2b-c8583f5ef924}'" )]
        [RunAsDefaultTenant]
        public void Test_GuidField_InvalidFormat( string jsonData )
        {
            // Create schema
            EntityType type = Entity.Create<EntityType>( );
            type.Name = "Test Type";
            GuidField field = Entity.Create<GuidField>( );
            field.Name = "Test Field";
            field.FieldIsOnType = type;
            type.Save( );

            Assert.Throws<ConnectorRequestException>( ( ) => RunSingleTest<Guid?>( jsonData, new EntityRef( type.Id ), new EntityRef( field.Id ) ), "field1 was formatted incorrectly." );
        }

        [TestCase( Direction.Forward, "Name" )]
        [TestCase( Direction.Reverse, "Name" )]
        [TestCase( Direction.Forward, "Guid" )]
        [TestCase( Direction.Reverse, "Guid" )]
        [TestCase( Direction.Forward, "StringField" )]
        [TestCase( Direction.Reverse, "StringField" )]
        [TestCase( Direction.Forward, "IntField" )]
        [TestCase( Direction.Reverse, "IntField" )]
        [TestCase( Direction.Forward, "Null" )]
        [TestCase( Direction.Reverse, "Null" )]
        [TestCase( Direction.Forward, "StringField,Null" )]
        [TestCase( Direction.Reverse, "IntField,Null" )]
        [RunAsDefaultTenant]
        public void Test_Lookup( Direction direction, string identityType )
        {
            string name = "Target" + Guid.NewGuid();
            
            // Create schema
            EntityType type = Entity.Create<EntityType>( );
            type.Name = "Test Type";
            type.Inherits.Add( Entity.Get<EntityType>( "core:resource" ) );
            type.Save( );
            Field stringField = new StringField( ).As<Field>( );
            Field intField = new IntField( ).As<Field>( );
            EntityType type2 = Entity.Create<EntityType>( );
            type2.Fields.Add( stringField );
            type2.Fields.Add( intField );
            type2.Name = "Test Type2";
            type.Inherits.Add( Entity.Get<EntityType>( "core:resource" ) );
            type2.Save( );
            Relationship relationship = Entity.Create<Relationship>( );
            relationship.Name = "Rel1";
            relationship.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
            relationship.FromType = direction == Direction.Forward ? type : type2;
            relationship.ToType = direction == Direction.Forward ? type2 : type;
            relationship.Save( );
            Resource target = Entity.Create( type2.Id ).AsWritable<Resource>();
            target.Name = name;
            target.SetField( stringField.Id, "StringVal" );
            target.SetField( intField.Id, 101 );
            target.Save( );

            using ( new SecurityBypassContext( ) )
            {
                new AccessRuleFactory( ).AddAllowByQuery( Entity.Get<Subject>( "core:everyoneRole" ).AsWritable<Subject>( ), type.As<SecurableEntity>( ), new [ ] { Permissions.Read, Permissions.Modify }, TestQueries.Entities( type.Id ).ToReport( ) );
                new AccessRuleFactory( ).AddAllowByQuery( Entity.Get<Subject>( "core:everyoneRole" ).AsWritable<Subject>( ), type2.As<SecurableEntity>( ), new [ ] { Permissions.Read, Permissions.Modify }, TestQueries.Entities( type2.Id ).ToReport( ) );
            }

            Field lookupField = null;

            string identity;
            if (identityType == "Name")
                identity = "\"" + name + "\"";
            else if (identityType == "Guid")
                identity = "\"" + target.UpgradeId.ToString() + "\"";
            else if (identityType.Contains( "Null" ) )
                identity = "null";
            else if ( identityType == "StringField" )
                identity = "\"StringVal\"";
            else if ( identityType == "IntField" )
                identity = "101";
            else
                throw new InvalidOperationException();

            if ( identityType.Contains( "StringField" ) )
                lookupField = stringField;
            else if ( identityType.Contains( "IntField" ) )
                lookupField = intField;

            // Create JSON
            string jsonMember = "rel1";
            string json = "{\"" + jsonMember + "\":" + identity + "}";

            // Create a mapping
            var mapping = CreateApiResourceMapping( new EntityRef(type.Id) );
            var relMapping = CreateApiRelationshipMapping( mapping, new EntityRef( relationship.Id ), jsonMember, direction == Direction.Reverse );
            relMapping.MappedRelationshipLookupField = lookupField;
            relMapping.Save( );

            // Fill entity
            IEntity entity = RunTest( json, mapping );
            IEntityRelationshipCollection<IEntity> value = entity.GetRelationships( relationship.Id, direction );

            // Assert mapping
            Assert.That( value, Is.Not.Null );
            if ( identityType.Contains("Null") )
            {
                Assert.That( value, Has.Count.EqualTo( 0 ) );
            }
            else
            {
                Assert.That( value, Has.Count.EqualTo( 1 ) );
                Assert.That( value.First( ).Id, Is.EqualTo( target.Id ) );
            }
        }

        [TestCase( Direction.Forward, "Name", 1 )]
        [TestCase( Direction.Reverse, "Name", 2 )]
        [TestCase( Direction.Forward, "Guid", 2 )]
        [TestCase( Direction.Reverse, "Guid", 1 )]
        [TestCase( Direction.Forward, "StringField", 2 )]
        [TestCase( Direction.Reverse, "StringField", 1 )]
        [TestCase( Direction.Forward, "IntField", 2 )]
        [TestCase( Direction.Reverse, "IntField", 1 )]
        [TestCase( Direction.Reverse, "Empty", 0 )]
        [TestCase( Direction.Forward, "StringField,Null", 0 )]
        [TestCase( Direction.Reverse, "IntField,Null", 0 )]
        [RunAsDefaultTenant]
        public void Test_Relationship( Direction direction, string identityType, int count )
        {
            // Create schema
            EntityType type = Entity.Create<EntityType>( );
            type.Name = "Test Type";
            type.Inherits.Add( Entity.Get<EntityType>( "core:resource" ) );
            type.Save( );
            Field stringField = new StringField( ).As<Field>( );
            Field intField = new IntField( ).As<Field>( );
            EntityType type2 = Entity.Create<EntityType>( );
            type2.Name = "Test Type2";
            type2.Fields.Add( stringField );
            type2.Fields.Add( intField );
            type.Inherits.Add( Entity.Get<EntityType>( "core:resource" ) );
            type2.Save( );
            Relationship relationship = Entity.Create<Relationship>( );
            relationship.Name = "Rel1";
            relationship.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
            relationship.FromType = direction == Direction.Forward ? type : type2;
            relationship.ToType = direction == Direction.Forward ? type2 : type;
            relationship.Save( );

            var targets = new List<Resource>( );
            for ( int i = 0; i < count; i++ )
            {
                string name = "Target" + Guid.NewGuid( );
                string stringVal = "StringVal" + i;
                int intVal = 100 + i;

                Resource target = Entity.Create( type2.Id ).AsWritable<Resource>( );
                target.SetField( stringField.Id, stringVal );
                target.SetField( intField.Id, intVal );
                target.Name = name;
                target.Save( );
                targets.Add( target );
            }


            using ( new SecurityBypassContext( ) )
            {
                new AccessRuleFactory( ).AddAllowByQuery( Entity.Get<Subject>( "core:everyoneRole" ).AsWritable<Subject>( ), type.As<SecurableEntity>( ), new [ ] { Permissions.Read, Permissions.Modify }, TestQueries.Entities( type.Id ).ToReport( ) );
                new AccessRuleFactory( ).AddAllowByQuery( Entity.Get<Subject>( "core:everyoneRole" ).AsWritable<Subject>( ), type2.As<SecurableEntity>( ), new [ ] { Permissions.Read, Permissions.Modify }, TestQueries.Entities( type2.Id ).ToReport( ) );
            }

            Field lookupField = null;
            if ( identityType.Contains( "StringField" ) )
                lookupField = stringField;
            else if ( identityType.Contains( "IntField" ) )
                lookupField = intField;

            var identities = new List<string>( );

            for ( int i = 0; i < count; i++ )
            {
                var target = targets[ i ];
                string identity;
                if ( identityType.Contains( "Null" ) )
                    continue;
                if ( identityType == "Name" )
                    identity = "\"" + target.Name + "\"";
                else if ( identityType == "Guid" )
                    identity = "\"" + target.UpgradeId + "\"";
                else if ( identityType == "StringField" )
                    identity = "\"StringVal" + i + "\"";
                else if ( identityType == "IntField" )
                    identity = ( 100 + i ).ToString( );
                else
                    throw new InvalidOperationException( );
                identities.Add( identity );
            }

            // Create JSON
            string jsonMember = "rel1";
            string json = "{\"" + jsonMember + "\":[" + string.Join(", ", identities) + "]}";

            // Create a mapping
            var mapping = CreateApiResourceMapping( new EntityRef( type.Id ) );
            var relMapping = CreateApiRelationshipMapping( mapping, new EntityRef( relationship.Id ), jsonMember, direction == Direction.Reverse );
            relMapping.MappedRelationshipLookupField = lookupField;
            relMapping.Save( );

            // Fill entity
            IEntity entity = RunTest( json, mapping );
            IEntityRelationshipCollection<IEntity> value = entity.GetRelationships( relationship.Id, direction );

            // Assert mapping
            Assert.That( value, Is.Not.Null );
            if ( identityType == "Null" )
            {
                Assert.That( value, Has.Count.EqualTo( 0 ) );
            }
            else
            {
                Assert.That( value, Has.Count.EqualTo( targets.Count ) );
                var actual = value.Select( e => e.Id ).OrderBy( id => id ).ToList( );
                var expected = targets.Select( e => e.Id ).OrderBy( id => id ).ToList( );

                Assert.That( actual, Is.EquivalentTo( expected ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Lookup_NotFound( )
        {
            // Create schema
            EntityType type = Entity.Create<EntityType>( );
            type.Name = "Test Type";
            type.Inherits.Add( Entity.Get<EntityType>( "core:resource" ) );
            type.Save( );
            EntityType type2 = Entity.Create<EntityType>( );
            type2.Name = "Test Type2";
            type.Inherits.Add( Entity.Get<EntityType>( "core:resource" ) );
            type2.Save( );
            Relationship relationship = Entity.Create<Relationship>( );
            relationship.Name = "Rel1";
            relationship.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
            relationship.FromType = type;
            relationship.ToType = type2;
            relationship.Save( );

            using ( new SecurityBypassContext( ) )
            {
                new AccessRuleFactory( ).AddAllowByQuery( Entity.Get<Subject>( "core:everyoneRole" ).AsWritable<Subject>( ), type.As<SecurableEntity>( ), new [ ] { Permissions.Read, Permissions.Modify }, TestQueries.Entities( type.Id ).ToReport( ) );
                new AccessRuleFactory( ).AddAllowByQuery( Entity.Get<Subject>( "core:everyoneRole" ).AsWritable<Subject>( ), type2.As<SecurableEntity>( ), new [ ] { Permissions.Read, Permissions.Modify }, TestQueries.Entities( type2.Id ).ToReport( ) );
            }

            string identity = "I dont exist";

            // Create JSON
            string jsonMember = "rel1";
            string json = "{\"" + jsonMember + "\":\"" + identity + "\"}";

            // Create a mapping
            var mapping = CreateApiResourceMapping( new EntityRef( type.Id ) );
            CreateApiRelationshipMapping( mapping, new EntityRef( relationship.Id ), jsonMember, false );

            // Attempt to fill entity
            Assert.Throws<ConnectorRequestException>( ( ) => RunTest( json, mapping ), "E1003 No resources were found that matched 'I dont exist'.");
        }

        [TestCase("Mapping", "null")]
        [TestCase("Mapping", "''")]
        [TestCase("Mapping", "'provided'")]
        [TestCase("Mapping", "'unprovided'")]
        [TestCase("Schema", "null")]
        [TestCase("Schema", "''")]
        [TestCase("Schema", "'provided'")]
        [TestCase("Schema", "'unprovided'")]
        [RunAsDefaultTenant]
        public void Test_MandatoryField( string mandatoryDefinedOn, string data)
        {
            // Create JSON
            string jsonMember = "member";
            string jsonData = data.Replace("'", "\"");
            string json = "{\"" + jsonMember + "\":" + jsonData + "}";
            
            if (data == "'unprovided'")
                json = json.Replace(jsonMember, "somethingelse");

            // Create a type mapping
            EntityType entityTypeResource = new EntityType();
            ApiResourceMapping typeMapping = new ApiResourceMapping();
            typeMapping.MappedType = entityTypeResource;

            // Create a member mapping
            StringField fieldResource = new StringField();
            ApiFieldMapping fieldMapping = new ApiFieldMapping();
            fieldMapping.MappedField = fieldResource.As<Field>();
            fieldMapping.Name = jsonMember;
            typeMapping.ResourceMemberMappings.Add(fieldMapping.As<ApiMemberMapping>());
            if (mandatoryDefinedOn == "Mapping")
                fieldMapping.ApiMemberIsRequired = true;
            if (mandatoryDefinedOn == "Schema")
                fieldResource.IsRequired = true;
            entityTypeResource.Save( );

            // Fill entity
            if (data == "'provided'")
            {
                IEntity entity = RunTest(json, typeMapping);
            }
            else
            {
                Assert.Throws<ConnectorRequestException>(() => RunTest(json, typeMapping), "E1010 '" + jsonMember + "' value is required.");
            }
        }

        [TestCase("Mapping", false, false)]
        [TestCase("Mapping", true, false)]
        [TestCase("Mapping", false, true)]
        [TestCase("Mapping", true, true)]
        [TestCase("Schema", false, false)]
        [TestCase("Schema", true, false)]
        [TestCase("Schema", false, true)]
        [TestCase("Schema", true, true)]
        [RunAsDefaultTenant]
        public void Test_MandatoryRelationship(string mandatoryDefinedOn, bool provided, bool isNull)
        {
            // Create JSON
            string jsonMember = "member";
            string jsonData = isNull ? "null" : "\"Test 10\"";
            string json = "{\"" + jsonMember + "\":" + jsonData + "}";

            if (!provided)
                json = json.Replace(jsonMember, "somethingelse");

            // Create a type mapping
            EntityType entityTypeResource = new EntityType();
            EntityType entityTypeResource2 = Entity.Get<EntityType>("test:allFields");
            ApiResourceMapping typeMapping = new ApiResourceMapping();
            typeMapping.MappedType = entityTypeResource;

            // Create a member mapping
            Relationship rel = new Relationship();
            rel.FromType = entityTypeResource;
            rel.ToType = entityTypeResource2;
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
            ApiRelationshipMapping relMapping = new ApiRelationshipMapping();
            relMapping.MappedRelationship = rel;
            relMapping.Name = jsonMember;
            typeMapping.ResourceMemberMappings.Add(relMapping.As<ApiMemberMapping>());
            if (mandatoryDefinedOn == "Mapping")
                relMapping.ApiMemberIsRequired = true;
            if (mandatoryDefinedOn == "Schema")
                rel.RelationshipIsMandatory = true;
            entityTypeResource.Save( );

            // Fill entity
            if (provided && !isNull)
            {
                IEntity entity = RunTest(json, typeMapping);
            }
            else
            {
                Assert.Throws<ConnectorRequestException>(() => RunTest(json, typeMapping), "E1010 '" + jsonMember + "' value is required.");
            }
        }

        [TestCase( true, "{\"member\":null}", "null" )]
        [TestCase( true, "{\"member\":\"Test 10\"}", "provided" )]
        [TestCase( true, "{\"something else\":null}", "null" )]
        [TestCase( false, "{\"something else\":null}", "default" )]
        [RunAsDefaultTenant]
        public void Test_DefaultToRelationship( bool mapped, string json, string expect )
        {
            string jsonMember = "member";

            // Create a type mapping
            EntityType entityTypeResource = new EntityType( );
            EntityType entityTypeResource2 = Entity.Get<EntityType>( "test:allFields" );
            Resource referencedValue = Factory.ScriptNameResolver.GetInstance( "Test 10", entityTypeResource2.Id ).As<Resource>( );
            Resource defaultValue = Factory.ScriptNameResolver.GetInstance( "Test 11", entityTypeResource2.Id ).As<Resource>( );
            ApiResourceMapping typeMapping = new ApiResourceMapping( );
            typeMapping.MappedType = entityTypeResource;

            // Create a member mapping
            Relationship rel = new Relationship( );
            rel.FromType = entityTypeResource;
            rel.ToType = entityTypeResource2;
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.OneToOne;
            rel.ToTypeDefaultValue = defaultValue;
            ApiRelationshipMapping relMapping = new ApiRelationshipMapping( );
            relMapping.MappedRelationship = rel;
            relMapping.Name = jsonMember;

            entityTypeResource.Save( );
            rel.Save( );

            if ( mapped )
                typeMapping.ResourceMemberMappings.Add( relMapping.As<ApiMemberMapping>( ) );

            // Fill entity
            IEntity entity = RunTest( json, typeMapping );
            IEntity target = entity.GetRelationships( rel.Id, Direction.Forward ).SingleOrDefault( );

            if ( expect == "null" )
                Assert.That( target, Is.Null );
            else
            {
                Assert.That( target, Is.Not.Null );
                if ( expect == "default" )
                    Assert.That( target.Id, Is.EqualTo( defaultValue.Id ) );
                else
                    Assert.That( target.Id, Is.EqualTo( referencedValue.Id ) );

            }
        }

        public void RunSingleTest<T>( string jsonData, string fieldAlias, IResolveConstraint constraint = null )
        {
            // Test using allFields
            RunSingleTest<T>( jsonData, new EntityRef( "test:allFields" ), new EntityRef( fieldAlias ), constraint );
        }

        public void RunSingleTest<T>( string jsonData, EntityRef typeRef, EntityRef fieldRef, IResolveConstraint constraint = null )
        {
            // Create JSON
            string jsonMember = "field1";
            string json = "{\"" + jsonMember + "\":" + jsonData + "}";

            // Create a mapping
            var mapping = CreateApiResourceMapping( typeRef );
            CreateApiFieldMapping( mapping, fieldRef, jsonMember );

            // Fill entity
            IEntity entity = RunTest( json, mapping );
            T value = entity.GetField<T>( fieldRef );
            
            // Perform test
            if ( constraint != null )
            {
                Assert.That( value, constraint );
            }
        }

        public IEntity RunTest( string json, ApiResourceMapping mapping, ReaderToEntityAdapterSettings settings = null )
        {
            if ( json != null )
            {
                json = json.Replace( "'", @"""" );
            }

            // Get dynamic object from JSON
            object raw = JilHelpers.Deserialize<object>( json );
            Assert.That( raw, Is.InstanceOf<IDynamicMetaObjectProvider>( ), "Dynamic object should be IDynamicMetaObjectProvider" );
            IDynamicMetaObjectProvider dynamicProvider = ( IDynamicMetaObjectProvider ) raw;

            // Get object reader
            IDynamicObjectReaderService objectReaderService = Factory.Current.Resolve<IDynamicObjectReaderService>( );
            IObjectReader reader = objectReaderService.GetObjectReader( dynamicProvider );

            // Settings
            ReaderToEntityAdapterSettings settingsToUse = settings ?? new ReaderToEntityAdapterSettings();

            // Run adapter provider
            IReaderToEntityAdapterProvider adapterProvider = Factory.Current.Resolve<IReaderToEntityAdapterProvider>( );
            IReaderToEntityAdapter adapter = adapterProvider.GetAdapter( mapping.Id, settingsToUse );

            IEntity entity = adapter.CreateEntity( reader, ConnectorRequestExceptionReporter.Instance );

            return entity;            
        }

        public static ApiResourceMapping CreateApiResourceMapping( EntityRef entityType )
        {
            EntityType entityTypeResource = Entity.Get<EntityType>(entityType);
            ApiResourceMapping mapping = new ApiResourceMapping( );
            mapping.MappedType = entityTypeResource;

            return mapping;
        }

        public static ApiFieldMapping CreateApiFieldMapping( ApiResourceMapping resourceMapping, EntityRef field, string memberName )
        {
            Field fieldResource = Entity.Get<Field>( field );
            ApiFieldMapping mapping = new ApiFieldMapping( );
            mapping.MappedField = fieldResource;
            mapping.Name = memberName;
            resourceMapping.ResourceMemberMappings.Add( mapping.As<ApiMemberMapping>( ) );

            return mapping;
        }

        public static ApiRelationshipMapping CreateApiRelationshipMapping( ApiResourceMapping resourceMapping, EntityRef relationship, string memberName, bool isReverse )
        {
            Relationship relResource = Entity.Get<Relationship>( relationship );
            ApiRelationshipMapping mapping = new ApiRelationshipMapping( );
            mapping.MappedRelationship = relResource;
            mapping.MapRelationshipInReverse = isReverse;
            mapping.Name = memberName;
            resourceMapping.ResourceMemberMappings.Add( mapping.As<ApiMemberMapping>( ) );

            return mapping;
        }

    }
}
