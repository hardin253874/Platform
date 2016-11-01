// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;

namespace EDC
{
    /// <summary>
    /// 
    /// </summary>
    public static class StringHelpers
    {
        private static readonly char[ ] SplitChars = { '\r', '\n' };

        /// <summary>
        /// Convert multi-line text to a single line.
        /// Trims leading and trailing whitespace.
        /// </summary>
        /// <param name="multiLine"></param>
        /// <returns></returns>
        public static string ToSingleLine( string multiLine )
        {
            if ( string.IsNullOrEmpty(multiLine) )
                return multiLine;

            string[ ] parts = multiLine.Split( SplitChars, StringSplitOptions.RemoveEmptyEntries );
            if ( parts.Length == 0 )
                return string.Empty;
            if ( parts.Length == 1 )
                return parts[ 0 ];

            return string.Join( " ", parts.Select( line => line.Trim( ) ).Where( line => line.Length > 0 ) );
        }
    }
}
