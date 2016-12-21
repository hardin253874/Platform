// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading;
using EDC.Syslog;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Syslog
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class SyslogQueueingMessageWriterTests
    {
        /// <summary>
        ///     Tests writing to the queued writer with a failed write which fails on retry.
        /// </summary>
        [Test]
        [TestCase(2)]
        [TestCase(6)]
        public void TestFailingWriteFailingRetryToQueuingWriter(int maxRetries)
        {
            const string msgId = "TestMessageId";
            var msg = new SyslogMessage
            {
                Facility = SyslogFacility.ClockDaemon1,
                Severity = SyslogSeverity.Alert,
                MsgId = msgId
            };

            var mockRepo = new MockRepository(MockBehavior.Strict);

			if ( maxRetries > 5 )
			{
				maxRetries = 5;
			}

			using ( CountdownEvent evt = new CountdownEvent( maxRetries + 1 ) )
			{
				Mock<ISyslogMessageWriter> syslogMessageWriterMock = mockRepo.Create<ISyslogMessageWriter>( );
				syslogMessageWriterMock.Setup( w => w.Write( msg ) ).Callback( ( ) =>
				{
					// ReSharper disable once AccessToDisposedClosure
					evt.Signal( );
					throw new InvalidOperationException( )
					;
				} );

				var queuingWriter = new SyslogQueueingMessageWriter( syslogMessageWriterMock.Object, maxRetries );

				// Test
				queuingWriter.Write( msg );

				evt.Wait( 3000 );

				// Validation
				syslogMessageWriterMock.Verify( w => w.Write( It.IsAny<SyslogMessage>( ) ), Times.Exactly( maxRetries + 1 ) );
			}

            mockRepo.VerifyAll();
        }


        /// <summary>
        ///     Tests writing to the queued writer with a failed write which succeeds on retry.
        /// </summary>
        [Test]
        public void TestFailingWriteSuccessfullRetryToQueuingWriter()
        {
            int maxRetries = 2;
            string msgId = "TestMessageId";
            var msg = new SyslogMessage
            {
                Facility = SyslogFacility.ClockDaemon1,
                Severity = SyslogSeverity.Alert,
                MsgId = msgId
            };

            var mockRepo = new MockRepository(MockBehavior.Strict);

			using ( CountdownEvent evt = new CountdownEvent( maxRetries ) )
			{

				Mock<ISyslogMessageWriter> syslogMessageWriterMock = mockRepo.Create<ISyslogMessageWriter>( );
				syslogMessageWriterMock.Setup( w => w.Write( msg ) ).Callback<SyslogMessage>( m =>
				{
					// ReSharper disable once AccessToDisposedClosure
					evt.Signal( );

					// ReSharper disable once AccessToDisposedClosure
					if ( evt.CurrentCount == 1 )
					{
						// Throw on first call
						throw new InvalidOperationException( );
					}
				} );

				var queuingWriter = new SyslogQueueingMessageWriter( syslogMessageWriterMock.Object, maxRetries );

				// Test
				queuingWriter.Write( msg );

				evt.Wait( 3000 );

				// Validation
				syslogMessageWriterMock.Verify( w => w.Write( It.IsAny<SyslogMessage>( ) ), Times.Exactly( maxRetries ) );
			}

            mockRepo.VerifyAll();
        }        


        /// <summary>
        ///     Tests the constructor with a null writer.
        /// </summary>
        [Test]
        public void TestQueuingWriterCtrNullWriter()
        {         
            Assert.Throws<ArgumentNullException>(() => new SyslogQueueingMessageWriter(null, 2));
        }


        /// <summary>
        ///     Tests writing a null message to queuing writer.
        /// </summary>
        [Test]
        public void TestWritingNullMessage()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);

            Mock<ISyslogMessageWriter> syslogMessageWriterMock = mockRepo.Create<ISyslogMessageWriter>();            

            var queuingWriter = new SyslogQueueingMessageWriter(syslogMessageWriterMock.Object, 2);

            Assert.Throws<ArgumentNullException>(() => queuingWriter.Write(null));
        }


        /// <summary>
        ///     Tests the writing to queuing writer.
        /// </summary>
        [Test]        
        public void TestWritingToQueuingWriter()
        {
            var msg = new SyslogMessage
            {
                Facility = SyslogFacility.ClockDaemon1,
                Severity = SyslogSeverity.Alert
            };

            var mockRepo = new MockRepository(MockBehavior.Strict);

            Mock<ISyslogMessageWriter> syslogMessageWriterMock = mockRepo.Create<ISyslogMessageWriter>();

			using ( AutoResetEvent evt = new AutoResetEvent( false ) )
			{
				syslogMessageWriterMock.Setup( w => w.Write( msg ) ).Callback( ( ) =>
				{
					// ReSharper disable once AccessToDisposedClosure
					evt.Set( );
				} );

				var queuingWriter = new SyslogQueueingMessageWriter( syslogMessageWriterMock.Object, 2 );

				queuingWriter.Write( msg );

				evt.WaitOne( 3000 );
			}

            mockRepo.VerifyAll();
        }
        

        /// <summary>
        ///     Tests that disposing of a writer flushes the queue.
        /// </summary>
        [Test]
        public void TestDisposeFlushesQueueingWriter()
        {
            var syncRoot = new object();            

            var msg = new SyslogMessage
            {
                Facility = SyslogFacility.ClockDaemon1,
                Severity = SyslogSeverity.Alert
            };

            var mockRepo = new MockRepository(MockBehavior.Strict);

            int messagesProcessed = 0;

            Mock<ISyslogMessageWriter> syslogMessageWriterMock = mockRepo.Create<ISyslogMessageWriter>();
            syslogMessageWriterMock.Setup(w => w.Write(msg)).Callback<SyslogMessage>(m =>
            {                
                lock (syncRoot)
                {
                    messagesProcessed++;                    
                }
            });            

            var queuingWriter = new SyslogQueueingMessageWriter(syslogMessageWriterMock.Object, 2);

            // Lock when writing to queue to force the writer to wait so that the dispose actually has work to do
            lock (syncRoot)
            {
                for (int i = 0; i < 10; i++)
                {
                    queuingWriter.Write(msg);
                }                
            }            

            queuingWriter.Dispose();

            Assert.AreEqual(10, messagesProcessed, "The number of processed messages is invalid.");

            mockRepo.VerifyAll();
        }
    }
}