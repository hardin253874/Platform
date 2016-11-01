// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using EDC.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Builder;
using EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.Builder
{
    /// <summary>
    ///     Summary description for CalculationExpressionTest
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    public class CalculationExpressionTest
    {
        [SetUp]
        public void TestInitialize( )
        {
            using ( new TenantAdministratorContext( "EDC" ) )
            {
                // Create a type to test with
                _testType = new EntityType( );
                _testType.Inherits.Add( Entity.Get<EntityType>( "core:resource" ) );
                _aColumn = ( new DecimalField( ) ).As<Field>( );
                _bColumn = ( new DecimalField( ) ).As<Field>( );
                _cColumn = ( new DecimalField( ) ).As<Field>( );
                _dColumn = ( new DecimalField( ) ).As<Field>( );
                _eColumn = ( new DateTimeField( ) ).As<Field>( );
                _fColumn = ( new DateTimeField( ) ).As<Field>( );
                _gColumn = ( new BoolField( ) ).As<Field>( );
                _hColumn = ( new StringField( ) ).As<Field>( );
                _iColumn = ( new StringField( ) ).As<Field>( );
                _xColumn = ( new StringField( ) ).As<Field>( );
                var fields = new [ ]
					{
						_aColumn, _bColumn, _cColumn, _dColumn, _eColumn, _fColumn, _gColumn, _hColumn, _iColumn, _xColumn
					};
                foreach ( Field field in fields )
                {
                    field.Save( );
                    _testType.Fields.Add( field );
                }
                _testType.Save( );
            }
        }


        /// <summary>
        ///     Performs any special cleanup after running each test.
        /// </summary>
        [TearDown]
        public void TestFinalize( )
        {
            using ( new TenantAdministratorContext( "EDC" ) )
            {
                _testType.Delete( );
            }
        }

        private DataTable RunQuery( StructuredQuery query )
        {
            QuerySettings settings = new QuerySettings( );
            return Factory.QueryRunner.ExecuteQuery( query, settings ).DataTable;
        }

        private EntityType _testType;

        private const decimal A = 10;
        private const decimal B = 30;
        private const decimal C = 5;
        private const decimal D = 100;
        private const string H = "Hello,";
        private const string I = "World!";
        private const bool G = true;
        private Field _aColumn; // decimal test field
        private Field _bColumn; // decimal test field
        private Field _cColumn; // decimal test field
        private Field _dColumn; // decimal test field
        private Field _eColumn; // datetime test field
        private Field _fColumn; // datetime test field
        private Field _gColumn; // bool test field
        private Field _hColumn; // string test field
        private Field _iColumn; // string test field
        private Field _xColumn; // string test field

        private readonly Guid _nodeId = new Guid( "8a2fdb07-a2ef-4488-af9d-362a2bf3e272" );

        [Test]
        [RunAsDefaultTenant]
        public void PureCodeTest( )
        {
            #region Add Test Data

            IEntity testData = Entity.Create( _testType.Id );
            testData.SetField( _aColumn, A );
            testData.SetField( _bColumn, B );
            testData.SetField( _cColumn, C );
            testData.SetField( _dColumn, D );
            testData.SetField( _gColumn, G );
            testData.SetField( _hColumn, H );
            testData.SetField( _iColumn, I );
            testData.Save( );

            #endregion

            var query = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = _testType.Id,
                    NodeId = _nodeId
                }
            };

            #region CalculationTest

            //A+B
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A+B",
                    DisplayName = "A+B",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Add,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _bColumn,
												}
										}
                        }
                } );
            var res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );

            //Get Data
            DataTable dataTable = RunQuery( query );
            decimal result;
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A+B" ].ToString( ), out result );

            Assert.AreEqual( A + B, result );
            //A-B
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A-B",
                    DisplayName = "A-B",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Subtract,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _bColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A-B" ].ToString( ), out result );

            Assert.AreEqual( A - B, result );

            //(A+B)-C
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "(A+B)-C",
                    DisplayName = "(A+B)-C",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Subtract,
                            Expressions = new List<ScalarExpression>
										{
											new CalculationExpression
												{
													Operator = CalculationOperator.Add,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _aColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																}
														}
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _cColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "(A+B)-C" ].ToString( ), out result );

            Assert.AreEqual( ( A + B ) - C, result );

            //A+(B-C)
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A+(B-C)",
                    DisplayName = "A+(B-C)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Add,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new CalculationExpression
												{
													Operator = CalculationOperator.Subtract,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _cColumn,
																}
														}
												}
										}
                        }
                } );
            QueryBuilder.GetSql( query );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A+(B-C)" ].ToString( ), out result );

            Assert.AreEqual( A + ( B - C ), result );

            //(A-B)-C
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "(A-B)-C",
                    DisplayName = "(A-B)-C",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Subtract,
                            Expressions = new List<ScalarExpression>
										{
											new CalculationExpression
												{
													Operator = CalculationOperator.Subtract,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _aColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																}
														}
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _cColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "(A-B)-C" ].ToString( ), out result );

            Assert.AreEqual( ( A - B ) - C, result );
            //A-(B-C)
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A-(B-C)",
                    DisplayName = "A-(B-C)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Subtract,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new CalculationExpression
												{
													Operator = CalculationOperator.Subtract,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _cColumn,
																}
														}
												},
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A-(B-C)" ].ToString( ), out result );

            Assert.AreEqual( A - ( B - C ), result );
            //A*B
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A*B",
                    DisplayName = "A*B",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Multiply,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _bColumn,
												}
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A*B" ].ToString( ), out result );

            Assert.AreEqual( A * B, result );
            //A*0
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A*0",
                    DisplayName = "A*0",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Multiply,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 0
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A*0" ].ToString( ), out result );

            Assert.AreEqual( A * 0, result );
            //A+B*C
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A+B*C",
                    DisplayName = "A+B*C",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Add,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new CalculationExpression
												{
													Operator = CalculationOperator.Multiply,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _cColumn,
																}
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A+B*C" ].ToString( ), out result );

            Assert.AreEqual( A + B * C, result );
            //(A+B)*C
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "(A+B)*C",
                    DisplayName = "(A+B)*C",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Multiply,
                            Expressions = new List<ScalarExpression>
										{
											new CalculationExpression
												{
													Operator = CalculationOperator.Add,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _aColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																}
														}
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _cColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "(A+B)*C" ].ToString( ), out result );

            Assert.AreEqual( ( A + B ) * C, result );

            //A-(B*C)
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A-(B*C)",
                    DisplayName = "A-(B*C)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Subtract,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new CalculationExpression
												{
													Operator = CalculationOperator.Multiply,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _cColumn,
																}
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A-(B*C)" ].ToString( ), out result );

            Assert.AreEqual( A - ( B * C ), result );
            //A+B*C+D
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A+B*C+D",
                    DisplayName = "A+B*C+D",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Add,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new CalculationExpression
												{
													Operator = CalculationOperator.Multiply,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _cColumn,
																}
														}
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _dColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A+B*C+D" ].ToString( ), out result );

            Assert.AreEqual( A + B * C + D, result );
            //A/B
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A/B",
                    DisplayName = "A/B",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Divide,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _bColumn,
												}
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A/B" ].ToString( ), out result );

            Assert.AreEqual( Math.Round( A / B, 4 ), Math.Round( result, 4 ) );

            //A+B/C
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A+B/C",
                    DisplayName = "A+B/C",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Add,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new CalculationExpression
												{
													Operator = CalculationOperator.Divide,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _cColumn,
																}
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A+B/C" ].ToString( ), out result );

            Assert.AreEqual( A + Math.Round( B / C, 4 ), Math.Round( result, 4 ) );
            //A/B*C*D
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A/B*C*D",
                    DisplayName = "A/B*C*D",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Multiply,
                            Expressions = new List<ScalarExpression>
										{
											new CalculationExpression
												{
													Operator = CalculationOperator.Divide,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _aColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																}
														}
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _cColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _dColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A/B*C*D" ].ToString( ), out result );

            Assert.AreEqual( Math.Round( A / B * C * D, 2 ), Math.Round( result, 2 ) );
            //A/B*C/D
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A/B*C/D",
                    DisplayName = "A/B*C/D",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Multiply,
                            Expressions = new List<ScalarExpression>
										{
											new CalculationExpression
												{
													Operator = CalculationOperator.Divide,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _aColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _bColumn,
																}
														}
												},
											new CalculationExpression
												{
													Operator = CalculationOperator.Divide,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _cColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _dColumn,
																}
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A/B*C/D" ].ToString( ), out result );

            Assert.AreEqual( Math.Round( A / B * C / D, 2 ), Math.Round( result, 2 ) );
            //A/B/C/D
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A/B/C/D",
                    DisplayName = "A/B/C/D",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Divide,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _bColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _cColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _dColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A/B/C/D" ].ToString( ), out result );

            Assert.AreEqual( Math.Round( A / B / C / D, 4 ), Math.Round( result, 4 ) );
            //A_modulo_B
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A_modulo_B",
                    DisplayName = "A_modulo_B",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Modulo,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 0
														}
												}
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            // ReSharper disable RedundantAssignment
            dataTable = RunQuery( query );
            // ReSharper restore RedundantAssignment
            //decimal.TryParse(dataTable.Rows[0]["A_modulo_B"].ToString(), out  result);

            //Assert.AreEqual(Math.Round((A % B),0), Math.Round( result,0));
            //A_modulo_1
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "A_modulo_1",
                    DisplayName = "A_modulo_1",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Modulo,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _aColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 1
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "A_modulo_1" ].ToString( ), out result );

            Assert.AreEqual( Math.Round( ( A % 1 ), 0 ), Math.Round( result, 0 ) );
            //Square_0
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Square_0",
                    DisplayName = "Square_0",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Square,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 0
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Square_0" ].ToString( ), out result );

            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Pow( 0, 2 ) == ( double ) result );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            //Square_3
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Square_3",
                    DisplayName = "Square_3",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Square,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 3
														}
												}
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Square_3" ].ToString( ), out result );

            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Pow( 3, 2 ) == ( double ) result );
            // ReSharper restore CompareOfFloatsByEqualityOperator
            //Power0_3
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Power0_3",
                    DisplayName = "Power0_3",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Power,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 0
														}
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 3
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Power0_3" ].ToString( ), out result );

            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Pow( 0, 3 ) == ( double ) result );
            // ReSharper restore CompareOfFloatsByEqualityOperator
            //Power3_4
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Power3_4",
                    DisplayName = "Power3_4",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Power,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 3
														}
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 4
														}
												}
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Power3_4" ].ToString( ), out result );

            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Pow( 3, 4 ) == ( double ) result );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            //SQRT1
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "SQRT1",
                    DisplayName = "SQRT1",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Sqrt,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 1
														}
												}
										}
                        }
                } );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "SQRT-1",
                    DisplayName = "SQRT-1",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Sqrt,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = -1
														}
												}
										}
                        }
                } );


            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "SQRT1" ].ToString( ), out result );

            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Sqrt( 1 ) == ( double ) result );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            Assert.IsTrue( dataTable.Rows [ 0 ] [ "SQRT-1" ].ToString( ) == string.Empty );
            //SQRT16
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "SQRT16",
                    DisplayName = "SQRT16",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Sqrt,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.DecimalType,
															Value = 16
														}
												}
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "SQRT16" ].ToString( ), out result );

            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Sqrt( 16 ) == ( double ) result );
            // ReSharper restore CompareOfFloatsByEqualityOperator
            //SQRT7
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "SQRT7",
                    DisplayName = "SQRT7",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Sqrt,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 7
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "SQRT7" ].ToString( ), out result );

            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Sqrt( 7 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            // ReSharper restore CompareOfFloatsByEqualityOperator
            //FLoor
            query.SelectColumns.Clear( );

            query.SelectColumns =
                new List<SelectColumn>
					{
						new SelectColumn
							{
								ColumnName = "FLoor1",
								DisplayName = "FLoor1",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Floor,
											Expressions = new List<ScalarExpression>
												{
													new CalculationExpression
														{
															Operator = CalculationOperator.Divide,
															Expressions = new List<ScalarExpression>
																{
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 12
																				}
																		},
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 5
																				}
																		}
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "FLoor2",
								DisplayName = "FLoor2",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Floor,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 2.7
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "FLoor3",
								DisplayName = "FLoor3",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Floor,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = -2.7
																}
														}
												}
										}
							}
						,
						new SelectColumn
							{
								ColumnName = "FLoor4",
								DisplayName = "FLoor4",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Floor,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = -2
																}
														}
												}
										}
							}
					};
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "FLoor1" ].ToString( ), out result );

            Assert.IsTrue( ( Math.Floor( ( decimal ) ( 12 / 5 ) ) == result ) );

            decimal.TryParse( dataTable.Rows [ 0 ] [ "FLoor2" ].ToString( ), out result );
            Assert.IsTrue( ( Math.Floor( ( decimal ) ( 2.7 ) ) == result ) );

            decimal.TryParse( dataTable.Rows [ 0 ] [ "FLoor3" ].ToString( ), out result );
            Assert.IsTrue( ( Math.Floor( ( decimal ) ( -2.7 ) ) == result ) );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "FLoor4" ].ToString( ), out result );
            Assert.IsTrue( ( Math.Floor( ( decimal ) ( -2 ) ) == result ) );
            //Ceiling
            query.SelectColumns.Clear( );

            query.SelectColumns =
                new List<SelectColumn>
					{
						new SelectColumn
							{
								ColumnName = "Ceiling1",
								DisplayName = "Ceiling1",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Ceiling,
											Expressions = new List<ScalarExpression>
												{
													new CalculationExpression
														{
															Operator = CalculationOperator.Divide,
															Expressions = new List<ScalarExpression>
																{
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 12
																				}
																		},
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 5
																				}
																		}
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "Ceiling2",
								DisplayName = "Ceiling2",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Ceiling,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 2.7
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "Ceiling3",
								DisplayName = "Ceiling3",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Ceiling,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = -2.7
																}
														}
												}
										}
							}
						,
						new SelectColumn
							{
								ColumnName = "Ceiling4",
								DisplayName = "Ceiling4",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Ceiling,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = -2
																}
														}
												}
										}
							}
					};
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Ceiling1" ].ToString( ), out result );

            Assert.IsTrue( ( Math.Ceiling( ( decimal ) ( 12 / 5 ) ) == result ) );

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Ceiling2" ].ToString( ), out result );
            Assert.IsTrue( ( Math.Ceiling( ( decimal ) ( 2.7 ) ) == result ) );

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Ceiling3" ].ToString( ), out result );
            Assert.IsTrue( ( Math.Ceiling( ( decimal ) ( -2.7 ) ) == result ) );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Ceiling4" ].ToString( ), out result );
            Assert.IsTrue( ( Math.Ceiling( ( decimal ) ( -2 ) ) == result ) );

            //Log,Log10
            query.SelectColumns.Clear( );

            query.SelectColumns =
                new List<SelectColumn>
					{
						new SelectColumn
							{
								ColumnName = "Log10(100)",
								DisplayName = "Log10(100)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log10,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 100
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "Log10(0.01)",
								DisplayName = "Log10(0.01)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log10,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 0.01
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "Log10(1)",
								DisplayName = "Log10(1)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log10,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 1
																}
														}
												}
										}
							}
						,
						new SelectColumn
							{
								ColumnName = "Log(8)",
								DisplayName = "Log(8)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.Int32Type,
																	Value = 8
																}
														}
												}
										}
							}
						,
						new SelectColumn
							{
								ColumnName = "Log(9)",
								DisplayName = "Log(9)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.Int32Type,
																	Value = 9
																}
														}
												}
										}
							}
						,
						new SelectColumn
							{
								ColumnName = "Log(5)",
								DisplayName = "Log(5)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.Int32Type,
																	Value = 5
																}
														}
												}
										}
							}
						,
						new SelectColumn
							{
								ColumnName = "Log(1/4)",
								DisplayName = "Log(1/4)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log,
											Expressions = new List<ScalarExpression>
												{
													new CalculationExpression
														{
															Operator = CalculationOperator.Divide,
															Expressions = new List<ScalarExpression>
																{
																	new ResourceDataColumn
																		{
																			NodeId = _nodeId,
																			FieldId = _aColumn,
																		},
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 4
																				}
																		}
																}
														}
												}
										}
							}
						,
						new SelectColumn
							{
								ColumnName = "Log(1.414)",
								DisplayName = "Log(1.414)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 1.414
																}
														}
												}
										}
							}
					};
            //Get Data

            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );

            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Log10(100)" ].ToString( ), out result );

            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Log10( 100 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Log10(0.01)" ].ToString( ), out result );
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Log10( 0.01 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Log10(1)" ].ToString( ), out result );
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Log10( 1 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Log(8)" ].ToString( ), out result );
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Log( 8 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Log(9)" ].ToString( ), out result );
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Log( 9 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Log(5)" ].ToString( ), out result );
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Log( 5 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Log(1/4)" ].ToString( ), out result );
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Log( ( double ) A / 4 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Log(1.414)" ].ToString( ), out result );
            // ReSharper disable CompareOfFloatsByEqualityOperator
            Assert.IsTrue( Math.Round( Math.Log( 1.414 ), 2 ) == Math.Round( ( double ) result, 2 ) );
            // ReSharper restore CompareOfFloatsByEqualityOperator

            //Sign
            query.SelectColumns.Clear( );

            query.SelectColumns =
                new List<SelectColumn>
					{
						new SelectColumn
							{
								ColumnName = "Sign(-100)",
								DisplayName = "Sign(-100)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Sign,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = -100
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "Sign(0)",
								DisplayName = "Sign(0)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Sign,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 0
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "Sign(10)",
								DisplayName = "Sign(10)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Log10,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 10
																}
														}
												}
										}
							}
					};
            //Get Data
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );

            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Sign(-100)" ].ToString( ), out result );

            Assert.IsTrue( Math.Sign( -100 ) == ( int ) result );

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Sign(0)" ].ToString( ), out result );
            Assert.IsTrue( Math.Sign( 0 ) == ( int ) result );

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Sign(10)" ].ToString( ), out result );
            Assert.IsTrue( Math.Sign( 10 ) == ( int ) result );
            //Absolute
            query.SelectColumns.Clear( );

            query.SelectColumns =
                new List<SelectColumn>
					{
						new SelectColumn
							{
								ColumnName = "Absolute(-100)",
								DisplayName = "Absolute(-100)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Abs,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = -100
																}
														}
												}
										}
							},
						new SelectColumn
							{
								ColumnName = "Absolute(100)",
								DisplayName = "Absolute(100)",
								Expression =
									new CalculationExpression
										{
											Operator = CalculationOperator.Abs,
											Expressions = new List<ScalarExpression>
												{
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.DecimalType,
																	Value = 100
																}
														}
												}
										}
							}
					};
            //Get Data
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            dataTable = RunQuery( query );
            decimal.TryParse( dataTable.Rows [ 0 ] [ "Absolute(-100)" ].ToString( ), out result );

            Assert.IsTrue( Math.Abs( -100 ) == ( int ) result );

            decimal.TryParse( dataTable.Rows [ 0 ] [ "Absolute(100)" ].ToString( ), out result );
            Assert.IsTrue( Math.Abs( 100 ) == ( int ) result );
            //Power(-2,4)
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Power(-2,4)",
                    DisplayName = "Power(-2,4)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Power,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = -2
														}
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 4
														}
												}
										}
                        }
                }
                );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( Math.Pow( -2, 4 ).ToString( CultureInfo.InvariantCulture ), dataTable.Rows [ 0 ] [ "Power(-2,4)" ].ToString( ) );
            //-Power(2,4)
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "-Power(2,4)",
                    DisplayName = "-Power(2,4)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Subtract,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 0
														}
												},
											new CalculationExpression
												{
													Operator = CalculationOperator.Power,
													Expressions = new List<ScalarExpression>
														{
															new LiteralExpression
																{
																	Value = new TypedValue
																		{
																			Type = DatabaseType.Int32Type,
																			Value = 2
																		}
																},
															new LiteralExpression
																{
																	Value = new TypedValue
																		{
																			Type = DatabaseType.Int32Type,
																			Value = 4
																		}
																}
														}
												}
										}
                        }
                }
                );
            QueryBuilder.GetSql( query );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( ( -Math.Pow( 2, 4 ) ).ToString( CultureInfo.InvariantCulture ), dataTable.Rows [ 0 ] [ "-Power(2,4)" ].ToString( ) );
            //Power(-2,5)
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Power(-2,5)",
                    DisplayName = "Power(-2,5)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Power,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = -2
														}
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 5
														}
												}
										}
                        }
                }
                );
            QueryBuilder.GetSql( query );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( Math.Pow( -2, 5 ).ToString( CultureInfo.InvariantCulture ), dataTable.Rows [ 0 ] [ "Power(-2,5)" ].ToString( ) );
            /* string funtion*/
            //Concatenate(H,I)
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Concatenate(H,I)",
                    DisplayName = "Concatenate(H,I)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Concatenate,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _iColumn,
												}
										}
                        }
                } );
            //res = new StructuredQueryResource();
            //res.StructuredQuery = query;
            //xml = res.ToXml();
            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( string.Concat( H, I ), dataTable.Rows [ 0 ] [ "Concatenate(H,I)" ].ToString( ) );
            //Concatenate("abc,X) X.value is NULL
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Concatenate('abc',null)",
                    DisplayName = "Concatenate('abc',null)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Concatenate,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.StringType,
															Value = "abc"
														}
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _xColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( string.Concat( "abc", null ), dataTable.Rows [ 0 ] [ "Concatenate('abc',null)" ].ToString( ) );
            //IsNull,replaced by "New String"
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "IsNull",
                    DisplayName = "IsNull",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.IsNull,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _xColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.StringType,
															Value = "New String"
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "IsNull" ].ToString( ), "New String" );
            //Null,always return Null, no matter what structure expression 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "AlwaysNull",
                    DisplayName = "AlwaysNull",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Null,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _xColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.StringType,
															Value = "New String"
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "AlwaysNull" ].ToString( ), string.Empty );

            //Replace 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Replace",
                    DisplayName = "Replace",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Replace,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.StringType,
															Value = "l"
														}
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.StringType,
															Value = "--replace text--"
														}
												}
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "Replace" ].ToString( ), H.Replace( "l", "--replace text--" ) );

            //stringlength 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "stringlength",
                    DisplayName = "stringlength",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.StringLength,
                            Expressions = new List<ScalarExpression>
										{
											new CalculationExpression
												{
													Operator = CalculationOperator.Concatenate,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _hColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _iColumn,
																},
															new LiteralExpression
																{
																	Value = new TypedValue
																		{
																			Type = DatabaseType.StringType,
																			Value = "New String"
																		}
																}
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "stringlength" ].ToString( ), ( string.Concat( H, I, "New String" ) ).Length.ToString( CultureInfo.InvariantCulture ) );

            //LowerCase 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "LowerCase",
                    DisplayName = "LowerCase",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.ToLower,
                            Expressions = new List<ScalarExpression>
										{
											new CalculationExpression
												{
													Operator = CalculationOperator.Concatenate,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _hColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _iColumn,
																},
															new LiteralExpression
																{
																	Value = new TypedValue
																		{
																			Type = DatabaseType.StringType,
																			Value = "New String"
																		}
																}
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "LowerCase" ].ToString( ), ( string.Concat( H, I, "New String" ) ).ToLower( ) );
            //UpperCase 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "UpperCase",
                    DisplayName = "UpperCase",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.ToUpper,
                            Expressions = new List<ScalarExpression>
										{
											new CalculationExpression
												{
													Operator = CalculationOperator.Concatenate,
													Expressions = new List<ScalarExpression>
														{
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _hColumn,
																},
															new ResourceDataColumn
																{
																	NodeId = _nodeId,
																	FieldId = _iColumn,
																},
															new LiteralExpression
																{
																	Value = new TypedValue
																		{
																			Type = DatabaseType.StringType,
																			Value = "New String"
																		}
																}
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "UpperCase" ].ToString( ), ( string.Concat( H, I, "New String" ) ).ToUpper( ) );
            //Left 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Left",
                    DisplayName = "Left",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Left,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 2
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "Left" ].ToString( ), H.Substring( 0, 2 ) );
            //Right 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Right",
                    DisplayName = "Right",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Right,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 3
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "Right" ].ToString( ), H.Substring( H.Length - 3, 3 ) );

            //Substring(x,y,z) 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Substring(x,y,z)",
                    DisplayName = "Substring(x,y,z)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Substring,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 2
														}
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 2
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "Substring(x,y,z)" ].ToString( ), H.Substring( 1, 2 ) );
            //Subsring(x,y) 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Substring(x,y)",
                    DisplayName = "Substring(x,y)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Substring,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 2
														}
												}
										}
                        }
                } );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "Substring(x,y)" ].ToString( ), H.Substring( 1 ) );
            //Subsring(x) 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Substring(x)",
                    DisplayName = "Substring(x)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Substring,
                            Expressions = new List<ScalarExpression>
										{
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "Substring(x)" ].ToString( ), H );

            //Charindex(x,y,z) 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Charindex(x,y,z)",
                    DisplayName = "Charindex(x,y,z)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Charindex,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.StringType,
															Value = "l"
														}
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												},
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.Int32Type,
															Value = 4
														}
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            //C# indexOf base 0, SQL Charindex base 1
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "Charindex(x,y,z)" ].ToString( ), ( H.IndexOf( "l", 3, StringComparison.Ordinal ) + 1 ).ToString( CultureInfo.InvariantCulture ) );
            //Charindex(x,y) 
            query.SelectColumns.Clear( );

            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Charindex(x,y)",
                    DisplayName = "Charindex(x,y)",
                    Expression =
                        new CalculationExpression
                        {
                            Operator = CalculationOperator.Charindex,
                            Expressions = new List<ScalarExpression>
										{
											new LiteralExpression
												{
													Value = new TypedValue
														{
															Type = DatabaseType.StringType,
															Value = "l"
														}
												},
											new ResourceDataColumn
												{
													NodeId = _nodeId,
													FieldId = _hColumn,
												}
										}
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            //C# indexOf base 0, SQL Charindex base 1
            Assert.AreEqual( dataTable.Rows [ 0 ] [ "Charindex(x,y)" ].ToString( ), ( H.IndexOf( "l", StringComparison.Ordinal ) + 1 ).ToString( CultureInfo.InvariantCulture ) );

            #endregion

            #region Logical

            #endregion

            #region IFELSE

            //IFELSEBASE
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "IFELSEBASE",
                    DisplayName = "IFELSEBASE",
                    Expression =
                        new IfElseExpression
                        {
                            BooleanExpression = new ComparisonExpression
                            {
                                Operator = ComparisonOperator.GreaterThanEqual,
                                Expressions = new List<ScalarExpression>
												{
													new ResourceDataColumn
														{
															NodeId = _nodeId,
															FieldId = _aColumn,
														},
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.Int32Type,
																	Value = 0
																}
														}
												}
                            },
                            IfBlockExpression = new LiteralExpression
                            {
                                Value = new TypedValue
                                {
                                    Type = DatabaseType.StringType,
                                    Value = "Positive"
                                }
                            },
                            ElseBlockExpression = new LiteralExpression
                            {
                                Value = new TypedValue
                                {
                                    Type = DatabaseType.StringType,
                                    Value = "Negative"
                                }
                            },
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );

            Assert.AreEqual( dataTable.Rows [ 0 ] [ "IFELSEBASE" ].ToString( ), "Positive" );
            //IFELSELogicalCondition
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "IFELSELogicalCondition",
                    DisplayName = "IFELSELogicalCondition",
                    Expression =
                        new IfElseExpression
                        {
                            BooleanExpression = new LogicalExpression
                            {
                                Operator = LogicalOperator.And,
                                Expressions = new List<ScalarExpression>
												{
													new ComparisonExpression
														{
															Operator = ComparisonOperator.GreaterThanEqual,
															Expressions = new List<ScalarExpression>
																{
																	new ResourceDataColumn
																		{
																			NodeId = _nodeId,
																			FieldId = _bColumn,
																		},
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 10
																				}
																		}
																}
														},
													new ComparisonExpression
														{
															Operator = ComparisonOperator.LessThan,
															Expressions = new List<ScalarExpression>
																{
																	new ResourceDataColumn
																		{
																			NodeId = _nodeId,
																			FieldId = _bColumn,
																		},
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 15
																				}
																		}
																}
														}
												}
                            },
                            IfBlockExpression = new LiteralExpression
                            {
                                Value = new TypedValue
                                {
                                    Type = DatabaseType.StringType,
                                    Value = "Positive"
                                }
                            },
                            ElseBlockExpression = new LiteralExpression
                            {
                                Value = new TypedValue
                                {
                                    Type = DatabaseType.StringType,
                                    Value = "Negative"
                                }
                            },
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );

            Assert.AreEqual( dataTable.Rows [ 0 ] [ "IFELSELogicalCondition" ].ToString( ), "Negative" );

            //IFELSENestWithLogicalCondition
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "IFELSENestWithLogicalCondition",
                    DisplayName = "IFELSENestWithLogicalCondition",
                    Expression =
                        new IfElseExpression
                        {
                            BooleanExpression = new LogicalExpression
                            {
                                Operator = LogicalOperator.And,
                                Expressions = new List<ScalarExpression>
												{
													new ComparisonExpression
														{
															Operator = ComparisonOperator.GreaterThanEqual,
															Expressions = new List<ScalarExpression>
																{
																	new ResourceDataColumn
																		{
																			NodeId = _nodeId,
																			FieldId = _bColumn,
																		},
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 10
																				}
																		}
																}
														},
													new ComparisonExpression
														{
															Operator = ComparisonOperator.LessThan,
															Expressions = new List<ScalarExpression>
																{
																	new ResourceDataColumn
																		{
																			NodeId = _nodeId,
																			FieldId = _bColumn,
																		},
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 1000
																				}
																		}
																}
														}
												}
                            },
                            IfBlockExpression = new ResourceDataColumn
                            {
                                NodeId = _nodeId,
                                FieldId = _aColumn,
                            },
                            ElseBlockExpression = new IfElseExpression
                            {
                                BooleanExpression = new LogicalExpression
                                {
                                    Operator = LogicalOperator.Or,
                                    Expressions = new List<ScalarExpression>
														{
															new ComparisonExpression
																{
																	Operator = ComparisonOperator.GreaterThan,
																	Expressions = new List<ScalarExpression>
																		{
																			new ResourceDataColumn
																				{
																					NodeId = _nodeId,
																					FieldId = _bColumn,
																				},
																			new ResourceDataColumn
																				{
																					NodeId = _nodeId,
																					FieldId = _aColumn,
																				}
																		}
																},
															new ComparisonExpression
																{
																	Operator = ComparisonOperator.GreaterThan,
																	Expressions = new List<ScalarExpression>
																		{
																			new ResourceDataColumn
																				{
																					NodeId = _nodeId,
																					FieldId = _bColumn,
																				},
																			new ResourceDataColumn
																				{
																					NodeId = _nodeId,
																					FieldId = _dColumn,
																				}
																		}
																}
														}
                                },
                                IfBlockExpression = new ResourceDataColumn
                                {
                                    NodeId = _nodeId,
                                    FieldId = _hColumn,
                                },
                                ElseBlockExpression = new ResourceDataColumn
                                {
                                    NodeId = _nodeId,
                                    FieldId = _iColumn,
                                }
                            }
                        }
                } );
            //res = new StructuredQueryResource();
            //res.StructuredQuery = query;
            //xml = res.ToXml();
            //Get Data
            // ReSharper disable RedundantAssignment
            dataTable = RunQuery( query );
            // ReSharper restore RedundantAssignment

            //Assert.AreEqual(dataTable.Rows[0]["IFELSENestWithLogicalCondition"].ToString(), "IF-ELSE (IF HERE! ELSE)");
            //Case:A less than $29,701, the deduction rate is D*15%
            //A+29701 greater than or equal to $29,701, but less than $71,950, the deduction rate is D*25%
            //A+71950 greater than or equal to $71,950, the deduction rate is D*28%
            // IF(A<29701,D*15%,IF(A<71950,D*25%,D*28%))
            //decimal  rates =(decimal)0.15;
            //decimal rates =(decimal) 0.25;
            const decimal rates = ( decimal ) 0.28;

            //CalculationExpression AConvertValue = new CalculationExpression()
            //{
            //    Operator = CalculationOperator.Add,
            //    Expressions = new List<ScalarExpression>()
            //      {
            //          new ResourceDataColumn(){
            //                     NodeId = NodeID,
            //                     FieldId = AColumn ,
            //         },
            //         new LiteralExpression(){
            //             Value = new TypedValue()
            //             {
            //                 Type =EDC.Database.DatabaseType.CurrencyType,
            //                 Value =29701
            //             }
            //         }
            //      }
            //};
            //A+71950
            var aConvertValue = new CalculationExpression
            {
                Operator = CalculationOperator.Add,
                Expressions = new List<ScalarExpression>
						{
							new ResourceDataColumn
								{
									NodeId = _nodeId,
									FieldId = _aColumn,
								},
							new LiteralExpression
								{
									Value = new TypedValue
										{
											Type = DatabaseType.CurrencyType,
											Value = 71950
										}
								}
						}
            };

            var dRate1 = new CalculationExpression
            {
                Operator = CalculationOperator.Multiply,
                Expressions = new List<ScalarExpression>
						{
							new LiteralExpression
								{
									Value = new TypedValue
										{
											Type = DatabaseType.DecimalType,
											Value = 80000
										}
								},
							new LiteralExpression
								{
									Value = new TypedValue
										{
											Type = DatabaseType.DecimalType,
											Value = rates
										}
								}
						}
            };

            var dRate2 = new CalculationExpression
            {
                Operator = CalculationOperator.Multiply,
                Expressions = new List<ScalarExpression>
						{
							new LiteralExpression
								{
									Value = new TypedValue
										{
											Type = DatabaseType.DecimalType,
											Value = 80000
										}
								},
							new LiteralExpression
								{
									Value = new TypedValue
										{
											Type = DatabaseType.DecimalType,
											Value = 0.28
										}
								}
						}
            };
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "IFELSECase",
                    DisplayName = "IFELSECase",
                    Expression =
                        new IfElseExpression
                        {
                            BooleanExpression = new ComparisonExpression
                            {
                                Operator = ComparisonOperator.LessThan,
                                Expressions = new List<ScalarExpression>
												{
													//new ResourceDataColumn(){
													//            NodeId = NodeID,
													//            FieldId = AColumn ,
													//},

													aConvertValue,
													new LiteralExpression
														{
															Value = new TypedValue
																{
																	Type = DatabaseType.Int32Type,
																	Value = 29701
																}
														}
												}
                            },
                            IfBlockExpression = dRate1,
                            ElseBlockExpression = new IfElseExpression
                            {
                                //A+29701>29701 <71950
                                BooleanExpression = new LogicalExpression
                                {
                                    Operator = LogicalOperator.And,
                                    Expressions = new List<ScalarExpression>
														{
															new ComparisonExpression
																{
																	Operator = ComparisonOperator.GreaterThan,
																	Expressions = new List<ScalarExpression>
																		{
																			aConvertValue,
																			new LiteralExpression
																				{
																					Value = new TypedValue
																						{
																							Type = DatabaseType.Int32Type,
																							Value = 29701
																						}
																				}
																		}
																},
															new ComparisonExpression
																{
																	Operator = ComparisonOperator.LessThan,
																	Expressions = new List<ScalarExpression>
																		{
																			aConvertValue,
																			new LiteralExpression
																				{
																					Value = new TypedValue
																						{
																							Type = DatabaseType.Int32Type,
																							Value = 71950
																						}
																				}
																		}
																}
														}
                                },
                                IfBlockExpression = dRate1,
                                ElseBlockExpression = dRate2
                            }
                        }
                } );

            //Get Data
            dataTable = RunQuery( query );
            Assert.IsTrue( decimal.Parse( dataTable.Rows [ 0 ] [ "IFELSECase" ].ToString( ) ) == 80000 * rates );

            //IFELSENOTLogical

            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "IFELSENOT",
                    DisplayName = "IFELSENOT",
                    Expression =
                        new IfElseExpression
                        {
                            BooleanExpression = new LogicalExpression
                            {
                                Operator = LogicalOperator.Not,
                                Expressions = new List<ScalarExpression>
												{
													new ComparisonExpression
														{
															Operator = ComparisonOperator.GreaterThan,
															Expressions = new List<ScalarExpression>
																{
																	new ResourceDataColumn
																		{
																			NodeId = _nodeId,
																			FieldId = _aColumn,
																		},
																	new LiteralExpression
																		{
																			Value = new TypedValue
																				{
																					Type = DatabaseType.Int32Type,
																					Value = 20
																				}
																		}
																}
														}
												}
                            },
                            IfBlockExpression = new LiteralExpression
                            {
                                Value = new TypedValue
                                {
                                    Type = DatabaseType.BoolType,
                                    Value = true
                                }
                            },
                            ElseBlockExpression = new LiteralExpression
                            {
                                Value = new TypedValue
                                {
                                    Type = DatabaseType.BoolType,
                                    Value = false
                                }
                            },
                        }
                } );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "LogicalNot2",
                    DisplayName = "LogicalNot2",
                    Expression = new IfElseExpression
                    {
                        BooleanExpression = new LogicalExpression
                        {
                            Operator = LogicalOperator.Not,
                            Expressions = new List<ScalarExpression>
											{
												new ComparisonExpression
													{
														Operator = ComparisonOperator.GreaterThan,
														Expressions = new List<ScalarExpression>
															{
																new CalculationExpression
																	{
																		Operator = CalculationOperator.Add,
																		Expressions = new List<ScalarExpression>
																			{
																				new ResourceDataColumn
																					{
																						NodeId = _nodeId,
																						FieldId = _aColumn,
																					},
																				new LiteralExpression
																					{
																						Value = new TypedValue
																							{
																								Type = DatabaseType.Int32Type,
																								Value = 10
																							}
																					}
																			}
																	},
																new LiteralExpression
																	{
																		Value = new TypedValue
																			{
																				Type = DatabaseType.Int32Type,
																				Value = 15
																			}
																	}
															}
													}
											}
                        },
                        IfBlockExpression = new LiteralExpression
                        {
                            Value = new TypedValue
                            {
                                Type = DatabaseType.BoolType,
                                Value = true
                            }
                        },
                        ElseBlockExpression = new LiteralExpression
                        {
                            Value = new TypedValue
                            {
                                Type = DatabaseType.BoolType,
                                Value = false
                            }
                        },
                    },
                }
                );

            //Get Data
            dataTable = RunQuery( query );
            Assert.IsTrue( dataTable.Rows [ 0 ] [ "IFELSENOT" ].ToString( ) == "True" );

            Assert.IsTrue( dataTable.Rows [ 0 ] [ "LogicalNot2" ].ToString( ) == "False" );

            #endregion

            #region Datetime Test

            //Today,UTCToday,time, utctime
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "TodayDateTime-value",
                    DisplayName = "TodayDateTime-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.TodayDateTime,
                        Expressions = new List<ScalarExpression>( )
                    }
                }
                );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "TodayDate-value",
                    DisplayName = "TodayDate-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.TodayDate,
                        Expressions = new List<ScalarExpression>( )
                    }
                }
                );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Time-value",
                    DisplayName = "Time-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.Time,
                        Expressions = new List<ScalarExpression>( )
                    }
                }
                );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "DAY-value",
                    DisplayName = "DAY-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.Day,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 30 )
													}
											}
									}
                    }
                }
                );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Month-value",
                    DisplayName = "Month-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.Month,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 30 )
													}
											}
									}
                    }
                }
                );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Year-value",
                    DisplayName = "Year-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.Year,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 30 )
													}
											}
									}
                    }
                }
                );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Hour-value",
                    DisplayName = "Hour-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.Hour,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 30, 12, 15, 32 )
													}
											}
									}
                    }
                }
                );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Minute-value",
                    DisplayName = "Minute-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.Minute,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 30, 12, 15, 32 )
													}
											}
									}
                    }
                }
                );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "Second-value",
                    DisplayName = "Second-value",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.Second,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 30, 12, 15, 32 )
													}
											}
									}
                    }
                }
                );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            // ReSharper disable RedundantAssignment
            dataTable = RunQuery( query );
            // ReSharper restore RedundantAssignment
            //if no exception return true, check result in datatable
            Assert.IsTrue( true );

            //Add 20 to day based on  30/10/2007
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "AddFunctionOfDate",
                    DisplayName = "AddFunctionOfDate",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.DateAdd,
                        DateTimePart = DateTimeParts.Day,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 30 )
													}
											},
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.Int32Type,
														Value = 20
													}
											}
									}
                    }
                }
                );
            QueryBuilder.GetSql( query );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            // ReSharper disable RedundantAssignment
            dataTable = RunQuery( query );
            // ReSharper restore RedundantAssignment

            //Calculate the difference between 5/10/2007 and 8/10/2007
            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "DateDiff",
                    DisplayName = "DateDiff",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.DateDiff,
                        DateTimePart = DateTimeParts.Day,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 5 )
													}
											},
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.DateTimeType,
														Value = new DateTime( 2007, 10, 8 )
													}
											}
									}
                    }
                }
                );
            QueryBuilder.GetSql( query );
            res = new StructuredQueryResource
            {
                StructuredQuery = query
            };
            res.ToXml( );
            //Get Data
            dataTable = RunQuery( query );

            Assert.AreEqual( 3, ( int ) dataTable.Rows [ 0 ] [ "DateDiff" ], "DateDiff" );

            #endregion

            #region Avoid Exception SQL string

            query.SelectColumns.Clear( );
            query.SelectColumns.Add(
                new SelectColumn
                {
                    ColumnName = "ExceptionDividHandler",
                    DisplayName = "ExceptionDividHandler",
                    Expression = new CalculationExpression
                    {
                        Operator = CalculationOperator.Divide,
                        Expressions = new List<ScalarExpression>
									{
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.Int32Type,
														Value = 10
													}
											},
										new LiteralExpression
											{
												Value = new TypedValue
													{
														Type = DatabaseType.Int32Type,
														Value = 0
													}
											}
									}
                    }
                }
                );
            QueryBuilder.GetSql( query );

            //Get Data
            dataTable = RunQuery( query );
            if ( dataTable.Rows [ 0 ] [ "ExceptionDividHandler" ] == null || dataTable.Rows [ 0 ] [ "ExceptionDividHandler" ].ToString( ) == string.Empty )
            {
                Assert.IsTrue( true );
            }
            else
            {
                Assert.IsTrue( false );
            }

            #endregion
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestDeserialize( )
        {
            #region Add Test Data

            IEntity testData = Entity.Create( _testType.Id );
            testData.SetField( _aColumn.Id, A );
            testData.SetField( _bColumn.Id, B );
            testData.SetField( _cColumn.Id, C );
            testData.SetField( _dColumn.Id, D );
            testData.SetField( _gColumn.Id, G );
            testData.SetField( _hColumn.Id, H );
            testData.SetField( _iColumn.Id, I );
            testData.Save( );

            #endregion

            #region test Calculation SQL and SQL Result

            string queryString = "<Query xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://enterprisedata.com.au/readinow/v2/query/2.0\">" +
                                 "<RootEntity xsi:type=\"ResourceEntity\" id=\"8a2fdb07-a2ef-4488-af9d-362a2bf3e272\">" +
                                 "<EntityTypeId entityRef=\"true\">" + _testType.Id.ToString( CultureInfo.InvariantCulture ) + "</EntityTypeId>" +
                                 "</RootEntity>" +
                                 "<Columns>" +
                                 "<Column id=\"09a340ab-6516-424d-9fc8-41c4bce55ea2\">" +
                                 "<Expression xsi:type=\"ResourceDataColumn\">" +
                                 "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                                 "<FieldId entityRef=\"true\">" + _aColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                                 "</Expression>" +
                                 "<ColumnName>A</ColumnName>" +
                                 "<DisplayName>A</DisplayName>" +
                                 "<IsHidden>false</IsHidden>" +
                                 "</Column>" +
                                 "<Column id=\"09a340ab-6516-424d-9fc8-41c4bce55ea2\">" +
                                 "<Expression xsi:type=\"ResourceDataColumn\">" +
                                 "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                                 "<FieldId entityRef=\"true\">" + _bColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                                 "</Expression>" +
                                 "<ColumnName>B</ColumnName>" +
                                 "<DisplayName>B</DisplayName>" +
                                 "<IsHidden>false</IsHidden>" +
                                 "</Column>" +
                                 "{0}" +
                                 "</Columns>" +
                                 "<Conditions />" +
                                 "</Query>";

            //A+B
            string calculationFormat =
                "<Column id=\"D71A297E-E37C-41B9-ADA1-9FB4E7C4309F\">" +
                "<Expression xsi:type=\"CalculationExpression\">" +
                "<Operator>Add</Operator>" +
                "<Expressions>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _aColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _bColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "</Expressions>" +
                "</Expression>" +
                "<ColumnName>A+B</ColumnName>" +
                "<DisplayName>A+B</DisplayName>" +
                "<IsHidden>false</IsHidden>" +
                "</Column>";
            using ( var reader = new StringReader( string.Format( queryString, calculationFormat ) ) )
            {
                var serializer = new XmlSerializer( typeof( StructuredQuery ) );

                var query = ( StructuredQuery ) serializer.Deserialize( reader );

                QueryBuilder.GetSql( query );

                DataTable dataTable = RunQuery( query );
                decimal result;
                decimal.TryParse( dataTable.Rows [ 0 ] [ "A+B" ].ToString( ), out result );

                Assert.AreEqual( A + B, result );
            }
            //A+(B-C)+D
            calculationFormat =
                "<Column id=\"D71A297E-E37C-41B9-ADA1-9FB4E7C4309F\">" +
                "<Expression xsi:type=\"CalculationExpression\">" +
                "<Operator>Add</Operator>" +
                "<Expressions>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _aColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "<Expression xsi:type=\"CalculationExpression\">" +
                "<Operator>Subtract</Operator>" +
                "<Expressions>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _bColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _cColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "</Expressions>" +
                "</Expression>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _dColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "</Expressions>" +
                "</Expression>" +
                "<ColumnName>A+(B-C)+D</ColumnName>" +
                "<DisplayName>A+(B-C)+D</DisplayName>" +
                "<IsHidden>false</IsHidden>" +
                "</Column>";
            using ( var reader = new StringReader( string.Format( queryString, calculationFormat ) ) )
            {
                var serializer = new XmlSerializer( typeof( StructuredQuery ) );

                var query = ( StructuredQuery ) serializer.Deserialize( reader );

                QueryBuilder.GetSql( query );

                DataTable dataTable = RunQuery( query );
                decimal result;
                decimal.TryParse( dataTable.Rows [ 0 ] [ "A+(B-C)+D" ].ToString( ), out result );

                Assert.AreEqual( A + ( B - C ) + D, result );
            }
            //A+(B*C)+D
            calculationFormat =
                "<Column id=\"D71A297E-E37C-41B9-ADA1-9FB4E7C4309F\">" +
                "<Expression xsi:type=\"CalculationExpression\">" +
                "<Operator>Add</Operator>" +
                "<Expressions>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _aColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "<Expression xsi:type=\"CalculationExpression\">" +
                "<Operator>Multiply</Operator>" +
                "<Expressions>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _bColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _cColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "</Expressions>" +
                "</Expression>" +
                "<Expression xsi:type=\"ResourceDataColumn\">" +
                "<NodeId>8a2fdb07-a2ef-4488-af9d-362a2bf3e272</NodeId>" +
                "<FieldId entityRef=\"true\">" + _dColumn.Id.ToString( CultureInfo.InvariantCulture ) + "</FieldId>" +
                "</Expression>" +
                "</Expressions>" +
                "</Expression>" +
                "<ColumnName>A+(B*C)+D</ColumnName>" +
                "<DisplayName>A+(B*C)+D</DisplayName>" +
                "<IsHidden>false</IsHidden>" +
                "</Column>";
            using ( var reader = new StringReader( string.Format( queryString, calculationFormat ) ) )
            {
                var serializer = new XmlSerializer( typeof( StructuredQuery ) );

                var query = ( StructuredQuery ) serializer.Deserialize( reader );

                QueryBuilder.GetSql( query );

                DataTable dataTable = RunQuery( query );
                decimal result;
                decimal.TryParse( dataTable.Rows [ 0 ] [ "A+(B*C)+D" ].ToString( ), out result );

                Assert.AreEqual( A + ( B * C ) + D, result );
            }

            //delete the row
            testData.Delete( );

            #endregion
        }
    }
}