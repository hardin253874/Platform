// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NUnit.Framework;
using ReadiNow.Connector.Spreadsheet;

namespace ReadiNow.Connector.Test.Spreadsheet
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class ExcelHelpersTests
    {
        [TestCase( "A1", "A" )]
        [TestCase( "A123", "A" )]
        [TestCase( "AA123", "AA" )]
        [TestCase( "AA", "AA" )]
        [TestCase( "1", "" )]
        [TestCase( null, null )]
        public void Test_GetColumnPart( string input, string expected )
        {
            string actual = ExcelHelpers.GetColumnPart( input );
            Assert.That( actual, Is.EqualTo( expected ) );
        }

        [Test]
        public void Test_GetWorksheetByName( )
        {
            using ( Stream stream = SheetTestHelper.GetStream( "TestSheet.xlsx" ) )
            using ( SpreadsheetDocument doc = SpreadsheetDocument.Open( stream, false ) )
            {
                Worksheet worksheet = ExcelHelpers.GetWorksheetByName( doc, "TestSheet" );
                Assert.That( worksheet, Is.Not.Null );
            }
        }
    }
}
