// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using System;
using NUnit.Framework;
using ReadiNow.Connector.Processing;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;

namespace ReadiNow.Connector.Test.Processing
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [RunWithTransaction]
    class ImportRunReporterTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_Null_ImportRun( )
        {
            Assert.Throws<ArgumentNullException>( () => new ImportRunReporter( null ) );
        }

        [TestCase( "Line 1", "Line 1: Message\r\n" )]
        [TestCase( null, "Message\r\n" )]
        [RunAsDefaultTenant]
        public void Test_ReportError( string location, string expectedMessage )
        {
            ImportRun importRun = new ImportRun( );
            ImportRunReporter reporter = new ImportRunReporter( importRun );

            Mock<IObjectReader> mockObject = new Mock<IObjectReader>( );
            mockObject.Setup( obj => obj.GetLocation( ) ).Returns( location );
            reporter.ReportError( mockObject.Object, "Message" );
            reporter.Flush( );

            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 1 ) );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 0 ) );
            Assert.That( importRun.ImportMessages, Is.EqualTo( expectedMessage ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ReportOk_Single( )
        {
            ImportRun importRun = new ImportRun( );
            ImportRunReporter reporter = new ImportRunReporter( importRun );

            reporter.ReportOk( );
            reporter.ReportOk( 2 );
            reporter.Flush( );

            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 0 ) );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 3 ) );
            Assert.That( importRun.ImportMessages, Is.Null );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Calls_AccumulateForDifferentRecords( )
        {
            ImportRun importRun = new ImportRun( );
            ImportRunReporter reporter = new ImportRunReporter( importRun );

            Mock<IObjectReader> mockObject = new Mock<IObjectReader>( );
            Mock<IObjectReader> mockObject2 = new Mock<IObjectReader>( );
            mockObject.Setup( obj => obj.GetLocation( ) ).Returns( ( ) => "Line 1" );
            mockObject2.Setup( obj => obj.GetLocation( ) ).Returns( ( ) => "Line 2" );
            reporter.ReportError( mockObject.Object, "Message" );
            reporter.ReportError( mockObject2.Object, "Message" );
            reporter.ReportOk( );
            reporter.Flush( );
            reporter.ReportOk( 2 );
            reporter.Flush( );

            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 2 ) );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 3 ) );
            Assert.That( importRun.ImportMessages, Is.EqualTo( "Line 1: Message\r\nLine 2: Message\r\n" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Calls_DontAccumulateForSameRecord( )
        {
            ImportRun importRun = new ImportRun( );
            ImportRunReporter reporter = new ImportRunReporter( importRun );

            Mock<IObjectReader> mockObject = new Mock<IObjectReader>( );
            mockObject.Setup( obj => obj.GetLocation( ) ).Returns( ( ) => "Line 1" );
            reporter.ReportError( mockObject.Object, "Message" );
            reporter.ReportError( mockObject.Object, "Message" );
            reporter.ReportOk( );
            reporter.Flush( );
            reporter.ReportOk( 2 );
            reporter.Flush( );

            Assert.That( importRun.ImportRecordsFailed, Is.EqualTo( 1 ) );
            Assert.That( importRun.ImportRecordsSucceeded, Is.EqualTo( 3 ) );
            Assert.That( importRun.ImportMessages, Is.EqualTo( "Line 1: Message\r\nLine 1: Message\r\n" ) );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_MessageReordering( )
        {
            ImportRun importRun = new ImportRun( );
            ImportRunReporter reporter = new ImportRunReporter( importRun );

            Mock<IObjectReader> mockObject = new Mock<IObjectReader>( );
            Mock<IObjectReader> mockObject2 = new Mock<IObjectReader>( );
            mockObject.Setup( obj => obj.GetLocation( ) ).Returns( ( ) => "Line 10" );
            mockObject2.Setup( obj => obj.GetLocation( ) ).Returns( ( ) => "Line 2" );
            reporter.ReportError( mockObject.Object, "Message" );
            reporter.ReportError( mockObject2.Object, "Message" );
            reporter.ReportError( mockObject.Object, "Message" );
            reporter.ReportError( mockObject2.Object, "Message" );
            reporter.Flush( );

            Assert.That( importRun.ImportMessages, Is.EqualTo( "Line 2: Message\r\nLine 2: Message\r\nLine 10: Message\r\nLine 10: Message\r\n" ) );
        }
    }
}
