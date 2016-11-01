// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Holds the relevant data required to initiate application based operations remotely.
    /// </summary>
    [DataContract]
    public class ApplicationOperationRequest : CastRequest
    {
        /// <summary>
        /// Identifies the application.
        /// </summary>
        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifies the version of the application.
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// The name of the <see cref="Tenant"/>.
        /// </summary>
        [DataMember(Name = "tenant")]
        public string Tenant { get; set; }

        /// <summary>
        /// The type of operation to attempt.
        /// </summary>
        [DataMember(Name = "operation")]
        public ApplicationOperation Operation { get; set; }
    }
}
