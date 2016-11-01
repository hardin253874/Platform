// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using NUnit.Framework;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Builder;
using EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Test;
using ReadiNow.QueryEngine.ReportConverter;

namespace ReadiNow.QueryEngine.Test.Builder
{
	/// <summary>
	///     This class tests the ConditionHelper class
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class StructuredQueryTests
    {

        /// <summary>
        ///     Important: this test rewrites the XML file by reading the queries, and then updating the SQL with
        ///     whatever the QueryBuilder generates.
        ///     DO NOT run it unless you are sure that ALL testcases passed prior to making whatever change you're making to the query builder.
        ///     DO carefully inspect the diff of the Query tests.xml to ensure that only expected changes get committed.
        /// </summary>
        [Test]
        [RunAsTenant("EDC")]
        //[Explicit] // warning... Expicit doesn't work with VS test integration
        public void RegenerateExpectedResults()
        {
            //string curDir = Assembly.GetExecutingAssembly( ).CodeBase;
            //// e.g. curDir = "file:///C:/Development/Untested/EDC.ReadiNow.Common.Test/bin/Debug/EDC.ReadiNow.Common.Test.DLL"
            //string solutionRoot = curDir.Substring( 8, curDir.IndexOf( "ReadiNow.QueryEngine.Test" ) - 8 ).Replace( "/", "\\" );
            //string path = Path.Combine( solutionRoot, "ReadiNow.QueryEngine.Test\\Builder\\Query tests.xml" );
            //// e.g: path= C:\Development\Untested\EDC.ReadiNow.Common.Test\Metadata\Query\Select single column.test

            //XmlDocument doc = new XmlDocument( );
            //doc.Load( path );

            //foreach ( XmlNode test in doc.DocumentElement.ChildNodes )
            //{
            //    if ( !( test is XmlElement ) )
            //        continue;
            //    string testName = ( ( XmlElement ) test ).GetAttribute( "name" );

            //    // Load test
            //    StructuredQuery query = null;
            //    query = StructuredQueryHelper.FromXml( test.FirstChild );
            //    query.TimeZoneName = TimeZoneHelper.SydneyTimeZoneName;

            //    int derivedTypesTempTableThreshold = testName == "DerivedTypesThreshold" ? 10 : -1;

            //    // Generate SQL
            //    string sql;
            //    try
            //    {
            //        QuerySettings settings = new QuerySettings( );
            //        settings.Hint = "test";
            //        settings.DebugMode = true;
            //        settings.DerivedTypesTempTableThreshold = derivedTypesTempTableThreshold;
            //        var res = QueryBuilder.GetSql( query, settings );
            //        sql = res.Sql;
            //    }
            //    catch ( Exception ex )
            //    {
            //        sql = ex.ToString( );
            //    }

            //    var cdata = ( XmlCDataSection ) test.SelectSingleNode( "Expect" ).FirstChild;
            //    cdata.Value = sql; //"\r\n" + sql + "\r\n      ";
            //}

            //doc.Save( path );
        }

		private void RunTest( string testName, Action<StructuredQuery> additionalProcessing = null, int derivedTypesTempTableThreshold = -1)
        {
            XmlDocument doc = GetTestCaseXml( );

            XmlNode test = doc.DocumentElement.SelectSingleNode( "Test[@name='" + testName + "']" );
            if ( test == null )
            {
                throw new Exception( "Test not found in Query tests.xml: " + testName );
            }

            // Load test
            StructuredQuery query;
            try
            {
                query = StructuredQueryHelper.FromXml( test.FirstChild );
            }
            catch ( Exception ex )
            {
                throw new Exception( "Failed to load query for " + testName, ex );
            }

            query.TimeZoneName = TimeZoneHelper.SydneyTimeZoneName;

            if ( additionalProcessing != null )
            {
                additionalProcessing( query );
            }

            // Generate SQL
            var settings = new QuerySettings { Hint = "test", DebugMode = true, DerivedTypesTempTableThreshold = derivedTypesTempTableThreshold };
            QueryBuild result = QueryBuilder.GetSql( query, settings );
            string sql = result.Sql;
            sql = Canonical( sql );

            // Get expected results
            XmlNode selectSingleNode = test.SelectSingleNode( "Expect/text()" );
            if ( selectSingleNode != null )
            {
                string expected = Canonical( selectSingleNode.Value );
                expected = expected.Replace( "{userResource}", new EntityRef( "core", "userResource" ).Id.ToString( CultureInfo.InvariantCulture ) );
                Assert.AreEqual( expected, sql, "Generated SQL did not match expected." );
            }

            StructuredQueryHelper.ToXml( query );
        }

        /// <summary>
        /// Load the XML of the test cases
        /// </summary>
        /// <returns></returns>
        private static XmlDocument GetTestCaseXml( )
        {
            Assembly assembly = Assembly.GetExecutingAssembly( );
            Stream testXmlStream = assembly.GetManifestResourceStream( "ReadiNow.QueryEngine.Test.Builder.Query tests.xml" );

            var doc = new XmlDocument( );
            if ( testXmlStream != null )
            {
                doc.Load( testXmlStream );
            }
            if ( doc.DocumentElement == null )
            {
                throw new Exception( "doc.DocumentElement was null " );
            }

            return doc;
        }

        /// <summary>
        ///     Format strings for comparison.
        /// </summary>
        private static string Canonical( string value )
		{
			return value.Replace( "\r\n", "\n" ).Trim( );
		}

        /// <summary>
        /// Standard test runner
        /// </summary>
        /// <param name="testName">Name of test to run</param>
        [Test]
        [TestCaseSource( "QueryTest_TestCases" )]
        [RunAsDefaultTenant]
        public void QueryTest(string testName)
        {
            RunTest( testName );
        }

        /// <summary>
        /// Generate list of tests to run
        /// </summary>
        public IEnumerable<TestCaseData> QueryTest_TestCases()
        {
            XmlDocument doc = GetTestCaseXml( );

            XmlNodeList nameAttrs = doc.DocumentElement.SelectNodes( "Test" );

            foreach (XmlElement node in nameAttrs)
            {
                string name = node.GetAttribute( "name" );
                string flags = node.GetAttribute( "flags" );

                if ( flags == "UsedElsewhere" )
                    continue;

                TestCaseData testCase = new TestCaseData( name );
                if ( flags == "Ignore" )
                    testCase = testCase.Ignore( );

                yield return testCase;
            }
        }
        
		[Test]
		[RunAsTenant( "EDC" )]
		public void EntityGetMatches( )
		{
			// Test to load all instances of a particular entity type that have a field matching a particular value.

			// Test data
			// (Load all nav sections that have name='Home')
			var type = new EntityRef( "console", "navSection" );
			var field = new EntityRef( "core", "name" );
			const string value = "Home";

			// Create query            
			var query = new StructuredQuery
				{
					RootEntity = new ResourceEntity( type )
				};
			query.Conditions.Add( new QueryCondition
				{
					Expression = new ResourceDataColumn( query.RootEntity, field ),
					Operator = ConditionType.Equal,
					Argument = new TypedValue( value )
				} );

			// Get results
			IEnumerable<NavSection> entities = Entity.GetMatches<NavSection>( query );
			NavSection[] eArr = entities.ToArray( );
			Assert.IsTrue( eArr.Count( ) == 1 );
		}

		[Test]
		[RunAsDefaultTenant]
		public void EntityGetMatchesWithRelationship( )
		{
			var query = new StructuredQuery
				{
					RootEntity = new ResourceEntity( Field.Field_Type )
				};

			// Root query type is 'EntityType'
            
			// Follow 'Fields' relationship
			var relatedResource = new RelatedResource
				{
					RelationshipDirection = RelationshipDirection.Forward,
					RelationshipTypeId = new EntityRef(Field.FieldIsOnType_Field.Id),
					ResourceMustExist = false
				};
			query.RootEntity.RelatedEntities.Add( relatedResource );

			query.Conditions.Add( new QueryCondition
				{
					Expression = new ResourceDataColumn( relatedResource, EntityType.Name_Field ),
					Operator = ConditionType.Equal,
					Argument = new TypedValue( "Person" )
				} );

			IEnumerable<Field> personFields = Entity.GetMatches<Field>( query );
			Assert.IsTrue( personFields.Any( ) );
		}

		[Test]
		[RunAsDefaultTenant]
		public void EntityGetMatchesWithRelationship2( )
		{
			var stringField = Entity.Get<EntityType>( new EntityRef( "core", "stringField" ) );
			var fieldIsOnTypeRel = Entity.Get<Relationship>( new EntityRef( "core", "fieldIsOnType" ) );

			var query = new StructuredQuery
				{
					RootEntity = new ResourceEntity( stringField )
				};

			// Root query type is 'EntityType'

			// Follow 'Fields' relationship
			var relatedResource = new RelatedResource
				{
					RelationshipDirection = RelationshipDirection.Forward,
					RelationshipTypeId = fieldIsOnTypeRel.Id,
					ResourceMustExist = false
				};
			query.RootEntity.RelatedEntities.Add( relatedResource );

			//// check the condition
			query.Conditions.Add( new QueryCondition
				{
					Expression = new ResourceDataColumn( relatedResource, EntityType.Name_Field ),
					Operator = ConditionType.Equal,
					Argument = new TypedValue( "Person" )
				} );

			IEnumerable<Field> descriptionStringFields = Entity.GetMatches<Field>( query );
			Assert.IsTrue( descriptionStringFields.Any( ), "There should be at least one person type" );
		}

        [Test]
        [RunAsDefaultTenant]
        [ExpectedException("System.InvalidOperationException")]
        public void RelatesBackToSelfUsingEntity()
        {
            RunTest("RelatesBackToSelfUsingEntity");
        }

        [Test]
        [RunAsDefaultTenant]
        public void SelectFromRelatedResourceWithFauxRelations( )
        {
            Action<StructuredQuery> callback = sq =>
                {
                    var rr = ( RelatedResource ) sq.RootEntity.RelatedEntities [ 0 ];
                    rr.FauxRelationships = new FauxRelationships
                    {
                        HasTargetResource = true,
                        HasIncludedResources = true,
                        HasExcludedResources = true
                    };
                };

            RunTest( "SelectFromRelatedResourceWithFauxRelations", callback );
        }

        [Test]
        [RunAsDefaultTenant]
        public void RunReportFromEntity()
        {
            var report = Entity.Get<Report>("templateReport");

            StructuredQuery sq = ReportToQueryConverter.Instance.Convert( report );
            QueryBuilder.GetSql(sq);
        }
	}
}