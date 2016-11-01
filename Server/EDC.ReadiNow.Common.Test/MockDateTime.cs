// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.Common;

namespace EDC.ReadiNow.Test
{
    /// <summary>
    /// Mock implementation of IDateTime for writing unit tests that rely on current time.
    /// </summary>
    public class MockDateTime : IDateTime
    {
        /// <summary>
        /// MUST have kind of Utc
        /// </summary>
        public DateTime UtcNow { get; set; }
        
        /// <summary>
        /// MUST have kind of Local
        /// </summary>
        public DateTime Now { get; set; }

        /// <summary>
        /// MUST have kind of Local
        /// </summary>
        public DateTime Today => Now.Date;
    }
}
