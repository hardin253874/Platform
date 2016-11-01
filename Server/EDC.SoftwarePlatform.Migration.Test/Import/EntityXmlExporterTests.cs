// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using NUnit.Framework;
using System;
using System.Linq;
using System.IO;
using System.Xml;
using EDC.ReadiNow.Test;
using ReadiNow.ImportExport;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using System.Globalization;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;

namespace EDC.SoftwarePlatform.Migration.Test.Import
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [RunAsDefaultTenant]
    public class EntityXmlExporterTests
    {
        [Test]
        public void Export_Af10( )
        {
            using ( RunAsImportExportRole( ) )
            {
                IEntity entity = Entity.Get( "test:af10" );
                IEntityXmlExporter exporter = Factory.EntityXmlExporter;
                string xml = exporter.GenerateXml( entity.Id, EntityXmlExportSettings.Default );
            }
        }

        /// <summary>
        /// Export an entity and verify that all required content is present.
        /// Tests scenarios where aliases are unavailable, and relationships in both directions.
        /// </summary>
        [Test]
        public void Export_NoAliases_AllFields( )
        {
            IEntityXmlExporter exporter = Factory.EntityXmlExporter;
            IEntityRepository repository = Factory.EntityRepository;
            IEntity entity = new Entity( new EntityRef( "test:allFields" ) );

            entity.SetField( "core:name", "ImportTest" );
            entity.SetField( "core:description", "" );
            entity.SetField( "test:afString", "Hello" );
            entity.SetField( "test:afBoolean", true );
            entity.SetField( "test:afDateTime", DateTime.Parse( "2016-12-19 08:02:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal ) );
            entity.SetField( "test:afDate", DateTime.Parse( "2016-12-24 13:00:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal ) );
            entity.SetField( "test:afTime", DateTime.Parse( "1753-01-01 20:51:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal ) );
            entity.SetField( "test:afDecimal", 123.4M );
            entity.SetField( "test:afCurrency", 123.45M );
            entity.SetField( "test:afNumber", 123 );
            entity.SetField( "test:afMultiline", "\nMulti\nLine\n" );

            var rel = entity.GetRelationships( "test:herbs" );
            rel.Add( Entity.Get( "test:aaBasil" ) );

            long snacksRelId = Entity.GetIdFromUpgradeId( new Guid( "05cd3a57-7b02-4bd0-aadc-272782e971b7" ) );
            var revRel = entity.GetRelationships( snacksRelId, Direction.Reverse );
            revRel.Add( Entity.Get( "test:aaGarlicBread" ) );

            Resource resource = entity.Cast<Resource>( );
            resource.SecurityOwner = repository.Get<UserAccount>( "core:administratorUserAccount" );

            try
            {
                entity.Save( );

                string xml;
                using ( RunAsImportExportRole( ) )
                {
                    xml = exporter.GenerateXml( entity.Id, EntityXmlExportSettings.Default );
                }

                XmlDocument doc = new XmlDocument( );
                doc.LoadXml( xml );

                XmlNamespaceManager ns = new XmlNamespaceManager( doc.NameTable );
                // Note: can't get xpath to work with default namespace
                ns.AddNamespace( "c", "core" );
                ns.AddNamespace( "k", "console" );

                XmlElement e = doc.SelectSingleNode( "/c:xml/c:entities/c:group/c:entity", ns ) as XmlElement;
                Assert.That( e, Is.Not.Null );

                Assert.That( e.SelectSingleNode( "@typeId", ns ).Value, Is.EqualTo( "ac7883f2-2997-41a7-9e62-a74d19d015b2" ) );
                Assert.That( e.SelectSingleNode( "@id", ns ).Value, Is.Not.Null );
                Assert.That( e.SelectSingleNode( "c:name", ns ).InnerText, Is.EqualTo( "ImportTest" ) );
                Assert.That( e.SelectSingleNode( "c:description", ns ).InnerText, Is.EqualTo( "" ) );
                Assert.That( e.SelectSingleNode( "c:bool[@id='5b9af0f4-e812-4fd3-9b92-954393746908']", ns ).InnerText, Is.EqualTo( "True" ) );
                Assert.That( e.SelectSingleNode( "c:dateTime[@id='4e1f3930-2953-406f-8fc2-ea2ffb5b7d97']", ns ).InnerText, Is.EqualTo( "2016-12-19 08:02:00Z" ) );
                Assert.That( e.SelectSingleNode( "c:dateTime[@id='78ec955f-b28c-4178-acab-e3bf543811c5']", ns ).InnerText, Is.EqualTo( "2016-12-24 13:00:00Z" ) );
                Assert.That( e.SelectSingleNode( "c:dateTime[@id='e54de990-35ce-46b5-a89d-3226ce33529e']", ns ).InnerText, Is.EqualTo( "1753-01-01 20:51:00Z" ) );
                Assert.That( e.SelectSingleNode( "c:decimal[@id='88359587-17fe-4998-82b9-51d6bca2fdce']", ns ).InnerText, Is.EqualTo( "123.4" ) );
                Assert.That( e.SelectSingleNode( "c:decimal[@id='bfe519f4-c80a-494b-8f8c-c4d3918ee5b3']", ns ).InnerText, Is.EqualTo( "123.45" ) );
                Assert.That( e.SelectSingleNode( "c:int[@id='8639a367-eb11-404b-8300-034e41a2dffc']", ns ).InnerText, Is.EqualTo( "123" ) );
                Assert.That( e.SelectSingleNode( "c:text[@id='0851aeb2-6550-4467-810c-653a19f71d52']", ns ).InnerText, Is.EqualTo( "\nMulti\nLine\n" ) );
                Assert.That( e.SelectSingleNode( "c:text[@id='73e42305-cf4d-4efd-a082-968ac70fb07a']", ns ).InnerText, Is.EqualTo( "Hello" ) );
                Assert.That( e.SelectSingleNode( "c:securityOwner", ns ).InnerText, Is.EqualTo( "administratorUserAccount" ) );
                Assert.That( e.SelectSingleNode( "c:rel[@id='bfe88a14-85a0-4299-9d19-fd0933f7b575']", ns ).InnerText, Is.EqualTo( "bd6beb4e-05de-4c8e-bf29-0251214be1c7" ) );
                Assert.That( e.SelectSingleNode( "c:revRel[@id='05cd3a57-7b02-4bd0-aadc-272782e971b7']", ns ).InnerText, Is.EqualTo( "93b6d431-226b-44da-9df2-1d672cb16c21" ) );
            }
            finally
            {
                entity.AsWritable( ).Delete( );
            }
        }

        /// <summary>
        /// Export an entity and verify that all required content is present.
        /// Tests scenarios where aliases are available, and nested relationships in both directions.
        /// </summary>
        [Test]
        public void Export_Nested_SimplifiedReport( )
        {
            IEntityRepository repository = Factory.EntityRepository;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            EntityXmlImportResult result;
            string xml;
            long entityId = 0;

            // Import
            using ( RunAsImportExportRole() )
            using ( Stream stream = EntityXmlImporterTests.GetStream( "SimplifiedReport.xml" ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                xml = reader.ReadToEnd( );
                result = importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }

            try
            {
                entityId = result.RootEntities.Single( );

                // Export
                IEntityXmlExporter exporter = Factory.EntityXmlExporter;

                string xml2;
                using ( RunAsImportExportRole( ) )
                {
                    xml2 = exporter.GenerateXml( entityId, EntityXmlExportSettings.Default );
                }

                XmlDocument doc = new XmlDocument( );
                doc.LoadXml( xml2 );

                XmlNamespaceManager ns = new XmlNamespaceManager( doc.NameTable );
                // Note: can't get xpath to work with default namespace
                ns.AddNamespace( "c", "core" );
                ns.AddNamespace( "k", "console" );

                XmlElement r = doc.SelectSingleNode( "//c:report", ns ) as XmlElement;
                Assert.That( r, Is.Not.Null );

                Assert.That( r.SelectSingleNode( "@id", ns )?.Value, Is.EqualTo( "7faec068-162c-4e99-995b-e84f9832fae0" ) );
                Assert.That( r.SelectSingleNode( "@typeId", ns ), Is.Null );
                Assert.That( r.SelectSingleNode( "c:name", ns )?.InnerText, Is.EqualTo( "Simple herb" ) );
                Assert.That( r.SelectSingleNode( "c:rootNode", ns ), Is.Not.Null );
                Assert.That( r.SelectSingleNode( "c:rootNode/c:resourceReportNode[@id='5e7c086c-08e5-4aa7-99dc-b4e94d787548']", ns ), Is.Not.Null );
                Assert.That( r.SelectSingleNode( "c:rootNode/c:resourceReportNode[@id='5e7c086c-08e5-4aa7-99dc-b4e94d787548']/c:resourceReportNodeType", ns )?.InnerText, Is.EqualTo( "a7bf22ab-6834-4d31-8133-5795acb27d69" ) );

                XmlElement col = r.SelectSingleNode( "c:reportColumns/c:reportColumn[@id='1bb4d52c-0900-435e-9afd-dfb0a6c98b15']", ns ) as XmlElement;
                Assert.That( col, Is.Not.Null );
                Assert.That( col.SelectSingleNode( "c:name", ns )?.InnerText, Is.EqualTo( "AA_Herb" ) );
                Assert.That( col.SelectSingleNode( "c:columnIsHidden", ns )?.InnerText, Is.EqualTo( "True" ) );
                Assert.That( col.SelectSingleNode( "c:columnExpression", ns ), Is.Not.Null );
                Assert.That( col.SelectSingleNode( "c:columnExpression/c:fieldExpression[@id='1059aca5-d937-4876-9e54-661e95c14e0a']", ns ), Is.Not.Null );
                Assert.That( col.SelectSingleNode( "c:columnExpression/c:fieldExpression[@id='1059aca5-d937-4876-9e54-661e95c14e0a']/c:fieldExpressionField", ns )?.InnerText, Is.EqualTo( "name" ) );
                Assert.That( col.SelectSingleNode( "c:columnExpression/c:fieldExpression[@id='1059aca5-d937-4876-9e54-661e95c14e0a']/c:sourceNode", ns )?.InnerText, Is.EqualTo( "5e7c086c-08e5-4aa7-99dc-b4e94d787548" ) );
                Assert.That( col.SelectSingleNode( "c:columnGrouping/c:reportRowGroup[@id='22bf140b-78e6-4ca7-8169-3d817a8eb1d7']/c:groupingMethod", ns )?.InnerText, Is.EqualTo( "groupList" ) );

                XmlElement a = doc.SelectSingleNode( "//c:aliasMap", ns ) as XmlElement;
                CheckAlias( a, ns, "c:columnExpression", "b4d64bef-e364-4786-ba7e-4a6b2c62d163", "rel" );
            }
            finally
            {
                Entity.Delete( entityId );
            }
        }

        [TestCase( "rel=fwd, fc=entity, rc=entity, expect=nest" )]  // CloneByEntity in both directions isn't really a supported scenario
        [TestCase( "rel=fwd, fc=entity, rc=drop,   expect=nest" )]
        [TestCase( "rel=fwd, fc=entity, rc=ref,    expect=nest" )]
        [TestCase( "rel=fwd, fc=drop,   rc=entity, expect=drop" )]
        [TestCase( "rel=fwd, fc=drop,   rc=drop,   expect=drop" )]
        [TestCase( "rel=fwd, fc=drop,   rc=ref,    expect=drop" )]
        [TestCase( "rel=fwd, fc=ref,    rc=entity, expect=ref" )]
        [TestCase( "rel=fwd, fc=ref,    rc=drop,   expect=ref" )]
        [TestCase( "rel=fwd, fc=ref,    rc=ref,    expect=ref" )]
        //[TestCase( "rel=rev, fc=entity, rc=entity, expect=nest" )]  // CloneByEntity in both directions isn't really a supported scenario
        [TestCase( "rel=rev, fc=entity, rc=drop,   expect=drop" )]
        [TestCase( "rel=rev, fc=entity, rc=ref,    expect=ref" )]
        [TestCase( "rel=rev, fc=drop,   rc=entity, expect=nest" )]
        [TestCase( "rel=rev, fc=drop,   rc=drop,   expect=drop" )]
        [TestCase( "rel=rev, fc=drop,   rc=ref,    expect=ref" )]
        [TestCase( "rel=rev, fc=ref,    rc=entity, expect=nest" )]
        [TestCase( "rel=rev, fc=ref,    rc=drop,   expect=drop" )]
        [TestCase( "rel=rev, fc=ref,    rc=ref,    expect=ref" )]
        [TestCase( "rel=fwd, fc=drop,   rc=drop,   expect=ref,  thirdPartyEntity" )]
        [TestCase( "rel=rev, fc=drop,   rc=drop,   expect=ref,  thirdPartyEntity" )]
        [TestCase( "rel=fwd, fc=drop,   rc=drop,   expect=drop,  thirdPartyRef" )]
        [TestCase( "rel=rev, fc=drop,   rc=drop,   expect=drop,  thirdPartyRef" )]
        [RunWithTransaction]
        public void InclusionScenario( string scenario )
        {
            // Setup schema
            EntityType parentType = new Definition( ).As<EntityType>( );
            parentType.Inherits.Add( UserResource.UserResource_Type );
            EntityType childType = new Definition( ).As<EntityType>( );
            childType.Inherits.Add( UserResource.UserResource_Type );
            Relationship relType = new Relationship( );
            Relationship relType2;

            bool rev = scenario.Contains( "rel=rev" );
            relType.FromType = rev ? childType : parentType;
            relType.ToType = rev ? parentType : childType;
            relType.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;

            if ( scenario.Contains( "fc=entity" ) )
                relType.CloneAction_Enum = CloneActionEnum_Enumeration.CloneEntities;
            if ( scenario.Contains( "fc=ref" ) )
                relType.CloneAction_Enum = CloneActionEnum_Enumeration.CloneReferences;
            if ( scenario.Contains( "fc=drop" ) )
                relType.CloneAction_Enum = CloneActionEnum_Enumeration.Drop;

            if ( scenario.Contains( "rc=entity" ) )
                relType.ReverseCloneAction_Enum = CloneActionEnum_Enumeration.CloneEntities;
            if ( scenario.Contains( "rc=ref" ) )
                relType.ReverseCloneAction_Enum = CloneActionEnum_Enumeration.CloneReferences;
            if ( scenario.Contains( "rc=drop" ) )
                relType.ReverseCloneAction_Enum = CloneActionEnum_Enumeration.Drop;

            relType.Save( );

            // Set up instances
            IEntity parentInst = Entity.Create( parentType );
            IEntity childInst = Entity.Create( childType );
            childInst.SetField( "core:name", "Nested Child" );
            var relInst = parentInst.GetRelationships( relType.Id, rev ? Direction.Reverse : Direction.Forward );
            relInst.Add( childInst );

            // Clone inclusion due to third party relationship
            if ( scenario.Contains( "thirdParty" ) )
            {
                relType2 = new Relationship( );
                relType2.FromType = parentType;
                relType2.ToType = childType;
                relType2.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                relType2.CloneAction_Enum = scenario.Contains( "thirdPartyEntity" ) ? CloneActionEnum_Enumeration.CloneEntities : CloneActionEnum_Enumeration.CloneReferences;
                relType2.ReverseCloneAction_Enum = relType2.CloneAction_Enum;
                relType2.Save( );
                var relInst2 = parentInst.GetRelationships( relType2.Id, Direction.Forward );
                relInst2.Add( childInst );
            }
            parentInst.Save( );

            // Export
            IEntityXmlExporter exporter = Factory.EntityXmlExporter;
            string xml;
            using ( RunAsImportExportRole( ) )
            {
                xml = exporter.GenerateXml( parentInst.Id, EntityXmlExportSettings.Default );
            }

            // Prep tests
            XmlDocument doc = new XmlDocument( );
            doc.LoadXml( xml );
            XmlNamespaceManager ns = new XmlNamespaceManager( doc.NameTable );
            ns.AddNamespace( "c", "core" ); // silly xpath requires a namespace prefix
            ns.AddNamespace( "k", "console" );

            string parentId = XmlConvert.ToString( parentInst.UpgradeId );
            string childId = XmlConvert.ToString( childInst.UpgradeId );
            string relId = XmlConvert.ToString( relType.UpgradeId );

            string tagName = rev ? "revRel" : "rel";
            string revTagName = rev ? "rel" : "revRel";

            bool hasChildRef = doc.SelectSingleNode( $"//c:entity[@id='{parentId}']/c:{tagName}[@id='{relId}']", ns )?.InnerText == childId
                    || doc.SelectSingleNode( $"//c:entity[@id='{childId}']/c:{revTagName}[@id='{relId}']", ns )?.InnerText == parentId;
            bool hasChildNested = doc.SelectSingleNode( $"//c:entity[@id='{parentId}']/c:{tagName}[@id='{relId}']/c:entity[@id='{childId}']", ns ) != null;

            bool expectChildRef = scenario.Contains( "expect=ref" );
            bool expectChildNested = scenario.Contains( "expect=nest" );

            Assert.That( hasChildRef, Is.EqualTo( expectChildRef ), "hasChildRef" );
            Assert.That( hasChildNested, Is.EqualTo( expectChildNested ), "hasChildRef" );
        }

        [Test]
        [RunWithTransaction]
        public void Export_Binary( )
        {
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            IEntityXmlExporter exporter = Factory.EntityXmlExporter;
            EntityXmlImportResult result;
            long entityId;

            // Import - to set up test
            try
            {
                using ( RunAsImportExportRole( ) )
                using ( Stream stream = EntityXmlImporterTests.GetStream( "TestPhoto.xml" ) )
                using ( StreamReader reader = new StreamReader( stream ) )
                {
                    string xml1 = reader.ReadToEnd( );
                    result = importer.ImportXml( xml1, EntityXmlImportSettings.Default );
                    entityId = result.RootEntities.Single( );
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( "Import failed during setup for export test", ex );
            }

            // Export
            string xml;
            using ( RunAsImportExportRole( ) )
            {
                xml = exporter.GenerateXml( entityId, EntityXmlExportSettings.Default );
            }

            // Verify
            XmlDocument doc = new XmlDocument( );
            doc.LoadXml( xml );

            XmlNamespaceManager ns = new XmlNamespaceManager( doc.NameTable );
            // Note: can't get xpath to work with default namespace
            ns.AddNamespace( "c", "core" );
            ns.AddNamespace( "k", "console" );

            string expectedHash = "BN7ED5KVSPBOQQL2OELSONW25BI5MBPWS2JSRHQDIO2KW4UBGGMA";
            XmlElement e = doc.SelectSingleNode( $"/c:xml/c:binaries/c:binary[@hash='{expectedHash}']", ns ) as XmlElement;
            Assert.That( e, Is.Not.Null, "Binary element missing" );

            string base64 = e.InnerText;
            Assert.That( base64, Is.StringStarting( "H4sIAAAAAAAEAOsM8" ).And.StringEnding( "lNAJmM74+OAQAA" ) );
        }

        [Test]
        [RunWithTransaction]
        public void Export_Document( )
        {
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            IEntityXmlExporter exporter = Factory.EntityXmlExporter;
            EntityXmlImportResult result;
            long entityId;

            // Import - to set up test
            try
            {
                using ( RunAsImportExportRole( ) )
                using ( Stream stream = EntityXmlImporterTests.GetStream( "TestDocument.xml" ) )
                using ( StreamReader reader = new StreamReader( stream ) )
                {
                    string xml1 = reader.ReadToEnd( );
                    result = importer.ImportXml( xml1, EntityXmlImportSettings.Default );
                    entityId = result.RootEntities.Single( );
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( "Import failed during setup for export test", ex );
            }

            // Export
            string xml;
            using ( RunAsImportExportRole( ) )
            {
                xml = exporter.GenerateXml( entityId, EntityXmlExportSettings.Default );
            }

            // Verify
            XmlDocument doc = new XmlDocument( );
            doc.LoadXml( xml );

            XmlNamespaceManager ns = new XmlNamespaceManager( doc.NameTable );
            // Note: can't get xpath to work with default namespace
            ns.AddNamespace( "c", "core" );
            ns.AddNamespace( "k", "console" );

            string expectedHash = "RV6PPKR5NJHP4GU76KIZ2HXB5BX3SPRKDQAIWQL33W7ADWUEOUJQ";
            XmlElement e = doc.SelectSingleNode( $"/c:xml/c:documents/c:document[@hash='{expectedHash}']", ns ) as XmlElement;
            Assert.That( e, Is.Not.Null, "Document element missing" );

            string base64 = e.InnerText;
            Assert.That( base64, Is.StringStarting( "H4sIAAAAAAAEAO16ZVRcS7P2QHBCcIK7uwQJGtw" ).And.StringEnding( "r/wGi+uxzRi0AAA==" ) );
        }

        /// <summary>
        /// Verify that a user cannot export unless they are in the export/import role.
        /// </summary>
        [Test]
        [ExpectedException( typeof( PlatformSecurityException ), ExpectedMessage = "Must be a member of the 'Import/Export' role." )]
        [RunWithTransaction]
        public void Security_CheckRole( )
        {
            var user = Entity.Create<UserAccount>( );
            using ( new SetUser( user ) )
            {
                IEntity entity = Entity.Get( "core:resource" );
                IEntityXmlExporter exporter = Factory.EntityXmlExporter;
                exporter.GenerateXml( entity.Id, EntityXmlExportSettings.Default );
            }
        }

        /// <summary>
        /// Verify that a user cannot export unless they have read permission on the items being exported.
        /// </summary>
        [TestCase( false )]
        [TestCase( true )]
        [RunWithTransaction]
        public void Security_CheckRootInstancesSecurity( bool grant )
        {
            IEntityXmlExporter exporter = Factory.EntityXmlExporter;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            string xml = null;

            Definition type = new Definition( );
            type.Inherits.Add( UserResource.UserResource_Type );
            type.Name = Guid.NewGuid( ).ToString( );
            type.Save( );

            Resource instance = Entity.Create( type.Id ).As<Resource>( );
            instance.Name = "Mordor";
            instance.Save( );

            // Create a user that can create the types, but not modify the instance
            var user = Entity.Create<UserAccount>( );
            user.Name = "Frodo";
            user.UserHasRole.Add( Entity.Get<Role>( "core:importExportRole" ) );
            user.Save( );

            if ( grant )
            {
                new AccessRuleFactory( ).AddAllowReadQuery( user.As<Subject>( ),
                    type.As<SecurableEntity>( ),
                    TestQueries.Entities( new EntityRef( type.Id ) ).ToReport( ) ).Save( );
            }

            // Try to reimport as non-priv user
            using ( new SetUser( user ) )
            {
                // Generate the XML as admin
                Func<string> run = ( ) => exporter.GenerateXml( instance.Id, EntityXmlExportSettings.Default );
                
                if ( grant )
                    xml = run( );
                else
                    Assert.That( Assert.Throws<PlatformSecurityException>( ( ) => run( ) ).Message, Is.StringStarting( "Frodo does not have view access to Mordor" ) );
            }
        }

        /// <summary>
        /// Verify that a user cannot export unless they have read permission on the items being exported.
        /// </summary>
        [TestCase( false )]
        [TestCase( true )]
        [RunWithTransaction]
        public void Security_CheckRelatedInstancesSecurity( bool grant )
        {
            IEntityXmlExporter exporter = Factory.EntityXmlExporter;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;

            EntityType type = new Definition( ).As<EntityType>( );
            type.Inherits.Add( UserResource.UserResource_Type );
            type.Name = Guid.NewGuid( ).ToString( );
            type.Save( );

            EntityType type2 = new Definition( ).As<EntityType>( );
            type2.Inherits.Add( UserResource.UserResource_Type );
            type2.Name = Guid.NewGuid( ).ToString( );
            type2.Save( );

            Relationship rel = new Relationship( );
            rel.FromType = type;
            rel.ToType = type2;
            rel.CloneAction_Enum = CloneActionEnum_Enumeration.CloneEntities;
            rel.ReverseCloneAction_Enum = CloneActionEnum_Enumeration.CloneReferences;
            rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
            rel.Save( );

            Resource instance2 = Entity.Create( type2.Id ).As<Resource>( );
            instance2.Name = "Mordor";
            instance2.Save( );

            Resource instance = Entity.Create( type.Id ).As<Resource>( );
            instance.Name = "Gondor";
            instance.GetRelationships( rel.Id, Direction.Forward ).Add( instance2 );
            instance.Save( );

            // Create a user that can create the types, but not modify the instance
            var user = Entity.Create<UserAccount>( );
            user.Name = "Frodo";
            user.UserHasRole.Add( Entity.Get<Role>( "core:importExportRole" ) );
            user.Save( );

            new AccessRuleFactory( ).AddAllowReadQuery( user.As<Subject>( ),
                type.As<SecurableEntity>( ),
                TestQueries.Entities( new EntityRef( type.Id ) ).ToReport( ) ).Save( );

            if ( grant )
            {
                new AccessRuleFactory( ).AddAllowReadQuery( user.As<Subject>( ),
                    type2.As<SecurableEntity>( ),
                    TestQueries.Entities( new EntityRef( type2.Id ) ).ToReport( ) ).Save( );
            }

            // Try to reimport as non-priv user
            using ( new SetUser( user ) )
            {
                // Generate the XML as admin
                string xml = exporter.GenerateXml( instance.Id, EntityXmlExportSettings.Default );

                if ( grant )
                    Assert.That( xml, Is.StringContaining( "Mordor" ) );
                else
                    Assert.That( xml, Is.Not.StringContaining( "Mordor" ) );
            }
        }

        private void CheckAlias( XmlElement aliasMap, XmlNamespaceManager ns, string alias, string guid, string type )
        {
            Assert.That( aliasMap.SelectSingleNode( alias + "/@id", ns )?.Value, Is.EqualTo( guid ) );
            Assert.That( aliasMap.SelectSingleNode( alias + "/@type", ns )?.Value, Is.EqualTo( type ) );
        }

        public IDisposable RunAsImportExportRole( )
        {
            return EntityXmlImporterTests.RunAsImportExportRole( );
        }

    }
}

