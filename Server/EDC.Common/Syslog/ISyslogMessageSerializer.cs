// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.IO;

namespace EDC.Syslog
{
    /// <summary>
    ///     Interface for serializing syslog messages.
    /// </summary>
    public interface ISyslogMessageSerializer
    {
        /// <summary>
        ///     Serializes the specified message to the specified stream.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="stream">The stream.</param>
        void Serialize(SyslogMessage message, Stream stream);
    }
}