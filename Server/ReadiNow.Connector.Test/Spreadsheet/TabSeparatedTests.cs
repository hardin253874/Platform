// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ReadiNow.Connector.ImportSpreadsheet;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class TabSeparatedTests
    {
        CsvFileReaderService GetService( )
        {
            return new CsvFileReaderService( );
        }

        [Test]
        public void Test_ReadSheet_Data( )
        {
            string csv =
                "TextCol\tNumberCol\n" +
                "Data1\t321\n" +
                "Data2\t123\n";

            TestTwoRecords( csv );
        }

        [Test]
        public void Test_ReadSheet_QuotedContent( )
        {
            string csv =
                "\"TextCol\"\t\"NumberCol\"\n" +
                "\"Data1\"\t\"321\"\n" +
                "\"Data2\"\t\"123\"";

            TestTwoRecords( csv );
        }

        [Test]
        public void Test_ReadSheet_NoHeaderRow( )
        {
            string csv =
                "Data1\t321\n" +
                "Data2\t123";

            var settings = new DataFileReaderSettings { FirstDataRowNumber = 1, ImportFormat = ImportFormat.Tab };
            TestTwoRecords( csv, settings );
        }

        private void TestTwoRecords( string csv, DataFileReaderSettings settings = null )
        {
            var objects = TestRecords( csv, settings, 2 );

            var object1 = objects [ 0 ];
            Assert.That( object1.GetString( "1" ), Is.EqualTo( "Data1" ) );
            Assert.That( object1.GetInt( "2" ), Is.EqualTo( 321 ) );

            var object2 = objects [ 1 ];
            Assert.That( object2.GetString( "1" ), Is.EqualTo( "Data2" ) );
            Assert.That( object2.GetInt( "2" ), Is.EqualTo( 123 ) );
        }

        private List<IObjectReader> TestRecords( string csv, DataFileReaderSettings settings, int expectedCount )
        {
            var service = GetService( );
            List<IObjectReader> objects;
            using ( Stream stream = SheetTestHelper.GetCsvStream( csv ) )
            using ( IObjectsReader objectsReader = service.OpenDataFile( stream, settings ?? new DataFileReaderSettings { ImportFormat = ImportFormat.Tab } ) )
            {
                objects = objectsReader.GetObjects( ).ToList( );
            }

            Assert.That( objects, Is.Not.Null, "objects" );
            Assert.That( objects.Count, Is.EqualTo( expectedCount ) );
            return objects;
        }
    }
}
