// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Core;
using ReadiNow.QueryEngine.CachingRunner;
using Model = EDC.ReadiNow.Model;

namespace ReadiNow.QueryEngine.Test.Caching
{
    enum Expect
    {
        Cache,
        Invalidate
    }

    [TestFixture]
    [RunAsDefaultTenant]
    public class CacheScenarioTests
    {
		[SetUp]
	    public void TestSetup( )
	    {
			CachingQueryRunner cachingQueryRunner;
			Mock<IQueryRunner> mockQueryRunner;
			IQueryRunner queryRunner;
			IQuerySqlBuilder queryBuilder = MockQuerySqlBuilder( );
			IUserRuleSetProvider userRuleSetProvider = MockUserRuleSetProvider( );

			mockQueryRunner = new Mock<IQueryRunner>( MockBehavior.Strict );
			queryRunner = mockQueryRunner.Object;

			cachingQueryRunner = new CachingQueryRunner( queryRunner, userRuleSetProvider, queryBuilder );

			cachingQueryRunner.Clear( );
	    }

		private static IQuerySqlBuilder MockQuerySqlBuilder( )
		{
			var mock = new Mock<IQuerySqlBuilder>( MockBehavior.Loose );
			mock.Setup( x => x.BuildSql( It.IsAny<StructuredQuery>( ), It.IsNotNull<QuerySqlBuilderSettings>( ) ) ).Returns( new QueryBuild( ) );
			return mock.Object;
		}

		private static IUserRuleSetProvider MockUserRuleSetProvider( )
		{
			var mock = new Mock<IUserRuleSetProvider>( MockBehavior.Loose );
			mock.Setup( x => x.GetUserRuleSet( It.IsAny<long>( ), It.IsAny<Model.EntityRef>( ) ) ).Returns( new UserRuleSet( new List<long>( ) ) );
			return mock.Object;
		}

