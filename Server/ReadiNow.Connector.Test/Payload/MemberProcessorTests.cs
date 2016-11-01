// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Connector.Interfaces;
using ReadiNow.Connector.Payload;

namespace ReadiNow.Connector.Test.Payload
{
    /// <summary>
    /// Test MemberProcessor class.
    /// </summary>
    [TestFixture]
    public class MemberProcessorTests
    {
        [Test]
        public void Test_BulkConstructor_Null( )
        {
            Action<IEnumerable<ReaderEntityPair>, IImportReporter> bulkAction = null;
            Assert.Throws<ArgumentNullException>( ( ) => new MemberProcessor( bulkAction ) );
        }

        [Test]
        public void Test_SingleConstructor_Null( )
        {
            Action<IObjectReader, IEntity, IImportReporter> singleAction = null;
            Assert.Throws<ArgumentNullException>( ( ) => new MemberProcessor( singleAction ) );
        }

        [Test]
        public void Test_BulkConstructor_Called( )
        {
            IImportReporter reporter = new Mock<IImportReporter>( ).Object;
            int called = 0;
            Action<IEnumerable<ReaderEntityPair>, IImportReporter> bulkAction = ( pairs1, reporter1 ) => { called++; };
            var processor = new MemberProcessor( bulkAction );

            var pairs = new ReaderEntityPair [ ]
            {
                new ReaderEntityPair(null, null),
                new ReaderEntityPair(null, null)
            };
            processor.Action( pairs, reporter );
            Assert.That( called, Is.EqualTo( 1 ) );
        }

        [Test]
        public void Test_SingleConstructor_Called( )
        {
            IImportReporter reporter = new Mock<IImportReporter>( ).Object;
            int called = 0;
            Action<IObjectReader, IEntity, IImportReporter> singleAction = (x,y,reporter1) => { called++; };
            var processor = new MemberProcessor( singleAction );

            var pairs = new ReaderEntityPair [ ]
            {
                new ReaderEntityPair(null, null),
                new ReaderEntityPair(null, null)
            };
            processor.Action( pairs, reporter );
            Assert.That( called, Is.EqualTo(2) );
        }

    }
}
