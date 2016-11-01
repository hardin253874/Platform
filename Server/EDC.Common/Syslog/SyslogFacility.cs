// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Syslog
{
    /// <summary>
    ///     Syslog facility.
    ///     See https://tools.ietf.org/html/rfc5424#section-6.2.1
    /// </summary>
    public enum SyslogFacility
    {
        /// <summary>
        ///     Kernel messages.
        /// </summary>
        KernelMessages = 0,


        /// <summary>
        ///     User level messages.
        /// </summary>
        UserLevelMessages = 1,


        /// <summary>
        ///     Mail system.
        /// </summary>
        MailSystem = 2,


        /// <summary>
        ///     System daemons.
        /// </summary>
        SystemDaemons = 3,


        /// <summary>
        ///     Security or authorization messages.
        /// </summary>
        SecurityOrAuthorizationMessages1 = 4,


        /// <summary>
        ///     Internal syslog messages.
        /// </summary>
        InternalSyslogMessages = 5,


        /// <summary>
        ///     Line printer subsystem.
        /// </summary>
        LinePrinterSubsystem = 6,


        /// <summary>
        ///     Network news subsystem.
        /// </summary>
        NetworkNewsSubsystem = 7,


        /// <summary>
        ///     UUCP subsystem.
        /// </summary>
        UucpSubsystem = 8,


        /// <summary>
        ///     Clock daemon.
        /// </summary>
        ClockDaemon1 = 9,


        /// <summary>
        ///     Security or authorization messages.
        /// </summary>
        SecurityOrAuthorizationMessages2 = 10,


        /// <summary>
        ///     FTP daemon.
        /// </summary>
        FtpDaemon = 11,


        /// <summary>
        ///     NTP subsystem.
        /// </summary>
        NtpSubsystem = 12,


        /// <summary>
        ///     Log audit.
        /// </summary>
        LogAudit = 13,


        /// <summary>
        ///     Log alert.
        /// </summary>
        LogAlert = 14,


        /// <summary>
        ///     Clock daemon.
        /// </summary>
        ClockDaemon2 = 15,


        /// <summary>
        ///     Lock use 0.
        /// </summary>
        LocalUse0 = 16,


        /// <summary>
        ///     Local use 1.
        /// </summary>
        LocalUse1 = 17,


        /// <summary>
        ///     Local use 2.
        /// </summary>
        LocalUse2 = 18,


        /// <summary>
        ///     Local use 3.
        /// </summary>
        LocalUse3 = 19,


        /// <summary>
        ///     Local use 4.
        /// </summary>
        LocalUse4 = 20,


        /// <summary>
        ///     Local use 5.
        /// </summary>
        LocalUse5 = 21,


        /// <summary>
        ///     Local use 6.
        /// </summary>
        LocalUse6 = 22,


        /// <summary>
        ///     Local use 7.
        /// </summary>
        LocalUse7 = 23
    }
}