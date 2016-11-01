// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.MessageQueue;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// The basic request structure with the minimal amount of information required to make a CAST based request.
    /// </summary>
    [DataContract]
    public class CastRequest : ICastRequest, IMessageQueueTypeAware
    {
        /// <summary>
        /// The database identifier of the platform that the request is addressed to.
        /// </summary>
        [DataMember(Name = "dbId")]
        public string DatabaseId { get; set; }

        /// <summary>
        /// The entity id of the workflow run that has initiated the request.
        /// </summary>
        [DataMember(Name = "runId")]
        public long RunId { get; set; }

        /// <summary>
        /// The step count within the workflow run of when the request was initiated.
        /// </summary>
        [DataMember(Name = "runStep")]
        public int RunStep { get; set; }

        /// <summary>
        /// The fully qualified type information that this request object should conform to. Used for <see cref="IMessageQueueTypeAware"/>
        /// dependent scenarios.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}
