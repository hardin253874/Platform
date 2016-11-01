// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Holds information about a managed user account.
    /// </summary>
    [DataContract]
    public class RemoteUserInfo
    {
        /// <summary>
        /// The id that is unique for the user account in the tenant.
        /// </summary>
        [DataMember(Name = "remoteId")]
        public long RemoteId { get; set; }

        /// <summary>
        /// The name of the user account (i.e. login).
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The status of the account.
        /// </summary>
        [DataMember(Name = "status")]
        public UserStatus Status { get; set; }

        /// <summary>
        /// The list of roles that the user belongs to.
        /// </summary>
        [DataMember(Name = "roles")]
        public RoleList Roles { get; set; }
    }

    /// <summary>
    /// A collection of user information.
    /// </summary>
    [CollectionDataContract(ItemName = "user")]
    public class UserList : List<RemoteUserInfo>
    {
        public UserList() { }
        public UserList(int capacity) : base(capacity) { }
        public UserList(IEnumerable<RemoteUserInfo> collection) : base(collection) { }
    }
}
