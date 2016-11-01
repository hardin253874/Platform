// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace EDC.SoftwarePlatform.Services.ExportData
{
	/// <summary>
	///     Class to provide all the methods to export data to word document.
	/// </summary>
	public class ExportToWordHelper
	{
		/// <summary>
		///     Add content row to the table.
		/// </summary>
		private static TableRow AddContentRow( QueryResult queryResult, DataRow contentRow )
		{
			TableCell tc;
			TableRow tr = new TableRow( );
			int dataColIndx = 0;
			foreach ( ResultColumn col in queryResult.Columns )
			{
				if ( !col.IsHidden )
				{
					object cellValue = contentRow[ dataColIndx ];
					if ( cellValue != null )
					{
						if ( col.ColumnType is AutoIncrementType )
						{
							string displayPattern = null;

							if ( contentRow.Table.Columns[ dataColIndx ].ExtendedProperties.ContainsKey( "DisplayPattern" ) )
							{
								displayPattern = ( string ) contentRow.Table.Columns[ dataColIndx ].ExtendedProperties[ "DisplayPattern" ];
							}

							var intStringVal = cellValue as string;

							int temp;

							if ( intStringVal != null )
							{
								temp = int.Parse( intStringVal );
							}
							else
							{
								temp = ( int ) cellValue;
							}

							if ( string.IsNullOrEmpty( displayPattern ) )
							{
								AddContentRow_NonString( tr, col, temp );
							}
							else
							{
								AddContentRow_String( tr, col, temp.ToString( displayPattern ) );
							}
						}
						else if ( col.ColumnType is DateTimeType || col.ColumnType is TimeType || col.ColumnType is DateType || col.ColumnType is Int32Type || col.ColumnType is NumericType<decimal> || col.ColumnType is NumericType<Single> )
						{
							AddContentRow_NonString( tr, col, cellValue );
						}
						else if ( col.ColumnType is StructureLevelsType )
						{
							//Get the structure view cell value
							string cellText = ExportDataHelper.GetStructureLevelCellValue( ( string ) contentRow[ dataColIndx ], true );
							string[ ] views = cellText.Split( new[ ]
							{
								Environment.NewLine
							}, StringSplitOptions.None );
							int i = 1;

							tc = new TableCell( );
							Paragraph paragraph = new Paragraph( );
							Run run = new Run( );
							foreach ( string structureView in views )
							{
								run.Append( new DocumentFormat.OpenXml.Wordprocessing.Text( structureView ) );
								if ( i != views.Length )
								{
									//Insert a break for new line.
									run.Append( new Break( ) );
								}
								else
								{
									paragraph.Append( run );
									tc.Append( paragraph );
									tr.Append( tc );
								}
								i++;
							}
						}
						else
						{
							AddContentRow_String( tr, col, cellValue );
						}
					}
					else
					{
						tc = new TableCell( new Paragraph( new Run(
							new DocumentFormat.OpenXml.Wordprocessing.Text( "" ) ) ) );
						tr.Append( tc );
					}
				}
				dataColIndx++;
			}
			return tr;
		}

		/// <summary>
		///     Adds the content row_ non string.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <param name="col">The col.</param>
		/// <param name="cellValue">The cell value.</param>
		private static void AddContentRow_NonString( TableRow tr, ResultColumn col, object cellValue )
		{
			TableCell tc;
			//if data types are Numeric or datetime type, the cell alignment set to right justified
			ParagraphProperties paragraphProperties = new ParagraphProperties( );
			Justification justification = new Justification
			{
				Val = JustificationValues.Right
			};

			paragraphProperties.Append( justification );

			string cellText = GetFormattedCellValue( cellValue, col.ColumnType );
			tc = new TableCell( new Paragraph( paragraphProperties, new Run(
				new DocumentFormat.OpenXml.Wordprocessing.Text( cellText ) ) ) );
			tr.Append( tc );
		}

		/// <summary>
		///     Adds the content row_ string.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <param name="col">The col.</param>
		/// <param name="cellValue">The cell value.</param>
		private static void AddContentRow_String( TableRow tr, ResultColumn col, object cellValue )
		{
			TableCell tc;
			string cellText = GetFormattedCellValue( cellValue, col.ColumnType, col.RequestColumn.Expression is AggregateExpression );
			tc = new TableCell( new Paragraph( new Run(
				new DocumentFormat.OpenXml.Wordprocessing.Text( cellText ) ) ) );
			tr.Append( tc );
		}

		/// <summary>
		///     Create Header row styles.
		/// </summary>
		private static TableStyleProperties CreateHeaderRowStyles( )
		{
			TableStyleProperties tableStyleProperties1 = new TableStyleProperties
			{
				Type = TableStyleOverrideValues.FirstRow
			};

			StyleParagraphProperties styleParagraphProperties2 = new StyleParagraphProperties( );
			SpacingBetweenLines spacingBetweenLines3 = new SpacingBetweenLines
			{
				Before = "0",
				After = "0",
				Line = "240",
				LineRule = LineSpacingRuleValues.Auto
			};

			styleParagraphProperties2.Append( spacingBetweenLines3 );

			RunPropertiesBaseStyle runPropertiesBaseStyle2 = new RunPropertiesBaseStyle( );
			Bold bold1 = new Bold( );
			BoldComplexScript boldComplexScript1 = new BoldComplexScript( );
			Color color1 = new Color
			{
				Val = "FFFFFF",
				ThemeColor = ThemeColorValues.Background1
			};

			runPropertiesBaseStyle2.Append( bold1 );
			runPropertiesBaseStyle2.Append( boldComplexScript1 );
			runPropertiesBaseStyle2.Append( color1 );
			TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties1 = new TableStyleConditionalFormattingTableProperties( );

			TableStyleConditionalFormattingTableRowProperties tableStyleConditionalFormattingTableRowProperties1 = new TableStyleConditionalFormattingTableRowProperties( );
			TableHeader tableHeader1 = new TableHeader( );

			tableStyleConditionalFormattingTableRowProperties1.Append( tableHeader1 );

			TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties1 = new TableStyleConditionalFormattingTableCellProperties( );

			TableCellBorders tableCellBorders1 = new TableCellBorders( );
			TopBorder topBorder2 = new TopBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			LeftBorder leftBorder2 = new LeftBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			BottomBorder bottomBorder2 = new BottomBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			RightBorder rightBorder2 = new RightBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			InsideHorizontalBorder insideHorizontalBorder2 = new InsideHorizontalBorder
			{
				Val = BorderValues.Nil
			};
			InsideVerticalBorder insideVerticalBorder1 = new InsideVerticalBorder
			{
				Val = BorderValues.Nil
			};

			tableCellBorders1.Append( topBorder2 );
			tableCellBorders1.Append( leftBorder2 );
			tableCellBorders1.Append( bottomBorder2 );
			tableCellBorders1.Append( rightBorder2 );
			tableCellBorders1.Append( insideHorizontalBorder2 );
			tableCellBorders1.Append( insideVerticalBorder1 );

			Shading shading1 = new Shading
			{
				Val = ShadingPatternValues.Clear,
				Color = "auto",
				Fill = "4F81BD",
				ThemeFill = ThemeColorValues.Accent1
			};

			tableStyleConditionalFormattingTableCellProperties1.Append( tableCellBorders1 );
			tableStyleConditionalFormattingTableCellProperties1.Append( shading1 );

			tableStyleProperties1.Append( styleParagraphProperties2 );
			tableStyleProperties1.Append( runPropertiesBaseStyle2 );
			tableStyleProperties1.Append( tableStyleConditionalFormattingTableProperties1 );
			tableStyleProperties1.Append( tableStyleConditionalFormattingTableRowProperties1 );
			tableStyleProperties1.Append( tableStyleConditionalFormattingTableCellProperties1 );
			return tableStyleProperties1;
		}

		/// <summary>
		///     Create Last Row Styles.
		/// </summary>
		private static TableStyleProperties CreateLastRowStyles( )
		{
			TableStyleProperties tableStyleProperties2 = new TableStyleProperties
			{
				Type = TableStyleOverrideValues.LastRow
			};

			StyleParagraphProperties styleParagraphProperties3 = new StyleParagraphProperties( );
			SpacingBetweenLines spacingBetweenLines4 = new SpacingBetweenLines
			{
				Before = "0",
				After = "0",
				Line = "240",
				LineRule = LineSpacingRuleValues.Auto
			};

			styleParagraphProperties3.Append( spacingBetweenLines4 );

			RunPropertiesBaseStyle runPropertiesBaseStyle3 = new RunPropertiesBaseStyle( );
			Bold bold2 = new Bold( );
			BoldComplexScript boldComplexScript2 = new BoldComplexScript( );

			runPropertiesBaseStyle3.Append( bold2 );
			runPropertiesBaseStyle3.Append( boldComplexScript2 );
			TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties2 = new TableStyleConditionalFormattingTableProperties( );

			TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties2 = new TableStyleConditionalFormattingTableCellProperties( );

			TableCellBorders tableCellBorders2 = new TableCellBorders( );
			TopBorder topBorder3 = new TopBorder
			{
				Val = BorderValues.Double,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 6U,
				Space = 0U
			};
			LeftBorder leftBorder3 = new LeftBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			BottomBorder bottomBorder3 = new BottomBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			RightBorder rightBorder3 = new RightBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			InsideHorizontalBorder insideHorizontalBorder3 = new InsideHorizontalBorder
			{
				Val = BorderValues.Nil
			};
			InsideVerticalBorder insideVerticalBorder2 = new InsideVerticalBorder
			{
				Val = BorderValues.Nil
			};

			tableCellBorders2.Append( topBorder3 );
			tableCellBorders2.Append( leftBorder3 );
			tableCellBorders2.Append( bottomBorder3 );
			tableCellBorders2.Append( rightBorder3 );
			tableCellBorders2.Append( insideHorizontalBorder3 );
			tableCellBorders2.Append( insideVerticalBorder2 );

			tableStyleConditionalFormattingTableCellProperties2.Append( tableCellBorders2 );

			tableStyleProperties2.Append( styleParagraphProperties3 );
			tableStyleProperties2.Append( runPropertiesBaseStyle3 );
			tableStyleProperties2.Append( tableStyleConditionalFormattingTableProperties2 );
			tableStyleProperties2.Append( tableStyleConditionalFormattingTableCellProperties2 );
			return tableStyleProperties2;
		}

		/// <summary>
		///     Create Table Border styles
		/// </summary>
		private static TableBorders CreateTableBorderStyles( )
		{
			TableBorders tableBorders1 = new TableBorders( );
			TopBorder topBorder1 = new TopBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			LeftBorder leftBorder1 = new LeftBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			BottomBorder bottomBorder1 = new BottomBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			RightBorder rightBorder1 = new RightBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};
			InsideHorizontalBorder insideHorizontalBorder1 = new InsideHorizontalBorder
			{
				Val = BorderValues.Single,
				Color = "7BA0CD",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeTint = "BF",
				Size = 8U,
				Space = 0U
			};

			tableBorders1.Append( topBorder1 );
			tableBorders1.Append( leftBorder1 );
			tableBorders1.Append( bottomBorder1 );
			tableBorders1.Append( rightBorder1 );
			tableBorders1.Append( insideHorizontalBorder1 );
			return tableBorders1;
		}

		/// <summary>
		///     Define table cell margins.
		/// </summary>
		private static TableCellMarginDefault CreateTableCellMarginStyle( )
		{
			TableCellMarginDefault tableCellMarginDefault2 = new TableCellMarginDefault( );
			TopMargin topMargin2 = new TopMargin
			{
				Width = "0",
				Type = TableWidthUnitValues.Dxa
			};
			TableCellLeftMargin tableCellLeftMargin2 = new TableCellLeftMargin
			{
				Width = 108,
				Type = TableWidthValues.Dxa
			};
			BottomMargin bottomMargin2 = new BottomMargin
			{
				Width = "0",
				Type = TableWidthUnitValues.Dxa
			};
			TableCellRightMargin tableCellRightMargin2 = new TableCellRightMargin
			{
				Width = 108,
				Type = TableWidthValues.Dxa
			};

			tableCellMarginDefault2.Append( topMargin2 );
			tableCellMarginDefault2.Append( tableCellLeftMargin2 );
			tableCellMarginDefault2.Append( bottomMargin2 );
			tableCellMarginDefault2.Append( tableCellRightMargin2 );
			return tableCellMarginDefault2;
		}

		/// <summary>
		///     Generate word document.
		/// </summary>
		/// <param name="queryResult">QueryResult</param>
		/// <param name="reportName">name of the report.</param>
		/// <returns>MemoryStream</returns>
		public static MemoryStream CreateWordDocument( QueryResult queryResult, string reportName )
		{
			MemoryStream ms = new MemoryStream( );
			using ( WordprocessingDocument doc = WordprocessingDocument.Create( ms, WordprocessingDocumentType.Document ) )
			{
				MainDocumentPart mainPart = doc.AddMainDocumentPart( );
				mainPart.Document = new Document( );

				//Create document styles..
				StyleDefinitionsPart stylePart = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>( );
				stylePart.Styles = new Styles( );
				stylePart.Styles.Append( GenerateReportHeadingStyle( ) );
				stylePart.Styles.Append( GenerateTableStyle( ) );
				stylePart.Styles.Save( );

				Body body = new Body(
					new SectionProperties(
						new PageSize
						{
							Width = 15840,
							Height = 12240,
							Orient = PageOrientationValues.Landscape
						} ) );

				Paragraph para = new Paragraph( );
				ParagraphProperties paraProperties = new ParagraphProperties( );
				paraProperties.ParagraphStyleId = new ParagraphStyleId
				{
					Val = "Heading1"
				};
				para.Append( paraProperties );

				Run run_para = new Run( );
				DocumentFormat.OpenXml.Wordprocessing.Text text_para = new DocumentFormat.OpenXml.Wordprocessing.Text( reportName );
				run_para.Append( text_para );
				para.Append( run_para );
				body.Append( para );

				//Append table to body
				Table wordTable = CreateWordTable( queryResult );
				body.Append( wordTable );

				mainPart.Document.Append( body );
			}
			return ms;
		}

		/// <summary>
		///     Create word table
		/// </summary>
		private static Table CreateWordTable( QueryResult queryResult )
		{
			Table table = new Table( );
			TableProperties tblPr = new TableProperties( );
			TableStyle tableStyle = new TableStyle
			{
				Val = "MediumShading1-Accent1"
			};
			tblPr.Append( tableStyle );
			TableWidth tableWidth = new TableWidth
			{
				Width = "100%",
				Type = TableWidthUnitValues.Pct
			};
			tblPr.Append( tableWidth );
			table.Append( tblPr );

			TableRow tr;
			TableCell tc;

			//Append Header row
			tr = new TableRow( );
			foreach ( ResultColumn col in queryResult.Columns )
			{
				if ( !col.IsHidden )
				{
					tc = new TableCell( new Paragraph( new Run(
						new DocumentFormat.OpenXml.Wordprocessing.Text( col.DisplayName ) ) ) );
					tr.Append( tc );
				}
			}
			table.Append( tr );

			//Append all the data rows from the table. 
			foreach ( DataRow row in queryResult.DataTable.Rows )
			{
				tr = AddContentRow( queryResult, row );
				table.Append( tr );
			}
			return table;
		}

		/// <summary>
		///     Export to word document.
		/// </summary>
		/// <param name="dataTable">Data table</param>
		/// <returns>ExportInfo</returns>
		public static ExportDataInfo ExportToWord( QueryResult queryResult, string reportName )
		{
			MemoryStream ms = new MemoryStream( );
			ms = CreateWordDocument( queryResult, reportName );
			//Convert the memory stream to the byte array.
			byte[ ] bytesInStream = new byte[ms.Length];
			ms.Position = 0;
			ms.Read( bytesInStream, 0, bytesInStream.Length );

			ExportDataInfo export = new ExportDataInfo( );
			export.FileStream = bytesInStream;
			ms.Close( );
			return export;
		}

		private static Style GenerateReportHeadingStyle( )
		{
			//set the run properties.
			RunProperties rPr = new RunProperties( );
			Color color = new Color
			{
				Val = "365F91",
				ThemeColor = ThemeColorValues.Accent1,
				ThemeShade = "BF"
			}; // the color is dark blue
			RunFonts rFont = new RunFonts( );
			rFont.Ascii = "Cambria"; // the font is Cambria
			rPr.Append( color );
			rPr.Append( rFont );
			rPr.Append( new Bold( ) ); // it is Bold
			rPr.Append( new FontSize
			{
				Val = "28"
			} );
			Style style = new Style( );

			style.StyleId = "Heading1"; //this is the ID of the style
			style.Append( new Name
			{
				Val = "Heading 1"
			} ); //this is name
			// our style based on Normal style
			style.Append( new BasedOn
			{
				Val = "Heading1"
			} );
			// the next paragraph is Normal type
			style.Append( new NextParagraphStyle
			{
				Val = "Normal"
			} );
			style.Append( rPr );

			return style;
		}

		/// <summary>
		///     Generate a Table style
		/// </summary>
		public static Style GenerateTableStyle( )
		{
			Style style = new Style
			{
				Type = StyleValues.Table,
				StyleId = "MediumShading1-Accent1"
			};
			StyleName styleName5 = new StyleName
			{
				Val = "Medium Shading 1 Accent 1"
			};
			BasedOn basedOn1 = new BasedOn
			{
				Val = "TableNormal"
			};
			UIPriority uIPriority4 = new UIPriority
			{
				Val = 63
			};
			Rsid rsid1 = new Rsid
			{
				Val = "004A67A2"
			};

			StyleParagraphProperties styleParagraphProperties1 = new StyleParagraphProperties( );
			SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines
			{
				After = "0",
				Line = "240",
				LineRule = LineSpacingRuleValues.Auto
			};

			styleParagraphProperties1.Append( spacingBetweenLines2 );

			StyleTableProperties styleTableProperties2 = new StyleTableProperties( );
			TableStyleRowBandSize tableStyleRowBandSize1 = new TableStyleRowBandSize
			{
				Val = 1
			};
			TableStyleColumnBandSize tableStyleColumnBandSize1 = new TableStyleColumnBandSize
			{
				Val = 1
			};
			TableIndentation tableIndentation2 = new TableIndentation
			{
				Width = 0,
				Type = TableWidthUnitValues.Dxa
			};

			//Create Table Border styles
			TableBorders tableBorders1 = CreateTableBorderStyles( );

			//Create Table Border styles
			TableCellMarginDefault tableCellMarginDefault2 = CreateTableCellMarginStyle( );

			styleTableProperties2.Append( tableStyleRowBandSize1 );
			styleTableProperties2.Append( tableStyleColumnBandSize1 );
			styleTableProperties2.Append( tableIndentation2 );
			styleTableProperties2.Append( tableBorders1 );
			styleTableProperties2.Append( tableCellMarginDefault2 );

			//Create Header row styles.
			TableStyleProperties tableStyleProperties1 = CreateHeaderRowStyles( );

			//Create Last Row Styles.
			TableStyleProperties tableStyleProperties2 = CreateLastRowStyles( );


			TableStyleProperties tableStyleProperties3 = new TableStyleProperties
			{
				Type = TableStyleOverrideValues.Band1Vertical
			};
			TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties3 = new TableStyleConditionalFormattingTableProperties( );

			TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties3 = new TableStyleConditionalFormattingTableCellProperties( );
			Shading shading2 = new Shading
			{
				Val = ShadingPatternValues.Clear,
				Color = "auto",
				Fill = "D3DFEE",
				ThemeFill = ThemeColorValues.Accent1,
				ThemeFillTint = "3F"
			};

			tableStyleConditionalFormattingTableCellProperties3.Append( shading2 );

			tableStyleProperties3.Append( tableStyleConditionalFormattingTableProperties3 );
			tableStyleProperties3.Append( tableStyleConditionalFormattingTableCellProperties3 );

			TableStyleProperties tableStyleProperties4 = new TableStyleProperties
			{
				Type = TableStyleOverrideValues.Band1Horizontal
			};
			TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties4 = new TableStyleConditionalFormattingTableProperties( );

			TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties4 = new TableStyleConditionalFormattingTableCellProperties( );

			TableCellBorders tableCellBorders3 = new TableCellBorders( );
			InsideHorizontalBorder insideHorizontalBorder4 = new InsideHorizontalBorder
			{
				Val = BorderValues.Nil
			};
			InsideVerticalBorder insideVerticalBorder3 = new InsideVerticalBorder
			{
				Val = BorderValues.Nil
			};

			tableCellBorders3.Append( insideHorizontalBorder4 );
			tableCellBorders3.Append( insideVerticalBorder3 );
			Shading shading3 = new Shading
			{
				Val = ShadingPatternValues.Clear,
				Color = "auto",
				Fill = "D3DFEE",
				ThemeFill = ThemeColorValues.Accent1,
				ThemeFillTint = "3F"
			};

			tableStyleConditionalFormattingTableCellProperties4.Append( tableCellBorders3 );
			tableStyleConditionalFormattingTableCellProperties4.Append( shading3 );

			tableStyleProperties4.Append( tableStyleConditionalFormattingTableProperties4 );
			tableStyleProperties4.Append( tableStyleConditionalFormattingTableCellProperties4 );

			TableStyleProperties tableStyleProperties5 = new TableStyleProperties
			{
				Type = TableStyleOverrideValues.Band2Horizontal
			};
			TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties5 = new TableStyleConditionalFormattingTableProperties( );

			TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties5 = new TableStyleConditionalFormattingTableCellProperties( );

			TableCellBorders tableCellBorders4 = new TableCellBorders( );
			InsideHorizontalBorder insideHorizontalBorder5 = new InsideHorizontalBorder
			{
				Val = BorderValues.Nil
			};
			InsideVerticalBorder insideVerticalBorder4 = new InsideVerticalBorder
			{
				Val = BorderValues.Nil
			};

			tableCellBorders4.Append( insideHorizontalBorder5 );
			tableCellBorders4.Append( insideVerticalBorder4 );

			tableStyleConditionalFormattingTableCellProperties5.Append( tableCellBorders4 );

			tableStyleProperties5.Append( tableStyleConditionalFormattingTableProperties5 );
			tableStyleProperties5.Append( tableStyleConditionalFormattingTableCellProperties5 );

			style.Append( styleName5 );
			style.Append( basedOn1 );
			style.Append( uIPriority4 );
			style.Append( rsid1 );
			style.Append( styleParagraphProperties1 );
			style.Append( styleTableProperties2 );
			style.Append( tableStyleProperties1 );
			style.Append( tableStyleProperties2 );
			style.Append( tableStyleProperties3 );
			style.Append( tableStyleProperties4 );
			style.Append( tableStyleProperties5 );

			return style;
		}

		/// <summary>
		///     Get the formatted cell value for the different data types.
		/// </summary>
		private static string GetFormattedCellValue( object cellValue, DatabaseType type, bool isRelationshipType = false )
		{
			if ( cellValue == null || cellValue is DBNull )
			{
				return "";
			}
			if ( type is BoolType )
			{
				string result = ExportDataHelper.GetBooleanCellValue( ( bool ) cellValue );
				return result;
			}
			if ( type is ChoiceRelationshipType || type is InlineRelationshipType || isRelationshipType )
			{
				string result = DatabaseTypeHelper.GetEntityXmlName( ( string ) cellValue );
				return result;
			}
			else
			{
				// TODO: UTC ??!!

				string formatString = DatabaseTypeHelper.GetDisplayFormatString( type );
				string result = string.Format( formatString, cellValue );

				return result;
			}
		}
	}
}