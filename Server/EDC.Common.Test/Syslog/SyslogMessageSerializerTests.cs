// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using EDC.Syslog;
using NUnit.Framework;

namespace EDC.Test.Syslog
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class SyslogMessageSerializerTests
    {
        // 260 chars
        private const string HostNameExceedingLength = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
        // 255 chars
        private const string HostNameTruncated = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234";

        // 60 chars
        private const string AppNameExceedingLength = "012345678901234567890123456789012345678901234567890123456789";
        // 48 chars
        private const string AppNameTruncated = "012345678901234567890123456789012345678901234567";

        // 150 chars
        private const string ProcIdExceedingLength = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
        // 48 chars
        private const string ProcIdTruncated = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567";

        // 40 chars
        private const string MsgIdExceedingLength = "0123456789012345678901234567890123456789";
        // 32 chars
        private const string MsgIdTruncated = "01234567890123456789012345678901";

        // 40 char sd-id and param name
        private const string StructuredDataExceedingLength = "0123456789012345678901234567890123456789,0123456789012345678901234567890123456789:value";

        // 32 char sd-id and param name
        private const string StructuredDataTruncated = "[01234567890123456789012345678901 01234567890123456789012345678901=\"value\"]";


        /// <summary>
        ///     Decodes the structured data.
        /// </summary>
        /// <param name="structuredData">The structured data.</param>
        /// <returns></returns>
        private IEnumerable<SyslogSdElement> DecodeStructuredData(string structuredData)
        {
            if (string.IsNullOrEmpty(structuredData))
            {
                return null;
            }

            var sdElementsList = new List<SyslogSdElement>();

            string[] sdElements = structuredData.Split(';');

            foreach (string sdElement in sdElements)
            {
                string[] sdElementParts = sdElement.Split(',');

                SyslogSdElement element = null;

                for (int i = 0; i < sdElementParts.Length; i++)
                {
                    if (i == 0)
                    {
                        element = new SyslogSdElement(sdElementParts[i]);
                    }
                    else
                    {
                        string[] paramParts = sdElementParts[i].Split(':');

                        element.Parameters.Add(new SyslogSdParameter(paramParts[0], paramParts[1]));
                    }
                }

                sdElementsList.Add(element);
            }

            return sdElementsList;
        }


        /// <summary>
        ///     Tests the serializer constructor with a null message.
        /// </summary>
        [Test]
        public void TestSerializerCtrNullMessage()
        {
            var syslogSerializer = new SyslogMessageSerializer();
            Assert.Throws<ArgumentNullException>(() => syslogSerializer.Serialize(null, new MemoryStream()));
        }


        /// <summary>
        ///     Tests the serializer constructor with a null stream.
        /// </summary>
        [Test]
        public void TestSerializerCtrNullStream()
        {
            var syslogSerializer = new SyslogMessageSerializer();
            Assert.Throws<ArgumentNullException>(() => syslogSerializer.Serialize(new SyslogMessage
            {
                Facility = SyslogFacility.ClockDaemon1,
                Severity = SyslogSeverity.Alert,
                Timestamp = DateTimeOffset.UtcNow
            }, null));
        }


        /// <summary>
        ///     Tests serializing messages.
        /// </summary>
        [Test]
        [TestCase(SyslogFacility.LocalUse0, SyslogSeverity.Informational, null, null, null, null, null, null, null,
            "<134>1 - - - - - -\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, null, "Host", "App", "Proc", "MsgId", null, "Msg",
            "<10>1 - Host App Proc MsgId - Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, null, null, "App", "Proc", "MsgId", null, "Msg",
            "<10>1 - - App Proc MsgId - Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, null, null, null, "Proc", "MsgId", null, "Msg",
            "<10>1 - - - Proc MsgId - Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, null, null, null, null, "MsgId", null, "Msg",
            "<10>1 - - - - MsgId - Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, null, null, null, null, null, null, "Msg",
            "<10>1 - - - - - - Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, "02/11/2015 11:20 +11:00", "Host", "App", "Proc", "MsgId", null, "Msg",
            "<10>1 2015-02-11T00:20:00.000000Z Host App Proc MsgId - Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, "02/11/2015 11:20 +11:00", "Ho st", "A pp", "Pr oc", "Msg Id", "s d1@123,p1 1:v11,p1 2:v12;sd2@123,p2 1:v21,p2 2:v22", "Msg",
            "<10>1 2015-02-11T00:20:00.000000Z Host App Proc MsgId [sd1@123 p11=\"v11\" p12=\"v12\"][sd2@123 p21=\"v21\" p22=\"v22\"] Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, "02/11/2015 11:20 +11:00", "Host", "App", "Proc", "MsgId", "sd1@123,p11:v11,p12:v12;sd2@123,p21:v21,p22:v22", "Msg",
            "<10>1 2015-02-11T00:20:00.000000Z Host App Proc MsgId [sd1@123 p11=\"v11\" p12=\"v12\"][sd2@123 p21=\"v21\" p22=\"v22\"] Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, "02/11/2015 11:20 +11:00", "Host", "App", "Proc", "MsgId", "sd1@123,p11:v11", null,
            "<10>1 2015-02-11T00:20:00.000000Z Host App Proc MsgId [sd1@123 p11=\"v11\"]\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, null, HostNameExceedingLength, AppNameExceedingLength, ProcIdExceedingLength, MsgIdExceedingLength, StructuredDataExceedingLength,
            "Msg", "<10>1 - " + HostNameTruncated + " " + AppNameTruncated + " " + ProcIdTruncated + " " + MsgIdTruncated + " " + StructuredDataTruncated + " Msg\n")]
        [TestCase(SyslogFacility.UserLevelMessages, SyslogSeverity.Critical, null, null, null, null, null, "sd 1=]\"@123,p 1=]\":v1\"\\]", null,
            "<10>1 - - - - - [sd1@123 p1=\"v1\\\"\\\\\\]\"]\n")]
        public void TestSerializerData(SyslogFacility facility, SyslogSeverity severity, string dateTime, string hostName, string appName, string procId, string msgId, string structuredData, string message, string result)
        {
            var syslogSerializer = new SyslogMessageSerializer();

            var syslogMsg = new SyslogMessage
            {
                Facility = facility,
                Severity = severity,
                Timestamp = string.IsNullOrEmpty(dateTime) ? (DateTimeOffset?) null : DateTimeOffset.ParseExact(dateTime, @"MM/dd/yyyy H:mm zzz", CultureInfo.InvariantCulture),
                HostName = hostName,
                AppName = appName,
                ProcId = procId,
                MsgId = msgId,
                Message = message
            };

            IEnumerable<SyslogSdElement> structuredDataList = DecodeStructuredData(structuredData);
            if (structuredDataList != null)
            {
                foreach (SyslogSdElement sd in structuredDataList)
                {
                    syslogMsg.StructuredDataElements.Add(sd);
                }
            }


            using (var stream = new MemoryStream())
            {
                // Serialize a message to a memory stream
                syslogSerializer.Serialize(syslogMsg, stream);

                stream.Position = 0;

                // Get the message as a string and compare
                using (var reader = new StreamReader(stream))
                {
                    Assert.AreEqual(result, reader.ReadToEnd(), "The serialized messages is invalid.");
                }
            }
        }
    }
}