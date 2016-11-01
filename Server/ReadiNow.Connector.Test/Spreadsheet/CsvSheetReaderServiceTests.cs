// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class CsvSheetReaderServiceTests
    {
        CsvFileReaderService GetService()
        {
            return new CsvFileReaderService( );
        }

        [Test]
        public void Test_ReadSheet_Null_TextReader()
        {
            var service = GetService();
            Assert.Throws<ArgumentNullException>(() => service.OpenDataFile(null, new DataFileReaderSettings()));
        }

        [Test]
        public void Test_ReadSheet_Null_Settings()
        {
            var service = GetService();
            using (Stream stream = new MemoryStream())
            {
                Assert.Throws<ArgumentNullException>(() => service.OpenDataFile(stream, null));
            }
        }

        [TestCase("cr")]
        [TestCase("lf")]
        [TestCase("crlf")]
        public void Test_ReadSheet_Data(string lineend)
        {
            string csv =
                "TextCol,NumberCol\n" +
                "Data1,321\n" +
                "Data2,123\n";

            if (lineend == "cr")
                csv = csv.Replace("\n", "\r");
            if (lineend == "crlf")
                csv = csv.Replace("\n", "\r\n");

            TestTwoRecords(csv);
        }

        [TestCase( "cr" )]
        [TestCase( "lf" )]
        [TestCase( "crlf" )]
        public void Test_ReadSheet_Data_LastRow( string lineend )
        {
            string csv =
                "TextCol,NumberCol\n" +
                "Data1,321\n" +
                "Data1,321\n" +
                "Data2,123\n";

            if ( lineend == "cr" )
                csv = csv.Replace( "\n", "\r" );
            if ( lineend == "crlf" )
                csv = csv.Replace( "\n", "\r\n" );

            var settings = new DataFileReaderSettings { LastDataRowNumber = 3 };
            TestRecords( csv, settings, 2 );
        }

        [Test]
        public void Test_ReadSheet_NoTrailingNewline()
        {
            string csv =
                "TextCol,NumberCol\n" +
                "Data1,321\n" +
                "Data2,123";

            TestTwoRecords(csv);
        }

        [Test]
        public void Test_ReadSheet_QuotedContent()
        {
            string csv =
                "\"TextCol\",\"NumberCol\"\n" +
                "\"Data1\",\"321\"\n" +
                "\"Data2\",\"123\"";

            TestTwoRecords(csv);
        }

        [Test]
        public void Test_ReadSheet_Empty()
        {
            TestRecords("", null, 0);
        }

        [Test]
        public void Test_ReadSheet_NoHeaderRow()
        {
            string csv =
                "Data1,321\n" +
                "Data2,123";

            var settings = new DataFileReaderSettings {FirstDataRowNumber = 1};
            TestTwoRecords(csv, settings);
        }

        [Test]
        public void Test_ReadSheet_DocHasLessRowsThanFirstRow()
        {
            string csv = "";

            var settings = new DataFileReaderSettings { FirstDataRowNumber = 10 };
            TestRecords(csv, settings, 0);
        }

        [Test]
        public void Test_ReadSheet_GapAfterHeaderRow()
        {
            string csv =
                "TextCol,NumberCol\n" +
                "\n" +
                "Data1,321\n" +
                "Data2,123";

            var settings = new DataFileReaderSettings { FirstDataRowNumber = 3 };
            TestTwoRecords(csv, settings);
        }

        [Test]
        public void Test_ReadSheet_GapAfterHeaderRow_LastRow( )
        {
            string csv =
                "TextCol,NumberCol\n" +
                "\n" +
                "Data1,321\n" +
                "Data2,123";

            var settings = new DataFileReaderSettings { FirstDataRowNumber = 3, LastDataRowNumber = 3 };
            TestRecords( csv, settings, 1 );
        }

        [Test]
        public void Test_ReadMetadata_Default( )
        {
            string csv =
                "TextCol,NumberCol\n" +
                "Data1,321\n" +
                "Data2,123";

            var settings = new DataFileReaderSettings();
            var metadata = GetMetadata(csv, settings);

            Assert.That(metadata.Fields, Has.Count.EqualTo(2));
            Assert.That(metadata.Fields[0].Key, Is.EqualTo("1"));
            Assert.That(metadata.Fields[0].Title, Is.EqualTo("TextCol"));
            Assert.That(metadata.Fields[1].Key, Is.EqualTo("2"));
            Assert.That(metadata.Fields[1].Title, Is.EqualTo("NumberCol"));
        }

        [Test]
        public void Test_ReadMetadata_NoData( )
        {
            string csv = "";

            var settings = new DataFileReaderSettings();
            var metadata = GetMetadata(csv, settings);

            Assert.That(metadata.Fields, Has.Count.EqualTo(0));
        }

        [Test]
        public void Test_ReadMetadata_NoHeading()
        {
            string csv =
                "Data1,321\n" +
                "Data2,123";

            var settings = new DataFileReaderSettings {HeadingRowNumber = 0};
            var metadata = GetMetadata(csv, settings);

            Assert.That(metadata.Fields, Has.Count.EqualTo(2));
            Assert.That(metadata.Fields[0].Key, Is.EqualTo("1"));
            Assert.That(metadata.Fields[0].Title, Is.EqualTo("1"));
            Assert.That(metadata.Fields[1].Key, Is.EqualTo("2"));
            Assert.That(metadata.Fields[1].Title, Is.EqualTo("2"));
        }

        [TestCase( "123", 123 )]
        [TestCase( "-1234.5", -1235 )]
        public void Test_ReadSheet_GetInt( string data, int expected )
        {
            string csv = "Header\n" + data;

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            Assert.That( reader.GetInt( "1" ), Is.EqualTo( expected ) );
        }

        [TestCase( "Hello", "Hello" )]
        [TestCase( "\"Test\"\",Test\"", "Test\",Test" )]
        public void Test_ReadSheet_GetString( string data, string expected )
        {
            string csv = "Header\n" + data;

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            Assert.That( reader.GetString( "1" ), Is.EqualTo( expected ) );
        }

        [Test]
        public void Test_ReadSheet_GetBoolean( )
        {
            string csv = "Header\nTrue";

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            Assert.That( reader.GetBoolean( "1" ), Is.EqualTo( true ) );
        }

        [Test]
        public void Test_ReadSheet_GetDecimal( )
        {
            string csv = "Header\n0.14";

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            Assert.That( reader.GetDecimal( "1" ), Is.EqualTo( 0.14M ) );
        }

        [Test]
        public void Test_ReadSheet_GetStringList( )
        {
            string csv = "Header\n Test1 ; ; Test2 ";

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            IReadOnlyList<string> list = reader.GetStringList( "1" );
            Assert.That( list, Is.EquivalentTo( new[ ]
            {
                "Test1",
                "Test2"
            } ) );
        }

        [Test]
        public void Test_ReadSheet_GetIntList( )
        {
            string csv = "Header\n 3 ; ; 5 ";

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            IReadOnlyList<int> list = reader.GetIntList( "1" );
            Assert.That( list, Is.EquivalentTo( new [ ]
            {
                3,
                5
            } ) );
        }

        [Test]
        public void Test_ReadSheet_GetDecimalList( )
        {
            string csv = "Header\n 3.3 ; ; 5.5 ";

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            IReadOnlyList<decimal> list = reader.GetDecimalList( "1" );
            Assert.That( list, Is.EquivalentTo( new [ ]
            {
                3.3M,
                5.5M
            } ) );
        }

        [TestCase( "2012-05-08", "2012-05-08T00:00:00" )]
        [TestCase( "2012-05-08T15:15:00", "2012-05-08T00:00:00" )]
        public void Test_ReadSheet_GetDate( string data, string expected )
        {
            DateTime expectedDate = DateTime.Parse( expected, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal );
            string csv = "Header\n" + data;

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            DateTime? value = reader.GetDate( "1" );
            Assert.That( value, Is.EqualTo( expectedDate ) );
        }

        [TestCase( "2012-05-08", "2012-05-08T00:00:00" )]
        [TestCase( "2012-05-08T15:15:00", "2012-05-08T15:15:00" )]
        public void Test_ReadSheet_GetDateTime( string data, string expected )
        {
            DateTime expectedDate = DateTime.Parse( expected, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal );
            string csv = "Header\n" + data;

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            DateTime? value = reader.GetDateTime( "1" );
            Assert.That( value, Is.EqualTo( expectedDate ) );
        }

        [TestCase( "15:15:00", "1753-01-01T15:15:00" )]
        [TestCase( "2012-05-08T15:15:00", "1753-01-01T15:15:00" )]
        public void Test_ReadSheet_GetTime( string data, string expected )
        {
            DateTime expectedDate = DateTime.Parse( expected, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal );
            string csv = "Header\n" + data;

            var objects = TestRecords( csv, null, 1 );
            var reader = objects.First( );
            DateTime? value = reader.GetTime( "1" );
            Assert.That( value, Is.EqualTo( expectedDate ) );
        }

        [TestCase( 1, 2, "Header\nData", "Line 2" )]
        [TestCase( 1, 2, "Header\nData\nData 2", "Line 2" )]
        [TestCase( 1, 3, "Header\n\nData\nData 2", "Line 3" )]
        [TestCase( 0, 1, "Data", "Line 1" )]
        [TestCase( 0, 1, "Data\nData 2", "Line 1" )]
        // The following cases report an incorrect line number because of the preceeding blank line and an issue in the .Net TextFieldParser. See notes in CsvFileReader.ReadFields
        //[TestCase( 0, 1, "\nData\nData", "Line 2" )]
        //[TestCase( 0, 2, "Header\n\nData\nData", "Line 3" )]
        //[TestCase( 0, 2, "Header\n\nData", "Line 3" )]
        //[TestCase( 0, 1, "\nData", "Line 2" )]
        public void Test_ReadSheet_GetLocation( int headerRow, int firstData, string csv, string expected )
        {
            DataFileReaderSettings settings = new DataFileReaderSettings( );
            settings.FirstDataRowNumber = firstData;
            settings.HeadingRowNumber = headerRow;

            var objects = TestRecords( csv, settings );
            var reader = objects.First( );
            string location = reader.GetLocation( );
            Assert.That( reader.GetString( "1" ), Is.EqualTo( "Data" ) );
            Assert.That( location, Is.EqualTo( expected ) );
        }

        private void TestTwoRecords(string csv, DataFileReaderSettings settings = null)
        {
            var objects = TestRecords(csv, settings, 2);

            var object1 = objects[0];
            Assert.That(object1.GetString("1"), Is.EqualTo("Data1"));
            Assert.That(object1.GetInt("2"), Is.EqualTo(321));

            var object2 = objects[1];
            Assert.That(object2.GetString("1"), Is.EqualTo("Data2"));
            Assert.That(object2.GetInt("2"), Is.EqualTo(123));
        }

        private List<IObjectReader> TestRecords(string csv, DataFileReaderSettings settings, int expectedCount = -1)
        {
            var service = GetService();
            List<IObjectReader> objects;
            using ( Stream stream = SheetTestHelper.GetCsvStream(csv) )
            using ( IObjectsReader objectsReader = service.OpenDataFile( stream, settings ?? new DataFileReaderSettings( ) ) )
            {
                objects = objectsReader.GetObjects( ).ToList( );
            }

            Assert.That(objects, Is.Not.Null, "objects");
            if ( expectedCount  != -1 )
                Assert.That(objects.Count, Is.EqualTo(expectedCount));
            return objects;
        }

        private SheetMetadata GetMetadata(string csv, DataFileReaderSettings settings)
        {
            var service = GetService();
            SheetMetadata metadata;
            using (MemoryStream stream = new MemoryStream() )
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(csv);
                writer.Flush();
                stream.Position = 0;
                using ( IDataFile dataFile = service.OpenDataFile( stream, settings ?? new DataFileReaderSettings( ) ) )
                {
                    metadata = dataFile.ReadMetadata( );
                }
            }

            Assert.That(metadata, Is.Not.Null, "metadata");
            return metadata;
        }
    }
}
