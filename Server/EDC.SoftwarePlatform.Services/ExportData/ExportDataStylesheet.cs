// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace EDC.SoftwarePlatform.Services.ExportData
{
    /// <summary>
    /// Creates a styles for the excel sheet.
    /// </summary>
    class ExportDataStylesheet : Stylesheet
    {
        public ExportDataStylesheet()
        {
            //Create Font styles
            Fonts fts = new Fonts();
            CreateFontStyles(fts);

            //Create fill styles
            Fills fills = new Fills();
            CreateFillStyles(fills);

            //Create border styles
            Borders borders = new Borders();
            CreateBorderStyles(borders);

            CellStyleFormats csfs = new CellStyleFormats();
            CellFormat cf = new CellFormat();
            cf.NumberFormatId = 0;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            csfs.Append(cf);
            csfs.Count = UInt32Value.FromUInt32((uint)csfs.ChildElements.Count);

            uint iExcelIndex = 164;
            NumberingFormats nfs = new NumberingFormats();
            CellFormats cfs = new CellFormats();

            cf = new CellFormat();
            cf.NumberFormatId = 0;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.FormatId = 0;
            cfs.Append(cf);

            //Create DateTime Format
            NumberingFormat nfDateTime = CreateNumberFormat(ref iExcelIndex, nfs, "d/mm/yyyy h:mm AM/PM");

            NumberingFormat nfDate = CreateNumberFormat(ref iExcelIndex, nfs, "d/mm/yyyy");


            NumberingFormat nfTime = CreateNumberFormat(ref iExcelIndex, nfs, "h:mm AM/PM");


            NumberingFormat nfinteger = CreateNumberFormat(ref iExcelIndex, nfs, "#,##0");


            // #,##0.00 is also Excel style index 4
            NumberingFormat nf3Decimal = CreateNumberFormat(ref iExcelIndex, nfs, "#,##0.000");

            // @ is also Excel style index 49
            NumberingFormat nfForcedText = CreateNumberFormat(ref iExcelIndex, nfs, "@");

            NumberingFormat nfcurrency = CreateNumberFormat(ref iExcelIndex, nfs, "$#,##0.00");


          //  // index 1
            // Format dd/mm/yyyy
            CreateCellFormat(cfs, 14);

            // index 2
            // Format h:mm:ss AM/PM
            CreateCellFormat(cfs, 19);

            // index 3
            // Format #,##0.00
            CreateCellFormat(cfs, 4);

            // index 4
            CreateCellFormat(cfs, nfDateTime.NumberFormatId);

            // index 5
            CreateCellFormat(cfs, nfinteger.NumberFormatId);

            // index 6
            CreateCellFormat(cfs, nf3Decimal.NumberFormatId);

            // index 7
            CreateCellFormat(cfs, nfForcedText.NumberFormatId);

            // index 8
            // wrap text style
            CreateWrapTextCellFormat(cfs);

            // index 9
            CreateCellFormat(cfs, nfcurrency.NumberFormatId);

            // index 10
            CreateCellFormat(cfs, nfTime.NumberFormatId);
            


            nfs.Count = UInt32Value.FromUInt32((uint)nfs.ChildElements.Count);
            cfs.Count = UInt32Value.FromUInt32((uint)cfs.ChildElements.Count);

            this.Append(nfs);
            this.Append(fts);
            this.Append(fills);
            this.Append(borders);
            this.Append(csfs);
            this.Append(cfs);

            CellStyles css = new CellStyles();
            CellStyle cs = new CellStyle();
            cs.Name = StringValue.FromString("Normal");
            cs.FormatId = 0;
            cs.BuiltinId = 0;
            css.Append(cs);
            css.Count = UInt32Value.FromUInt32((uint)css.ChildElements.Count);
            this.Append(css);

            DifferentialFormats dfs = new DifferentialFormats();
            dfs.Count = 0;
            this.Append(dfs);

            TableStyles tss = new TableStyles();
            tss.Count = 0;
            tss.DefaultTableStyle = StringValue.FromString("TableStyleMedium9");
            tss.DefaultPivotStyle = StringValue.FromString("PivotStyleLight16");
            this.Append(tss);
        }

        /// <summary>
        /// Create wrap text cell format.
        /// </summary>       
        private void CreateWrapTextCellFormat(CellFormats cfs)
        {
            CellFormat cf = new CellFormat();
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.FormatId = 0;
            cf.ApplyAlignment = true;
            Alignment algn = new Alignment() { WrapText = true };
            cf.Append(algn);
            cfs.Append(cf);
        }

        /// <summary>
        /// Create cell format and add the cellformats collection
        /// </summary>        
        private static CellFormat CreateCellFormat(CellFormats cfs, UInt32Value NumberFormatId)
        {
            CellFormat cf = new CellFormat();
            cf.NumberFormatId = NumberFormatId;
            cf.FontId = 0;
            cf.FillId = 0;
            cf.BorderId = 0;
            cf.FormatId = 0;
            cf.ApplyNumberFormat = BooleanValue.FromBoolean(true);
            cfs.Append(cf);
            return cf;
        }

        /// <summary>
        /// Create Numbering format
        /// </summary>     
        private static NumberingFormat CreateNumberFormat(ref uint iExcelIndex, NumberingFormats nfs, string formatString)
        {
            NumberingFormat nfDateTime = new NumberingFormat();
            nfDateTime.NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++);
            nfDateTime.FormatCode = StringValue.FromString(formatString);
            nfs.Append(nfDateTime);
            return nfDateTime;
        }

        /// <summary>
        /// Create border styles
        /// </summary>        
        private static void CreateBorderStyles(Borders borders)
        {
            Border border = new Border();
            border.LeftBorder = new LeftBorder();
            border.RightBorder = new RightBorder();
            border.TopBorder = new TopBorder();
            border.BottomBorder = new BottomBorder();
            border.DiagonalBorder = new DiagonalBorder();
            borders.Append(border);

            //Border Index 1
            border = new Border();
            border.LeftBorder = new LeftBorder();
            border.LeftBorder.Style = BorderStyleValues.Thin;
            border.RightBorder = new RightBorder();
            border.RightBorder.Style = BorderStyleValues.Thin;
            border.TopBorder = new TopBorder();
            border.TopBorder.Style = BorderStyleValues.Thin;
            border.BottomBorder = new BottomBorder();
            border.BottomBorder.Style = BorderStyleValues.Thin;
            border.DiagonalBorder = new DiagonalBorder();
            borders.Append(border);


            //Border Index 2
            border = new Border();
            border.LeftBorder = new LeftBorder();
            border.RightBorder = new RightBorder();
            border.TopBorder = new TopBorder();
            border.TopBorder.Style = BorderStyleValues.Thin;
            border.BottomBorder = new BottomBorder();
            border.BottomBorder.Style = BorderStyleValues.Thin;
            border.DiagonalBorder = new DiagonalBorder();
            borders.Append(border);


            borders.Count = UInt32Value.FromUInt32((uint)borders.ChildElements.Count);
        }

        /// <summary>
        /// Create Fill styles
        /// </summary>       
        private static void CreateFillStyles(Fills fills)
        {
            Fill fill = new Fill();
            PatternFill patternFill = new PatternFill();
            patternFill.PatternType = PatternValues.None;
            fill.PatternFill = patternFill;
            fills.Append(fill);

            //Fill index 1
            fill = new Fill();
            patternFill = new PatternFill();
            patternFill.PatternType = PatternValues.Gray125;
            fill.PatternFill = patternFill;
            fills.Append(fill);

            //Fill index 2
            fill = new Fill();
            patternFill = new PatternFill();
            patternFill.PatternType = PatternValues.Solid;
            patternFill.ForegroundColor = new ForegroundColor();
            patternFill.ForegroundColor.Rgb = HexBinaryValue.FromString("00ff9728");
            patternFill.BackgroundColor = new BackgroundColor();
            patternFill.BackgroundColor.Rgb = patternFill.ForegroundColor.Rgb;
            fill.PatternFill = patternFill;
            fills.Append(fill);

            fills.Count = UInt32Value.FromUInt32((uint)fills.ChildElements.Count);
        }

        /// <summary>
        /// Create Font styles
        /// </summary>        
        private static void CreateFontStyles(Fonts fts)
        {
            Font ft = new Font();
            FontName ftn = new FontName();
            ftn.Val = StringValue.FromString("Calibri");
            FontSize ftsz = new FontSize();
            ftsz.Val = DoubleValue.FromDouble(11);
            ft.FontName = ftn;
            ft.FontSize = ftsz;
            fts.Append(ft);

            ft = new DocumentFormat.OpenXml.Spreadsheet.Font();
            ftn = new FontName();
            ftn.Val = StringValue.FromString("Palatino Linotype");
            ftsz = new FontSize();
            ftsz.Val = DoubleValue.FromDouble(18);
            ft.FontName = ftn;
            ft.FontSize = ftsz;
            fts.Append(ft);

            fts.Count = UInt32Value.FromUInt32((uint)fts.ChildElements.Count);
        }
    }
}
