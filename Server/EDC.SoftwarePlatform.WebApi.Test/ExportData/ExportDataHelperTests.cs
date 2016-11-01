// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.IO;
using EDC.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Services.ExportData;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.ExportData
{
	[TestFixture]
	public class ExportDataHelperTests
	{
		/// <summary>
		///     Create Export Directory if it doesn't exist.
		/// </summary>
		private static string CreateExportDirectory( )
		{
			const string newpath = @"C:\Test";

			//Create new Tets folder
			if ( !Directory.Exists( newpath ) )
			{
				Directory.CreateDirectory( newpath );
				return newpath;
			}

			return newpath;
		}

		/// <summary>
		///     Create dataset contains 3 tables with sample data to test export function.
		/// </summary>
		private QueryResult CreateSampleData( )
		{
		    var queryBuild = new QueryBuild( );
		    var table = new QueryResult( queryBuild );
            table.DataTable = new DataTable( "Table" );
			table.DataTable.Columns.Add( new DataColumn( "A", typeof ( string ) ) );
			table.DataTable.Columns.Add( new DataColumn( "B", typeof ( string ) ) );
			table.DataTable.Columns.Add( new DataColumn( "C", typeof ( DateTime ) ) );
			table.DataTable.Columns.Add( new DataColumn( "D", typeof ( bool ) ) );
			table.DataTable.Columns.Add( new DataColumn( "E", typeof ( int ) ) );

			var col = new ResultColumn
			{
				RequestColumn = new SelectColumn
				{
					DisplayName = "First Name"
				},
				ColumnType = DatabaseType.StringType
			};
			table.Columns.Add( col );

			col = new ResultColumn
			{
				RequestColumn = new SelectColumn
				{
					DisplayName = "LastName"
				},
				ColumnType = DatabaseType.StringType
			};
			table.Columns.Add( col );

			col = new ResultColumn
			{
				RequestColumn = new SelectColumn
				{
					DisplayName = "DateOfBirth"
				},
				ColumnType = DatabaseType.DateTimeType
			};
			table.Columns.Add( col );

			col = new ResultColumn
			{
				RequestColumn = new SelectColumn
				{
					DisplayName = "Male"
				},
				ColumnType = DatabaseType.BoolType
			};
			table.Columns.Add( col );

			col = new ResultColumn
			{
				RequestColumn = new SelectColumn
				{
					DisplayName = "Age"
				},
				ColumnType = DatabaseType.Int32Type
			};
			table.Columns.Add( col );

			var row = table.DataTable.NewRow( );
			row[ 0 ] = "James";
			row[ 1 ] = "Brown";
			row[ 2 ] = new DateTime( 1962, 3, 19 );
			row[ 3 ] = true;
			row[ 4 ] = 28;
			table.DataTable.Rows.Add( row );

			row = table.DataTable.NewRow( );
			row[ 0 ] = "Edward";
			row[ 1 ] = "Jones";
			row[ 2 ] = new DateTime( 1939, 7, 12 );
			row[ 3 ] = true;
			row[ 4 ] = 52;
			table.DataTable.Rows.Add( row );

			row = table.DataTable.NewRow( );
			row[ 0 ] = "Janet";
			row[ 1 ] = "Spender";
			row[ 2 ] = new DateTime( 1996, 1, 7 );
			row[ 3 ] = false;
			row[ 4 ] = 75;
			table.DataTable.Rows.Add( row );

			row = table.DataTable.NewRow( );
			row[ 0 ] = "Maria";
			row[ 1 ] = "Percy";
			row[ 2 ] = new DateTime( 1991, 10, 24 );
			row[ 3 ] = false;
			row[ 4 ] = 23;
			table.DataTable.Rows.Add( row );

			row = table.DataTable.NewRow( );
			row[ 0 ] = "Malcolm";
			row[ 1 ] = "Marvelous";
			row[ 2 ] = new DateTime( 1973, 5, 7 );
			row[ 3 ] = true;
			row[ 4 ] = 64;
			table.DataTable.Rows.Add( row );

			return table;
		}

		[Test]
		[RunAsDefaultTenant]
		public void ExportDataToCSV_Valid( )
		{
			var table = CreateSampleData( );
			ExportDataInfo exportInfo = ExportDataHelper.ExportToCsvDocument( table );

			byte[ ] byteStrem = exportInfo.FileStream;
			string path = CreateExportDirectory( );
			string filePath = Path.Combine( path, "Test.csv" );

			var file = new FileStream( filePath, FileMode.Create );
			file.Write( byteStrem, 0, byteStrem.Length );
			file.Close( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void ExportDataToExcel_Valid( )
		{
			var table = CreateSampleData( );
			ExportDataInfo exportInfo = ExportDataHelper.ExportToExcelDocument( table );

			byte[ ] byteStrem = exportInfo.FileStream;
			string path = CreateExportDirectory( );
			string filePath = Path.Combine( path, "Test.xlsx" );

			var file = new FileStream( filePath, FileMode.Create );
			file.Write( byteStrem, 0, byteStrem.Length );
			file.Close( );
		}

		[Test]
		[RunAsDefaultTenant]
		public void ExportDataToWord_Valid( )
		{
			var table = CreateSampleData( );
			ExportDataInfo exportInfo = ExportToWordHelper.ExportToWord( table, "Test Report" );

			byte[ ] byteStrem = exportInfo.FileStream;
			string path = CreateExportDirectory( );
			string filePath = Path.Combine( path, "Test.docx" );

			var file = new FileStream( filePath, FileMode.Create );
			file.Write( byteStrem, 0, byteStrem.Length );
			file.Close( );
		}
	}
}