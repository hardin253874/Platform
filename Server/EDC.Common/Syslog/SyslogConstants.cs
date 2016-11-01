// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Syslog
{
    /// <summary>
    /// 
    /// </summary>
    public static class SyslogReadiNowConstants
    {
        /// <summary>
        /// The application name.
        /// </summary>
        public const string ApplicationName = "SoftwarePlatform";  

        /// <summary>
        /// The ReadiNow enterprise identifier.        
        /// </summary>
        /// <remarks>This number was chosen at random.</remarks>
        public const int EnterpriseId = 1010101;        
    }

    /// <summary>
    ///     TimeQuality constants
    ///     See https://tools.ietf.org/html/rfc5424#section-7.1
    /// </summary>
    public static class SyslogTimeQualityConstants
    {
        /// <summary>
        ///     The time quality sd-id.
        ///     Available parameters are tzKnown, isSynced, syncAccuracy
        /// </summary>
        public const string TimeQuality = "timeQuality";


        /// <summary>
        ///     The tz known timeQuality parameter.
        /// </summary>
        public const string TzKnown = "tzKnown";


        /// <summary>
        ///     The is synced timeQuality parameter.
        /// </summary>
        public const string IsSynced = "isSynced";


        /// <summary>
        ///     The synchronize accuracy timeQuality parameter.
        /// </summary>
        public const string SyncAccuracy = "syncAccuracy";
    }

    /// <summary>
    ///     Origin constants
    ///     See https://tools.ietf.org/html/rfc5424#section-7.2
    /// </summary>
    public static class SyslogOriginConstants
    {
        /// <summary>
        ///     The origin sd-id.
        ///     Available parameters are ip, enterpriseId, software and swVersion
        /// </summary>
        public const string Origin = "origin";


        /// <summary>
        ///     The ip origin parameter.
        /// </summary>
        public const string Ip = "ip";


        /// <summary>
        ///     The enterprise identifier origin parameter.
        /// </summary>
        public const string EnterpriseId = "enterpriseId";


        /// <summary>
        ///     The software origin parameter.
        /// </summary>
        public const string Software = "software";


        /// <summary>
        ///     The sw version origin parameter.
        /// </summary>
        public const string SwVersion = "swVersion";
    }

    /// <summary>
    ///     Meta constants
    ///     See https://tools.ietf.org/html/rfc5424#section-7.3
    /// </summary>
    public static class SyslogMetaConstants
    {
        /// <summary>
        ///     The meta sd-id.
        ///     Available parameters are sequenceId, sysUpTime and language
        /// </summary>
        public const string Meta = "meta";


        /// <summary>
        ///     The sequence identifier meta paramater.
        /// </summary>
        public const string SequenceId = "sequenceId";


        /// <summary>
        ///     The system up time meta paramater.
        /// </summary>
        public const string SysUpTime = "sysUpTime";


        /// <summary>
        ///     The language meta paramater.
        /// </summary>
        public const string Language = "language";
    }
}