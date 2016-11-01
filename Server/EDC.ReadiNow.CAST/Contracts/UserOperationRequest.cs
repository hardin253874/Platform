// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Holds the relevant data required to perform actions on a <see cref="UserAccount"/> remotely.
    /// </summary>
    [DataContract]
    public class UserOperationRequest : CastRequest
    {
        /// <summary>
        /// The name of the user.
        /// </summary>
        [DataMember(Name = "user")]
        public string User { get; set; }

        /// <summary>
        /// The password.
        /// </summary>
        [DataMember(Name = "password")]
        public string Password { get; set; }

        /// <summary>
        /// The name of the <see cref="Tenant"/>.
        /// </summary>
        [DataMember(Name = "tenant")]
        public string Tenant { get; set; }

        /// <summary>
        /// The list of roles that the user should belong to.
        /// </summary>
        [DataMember(Name = "roles")]
        public RoleList Roles { get; set; }

        /// <summary>
        /// The type of operation to attempt.
        /// </summary>
        [DataMember(Name = "operation")]
        public Operation Operation { get; set; }
    }

    /// <summary>
    /// A collection of role names.
    /// </summary>
    [CollectionDataContract(ItemName = "role")]
    public class RoleList : List<string>
    {
        public RoleList() { }
        public RoleList(int capacity) : base(capacity) { }
        public RoleList(IEnumerable<string> collection) : base(collection) { }
    }
}
