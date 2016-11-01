// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Metadata.Query.Structured;
using AggregateExpression = EDC.ReadiNow.Metadata.Query.Structured.AggregateExpression;
using CellValue = DocumentFormat.OpenXml.Spreadsheet.CellValue;
using Color = System.Drawing.Color;
using DataRow = System.Data.DataRow;
using ReportColumn = ReadiNow.Reporting.Result.ReportColumn;
using Result = ReadiNow.Reporting.Result;

namespace EDC.SoftwarePlatform.Services.ExportData
{
	/// <summary>
	///     Class to provide all the methods to export data to Excel and csv file.
	/// </summary>
	public class ExportDataHelper
	{
		private static readonly DateTime OaDateOrigin = new DateTime( 1899, 12, 31 );		

		/// <summary>
		///     Creates the auto increment cell.
		/// </summary>
		/// <param name="cellValue">The cell value.</param>
		/// <param name="styleIndex">Index of the style.</param>
		/// <param name="displayFormat">The display format.</param>
		/// <returns></returns>
		private static Cell CreateAutoIncrementCell( object cellValue, uint? styleIndex, string displayFormat )
		{
			if ( cellValue == null || cellValue == DBNull.Value )
			{
				return CreateTextCell( cellValue, styleIndex );
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

			if ( string.IsNullOrEmpty( displayFormat ) )
			{
				return CreateNumericCell( temp, styleIndex );
			}

			return CreateTextCell( temp.ToString( displayFormat ), styleIndex );
		}

		/// <summary>
		///     Create Excel columns.
		/// </summary>
		private static Column CreateColumnData( int index, double columnWidth )
		{
			var column = new Column
			{
				Min = ( uint ) index,
				Max = ( uint ) index,
				Width = columnWidth,
				CustomWidth = true
			};
			return column;
		}

		/// <summary>
		///     Create content row of the datatable.
		/// </summary>
		private static Row CreateContentRow( QueryResult queryResults, DataRow contentRow, int rowIndex )
		{
			var row = new Row
			{
				RowIndex = ( UInt32 ) rowIndex
			};

			int columnIndex = 0;
			int dataColumnIndex = 0;

			foreach ( ResultColumn col in queryResults.Columns )
			{
				if ( !col.IsHidden )
				{
					Cell cell = null;
					object cellValue = contentRow[ dataColumnIndex ];
					string cellType = GetCellTypeFromDatabaseType( col.ColumnType );
					DateTime dateValue;

					if ( cellValue == null || cellValue is DBNull )
					{
						cellType = "Null";
					}

					switch ( cellType )
					{
						case "String":
							cell = CreateTextCell( cellValue, 8, col.RequestColumn.Expression is AggregateExpression );
							break;

						case "Bool":
							if ( cellValue != null )
							{
								string boolValue = GetBooleanCellValue( ( bool ) cellValue );
								cell = CreateTextCell( boolValue, null );
							}

							break;

						case "DateTime":
							if ( cellValue != null )
							{
								dateValue = ( DateTime ) cellValue;
								cell = CreateDateValueCell( dateValue, 4 );
							}

							break;

						case "Date":
							if ( cellValue != null )
							{
								dateValue = ( DateTime ) cellValue;
								cell = CreateDateValueCell( dateValue, 1 );
							}

							break;

						case "Time":
							if ( cellValue != null )
							{
								dateValue = OaDateOrigin + ( ( DateTime ) cellValue ).TimeOfDay;
								cell = CreateDateValueCell( dateValue, 2 );
							}

							break;

						case "AutoIncrement":
							string displayPattern = null;

							if ( contentRow.Table.Columns[ dataColumnIndex ].ExtendedProperties.ContainsKey( "DisplayPattern" ) )
							{
								displayPattern = ( string ) contentRow.Table.Columns[ dataColumnIndex ].ExtendedProperties[ "DisplayPattern" ];
							}

							cell = CreateAutoIncrementCell( cellValue, string.IsNullOrEmpty( displayPattern ) ? 5 : ( uint? ) 8, displayPattern );
							break;
						case "Number":
							cell = CreateNumericCell( cellValue, 5 );
							break;

						case "Decimal":
							cell = CreateNumericCell( cellValue, 6 );
							break;

						case "StructureLevels":
							cellValue = GetStructureLevelCellValue( ( string ) cellValue, true );
							cell = CreateTextCell( cellValue, 8 );
							break;

						case "ChoiceRelationship":
						case "InlineRelationship":
							cellValue = GetResourceCellValue( ( string ) cellValue );
							cell = CreateTextCell( cellValue, 8 );
							break;

						default:
							cell = new Cell( );
							break;
					}

					// Append the cell
					SetCellLocation( cell, columnIndex, rowIndex );
					row.AppendChild( cell );
					columnIndex++;
				}
				dataColumnIndex++;
			}
			return row;
		}

		/// <summary>
		///     Creates a new Cell object for date.
		/// </summary>
		private static Cell CreateDateValueCell(
			DateTime cellValue,
			uint? styleIndex )
		{
			var cell = new Cell( );
			var value = new CellValue
			{
				Text = cellValue.ToOADate( ).ToString( CultureInfo.InvariantCulture )
			};

			//apply the cell style if supplied
			if ( styleIndex.HasValue )
				cell.StyleIndex = styleIndex.Value;

			cell.AppendChild( value );

			return cell;
		}

		/// <summary>
		///     Generate Excel Document.
		/// </summary>
		/// <param name="queryResult">QueryResult</param>
		/// <returns>MemoryStream</returns>
		public static MemoryStream CreateExcelDocument( QueryResult queryResult )
		{
			var ms = new MemoryStream( );

			using ( SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create( ms, SpreadsheetDocumentType.Workbook, true ) )
			{
				// Create the Workbook
				WorkbookPart workbookPart = spreadSheet.AddWorkbookPart( );
				spreadSheet.WorkbookPart.Workbook = new Workbook( );

				// A Workbook must only have exactly one <Sheets> section
				spreadSheet.WorkbookPart.Workbook.AppendChild( new Sheets( ) );

				var newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>( );
				newWorksheetPart.Worksheet = new Worksheet( );

				// Create a new Excel worksheet 
				SheetData sheetData = newWorksheetPart.Worksheet.AppendChild( new SheetData( ) );

				var tdp = newWorksheetPart.AddNewPart<TableDefinitionPart>( );

				string rId = newWorksheetPart.GetIdOfPart( tdp );
				var T = new Table
				{
					Id = 1U,
					Name = "MyTable",
					DisplayName = "MyTable",
					Reference = "A1:B10",
					TotalsRowShown = false
				};
				var columns = new TableColumns
				{
					Count = 2U
				};
				var column1 = new TableColumn
				{
					Id = 1U,
					Name = "Column1"
				};
				var column2 = new TableColumn
				{
					Id = 2U,
					Name = "Column2"
				};
				var styleInfo = new TableStyleInfo
				{
					Name = "TableStyleMedium2",
					ShowFirstColumn = false,
					ShowLastColumn = false,
					ShowRowStripes = true,
					ShowColumnStripes = false
				};
				var autoFilter = new AutoFilter
				{
					Reference = "A1:B10"
				};
				columns.Append( column1 );
				columns.Append( column2 );
				T.Append( autoFilter );
				T.Append( columns );
				T.Append( styleInfo );
				tdp.Table = T;
				T.Save( );

				var tableParts = new TableParts
				{
					Count = 1U
				};
				var tablePart = new TablePart
				{
					Id = rId
				};
				tableParts.Append( tablePart );
				newWorksheetPart.Worksheet.Append( tableParts );

				newWorksheetPart.Worksheet.Save( );


				// Create Styles and Insert into Workbook
				var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>( );
				Stylesheet styles = new ExportDataStylesheet( );
				styles.Save( stylesPart );

				// Insert Datatable data into the worksheet.
				InsertTableData( queryResult, sheetData, stylesPart );

				// Save the worksheet.
				newWorksheetPart.Worksheet.Save( );

				// Link this worksheet to our workbook
				spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>( ).AppendChild( new Sheet
				{
					Id = spreadSheet.WorkbookPart.GetIdOfPart( newWorksheetPart ),
					SheetId = 1,
					Name = "Table"
				} );

				// Save the workbook.
				spreadSheet.WorkbookPart.Workbook.Save( );
			}
			return ms;
		}


		/// <summary>
		///     Create header styles and append to the workbook stylesheet.
		/// </summary>
		private static UInt32Value CreateHeaderStyles( Stylesheet styleSheet )
		{
			//build the formatted header style
			UInt32Value headerFontIndex =
				ExportDataStyle.CreateFont(
					styleSheet,
					"Calibri",
					12,
					true,
					Color.Black );
			//set the background color style
			UInt32Value headerFillIndex =
				ExportDataStyle.CreateFill(
					styleSheet,
					ColorTranslator.FromHtml( "#F2EEEE" ) );
			// System.Drawing.Color.LightBlue);
			//create the cell style by combining font/background
			UInt32Value headerStyleIndex =
				ExportDataStyle.CreateCellFormat(
					styleSheet,
					headerFontIndex,
					headerFillIndex,
					null );
			return headerStyleIndex;
		}


		/// <summary>
		///     Creates a new Cell object.
		/// </summary>
		private static Cell CreateNumericCell(
			object cellValue,
			uint? styleIndex )
		{
			var cell = new Cell
			{
				DataType = CellValues.Number
			};
			var value = new CellValue( );
			if ( cellValue != null )
				value.Text = cellValue.ToString( );

			//apply the cell style if supplied
			if ( styleIndex.HasValue )
				cell.StyleIndex = styleIndex.Value;

			cell.AppendChild( value );

			return cell;
		}

		/// <summary>
		///     Creates a new Cell object with the InlineString data type.
		/// </summary>
		private static Cell CreateTextCell(
			object cellValue,
			uint? styleIndex, bool isRelationshipValue = false )
		{
			var cell = new Cell
			{
				DataType = CellValues.InlineString
			};

			//apply the cell style if supplied
			if ( styleIndex.HasValue )
				cell.StyleIndex = styleIndex.Value;

			var inlineString = new InlineString( );
			var t = new DocumentFormat.OpenXml.Spreadsheet.Text( );
			if ( cellValue != null )
				t.Text = isRelationshipValue ? DatabaseTypeHelper.GetEntityXmlName( cellValue.ToString( ) ) : cellValue.ToString( );

			inlineString.AppendChild( t );
			cell.AppendChild( inlineString );

			return cell;
		}

		/// <summary>
		///     Export Data to Excel document.
		/// </summary>
		/// <param name="queryResult">The query data.</param>
		/// <returns>ExportDataInfo</returns>
		public static ExportDataInfo ExportToExcelDocument( QueryResult queryResult )
		{
			var ms = CreateExcelDocument( queryResult );
			// Convert the memory stream to the byte array.
			var bytesInStream = new byte[ms.Length];
			ms.Position = 0;
			ms.Read( bytesInStream, 0, bytesInStream.Length );

			var export = new ExportDataInfo
			{
				FileStream = bytesInStream
			};
			ms.Close( );
			return export;
		}

		/// <summary>
		///     Get Boolean Cell Value
		/// </summary>
		public static string GetBooleanCellValue( bool cellValue )
		{
			return cellValue ? "Yes" : "No";
		}

		/// <summary>
		///     Get cell value type from database type.
		/// </summary>
		public static string GetCellTypeFromDatabaseType( DatabaseType databaseType )
		{
			if ( databaseType is BinaryType )
			{
				return "Binary";
			}
			if ( databaseType is BoolType )
			{
				return "Bool";
			}
			if ( databaseType is DateTimeType )
			{
				return "DateTime";
			}
			if ( databaseType is TimeType )
			{
				return "Time";
			}
			if ( databaseType is DateType )
			{
				return "Date";
			}
			if ( databaseType is AutoIncrementType )
			{
				return "AutoIncrement";
			}
			if ( databaseType is Int32Type )
			{
				return "Number";
			}
			if ( databaseType is CurrencyType || databaseType is DecimalType )
			{
				return "Decimal";
			}
			if ( databaseType is StringType || databaseType is UnknownType || databaseType is XmlType || databaseType is GuidType )
			{
				return "String";
			}
			if ( databaseType is StructureLevelsType )
			{
				return "StructureLevels";
			}
			if ( databaseType is ChoiceRelationshipType )
			{
				return "ChoiceRelationship";
			}
			if ( databaseType is InlineRelationshipType )
			{
				return "InlineRelationship";
			}
			throw new InvalidOperationException( "The specified database type cannot be converted." );
		}

		public static string GetCellValue( Result.DataRow contentRow, int dataColumnIndex )
		{
			string cellValue = "";
			cellValue = contentRow.Values[ dataColumnIndex ].Value;
			if ( string.IsNullOrEmpty( cellValue ) )
			{
				if ( contentRow.Values [ dataColumnIndex ].Values != null && contentRow.Values [ dataColumnIndex ].Values.Count > 0 )
				{
					cellValue = string.Join( ",", contentRow.Values[ dataColumnIndex ].Values.Values );
				}
			}

			return cellValue;
		}

		/// <summary>
		///     Get the column width in pixels by column name.
		/// </summary>
		/// <param name="columnName"></param>
		private static double GetColumnWidth( string columnName )
		{
			const double fMaxDigitWidth = 10.0f;
			double fTruncWidth = Math.Truncate( ( columnName.ToCharArray( ).Count( ) * fMaxDigitWidth + 5.0 ) / fMaxDigitWidth * 256.0 ) / 256.0;
			return fTruncWidth;
		}


		/// <summary>
		///     Get Excel column name.
		/// </summary>
		/// <param name="columnIndex">Column Index</param>
		/// <returns>1 = A, 2 = B... 27 = AA, etc.</returns>
		private static string GetExcelColumnName( int columnIndex )
		{
			//  Convert a zero-based column index into an Excel column reference (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
			//  Each Excel cell we write must have the cell name stored with it.            
			int dividend = columnIndex + 1;
			string columnName = String.Empty;

			while ( dividend > 0 )
			{
				int modifier = ( dividend - 1 ) % 26;
				columnName =
					Convert.ToChar( 65 + modifier ) + columnName;
				dividend = ( dividend - modifier ) / 26;
			}
			return columnName;
		}

		/// <summary>
		///     Get the formatted cell value for the different data types.
		/// </summary>
		public static string GetFormattedCellValue( DatabaseType type, string cellValue, ReportColumn col )
		{
			string result;

			//convert cell value to its particular type
			object value = DatabaseTypeHelper.ConvertFromString( type, cellValue );
			if ( cellValue == null )
			{
				return "";
			}
			if ( type is BoolType )
			{
				result = GetBooleanCellValue( ( bool ) value );
				return result;
			}
			if ( type is AutoIncrementType )
			{
				result = DatabaseTypeHelper.ConvertToString( type, value, col.AutoNumberDisplayPattern );
				return result;
			}
			if ( type is DateType )
			{
				return ( ( DateTime ) value ).ToString( "d/MM/yyyy" );
			}
			if ( type is TimeType )
			{
				return ( ( DateTime ) value ).ToUniversalTime( ).ToString( "h:mm tt" );
			}
			if ( type is DateTimeType )
			{
				return ( ( DateTime ) value ).ToString( "d/MM/yyyy h:mm tt" );
			}
			if ( type is Int32Type )
			{
				return Convert.ToInt32( value ).ToString( "#,##0" );
			}
			if ( type is DecimalType )
			{
				long? decimalPlaces = col.DecimalPlaces;
				NumberFormatInfo numberFormatInfo = new CultureInfo( "en-US", false ).NumberFormat;
				numberFormatInfo.NumberDecimalDigits = decimalPlaces != null ? Convert.ToInt32( decimalPlaces ) : 3;
				decimal temp = Convert.ToDecimal( value );
				return temp.ToString( "N", numberFormatInfo );
			}
			if ( type is CurrencyType )
			{
				long? decimalPlaces = col.DecimalPlaces;
				NumberFormatInfo numberFormatInfo = new CultureInfo( "en-US", false ).NumberFormat;
				numberFormatInfo.CurrencyDecimalDigits = decimalPlaces != null ? Convert.ToInt32( decimalPlaces ) : 2;
				numberFormatInfo.CurrencySymbol = "$";
				numberFormatInfo.CurrencyNegativePattern = 1;
				decimal temp = Convert.ToDecimal( value );
				return temp.ToString( "C", numberFormatInfo );
			}
		    if (type is StructureLevelsType)
		    {
		        return GetStructureLevelCellValue(cellValue, true);
		    }

			result = value.ToString( );
			return result;
		}

		/// <summary>
		///     Get Boolean Cell Value
		/// </summary>
		public static string GetResourceCellValue( string cellValue )
		{
			string result = DatabaseTypeHelper.GetEntityXmlName( cellValue );
			return result;
		}

		/// <summary>
		///     Get structure path from the structure path encoded string.
		/// </summary>
		/// <param name="cellValue">The cell value.</param>
		/// <param name="createNewLineForEachLevel">if set to <c>true</c> create a new line for each level.</param>
		/// <returns></returns>
		public static string GetStructureLevelCellValue( string cellValue, bool createNewLineForEachLevel )
		{
		    if (string.IsNullOrWhiteSpace(cellValue))
		    {
		        return string.Empty;
		    }

			// Get the distinct list of paths	
            IEnumerable<string> paths = cellValue.Split('\u0003').Distinct().ToList();
            List<string> distinctPaths = (from path in paths let filteredPaths = paths.Where(p => p.IndexOf(path) >= 0) where filteredPaths.Count() == 1 select path).ToList();
            var resultPaths = new HashSet<string>();            

            foreach (string path in distinctPaths)
			{
				string levelPath;

                // Get the levels names from the path
				List<string> nodeNames = path.Split('\u0002').Select(node =>
                {
                    int colonIndex = node.IndexOf(":");

                    return colonIndex <= 0 ? node : node.Substring(colonIndex + 1);
                }).ToList();

                if (nodeNames.Count > 2) 
                {
                    string secLast = nodeNames[nodeNames.Count - 2];
                    string last = nodeNames[nodeNames.Count - 1];
                    levelPath = secLast + " > " + last;
                } 
                else
                {
                    levelPath = string.Join(" > ", nodeNames);
                }

                // As we are only showing immediate parents
                // prevent duplicates from being shown
			    resultPaths.Add(levelPath);			    
			}

            var sortedPaths = resultPaths.OrderBy(s => s);		    
            return string.Join(createNewLineForEachLevel ? Environment.NewLine : ", ", sortedPaths);		    
		}

		/// <summary>
		///     Insert date from datatable to worksheet.
		/// </summary>
        private static void InsertTableData(QueryResult queryResult, SheetData sheetData, WorkbookStylesPart stylesPart)
		{
			// Create excel columns to define a column widths.
			int columnIndx = 0;
			var cols = new Columns( );
			foreach ( ResultColumn column in queryResult.Columns )
			{
				if ( !column.IsHidden )
				{
					double width = GetColumnWidth( column.DisplayName );
					cols.Append( CreateColumnData( columnIndx, width ) );
					columnIndx++;
				}
			}
			sheetData.Append( cols );

			UInt32Value headerStyleIndex = CreateHeaderStyles( stylesPart.Stylesheet );

			// Add column names to the first row
			var header = new Row
			{
				RowIndex = ( UInt32 ) 1
			};
			int headerColInx = 0;
			foreach ( ResultColumn column in queryResult.Columns )
			{
				if ( !column.IsHidden )
				{
					Cell headerCell = CreateTextCell(
						column.DisplayName,
						headerStyleIndex );

					SetCellLocation( headerCell, headerColInx, 1 );
					header.AppendChild( headerCell );
					headerColInx++;
				}
			}
			sheetData.AppendChild( header );

			// Loop through each data row
			int excelRowIndex = 2;
			foreach ( DataRow row in queryResult.DataTable.Rows )
			{
				Row excelRow = CreateContentRow( queryResult, row, excelRowIndex++ );
				sheetData.AppendChild( excelRow );
			}
		}

		/// <summary>
		///     Assigns a cell to a location.
		/// </summary>
		/// <param name="cell">The cell to update.</param>
		/// <param name="columnIndex">One-based column.</param>
		/// <param name="rowIndex">One-based row.</param>
		private static void SetCellLocation( Cell cell, int columnIndex, int rowIndex )
		{
			cell.CellReference = GetExcelColumnName( columnIndex ) + rowIndex;
		}

		#region Export to CSV file methods

		private const string EscapedQuote = "\"\"";
		private const string Quote = "\"";

		private static readonly char[ ] CharactersThatMustBeQuoted =
		{
			',',
			'"',
			'\n'
		};

		/// <summary>
		///     Generate CSV document byte stream
		/// </summary>
		/// <param name="queryResult">QueryResult</param>
		/// <returns>byte[]</returns>
		public static byte[ ] CreateCsvDocument( QueryResult queryResult )
		{
			var currentCulure = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			try
			{
				var sb = new StringBuilder( );
				int headerIndx = 1;

				foreach ( ResultColumn col in queryResult.Columns )
				{
					//add separator 
					if ( !col.IsHidden )
					{
						sb.Append( col.DisplayName );
						if ( headerIndx != queryResult.Columns.Count )
							sb.Append( ',' );
					}
					headerIndx++;
				}
				//append new line 
				sb.Append( "\r\n" );

				foreach ( DataRow row in queryResult.DataTable.Rows )
				{
					int colIndx = 0;
					bool first = true;
					foreach ( ResultColumn col in queryResult.Columns )
					{
						if ( !col.IsHidden )
						{
							if ( first )
								first = false;
							else
								sb.Append( ',' );

							object cellValue = row[ colIndx ];
							string csvValue;

							if ( cellValue == null ||
							     cellValue is DBNull )
							{
								csvValue = string.Empty;
							}
							else if ( col.ColumnType is StructureLevelsType )
							{
								csvValue = GetStructureLevelCellValue( ( string ) cellValue, false );
							}
							else if ( col.ColumnType is ChoiceRelationshipType || col.ColumnType is InlineRelationshipType )
							{
								csvValue = DatabaseTypeHelper.GetEntityXmlName( ( string ) cellValue );
							}
							else if ( col.ColumnType is AutoIncrementType )
							{
								string displayPattern = null;

								if ( row.Table.Columns[ colIndx ].ExtendedProperties.ContainsKey( "DisplayPattern" ) )
								{
									displayPattern = ( string ) row.Table.Columns[ colIndx ].ExtendedProperties[ "DisplayPattern" ];
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

								csvValue = DatabaseTypeHelper.ConvertToString( col.ColumnType, temp, displayPattern );
							}
							else if ( col.ColumnType is BoolType )
							{
								csvValue = GetBooleanCellValue( ( bool ) cellValue );
							}
							else if ( col.ColumnType is DateType )
							{
								csvValue = ( ( DateTime ) cellValue ).ToString( "d/MM/yyyy" );
							}
							else if ( col.ColumnType is TimeType )
							{
								csvValue = ( ( DateTime ) cellValue ).ToString( "h:mm tt" );
							}
							else if ( col.ColumnType is DateTimeType )
							{
								csvValue = ( ( DateTime ) cellValue ).ToString( "d/MM/yyyy h:mm tt" );
							}
							else
							{
								//The cell value is string type.
								bool isRelationshipValue = col.RequestColumn.Expression is AggregateExpression;
								// Use ConvertToString, which is the network format, because it is invariant.
								csvValue = isRelationshipValue ? DatabaseTypeHelper.GetEntityXmlName( cellValue.ToString( ) ) : DatabaseTypeHelper.ConvertToString( col.ColumnType, cellValue );
								csvValue = csvValue.Replace( "\r", string.Empty ).Replace( "\n", string.Empty );
							}
							sb.Append( CsvEscape( csvValue ) );
						}
						colIndx++;
					}
					//append new line 
					sb.Append( "\r\n" );
				}
				byte[ ] bytesInStream = Encoding.UTF8.GetBytes( sb.ToString( ) );

				return bytesInStream;
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = currentCulure;
			}
		}

		/// <summary>
		///     Escape few special characters while writing to the CSV file.
		/// </summary>
		public static string CsvEscape( string s )
		{
			if ( string.IsNullOrEmpty( s ) )
				return s;

			if ( s.Contains( Quote ) )
				s = s.Replace( Quote, EscapedQuote );

			if ( s.IndexOfAny( CharactersThatMustBeQuoted ) > -1 )
				s = Quote + s + Quote;

			return s;
		}

		/// <summary>
		///     Export Data to CSV document.
		/// </summary>
		/// <returns>ExportDataInfo</returns>
		public static ExportDataInfo ExportToCsvDocument( QueryResult queryResult )
		{
			byte[ ] bytesInStream = CreateCsvDocument( queryResult );
			var export = new ExportDataInfo
			{
				FileStream = bytesInStream
			};

			return export;
		}

		/// <summary>
		///     Get the strings for csv file for different data types.
		/// </summary>
		/// <param name="type">DatabaseType</param>
		/// <param name="cellValue">string</param>
		/// <param name="col">ReportColumn</param>
		/// <returns>string</returns>
		public static string GetCsvCellValue( DatabaseType type, string cellValue, ReportColumn col )
		{
			//convert cell value to its particular type
			object value = DatabaseTypeHelper.ConvertFromString( type, cellValue );
			if ( cellValue == null )
			{
				return "";
			}
			if ( type is BoolType )
			{
				return GetBooleanCellValue( ( bool ) value );
			}
			if ( type is AutoIncrementType )
			{
				return DatabaseTypeHelper.ConvertToString( type, value, col.AutoNumberDisplayPattern );
			}
			if ( type is DateType || type is DateTimeType )
			{
				return cellValue;
			}
			if ( type is TimeType )
			{
				return ( ( DateTime ) value ).ToUniversalTime( ).TimeOfDay.ToString( );
			}
		    if (type is StructureLevelsType)
		    {
		        return GetStructureLevelCellValue(cellValue, false);
		    }
			
			return value.ToString( );			
		}

		#endregion
	}
}