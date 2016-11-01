// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using EDC.Database;
using ReadiNow.Reporting.Result;
using CellValue = DocumentFormat.OpenXml.Spreadsheet.CellValue;

namespace EDC.SoftwarePlatform.Services.ExportData
{
    /// <summary>
    /// Class to provide all the methods to export data to excel document.
    /// </summary>
    public class ExportToExcel
    {
        private static readonly DateTime OADateOrigin = new DateTime(1899, 12, 31);
        private static int sharedStringIndex;
        private static ConcurrentDictionary<int, string> sharedStrings;
        /// <summary>
        /// Generate Excel Document.
        /// </summary>
        /// <param name="reportResult">QueryResult</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream CreateExcelDocument(ReportResult reportResult, List<DataRow> rows)
        {
            sharedStringIndex = 0;
            sharedStrings = new ConcurrentDictionary<int, string>();
            var ms = new MemoryStream();

            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                // Create the Workbook
                WorkbookPart workbookPart = spreadSheet.AddWorkbookPart();
                spreadSheet.WorkbookPart.Workbook = new Workbook();

                // A Workbook must only have exactly one <Sheets> section
                spreadSheet.WorkbookPart.Workbook.AppendChild(new Sheets());

                WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>("rId1");
                newWorksheetPart.Worksheet = new Worksheet();


                //Add columns of constant width
                int columnIndx = 1;
                Columns cols = new Columns();
                foreach (ReportColumn column in reportResult.Metadata.ReportColumns.Values)
                {
                    if (!column.IsHidden && column.Type != "Image")
                    {
                        cols.Append(CreateColumnData(columnIndx));
                        columnIndx++;
                    }
                }
                newWorksheetPart.Worksheet.Append(cols);
                newWorksheetPart.Worksheet.Save();

                // Create a new Excel worksheet 
                SheetData sheetData = newWorksheetPart.Worksheet.AppendChild(new SheetData());

                // Create Styles and Insert into Workbook
                WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("rId3");
                Stylesheet styles = new ExportDataStylesheet();
                styles.Save(stylesPart);
              
                // Insert Datatable data into the worksheet.
                InsertTableData(reportResult, sheetData, rows);
                
                //Create table part.
                TableDefinitionPart tableDefinitionPart = newWorksheetPart.AddNewPart<TableDefinitionPart>("rId1");
                GenerateTablePartContent(tableDefinitionPart, reportResult,rows, columnIndx-2);
                TableParts tableParts1 = new TableParts() { Count = (UInt32Value)1U };
                TablePart tablePart1 = new TablePart() { Id = "rId1" };
                tableParts1.Append(tablePart1);
                newWorksheetPart.Worksheet.Append(tableParts1);
                newWorksheetPart.Worksheet.Save();

                //add shared stringd
                SharedStringTablePart sharedStringTablePart = spreadSheet.WorkbookPart.AddNewPart<SharedStringTablePart>("rId2");
                SharedStringTable sharedStringTable = new SharedStringTable();
                foreach (string sharedString in sharedStrings.Values)
                {
                    CreateSharedStringItem(sharedStringTable, sharedString);
                }
                sharedStringTablePart.SharedStringTable = sharedStringTable;
                // Save the worksheet.
                newWorksheetPart.Worksheet.Save();

                // Link this worksheet to our workbook
                spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
                {
                    Id = spreadSheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
                    SheetId = 1,
                    Name = "Table"
                });

