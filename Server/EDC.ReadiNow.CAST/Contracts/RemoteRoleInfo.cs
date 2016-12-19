// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Holds information about a managed role.
    /// </summary>
    [DataContract]
    public class RemoteRoleInfo
    {
        /// <summary>
        /// The id that is unique for the role in the tenant.
        /// </summary>
        [DataMember(Name = "remoteId")]
        public long RemoteId { get; set; }

        /// <summary>
        /// The name of the user account (i.e. login).
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The list of roles that are contained by this role.
        /// </summary>
        [DataMember(Name = "roles")]
        public RoleList Roles { get; set; }

        /// <summary>
        /// The list of users that are contained by this role.
        /// </summary>
        [DataMember(Name = "users")]
        public UserList Users { get; set; }
    }

    /// <summary>
    /// A collection of user information.
    /// </summary>
    [CollectionDataContract(ItemName = "role")]
    public class RoleList : List<RemoteRoleInfo>
    {
        public RoleList() { }
        public RoleList(int capacity) : base(capacity) { }
        public RoleList(IEnumerable<RemoteRoleInfo> collection) : base(collection) { }
    }
}
