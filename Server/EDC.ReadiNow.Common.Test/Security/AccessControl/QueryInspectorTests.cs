// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    [FailOnEvent]
    public class QueryInspectorTests
    {
        [Test]
        [TestCase( true, "" )]
        [TestCase( false, "nodes" )]
        [TestCase( false, "columns" )]
        [TestCase( false, "conditions" )]
        public void Test_IsQueryUndamaged( bool expected, string errorType )
        {
            StructuredQuery sq = new StructuredQuery( );
            sq.InvalidReportInformation = new Dictionary<string,Dictionary<long,string>>();

            var errors = new Dictionary<long,string>();
            errors[1] = "Some error";

            if ( !string.IsNullOrEmpty( errorType ) )
            {
                sq.InvalidReportInformation.Add( errorType, errors );
            }

            bool actual = QueryInspector.IsQueryUndamaged( sq );

            Assert.AreEqual( expected, actual );
        }


        [Test]
        [TestCase( true, "test:person", "test:person", "" )]
        [TestCase( true, "test:person", "test:employee", "" )]
        [TestCase( true, "test:person", "test:person", "addUnusedCondition" )]
        [TestCase( true, "test:person", "test:person", "addSafeRel" )]
        [TestCase( true, "test:person", "test:person", "addAgg" )]
        [TestCase( false, "test:employee", "test:person", "" )]
        [TestCase( false, "somethingInvalid", "test:person", "addUnsafeRel" )]
        [TestCase( false, "somethingInvalid", "test:person", "addUnsafeCondition" )]
        [TestCase( false, "somethingInvalid", "test:person", "exactType" )]
        [TestCase( false, "somethingInvalid", "test:person", "summarize" )]
        public void Test_DoesAccessRuleQueryGrantAllOfType( bool expected, string reportType, string resourceType, string flags )
        {
            // Note: if alias is 'somethingInvalid', then we're expecting the test to short-circuit before even attempting to check type equality or inheritance
            // I.e. the system should never have the opportunity to evaluate the invalid alias, so the test should pass.

            StructuredQuery sq = new StructuredQuery( );
            sq.RootEntity = new ResourceEntity() {
                ExactType = flags.Contains( "exactType" ),
                EntityTypeId = new EntityRef(reportType)
            };

            if ( flags.Contains( "addSafeRel" ) )
            {
                sq.RootEntity.RelatedEntities = new List<EDC.ReadiNow.Metadata.Query.Structured.Entity>( );
                sq.RootEntity.RelatedEntities.Add( new RelatedResource( ) );
            }
            if ( flags.Contains( "addUnsafeRel" ) )
            {
                sq.RootEntity.RelatedEntities = new List<EDC.ReadiNow.Metadata.Query.Structured.Entity>();
                sq.RootEntity.RelatedEntities.Add( new RelatedResource { ResourceMustExist = true } );
            }
            if ( flags.Contains( "addAgg" ) )
            {
                sq.RootEntity.RelatedEntities = new List<EDC.ReadiNow.Metadata.Query.Structured.Entity>( );
                sq.RootEntity.RelatedEntities.Add( new AggregateEntity( ) );
            }
            if ( flags.Contains( "addUnsafeCondition" ) )
            {
                sq.Conditions = new List<QueryCondition>( );
                sq.Conditions.Add( new QueryCondition { Operator = ConditionType.Equal } );
            }
            if ( flags.Contains( "addUnusedCondition" ) )
            {
                sq.Conditions = new List<QueryCondition>( );
                sq.Conditions.Add( new QueryCondition { Operator = ConditionType.Unspecified } );
            }
            if ( flags.Contains( "summarize" ) )
            {
                sq.RootEntity = new AggregateEntity( );
            }


            bool result = QueryInspector.DoesAccessRuleQueryGrantAllOfType( sq, (new EntityRef(resourceType)).Id );

            Assert.That( result, Is.EqualTo( expected ) );
        }


        [Test]
        [TestCase( true, "test:person", "test:person", "" )]
        [TestCase( true, "test:person", "test:employee", "" )]
        [TestCase( true, "test:person", "test:person,test:herb", "" )]
        [TestCase( true, "test:person", "test:employee,test:herb", "" )]
        [TestCase( false, "test:employee", "test:person", "" )]
        [TestCase( false, "test:person", "test:truck,test:herb", "" )]
        [TestCase( false, "test:employee", "test:person,test:herb", "" )]
        public void DoesAccessRuleQueryGrantAllOfTypes( bool expected, string reportType, string resourceTypes, string flags )
        {
            // Note: if alias is 'somethingInvalid', then we're expecting the test to short-circuit before even attempting to check type equality or inheritance
            // I.e. the system should never have the opportunity to evaluate the invalid alias, so the test should pass.

            StructuredQuery sq = new StructuredQuery( );
            sq.RootEntity = new ResourceEntity( )
            {
                EntityTypeId = new EntityRef( reportType )
            };

            var types = resourceTypes.Split( ',' ).Select( alias => ( new EntityRef( alias ) ).Id ).ToList( );
            bool result = QueryInspector.DoesAccessRuleQueryGrantAllOfTypes( sq, types );

            Assert.That( result, Is.EqualTo( expected ) );
        }
    }
}