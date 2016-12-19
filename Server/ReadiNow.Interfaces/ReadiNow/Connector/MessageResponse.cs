// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace ReadiNow.Connector
{
    /// <summary>
    /// Convenient container for standard response messages consisting of a response code and text.
    /// </summary>
    [DataContract]
    public class MessageResponse
    {
        /// <summary>
        /// Error code. Should be of the form Exxxx.
        /// </summary>
        [DataMember(Name = "code")]
        public string PlatformMessageCode { get; set;  }

        /// <summary>
        /// Response message.
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}
