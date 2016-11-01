// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using EDC.Database.Types;
using EDC.Database;
using ReadiNow.Reporting.Result;

namespace EDC.SoftwarePlatform.Services.ExportData
{
    /// <summary>
    /// Class to provide all the methods to export data to word document.
    /// </summary>
    public class ExportToWord
    {
        /// <summary>
        /// Generate word document.
        /// </summary>
        /// <param name="reportResult">ReportResult</param>
        /// <param name="reportName">name of the report.</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream CreateWordDocument(ReportResult reportResult, List<DataRow> rows, string reportName)
        {
             MemoryStream ms = new MemoryStream();
            using(WordprocessingDocument doc = WordprocessingDocument.Create(ms,WordprocessingDocumentType.Document))
            {               
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();

                //Create document styles..
                StyleDefinitionsPart stylePart = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
                stylePart.Styles = new Styles();
                stylePart.Styles.Append(GenerateReportHeadingStyle());
                stylePart.Styles.Append(GenerateTableStyle());
                stylePart.Styles.Save(); 

                Body body = new Body(
                    new SectionProperties(
                        new PageSize 
                        { 
                            Width = 15840, 
                            Height = 12240, 
                            Orient = PageOrientationValues.Landscape 
                        }));
                
                Paragraph para = new Paragraph();
                ParagraphProperties paraProperties = new ParagraphProperties();                    
                paraProperties.ParagraphStyleId = new ParagraphStyleId() { Val = "Heading1" };
                para.Append(paraProperties);
                
                Run run_para = new Run();
                DocumentFormat.OpenXml.Wordprocessing.Text text_para = new DocumentFormat.OpenXml.Wordprocessing.Text(reportName);
                run_para.Append(text_para);
                para.Append(run_para);
                body.Append(para);

                //Append table to body
                Table wordTable = CreateWordTable(reportResult, rows);
                body.Append(wordTable);

                mainPart.Document.Append(body);
            }
            return ms;
        }
       
        /// <summary>
        /// Create word table
        /// </summary>       
        private static Table CreateWordTable(ReportResult reportResult, List<DataRow> rows)
        {
            Table table = new Table();            
            TableProperties tblPr = new TableProperties();
            TableStyle tableStyle = new TableStyle() { Val = "MediumShading1-Accent1" };
            tblPr.Append(tableStyle);            
            TableWidth tableWidth = new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Pct };
            tblPr.Append(tableWidth);  
            table.Append(tblPr);

            TableRow tr;
            TableCell tc;            
           
            //Append Header row
            tr = new TableRow();
            foreach (ReportColumn col in reportResult.Metadata.ReportColumns.Values)
            {
                if (!col.IsHidden && col.Type != "Image")
                {
                    tc = new TableCell(new Paragraph(new Run(
                                     new DocumentFormat.OpenXml.Wordprocessing.Text(col.Title))));
                    tr.Append(tc);
                }
            }
            table.Append(tr);

            //Append all the data rows from the table. 
            foreach (DataRow row in rows)
            {
                tr = AddContentRow(reportResult, row);
                table.Append(tr);
            }
            return table;
        }

