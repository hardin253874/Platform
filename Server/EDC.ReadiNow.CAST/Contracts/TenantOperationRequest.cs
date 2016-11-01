// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Holds the relevant data required to perform actions on a <see cref="Tenant"/> remotely.
    /// </summary>
    [DataContract]
    public class TenantOperationRequest : CastRequest
    {
        /// <summary>
        /// The name of the <see cref="Tenant"/> to perform the operation with.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Identifies the <see cref="Tenant"/> on the platform. Use for rename.
        /// </summary>
        [DataMember(Name = "id")]
        public long Id { get; set; }

        /// <summary>
        /// The type of operation to attempt.
        /// </summary>
        [DataMember(Name = "operation")]
        public Operation Operation { get; set; }
    }
}