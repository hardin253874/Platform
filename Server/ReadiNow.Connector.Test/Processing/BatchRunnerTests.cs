// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using NUnit.Framework;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Processing;

namespace ReadiNow.Connector.Test.Processing
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class BatchRunnerTests
    {
        [Test]
        public void Test_ObjectsReader_NotSet( )
        {
            BatchRunner batchRunner = new BatchRunner( );
            batchRunner.RecordImporter = new Mock<IRecordImporter>( ).Object;
            Assert.Throws<InvalidOperationException>( ( ) => batchRunner.ProcessAll( ) );
        }

        [Test]
        public void Test_RecordImporter_NotSet( )
        {
            BatchRunner batchRunner = new BatchRunner( );
            batchRunner.ObjectsReader = new Mock<IObjectsReader>( ).Object;
            Assert.Throws<InvalidOperationException>( ( ) => batchRunner.ProcessAll( ) );
        }

        [TestCase( -1 )]
        [TestCase( 0 ) ]
        public void Test_BatchSize_OutOfRange( int batchSize )
        {
            BatchRunner batchRunner = new BatchRunner( );
            batchRunner.RecordImporter = new Mock<IRecordImporter>( ).Object;
            batchRunner.ObjectsReader = new Mock<IObjectsReader>( ).Object;
            batchRunner.BatchSize = batchSize;
            Assert.Throws<InvalidOperationException>( ( ) => batchRunner.ProcessAll( ) );
        }

        [TestCase( 10, 10, 0 )]
        [TestCase( 20, 20, 0 )]
        [TestCase( 30, 20, 10 )]
        public void Test_Runs( int sampleSize, int call1, int call2 )
        {
            Mock<IRecordImporter> mockImporter = new Mock<IRecordImporter>( MockBehavior.Strict );
            Mock<IObjectsReader> mockReader = new Mock<IObjectsReader>( MockBehavior.Strict );
            Mock<ICancellationWatcher> mockCancellation = new Mock<ICancellationWatcher>( MockBehavior.Loose );

            IList<IObjectReader> readers;
            readers = Enumerable.Range( 0, sampleSize ).Select( i => new Mock<IObjectReader>( ).Object ).ToList( );

            mockReader.Setup( reader => reader.GetObjects( ) ).Returns( ( ) => readers );
            mockImporter.Setup( importer => importer.ImportRecords( It.Is<IEnumerable<IObjectReader>>( list => list.Count( ) == call1 && list.First() == readers[0] ) ) );
            if ( call2 > 0 )
            {
                mockImporter.Setup( importer => importer.ImportRecords( It.Is<IEnumerable<IObjectReader>>( list => list.Count( ) == call2 && list.First( ) == readers [ call1 ] ) ) );
            }
            mockCancellation.Setup( c => c.IsCancellationRequested ).Returns( false );

            BatchRunner batchRunner = new BatchRunner
            {
                RecordImporter = mockImporter.Object,
                ObjectsReader = mockReader.Object,
                CancellationWatcher = mockCancellation.Object
            };
            
            batchRunner.ProcessAll( );

            mockReader.VerifyAll( );
            mockImporter.VerifyAll( );
        }

        [Test]
        public void Test_Cancelled( )
        {
            Mock<IRecordImporter> mockImporter = new Mock<IRecordImporter>( MockBehavior.Strict );
            Mock<IObjectsReader> mockReader = new Mock<IObjectsReader>( MockBehavior.Strict );
            Mock<ICancellationWatcher> mockCancellation = new Mock<ICancellationWatcher>( MockBehavior.Loose );

            IList<IObjectReader> readers;
            readers = Enumerable.Range( 0, 10 ).Select( i => new Mock<IObjectReader>( ).Object ).ToList( );

            mockReader.Setup( reader => reader.GetObjects( ) ).Returns( ( ) => readers );
            mockCancellation.Setup( c => c.IsCancellationRequested ).Returns( true );

            BatchRunner batchRunner = new BatchRunner
            {
                RecordImporter = mockImporter.Object,
                ObjectsReader = mockReader.Object,
                CancellationWatcher = mockCancellation.Object
            };

            batchRunner.ProcessAll( );

            mockReader.VerifyAll( );
            mockImporter.VerifyAll( );
        }
    }
}
