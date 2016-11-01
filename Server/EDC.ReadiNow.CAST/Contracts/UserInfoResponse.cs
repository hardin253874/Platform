// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Packages information about users and roles.
    /// </summary>
    [DataContract]
    public class UserInfoResponse : CastResponse
    {
        /// <summary>
        /// List of role names found in the tenant.
        /// </summary>
        [DataMember(Name = "roles")]
        public RoleList Roles { get; set; }

        /// <summary>
        /// List of user information.
        /// </summary>
        [DataMember(Name = "users")]
        public UserList Users { get; set; }
    }
}
