// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.SoftwarePlatform.Migration.Contract;
using System;

namespace PlatformConfigure
{
    /// <summary>
    /// 
    /// </summary>
    static class Helpers
    {
        /// <summary>
        ///     Converts a file format argument.    
        /// </summary>
        /// <param name="formatArgument">Text passed on command line</param>
        /// <returns>Format</returns>
        public static Format GetFileFormat( string formatArgument )
        {
            switch ( formatArgument?.ToLowerInvariant( ) )
            {
                case "xml":
                    return Format.Xml;
                case "xml1.0":
                    return Format.Xml;
                case "xml2.0":
                    return Format.XmlVer2;
                case "sql":
                    return Format.Sqlite;
                default:
                    return Format.Undefined;
            }
        }
    }
}
