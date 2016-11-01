// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using EDC;
using NUnit.Framework;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    /// 
    /// </summary>
    class ExcelSheetReaderServiceTests
    {
        ExcelFileReaderService GetService()
        {
            return new ExcelFileReaderService();
        }

        [Test]
        public void Test_OpenDataFile_Null_TextReader( )
        {
            var service = GetService();
            Assert.Throws<ArgumentNullException>(() => service.OpenDataFile(null, new DataFileReaderSettings()));
        }

        [Test]
        public void Test_OpenDataFile_Null_Settings( )
        {
            var service = GetService();
            using (Stream stream = new MemoryStream())
            {
                Assert.Throws<ArgumentNullException>(() => service.OpenDataFile(stream, null));
            }
        }

        [Test]
        public void Test_ReadSheet_NoSheets( )
        {
            var service = GetService( );
            DataFileReaderSettings settings = new DataFileReaderSettings( );
            using ( Stream stream = SheetTestHelper.GetStream( "NoSheets.xlsx" ) )
            using ( IObjectsReader reader = service.OpenDataFile( stream, settings ) )
            {
                int count = reader.GetObjects( ).Count( );
                Assert.That( count, Is.EqualTo( 0 ) );
            }
        }

        [TestCase( "Boolean", null, 14 )]
        [TestCase( "Boolean", 4, 3 )]
        [TestCase( "Currency", null, 9 )]
        public void Test_ReadSheet_BlankRows( string sheet, int? lastRow, int expected )
        {
            var service = GetService( );
            DataFileReaderSettings settings;
            using ( Stream stream = SheetTestHelper.GetStream( "Test File.xlsx" ) )
            {
                settings = SheetTestHelper.GetSettings( stream, sheet );
                settings.LastDataRowNumber = lastRow;
            }
            using ( Stream stream = SheetTestHelper.GetStream( "Test File.xlsx" ) )
            using ( IObjectsReader reader = service.OpenDataFile( stream, settings ) )
            {
                int count = reader.GetObjects( ).Count( );
                Assert.That( count, Is.EqualTo( expected ) );
            }
        }

        [TestCase(10, null, null)]
        [TestCase(10, 12, 3)]
        public void Test_ReadSheet_Data(int firstRow, int? lastRow, int? expectedCount )
        {
            var service = GetService();
            DataFileReaderSettings settings;
            using ( Stream stream = SheetTestHelper.GetStream( "TestSheet.xlsx" ) )
            {
                settings = SheetTestHelper.GetSettings( stream, "TestSheet" );
                settings.FirstDataRowNumber = 10;
                settings.LastDataRowNumber = lastRow;
            }
            using ( Stream stream = SheetTestHelper.GetStream( "TestSheet.xlsx" ) )
            using ( IObjectsReader reader = service.OpenDataFile( stream, settings ) )
            {

                int count = 0;
                foreach ( IObjectReader obj in reader.GetObjects( ) )
                {
                    string column = "A";
                    string dataType = obj.GetString( column );
                    Assert.That( dataType, Is.Not.Null.Or.Empty );
                    count++;
                    if ( count == 20 )
                        break;  // that's enough
                }
                Assert.That( count, Is.GreaterThan( 0 ) );
                if ( expectedCount != null )
                    Assert.That( count, Is.EqualTo(expectedCount.Value) );
            }
        }

        [Test]
        public void Test_ReadMetadata_27728( )
        {
            var service = GetService( );
            DataFileReaderSettings settings = new DataFileReaderSettings( );
            using ( Stream stream = SheetTestHelper.GetStream( "Qualification.xlsx" ) )
            using ( IDataFile reader = service.OpenDataFile( stream, settings ) )
            {
                SheetMetadata metadata = reader.ReadMetadata( );
                Assert.That( metadata.Fields, Has.Count.EqualTo( 2 ) );
                Assert.That( metadata.Fields[ 0 ].Key, Is.EqualTo( "A" ) );
                Assert.That( metadata.Fields [ 1 ].Key, Is.EqualTo( "B" ) );
                Assert.That( metadata.Fields [ 0 ].Title, Is.EqualTo( "Name" ) );
                Assert.That( metadata.Fields [ 1 ].Title, Is.EqualTo( "Qualifcation code" ) );
            }
        }

        private static readonly int[ ] TestRowNumbers = Enumerable.Range( 10, 193 - 10 + 1 ).ToArray( );

        [Test]
        [TestCaseSource( "TestRowNumbers" )]
        public void Test_Scenarios( int rowNum )
        {
            var service = GetService( );
            DataFileReaderSettings settings;
            using ( Stream stream = SheetTestHelper.GetStream( "TestSheet.xlsx" ) ) // IMPORTANT: Ensure TestRowNumbers has the right number of rows
            {
                settings = SheetTestHelper.GetSettings( stream, "TestSheet" );
                settings.FirstDataRowNumber = rowNum;
            }
            using ( Stream stream = SheetTestHelper.GetStream( "TestSheet.xlsx" ) )
            using ( IObjectsReader reader = service.OpenDataFile( stream, settings ) )
            {
                IObjectReader obj = reader.GetObjects().First();

                string actualColRef = "D";
                string[] expectedColumns = { "E", "G", "I", "K", "M", "O", "Q" };

                foreach ( string expectedColumn in expectedColumns )
                {
                    string expectedNative = obj.GetString( expectedColumn );

                    if ( string.IsNullOrEmpty( expectedNative ) )
                        continue;

                    switch ( expectedColumn )
                    {
                        case "E": // String
                            string actualString = obj.GetString( actualColRef );
                            string actualSingleLine = StringHelpers.ToSingleLine( actualString );
                            Assert.That( actualSingleLine, Is.EqualTo( expectedNative ) );
                            break;
                        case "G": // Number
                            int? actualNumber = obj.GetInt( actualColRef );
                            int expectedNumber = int.Parse( expectedNative, CultureInfo.InvariantCulture );
                            Assert.That( actualNumber, Is.EqualTo( expectedNumber ) );
                            break;
                        case "I": // Decimal
                            decimal? actualDecimal = obj.GetDecimal( actualColRef );
                            decimal expectedDecimal = decimal.Parse( expectedNative, CultureInfo.InvariantCulture );
                            Assert.That( actualDecimal, Is.EqualTo( expectedDecimal ) );
                            break;
                        case "K": // Boolean
                            bool? actualBool = obj.GetBoolean( actualColRef );
                            bool expectedBool = expectedNative == "Yes" || expectedNative != "No" && bool.Parse( expectedNative );
                            Assert.That( actualBool, Is.EqualTo( expectedBool ) );
                            break;
                        case "M": // DateTime
                            DateTime? actualDateTime = obj.GetDateTime( actualColRef );
                            DateTime expectedDateTime = DateTime.Parse( expectedNative, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal );
                            Assert.That( actualDateTime, Is.EqualTo( expectedDateTime ) );
                            break;
                        case "O": // Date
                            DateTime? actualDate = obj.GetDate( actualColRef );
                            DateTime expectedDate = DateTime.Parse( expectedNative, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal );
                            Assert.That( actualDate, Is.EqualTo( expectedDate ) );
                            break;
                        case "Q": // Time
                            DateTime? actualTime = obj.GetTime( actualColRef );
                            DateTime expectedTime = DateTime.Parse( expectedNative, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal );
                            Assert.That( actualTime, Is.EqualTo( expectedTime ) );
                            break;
                    }

                }

                
            }
        }

    }
}
