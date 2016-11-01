// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    /// 
    /// </summary>
    class SheetTestHelper
    {
        public static Stream GetCsvStream( string csvData )
        {
            MemoryStream stream = new MemoryStream( );
            using ( StreamWriter writer = new StreamWriter( stream, Encoding.Default, 2048, true ) )
            {
                writer.Write( csvData );
                writer.Flush( );
            }
            stream.Position = 0;
            return stream;
        }


        public static Stream GetStream(string file)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("ReadiNow.Connector.Test.Spreadsheet.TestFiles." + file);
            return stream;
        }

        public static string GetSheetId( string resourceFileName, string sheetName )
        {
            using ( var stream = GetStream( resourceFileName ) )
            {
                return GetSheetId( stream, sheetName );
            }
        }

        public static string GetSheetId(Stream excel, string sheetName)
        {
            // We now identify sheets by their name
            return sheetName;

            //using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(excel, false))
            //{
            //    IEnumerable<Sheet> sheets =
            //        spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

            //    Sheet sheet = sheets.FirstOrDefault(s => s.Name == sheetName);
            //    return sheet.Id.Value;
            //}
        }

        public static DataFileReaderSettings GetSettings(Stream excel, string sheetName)
        {
            DataFileReaderSettings settings = new DataFileReaderSettings
            {
                SheetId = GetSheetId(excel, sheetName)
            };
            return settings;
        }
    }
}