	    [RunWithTransaction]
        [TestCase( "QueryRunner : Cached" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_NothingChanged( string cacheTest  )
        {
            StructuredQuery sq = new StructuredQuery { RootEntity = new ResourceEntity( "test:person" ) };
            sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );

            Test_Scenario( cacheTest, sq, ( ) => { } );
        }

        
        [RunWithTransaction]
        [TestCase( "QueryRunner : Invalidate" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_NewInstance( string cacheTest )
        {
            Model.EntityType type;
            StructuredQuery sq;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                type = Model.Entity.Create<Model.EntityType>();
                type.SetField("core:name", "TmpType1");
                type.Save();


                sq = new StructuredQuery { RootEntity = new ResourceEntity( type.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );
                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    var instance = Model.Entity.Create(type);
                    instance.SetField("core:name", "TmpInstance1");
                    instance.Save();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Invalidate" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_NewInstanceDerived( string cacheTest )
        {
            Model.EntityType parent, child;
            StructuredQuery sq;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                parent = Model.Entity.Create<Model.EntityType>();
                parent.Name = "TmpParentType1";
                parent.Save();
                child = Model.Entity.Create<Model.EntityType>();
                child.Name = "TmpChildType1";
                child.Inherits.Add(parent);
                child.Save();


                sq = new StructuredQuery { RootEntity = new ResourceEntity( parent.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );

                ctx.CommitTransaction();
            }
            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    var instance = Model.Entity.Create(child);
                    instance.SetField("core:name", "TmpInstance1");
                    instance.Save();

                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Cached" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_NewInstanceAncestor( string cacheTest )
        {
            StructuredQuery sq;
            Model.EntityType parent, child;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                parent = Model.Entity.Create<Model.EntityType>();
                parent.Name = "TmpParentType1";
                parent.Save();
                child = Model.Entity.Create<Model.EntityType>();
                child.Name = "TmpChildType1";
                child.Inherits.Add(parent);
                child.Save();


                sq = new StructuredQuery { RootEntity = new ResourceEntity( child.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );

                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    var instance = Model.Entity.Create(parent);
                    instance.SetField("core:name", "TmpInstance1");
                    instance.Save();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Invalidate" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_NewInstance_NoFields( string cacheTest )
        {
            StructuredQuery sq;
            Model.EntityType type;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                type = Model.Entity.Create<Model.EntityType>();
                type.SetField("core:name", "TmpType1");
                type.Save();


                sq = new StructuredQuery { RootEntity = new ResourceEntity( type.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );

                ctx.CommitTransaction();
            }
            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    var instance = Model.Entity.Create(type);
                    instance.Save();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase("QueryRunner : Invalidate")]
        [TestCase("QuerySqlBuilder : Cached")]
        public void Test_DeleteInstance(string cacheTest)
        {
            StructuredQuery sq;
            Model.EntityType type;
            Model.IEntity instance;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                type = Model.Entity.Create<Model.EntityType>();
                type.SetField("core:name", "TmpType1");
                type.Save();
                instance = Model.Entity.Create(type);
                instance.SetField("core:name", "TmpInstance1");
                instance.Save();
            
                sq = new StructuredQuery { RootEntity = new ResourceEntity( type.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );
                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    instance.Delete();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Invalidate" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_DeleteInstanceDerived( string cacheTest )
        {
            StructuredQuery sq;
            Model.EntityType parent, child;
            Model.IEntity instance;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                parent = Model.Entity.Create<Model.EntityType>();
                parent.Name = "TmpParentType1";
                parent.Save();
                child = Model.Entity.Create<Model.EntityType>();
                child.Name = "TmpChildType1";
                child.Inherits.Add(parent);
                child.Save();
                instance = Model.Entity.Create(child);
                instance.SetField("core:name", "TmpInstance1");
                instance.Save();


                sq = new StructuredQuery { RootEntity = new ResourceEntity( parent.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );

                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    instance.Delete();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Cached" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_DeleteInstanceAncestor( string cacheTest )
        {
            StructuredQuery sq;
            Model.EntityType parent, child;
            Model.IEntity instance;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                parent = Model.Entity.Create<Model.EntityType>();
                parent.Name = "TmpParentType1";
                parent.Save();
                child = Model.Entity.Create<Model.EntityType>();
                child.Name = "TmpChildType1";
                child.Inherits.Add(parent);
                child.Save();
                instance = Model.Entity.Create(parent);
                instance.SetField("core:name", "TmpInstance1");
                instance.Save();


                sq = new StructuredQuery { RootEntity = new ResourceEntity(child.Id) };
                sq.SelectColumns.Add(new SelectColumn { Expression = new ResourceExpression(sq.RootEntity, "core:name") });

                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    instance.Delete();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Invalidate" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_ModifyInstanceField( string cacheTest )
        {
            StructuredQuery sq;
            Model.IEntity instance;
            Model.EntityType type;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                type = Model.Entity.Create<Model.EntityType>();
                type.SetField("core:name", "TmpType1");
                type.Save();

                instance = Model.Entity.Create(type);
                type.SetField("core:name", "TmpInstance1");
                type.Save();
             

                sq = new StructuredQuery { RootEntity = new ResourceEntity( type.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );

                ctx.CommitTransaction();
            }
            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    instance.SetField("core:name", "TmpInstance1b");
                    instance.Save();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Cached (Ignore)" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_ModifyInstanceUnrelatedField( string cacheTest )
        {
            Model.EntityType type;
            StructuredQuery sq;
            Model.IEntity instance;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                type = Model.Entity.Create<Model.EntityType>();
                type.SetField("core:name", "TmpType1");
                type.Save();

                instance = Model.Entity.Create(type);
                type.SetField("core:name", "TmpInstance1");
                type.Save();

                sq = new StructuredQuery { RootEntity = new ResourceEntity(type.Id) };
                sq.SelectColumns.Add(new SelectColumn { Expression = new ResourceExpression(sq.RootEntity, "core:name") });

                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    instance.SetField("core:description", "Description");
                    instance.Save();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Invalidate (Ignore)" )]   // it probably works, but is hard to mock correctly, because the manual mock invalidation registration is insufficient. But it doesn't really matter anyway.
        [TestCase( "QuerySqlBuilder : Invalidate" )]
        public void Test_RootTypeSaved( string cacheTest )
        {
            Model.EntityType type;
            StructuredQuery sq;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                type = Model.Entity.Create<Model.EntityType>();
                type.SetField("core:name", "TmpType1");
                type.Save();

                sq = new StructuredQuery { RootEntity = new ResourceEntity(type.Id) };
                sq.SelectColumns.Add(new SelectColumn { Expression = new ResourceExpression(sq.RootEntity, "core:name") });

                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    type.SetField("core:description", "TmpType1b");
                    type.Save();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Cached" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_NewInstanceOfDifferentType( string cacheTest )
        {
            Model.EntityType type2;
            StructuredQuery sq;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                var type1 = Model.Entity.Create<Model.EntityType>();
                type1.Name = "TmpType1";
                type1.Save();
                type2 = Model.Entity.Create<Model.EntityType>();
                type2.Name = "TmpType2";
                type2.Save();


                sq = new StructuredQuery { RootEntity = new ResourceEntity( type1.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceExpression( sq.RootEntity, "core:name" ) } );

                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    var instance = Model.Entity.Create(type2);
                    instance.SetField("core:name", "TmpInstance2");
                    instance.Save();
                    ctx.CommitTransaction();
                }
            } );
        }

        [RunWithTransaction]
        [TestCase( "QueryRunner : Cached" )]
        [TestCase( "QuerySqlBuilder : Cached" )]
        public void Test_NewInstanceOfDifferentType_NoFields( string cacheTest )
        {
            Model.EntityType type2;
            StructuredQuery sq;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                var type1 = Model.Entity.Create<Model.EntityType>();
                type1.Name = "TmpType1";
                type1.Save();
                type2 = Model.Entity.Create<Model.EntityType>();
                type2.Name = "TmpType2";
                type2.Save();

                sq = new StructuredQuery { RootEntity = new ResourceEntity(type1.Id) };
                sq.SelectColumns.Add(new SelectColumn { Expression = new ResourceExpression(sq.RootEntity, "core:name") });

                ctx.CommitTransaction();
            }

            Test_Scenario( cacheTest, sq, ( ) =>
            {
                using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
                {
                    var instance = Model.Entity.Create(type2);
                    instance.Save();
                    ctx.CommitTransaction();
                }
            } );
        }

        [Test]
        public void Test_CalculatedFieldFollowsRelation_Bug27901( )
        {
            // Note : this test does not follow the mocked scenario because it needs to test the particular nature of how the cache invalidations
            // get registered - and at present the mock only simulates the actual code.

            Model.EntityType type1 = null;
            Model.EntityType type2 = null;
            Model.EntityType type3 = null;
            try
            {
                StructuredQuery sq;

                type1 = Model.Entity.Create<Model.EntityType>( );
                type1.Name = "TmpType1";
                type1.Save( );
                type2 = Model.Entity.Create<Model.EntityType>( );
                type2.Name = "TmpType2";
                type2.Save( );
                type3 = Model.Entity.Create<Model.EntityType>( );
                type3.Name = "TmpType3";
                type3.Save( );
                var rel12 = Model.Entity.Create<Model.Relationship>( );
                rel12.FromType = type1;
                rel12.ToType = type2;
                rel12.ToName = "Forward";
                rel12.Cardinality_Enum = EDC.ReadiNow.Model.CardinalityEnum_Enumeration.ManyToMany;
                rel12.Save( );
                var rel23 = Model.Entity.Create<Model.Relationship>( );
                rel23.FromType = type2;
                rel23.ToType = type3;
                rel23.ToName = "Forward2";
                rel23.Cardinality_Enum = EDC.ReadiNow.Model.CardinalityEnum_Enumeration.ManyToMany;
                rel23.Save( );
                var field1 = Model.Entity.Create<Model.StringField>( );
                field1.Name = "FieldName";
                field1.FieldScriptName = "FieldName";
                field1.FieldCalculation = "Forward.Forward2.Name";
                field1.IsCalculatedField = true;
                field1.FieldIsOnType = type1;
                field1.Save( );

                var instance1 = Model.Entity.Create( type1 );
                instance1.Save( );

                var instance2 = Model.Entity.Create( type2 );
                instance2.SetField( new Model.EntityRef( "core:name" ), "2" );
                var relInst12 = instance2.GetRelationships( rel12.Id, Model.Direction.Reverse );
                relInst12.Add( instance1 );
                instance2.Save( );

                sq = new StructuredQuery { RootEntity = new ResourceEntity( type1.Id ) };
                sq.SelectColumns.Add( new SelectColumn { Expression = new ResourceDataColumn( sq.RootEntity, new Model.EntityRef( field1.Id ) ) } );

                QuerySettings settings = new QuerySettings( );
                QueryResult result = Factory.QueryRunner.ExecuteQuery( sq, settings );

                Assert.That( result.DataTable.Rows [ 0 ].IsNull( 0 ), Is.True );

                var instance3 = Model.Entity.Create( type3 );
                instance3.SetField( new Model.EntityRef( "core:name" ), "Test" );
                var relInst23 = instance3.GetRelationships( rel23.Id, Model.Direction.Reverse );
                relInst23.Add( instance2 );
                instance3.Save( );

                result = Factory.QueryRunner.ExecuteQuery( sq, settings );

                Assert.That( result.DataTable.Rows [ 0 ] [ 0 ], Is.EqualTo( "Test" ) );
            }
            finally
            {
                type1?.Delete( );
                type2?.Delete( );
                type3?.Delete( );
            }
            
        }

        #region Helper
        /// <summary>
        /// Test scenario
        /// </summary>
        private static void Test_Scenario( string cacheTest, StructuredQuery structuredQuery, Action invalidationCallback )
        {
            if ( cacheTest.Contains( "Ignore" ) )
                Assert.Ignore( );

            string [ ] parts = cacheTest.Split( ':' ).Select( p => p.Trim( ) ).ToArray( );

            bool cached = parts [ 1 ].Contains( "Cached" );
            bool invalidate = parts [ 1 ].Contains( "Invalidate" );

            if ( !cached && !invalidate )
                Assert.Fail( "Unknown expected result" );

            switch ( parts [ 0 ] )
            {
                case "QueryRunner":
                    CachingQueryRunnerTests.Test_Scenario( structuredQuery, invalidationCallback, invalidate );
                    break;
                case "QuerySqlBuilder":
                    CachingQuerySqlBuilderTests.Test_Scenario( structuredQuery, invalidationCallback, invalidate );
                    invalidate = true;
                    break;
                default:
                    throw new ArgumentException( "Unknown cache" );
            }
        }
        #endregion
    }
}
