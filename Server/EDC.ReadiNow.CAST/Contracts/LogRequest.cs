// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// A request message that will log a message on a remote platform.
    /// </summary>
    [DataContract]
    public class LogRequest : CastRequest
    {
        /// <summary>
        /// The message to write into the log.
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}
