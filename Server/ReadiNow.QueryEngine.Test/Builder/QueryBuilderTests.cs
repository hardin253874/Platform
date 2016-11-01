// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using NUnit.Framework;

namespace ReadiNow.QueryEngine.Test.Builder
{
    /// <summary>
    /// 
    /// </summary>
    class QueryBuilderTests
    {

        private QueryBuild BuildSql( StructuredQuery query, QuerySqlBuilderSettings settings )
        {
            return Factory.NonCachedQuerySqlBuilder.BuildSql( query, settings );
        }



        [Test]
        [RunAsDefaultTenant]
        [TestCase( false, false, false, "core:resource" )] // root type
        [TestCase( false, false, false, "test:person" )] // has derived types
        [TestCase( false, false, false, "test:drink" )] // does not have derived types
        [TestCase( false, false, true, "core:resource" )]
        [TestCase( false, false, true, "test:person" )]
        [TestCase( false, false, true, "test:drink" )]
        [TestCase( false, true, false, "core:resource" )]
        [TestCase( false, true, false, "test:person" )]
        [TestCase( false, true, false, "test:drink" )]
        [TestCase( false, true, true, "core:resource" )]
        [TestCase( false, true, true, "test:person" )]
        [TestCase( false, true, true, "test:drink" )]
        [TestCase( true, false, false, "core:resource" )]
        [TestCase( true, false, false, "test:person" )]
        [TestCase( true, false, false, "test:drink" )]
        [TestCase( true, false, true, "core:resource" )]
        [TestCase( true, false, true, "test:person" )]
        [TestCase( true, false, true, "test:drink" )]
        [TestCase( true, true, false, "core:resource" )]
        [TestCase( true, true, false, "test:person" )]
        [TestCase( true, true, false, "test:drink" )]
        [TestCase( true, true, true, "core:resource" )]
        [TestCase( true, true, true, "test:person" )]
        [TestCase( true, true, true, "test:drink" )]
        public void Test_RootTable( bool supportRootIdFilter, bool suppressRootTypeCheck, bool exactType, string typeAlias )
        {
            StructuredQuery structuredQuery;
            QuerySettings querySettings;
            QueryBuild queryResult;
            const long testEntityId = 42;

            structuredQuery = TestQueries.Entities( new EntityRef( typeAlias ) );
            querySettings = new QuerySettings( )
            {
                SecureQuery = false,
                SupportRootIdFilter = supportRootIdFilter,
                SuppressRootTypeCheck = suppressRootTypeCheck,
                RootIdFilterList = testEntityId.ToEnumerable( )
            };
            ( (ResourceEntity) structuredQuery.RootEntity ).ExactType = exactType;

            queryResult = BuildSql( structuredQuery, querySettings );

            if ( supportRootIdFilter )
            {
                Assert.That( queryResult.Sql,
                    Contains.Substring( "@entitylist" ) );

                // If the @entitylist attribute is present, then we do not need to join against the entity table, so it should not be present
                Assert.That( queryResult.Sql,
                    Is.Not.ContainsSubstring( "Entity" ) );
            }
            else
            {
                Assert.That( queryResult.Sql,
                    Is.Not.ContainsSubstring( "@entitylist" ) );
            }

            // Expect a type check, unless it is suppressed, or unless every type of resource is permitted
            bool expectTypeCheck = !suppressRootTypeCheck && ( typeAlias != "core:resource" || exactType );

            if ( expectTypeCheck )
            {
                Assert.That( queryResult.Sql,
                    Contains.Substring( "/*isOfType*/" ) );

                // If there's any form of type check, then there's never a need to use the entity table, because we can use the relationship table, if nothing else
                Assert.That( queryResult.Sql,
                    Is.Not.ContainsSubstring( "Entity" ) );

                bool expectExactType = exactType || typeAlias == "test:drink"; // drink has no derived types
                if ( expectExactType )
                {
                    Assert.That( queryResult.Sql,
                        Contains.Substring( ".ToId = @param2" ) );
                    Assert.That( queryResult.Sql,
                        Is.Not.ContainsSubstring( ".ToId in (@param2, @param3" ) );
                }
                else
                {
                    Assert.That( queryResult.Sql,
                        Contains.Substring( ".ToId in (@param2, @param3" ) );
                    Assert.That( queryResult.Sql,
                        Is.Not.ContainsSubstring( ".ToId = @param2" ) );
                }
            }
            else
            {
                Assert.That( queryResult.Sql,
                    Is.Not.ContainsSubstring( "/*isOfType*/" ) );
            }

            // Look for a join to the entity list if its type is to be checked 
            if ( expectTypeCheck && supportRootIdFilter )
            {
                Assert.That( queryResult.Sql,
                    Contains.Substring( "r.FromId = el.Id" ) );
            }
            else
            {
                Assert.That( queryResult.Sql,
                    Is.Not.ContainsSubstring( "r.FromId = el.Id" ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( true )]
        [TestCase( false )]
        public void Test_QuickSearchFilter( bool includeFilter )
        {
            StructuredQuery structuredQuery;
            QuerySettings querySettings;
            QueryBuild queryResult;

            structuredQuery = TestQueries.Entities( );
            structuredQuery.SelectColumns.Add( new SelectColumn
            {
                Expression = new ResourceDataColumn( structuredQuery.RootEntity, new EntityRef( "core:name" ) )
            } );
            querySettings = new QuerySettings( )
            {
                SecureQuery = false,
                SupportQuickSearch = includeFilter
            };

            queryResult = BuildSql( structuredQuery, querySettings );
            if ( includeFilter )
            {
                Assert.That( queryResult.Sql,
                    Contains.Substring( "@quicksearch" ) );
            }
            else
            {
                Assert.That( queryResult.Sql,
                    Is.Not.ContainsSubstring( "@quicksearch" ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase( true )]
        [TestCase( false )]
        public void Test_QuickSearchFilterCaseInsensitive( bool includeFilter )
        {
            StructuredQuery structuredQuery;
            QuerySettings querySettings;
            QueryBuild queryResult;

            structuredQuery = TestQueries.Entities( );
            structuredQuery.SelectColumns.Add( new SelectColumn
            {
                Expression = new ResourceDataColumn( structuredQuery.RootEntity, new EntityRef( "core:name" ) )
            } );
            querySettings = new QuerySettings( )
            {
                SecureQuery = false,
                SupportQuickSearch = includeFilter
            };

            queryResult = BuildSql( structuredQuery, querySettings );
            if ( includeFilter )
            {
                Assert.That( queryResult.Sql,
                    Contains.Substring( "COLLATE Latin1_General_CI_AI" ) );
            }
            else
            {
                Assert.That( queryResult.Sql,
                    Is.Not.ContainsSubstring( "COLLATE Latin1_General_CI_AI" ) );
            }
        }
    }
}
