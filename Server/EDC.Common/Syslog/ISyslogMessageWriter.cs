// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Syslog
{
    /// <summary>
    ///     Interface for writing syslog messages.
    /// </summary>
    public interface ISyslogMessageWriter
    {
        /// <summary>
        ///     Write the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Write(SyslogMessage message);
    }
}