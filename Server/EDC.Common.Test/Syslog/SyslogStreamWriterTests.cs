// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using EDC.IO;
using EDC.Syslog;
using Moq;
using NUnit.Framework;

namespace EDC.Test.Syslog
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class SyslogStreamWriterTests
    {
        /// <summary>
        ///     Writes the test data to stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private void WriteTestDataToStream(Stream stream, string data)
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(data);
            }
        }


        /// <summary>
        ///     Tests the syslog stream writer constructor with a null serializer.
        /// </summary>
        [Test]
        public void TestSyslogStreamWriterCtrNullSerializer()
        {
            var streamProviderMock = new Mock<IStreamProvider>(MockBehavior.Loose);
            Assert.Throws<ArgumentNullException>(() => new SyslogStreamWriter(streamProviderMock.Object, null));
        }


        /// <summary>
        ///     Tests the syslog stream writer constructor with a null provider.
        /// </summary>
        [Test]
        public void TestSyslogStreamWriterCtrNullStreamProvider()
        {
            var syslogSerializerMock = new Mock<ISyslogMessageSerializer>(MockBehavior.Loose);
            Assert.Throws<ArgumentNullException>(() => new SyslogStreamWriter(null, syslogSerializerMock.Object));
        }


        /// <summary>
        ///     Tests the syslog stream writer failing write.
        /// </summary>
        [Test]
        public void TestSyslogStreamWriterFailingWrite()
        {
            var memoryStream = new MemoryStream();

            // Setup the mocks
            var mockRepo = new MockRepository(MockBehavior.Strict);

            Mock<IStreamProvider> streamProviderMock = mockRepo.Create<IStreamProvider>();
            streamProviderMock.Setup(s => s.GetStream()).Returns(memoryStream);
            streamProviderMock.Setup(s => s.CloseStream());

            // Create the message
            var syslogMessage = new SyslogMessage
            {
                Facility = SyslogFacility.ClockDaemon1,
                Severity = SyslogSeverity.Alert
            };

            Mock<ISyslogMessageSerializer> syslogSerializerMock = mockRepo.Create<ISyslogMessageSerializer>();
            syslogSerializerMock.Setup(sm => sm.Serialize(syslogMessage, It.IsAny<Stream>())).Throws<InvalidOperationException>();

            // Write a message and see that it fails
            var streamWriter = new SyslogStreamWriter(streamProviderMock.Object, syslogSerializerMock.Object);
            Assert.Throws<InvalidOperationException>(() => streamWriter.Write(syslogMessage));

            Assert.AreEqual(0, memoryStream.Length);

            mockRepo.VerifyAll();
        }


        /// <summary>
        ///     Tests the syslog stream writer.
        /// </summary>
        [Test]
        public void TestSyslogStreamWriterNullMessage()
        {
            var mockRepo = new MockRepository(MockBehavior.Loose);

            Mock<IStreamProvider> streamProviderMock = mockRepo.Create<IStreamProvider>();
            Mock<ISyslogMessageSerializer> syslogSerializerMock = mockRepo.Create<ISyslogMessageSerializer>();

            var streamWriter = new SyslogStreamWriter(streamProviderMock.Object, syslogSerializerMock.Object);
            Assert.Throws<ArgumentNullException>(() => streamWriter.Write(null));
        }


        /// <summary>
        ///     Tests the syslog stream writer.
        /// </summary>
        [Test]
        public void TestSyslogStreamWriterValidWrite()
        {
            var memoryStream = new MemoryStream();

            // Setup the mocks
            var mockRepo = new MockRepository(MockBehavior.Strict);

            Mock<IStreamProvider> streamProviderMock = mockRepo.Create<IStreamProvider>();
            streamProviderMock.Setup(s => s.GetStream()).Returns(memoryStream);
            streamProviderMock.Setup(s => s.Dispose());

            string syslogMsg = "Msg" + Guid.NewGuid();

            var syslogMessage = new SyslogMessage {Facility = SyslogFacility.ClockDaemon1, Severity = SyslogSeverity.Alert};

            Mock<ISyslogMessageSerializer> syslogSerializerMock = mockRepo.Create<ISyslogMessageSerializer>();
            syslogSerializerMock.Setup(sm => sm.Serialize(syslogMessage, It.IsAny<Stream>())).Callback<SyslogMessage, Stream>((m, s) => WriteTestDataToStream(s, syslogMsg));

            using (var streamWriter = new SyslogStreamWriter(streamProviderMock.Object, syslogSerializerMock.Object))
            {
                streamWriter.Write(syslogMessage);
            }

            // Validation
            memoryStream.Position = 0;
            var reader = new StreamReader(memoryStream);
            Assert.AreEqual(syslogMsg, reader.ReadToEnd());

            mockRepo.VerifyAll();
        }
    }
}