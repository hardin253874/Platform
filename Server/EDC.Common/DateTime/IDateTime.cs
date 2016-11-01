// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace ReadiNow.Common
{
    /// <summary>
    /// Interface for offering date-time services.
    /// </summary>
    /// <remarks>
    /// Provided as an interface, so they can be mocked.
    /// </remarks>
    public interface IDateTime
    {
        /// <summary>
        /// Gets a System.DateTime object that is set to the current date and time on this
        /// computer, expressed as the Coordinated Universal Time (UTC).
        /// </summary>
        /// <remarks>
        /// Result MUST have DateTimeKind = Utc.
        /// </remarks>
        DateTime UtcNow { get; }

        /// <summary>
        /// Returns an object that is set to today's date, with the time component set to 00:00:00.
        /// </summary>
        /// <remarks>
        /// Result MUST have DateTimeKind = Local.
        /// </remarks>
        DateTime Now { get; }

        /// <summary>
        /// Gets a System.DateTime object that is set to the current date and time on this
        /// computer, expressed as the local time.
        /// </summary>
        /// <remarks>
        /// Result MUST have DateTimeKind = Local.
        /// </remarks>
        DateTime Today { get; }
    }
}
