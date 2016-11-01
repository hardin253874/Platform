// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Syslog
{
    /// <summary>
    ///     Syslog severity
    /// See https://tools.ietf.org/html/rfc5424#section-6.2.1
    /// </summary>
    public enum SyslogSeverity
    {
        /// <summary>
        ///     The system is unusable.
        /// </summary>
        Emergency = 0,


        /// <summary>
        ///     Action must be taken immediately.
        /// </summary>
        Alert = 1,


        /// <summary>
        ///     Critical conditions.
        /// </summary>
        Critical = 2,


        /// <summary>
        ///     Error conditions.
        /// </summary>
        Error = 3,


        /// <summary>
        ///     Warning conditions.
        /// </summary>
        Warning = 4,


        /// <summary>
        ///     Normal but significant condition.
        /// </summary>
        Notice = 5,


        /// <summary>
        ///     Informational messages.
        /// </summary>
        Informational = 6,


        /// <summary>
        ///     Debug level messages.
        /// </summary>
        Debug = 7
    }
}