                // Save the workbook.
                spreadSheet.WorkbookPart.Workbook.Save();
            }
            return ms;
        }
     
        private static void GenerateTablePartContent(TableDefinitionPart part, ReportResult reportResult,List<DataRow> rows,int columnCount)
        {
            int rowCount = 2;
            if (rows.Count() > 0)
                rowCount = rows.Count() + 1;
           // int columnCount = reportResult.Metadata.ReportColumns.Values.Count()-1;
            string reference = "A1:" + GetExcelColumnName(columnCount) + rowCount.ToString(); 
            Table table1 = new Table() { Id = (UInt32Value)1U, Name = "Table1", DisplayName = "Table1", Reference = reference, TotalsRowShown = false };
            AutoFilter autoFilter1 = new AutoFilter() { Reference = reference };
            TableColumns tableColumns1 = new TableColumns() { Count = Convert.ToUInt32(columnCount+1) };
            int i = 1;
            foreach (ReportColumn column in reportResult.Metadata.ReportColumns.Values)
            {
                if (!column.IsHidden && column.Type != "Image")
                {
                    tableColumns1.Append(new TableColumn() {Id = Convert.ToUInt32(i++), Name = column.Title.Trim()});
                }
            }
            TableStyleInfo tableStyleInfo1 = new TableStyleInfo() { Name = "TableStyleMedium2", ShowFirstColumn = false, ShowLastColumn = false, ShowRowStripes = true, ShowColumnStripes = false };
            table1.Append(autoFilter1);
            table1.Append(tableColumns1);
            table1.Append(tableStyleInfo1);
            table1.HeaderRowCount = 1;
            part.Table = table1;
        }


        /// <summary>
        /// Insert date from datatable to worksheet.
        /// </summary>     
        private static void InsertTableData(ReportResult reportResult, SheetData sheetData, List<DataRow> rows)
        {
            // Add column names to the first row
            Row header = new Row();
            header.RowIndex = (UInt32)1;
            int headerColInx = 0;
            foreach (ReportColumn column in reportResult.Metadata.ReportColumns.Values)
            {
                if (!column.IsHidden && column.Type != "Image")
                {
                    Cell headerCell = CreateTextCell(null);
                    SetCellLocation(headerCell, headerColInx, 1);
                    header.AppendChild(headerCell);
                    sharedStrings.TryAdd(sharedStringIndex, column.Title.Trim());
                    headerColInx++;
                }
            }
            sheetData.AppendChild(header);

            // Loop through each data row
            int excelRowIndex = 2;
            foreach (DataRow row in rows)
            {
                Row excelRow = CreateContentRow(reportResult, row, excelRowIndex++);
                sheetData.AppendChild(excelRow);
            }
        }

        /// <summary>
        /// Create Excel columns.
        /// </summary>      
        private static Column CreateColumnData(int index)
        {
            Column column = new Column();
            column.Min = (uint)index;
            column.Max = (uint)index;
            column.Width = 20;
            column.CustomWidth = true;
            return column;
        }


        /// <summary>
        /// Create content row of the datatable.
        /// </summary>      
        private static Row CreateContentRow(ReportResult reportResults, DataRow contentRow, int rowIndex)
        {
            Row row = new Row
            {
                RowIndex = (UInt32)rowIndex
            };

            int columnIndex = 0;
            int dataColumnIndex = 0;

            foreach (ReportColumn col in reportResults.Metadata.ReportColumns.Values)
            {
                if (!col.IsHidden && col.Type != "Image")
                {
                    Cell cell;
                    string cellValue = ExportDataHelper.GetCellValue(contentRow, dataColumnIndex);
                    DatabaseType cellType= DatabaseTypeHelper.ConvertFromDisplayName(col.Type);

                    if (!string.IsNullOrEmpty(col.AutoNumberDisplayPattern))
                    {
                        cellType = DatabaseType.AutoIncrementType;
                        col.Type = "AutoIncrement";
                    }
                    object value = DatabaseTypeHelper.ConvertFromString(cellType, cellValue);
                    if (string.IsNullOrEmpty(cellValue))
                    {
                        cell = CreateTextCell(null);
                        sharedStrings.TryAdd(sharedStringIndex, null);
                    }
                    else
                    {
                        DateTime dateValue;
                        switch (col.Type)
                        {
                            case "DateTime":
                                dateValue = Convert.ToDateTime(value);
                                cell = CreateDateValueCell(dateValue, 4);
                                break;
                            case "Date":
                                dateValue = Convert.ToDateTime(value);
                                cell = CreateDateValueCell(dateValue, 1);
                                break;
                            case "Time":
                                dateValue = OADateOrigin + (Convert.ToDateTime(value)).ToUniversalTime().TimeOfDay;
                                cell = CreateDateValueCell(dateValue, 10);
                                break;
                            case "Int32":
                                cell = CreateNumericCell(value, 5);
                                break;
                            case "Decimal":
                                cell = CreateNumericCell(value, 6);
                                break;
                            case "Currency":
                                cell = CreateNumericCell(value, 9);
                                break;
                            case "AutoIncrement":
                                cellValue = ExportDataHelper.GetFormattedCellValue(cellType, cellValue, col);
                                cell = CreateTextCell(null);
                                sharedStrings.TryAdd(sharedStringIndex, cellValue);
                                break;
                            case "StructureLevels":
                                cellValue = ExportDataHelper.GetStructureLevelCellValue(cellValue, true);
                                cell = CreateTextCell(8);
                                sharedStrings.TryAdd(sharedStringIndex, cellValue);
                                break;
                            default:
                                cellValue = ExportDataHelper.GetFormattedCellValue(cellType, cellValue, col);
                                cell = CreateTextCell(8);
                                sharedStrings.TryAdd(sharedStringIndex, cellValue);
                                break;
                        }
                    }

                    // Append the cell
                    SetCellLocation(cell, columnIndex, rowIndex);
                    row.AppendChild(cell);
                    columnIndex++;
                }
                dataColumnIndex++;
            }
            return row;
        }


        /// <summary>
        /// Creates a new Cell object with the InlineString data type.
        /// </summary>
        private static Cell CreateTextCell(uint? styleIndex)
        {
            Cell cell = new Cell();

            cell.DataType = CellValues.SharedString;

            //apply the cell style if supplied
            if (styleIndex.HasValue)
                cell.StyleIndex = styleIndex.Value;

            CellValue cellValue1 = new CellValue();
            cellValue1.Text = sharedStringIndex.ToString();
            sharedStringIndex += 1;
            cell.AppendChild(cellValue1);
            return cell;
        }

        /// <summary>
        /// Creates a new Cell object for date.
        /// </summary>
        private static Cell CreateDateValueCell(
            DateTime cellValue,
            uint? styleIndex)
        {
            Cell cell = new Cell();
            CellValue value = new CellValue();
            if (cellValue != null)
                value.Text = cellValue.ToOADate().ToString();

            //apply the cell style if supplied
            if (styleIndex.HasValue)
                cell.StyleIndex = styleIndex.Value;

            cell.AppendChild(value);

            return cell;
        }
        /// <summary>
        /// Creates a new Cell object.
        /// </summary>
        private static Cell CreateNumericCell(
            object cellValue,
            uint? styleIndex)
        {
            Cell cell = new Cell();
            CellValue value = new CellValue();
            if (cellValue != null)
                value.Text = cellValue.ToString();

            //apply the cell style if supplied
            if (styleIndex.HasValue)
                cell.StyleIndex = styleIndex.Value;

            cell.AppendChild(value);

            return cell;
        }

        /// <summary>
        /// Assigns a cell to a location.
        /// </summary>
        /// <param name="cell">The cell to update.</param>
        /// <param name="columnIndex">One-based column.</param>
        /// <param name="rowIndex">One-based row.</param>
        private static void SetCellLocation(Cell cell, int columnIndex, int rowIndex)
        {
            cell.CellReference = GetExcelColumnName(columnIndex) + rowIndex;
        }


        /// <summary>
        /// Get Excel column name.
        /// </summary>
        /// <param name="columnIndex">Column Index</param>
        /// <returns>1 = A, 2 = B... 27 = AA, etc.</returns>
        private static string GetExcelColumnName(int columnIndex)
        {
            //  Convert a zero-based column index into an Excel column reference (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
            //  Each Excel cell we write must have the cell name stored with it.            
            int dividend = columnIndex + 1;
            string columnName = String.Empty;
            int modifier;

            while (dividend > 0)
            {
                modifier = (dividend - 1) % 26;
                columnName =
                    Convert.ToChar(65 + modifier).ToString() + columnName;
                dividend = (int)((dividend - modifier) / 26);
            }
            return columnName;
        }

        private static void CreateSharedStringItem(SharedStringTable sharedTable, string text)
        {
            SharedStringItem sharedStringItem = new SharedStringItem();
            DocumentFormat.OpenXml.Spreadsheet.Text text1 = new DocumentFormat.OpenXml.Spreadsheet.Text();
            text1.Text = text;
            sharedStringItem.Append(text1);
            sharedTable.Append(sharedStringItem);
        }


    }
}
