// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Plugin.Application
{
    /// <summary>
    /// Platform History Item.
    /// </summary>
    public class PlatformHistoryItem
    {
        /// <summary>
        /// The timestamp of the operation (in local time).
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The name of the tenant the operation was performed on (empty of global or not applicable).
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// The ID of the tenant the operation was performed on.
        /// </summary>
        public long TenantId { get; set; }

        /// <summary>
        /// The name of the package or application involved in the operation.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// The identifier of the package or application involved in the operation.
        /// </summary>
        public Guid PackageId { get; set; }

        /// <summary>
        /// The type of operation that was performed.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// The name of the machine that the operation was initiated from.
        /// </summary>
        public string Machine { get; set; }

        /// <summary>
        /// The user account and windows account name that initiated the operation.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The process that the operation initiated from.
        /// </summary>
        public string Process { get; set; }
        
        /// <summary>
        /// The arguments that were given to the operation to launch with.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Information about any exception that may have occurred during the operation.
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// Flag indicating that an exception was logged during the operation.
        /// </summary>
        public bool IsError { get; set; }
    }
}
