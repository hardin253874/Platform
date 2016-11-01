// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.Syslog
{
    /// <summary>
    ///     Represents an RFC5424 syslog message
    /// </summary>
    public class SyslogMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SyslogMessage" /> class.
        /// </summary>
        public SyslogMessage()
        {
            StructuredDataElements = new List<SyslogSdElement>();
        }


        /// <summary>
        ///     Gets the priority.
        ///     See https://tools.ietf.org/html/rfc5424#section-6.2.1
        /// </summary>
        /// <value>
        ///     The priority.
        /// </value>
        public int Priority => ((int) Facility*8) + (int) Severity;


        /// <summary>
        ///     Gets or sets the timestamp.
        /// </summary>
        /// <value>
        ///     The timestamp.
        /// </value>
        public DateTimeOffset? Timestamp { get; set; }


        /// <summary>
        ///     Gets or sets the name of the host.
        /// </summary>
        /// <value>
        ///     The name of the host.
        /// </value>
        public string HostName { get; set; }


        /// <summary>
        ///     Gets or sets the name of the application.
        /// </summary>
        /// <value>
        ///     The name of the application.
        /// </value>
        public string AppName { get; set; }


        /// <summary>
        ///     Gets or sets the proc identifier.
        /// </summary>
        /// <value>
        ///     The proc identifier.
        /// </value>
        public string ProcId { get; set; }


        /// <summary>
        ///     Gets or sets the msg identifier.
        /// </summary>
        /// <value>
        ///     The msg identifier.
        /// </value>
        public string MsgId { get; set; }


        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string Message { get; set; }


        /// <summary>
        ///     Gets or sets the facility.
        /// </summary>
        /// <value>
        ///     The facility.
        /// </value>
        public SyslogFacility Facility { get; set; }


        /// <summary>
        ///     Gets or sets the severity.
        /// </summary>
        /// <value>
        ///     The severity.
        /// </value>
        public SyslogSeverity Severity { get; set; }


        /// <summary>
        ///     Gets the structured data elements.
        /// </summary>
        /// <value>
        ///     The structured data elements.
        /// </value>
        public IList<SyslogSdElement> StructuredDataElements { get; private set; }


        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public int Version => 1;
    }
}