        /// <summary>
        /// Add content row to the table.
        /// </summary>       
        private static TableRow AddContentRow(ReportResult reportResult, DataRow contentRow)
        {
            TableCell tc;
            TableRow tr = new TableRow();
            int dataColIndx = 0;
            foreach (ReportColumn col in reportResult.Metadata.ReportColumns.Values)
            {
                if (!col.IsHidden && col.Type != "Image")
                {
                    string cellValue = ExportDataHelper.GetCellValue(contentRow, dataColIndx);
                    DatabaseType cellType = DatabaseTypeHelper.ConvertFromDisplayName(col.Type);
                    if (!string.IsNullOrEmpty(col.AutoNumberDisplayPattern))
                    {
                        cellType = DatabaseType.AutoIncrementType;
                    }
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        cellValue = ExportDataHelper.GetFormattedCellValue(cellType, cellValue, col);

                        if (cellType is DateTimeType || cellType is TimeType || cellType is DateType || cellType is Int32Type || cellType is NumericType<decimal> || cellType is NumericType<Single>)
                        {
                            AddContentRow_NonString(tr, cellValue);
                        }
                        else if (cellType is StructureLevelsType)
                        {
                            AddContentRow_StructureLevel(tr, cellValue);
                        }
                        else
                        {
                            AddContentRow_String(tr, cellValue);
                        }
                    }
                    else
                    {
                        tc = new TableCell(new Paragraph(new Run(
                               new DocumentFormat.OpenXml.Wordprocessing.Text(""))));
                        tr.Append(tc);
                    }                    
                }
                dataColIndx++;
            }
            return tr;
        }

        /// <summary>
        /// Adds a structure level string.
        /// </summary>
        /// <param name="tableRow"></param>
        /// <param name="cellValue"></param>
        private static void AddContentRow_StructureLevel(TableRow tableRow, string cellValue)
        {                     
            string[] paths = cellValue.Split(new[]
            {
                Environment.NewLine
            }, StringSplitOptions.None);
            int i = 1;
            var tableCell = new TableCell();
            var paragraph = new Paragraph();
            var run = new Run();

            foreach (string path in paths)
            {
                run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text(path));
                if (i != paths.Length)
                {
                    //Insert a break for new line.
                    run.Append(new Break());
                }
                else
                {
                    paragraph.Append(run);
                    tableCell.Append(paragraph);
                    tableRow.Append(tableCell);
                }
                i++;
            }
        }

		/// <summary>
		/// Adds the content row_ string.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <param name="col">The col.</param>
		/// <param name="cellValue">The cell value.</param>
		private static void AddContentRow_String( TableRow tr, string cellValue )
		{
			TableCell tc;
			tc = new TableCell( new Paragraph( new Run(
                         new DocumentFormat.OpenXml.Wordprocessing.Text(cellValue))));
			tr.Append( tc );
		}

		/// <summary>
		/// Adds the content row_ non string.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <param name="col">The col.</param>
		/// <param name="cellValue">The cell value.</param>
		private static void AddContentRow_NonString( TableRow tr, string cellValue )
		{
			TableCell tc;
			//if data types are Numeric or datetime type, the cell alignment set to right justified
			ParagraphProperties paragraphProperties = new ParagraphProperties( );
			Justification justification = new Justification( )
			{
				Val = JustificationValues.Right
			};
			paragraphProperties.Append( justification );
          
			tc = new TableCell( new Paragraph( paragraphProperties, new Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text(cellValue))));
			tr.Append( tc );
		}
       
        /// <summary>
        /// Generate a Table style
        /// </summary>       
        public static Style GenerateTableStyle()
        {

            Style style = new Style() { Type = StyleValues.Table, StyleId = "MediumShading1-Accent1" };
            StyleName styleName5 = new StyleName() { Val = "Medium Shading 1 Accent 1" };
            BasedOn basedOn1 = new BasedOn() { Val = "TableNormal" };
            UIPriority uIPriority4 = new UIPriority() { Val = 63 };
            Rsid rsid1 = new Rsid() { Val = "004A67A2" };

            StyleParagraphProperties styleParagraphProperties1 = new StyleParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };

            styleParagraphProperties1.Append(spacingBetweenLines2);

            StyleTableProperties styleTableProperties2 = new StyleTableProperties();
            TableStyleRowBandSize tableStyleRowBandSize1 = new TableStyleRowBandSize() { Val = 1 };
            TableStyleColumnBandSize tableStyleColumnBandSize1 = new TableStyleColumnBandSize() { Val = 1 };
            TableIndentation tableIndentation2 = new TableIndentation() { Width = 0, Type = TableWidthUnitValues.Dxa };

            //Create Table Border styles
            TableBorders tableBorders1 = CreateTableBorderStyles();

            //Create Table Border styles
            TableCellMarginDefault tableCellMarginDefault2 = CreateTableCellMarginStyle();

            styleTableProperties2.Append(tableStyleRowBandSize1);
            styleTableProperties2.Append(tableStyleColumnBandSize1);
            styleTableProperties2.Append(tableIndentation2);
            styleTableProperties2.Append(tableBorders1);
            styleTableProperties2.Append(tableCellMarginDefault2);

            //Create Header row styles.
            TableStyleProperties tableStyleProperties1 = CreateHeaderRowStyles();

            //Create Last Row Styles.
            TableStyleProperties tableStyleProperties2 = CreateLastRowStyles();
         

            TableStyleProperties tableStyleProperties3 = new TableStyleProperties() { Type = TableStyleOverrideValues.Band1Vertical };
            TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties3 = new TableStyleConditionalFormattingTableProperties();

            TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties3 = new TableStyleConditionalFormattingTableCellProperties();
            Shading shading2 = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "D3DFEE", ThemeFill = ThemeColorValues.Accent1, ThemeFillTint = "3F" };

            tableStyleConditionalFormattingTableCellProperties3.Append(shading2);

            tableStyleProperties3.Append(tableStyleConditionalFormattingTableProperties3);
            tableStyleProperties3.Append(tableStyleConditionalFormattingTableCellProperties3);

            TableStyleProperties tableStyleProperties4 = new TableStyleProperties() { Type = TableStyleOverrideValues.Band1Horizontal };
            TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties4 = new TableStyleConditionalFormattingTableProperties();

            TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties4 = new TableStyleConditionalFormattingTableCellProperties();

            TableCellBorders tableCellBorders3 = new TableCellBorders();
            InsideHorizontalBorder insideHorizontalBorder4 = new InsideHorizontalBorder() { Val = BorderValues.Nil };
            InsideVerticalBorder insideVerticalBorder3 = new InsideVerticalBorder() { Val = BorderValues.Nil };

            tableCellBorders3.Append(insideHorizontalBorder4);
            tableCellBorders3.Append(insideVerticalBorder3);
            Shading shading3 = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "D3DFEE", ThemeFill = ThemeColorValues.Accent1, ThemeFillTint = "3F" };

            tableStyleConditionalFormattingTableCellProperties4.Append(tableCellBorders3);
            tableStyleConditionalFormattingTableCellProperties4.Append(shading3);

            tableStyleProperties4.Append(tableStyleConditionalFormattingTableProperties4);
            tableStyleProperties4.Append(tableStyleConditionalFormattingTableCellProperties4);

            TableStyleProperties tableStyleProperties5 = new TableStyleProperties() { Type = TableStyleOverrideValues.Band2Horizontal };
            TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties5 = new TableStyleConditionalFormattingTableProperties();

            TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties5 = new TableStyleConditionalFormattingTableCellProperties();

            TableCellBorders tableCellBorders4 = new TableCellBorders();
            InsideHorizontalBorder insideHorizontalBorder5 = new InsideHorizontalBorder() { Val = BorderValues.Nil };
            InsideVerticalBorder insideVerticalBorder4 = new InsideVerticalBorder() { Val = BorderValues.Nil };

            tableCellBorders4.Append(insideHorizontalBorder5);
            tableCellBorders4.Append(insideVerticalBorder4);

            tableStyleConditionalFormattingTableCellProperties5.Append(tableCellBorders4);

            tableStyleProperties5.Append(tableStyleConditionalFormattingTableProperties5);
            tableStyleProperties5.Append(tableStyleConditionalFormattingTableCellProperties5);

            style.Append(styleName5);
            style.Append(basedOn1);
            style.Append(uIPriority4);
            style.Append(rsid1);
            style.Append(styleParagraphProperties1);
            style.Append(styleTableProperties2);
            style.Append(tableStyleProperties1);
            style.Append(tableStyleProperties2);
            style.Append(tableStyleProperties3);
            style.Append(tableStyleProperties4);           
            style.Append(tableStyleProperties5);

            return style;

        }

        /// <summary>
        /// Create Last Row Styles.
        /// </summary>      
        private static TableStyleProperties CreateLastRowStyles()
        {
            TableStyleProperties tableStyleProperties2 = new TableStyleProperties() { Type = TableStyleOverrideValues.LastRow };

            StyleParagraphProperties styleParagraphProperties3 = new StyleParagraphProperties();
            SpacingBetweenLines spacingBetweenLines4 = new SpacingBetweenLines() { Before = "0", After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };

            styleParagraphProperties3.Append(spacingBetweenLines4);

            RunPropertiesBaseStyle runPropertiesBaseStyle3 = new RunPropertiesBaseStyle();
            Bold bold2 = new Bold();
            BoldComplexScript boldComplexScript2 = new BoldComplexScript();

            runPropertiesBaseStyle3.Append(bold2);
            runPropertiesBaseStyle3.Append(boldComplexScript2);
            TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties2 = new TableStyleConditionalFormattingTableProperties();

            TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties2 = new TableStyleConditionalFormattingTableCellProperties();

            TableCellBorders tableCellBorders2 = new TableCellBorders();
            TopBorder topBorder3 = new TopBorder() { Val = BorderValues.Double, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)6U, Space = (UInt32Value)0U };
            LeftBorder leftBorder3 = new LeftBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            BottomBorder bottomBorder3 = new BottomBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            RightBorder rightBorder3 = new RightBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            InsideHorizontalBorder insideHorizontalBorder3 = new InsideHorizontalBorder() { Val = BorderValues.Nil };
            InsideVerticalBorder insideVerticalBorder2 = new InsideVerticalBorder() { Val = BorderValues.Nil };

            tableCellBorders2.Append(topBorder3);
            tableCellBorders2.Append(leftBorder3);
            tableCellBorders2.Append(bottomBorder3);
            tableCellBorders2.Append(rightBorder3);
            tableCellBorders2.Append(insideHorizontalBorder3);
            tableCellBorders2.Append(insideVerticalBorder2);

            tableStyleConditionalFormattingTableCellProperties2.Append(tableCellBorders2);

            tableStyleProperties2.Append(styleParagraphProperties3);
            tableStyleProperties2.Append(runPropertiesBaseStyle3);
            tableStyleProperties2.Append(tableStyleConditionalFormattingTableProperties2);
            tableStyleProperties2.Append(tableStyleConditionalFormattingTableCellProperties2);
            return tableStyleProperties2;
        }

        /// <summary>
        /// Create Header row styles.
        /// </summary>       
        private static TableStyleProperties CreateHeaderRowStyles()
        {
            TableStyleProperties tableStyleProperties1 = new TableStyleProperties() { Type = TableStyleOverrideValues.FirstRow };

            StyleParagraphProperties styleParagraphProperties2 = new StyleParagraphProperties();
            SpacingBetweenLines spacingBetweenLines3 = new SpacingBetweenLines() { Before = "0", After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };

            styleParagraphProperties2.Append(spacingBetweenLines3);

            RunPropertiesBaseStyle runPropertiesBaseStyle2 = new RunPropertiesBaseStyle();
            Bold bold1 = new Bold();
            BoldComplexScript boldComplexScript1 = new BoldComplexScript();
            Color color1 = new Color() { Val = "FFFFFF", ThemeColor = ThemeColorValues.Background1 };

            runPropertiesBaseStyle2.Append(bold1);
            runPropertiesBaseStyle2.Append(boldComplexScript1);
            runPropertiesBaseStyle2.Append(color1);
            TableStyleConditionalFormattingTableProperties tableStyleConditionalFormattingTableProperties1 = new TableStyleConditionalFormattingTableProperties();

            TableStyleConditionalFormattingTableRowProperties tableStyleConditionalFormattingTableRowProperties1 = new TableStyleConditionalFormattingTableRowProperties();
            TableHeader tableHeader1 = new TableHeader();

            tableStyleConditionalFormattingTableRowProperties1.Append(tableHeader1);

            TableStyleConditionalFormattingTableCellProperties tableStyleConditionalFormattingTableCellProperties1 = new TableStyleConditionalFormattingTableCellProperties();

            TableCellBorders tableCellBorders1 = new TableCellBorders();
            TopBorder topBorder2 = new TopBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            LeftBorder leftBorder2 = new LeftBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            BottomBorder bottomBorder2 = new BottomBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            RightBorder rightBorder2 = new RightBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            InsideHorizontalBorder insideHorizontalBorder2 = new InsideHorizontalBorder() { Val = BorderValues.Nil };
            InsideVerticalBorder insideVerticalBorder1 = new InsideVerticalBorder() { Val = BorderValues.Nil };

            tableCellBorders1.Append(topBorder2);
            tableCellBorders1.Append(leftBorder2);
            tableCellBorders1.Append(bottomBorder2);
            tableCellBorders1.Append(rightBorder2);
            tableCellBorders1.Append(insideHorizontalBorder2);
            tableCellBorders1.Append(insideVerticalBorder1);

            Shading shading1 = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "4F81BD", ThemeFill = ThemeColorValues.Accent1 };

            tableStyleConditionalFormattingTableCellProperties1.Append(tableCellBorders1);
            tableStyleConditionalFormattingTableCellProperties1.Append(shading1);

            tableStyleProperties1.Append(styleParagraphProperties2);
            tableStyleProperties1.Append(runPropertiesBaseStyle2);
            tableStyleProperties1.Append(tableStyleConditionalFormattingTableProperties1);
            tableStyleProperties1.Append(tableStyleConditionalFormattingTableRowProperties1);
            tableStyleProperties1.Append(tableStyleConditionalFormattingTableCellProperties1);
            return tableStyleProperties1;
        }

        /// <summary>
        /// Define table cell margins.
        /// </summary>        
        private static TableCellMarginDefault CreateTableCellMarginStyle()
        {
            TableCellMarginDefault tableCellMarginDefault2 = new TableCellMarginDefault();
            TopMargin topMargin2 = new TopMargin() { Width = "0", Type = TableWidthUnitValues.Dxa };
            TableCellLeftMargin tableCellLeftMargin2 = new TableCellLeftMargin() { Width = 108, Type = TableWidthValues.Dxa };
            BottomMargin bottomMargin2 = new BottomMargin() { Width = "0", Type = TableWidthUnitValues.Dxa };
            TableCellRightMargin tableCellRightMargin2 = new TableCellRightMargin() { Width = 108, Type = TableWidthValues.Dxa };

            tableCellMarginDefault2.Append(topMargin2);
            tableCellMarginDefault2.Append(tableCellLeftMargin2);
            tableCellMarginDefault2.Append(bottomMargin2);
            tableCellMarginDefault2.Append(tableCellRightMargin2);
            return tableCellMarginDefault2;
        }

        /// <summary>
        /// Create Table Border styles
        /// </summary>      
        private static TableBorders CreateTableBorderStyles()
        {
            TableBorders tableBorders1 = new TableBorders();
            TopBorder topBorder1 = new TopBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            LeftBorder leftBorder1 = new LeftBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            BottomBorder bottomBorder1 = new BottomBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            RightBorder rightBorder1 = new RightBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };
            InsideHorizontalBorder insideHorizontalBorder1 = new InsideHorizontalBorder() { Val = BorderValues.Single, Color = "7BA0CD", ThemeColor = ThemeColorValues.Accent1, ThemeTint = "BF", Size = (UInt32Value)8U, Space = (UInt32Value)0U };

            tableBorders1.Append(topBorder1);
            tableBorders1.Append(leftBorder1);
            tableBorders1.Append(bottomBorder1);
            tableBorders1.Append(rightBorder1);
            tableBorders1.Append(insideHorizontalBorder1);
            return tableBorders1;
        }

        private static Style GenerateReportHeadingStyle()
        {

            //set the run properties.
            RunProperties rPr = new RunProperties();
            Color color = new Color() { Val = "365F91", ThemeColor = ThemeColorValues.Accent1, ThemeShade = "BF" }; // the color is dark blue
            RunFonts rFont = new RunFonts();
            rFont.Ascii = "Cambria"; // the font is Cambria
            rPr.Append(color);
            rPr.Append(rFont);
            rPr.Append(new Bold()); // it is Bold
            rPr.Append(new FontSize() { Val = "28" }); 
            Style style = new Style();

            style.StyleId = "Heading1"; //this is the ID of the style
            style.Append(new Name() { Val = "Heading 1" }); //this is name
            // our style based on Normal style
            style.Append(new BasedOn() { Val = "Heading1" });
            // the next paragraph is Normal type
            style.Append(new NextParagraphStyle() { Val = "Normal" });
            style.Append(rPr);

            return style;

        }
    }
}
