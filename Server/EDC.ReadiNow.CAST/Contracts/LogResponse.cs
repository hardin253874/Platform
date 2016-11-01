// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// A response message that follows a <see cref="LogRequest"/> being made.
    /// </summary>
    [DataContract]
    public class LogResponse : CastResponse
    {
        /// <summary>
        /// The time in UTC that the log entry was written.
        /// </summary>
        [DataMember(Name = "time")]
        public DateTime Time { get; set; }
    }
}
