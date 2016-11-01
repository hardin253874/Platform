// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogViewer.Common
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Regex to parsing a guid.
        /// </summary>
        public static readonly Regex GuidRegEx = new Regex(@"(([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12})");


        /// <summary>
        /// The maximum grid message length.
        /// </summary>
        public static int MaxGridMessageLength = 100;
    }
}
