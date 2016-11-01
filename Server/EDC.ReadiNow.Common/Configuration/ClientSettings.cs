// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Configuration;
using System.Diagnostics;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// client configuration section.
    /// </summary>
 
    [DebuggerStepThrough]
    public class ClientSettings : ConfigurationElement
    {
        /// <summary>
        /// The minimum version that the client must be at. This value will be ignored if it is less than the major and minor version of the server build.
        ///  If not specified then it will fall back to the major and minor version of the server build.
        /// </summary>
        [ConfigurationProperty("minClientVersion", IsRequired = false)]
        public string MinClientVersion
        {
            get
            {
                return (string)this["minClientVersion"];
            }
            set
            {
                this["minClientVersion"] = value;
            }
        }

        /// <summary>
        /// Get the required client version including fallback logic. If it is not specified then use the provided server major/minor version. If it is provided but is less than
        /// the server major/minor return the server major/minor.
        /// </summary>
        /// <param name="platformVersion">The version of the platform</param>
        /// <returns></returns>
        public string GetMinClientVersion(string platformVersion)
        {
            var required = MinClientVersion;

            var majorMinorServerVersion = ToMajorMinorVersion(platformVersion);

            // If unspecified use the server major minor
            if (String.IsNullOrEmpty(required))
                return majorMinorServerVersion;

            // If the version is from a previous release, ignore it and go server major minor
            if (new Version(required) < new Version(majorMinorServerVersion))
                return majorMinorServerVersion;

            return required;
        }

        private string ToMajorMinorVersion(string version)
        {
            var v = new Version(version);
            return $"{v.Major}.{v.Minor}";
        }
    }
}
