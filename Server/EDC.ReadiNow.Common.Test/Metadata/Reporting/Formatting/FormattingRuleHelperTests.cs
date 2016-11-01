// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using EDC.Database;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Media;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using EDC.ReadiNow.Model;
using NUnit.Framework;

using BarFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.BarFormattingRule;
using ImageFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ImageFormattingRule;
using ColorFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ColorFormattingRule;
using IconFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.IconFormattingRule;
using IconRule = EDC.ReadiNow.Metadata.Reporting.Formatting.IconRule;
using ColorRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ColorRule;

namespace EDC.ReadiNow.Test.Metadata.Reporting.Formatting
{
	/// <summary>
	///     This class tests the FormattingRuleHelper class.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class FormattingRuleHelperTests
	{
		/// <summary>
		///     Create test color formatting rules.
		/// </summary>
		/// <returns></returns>
		public static List<ColumnFormatting> CreateTestColorFormattingRules( )
		{
			var formats = new List<ColumnFormatting>( );

			var format1 = new ColumnFormatting
				{
					QueryColumnId = new Guid( "{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}" ),
					ColumnName = "Column1",
					ColumnType = DatabaseType.Int32Type,
					ShowText = true,
					FormattingRule = new ColorFormattingRule
						{
							Rules = new List<ColorRule>
								{
									new ColorRule
										{
											BackgroundColor = new ColorInfo
												{
													A = 0,
													B = 0,
													R = 0,
													G = 255
												},
											ForegroundColor = new ColorInfo
												{
													A = 0,
													B = 0,
													R = 255,
													G = 0
												},
											Condition = new Condition
												{
													ColumnName = "Column1",
													ColumnType = DatabaseType.Int32Type,
													Operator = ConditionType.LessThan,
													Arguments = new List<TypedValue>
														{
															new TypedValue
																{
																	Type = DatabaseType.Int32Type,
																	Value = 20
																}
														}
												}
										}
								}
						}
				};

			var format2 = new ColumnFormatting
				{
					QueryColumnId = new Guid( "{8EE81FB6-F037-4031-A2B0-D5E585B8BA4E}" ),
					ColumnName = "Column2",
					ColumnType = DatabaseType.IdentifierType,
					ShowText = false,
					FormattingRule = new ColorFormattingRule
						{
							Rules = new List<ColorRule>
								{
									new ColorRule
										{
											BackgroundColor = new ColorInfo
												{
													A = 10,
													B = 10,
													R = 0,
													G = 255
												},
											ForegroundColor = new ColorInfo
												{
													A = 0,
													B = 0,
													R = 255,
													G = 5
												},
											Condition = new Condition
												{
													ColumnName = "Column2",
													ColumnType = DatabaseType.IdentifierType,
													Operator = ConditionType.GreaterThanOrEqual,
													Arguments = new List<TypedValue>
														{
															new TypedValue
																{
																	Type = DatabaseType.IdentifierType,
																	Value = 100L
																}
														}
												}
										}
								}
						}
				};

			formats.Add( format1 );
			formats.Add( format2 );

			return formats;
		}


		/// <summary>
		///     Create test icon formatting rules.
		/// </summary>
		/// <returns></returns>
		public static List<ColumnFormatting> CreateTestIconFormattingRules( )
		{
			var formats = new List<ColumnFormatting>( );

			var format1 = new ColumnFormatting
				{
					QueryColumnId = new Guid( "{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}" ),
					ColumnName = "Column1",
					ColumnType = DatabaseType.Int32Type,
					ShowText = true,
					FormattingRule = new IconFormattingRule
						{
							Rules = new List<IconRule>
								{
									new IconRule
										{
											Color = new ColorInfo
												{
													A = 0,
													B = 0,
													R = 0,
													G = 255
												},
											Icon = IconType.Cross,
											Condition = new Condition
												{
													ColumnName = "Column1",
													ColumnType = DatabaseType.Int32Type,
													Operator = ConditionType.LessThan,
													Arguments = new List<TypedValue>
														{
															new TypedValue
																{
																	Type = DatabaseType.Int32Type,
																	Value = 20
																}
														}
												},
											Scale = 1
										}
								}
						}
				};

			var format2 = new ColumnFormatting
				{
					QueryColumnId = new Guid( "{8EE81FB6-F037-4031-A2B0-D5E585B8BA4E}" ),
					ColumnName = "Column2",
					ColumnType = DatabaseType.IdentifierType,
					ShowText = false,
					FormattingRule = new IconFormattingRule
						{
							Rules = new List<IconRule>
								{
									new IconRule
										{
											Color = new ColorInfo
												{
													A = 10,
													B = 10,
													R = 0,
													G = 255
												},
											Icon = IconType.Square,
											Condition = new Condition
												{
													ColumnName = "Column2",
													ColumnType = DatabaseType.IdentifierType,
													Operator = ConditionType.GreaterThanOrEqual,
													Arguments = new List<TypedValue>
														{
															new TypedValue
																{
																	Type = DatabaseType.IdentifierType,
																	Value = 100L
																}
														}
												},
											Scale = 1
										}
								}
						}
				};

			formats.Add( format1 );
			formats.Add( format2 );

			return formats;
		}


        /// <summary>
        ///     Create test image formatting rules.
        /// </summary>
        /// <returns></returns>
        public static List<ColumnFormatting> CreateTestImageFormattingRules()
        {
            var formats = new List<ColumnFormatting>();

            var format1 = new ColumnFormatting
            {
                QueryColumnId = new Guid("{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}"),
                ColumnName = "Column1",
                ColumnType = DatabaseType.Int32Type,
                ShowText = true,
                FormattingRule = new ImageFormattingRule
                {
                    ThumbnailScaleId = new EntityRef(7969),
                    ThumbnailSizeId = new EntityRef(11807)                   
                }
            };

            var format2 = new ColumnFormatting
            {
                QueryColumnId = new Guid("{8EE81FB6-F037-4031-A2B0-D5E585B8BA4E}"),
                ColumnName = "Column2",
                ColumnType = DatabaseType.IdentifierType,
                ShowText = false,
                FormattingRule = new ImageFormattingRule
                {
                    ThumbnailScaleId = new EntityRef("core","scaleImageProportionally"),
                    ThumbnailSizeId = new EntityRef("console","smallThumbnail")                    
                }
            };

            formats.Add(format1);
            formats.Add(format2);

            return formats;
        }


		/// <summary>
		///     Create test bar formatting rules.
		/// </summary>
		/// <returns></returns>
		public static List<ColumnFormatting> CreateTestBarFormattingRules( )
		{
			var formats = new List<ColumnFormatting>( );

			var format1 = new ColumnFormatting
				{
					QueryColumnId = new Guid( "{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}" ),
					ColumnName = "Column1",
					ColumnType = DatabaseType.Int32Type,
					ShowText = true,
					FormattingRule = new BarFormattingRule
						{
							Color = new ColorInfo
								{
									A = 0,
									B = 0,
									R = 0,
									G = 255
								},
							Minimum = new TypedValue
								{
									Type = DatabaseType.Int32Type,
									Value = 0
								},
							Maximum = new TypedValue
								{
									Type = DatabaseType.Int32Type,
									Value = 100
								}
						}
				};

			var format2 = new ColumnFormatting
				{
					QueryColumnId = new Guid( "{8EE81FB6-F037-4031-A2B0-D5E585B8BA4E}" ),
					ColumnName = "Column2",
					ColumnType = DatabaseType.IdentifierType,
					ShowText = false,
					FormattingRule = new BarFormattingRule
						{
							Color = new ColorInfo
								{
									A = 0,
									B = 0,
									R = 100,
									G = 0
								},
							Minimum = new TypedValue
								{
									Type = DatabaseType.IdentifierType,
									Value = 100L
								},
							Maximum = new TypedValue
								{
									Type = DatabaseType.IdentifierType,
									Value = 1000L
								}
						}
				};

			formats.Add( format1 );
			formats.Add( format2 );

			return formats;
		}


		/// <summary>
		/// </summary>
		/// <param name="formats1"></param>
		/// <param name="formats2"></param>
		public static void CompareColumnFormats( IList<ColumnFormatting> formats1, IList<ColumnFormatting> formats2 )
		{
			Assert.AreEqual( formats1.Count, formats2.Count, "The count of formats is invalid." );

			for ( int i = 0; i < formats1.Count; i++ )
			{
				ColumnFormatting cf1 = formats1[ i ];
				ColumnFormatting cf2 = formats2[ i ];

				Assert.AreEqual( cf1.QueryColumnId, cf2.QueryColumnId, "Format Index:{0} QueryColumnId is invalid.", i.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cf1.ColumnName, cf2.ColumnName, "Format Index:{0} ColumnName is invalid.", i.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cf1.ColumnType.GetType( ), cf2.ColumnType.GetType( ), "Format Index:{0} ColumnType is invalid.", i.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cf1.ShowText, cf2.ShowText, "Format Index:{0} ShowText is invalid.", i.ToString( CultureInfo.InvariantCulture ) );

				Assert.IsNotNull( cf1.FormattingRule, "Format Index:{0} FormattingRule 1 should not be null.", i.ToString( CultureInfo.InvariantCulture ) );
				Assert.IsNotNull( cf2.FormattingRule, "Format Index:{0} FormattingRule 2 should not be null.", i.ToString( CultureInfo.InvariantCulture ) );

				if ( cf1.FormattingRule is ColorFormattingRule )
				{
					var cfr1 = cf1.FormattingRule as ColorFormattingRule;
					var cfr2 = cf2.FormattingRule as ColorFormattingRule;
					CompareColorFormattingRules( i, cfr1, cfr2 );
				}
				else if ( cf1.FormattingRule is IconFormattingRule )
				{
					var ifr1 = cf1.FormattingRule as IconFormattingRule;
					var ifr2 = cf2.FormattingRule as IconFormattingRule;
					CompareIconFormattingRules( i, ifr1, ifr2 );
				}
				else if ( cf1.FormattingRule is BarFormattingRule )
				{
					var bfr1 = cf1.FormattingRule as BarFormattingRule;
					var bfr2 = cf2.FormattingRule as BarFormattingRule;
					CompareBarFormattingRules( i, bfr1, bfr2 );
				}
                else if (cf1.FormattingRule is ImageFormattingRule)
                {
                    var imfr1 = cf1.FormattingRule as ImageFormattingRule;
                    var imfr2 = cf2.FormattingRule as ImageFormattingRule;
                    CompareImageFormattingRules(i, imfr1, imfr2);
                }
                else
                {
                    Assert.Fail("The formatting rule is invalid.");
                }
			}
		}


		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		/// <param name="cfr1"></param>
		/// <param name="cfr2"></param>
		private static void CompareColorFormattingRules( int index, ColorFormattingRule cfr1, ColorFormattingRule cfr2 )
		{
			Assert.IsNotNull( cfr1, "Format Index:{0} ColorFormattingRule 1 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.IsNotNull( cfr2, "Format Index:{0} ColorFormattingRule 2 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );

			Assert.IsNotNull( cfr1.Rules, "Format Index:{0} ColorFormattingRule.Rules 1 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.IsNotNull( cfr2.Rules, "Format Index:{0} ColorFormattingRule.Rules 2 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );

			Assert.AreEqual( cfr1.Rules.Count, cfr2.Rules.Count, "Format Index:{0} ColorFormattingRule.Rules.Count is invalid.", index.ToString( CultureInfo.InvariantCulture ) );

			for ( int ri = 0; ri < cfr1.Rules.Count; ri++ )
			{
				ColorRule cr1 = cfr1.Rules[ ri ];
				ColorRule cr2 = cfr2.Rules[ ri ];

				Assert.AreEqual( cr1.BackgroundColor.A, cr2.BackgroundColor.A, "Format Index:{0} Rule Index:{1} BackgroundColor.A is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.BackgroundColor.R, cr2.BackgroundColor.R, "Format Index:{0} Rule Index:{1} BackgroundColor.R is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.BackgroundColor.G, cr2.BackgroundColor.G, "Format Index:{0} Rule Index:{1} BackgroundColor.G is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.BackgroundColor.B, cr2.BackgroundColor.B, "Format Index:{0} Rule Index:{1} BackgroundColor.B is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );

				Assert.AreEqual( cr1.ForegroundColor.A, cr2.ForegroundColor.A, "Format Index:{0} Rule Index:{1} ForegroundColor.A is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.ForegroundColor.R, cr2.ForegroundColor.R, "Format Index:{0} Rule Index:{1} ForegroundColor.R is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.ForegroundColor.G, cr2.ForegroundColor.G, "Format Index:{0} Rule Index:{1} ForegroundColor.G is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.ForegroundColor.B, cr2.ForegroundColor.B, "Format Index:{0} Rule Index:{1} ForegroundColor.B is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );

				Assert.AreEqual( cr1.Condition.ColumnName, cr2.Condition.ColumnName, "Format Index:{0} Rule Index:{1} Condition.ColumnName is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.Condition.ColumnType.GetType( ), cr2.Condition.ColumnType.GetType( ), "Format Index:{0} Rule Index:{1} Condition.ColumnType is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.Condition.Operator, cr2.Condition.Operator, "Format Index:{0} Rule Index:{1} Condition.Operator is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( cr1.Condition.Arguments.Count, cr2.Condition.Arguments.Count, "Format Index:{0} Rule Index:{1} Condition.Arguments.Count is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );

				for ( int ti = 0; ti < cr1.Condition.Arguments.Count; ti++ )
				{
					TypedValue tv1 = cr1.Condition.Arguments[ ti ];
					TypedValue tv2 = cr1.Condition.Arguments[ ti ];

					Assert.AreEqual( tv1.Type, tv2.Type, "Format Index:{0} Rule Index:{1} Arg Index:{2} Argument.Type is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ), ti.ToString( CultureInfo.InvariantCulture ) );
					Assert.AreEqual( tv1.Value, tv2.Value, "Format Index:{0} Rule Index:{1} Arg Index:{2} Argument.Value is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ), ti.ToString( CultureInfo.InvariantCulture ) );
				}
			}
		}


		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		/// <param name="ifr1"></param>
		/// <param name="ifr2"></param>
		private static void CompareIconFormattingRules( int index, IconFormattingRule ifr1, IconFormattingRule ifr2 )
		{
			Assert.IsNotNull( ifr1, "Format Index:{0} IconFormattingRule 1 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.IsNotNull( ifr2, "Format Index:{0} IconFormattingRule 2 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );

			Assert.IsNotNull( ifr1.Rules, "Format Index:{0} IconFormattingRule.Rules 1 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.IsNotNull( ifr2.Rules, "Format Index:{0} IconFormattingRule.Rules 2 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );

			Assert.AreEqual( ifr1.Rules.Count, ifr2.Rules.Count, "Format Index:{0} IconFormattingRule.Rules.Count is invalid.", index.ToString( CultureInfo.InvariantCulture ) );

			for ( int ri = 0; ri < ifr1.Rules.Count; ri++ )
			{
				IconRule ir1 = ifr1.Rules[ ri ];
				IconRule ir2 = ifr2.Rules[ ri ];

				Assert.AreEqual( ir1.Color.A, ir2.Color.A, "Format Index:{0} Rule Index:{1} Color.A is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( ir1.Color.R, ir2.Color.R, "Format Index:{0} Rule Index:{1} Color.R is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( ir1.Color.G, ir2.Color.G, "Format Index:{0} Rule Index:{1} Color.G is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( ir1.Color.B, ir2.Color.B, "Format Index:{0} Rule Index:{1} Color.B is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );

				Assert.AreEqual( ir1.Icon, ir2.Icon, "Format Index:{0} Rule Index:{1} Icon is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );

				Assert.AreEqual( ir1.Condition.ColumnName, ir2.Condition.ColumnName, "Format Index:{0} Rule Index:{1} Condition.ColumnName is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( ir1.Condition.ColumnType.GetType( ), ir2.Condition.ColumnType.GetType( ), "Format Index:{0} Rule Index:{1} Condition.ColumnType is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( ir1.Condition.Operator, ir2.Condition.Operator, "Format Index:{0} Rule Index:{1} Condition.Operator is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );
				Assert.AreEqual( ir1.Condition.Arguments.Count, ir2.Condition.Arguments.Count, "Format Index:{0} Rule Index:{1} Condition.Arguments.Count is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ) );

				for ( int ti = 0; ti < ir1.Condition.Arguments.Count; ti++ )
				{
					TypedValue tv1 = ir1.Condition.Arguments[ ti ];
					TypedValue tv2 = ir1.Condition.Arguments[ ti ];

					Assert.AreEqual( tv1.Type, tv2.Type, "Format Index:{0} Rule Index:{1} Arg Index:{2} Argument.Type is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ), ti.ToString( CultureInfo.InvariantCulture ) );
					Assert.AreEqual( tv1.Value, tv2.Value, "Format Index:{0} Rule Index:{1} Arg Index:{2} Argument.Value is invalid", index.ToString( CultureInfo.InvariantCulture ), ri.ToString( CultureInfo.InvariantCulture ), ti.ToString( CultureInfo.InvariantCulture ) );
				}
			}
		}


		/// <summary>
		/// </summary>
		/// <param name="index"></param>
		/// <param name="bfr1"></param>
		/// <param name="bfr2"></param>
		private static void CompareBarFormattingRules( int index, BarFormattingRule bfr1, BarFormattingRule bfr2 )
		{
			Assert.IsNotNull( bfr1, "Format Index:{0} BarFormattingRule 1 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.IsNotNull( bfr2, "Format Index:{0} BarFormattingRule 2 should not be null.", index.ToString( CultureInfo.InvariantCulture ) );

			Assert.AreEqual( bfr1.Color.A, bfr2.Color.A, "Format Index:{0} Color.A is invalid", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.AreEqual( bfr1.Color.R, bfr2.Color.R, "Format Index:{0} Color.R is invalid", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.AreEqual( bfr1.Color.G, bfr2.Color.G, "Format Index:{0} Color.G is invalid", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.AreEqual( bfr1.Color.B, bfr2.Color.B, "Format Index:{0} Color.B is invalid", index.ToString( CultureInfo.InvariantCulture ) );

			Assert.AreEqual( bfr1.Minimum.Type.GetType( ), bfr2.Minimum.Type.GetType( ), "Format Index:{0} Minimum.Type is invalid", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.AreEqual( bfr1.Minimum.Value, bfr2.Minimum.Value, "Format Index:{0} Minimum.Value is invalid", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.AreEqual( bfr1.Maximum.Type.GetType( ), bfr2.Maximum.Type.GetType( ), "Format Index:{0} Maximum.Type is invalid", index.ToString( CultureInfo.InvariantCulture ) );
			Assert.AreEqual( bfr1.Maximum.Value, bfr2.Maximum.Value, "Format Index:{0} Maximum.Value is invalid", index.ToString( CultureInfo.InvariantCulture ) );
		}

        /// <summary>
        /// Compares the image formatting rules.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="ifr1">The ifr1.</param>
        /// <param name="ifr2">The ifr2.</param>
	    private static void CompareImageFormattingRules(int index, ImageFormattingRule ifr1, ImageFormattingRule ifr2)
	    {
            Assert.IsNotNull(ifr1, "Format Index:{0} ImageFormattingRule 1 should not be null.", index.ToString(CultureInfo.InvariantCulture));
            Assert.IsNotNull(ifr2, "Format Index:{0} ImageFormattingRule 2 should not be null.", index.ToString(CultureInfo.InvariantCulture));

            Assert.AreEqual(ifr1.ThumbnailScaleId.XmlSerializationText, ifr2.ThumbnailScaleId.XmlSerializationText, "Format Index:{0} ThumbnailScaleId is invalid", index.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(ifr1.ThumbnailSizeId.XmlSerializationText, ifr2.ThumbnailSizeId.XmlSerializationText, "Format Index:{0} ThumbnailSizeId is invalid", index.ToString(CultureInfo.InvariantCulture));            
	    }

	    /// <summary>
		///     Tests the ReadColumnFormatsXml method when valid data
		///     for a barFormattingRule is used.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void ReadColumnFormatsXmlBarFormattingRuleNodeTest( )
		{
			#region Setup

			const string image = @"<gridReportDataView>
  <columnFormats>
	<columnFormat>
	  <queryColumnId>{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
	  <columnName>Column1</columnName>
	  <type>Int32</type>
	  <showText>True</showText>
	  <barFormattingRule>
		<color a='0' r='0' g='255' b='0' />
		<minimum>
		  <type>Int32</type>
		  <value>0</value>
		</minimum>
		<maximum>
		  <type>Int32</type>
		  <value>100</value>
		</maximum>
	  </barFormattingRule>
	</columnFormat>
	<columnFormat>
	  <queryColumnId>{8EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
	  <columnName>Column2</columnName>
	  <type>Identifier</type>
	  <showText>False</showText>
	  <barFormattingRule>
		<color a='0' r='100' g='0' b='0' />
		<minimum>
		  <type>Identifier</type>
		  <value>100</value>
		</minimum>
		<maximum>
		  <type>Identifier</type>
		  <value>1000</value>
		</maximum>
	  </barFormattingRule>
	</columnFormat>
  </columnFormats>
</gridReportDataView>";

			#endregion

			#region Test           

			var document = new XmlDocument( );
			document.LoadXml( image );

			IList<ColumnFormatting> newColumnFormats = FormattingRuleHelper.ReadColumnFormatsXml( document.DocumentElement );

			#endregion

			#region Validate

			List<ColumnFormatting> originalColumnFormats = CreateTestBarFormattingRules( );
			CompareColumnFormats( originalColumnFormats, newColumnFormats );

			#endregion
		}

		/// <summary>
		///     Tests the ReadColumnFormatsXml method for a
		///     colorFormattingRule with no rules.
		/// </summary>
		[Test]
		public void ReadColumnFormatsXmlColorFormattingRuleNoRulesTest( )
		{
			#region Setup

			const string image = @"<gridReportDataView>
	<columnFormats>
	  <columnFormat>
		<queryColumnId>{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
		<columnName>Test Column</columnName>
		<type>Int32</type>
		<showText>True</showText>
		<colorFormattingRule>        
		</colorFormattingRule>
	  </columnFormat>
   </columnFormats>
</gridReportDataView>";

			var document = new XmlDocument( );
			document.LoadXml( image );

			#endregion

			#region Test

			List<ColumnFormatting> formats = FormattingRuleHelper.ReadColumnFormatsXml( document.DocumentElement );

			#endregion

			#region Validate

			Assert.AreEqual( 1, formats.Count, "The number of column formats is invalid." );
			Assert.AreEqual( "Test Column", formats[ 0 ].ColumnName, "ColumnName is invalid." );
			Assert.AreEqual( new Guid( "{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}" ), formats[ 0 ].QueryColumnId, "QueryColumnId is invalid." );
			Assert.AreEqual( DatabaseType.Int32Type.GetType( ), formats[ 0 ].ColumnType.GetType( ), "ColumnType is invalid." );
			Assert.AreEqual( true, formats[ 0 ].ShowText, "ShowText is invalid." );
			Assert.IsNotNull( formats[ 0 ].FormattingRule, "FormattingRule is invalid." );

			var colorFormattingRule = formats[ 0 ].FormattingRule as ColorFormattingRule;
			Assert.IsNotNull( colorFormattingRule, "ColorFormattingRule is invalid." );
			Assert.AreEqual( 0, colorFormattingRule.Rules.Count, "Rules count is invalid." );

			#endregion
		}

		/// <summary>
		///     Tests the ReadColumnFormatsXml method when valid data
		///     for a colorFormattingRule is used.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void ReadColumnFormatsXmlColorFormattingRuleNodeTest( )
		{
			#region Setup

			const string image = @"<gridReportDataView>
  <columnFormats>
	<columnFormat>
	  <queryColumnId>{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
	  <columnName>Column1</columnName>
	  <type>Int32</type>
	  <showText>True</showText>
	  <colorFormattingRule>
		<rules>
		  <rule>
			<backgroundColor a='0' r='0' g='255' b='0' />
			<foregroundColor a='0' r='255' g='0' b='0' />
			<condition>
			  <operator>LessThan</operator>
			  <columnName>Column1</columnName>
			  <type>Int32</type>
			  <arguments>
				<argument>
				  <type>Int32</type>
				  <value>20</value>
				</argument>
			  </arguments>
			</condition>
		  </rule>
		</rules>
	  </colorFormattingRule>
	</columnFormat>
	<columnFormat>
	  <queryColumnId>{8EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
	  <columnName>Column2</columnName>
	  <type>Identifier</type>
	  <showText>False</showText>
	  <colorFormattingRule>
		<rules>
		  <rule>
			<backgroundColor a='10' r='0' g='255' b='10' />
			<foregroundColor a='0' r='255' g='5' b='0' />
			<condition>
			  <operator>GreaterThanOrEqual</operator>
			  <columnName>Column2</columnName>
			  <type>Identifier</type>
			  <arguments>
				<argument>
				  <type>Identifier</type>
				  <value>100</value>
				</argument>
			  </arguments>
			</condition>
		  </rule>
		</rules>
	  </colorFormattingRule>
	</columnFormat>
  </columnFormats>
</gridReportDataView>";

			#endregion

			#region Test            

			var document = new XmlDocument( );
			document.LoadXml( image );

			IList<ColumnFormatting> newColumnFormats = FormattingRuleHelper.ReadColumnFormatsXml( document.DocumentElement );

			#endregion

			#region Validate

			List<ColumnFormatting> originalColumnFormats = CreateTestColorFormattingRules( );
			CompareColumnFormats( originalColumnFormats, newColumnFormats );

			#endregion
		}


		/// <summary>
		///     Tests the ReadColumnFormatsXml method when valid data
		///     for a iconFormattingRule is used.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void ReadColumnFormatsXmlIconFormattingRuleNodeTest( )
		{
			#region Setup

			const string image = @"<gridReportDataView>
  <columnFormats>
	<columnFormat>
	  <queryColumnId>{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
	  <columnName>Column1</columnName>
	  <type>Int32</type>
	  <showText>True</showText>
	  <iconFormattingRule>
		<rules>
		  <rule>
			<color a='0' r='0' g='255' b='0' />
			<icon>Cross</icon>
			<condition>
			  <operator>LessThan</operator>
			  <columnName>Column1</columnName>
			  <type>Int32</type>
			  <arguments>
				<argument>
				  <type>Int32</type>
				  <value>20</value>
				</argument>
			  </arguments>
			</condition>
		  </rule>
		</rules>
	  </iconFormattingRule>
	</columnFormat>
	<columnFormat>
	  <queryColumnId>{8EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
	  <columnName>Column2</columnName>
	  <type>Identifier</type>
	  <showText>False</showText>
	  <iconFormattingRule>
		<rules>
		  <rule>
			<color a='10' r='0' g='255' b='10' />
			<icon>Square</icon>
			<condition>
			  <operator>GreaterThanOrEqual</operator>
			  <columnName>Column2</columnName>
			  <type>Identifier</type>
			  <arguments>
				<argument>
				  <type>Identifier</type>
				  <value>100</value>
				</argument>
			  </arguments>
			</condition>
		  </rule>
		</rules>
	  </iconFormattingRule>
	</columnFormat>
  </columnFormats>
</gridReportDataView>";

			#endregion

			#region Test                                    

			var document = new XmlDocument( );
			document.LoadXml( image );

			IList<ColumnFormatting> newColumnFormats = FormattingRuleHelper.ReadColumnFormatsXml( document.DocumentElement );

			#endregion

			#region Validate

			List<ColumnFormatting> originalColumnFormats = CreateTestIconFormattingRules( );
			CompareColumnFormats( originalColumnFormats, newColumnFormats );

			#endregion
		}

        /// <summary>
        ///     Tests the ReadColumnFormatsXml method when valid data
        ///     for an imageFormattingRule is used.
        /// </summary>
        [Test]
        [RunAsGlobalTenant]
        public void ReadColumnFormatsXmlImageFormattingRuleNodeTest()
        {
            #region Setup

            const string image = @"<gridReportDataView>
  <columnFormats>
	<columnFormat>
	  <queryColumnId>{7EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
	  <columnName>Column1</columnName>
	  <type>Int32</type>
	  <showText>True</showText>
	  <imageFormattingRule>
         <thumbnailScaleId entityRef='true'>7969</thumbnailScaleId>
         <thumbnailSizeId entityRef='true'>11807</thumbnailSizeId>
      </imageFormattingRule>
	</columnFormat>
	<columnFormat>
	  <queryColumnId>{8EE81FB6-F037-4031-A2B0-D5E585B8BA4E}</queryColumnId>
	  <columnName>Column2</columnName>
	  <type>Identifier</type>
	  <showText>False</showText>
	  <imageFormattingRule>
         <thumbnailScaleId entityRef='true'>core:scaleImageProportionally</thumbnailScaleId>
         <thumbnailSizeId entityRef='true'>console:smallThumbnail</thumbnailSizeId>
      </imageFormattingRule>
	</columnFormat>
  </columnFormats>
</gridReportDataView>";

            #endregion

            #region Test

            var document = new XmlDocument();
            document.LoadXml(image);

            IList<ColumnFormatting> newColumnFormats = FormattingRuleHelper.ReadColumnFormatsXml(document.DocumentElement);

            #endregion

            #region Validate

            List<ColumnFormatting> originalColumnFormats = CreateTestImageFormattingRules();
            CompareColumnFormats(originalColumnFormats, newColumnFormats);

            #endregion
        }

		/// <summary>
		///     Tests the ReadColumnFormatsXml method for an
		///     invalid column formats xml.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void ReadColumnFormatsXmlInvalidColumnFormatsTest( )
		{
			#region Setup

			const string image = @"<randomXml>    
</randomXml>";

			var document = new XmlDocument( );
			document.LoadXml( image );

			#endregion

			#region Test

			List<ColumnFormatting> formats = FormattingRuleHelper.ReadColumnFormatsXml( document.DocumentElement );

			#endregion

			#region Validate

			Assert.AreEqual( 0, formats.Count, "The number of column formats is invalid." );

			#endregion
		}

		/// <summary>
		///     Tests that the ReadColumnFormatsXml method throws the correct exception when
		///     a null node is used.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void ReadColumnFormatsXmlNullNodeTest( )
		{
			FormattingRuleHelper.ReadColumnFormatsXml( null );
		}

		/// <summary>
		///     Tests the WriteColumnFormatsXml method when valid data
		///     for a barFormattingRule is used.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void WriteColumnFormatsXmlBarFormattingRuleNodeTest( )
		{
			#region Setup

			List<ColumnFormatting> originalColumnFormats = CreateTestBarFormattingRules( );

			#endregion

			#region Test

			var writerText = new StringBuilder( );
			using ( XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} ) )
			{
				FormattingRuleHelper.WriteColumnFormatsXml( originalColumnFormats, writer );
			}
			writerText.Insert( 0, "<gridReportDataView>" );
			writerText.Append( "</gridReportDataView>" );

			#endregion

			#region Validate

			var document = new XmlDocument( );
			document.LoadXml( writerText.ToString( ) );

			IList<ColumnFormatting> newColumnFormats = FormattingRuleHelper.ReadColumnFormatsXml( document.DocumentElement );

			CompareColumnFormats( originalColumnFormats, newColumnFormats );

			#endregion
		}

		/// <summary>
		///     Tests the WriteColumnFormatsXml method when valid data
		///     for a colorFormattingRule is used.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void WriteColumnFormatsXmlColorFormattingRuleNodeTest( )
		{
			#region Setup

			List<ColumnFormatting> originalColumnFormats = CreateTestColorFormattingRules( );

			#endregion

			#region Test

			var writerText = new StringBuilder( );
			using ( XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} ) )
			{
				FormattingRuleHelper.WriteColumnFormatsXml( originalColumnFormats, writer );
			}
			writerText.Insert( 0, "<gridReportDataView>" );
			writerText.Append( "</gridReportDataView>" );

			#endregion

			#region Validate

			var document = new XmlDocument( );
			document.LoadXml( writerText.ToString( ) );

			IList<ColumnFormatting> newColumnFormats = FormattingRuleHelper.ReadColumnFormatsXml( document.DocumentElement );

			CompareColumnFormats( originalColumnFormats, newColumnFormats );

			#endregion
		}


		/// <summary>
		///     Tests the WriteColumnFormatsXml method when valid data
		///     for a iconFormattingRule is used.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void WriteColumnFormatsXmlIconFormattingRuleNodeTest( )
		{
			#region Setup

			List<ColumnFormatting> originalColumnFormats = CreateTestIconFormattingRules( );

			#endregion

			#region Test

			var writerText = new StringBuilder( );
			using ( XmlWriter writer = XmlWriter.Create( writerText, new XmlWriterSettings
				{
					OmitXmlDeclaration = true
				} ) )
			{
				FormattingRuleHelper.WriteColumnFormatsXml( originalColumnFormats, writer );
			}
			writerText.Insert( 0, "<gridReportDataView>" );
			writerText.Append( "</gridReportDataView>" );

			#endregion

			#region Validate

			var document = new XmlDocument( );
			document.LoadXml( writerText.ToString( ) );

			IList<ColumnFormatting> newColumnFormats = FormattingRuleHelper.ReadColumnFormatsXml( document.DocumentElement );

			CompareColumnFormats( originalColumnFormats, newColumnFormats );

			#endregion
		}

        /// <summary>
        ///     Tests the WriteColumnFormatsXml method when valid data
        ///     for a imageFormattingRule is used.
        /// </summary>
        [Test]
        [RunAsGlobalTenant]
        public void WriteColumnFormatsXmlImageFormattingRuleNodeTest()
        {
            #region Setup

            List<ColumnFormatting> originalColumnFormats = CreateTestImageFormattingRules();

            #endregion

            #region Test

            var writerText = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(writerText, new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            }))
            {
                FormattingRuleHelper.WriteColumnFormatsXml(originalColumnFormats, writer);
            }
            writerText.Insert(0, "<gridReportDataView>");
            writerText.Append("</gridReportDataView>");

            #endregion

            #region Validate

            var document = new XmlDocument();
            document.LoadXml(writerText.ToString());

            IList<ColumnFormatting> newColumnFormats = FormattingRuleHelper.ReadColumnFormatsXml(document.DocumentElement);

            CompareColumnFormats(originalColumnFormats, newColumnFormats);

            #endregion
        }

		/// <summary>
		///     Tests that the WriteColumnFormatsXml throws the correct exception when
		///     a null value for the column formats is specified.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteColumnFormatsXmlNullColumnFormatsTest( )
		{
			var writerText = new StringBuilder( );
			XmlWriter writer = XmlWriter.Create( writerText );

			FormattingRuleHelper.WriteColumnFormatsXml( null, writer );
		}


		/// <summary>
		///     Tests that the WriteColumnFormatsXml throws the correct exception when
		///     a null value for the xml writer is specified.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void WriteColumnFormatsXmlNullWriterTest( )
		{
			var columnFormats = new List<ColumnFormatting>( );

			FormattingRuleHelper.WriteColumnFormatsXml( columnFormats, null );
		}
	}
}