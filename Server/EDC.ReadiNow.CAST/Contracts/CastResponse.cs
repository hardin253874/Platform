// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.MessageQueue;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// The basic response structure with the minimal amount of information required to make a CAST based response.
    /// </summary>
    [DataContract]
    public class CastResponse : ICastResponse, IMessageQueueTypeAware
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        public CastResponse()
        {
            Type = GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Flag indicating an error has occurred from the request.
        /// </summary>
        [DataMember(Name = "isError")]
        public bool IsError { get; set; }

        /// <summary>
        /// Provides information about any error that may have occurred.
        /// </summary>
        [DataMember(Name = "error")]
        public string Error { get; set; }

        /// <summary>
        /// The fully qualified type information that this response object should conform to. Used for <see cref="IMessageQueueTypeAware"/>
        /// dependent scenarios.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}
