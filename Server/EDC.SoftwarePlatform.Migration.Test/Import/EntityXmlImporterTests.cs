// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using NUnit.Framework;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;
using EDC.ReadiNow.Test;
using ReadiNow.ImportExport;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using System.Globalization;
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
    public class EntityXmlImporterTests
    {
        [ Explicit]
        [Test]
        [RunWithTransaction]
        public void RoundTrip( )
        {
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            EntityXmlImportResult result;
            string xml;
            long entityId;

            // Import
            using ( RunAsImportExportRole() )
            using ( Stream stream = GetStream( "SimplifiedReport.xml" ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                xml = reader.ReadToEnd( );
                result = importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }
            entityId = result.RootEntities.Single( );

            // Export
            IEntityXmlExporter exporter = Factory.EntityXmlExporter;
            string xml2 = exporter.GenerateXml( entityId, EntityXmlExportSettings.Default );
        }

        /// <summary>
        /// Import an entity and verify that all required content is present.
        /// Tests scenarios where aliases are unavailable, and relationships in both directions.
        /// </summary>
        [Test]
        [RunWithTransaction]
        public void Import_NoAliases_AllFields( )
        {
            IEntityRepository repository = Factory.EntityRepository;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            EntityXmlImportResult result;
            string xml;
            long entityId;
            IEntity entity;

            // Import
            using ( RunAsImportExportRole( ) )
            using ( Stream stream = GetStream( "AllFields.xml" ) )
            using ( StreamReader reader = new StreamReader(stream) )
            {
                xml = reader.ReadToEnd( );
                result = importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }

            // Check
            Assert.That( result, Is.Not.Null );
            Assert.That( result.RootEntities, Is.Not.Null );
            entityId = result.RootEntities.Single( );

            entity = repository.Get( entityId );
            try
            {
                Assert.That( entity.GetField<string>( "core:name" ), Is.EqualTo( "ImportTest" ) );
                Assert.That( entity.GetField<string>( "core:description" ), Is.EqualTo( "" ) );
                Assert.That( entity.GetField<string>( "test:afString" ), Is.EqualTo( "Hello" ) );
                Assert.That( entity.GetField<bool>( "test:afBoolean" ), Is.EqualTo( true ) );
                Assert.That( entity.GetField<DateTime>( "test:afDateTime" ), Is.EqualTo( DateTime.Parse( "2016-12-19 08:02:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal ) ) );
                Assert.That( entity.GetField<DateTime>( "test:afDate" ), Is.EqualTo( DateTime.Parse( "2016-12-24 13:00:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal ) ) );
                Assert.That( entity.GetField<DateTime>( "test:afTime" ), Is.EqualTo( DateTime.Parse( "1753-01-01 20:51:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal ) ) );
                Assert.That( entity.GetField<decimal>( "test:afDecimal" ), Is.EqualTo( 123.4M ) );
                Assert.That( entity.GetField<decimal>( "test:afCurrency" ), Is.EqualTo( 123.45M ) );
                Assert.That( entity.GetField<int>( "test:afNumber" ), Is.EqualTo( 123 ) );
                Assert.That( entity.GetField<string>( "test:afMultiline" ), Is.EqualTo( "\nMulti\nLine\n" ) );

                Resource resource = entity.Cast<Resource>( );
                Assert.That( resource.IsOfType?.First()?.Alias, Is.EqualTo( "test:allFields" ) );
                Assert.That( resource.SecurityOwner?.Alias, Is.EqualTo( "core:administratorUserAccount" ) );

                var relationships = entity.GetRelationships( "test:herbs" );
                Assert.That( relationships, Has.Count.EqualTo( 1 ) );
                Assert.That( relationships.Single( ).Alias, Is.EqualTo( "aaBasil" ) );

                long snacksRelId = Entity.GetIdFromUpgradeId( new Guid( "05cd3a57-7b02-4bd0-aadc-272782e971b7" ) );
                var revRel = entity.GetRelationships( snacksRelId, Direction.Reverse );
                Assert.That( revRel, Has.Count.EqualTo( 1 ) );
                Assert.That( revRel.Single( ).Alias, Is.EqualTo( "aaGarlicBread" ) );
            }
            finally
            {
                entity.AsWritable().Delete( );
            }
        }

        /// <summary>
        /// Import a reverseAlias nested relationship.
        /// </summary>
        [Test]
        [RunWithTransaction]
        public void Import_ReverseAlias( )
        {
            IEntityRepository repository = Factory.EntityRepository;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            EntityXmlImportResult result;
            string xml;
            long entityId;

            // Import
            using ( RunAsImportExportRole( ) )
            using ( Stream stream = GetStream( "ReverseAlias.xml" ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                xml = reader.ReadToEnd( );
                result = importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }

            // Check
            Assert.That( result, Is.Not.Null );
            Assert.That( result.RootEntities, Is.Not.Null );
            entityId = result.RootEntities.Single( );

            Definition type = repository.Get<Definition>( entityId );
            Assert.That( type.Fields, Has.Count.EqualTo( 1 ) );
        }

        /// <summary>
        /// Import an entity and verify that all required content is present.
        /// Tests scenarios where aliases are unavailable, and relationships in both directions.
        /// </summary>
        [Test]
        [RunWithTransaction]
        public void Import_Nested_SimplifiedReport( )
        {
            IEntityRepository repository = Factory.EntityRepository;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            EntityXmlImportResult result;
            string xml;
            long entityId;
            Report report;

            // Import
            using ( RunAsImportExportRole( ) )
            using ( Stream stream = GetStream( "SimplifiedReport.xml" ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                xml = reader.ReadToEnd( );
                result = importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }

            // Check
            Assert.That( result, Is.Not.Null );
            Assert.That( result.RootEntities, Is.Not.Null );
            entityId = result.RootEntities.Single( );

            report = repository.Get<Report>( entityId );
            try
            {
                Assert.That( report, Is.Not.Null );
                Assert.That( report.Name, Is.EqualTo( "Simple herb" ) );
                ResourceReportNode rootNode = report.RootNode?.As<ResourceReportNode>( );
                Assert.That( rootNode, Is.Not.Null, "Root node" );
                Assert.That( rootNode.ResourceReportNodeType, Is.Not.Null );
                Assert.That( rootNode.ResourceReportNodeType.Alias, Is.EqualTo( "test:herb" ) );
                Assert.That( report.ReportColumns, Has.Count.EqualTo( 1 ), "ReportColumns" );
                ReportColumn column = report.ReportColumns.First( );
                Assert.That( column, Is.Not.Null, "Column" );
                Assert.That( column.Name, Is.EqualTo( "AA_Herb" ) );
                Assert.That( column.ColumnIsHidden == true, Is.True, "ColumnIsHidden" );
                FieldExpression fieldExpr = column.ColumnExpression?.As<FieldExpression>( );
                Assert.That( fieldExpr, Is.Not.Null, "FieldExpression" );
                Assert.That( fieldExpr.FieldExpressionField.Alias, Is.EqualTo( "core:name" ) );
                Assert.That( fieldExpr.SourceNode?.Id, Is.EqualTo( rootNode?.Id ), "SourceNode" );
                Assert.That( column.ColumnGrouping, Has.Count.EqualTo( 1 ), "ColumnGrouping" );
                ReportRowGroup grouping = column.ColumnGrouping.First( );
                Assert.That( grouping, Is.Not.Null, "ColumnGrouping" );
                Assert.That( grouping.GroupingPriority, Is.EqualTo( 1 ), "GroupingPriority" );
            }
            finally
            {
                Entity.Delete( entityId );
            }
        }

        [Test]
        [RunWithTransaction]
        public void Import_Binary( )
        {
            IEntityRepository repository = Factory.EntityRepository;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            EntityXmlImportResult result;
            string xml;
            long entityId;
            IEntity entity;

            // Import
            using ( RunAsImportExportRole( ) )
            using ( Stream stream = GetStream( "TestPhoto.xml" ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                xml = reader.ReadToEnd( );
                result = importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }

            // Check
            Assert.That( result, Is.Not.Null );
            Assert.That( result.RootEntities, Is.Not.Null );
            entityId = result.RootEntities.Single( );

            entity = repository.Get( entityId );
            try
            {
                string expectedHash = "BN7ED5KVSPBOQQL2OELSONW25BI5MBPWS2JSRHQDIO2KW4UBGGMA";
                string actualHash = entity.GetField<string>( "core:fileDataHash" );
                Assert.That( actualHash, Is.EqualTo( expectedHash ) );

                using ( Stream binary = Factory.BinaryFileRepository.Get( actualHash ) )
                {
                    Assert.That( binary.Length, Is.EqualTo( 398 ) );
                }
            }
            finally
            {
                entity.AsWritable( ).Delete( );
            }
        }

        [Test]
        [RunWithTransaction]
        public void Import_Document( )
        {
            IEntityRepository repository = Factory.EntityRepository;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            EntityXmlImportResult result;
            string xml;
            long entityId;
            IEntity entity;

            // Import
            using ( RunAsImportExportRole( ) )
            using ( Stream stream = GetStream( "TestDocument.xml" ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                xml = reader.ReadToEnd( );
                result = importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }

            // Check
            Assert.That( result, Is.Not.Null );
            Assert.That( result.RootEntities, Is.Not.Null );
            entityId = result.RootEntities.Single( );

            entity = repository.Get( entityId );
            try
            {
                string expectedHash = "RV6PPKR5NJHP4GU76KIZ2HXB5BX3SPRKDQAIWQL33W7ADWUEOUJQ";
                string actualHash = entity.GetField<string>( "core:fileDataHash" );
                Assert.That( actualHash, Is.EqualTo( expectedHash ) );

                using ( Stream binary = Factory.DocumentFileRepository.Get( actualHash ) )
                {
                    Assert.That( binary.Length, Is.EqualTo( 11590 ) );
                }
            }
            finally
            {
                entity.AsWritable( ).Delete( );
            }
        }

        /// <summary>
        /// Verify that a user cannot export unless they are in the export/import role.
        /// </summary>
        [Test]
        [ExpectedException(typeof(PlatformSecurityException), ExpectedMessage = "Must be a member of the 'Import/Export' role." )]
        [RunWithTransaction]
        public void Security_CheckRole( )
        {
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            string xml;

            var user = Entity.Create<UserAccount>( );
            using ( new SetUser( user ) )
            using ( Stream stream = GetStream( "AllFields.xml" ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                xml = reader.ReadToEnd( );
                importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }
        }

        /// <summary>
        /// Verify that a user cannot export unless they are in the export/import role.
        /// </summary>
        [Test]
        [ExpectedException( typeof( PlatformSecurityException ), ExpectedMessage = "Permission denied to create records of type: AA_All Fields" )]
        [RunWithTransaction]
        public void Security_CheckCreateTypes( )
        {
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            string xml;

            var user = Entity.Create<UserAccount>( );
            user.UserHasRole.Add( Entity.Get<Role>( "core:importExportRole" ) );
            using ( new SetUser( user ) )
            using ( Stream stream = GetStream( "AllFields.xml" ) )
            using ( StreamReader reader = new StreamReader( stream ) )
            {
                xml = reader.ReadToEnd( );
                importer.ImportXml( xml, EntityXmlImportSettings.Default );
            }
        }

        /// <summary>
        /// Verify that a user cannot export unless they have modify permissions on the items being imported
        /// </summary>
        [TestCase( false )]
        [TestCase( true )]
        [RunWithTransaction]
        public void Security_CheckInstancesSecurity( bool grant )
        {
            IEntityXmlExporter exporter = Factory.EntityXmlExporter;
            IEntityXmlImporter importer = Factory.EntityXmlImporter;
            string xml;

            Definition type = new Definition( );
            type.Inherits.Add( UserResource.UserResource_Type );
            type.Name = Guid.NewGuid( ).ToString( );
            type.Save( );

            Resource instance = Entity.Create( type.Id ).As<Resource>( );
            instance.Name = "Mordor";
            instance.Save( );

            // Generate the XML as admin
            using ( RunAsImportExportRole( ) )
            {
                xml = exporter.GenerateXml( instance.Id, EntityXmlExportSettings.Default );
            }

            // Create a user that can create the types, but not modify the instance
            var user = Entity.Create<UserAccount>( );
            user.Name = "Frodo";
            user.UserHasRole.Add( Entity.Get<Role>( "core:importExportRole" ) );
            user.Save( );

            new AccessRuleFactory( ).AddAllowCreate( user.As<Subject>( ), type.As<SecurableEntity>( ) ).Save( );
            if ( grant )
            {
                new AccessRuleFactory( ).AddAllowByQuery( user.As<Subject>( ),
                    type.As<SecurableEntity>( ),
                    new[] { Permissions.Read, Permissions.Modify },
                    TestQueries.Entities( new EntityRef( type.Id ) ).ToReport( ) ).Save( );
            }

            var res = new EntityAccessControlChecker( ).CheckTypeAccess( new[] { type.As<EntityType>() }, Permissions.Create, user );

            // Try to reimport as non-priv user
            using ( new SetUser( user ) )
            {
                Action run = ( ) => importer.ImportXml( xml, EntityXmlImportSettings.Default );

                if ( grant )
                    run( );
                else
                    Assert.That( Assert.Throws<PlatformSecurityException>( ( ) => run( ) ).Message, Is.StringStarting( "Frodo does not have edit access to Mordor" ) );
            }
        }

        public static Stream GetStream( string testFileName )
        {
            Assembly assembly = Assembly.GetExecutingAssembly( );
            return assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.Migration.Test.Import.TestFiles." + testFileName );
        }

        public static IDisposable RunAsImportExportRole( )
        {
            UserAccount user = new UserAccount( );
            user.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            user.Name = Guid.NewGuid( ).ToString( );
            user.UserHasRole.Add( Entity.Get<Role>( "core:administratorRole" ) );
            user.UserHasRole.Add( Entity.Get<Role>( "core:importExportRole" ) );
            user.Save( );
            return new SetUser( user );
        }
    }
}

