// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace ReadiNow.Common
{
    /// <summary>
    /// An implementation of IDateTime that wraps System.DateTime to return current values for the current server.
    /// </summary>
    public class DateTimeProvider : IDateTime
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static IDateTime Instance { get; } = new DateTimeProvider( );

        /// <summary>
        /// Gets a System.DateTime object that is set to the current date and time on this
        /// computer, expressed as the Coordinated Universal Time (UTC).
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// Returns an object that is set to today's date, with the time component set to 00:00:00.
        /// </summary>
        public DateTime Today => DateTime.Today;

        /// <summary>
        /// Gets a System.DateTime object that is set to the current date and time on this
        /// computer, expressed as the local time.
        /// </summary>
        public DateTime Now => DateTime.Now;
    }
}
